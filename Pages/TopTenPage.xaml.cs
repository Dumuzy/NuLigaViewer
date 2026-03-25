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

        public async void OnTeamSelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
        }
    }
}