using NuLigaViewer.ViewModels;

namespace NuLigaViewer.Pages
{
    public partial class LastGameDayPage : ContentPage
    {
        public LastGameDayPage()
        {
            InitializeComponent();

            BindingContext = NavigationState.SelectedLeagueViewModel;
        }

        async void OnBackButtonClicked(object? sender, EventArgs e)
        {
            await Shell.Current.GoToAsync($"//home");
        }

        public async void OnSelectedTeamPairingChanged(object? sender, SelectionChangedEventArgs e)
        {
            var selectedTeamPairing = e.CurrentSelection.FirstOrDefault() as TeamPairingViewModel;
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