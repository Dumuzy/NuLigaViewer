using NuLigaViewer.ViewModels;

namespace NuLigaViewer
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