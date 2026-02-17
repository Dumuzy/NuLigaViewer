namespace NuLigaViewer.Data
{
    public class GameDay
    {
        public DateTime Datum { get; set; }
        public int Runde { get; set; }
        public string Title => $"{Runde}. Spieltag am {Datum.ToShortDateString()}";
        public string? HeimMannschaft { get; set; }
        public double HeimMannschaftDWZ => (Report != null) ? Report.AverageHomeDWZ : 0;
        public string? GastMannschaft { get; set; }
        public double GastMannschaftDWZ => (Report != null) ? Report.AverageGuestDWZ : 0;
        public string? BrettPunkte { get; set; }
        public string? ReportUrl { get; set; }
        public GameReport? Report { get; set; }

        public override string ToString()
        {
            return $"Round {Runde} on {Datum.ToShortDateString()}: {HeimMannschaft} vs {GastMannschaft} - BoardPoints: {BrettPunkte}";
        }
    }
}