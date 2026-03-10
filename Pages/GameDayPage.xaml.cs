using NuLigaViewer.ViewModels;

namespace NuLigaViewer.Pages;

public partial class GameDayPage : ContentPage
{
	public GameDayPage(GameDayViewModel gameDay)
	{
		InitializeComponent();
        BindingContext = gameDay ?? throw new ArgumentNullException(nameof(gameDay));
    }
}