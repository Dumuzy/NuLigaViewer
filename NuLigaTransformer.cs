using NuLigaViewer.Data;

namespace NuLigaViewer
{
    public static class NuLigaTransformer
    {
        public static List<GameDay> TransformTeamsToLastGameDayReport(Team[] teams)
        {
            if (teams.Length < 1)
            {
                return [];
            }

            var currentGameDay = teams[0].GameDays?.Last(gd => gd.ReportUrl != null)?.Datum;
            if (currentGameDay == null)
            {
                return [];
            }

            var currentGameDayReport = new List<GameDay>();
            foreach (var team in teams)
            {
                var gameDay = team.GameDays?.FirstOrDefault(gd => gd.Datum == currentGameDay);
                if (gameDay != null && !currentGameDayReport.Any(gd => gd.HeimMannschaft == gameDay.HeimMannschaft && gd.GastMannschaft == gameDay.GastMannschaft))
                {
                    currentGameDayReport.Add(gameDay);
                }
            }

            return currentGameDayReport;
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
