using NuLigaViewer.ViewModels;

namespace NuLigaViewer.Pages
{
    public partial class PlayerPage : ContentPage
    {
        public PlayerPage(PlayerRow playerRow)
        {
            InitializeComponent();
            BindingContext = playerRow ?? throw new ArgumentNullException(nameof(playerRow));
        }
    }
}