using NuLigaCore.Data;

namespace NuLigaViewer
{
    public partial class TeamsPage : ContentPage, IQueryAttributable
    {
        public TeamsPage()
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

                BindingContext = ViewModels.TeamsViewModel.GetOrCreate(league);
            }
        }

        async void OnBackButtonClicked(object? sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("..");
        }

        protected override bool OnBackButtonPressed()
        {
            MainThread.BeginInvokeOnMainThread(async () => await Shell.Current.GoToAsync(".."));

            return true;
        }

        async void OnTeamSelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            var selected = e.CurrentSelection.FirstOrDefault() as ViewModels.TeamViewModel;
            if (selected is null)
                return;

            await Navigation.PushAsync(new PlayersPage(selected));

            if (sender is CollectionView cv)
            {
                cv.SelectedItem = null;
            }
        }
    }
}