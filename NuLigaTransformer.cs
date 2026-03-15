using NuLigaViewer.Data;

namespace NuLigaViewer
{
    public static class NuLigaTransformer
    {
        public static List<TeamPairing> TransformTeamsToLastGameDay(Team[] teams)
        {
            if (teams.Length < 1)
            {
                return [];
            }

            var currentGameDayDate = teams[0].GameDays?.Last(gd => gd.ReportUrl != null)?.Datum;
            if (currentGameDayDate == null)
            {
                return [];
            }

            var currentGameDay = new List<TeamPairing>();
            foreach (var team in teams)
            {
                var teamPairing = team.GameDays?.FirstOrDefault(gd => gd.Datum == currentGameDayDate);
                if (teamPairing != null && !currentGameDay.Any(gd => gd.HeimMannschaft == teamPairing.HeimMannschaft && gd.GastMannschaft == teamPairing.GastMannschaft))
                {
                    currentGameDay.Add(teamPairing);
                }
            }

            return currentGameDay;
        }

        public static List<Player> TransformTeamsToAllPlayerList(Team[] teams)
        {
            if (teams.Length < 1)
            {
                return [];
            }

            var allPlayers = teams.Where(x => x.TeamPlayers != null).SelectMany(x => x.TeamPlayers!).ToList();
            allPlayers.Sort((a, b) =>
            {
                int pointsComparison = b.Points.CompareTo(a.Points);
                if (pointsComparison != 0)
                {
                    return pointsComparison;
                }
                return a.Games.CompareTo(b.Games);
            });

            return allPlayers;
        }
    }
}
