using CommunityToolkit.Mvvm.Input;
using System.Threading.Tasks;

namespace BeatmapExporterGUI.ViewModels.HomePage
{
    /// <summary>
    /// Page displayed when an osu!lazer database is not loaded, allowing the user to select a database to load.
    /// </summary>
    public partial class NotLoadedViewModel : ViewModelBase
    {
        private readonly HomeViewModel home;

        public NotLoadedViewModel(HomeViewModel home)
        {
            this.home = home;
        }

        /// <summary>
        /// User-input command, displays a dialog to the user to select a database directory and attempts to load that directory. 
        /// If successful, this NotLoadedView will itself be unloaded throuhg the HomeView.
        /// </summary>
        [RelayCommand]
        private async Task Open()
        {
            var selectDir = await App.Current.DialogService.SelectDatabaseDialogAsync();
            await home.LoadDatabase(selectDir);
        }
    }
}