using HtmlAgilityPack;
using NuLigaViewer.Data;
using System.Globalization;

namespace NuLigaViewer
{
    public static class NuLigaParser
    {
        private static readonly string urlRoot = "https://bsv-schach.liga.nu/";

        public static event Action<League, TeamPairing>? TeamPairingReportLoadedForGui;

        private static readonly HtmlWeb web = new();

        public static List<BadenRegion> ParseLeagues()
        {
            var regions = new List<BadenRegion>();
            var badenLeagues = new List<string>
            {
                "Baden",
                "Mannheim",
                "Heidelberg",
                "Karlsruhe",
                "Pforzheim",
                "Mittelbaden",
                "Ortenau",
                "Odenwald",
                "Freiburg",
                "Hochrhein",
                "Schwarzwald",
                "Bodensee",
            };

            foreach (var region in badenLeagues)
            {
                try
                {
                    var url = $"{urlRoot}cgi-bin/WebObjects/nuLigaSCHACHDE.woa/wa/leaguePage?championship={region}+25%2F26";
                    regions.Add(new BadenRegion { Name = region, Leagues = ParseLeaguesFromUrl(web, url) });
                }
                catch (Exception e)
                {
                    System.Diagnostics.Debug.WriteLine(e.ToString());
                }
            }

            return regions;
        }

        private static List<League> ParseLeaguesFromUrl(HtmlWeb web, string url)
        {
            var doc = web.Load(url);
            var crossTableList = doc.DocumentNode.SelectNodes("//table[@class='matrix']");
            if (crossTableList == null || crossTableList.Count < 1)
            {
                return [];
            }

            var leagueList = crossTableList[0];
            var leagues = new List<League>();
            var rows = leagueList.SelectNodes(".//a[starts-with(@href, '/cgi')]");
            for (var row = 0; row < rows.Count; row++)
            {
                var league = new League
                {
                    Name = rows[row].InnerText.TrimStart('\n', '\t', ' ').TrimEnd('\n', '\t', ' '),
                    Url = urlRoot + rows[row].Attributes["href"].Value.TrimStart('/').Replace("amp;", ""),
                };
                leagues.Add(league);
            }

            return leagues;
        }

        public static Team[] ParseTeams(League league)
        {
            var doc = web.Load(league.Url);
            var crossTableList = doc.DocumentNode.SelectNodes("//table[@class='cross-table']");
            if (crossTableList == null || crossTableList.Count < 1)
            {
                return [];
            }

            var rankingTable = crossTableList[0];
            var rows = rankingTable.SelectNodes("tr");
            var numberOfTeams = rows.Count - 1; // BW Liga has 12 teams, others 10, KKC has 8 (+ header)
            var teams = new Team[numberOfTeams];

            // start with 1, skip headers in 0
            Parallel.For(1, rows.Count, row =>
            {
                var cells = rows[row].SelectNodes("th|td");
                var newTeam = ParseTeam(cells, row, numberOfTeams, league);
                teams[row - 1] = newTeam;
            });
            return teams;
        }

        private static Team ParseTeam(HtmlNodeCollection cells, int row, int numberOfTeams, League league)
        {
            var teamUrl = cells[2].QuerySelector("a").Attributes["href"].Value;

            var newTeam = new Team
            {
                Rang = int.Parse(cells[1].InnerText),
                Name = cells[2].InnerText,
                TeamUrl = string.IsNullOrEmpty(teamUrl) ? null : urlRoot + teamUrl,
                Spiele = int.Parse(cells[numberOfTeams + 3].InnerText),
                Punkte = int.Parse(cells[numberOfTeams + 4].InnerText),
                BP = double.Parse(cells[numberOfTeams + 5].InnerText),
                BoardPointsPerRank = new double[numberOfTeams - 1]
            };

            ParseGameDaysAndPlayers(newTeam, numberOfTeams, league);

            var rankIndex = 0;
            for (var i = 0; i < numberOfTeams; i++)
            {
                if (row == i + 1)
                {
                    continue;
                }
                var value = string.IsNullOrEmpty(cells[3 + i].InnerText) ? "0" : cells[3 + i].InnerText;
                newTeam.BoardPointsPerRank[rankIndex] = double.TryParse(value, out var result) ? result : 0;
                rankIndex++;
            }

            return newTeam;
        }

        private static void ParseGameDaysAndPlayers(Team newTeam, int numberOfTeams, League league)
        {
            if (newTeam.TeamUrl == null)
            {
                return;
            }

            var resultSetList = TryLoadWebResourceThreeTimes(web, newTeam.TeamUrl, "//table[@class='result-set']");

            newTeam.GameDays = ParseGameDays(resultSetList, newTeam.GameDayReportLoaded, league);
            newTeam.TeamPlayers = ParsePlayers(resultSetList, newTeam.Name, newTeam.GameDays?.Count ?? numberOfTeams - 1);
        }

