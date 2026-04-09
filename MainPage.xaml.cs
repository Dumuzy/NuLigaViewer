using NuLigaViewer.Data;

namespace NuLigaViewer
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();

            var leagues = NuLigaParser.ParseLeagues();
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
