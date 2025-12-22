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
    }
}