        private static List<TeamPairing>? ParseGameDays(HtmlNodeCollection? resultSetList, Action<TeamPairing> gameDayReportLoaded, League league)
        {
            if (resultSetList == null || resultSetList.Count < 2)
            {
                var errorReason = resultSetList == null ? "resultSetList is null" : "resultSetList's count < 2";
                System.Diagnostics.Debug.WriteLine($"Error in loaded data for game days: {errorReason}");
                return null;
            }

            var gameDays = new List<TeamPairing>();
            var gameDayTable = resultSetList[1];

            var rows = gameDayTable.SelectNodes("tr");
            for (var row = 1; row < rows.Count; row++)
            {
                var cells = rows[row].SelectNodes("th|td");
                var date = cells[1].InnerText.TrimStart('\n', '\t', ' ').TrimEnd('\n', '\t', ' ');
                var round = cells[4].InnerText.TrimStart('\n', '\t', ' ').TrimEnd('\n', '\t', ' ');
                var homeTeam = cells[6].InnerText.TrimStart('\n', '\t', ' ').TrimEnd('\n', '\t', ' ');
                var guestTeam = cells[7].InnerText.TrimStart('\n', '\t', ' ').TrimEnd('\n', '\t', ' ').Replace("&nbsp;", "");
                var boardPoints = cells[8].InnerText.TrimStart('\n', '\t', ' ').TrimEnd('\n', '\t', ' ');
                var reportUrl = cells[8].QuerySelector("a")?.Attributes["href"].Value.TrimStart('/').Replace("amp;", "");

                var gameDay = new TeamPairing
                {
                    Datum = DateTime.ParseExact(date, "d", new CultureInfo("de-DE")),
                    RoundByCount = row,
                    Runde = int.Parse(round),
                    HeimMannschaft = homeTeam,
                    GastMannschaft = guestTeam,
                    BrettPunkte = boardPoints,
                    ReportUrl = string.IsNullOrEmpty(reportUrl) ? null : urlRoot + reportUrl
                };

                gameDays.Add(gameDay);

                _ = LoadGameReportAsync(gameDay, gameDayReportLoaded, league);
            }

            return gameDays;
        }

        private static async Task LoadGameReportAsync(TeamPairing? teamPairing, Action<TeamPairing> gameDayReportLoaded, League league)
        {
            if (teamPairing == null || string.IsNullOrWhiteSpace(teamPairing.ReportUrl))
            {
                return;
            }

            await Task.Run(() =>
            {
                var web = new HtmlWeb();
                var resultSetList = TryLoadWebResourceThreeTimes(web, teamPairing.ReportUrl, "//table[@class='result-set']");
                teamPairing.Report = ParseGameReport(resultSetList, teamPairing);

                gameDayReportLoaded(teamPairing);
                TeamPairingReportLoadedForGui?.Invoke(league, teamPairing);
            });
        }

        private static GameReport? ParseGameReport(HtmlNodeCollection? resultSetList, TeamPairing teamPairing)
        {
            if (resultSetList == null || resultSetList.Count < 1)
            {
                var errorReason = resultSetList == null ? "resultSetList is null" : "resultSetList's count < 1";
                System.Diagnostics.Debug.WriteLine($"Error in loaded data for the game report: {errorReason}");
                return null;
            }

            var pairings = new List<Pairing>();
            var gameReportTable = resultSetList[0];

            var rows = gameReportTable.SelectNodes("tr");
            for (var row = 1; row < rows.Count; row++)
            {
                var cells = rows[row].SelectNodes("th|td");
                if (cells.Count < 6)
                {
                    continue;
                }
                var homePlayerDWZ = cells[2].InnerText.TrimStart('\n', '\t', ' ').TrimEnd('\n', '\t', ' ');
                var guestPlayerDWZ = cells[4].InnerText.TrimStart('\n', '\t', ' ').TrimEnd('\n', '\t', ' ');

                var pairing = new Pairing
                {
                    Brett = int.Parse(cells[0].InnerText.TrimStart('\n', '\t', ' ').TrimEnd('\n', '\t', ' ')),
                    HeimSpieler = cells[1].InnerText.TrimStart('\n', '\t', ' ').TrimEnd('\n', '\t', ' '),
                    HeimSpielerDWZ = int.Parse(string.IsNullOrEmpty(homePlayerDWZ) ? "1000" : homePlayerDWZ),
                    GastSpieler = cells[3].InnerText.TrimStart('\n', '\t', ' ').TrimEnd('\n', '\t', ' '),
                    GastSpielerDWZ = int.Parse(string.IsNullOrEmpty(guestPlayerDWZ) ? "1000" : guestPlayerDWZ),
                    Ergebnis = cells[5].InnerText.TrimStart('\n', '\t', ' ').TrimEnd('\n', '\t', ' '),
                    RelatedTeamPairing = teamPairing
                };
                pairings.Add(pairing);
            }

            return new GameReport { Pairings = pairings };
        }

