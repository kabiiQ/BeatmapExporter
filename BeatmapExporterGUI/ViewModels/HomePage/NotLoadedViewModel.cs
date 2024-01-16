using CommunityToolkit.Mvvm.Input;

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
        private async void Open()
        {
            var selectDir = await App.Current.DialogService.SelectDatabaseDialogAsync();
            await home.LoadDatabase(selectDir);
        }
    }
}