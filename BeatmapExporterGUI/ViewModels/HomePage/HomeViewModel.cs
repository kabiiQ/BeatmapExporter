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

        [ObservableProperty]
        private ViewModelBase _DatabaseStatus;

        public void SetLoading() => DatabaseStatus = new LoadingViewModel();

        public void SetLoaded() => DatabaseStatus = new LoadedViewModel();

        public void SetNotLoaded() => DatabaseStatus = new NotLoadedViewModel(this);

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

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsUpdateAvailable))]
        private string? _UpdateAvailable;
        
        public bool IsUpdateAvailable => UpdateAvailable != null;

        public async Task CheckForUpdate()
        {
            var update = await ExporterUpdater.CheckNewerVersionAvailable();
            if (update.HasValue)
            {
                UpdateAvailable = $"{update.Value.Current} -> {update.Value.New}";
            }
        }

        public void Release() => ExporterApp.OpenLatestRelease();
    }
}
