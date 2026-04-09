using NuLigaViewer.Data;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace NuLigaViewer.ViewModels
{
    public class LeaguesViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<BadenRegion> Regions { get; }

        public LeaguesViewModel(IEnumerable<BadenRegion> region)
        {
            Regions = new ObservableCollection<BadenRegion>(region);
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}