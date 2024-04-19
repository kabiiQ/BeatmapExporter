using Avalonia.Controls;
using BeatmapExporterGUI.Utilities;

namespace BeatmapExporterGUI.Views.List;

public partial class BeatmapExplorerView : UserControl
{
    public BeatmapExplorerView()
    {
        InitializeComponent();

        var each = SizeHelper.RemainingHeight(300) / 3;
        DiffList.Height = each;
        FileList.Height = each;
        ReplayList.Height = each;
    }
}