using NuLigaViewer.Data;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace NuLigaViewer.ViewModels
{
    public class TeamOverviewViewModel : INotifyPropertyChanged
    {
        private TeamViewModel? _selectedTeam;
        public TeamViewModel? SelectedTeam
        {
            get => _selectedTeam;
            set
            {
                if (_selectedTeam != value)
                {
                    _selectedTeam = value;
                    OnPropertyChanged(nameof(Name));
                    OnPropertyChanged(nameof(RoundHeaders));
                    OnPropertyChanged(nameof(PlayerRows));
                    OnPropertyChanged(nameof(GameDays));
                }
            }
        }


        public string? Name => SelectedTeam?.Name;

        public IReadOnlyList<string>? RoundHeaders => SelectedTeam?.RoundHeaders;
        public ObservableCollection<PlayerRow>? PlayerRows => SelectedTeam?.PlayerRows;

        public ObservableCollection<GameDayViewModel> GameDays { get; } = new();
        public ObservableCollection<ClubPlayerViewModel> ClubPlayers { get; } = new();

        public void LoadGameDays(TeamViewModel teamVm)
        {
            GameDays.Clear();
            foreach (var gameDay in teamVm.GameDays ?? [])
            {
                GameDays.Add(new GameDayViewModel(gameDay));
            }
        }

        public async Task LoadClubPlayersAsync(TeamViewModel teamVm)
        {
            try
            {
                var clubPlayers = await Task.Run(() => NuLigaParser.ParseAllClubPlayers(teamVm.ClubLineUpsUrl, teamVm.Name) ?? []);

                MainThread.BeginInvokeOnMainThread(() =>
                {
                    ClubPlayers.Clear();
                    foreach (var clubPlayer in clubPlayers)
                    {
                        ClubPlayers.Add(new ClubPlayerViewModel(clubPlayer));
                    }
                });
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.ToString());
            }
        }

        public void Refresh()
        {
            if (SelectedTeam == null)
            {
                return;
            }

            foreach (var gameDay in GameDays)
            {
                gameDay.Refresh();
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}