        private static List<Player>? ParsePlayers(HtmlNodeCollection? resultSetList, string teamName, int numberOfGameDays)
        {
            if (resultSetList == null || resultSetList.Count < 3)
            {
                var errorReason = resultSetList == null ? "resultSetList is null" : "resultSetList's count < 3";
                System.Diagnostics.Debug.WriteLine($"Error in loaded data for the players: {errorReason}");
                return null;
            }

            var players = new List<Player>();
            var playerTable = resultSetList[2];

            var rows = playerTable.SelectNodes("tr");
            for (var row = 0; row < rows.Count; row++)
            {
                var cells = rows[row].SelectNodes("th|td");
                if (cells.Count < 6 || cells[0].InnerText == "Brett" || int.Parse(cells[4].InnerText) < 1)
                {
                    continue;
                }

                var player = new Player
                {
                    Brett = int.Parse(cells[0].InnerText),
                    Name = cells[1].InnerText.Trim().TrimStart('\n').TrimEnd('\n').Trim(),
                    DWZ = int.Parse(string.IsNullOrEmpty(cells[3].InnerText) ? "1000" : cells[3].InnerText),
                    Games = int.Parse(cells[4].InnerText),
                    BoardPoints = cells[5].InnerText.TrimStart('\n', '\t', ' ').TrimEnd('\n', '\t', ' '),
                    TeamName = teamName,
                    PlayerInfoPerGameDay = new PlayerGameDayInfo[numberOfGameDays]
                };
                players.Add(player);
            }

            return players;
        }

        private static HtmlNodeCollection? TryLoadWebResourceThreeTimes(HtmlWeb web, string url, string nodeRequest)
        {
            for (var attempt = 1; attempt <= 3; attempt++)
            {
                try
                {
                    var teamDoc = web.Load(url);
                    var resultSetList = teamDoc.DocumentNode.SelectNodes(nodeRequest);
                    if (resultSetList != null)
                    {
                        return resultSetList;
                    }
                    Thread.Sleep(10);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Attempt {attempt} to load {url} failed: {ex.Message}");
                }
            }

            return null;
        }

        public static List<ClubPlayer> ParseAllClubPlayers(string? teamUrl)
        {
            if (string.IsNullOrEmpty(teamUrl))
            {
                return [];
            }

            var teamDoc = web.Load(teamUrl);
            var teamInfoTable = teamDoc.DocumentNode.SelectNodes("//table[@class='result-set']")[0];
            var teamMainPageUrl = urlRoot + teamInfoTable.SelectNodes("tr")[0].SelectNodes("th|td")[1].QuerySelector("a").Attributes["href"].Value;

            var clubPage = web.Load(teamMainPageUrl);
            var clubPageLinks = clubPage.DocumentNode.SelectNodes("//table")[0].SelectNodes("tr")[0].SelectNodes("th|td")[0].SelectNodes("ul")[0].SelectNodes("li");
            var clubLineUpsUrl = urlRoot + clubPageLinks[2].QuerySelector("a").Attributes["href"].Value;

            var clubLineUpsPage = web.Load(clubLineUpsUrl);
            var lineUps = clubLineUpsPage.DocumentNode.SelectNodes("//table[@class='result-set']")[0].SelectNodes("tr");
            if (lineUps == null)
            {
                return [];
            }

            var lineUpUrl = string.Empty;
            for (var lineUpIndex = 0; lineUpIndex < lineUps.Count; lineUpIndex++)
            {
                var cells = lineUps[lineUpIndex].SelectNodes("th|td");

                var lineUpFound = cells.Any(x => x.InnerHtml.Contains("Punktspielbetrieb")
                                        && x.InnerHtml.Contains("2025/26")
                                        && !x.InnerHtml.Contains("Senioren")
                                        && !x.InnerHtml.Contains("Jugend")
                                        && !x.InnerHtml.Contains("Pokal"));
                if (lineUpFound && lineUpIndex + 2 < lineUps.Count)
                {
                    var lineUpRoute = lineUps[lineUpIndex + 2].SelectNodes("th|td")[1].QuerySelector("a").Attributes["href"].Value.Replace("&amp;", "&");
                    lineUpUrl = urlRoot + lineUpRoute.TrimStart('/');
                    break;
                }
            }

            var lineUpPage = web.Load(lineUpUrl);
            var playerRows = lineUpPage.DocumentNode.SelectNodes("//table[@class='result-set']")[0].SelectNodes("tr");
            var clubPlayers = new List<ClubPlayer>();

            // start with 1, skip headers in 0
            for (var row = 1; row < playerRows.Count; row++)
            {
                var cells = playerRows[row].SelectNodes("th|td");

                var dwz = int.TryParse(cells[1].InnerText, out var parsedDWZ) ? parsedDWZ : (int?)null;
                var number = int.TryParse(cells[3].InnerText, out var parsedNumber) ? parsedNumber : (int?)null;

                var player = new ClubPlayer
                {
                    Rang = int.Parse(cells[0].InnerText),
                    DWZ = dwz,
                    Name = cells[2].InnerText.Trim().TrimStart('\n').TrimEnd('\n').Trim(),
                    Number = number,
                    Status = cells[5].InnerText.Trim().TrimStart('\n').TrimEnd('\n').Trim()
                };
                clubPlayers.Add(player);
            }
            return clubPlayers;
        }
    }
}
