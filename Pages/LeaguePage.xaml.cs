namespace NuLigaViewer.Pages
{
    public partial class LeaguePage : ContentPage
    {
        public LeaguePage()
        {
            InitializeComponent();

            BindingContext = NavigationState.SelectedLeagueViewModel;
        }

        async void OnBackButtonClicked(object? sender, EventArgs e)
        {
            await Shell.Current.GoToAsync($"//home");
        }

        public async void OnTeamSelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            var teamVm = e.CurrentSelection.FirstOrDefault() as ViewModels.TeamViewModel;
            if (teamVm is null)
            {
                return;
            }

            if (sender is CollectionView cv)
            {
                cv.SelectedItem = null;
            }

            teamVm.Refresh();
            NavigationState.SelectedTeamOverview.SelectedTeam = teamVm;
            NavigationState.SelectedTeamOverview.LoadGameDays(teamVm);
            _ = NavigationState.SelectedTeamOverview.LoadClubPlayersAsync(teamVm);
            await Shell.Current.GoToAsync($"//team/gamedays");
        }
    }
}