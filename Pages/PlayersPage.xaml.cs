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
            if (query.TryGetValue("leagueName", out var nameObj) && nameObj is string leagueName)
            {
                var leagueVm = LeagueViewModel.Get(leagueName);
                var teamVm = leagueVm?.Teams.FirstOrDefault(team => team.Name == query["teamName"]?.ToString());

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

            var leagueName = Uri.EscapeDataString(NavigationState.LastLeagueName ?? string.Empty);
            var playerName = Uri.EscapeDataString(playerRow.Spieler ?? string.Empty);
            await Shell.Current.GoToAsync($"playerdetails?leagueName={leagueName}&playerName={playerName}");
        }
    }
}