using NuLigaViewer.Data;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;

namespace NuLigaViewer.ViewModels
{
    public class TeamViewModel : INotifyPropertyChanged
    {
        private readonly Team _team;

        public TeamViewModel(Team team)
        {
            _team = team ?? throw new ArgumentNullException(nameof(team));
            BuildPlayerRows();
        }

        public int Rank => _team.Rang;
        public string Name => _team.Name;
        public int Games => _team.Spiele;
        public int Points => _team.Punkte;
        public double BoardPointsSum => _team.BP;
        public double AverageDwz => _team.DWZ;
        public double BerlinTieBreak => _team.BW;
        public ObservableCollection<PlayerRow> PlayerRows { get; } = new();
        public IReadOnlyList<string> RoundHeaders { get; private set; } = Array.Empty<string>();

        public IEnumerable<Player> Players => _team.TeamPlayers ?? Enumerable.Empty<Player>();
        public IList<GameDay>? GameDays => _team.GameDays;

        public bool ContainsGameDay(GameDay gameDay) =>
            _team.GameDays != null && _team.GameDays.Contains(gameDay);

        private void BuildPlayerRows()
        {
            PlayerRows.Clear();

            // determine number of rounds from team.GameDays or first player's PunkteProSpieltag length
            var rounds = _team.GameDays?.Count
                         ?? _team.TeamPlayers?.FirstOrDefault()?.PunkteProSpieltag?.Length
                         ?? 0;

            RoundHeaders = Enumerable.Range(1, Math.Max(0, rounds)).Select(i => i.ToString()).ToList();

            foreach (var p in _team.TeamPlayers ?? Enumerable.Empty<Player>())
            {
                var row = new PlayerRow
                {
                    Brett = p.Brett,
                    Spieler = p.Name ?? string.Empty,
                    DWZ = p.DWZ,
                    Rounds = [],
                };

                var totalPoints = 0.0;
                var totalGames = 0;

                for (var i = 0; i < rounds; i++)
                {
                    var pointsString = "-";
                    if (p.PunkteProSpieltag != null && i < p.PunkteProSpieltag.Length)
                    {
                        var points = p.PunkteProSpieltag[i];
                        pointsString = points == -1 ? "-" : (points == 1000 ? "+" : points.ToString());
                        if (points >= 0)
                        {
                            totalPoints += (points == 1000 ? 1 : points);
                            totalGames++;
                        }
                    }

                    row.Rounds.Add(pointsString);
                }

                row.Total = $"{totalPoints}/{totalGames}";

                PlayerRows.Add(row);
            }

            OnPropertyChanged(nameof(RoundHeaders));
            OnPropertyChanged(nameof(PlayerRows));
        }

        public void Refresh()
        {
            OnPropertyChanged(nameof(Rank));
            OnPropertyChanged(nameof(Name));
            OnPropertyChanged(nameof(Games));
            OnPropertyChanged(nameof(Points));
            OnPropertyChanged(nameof(BoardPointsSum));
            OnPropertyChanged(nameof(AverageDwz));
            OnPropertyChanged(nameof(BerlinTieBreak));

            BuildPlayerRows();
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public class PlayerRow
    {
        public int Brett { get; set; }
        public string Spieler { get; set; } = string.Empty;
        public int DWZ { get; set; }
        public List<string> Rounds { get; set; } = new();
        public string Total { get; set; } = string.Empty;
    }
}