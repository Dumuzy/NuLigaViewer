using NuLigaViewer.ViewModels;

namespace NuLigaViewer.Pages
{
    public partial class TopTenPage : ContentPage
    {
        public TopTenPage()
        {
            InitializeComponent();

            BindingContext = NavigationState.SelectedLeagueViewModel;
        }

        public async void OnBackButtonClicked(object? sender, EventArgs e)
        {
            await Shell.Current.GoToAsync($"//home");
        }

        async void OnPlayerSelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            var playerRow = e.CurrentSelection.FirstOrDefault() as TopTenPlayerViewModel;
            if (playerRow is null)
            {
                return;
            }

            if (sender is CollectionView cv)
            {
                cv.SelectedItem = null;
            }

            var playerName = Uri.EscapeDataString(playerRow.Name ?? string.Empty);
            await Shell.Current.GoToAsync($"playerdetails?playerName={playerName}");
        }
    }
}