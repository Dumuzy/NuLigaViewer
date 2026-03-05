using System.Text.Json.Serialization;

namespace NuLigaViewer.Data
{
    public class Player
    {
        public int Brett { get; set; }
        public string? Name { get; set; }
        public int DWZ { get; set; }

        [JsonIgnore]
        public int Games { get; set; }

        public PlayerGameDayInfo?[]? PlayerInfoPerGameDay { get; set; }

        public override string ToString()
        {
            return $"{Brett}. {Name} (DWZ: {DWZ}) - Games: {Games}";
        }
    }

    public class PlayerGameDayInfo
    {
        public Pairing? Pairing { get; set; }
        public double Points { get; set; } = -1;
        public bool PlayerIsInHomeTeam { get; set; }
        public string? ResultForPlayer => Pairing?.BoardPoints.AsString(PlayerIsInHomeTeam);
        public string? Opponent => PlayerIsInHomeTeam ? Pairing?.GastSpieler : Pairing?.HeimSpieler;
        public int? OpponentDWZ => PlayerIsInHomeTeam ? Pairing?.GastSpielerDWZ : Pairing?.HeimSpielerDWZ;
        public string? OpponentDWZString => PlayerIsInHomeTeam ? Pairing?.VisibleGuestDWZ : Pairing?.VisibleHomeDWZ;
        public string? OpponentTeam => PlayerIsInHomeTeam ? Pairing?.RelatedGameDay?.GastMannschaft : Pairing?.RelatedGameDay?.HeimMannschaft;
    }
}