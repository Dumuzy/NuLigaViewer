using NuLigaViewer.Data;
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
            if (query.TryGetValue("playerName", out var nameObj) && nameObj is string playerName)
            {
                var player = NavigationState.SelectedLeagueViewModel.AllAvailablePlayer.FirstOrDefault(player => player.Name == playerName);

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

        async void OnPlayerSelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            var playerInfo = e.CurrentSelection.FirstOrDefault() as PlayerGameDayInfo;
            if (playerInfo is null)
            {
                return;
            }

            if (sender is CollectionView cv)
            {
                cv.SelectedItem = null;
            }

            var playerName = Uri.EscapeDataString(playerInfo.Opponent ?? string.Empty);
            await Shell.Current.GoToAsync($"playerdetails?playerName={playerName}");
        }
    }
}