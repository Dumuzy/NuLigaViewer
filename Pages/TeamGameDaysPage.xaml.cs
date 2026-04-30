using NuLigaViewer.ViewModels;

namespace NuLigaViewer.Pages
{
    public partial class TeamGameDaysPage : ContentPage
    {
        public TeamGameDaysPage()
        {
            InitializeComponent();

            BindingContext = NavigationState.SelectedTeamOverview;
        }

        async void OnBackButtonClicked(object? sender, EventArgs e)
        {
            await Shell.Current.GoToAsync($"//league/table");
        }

        public async void OnSelectedTeamPairingChanged(object? sender, SelectionChangedEventArgs e)
        {
            var selectedTeamPairing = e.CurrentSelection.FirstOrDefault() as GameDayViewModel;
            if (selectedTeamPairing is null)
            {
                return;
            }

            if (sender is CollectionView cv)
            {
                cv.SelectedItem = null;
            }

            var homeTeam = Uri.EscapeDataString(selectedTeamPairing.HeimMannschaft ?? string.Empty);
            var guestTeam = Uri.EscapeDataString(selectedTeamPairing.GastMannschaft ?? string.Empty);
            await Shell.Current.GoToAsync($"teampairing?homeTeam={homeTeam}&guestTeam={guestTeam}");
        }
    }
}