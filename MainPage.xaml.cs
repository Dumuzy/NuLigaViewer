namespace NuLigaViewer
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();

            List<Data.League>? leagues = null;

            try
            {
                leagues = NuLigaParser.ParseLeagues();
            }
            catch { }

            BindingContext = new ViewModels.LeaguesViewModel(leagues);
        }

        async void OnLeagueSelected(object sender, SelectionChangedEventArgs e)
        {
            var league = (e.CurrentSelection?.FirstOrDefault() as Data.League);
            if (league == null)
            {
                return;
            }

            if (sender is CollectionView cv)
            {
                cv.SelectedItem = null;
            }

            var url = Uri.EscapeDataString(league.Url ?? string.Empty);
            var name = Uri.EscapeDataString(league.Name ?? string.Empty);

            NavigationState.LastLeagueUrl = league.Url;
            NavigationState.LastLeagueName = league.Name;

            await Shell.Current.GoToAsync($"//league/table?leagueUrl={url}&leagueName={name}");
        }
    }
}
