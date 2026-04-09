namespace NuLigaViewer.Data
{
    public class BadenRegion
    {
        public required string Name { get; set; }
        public required List<League> Leagues { get; set; }
    }

    public class League
    {
        public required string Name { get; set; }
        public required string Url { get; set; }
    }
}