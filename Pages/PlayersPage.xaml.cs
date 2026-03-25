using NuLigaViewer.ViewModels;

namespace NuLigaViewer.Pages
{
    public partial class PlayersPage : ContentPage, IQueryAttributable
    {
        public PlayersPage()
        {
            InitializeComponent();
        }

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            if (query.TryGetValue("teamName", out var nameObj) && nameObj is string teamName)
            {
                var teamVm = NavigationState.SelectedLeagueViewModel.Teams.FirstOrDefault(team => team.Name == teamName);

                BindingContext = teamVm != null ? teamVm : null;
            }
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