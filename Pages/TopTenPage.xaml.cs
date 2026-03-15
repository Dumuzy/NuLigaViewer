using NuLigaViewer.Data;
using NuLigaViewer.ViewModels;

namespace NuLigaViewer.Pages
{
    public partial class TopTenPage : ContentPage, IQueryAttributable
    {
        public TopTenPage()
        {
            InitializeComponent();
        }

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            string? url = null;
            string? name = null;

            if (query.TryGetValue("leagueUrl", out var urlObj) && urlObj is string u)
            {
                url = Uri.UnescapeDataString(u);
            }

            if (query.TryGetValue("leagueName", out var nameObj) && nameObj is string n)
            {
                name = Uri.UnescapeDataString(n);
            }

            if (string.IsNullOrWhiteSpace(url) && !string.IsNullOrWhiteSpace(NavigationState.LastLeagueUrl))
            {
                url = NavigationState.LastLeagueUrl;
            }

            if (string.IsNullOrWhiteSpace(name) && !string.IsNullOrWhiteSpace(NavigationState.LastLeagueName))
            {
                name = NavigationState.LastLeagueName;
            }

            if (!string.IsNullOrWhiteSpace(url))
            {
                var league = new League
                {
                    Url = url,
                    Name = name ?? string.Empty
                };

                BindingContext = LeagueViewModel.GetOrCreate(league);
            }
        }

        async void OnBackButtonClicked(object? sender, EventArgs e)
        {
            await Shell.Current.GoToAsync($"//home");
        }

        async void OnTeamSelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
        }
    }
}