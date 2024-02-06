using BeatmapExporterCore.Utilities;
using BeatmapExporterGUI.Exporter;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Threading.Tasks;

namespace BeatmapExporterGUI.ViewModels.HomePage
{
    /// <summary>
    /// Initial loading page and app summary page which may be loaded at any time by the user 
    /// </summary>
    public partial class HomeViewModel : ViewModelBase
    {
        public HomeViewModel()
        {
            DatabaseStatus = new LoadingViewModel();
        }

        /// <summary>
        /// The View displayed on the left side of the HomeView, containing information about the lazer database status.
        /// </summary>
        [ObservableProperty]
        private ViewModelBase _DatabaseStatus;

        /// <summary>
        /// Sets the HomeView's database status to the 'loading' view. Should only be very shortly displayed.
        /// </summary>
        public void SetLoading() => DatabaseStatus = new LoadingViewModel();

        /// <summary>
        /// Sets the HomeView's database status to the 'loaded' view, containing basic database information.
        /// </summary>
        public void SetLoaded() => DatabaseStatus = new LoadedViewModel();

        /// <summary>
        /// Sets the HomeView's database status to the 'not loaded' view, allowing the user to load a database.
        /// </summary>
        public void SetNotLoaded() => DatabaseStatus = new NotLoadedViewModel(this);

        /// <summary>
        /// Attempts to load the osu!lazer database from the default or user-provided directory.
        /// </summary>
        public async Task LoadDatabase(string? directory) => await Exporter.RealmScheduler.Schedule(() => LoadDatabaseSync(directory));

        private void LoadDatabaseSync(string? directory)
        {
            SetLoading();
            if (Exporter.LoadDatabase(directory))
            {
                SetLoaded();
            } 
            else
            {
                SetNotLoaded();
            }
        }

        /// <summary>
        /// Contains a version number detected as a latest release on BeatmapExporter launch, null if no update is available.
        /// </summary>
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsUpdateAvailable))]
        private string? _UpdateAvailable;
        
        /// <summary>
        /// If an update is available for BeatmapExporter.
        /// </summary>
        public bool IsUpdateAvailable => UpdateAvailable != null;

        /// <summary>
        /// Performs the online check for a different available version of BeatmapExporter, and updates the <see cref="UpdateAvailable" /> string.
        /// </summary>
        /// <returns></returns>
        public async Task CheckForUpdate()
        {
            var update = await ExporterUpdater.CheckNewerVersionAvailable();
            if (update.HasValue)
            {
                UpdateAvailable = $"{update.Value.Current} -> {update.Value.New}";
            }
        }
        /// <summary>
        /// Opens the latest BeatmapExporter release in web browser.
        /// </summary>
        public void Release() => ExporterApp.OpenLatestRelease();
    }
}
