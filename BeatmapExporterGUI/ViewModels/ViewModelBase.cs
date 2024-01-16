using BeatmapExporterGUI.Exporter;
using CommunityToolkit.Mvvm.ComponentModel;

namespace BeatmapExporterGUI.ViewModels;

public class ViewModelBase : ObservableObject
{
    public ViewModelBase()
    {
    }

    public ExporterApp Exporter => App.Current.Exporter;
}