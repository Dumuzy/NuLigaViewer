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

        public int RankAfterSorting { get; set; }
        public int Rank => _team.Rang;
        public string Name => _team.Name;
        public int Games => _team.Spiele;
        public int Points => _team.Punkte;
        public double BoardPointsSum => _team.BP;
        public double AverageDwz => _team.DWZ;
        public double BerlinTieBreak => _team.BW;
        public ObservableCollection<PlayerRow> PlayerRows { get; } = new();
        public IReadOnlyList<string> RoundHeaders { get; private set; } = Array.Empty<string>();

        private Color _rowColor = Application.Current?.RequestedTheme == AppTheme.Dark ? Colors.White : Colors.Black;
        public Color RowColor
        {
            get => _rowColor;
            set
            {
                if (_rowColor != value)
                {
                    _rowColor = value;
                    OnPropertyChanged(nameof(RowColor));
                }
            }
        }

        public IEnumerable<Player> Players => _team.TeamPlayers ?? Enumerable.Empty<Player>();
        public IList<GameDay>? GameDays => _team.GameDays;

        public bool ContainsGameDay(GameDay gameDay) =>
            _team.GameDays != null && _team.GameDays.Contains(gameDay);

        private void BuildPlayerRows()
        {
            PlayerRows.Clear();

            // determine number of rounds from team.GameDays or first player's PairingsPerGameDay length
            var rounds = _team.GameDays?.Count
                         ?? _team.TeamPlayers?.FirstOrDefault()?.PlayerInfoPerGameDay?.Length
                         ?? 0;

            RoundHeaders = Enumerable.Range(1, Math.Max(0, rounds)).Select(i => i.ToString()).ToList();

            foreach (var p in _team.TeamPlayers ?? Enumerable.Empty<Player>())
            {
                var row = new PlayerRow
                {
                    Brett = p.Brett,
                    Spieler = p.Name ?? string.Empty,
                    DWZ = p.DWZ == 1000 ? null : p.DWZ,
                    Rounds = [],
                    PlayerGameDayInfos = p.PlayerInfoPerGameDay?.Where(x => x != null).ToList() ?? []
                };

                var totalPoints = 0.0;
                var totalGames = 0;

                for (var i = 0; i < rounds; i++)
                {
                    var pointsString = "-";
                    if (p.PlayerInfoPerGameDay != null && i < p.PlayerInfoPerGameDay.Length)
                    {
                        var points = p.PlayerInfoPerGameDay[i]?.Points ?? -1;
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
        public int? DWZ { get; set; }
        public int AverageOpponentDWZ => PlayerGameDayInfos.Count > 0 ? (int)Math.Round(PlayerGameDayInfos.Where(x => x != null).Average(x => x.OpponentDWZ ?? 0)) : 0;
        public int? Performance => DWZ == null ? null : DWZ + (int)Math.Round((PointsSum - SumOfExpectedPoints) / GamesPlayed * 800);
        public int GamesPlayed => PlayerGameDayInfos.Count(x => x != null && x.Points >= 0);
        public double PointsSum => PlayerGameDayInfos.Where(x => x != null && x.Points >= 0).Sum(x => x.Points);
        public double SumOfExpectedPoints => DWZ == null ? 0 : PlayerGameDayInfos.Where(x => x != null && x.Points >= 0).Sum(x => x.OpponentDWZ.HasValue ? (1 / (1 + Math.Pow(10, ((x.OpponentDWZ.Value - DWZ ?? 0) / 400.0)))) : 0);
        public int? EstimatedDWZ => DWZ == null ? null : DWZ + (int)Math.Round((PointsSum - SumOfExpectedPoints) / (30 + GamesPlayed) * 800);
        public List<string> Rounds { get; set; } = new();
        public List<PlayerGameDayInfo?> PlayerGameDayInfos { get; set; } = new();
        public string Total { get; set; } = string.Empty;
    }
}