using NuLigaCore.Data;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;

namespace NuLigaViewer.ViewModels
{
    public class LeaguesViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<League> Leagues { get; }

        public ObservableCollection<TeamViewModel> Teams { get; } = new();
        public ObservableCollection<GameDay> GameDays { get; } = new();

        private readonly Dictionary<string, List<Team>> _teamsCache = new();
        private readonly object _cacheLock = new();

        private readonly RelayCommand _collapseSelectionsCommand;
        public ICommand CollapseSelectionsCommand => _collapseSelectionsCommand;

        private League? _selectedLeague;
        public League? SelectedLeague
        {
            get => _selectedLeague;
            set
            {
                if (_selectedLeague != value)
                {
                    _selectedLeague = value;
                    OnPropertyChanged(nameof(SelectedLeague));

                    _ = LoadTeamsAsync(_selectedLeague);
                }
            }
        }

        private TeamViewModel? _selectedTeamView;
        public TeamViewModel? SelectedTeamView
        {
            get => _selectedTeamView;
            set
            {
                if (_selectedTeamView != value)
                {
                    _selectedTeamView = value;
                    OnPropertyChanged(nameof(SelectedTeamView));
                }
            }
        }

        private GameDay? _selectedGameDay;
        public GameDay? SelectedGameDay
        {
            get => _selectedGameDay;
            set
            {
                if (_selectedGameDay != value)
                {
                    _selectedGameDay = value;
                    OnPropertyChanged(nameof(SelectedGameDay));

                    _collapseSelectionsCommand.RaiseCanExecuteChanged();
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

        public LeaguesViewModel(IEnumerable<League> leagues)
        {
            Leagues = new ObservableCollection<League>(leagues.Where(l => l is not null).ToList());

            _collapseSelectionsCommand = new RelayCommand(CollapseSelectionsAsync, () => SelectedGameDay != null || SelectedTeamView != null);

            NuLigaParser.GameDayReportLoadedForGui += NuLigaParser_GameDayReportLoaded;
        }

        private void NuLigaParser_GameDayReportLoaded(GameDay gameDay)
        {
            if (Teams.Count == 0)
            {
                return; 
            }

            MainThread.BeginInvokeOnMainThread(() =>
            {
                var vm = Teams.FirstOrDefault(t => t.ContainsGameDay(gameDay));
                if (vm != null)
                {
                    vm.Refresh();
                }
            });
        }

        private IEnumerable<TeamViewModel> ToViewModels(IEnumerable<Team> teams)
        {
            return teams.Select(t => new TeamViewModel(t));
        }

        private async Task<List<Team>> LoadTeamsAsync(League? league)
        {
            if (league == null || string.IsNullOrWhiteSpace(league.Url))
            {
                return new List<Team>();
            }

            var key = league.Url!.Trim();

            List<Team>? cached;
            lock (_cacheLock)
            {
                _teamsCache.TryGetValue(key, out cached);
            }

            if (cached is not null)
            {
                var lastGameDayReport = NuLigaTransformer.TransformTeamsToGameDayReport(cached);

                MainThread.BeginInvokeOnMainThread(() =>
                {
                    Teams.Clear();
                    foreach (var vm in ToViewModels(cached))
                    {
                        Teams.Add(vm);
                    }
                    SelectedTeamView = null;

                    GameDays.Clear();
                    foreach (var gd in lastGameDayReport)
                    {
                        GameDays.Add(gd);
                    }

                    SelectedGameDay = null;
                    _collapseSelectionsCommand.RaiseCanExecuteChanged();
                });

                return cached;
            }

            try
            {
                IsLoading = true;

                var teams = await Task.Run(() =>
                {
                    return NuLigaParser.ParseTeams(league.Url) ?? new List<Team>();
                });

                lock (_cacheLock)
                {
                    _teamsCache[key] = teams;
                }

                var lastGameDayReport = NuLigaTransformer.TransformTeamsToGameDayReport(teams);

                MainThread.BeginInvokeOnMainThread(() =>
                {
                    Teams.Clear();
                    foreach (var vm in ToViewModels(teams))
                    {
                        Teams.Add(vm);
                    }
                    SelectedTeamView = null;

                    GameDays.Clear();
                    foreach (var gd in lastGameDayReport)
                    {
                        GameDays.Add(gd);
                    }

                    SelectedGameDay = null;
                    _collapseSelectionsCommand.RaiseCanExecuteChanged();
                });

                return teams;
            }
            catch (Exception)
            {
                // Consider logging or surface error via another property/command.
                return [];
            }
            finally
            {
                IsLoading = false;
            }
        }

        private Task CollapseSelectionsAsync()
        {
            SelectedGameDay = null;
            SelectedTeamView = null;
            return Task.CompletedTask;
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}