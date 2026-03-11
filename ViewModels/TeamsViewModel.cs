using NuLigaViewer.Data;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;

namespace NuLigaViewer.ViewModels
{
    public class TeamsViewModel : INotifyPropertyChanged
    {
        private static readonly ConcurrentDictionary<string, TeamsViewModel> _instances = new();

        public static TeamsViewModel GetOrCreate(League league)
        {
            if (league == null) throw new ArgumentNullException(nameof(league));

            var key = (league.Url ?? string.Empty).Trim();
            return _instances.GetOrAdd(key, _ => new TeamsViewModel(league));
        }

        public ObservableCollection<TeamViewModel> Teams { get; } = new();
        public ObservableCollection<GameDayViewModel> GameDays { get; } = new();
        public ObservableCollection<TopTenPlayerViewModel> TopTenPlayer { get; } = new();

        private readonly RelayCommand _sortTeamsCommand;
        public ICommand SortTeamsCommand => _sortTeamsCommand;

        public League League { get; }

        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            private set
            {
                if (_isLoading != value)
                {
                    _isLoading = value;
                    OnPropertyChanged(nameof(IsLoading));
                }
            }
        }

        public TeamsViewModel(League league)
        {
            League = league ?? throw new ArgumentNullException(nameof(league));

            _sortTeamsCommand = new RelayCommand(SortTeamsAsync, () => Teams.Count > 0);

            NuLigaParser.GameDayReportLoadedForGui += NuLigaParser_GameDayReportLoaded;

            _ = LoadTeamsAsync();
        }

        private void NuLigaParser_GameDayReportLoaded(GameDay gameDay)
        {
            if (Teams.Count == 0)
            {
                return;
            }

            MainThread.BeginInvokeOnMainThread(() =>
            {
                var teamViewModel = Teams.FirstOrDefault(t => t.ContainsGameDay(gameDay));
                if (teamViewModel != null)
                {
                    teamViewModel.Refresh();
                }

                var gdViewModel = GameDays.FirstOrDefault(gd => gd.ContainsGameDay(gameDay));
                if (gdViewModel != null)
                {
                    gdViewModel.Refresh();
                }
            });
        }

        private async Task LoadTeamsAsync()
        {
            if (League == null || string.IsNullOrWhiteSpace(League.Url))
            {
                return;
            }

            if (Teams.Count > 0)
            {
                return;
            }

            try
            {
                IsLoading = true;

                var teams = await Task.Run(() => NuLigaParser.ParseTeams(League.Url) ?? []);
                var lastGameDayReport = NuLigaTransformer.TransformTeamsToLastGameDayReport(teams);
                var allPlayers = NuLigaTransformer.TransformTeamsToAllPlayerList(teams);

                var vms = teams.Select(t => new TeamViewModel(t)).ToList();
                if (vms.Count > 0)
                {
                    vms[0].RowColor = Colors.Green;

                    for (int i = Math.Max(0, vms.Count - 2); i < vms.Count; i++)
                    {
                        vms[i].RowColor = Colors.Red;
                    }
                }

                MainThread.BeginInvokeOnMainThread(() =>
                {
                    Teams.Clear();
                    var index = 1;
                    foreach (var vm in vms)
                    {
                        vm.RankAfterSorting = index;
                        Teams.Add(vm);
                        index++;
                    }

                    GameDays.Clear();
                    foreach (var gd in lastGameDayReport)
                    {
                        GameDays.Add(new GameDayViewModel(gd));
                    }

                    TopTenPlayer.Clear();
                    var rang = 1;
                    foreach (var player in allPlayers.Take(10))
                    {
                        var ttpVm = new TopTenPlayerViewModel(player)
                        {
                            Rang = rang
                        };
                        TopTenPlayer.Add(ttpVm);
                        rang++;
                    }

                    _sortTeamsCommand.RaiseCanExecuteChanged();
                });
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.ToString());
            }
            finally
            {
                IsLoading = false;
            }
        }

        public async Task SortTeamsAsync()
        {
            var vms = Teams.Select(t => t).ToList();
            vms.Sort((a, b) =>
            {
                int pointsComparison = b.Points.CompareTo(a.Points);
                if (pointsComparison != 0)
                {
                    return pointsComparison;
                }

                int bpComparison = b.BoardPointsSum.CompareTo(a.BoardPointsSum);
                if (bpComparison != 0)
                {
                    return bpComparison;
                }
                return b.BerlinTieBreak.CompareTo(a.BerlinTieBreak);
            });

            for (var i = 0; i < vms.Count; i++)
            {
                if (i == 0)
                {
                    vms[i].RowColor = Colors.Green;
                }
                else if (i >= Math.Max(0, vms.Count - 2))
                {
                    vms[i].RowColor = Colors.Red;
                }
                else
                {
                    vms[i].RowColor = Application.Current?.RequestedTheme == AppTheme.Dark ? Colors.White : Colors.Black;
                }
            }

            MainThread.BeginInvokeOnMainThread(() =>
            {
                Teams.Clear();
                var index = 1;
                foreach (var vm in vms)
                {
                    vm.RankAfterSorting = index;
                    Teams.Add(vm);
                    index++;
                }
            });
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}