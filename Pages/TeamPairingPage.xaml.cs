using NuLigaViewer.ViewModels;

namespace NuLigaViewer.Pages;

public partial class TeamPairingPage : ContentPage, IQueryAttributable
{
	public TeamPairingPage()
	{
		InitializeComponent();
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("homeTeam", out var homeNameObj) && homeNameObj is string homeTeam
            && query.TryGetValue("guestTeam", out var guestNameObj) && guestNameObj is string guestTeam)
        {
            var teamPairing = NavigationState.SelectedLeagueViewModel.AllAvailableTeamPairings
                .FirstOrDefault(tp => tp.HeimMannschaft == homeTeam && tp.GastMannschaft == guestTeam);

            BindingContext = teamPairing != null ? new TeamPairingViewModel(teamPairing): null;
        }
    }
}