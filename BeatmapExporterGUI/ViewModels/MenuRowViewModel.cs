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

    private bool DatabaseLoaded => Exporter.Lazer != null;

    private bool CanNavigate => !outer.IsExporting;

    private bool CanUnload => DatabaseLoaded && CanNavigate;

    [RelayCommand]
    private void Exit() => ExporterApp.Exit();

    [RelayCommand(CanExecute = nameof(CanUnload))]
    private void Close()
    {
        Exporter.Unload();
        outer.Home.SetNotLoaded();
        outer.NavigateHome();
    }

    [RelayCommand(CanExecute = nameof(CanNavigate))]
    private void Home() => outer.NavigateHome();

    [RelayCommand(CanExecute = nameof(CanNavigate))]
    private void Beatmaps() => outer.ListBeatmaps();

    [RelayCommand(CanExecute = nameof(CanNavigate))]
    private void Collections() => outer.ListCollections();

    [RelayCommand(CanExecute = nameof(CanNavigate))]
    private void Configuration() => outer.EditFilters();

    [RelayCommand(IncludeCancelCommand = true, CanExecute = nameof(CanNavigate))]
    private async Task Export(CancellationToken token) => await outer.Export(token);

    public string ExportDescription => Exporter.Lazer?.Configuration != null ? $"{Exporter.Lazer.Configuration.ExportFormat.UnitName()}, {Exporter.Lazer.SelectedBeatmapSetCount} sets" : "not loaded";

    public string ProgramVersion => ExporterUpdater.Version;

    public string DatabaseVersion => LazerDatabase.LazerSchemaVersion.ToString();

    public string LazerVersion => LazerDatabase.FirstLazerVersion;

    public void GitHub() => ProcessHelper.OpenUrl(ExporterUpdater.Project);

    public void Releases() => ProcessHelper.OpenUrl(ExporterUpdater.Releases);

    public void Osu() => ProcessHelper.OpenUrl("https://github.com/ppy/osu/releases");
}