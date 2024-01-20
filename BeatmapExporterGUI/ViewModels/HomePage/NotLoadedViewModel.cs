using CommunityToolkit.Mvvm.Input;
using System.Threading.Tasks;

namespace BeatmapExporterGUI.ViewModels.HomePage
{
    public partial class NotLoadedViewModel : ViewModelBase
    {
        private readonly HomeViewModel home;

        public NotLoadedViewModel(HomeViewModel home)
        {
            this.home = home;
        }

        [RelayCommand]
        private async Task Open()
        {
            var selectDir = await App.Current.DialogService.SelectDatabaseDialogAsync();
            await home.LoadDatabase(selectDir);
        }
    }
}