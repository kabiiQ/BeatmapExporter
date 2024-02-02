using Avalonia.Controls;
using BeatmapExporterGUI.Utilities;

namespace BeatmapExporterGUI.Views.List;

public partial class CollectionListView : UserControl
{
    public CollectionListView()
    {
        InitializeComponent();

        var remaining = SizeHelper.RemainingHeight(200);
        CollectionList.Height = remaining;
        BeatmapList.Height = remaining;
    }
}