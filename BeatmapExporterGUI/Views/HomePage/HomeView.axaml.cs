using Avalonia;
using Avalonia.Controls;
using Avalonia.Threading;
using BeatmapExporterGUI.Utilities;

namespace BeatmapExporterGUI.Views.HomePage;

public partial class HomeView : UserControl
{
    public HomeView()
    {
        InitializeComponent();
        SystemMessages.Height = SizeHelper.RemainingHeight(350);

        // Scroll to bottom when system messages are added
        ItemsControl.ItemCountProperty.Changed.AddClassHandler<ItemsControl>((sender, e) =>
        {
            Dispatcher.UIThread.Post(() => SystemMessages.ScrollToEnd());
        });
    }
}