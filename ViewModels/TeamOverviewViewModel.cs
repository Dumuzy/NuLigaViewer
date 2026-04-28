using NuLigaViewer.Data;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace NuLigaViewer.ViewModels
{
    public class TeamOverviewViewModel : INotifyPropertyChanged
    {
        public class TeamOverViewVieModel()
        {
        }

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
        public IList<TeamPairing>? GameDays => SelectedTeam?.GameDays;


        public ObservableCollection<ClubPlayerViewModel> ClubPlayers { get; } = new();

        public async Task LoadClubPlayersAsync(TeamViewModel teamVm)
        {
            if (teamVm == null)
            {
                return;
            }

            SelectedTeam = teamVm;

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

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}