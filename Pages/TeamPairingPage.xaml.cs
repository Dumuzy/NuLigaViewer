using NuLigaViewer.ViewModels;

namespace NuLigaViewer.Pages;

public partial class TeamPairingPage : ContentPage
{
	public TeamPairingPage(TeamPairingViewModel teamPairing)
	{
		InitializeComponent();
        BindingContext = teamPairing ?? throw new ArgumentNullException(nameof(teamPairing));
    }
}