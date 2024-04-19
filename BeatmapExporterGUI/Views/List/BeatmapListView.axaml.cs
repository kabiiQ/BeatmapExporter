using Avalonia.Controls;
using BeatmapExporterGUI.Utilities;

namespace BeatmapExporterGUI.Views.List;

public partial class BeatmapListView : UserControl
{
    public BeatmapListView()
    {
        InitializeComponent();
        BeatmapList.Height = SizeHelper.RemainingHeight(240);
    }
}