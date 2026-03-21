using NuLigaViewer.ViewModels;

namespace NuLigaViewer.Pages
{
    public partial class PlayerPage : ContentPage, IQueryAttributable
    {
        public PlayerPage()
        {
            InitializeComponent();
        }

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            if (query.TryGetValue("leagueName", out var nameObj) && nameObj is string leagueName)
            {
                var leagueVm = LeagueViewModel.Get(leagueName);
                var player = leagueVm?.AllAvailablePlayer.FirstOrDefault(player => player.Name == query["playerName"]?.ToString());

                BindingContext = player != null ? new PlayerRow
                {
                    Brett = player.Brett,
                    Spieler = player.Name ?? string.Empty,
                    DWZ = player.DWZ == 1000 ? null : player.DWZ,
                    Rounds = [],
                    PlayerGameDayInfos = player.PlayerInfoPerGameDay?.Where(x => x != null).ToList() ?? []
                } : null;
            }
        }
    }
}