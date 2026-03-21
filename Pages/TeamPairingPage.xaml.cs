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
        if (query.TryGetValue("leagueName", out var nameObj) && nameObj is string leagueName)
        {
            var leagueVm = LeagueViewModel.Get(leagueName);
            var teamPairing = leagueVm?.AllAvailableTeamPairings
                .FirstOrDefault(tp => tp.HeimMannschaft == query["homeTeam"]?.ToString() && tp.GastMannschaft == query["guestTeam"]?.ToString());

            BindingContext = teamPairing != null ? new TeamPairingViewModel(teamPairing): null;
        }
    }
}