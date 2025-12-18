using System.Linq;
using System;
using Microsoft.Maui.Controls;

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

        async void OnLeagueSelected(object sender, SelectionChangedEventArgs e)
        {
            var league = e.CurrentSelection?.FirstOrDefault() as NuLigaCore.Data.League;
            if (league == null)
            {
                return;
            }

            // clear selection to allow re-selection later
            if (sender is CollectionView cv)
            {
                cv.SelectedItem = null;
            }

            var url = Uri.EscapeDataString(league.Url ?? string.Empty);
            var name = Uri.EscapeDataString(league.Name ?? string.Empty);

            // Navigate using Shell and pass league data via query parameters
            await Shell.Current.GoToAsync($"{nameof(TeamsPage)}?leagueUrl={url}&leagueName={name}");
        }
    }
}
