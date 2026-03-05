namespace NuLigaViewer.Data
{
    public class Pairing
    {
        public int Brett { get; set; }
        public string? HeimSpieler { get; set; }
        public int HeimSpielerDWZ { get; set; }
        public string VisibleHomeDWZ => HeimSpieler?.Contains("nicht anwesend") == true ? "" : HeimSpielerDWZ.ToString();
        public string? GastSpieler { get; set; }
        public int GastSpielerDWZ { get; set; }
        public string VisibleGuestDWZ => GastSpieler?.Contains("nicht anwesend") == true ? "" : GastSpielerDWZ.ToString();
        public string? Ergebnis { get; set; }
        public BoardPoints BoardPoints => Ergebnis?.AsBoardPoints() ?? BoardPoints.NotPlayed;
        public GameDay? RelatedGameDay { get; set; }
    }
}