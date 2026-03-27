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

            if (e.Source != ShellNavigationSource.ShellSectionChanged || e.Current is null || e.Target is null || e.Target.Location == _pageBeingPopped?.Location)
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

        protected override void OnNavigated(ShellNavigatedEventArgs args)
        {
            base.OnNavigated(args);
            if (Uri == null || args.Previous == null)
            {
                return;
            }

            if (args.Current != null && args.Current.Location.ToString() == "//home")
            {
                Uri.Clear();
                return;
            }

            if (_pageBeingPopped == null || _pageBeingPopped.Location != args.Current?.Location)
            {
                Uri.Push(args.Previous);
                _pageBeingPopped = null;
            }
        }

        protected override bool OnBackButtonPressed()
        {
            if (Uri != null && Uri.Count > 0)
            {
                Dispatcher.Dispatch(async () =>
                {
                    await GoBackInStack();
                });
                return true;
            }
            return base.OnBackButtonPressed();
        }

        public async static Task GoBackInStack()
        {
            if (Uri != null && Uri.Count > 0)
            {
                var previousPage = Uri.Pop();
                _pageBeingPopped = previousPage;
                await Shell.Current.GoToAsync(previousPage);
            }
        }
        private static Stack<ShellNavigationState> Uri { get; } = new Stack<ShellNavigationState>();
        private static ShellNavigationState? _pageBeingPopped { get; set; }
    }
}