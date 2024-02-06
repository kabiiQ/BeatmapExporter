using BeatmapExporter.Exporters.Lazer.LazerDB;
using BeatmapExporterCore.Utilities;
using BeatmapExporterGUI.Exporter;
using CommunityToolkit.Mvvm.Input;
using System.Threading;
using System.Threading.Tasks;

namespace BeatmapExporterGUI.ViewModels;

/// <summary>
/// The top menu row/bar providing user access to all functions of the program.
/// </summary>
public partial class MenuRowViewModel : ViewModelBase
{
    private readonly OuterViewModel outer;

    public MenuRowViewModel(OuterViewModel outer)
    {
        this.outer = outer;
    }

    /// <summary>
    /// If an osu! database is currently loaded into the application.
    /// </summary>
    private bool DatabaseLoaded => Exporter.Lazer != null;

    /// <summary>
    /// If user navigation around the program should be allowed.
    /// </summary>
    private bool CanNavigate => !outer.IsExporting;

    /// <summary>
    /// If the user should be able to unload the osu! database manually.
    /// </summary>
    private bool CanUnload => DatabaseLoaded && CanNavigate;

    /// <summary>
    /// User-requested action to exit the entire BeatmapExporter program.
    /// </summary>
    [RelayCommand]
    private void Exit() => ExporterApp.Exit();

    /// <summary>
    /// User-requested action to unload the currently loaded osu! database.
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanUnload))]
    private void Close()
    {
        Exporter.Unload();
        outer.Home.SetNotLoaded();
        outer.NavigateHome();
    }
    
    // The below commands are user-requested navigation to specific program pages/functionality.

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

    // The below properties are references to the relevant BeatmapExporter version numbers

    public string ProgramVersion => ExporterUpdater.Version;

    public string DatabaseVersion => LazerDatabase.LazerSchemaVersion.ToString();

    public string LazerVersion => LazerDatabase.FirstLazerVersion;

    // The below commands are all web links available for the user to open in browser.

    public void GitHub() => PlatformUtil.OpenUrl(ExporterUpdater.Project);

    public void Releases() => PlatformUtil.OpenUrl(ExporterUpdater.Releases);

    public void Osu() => PlatformUtil.OpenUrl("https://github.com/ppy/osu/releases");
}