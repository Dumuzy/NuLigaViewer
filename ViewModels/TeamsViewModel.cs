using NuLigaViewer.Data;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.ComponentModel;

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
                var lastGameDayReport = NuLigaTransformer.TransformTeamsToGameDayReport(teams);

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

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}