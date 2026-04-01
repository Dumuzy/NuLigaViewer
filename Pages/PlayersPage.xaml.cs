using NuLigaViewer.ViewModels;

namespace NuLigaViewer.Pages
{
    public partial class PlayersPage : ContentPage
    {
        public PlayersPage()
        {
            InitializeComponent();

            BindingContext = NavigationState.SelectedTeamOverview;
        }

        async void OnBackButtonClicked(object? sender, EventArgs e)
        {
            await Shell.Current.GoToAsync($"//league/table");
        }

        async void OnPlayerSelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            var playerRow = e.CurrentSelection.FirstOrDefault() as PlayerRow;
            if (playerRow is null)
            {
                return;
            }

            if (sender is CollectionView cv)
            {
                cv.SelectedItem = null;
            }

            var playerName = Uri.EscapeDataString(playerRow.Spieler ?? string.Empty);
            await Shell.Current.GoToAsync($"playerdetails?playerName={playerName}");
        }
    }
}