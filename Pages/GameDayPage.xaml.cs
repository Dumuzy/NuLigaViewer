using NuLigaViewer.Data;

namespace NuLigaViewer.Pages;

public partial class GameDayPage : ContentPage
{
	public GameDayPage(GameDay gameDay)
	{
		InitializeComponent();
        BindingContext = gameDay ?? throw new ArgumentNullException(nameof(gameDay));
    }
}