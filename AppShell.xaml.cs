namespace NuLigaViewer
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            Routing.RegisterRoute("teampairing", typeof(Pages.TeamPairingPage));
            Routing.RegisterRoute("teamplayers", typeof(Pages.PlayersPage));
            Routing.RegisterRoute("playerdetails", typeof(Pages.PlayerPage));

            Navigated += AppShell_Navigated;
        }

        private async void AppShell_Navigated(object? sender, ShellNavigatedEventArgs e)
        {
            try
            {
                var location = e.Current?.Location?.ToString() ?? string.Empty;
                if (string.IsNullOrEmpty(location))
                    return;

                if ((location.EndsWith("league/table", StringComparison.OrdinalIgnoreCase)
                    || location.EndsWith("league/lastgameday", StringComparison.OrdinalIgnoreCase)
                    || location.EndsWith("league/topten", StringComparison.OrdinalIgnoreCase))
                    && !string.IsNullOrWhiteSpace(NavigationState.LastLeagueUrl))
                {
                    var trimmed = location.TrimStart('/');
                    var route = $"//{trimmed}?leagueUrl={Uri.EscapeDataString(NavigationState.LastLeagueUrl)}&leagueName={Uri.EscapeDataString(NavigationState.LastLeagueName ?? string.Empty)}";

                    await Shell.Current.GoToAsync(route);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
            }
        }
    }
}
