namespace NuLigaViewer.Data
{
    public class GameReport
    {
        public List<Pairing> Pairings { get; set; } = [];
        public IEnumerable<Pairing> HomePairingsForAverage => Pairings.Where(x => x.BoardPoints != BoardPoints.NotPlayed && x.HeimSpieler?.Contains("nicht anwesend") == false);
        public IEnumerable<Pairing> GuestPairingsForAverage => Pairings.Where(x => x.BoardPoints != BoardPoints.NotPlayed && x.GastSpieler?.Contains("nicht anwesend") == false);

        public double AverageHomeDWZ => HomePairingsForAverage.Any() ? Math.Round(HomePairingsForAverage.Average(x => x.HeimSpielerDWZ)) : 0;
        public double AverageGuestDWZ => GuestPairingsForAverage.Any() ? Math.Round(GuestPairingsForAverage.Average(x => x.GastSpielerDWZ)) : 0;

        public IEnumerable<Pairing> GetPairingForPlayer(string? playerName, bool forHomeTeam)
        {
            return forHomeTeam
                ? Pairings.Where(p => p.HeimSpieler == playerName)
                : Pairings.Where(p => p.GastSpieler == playerName);
        }

        public double ComputeBw(bool forHomeTeam)
        {
            var boardCount = Pairings.Count;
            var bwTotal = 0.0;
            foreach (var pairing in Pairings)
            {
                var points = (boardCount + 1) - pairing.Brett;
                bwTotal += points * FactorForBoardResult(pairing.BoardPoints, forHomeTeam);
            }

            return bwTotal;
        }

        private static double FactorForBoardResult(BoardPoints boardPoints, bool forHomeTeam)
        {
            if (!forHomeTeam)
            {
                return boardPoints switch
                {
                    BoardPoints.GuestWin => 1.0,
                    BoardPoints.GuestWinByDefault => 1.0,
                    BoardPoints.Draw => 0.5,
                    _ => 0.0,
                };
            }
            return boardPoints switch
            {
                BoardPoints.HomeWin => 1.0,
                BoardPoints.HomeWinByDefault => 1.0,
                BoardPoints.Draw => 0.5,
                _ => 0.0,
            };
        }
    }
}