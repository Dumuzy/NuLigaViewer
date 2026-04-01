using NuLigaViewer.ViewModels;

namespace NuLigaViewer
{
    public static class NavigationState
    {
        public static LeagueViewModel SelectedLeagueViewModel { get; } = new LeagueViewModel();
        public static TeamOverviewViewModel SelectedTeamOverview { get; } = new TeamOverviewViewModel();
    }
}