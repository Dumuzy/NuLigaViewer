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
        }

        protected async override void OnNavigating(ShellNavigatingEventArgs e)
        {
            base.OnNavigating(e);

            if (e.Source != ShellNavigationSource.ShellSectionChanged || e.Current is null || e.Target is null)
            {
                return;
            }

            var targetPath = e.Target.Location?.ToString() ?? string.Empty;
            var levels = targetPath.Trim('/').Count('/') - 1;

            if (levels < 1)
            {
                return;
            }

            if (!(targetPath.StartsWith("//league/table")
                || targetPath.StartsWith("//league/lastgameday")
                || targetPath.StartsWith("//league/topten")))
            {
                return;
            }

            if (e.Cancel())
            {
                try
                {
                    if (targetPath.StartsWith("//league/table"))
                    {
                        await LeagueTab.Navigation.PopToRootAsync();
                    }
                    else if (targetPath.StartsWith("//league/lastgameday"))
                    {
                        await LastGameDayTab.Navigation.PopToRootAsync();
                    }
                    else if (targetPath.StartsWith("//league/topten"))
                    {
                        await TopTenTab.Navigation.PopToRootAsync();
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(ex);
                }
            }
        }
    }
}
