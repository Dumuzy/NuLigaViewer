using NuLigaViewer.Data;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace NuLigaViewer.ViewModels
{
    public class LeagueViewModel : INotifyPropertyChanged
    {
        public LeagueViewModel()
        {
            NuLigaParser.TeamPairingReportLoadedForGui += NuLigaParser_TeamPairingReportLoaded;
        }

        private static readonly ConcurrentDictionary<string, Team[]> _cachedLeagues = new();

        private League? _league;
        public League? League
        {
            get => _league;
            set
            {
                if (_league != value)
                {
                    _league = value;
                    OnPropertyChanged(nameof(League));
                }
            }
        }

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

        public ObservableCollection<TeamViewModel> Teams { get; } = new();
        public ObservableCollection<TeamPairingViewModel> LastGameDay { get; } = new();
        public string? LastGameTitle => LastGameDay.Any() ? LastGameDay.First().Title : null;

        public ObservableCollection<TopTenPlayerViewModel> TopTenPlayer { get; } = new();

        public List<TeamPairing> AllAvailableTeamPairings => Teams.SelectMany(t => t.GameDays ?? []).Distinct().ToList();
        public List<Player> AllAvailablePlayer => Teams.Where(x => x.Players != null).SelectMany(x => x.Players!).ToList();

        public async Task LoadLeagueAsync(League league)
        {
            if (league == null || string.IsNullOrWhiteSpace(league.Url) || string.IsNullOrWhiteSpace(league.Name))
            {
                return;
            }

            League = league;

            if (_cachedLeagues.TryGetValue(league.Name, out var cachedTeams))
            {
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    RefreshTeams(cachedTeams);
                });
                return;
            }

            MainThread.BeginInvokeOnMainThread(() =>
            {
                Teams.Clear();
                LastGameDay.Clear();
                TopTenPlayer.Clear();
            });

            try
            {
                IsLoading = true;

                var teams = await Task.Run(() => NuLigaParser.ParseTeams(league) ?? []);

                _cachedLeagues.TryAdd(league.Name, teams);

                MainThread.BeginInvokeOnMainThread(() =>
                {
                    RefreshTeams(teams);
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

        private void RefreshTeams(Team[] teams)
        {
            if (!_cachedLeagues.TryGetValue(League?.Name ?? string.Empty, out var cachedTeams) || cachedTeams != teams)
            {
                return;
            }

            try
            {
                var lastGameDay = NuLigaTransformer.TransformTeamsToLastGameDay(teams);
                var allPlayers = NuLigaTransformer.TransformTeamsToAllPlayerList(teams);

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
                IsLoading = false;
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.ToString());
            }
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

        private async Task SortTeamsAsync()
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