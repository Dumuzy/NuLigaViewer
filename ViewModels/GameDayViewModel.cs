using NuLigaViewer.Data;
using System.ComponentModel;

namespace NuLigaViewer.ViewModels
{
    public class GameDayViewModel : INotifyPropertyChanged
    {
        private readonly GameDay _gameDay;

        public GameDayViewModel(GameDay gameDay)
        {
            _gameDay = gameDay ?? throw new ArgumentNullException(nameof(gameDay));
        }

        public int RankAfterSorting { get; set; }
        public int Runde => _gameDay.Runde;
        public DateTime Datum => _gameDay.Datum;
        public string Title => _gameDay.Title;
        public string? GastMannschaft => _gameDay.GastMannschaft;
        public double GastMannschaftDWZ => _gameDay.GastMannschaftDWZ;
        public string? HeimMannschaft => _gameDay.HeimMannschaft;
        public double HeimMannschaftDWZ => _gameDay.HeimMannschaftDWZ;
        public string? Brettpunkte => _gameDay.BrettPunkte;
        public GameReport? Report => _gameDay.Report;

        public bool ContainsGameDay(GameDay gameDay) => gameDay == _gameDay;

        public void Refresh()
        {
            OnPropertyChanged(nameof(Runde));
            OnPropertyChanged(nameof(Datum));
            OnPropertyChanged(nameof(Title));
            OnPropertyChanged(nameof(GastMannschaft));
            OnPropertyChanged(nameof(GastMannschaftDWZ));
            OnPropertyChanged(nameof(HeimMannschaft));
            OnPropertyChanged(nameof(HeimMannschaftDWZ));
            OnPropertyChanged(nameof(Brettpunkte));
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}