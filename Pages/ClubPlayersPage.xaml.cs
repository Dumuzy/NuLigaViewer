namespace NuLigaViewer.Pages
{
    public partial class ClubPlayersPage : ContentPage
    {
        public ClubPlayersPage()
        {
            InitializeComponent();

            BindingContext = NavigationState.SelectedTeamOverview;
        }

        async void OnBackButtonClicked(object? sender, EventArgs e)
        {
            await Shell.Current.GoToAsync($"//league/table");
        }

        async void OnPlayerSelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
        }
    }
}