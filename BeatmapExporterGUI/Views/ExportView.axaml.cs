using Avalonia.Controls;
using BeatmapExporterGUI.Utilities;

namespace BeatmapExporterGUI.Views;

public partial class ExportView : UserControl
{
    public ExportView()
    {
        InitializeComponent();
        ExportViewer.Height = SizeHelper.RemainingHeight(300);
    }
}