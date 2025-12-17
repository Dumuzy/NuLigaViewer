namespace NuLigaViewer
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();

            var leagues = NuLigaParser.ParseLeagues();
            BindingContext = new ViewModels.LeaguesViewModel(leagues);
        }
    }
}
