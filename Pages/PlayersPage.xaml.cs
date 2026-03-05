using NuLigaViewer.ViewModels;

namespace NuLigaViewer.Pages
{
    public partial class PlayersPage : ContentPage
    {
        public PlayersPage(TeamViewModel teamViewModel)
        {
            InitializeComponent();
            BindingContext = teamViewModel ?? throw new ArgumentNullException(nameof(teamViewModel));
        }

        async void OnPlayerSelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            var selected = e.CurrentSelection.FirstOrDefault() as ViewModels.PlayerRow;
            if (selected is null)
                return;

            await Navigation.PushAsync(new PlayerPage(selected));

            if (sender is CollectionView cv)
            {
                cv.SelectedItem = null;
            }
        }
    }
}