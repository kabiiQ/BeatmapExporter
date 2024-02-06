using Avalonia.Controls;
using BeatmapExporterGUI.Utilities;

namespace BeatmapExporterGUI.Views.HomePage;

public partial class HomeView : UserControl
{
    public HomeView()
    {
        InitializeComponent();
        SystemMessages.Height = SizeHelper.RemainingHeight(350);
    }
}