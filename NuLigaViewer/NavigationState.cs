using NuLigaViewer.ViewModels;

namespace NuLigaViewer
{
    public static class NavigationState
    {
        public static LeagueViewModel SelectedLeagueViewModel { get; } = new LeagueViewModel();
    }
}