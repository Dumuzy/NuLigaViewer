namespace NuLigaViewer
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            Routing.RegisterRoute(nameof(TeamsPage), typeof(TeamsPage));
        }
    }
}
