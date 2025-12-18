using NuLigaCore.Data;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace NuLigaViewer.ViewModels
{
    public class LeaguesViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<League> Leagues { get; }

        public LeaguesViewModel(IEnumerable<League> leagues)
        {
            Leagues = new ObservableCollection<League>(leagues.Where(l => l is not null).ToList());
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}