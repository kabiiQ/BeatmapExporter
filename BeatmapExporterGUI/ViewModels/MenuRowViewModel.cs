using BeatmapExporter.Exporters.Lazer.LazerDB;
using BeatmapExporterCore.Exporters;
using BeatmapExporterCore.Utilities;
using BeatmapExporterGUI.Exporter;
using BeatmapExporterGUI.Utilities;
using CommunityToolkit.Mvvm.Input;
using System.Threading;
using System.Threading.Tasks;

namespace BeatmapExporterGUI.ViewModels;

public partial class MenuRowViewModel : ViewModelBase
{
    private readonly OuterViewModel outer;

    public MenuRowViewModel(OuterViewModel outer)
    {
        this.outer = outer;
    }

    public void Exit() => ExporterApp.Exit();

    [RelayCommand(CanExecute = nameof(DatabaseLoaded))]
    private void Close()
    {
        Exporter.Unload();
        outer.Home.SetNotLoaded();
        outer.NavigateHome();
    }

    private bool DatabaseLoaded() => Exporter.Lazer != null;

    public void Home() => outer.NavigateHome();

    public void Beatmaps() => outer.ListBeatmaps();

    public void Collections() => outer.ListCollections();

    public void Configuration() => outer.EditFilters();

    [RelayCommand(IncludeCancelCommand = true)]
    private async Task Export(CancellationToken token) => await outer.Export(token);

    public string ExportDescription => Exporter.Lazer?.Configuration != null ? $"{Exporter.Lazer.Configuration.ExportFormat.UnitName()}, {Exporter.Lazer.SelectedBeatmapSetCount} sets" : "not loaded";

    public string ProgramVersion => ExporterUpdater.Version;

    public string DatabaseVersion => LazerDatabase.LazerSchemaVersion.ToString();

    public void GitHub() => ProcessHelper.OpenUrl(ExporterUpdater.Project);

    public void Releases() => ProcessHelper.OpenUrl(ExporterUpdater.Releases);

    public void Osu() => ProcessHelper.OpenUrl("https://github.com/ppy/osu/releases");
}