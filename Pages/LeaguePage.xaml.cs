using NuLigaViewer.Data;

namespace NuLigaViewer.Pages
{
    public partial class LeaguePage : ContentPage, IQueryAttributable
    {
        public LeaguePage()
        {
            InitializeComponent();
        }

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            if (query.TryGetValue("leagueUrl", out var urlObj) && urlObj is string url)
            {
                var leagueName = query.TryGetValue("leagueName", out var nameObj) && nameObj is string n
                    ? Uri.UnescapeDataString(n)
                    : string.Empty;

                var league = new League
                {
                    Url = Uri.UnescapeDataString(url),
                    Name = leagueName
                };

                BindingContext = ViewModels.LeagueViewModel.GetOrCreate(league);
            }
        }

        async void OnBackButtonClicked(object? sender, EventArgs e)
        {
            await Shell.Current.GoToAsync($"//home");
        }

        async void OnTeamSelectionChanged(object? sender, SelectionChangedEventArgs e)
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

            var leagueName = Uri.EscapeDataString(NavigationState.LastLeagueName ?? string.Empty);
            var teamName = Uri.EscapeDataString(teamVm.Name ?? string.Empty);
            await Shell.Current.GoToAsync($"teamplayers?leagueName={leagueName}&teamName={teamName}");
        }
    }
}