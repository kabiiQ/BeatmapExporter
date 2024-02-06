using BeatmapExporterGUI.ViewModels.HomePage;
using BeatmapExporterGUI.ViewModels.List;
using BeatmapExporterGUI.ViewModels.Settings;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Threading;
using System.Threading.Tasks;

namespace BeatmapExporterGUI.ViewModels;

/// <summary>
/// Outer, "wrapping" ViewModel. Always visible, maintains the operation that is displayed to the user within
/// </summary>
public partial class OuterViewModel : ViewModelBase
{
    public OuterViewModel()
    {
        Home = new HomeViewModel();
        MenuRow = new MenuRowViewModel(this);

        CurrentOperation = Home;
    }

    /// <summary>
    /// The operational page currently displayed to the user, below the menu row. 
    /// </summary>
    [ObservableProperty]
    private ViewModelBase _CurrentOperation;

    /// <summary>
    /// The home page view model instance.
    /// </summary>
    public HomeViewModel Home { get; }

    /// <summary>
    /// The menu button row view model instance.
    /// </summary>
    public MenuRowViewModel MenuRow { get; }

    /// <summary>
    /// Changes the active operation to the HomeView instance.
    /// </summary>
    public void NavigateHome() => CurrentOperation = Home;

    /// <summary>
    /// Changes the active operation to the beatmap list page.
    /// </summary>
    public void ListBeatmaps() => CurrentOperation = new BeatmapListViewModel();

    /// <summary>
    /// Changes the active operation to the collection list page.
    /// </summary>
    public void ListCollections() => CurrentOperation = new CollectionListViewModel();

    /// <summary>
    /// Changes the active operation to the export filters/settings page.
    /// </summary>
    public void EditFilters() => CurrentOperation = new ExportConfigViewModel(this);

    /// <summary>
    /// Changes the active operation to the export page and begins the export operation immediately.
    /// </summary>
    public async Task Export(CancellationToken token)
    {
        var export = new ExportViewModel(this);
        CurrentOperation = export;
        await export.StartExport(token);
    }
    
    /// <summary>
    /// If the current operation is an actively running export.
    /// </summary>
    public bool IsExporting => (CurrentOperation as ExportViewModel)?.ActiveExport ?? false;
}
