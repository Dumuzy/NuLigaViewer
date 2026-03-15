using NuLigaViewer.Data;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace NuLigaViewer.ViewModels
{
    public class LeagueViewModel : INotifyPropertyChanged
    {
        private static readonly ConcurrentDictionary<string, LeagueViewModel> _instances = new();

        public static LeagueViewModel GetOrCreate(League league)
        {
            if (league == null) throw new ArgumentNullException(nameof(league));

            var key = (league.Url ?? string.Empty).Trim();
            return _instances.GetOrAdd(key, _ => new LeagueViewModel(league));
        }

        public ObservableCollection<TeamViewModel> Teams { get; } = new();
        public ObservableCollection<TeamPairingViewModel> LastGameDay { get; } = new();
        public string? LastGameTitle => LastGameDay.Any() ? LastGameDay.First().Title : null;

        public ObservableCollection<TopTenPlayerViewModel> TopTenPlayer { get; } = new();

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

        public LeagueViewModel(League league)
        {
            League = league ?? throw new ArgumentNullException(nameof(league));

            NuLigaParser.TeamPairingReportLoadedForGui += NuLigaParser_TeamPairingReportLoaded;

            _ = LoadTeamsAsync();
        }

        private void NuLigaParser_TeamPairingReportLoaded(League league, TeamPairing teamPairing)
        {
            if (Teams.Count == 0 || league != League)
            {
                return;
            }

            MainThread.BeginInvokeOnMainThread(() =>
            {
                var teamViewModel = Teams.FirstOrDefault(t => t.ContainsTeamPairing(teamPairing));
                if (teamViewModel != null)
                {
                    teamViewModel.Refresh();
                }

                var tpViewModel = LastGameDay.FirstOrDefault(t => t.ContainsTeamPairing(teamPairing));
                if (tpViewModel != null)
                {
                    tpViewModel.Refresh();
                }
            });

            if (IsLoading || Teams.Any(team => !team.AllReportsLoaded))
            {
                return;
            }
            _ = SortTeamsAsync();
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

                var teams = await Task.Run(() => NuLigaParser.ParseTeams(League) ?? []);
                var lastGameDay = NuLigaTransformer.TransformTeamsToLastGameDay(teams);
                var allPlayers = NuLigaTransformer.TransformTeamsToAllPlayerList(teams);

                MainThread.BeginInvokeOnMainThread(() =>
                {
                    Teams.Clear();
                    foreach (var team in teams)
                    {
                        Teams.Add(new TeamViewModel(team));
                    }

                    LastGameDay.Clear();
                    foreach (var teamPairing in lastGameDay)
                    {
                        LastGameDay.Add(new TeamPairingViewModel(teamPairing));
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

                    OnPropertyChanged(nameof(LastGameTitle));
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

            SortAndColorTeams(ref vms);

            MainThread.BeginInvokeOnMainThread(() =>
            {
                Teams.Clear();
                var index = 1;
                foreach (var vm in vms)
                {
                    vm.Rank = index;
                    Teams.Add(vm);
                    index++;
                }
            });
        }

        private void SortAndColorTeams(ref List<TeamViewModel> vms)
        {
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
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}