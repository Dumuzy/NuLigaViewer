using NuLigaViewer.Data;

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
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.ToString());
            }

            BindingContext = new ViewModels.LeaguesViewModel(leagues);
        }

        public async void OnLeagueSelected(object sender, SelectionChangedEventArgs e)
        {
            var selectedLeague = (e.CurrentSelection?.FirstOrDefault() as League);
            if (selectedLeague == null)
            {
                return;
            }

            if (sender is CollectionView cv)
            {
                cv.SelectedItem = null;
            }

            _ = NavigationState.SelectedLeagueViewModel.LoadLeagueAsync(selectedLeague);
            await Shell.Current.GoToAsync($"//league/table");
        }
    }
}
