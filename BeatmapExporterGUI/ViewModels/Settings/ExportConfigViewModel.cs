using BeatmapExporter.Exporters;
using BeatmapExporter.Exporters.Lazer;
using BeatmapExporterCore.Exporters;
using BeatmapExporterCore.Filters;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BeatmapExporterGUI.ViewModels.Settings
{
    /// <summary>
    /// Page allowing users to configure the beatmap export. Currently contains beatmap filtering on the left half and general/advanced export options on the right half.
    /// </summary>
    public partial class ExportConfigViewModel : ViewModelBase
    {
        private readonly OuterViewModel outerViewModel;

        public ExportConfigViewModel(OuterViewModel outer)
        {
            outerViewModel = outer;
            ExportModes = ExportFormats.All().Select(format => format.UnitName());
            BeatmapFilters = new List<string>();
            Task.Run(() => UpdateBeatmapFilters());
            SelectedFilterIndex = -1;
        }

        /// <summary>
        /// Reference to the currently loaded <see cref="LazerExporter" />
        /// </summary>
        public LazerExporter Lazer => Exporter.Lazer!;

        protected ExporterConfiguration Config => Exporter.Configuration!;

        #region Beatmap Filters
        /// <summary>
        /// String representations for all the currently applied beatmap filters on the exporter.
        /// </summary>
        [ObservableProperty]
        private IEnumerable<string> _BeatmapFilters;

        /// <summary>
        /// Updates current beatmap filter list (computation must be done on Realm thread)
        /// </summary>
        private async Task UpdateBeatmapFilters()
        {
            await Exporter.RealmScheduler.Schedule(() =>
            {
                BeatmapFilters = Exporter.Lazer!.Filters()
                    .Select(filter => $"+ {filter.Description} ({filter.DiffCount} beatmaps)")
                    .ToList(); // ToList is essential to build filter list on the RealmScheduler thread only

                Exporter.Lazer.UpdateSelectedBeatmaps();
            });

            RemoveSelectedFilterCommand.NotifyCanExecuteChanged();
            ResetFiltersCommand.NotifyCanExecuteChanged();
            OnPropertyChanged(nameof(SelectionSummary));
        }

        /// <summary>
        /// String describing the currently selected beatmap set and diff counts versus the total counts, on two lines.
        /// </summary>
        public string SelectionSummary => $"Beatmap sets selected: {Lazer.SelectedBeatmapSetCount}/{Lazer.TotalBeatmapSetCount}\n\nBeatmap diffs selected: {Lazer.SelectedBeatmapCount}/{Lazer.TotalBeatmapCount}";

        /// <summary>
        /// The currently selected beatmap filter, indexed 1:1 to <see cref="ExporterConfiguration.Filters" /> for this exporter
        /// </summary>
        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(RemoveSelectedFilterCommand))]
        private int _SelectedFilterIndex;

        /// <summary>
        /// If a filter is currently selected by the user for deletion.
        /// </summary>
        public bool IsFilterSelected => SelectedFilterIndex != -1;

        /// <summary>
        /// If any filters are currently registered.
        /// </summary>
        public bool IsResettable => Config.Filters.Count > 0;

        /// <summary>
        /// User-requested action to remove the currently selected beatmap filter.
        /// </summary>
        /// <returns></returns>
        [RelayCommand(CanExecute = nameof(IsFilterSelected))]
        private async Task RemoveSelectedFilter()
        {
            Config.Filters.RemoveAt(SelectedFilterIndex);
            await UpdateBeatmapFilters();
        }

        /// <summary>
        /// User-requested action to remove all active beatmap filters.
        /// </summary>
        /// <returns></returns>
        [RelayCommand(CanExecute = nameof(IsResettable))]
        private async Task ResetFilters()
        {
            Config.Filters.Clear();
            await UpdateBeatmapFilters();
        }

        /// <summary>
        /// Reference to the Export Beatmaps command functionality for this page to allow an alternate method to begin exporting.
        /// </summary>
        public IAsyncRelayCommand ExportBeatmapsCommand => outerViewModel.MenuRow.ExportCommand;

        /// <summary>
        /// User-requested action to change the application view to the beatmap list.
        /// </summary>
        [RelayCommand]
        private void ListBeatmaps() => outerViewModel.ListBeatmaps();

        /// <summary>
        /// The current beatmap filter 'builder' displayed to the user, if one exists.
        /// </summary>
        [ObservableProperty]
        private NewFilterViewModel? _CurrentFilterCreationControl;

        /// <summary>
        /// User-requested action to create a new filter 'builder'. Any existing builder will be lost.
        /// </summary>
        public void CreateFilterBuilder() => CurrentFilterCreationControl = new(this);

        /// <summary>
        /// User-requested action to delete the active filter 'builder'.
        /// </summary>
        public void CancelFilterBuilder() => CurrentFilterCreationControl = null;

        /// <summary>
        /// User-requesterd action to save the current filter 'builder' as a real active beatmap filter.
        /// </summary>
        public async Task ApplyFilterBuilder(BeatmapFilter filter)
        {
            Config.Filters.Add(filter);
            await UpdateBeatmapFilters();
            CancelFilterBuilder();
        }
        #endregion

        #region Advanced Export Settings
        /// <summary>
        /// Current export format selected.
        /// Export format in UI will be kept 1:1 to ExportFormat enum's int value
        /// </summary>
        public int SelectedExportIndex
        {
            get => (int)Config.ExportFormat;
            set
            {
                Config.ExportFormat = (ExportFormat)value;
                OnPropertyChanged(nameof(ModeDescriptor));
                OnPropertyChanged(nameof(ExportPath));
                OnPropertyChanged(nameof(IsBeatmapExport));
            }
        }

        /// <summary>
        /// List of all supported export formats in user friendly form
        /// </summary>
        public IEnumerable<string> ExportModes { get; }

        /// <summary>
        /// If the export mode is currently set to export whole beatmaps, the default export mode.
        /// </summary>
        public bool IsBeatmapExport => Config.ExportFormat == ExportFormat.Beatmap;

        /// <summary>
        /// Descriptor string for the currently selected export format 
        /// </summary>
        public string ModeDescriptor
        {
            get => Config.ExportFormat.Descriptor();
        }

        /// <summary>
        /// Reference to the current full file export directory.
        /// </summary>
        public string ExportPath => Config.FullPath;

        /// <summary>
        /// If beatmap export compression is currently enabled by the user.
        /// </summary>
        public bool CompressionEnabled
        {
            get => Config.CompressionEnabled;
            set
            {
                Config.CompressionEnabled = value;
                OnPropertyChanged(nameof(CompressionDescriptor));
            }
        }

        /// <summary>
        /// Description of the current <see cref="CompressionEnabled" /> setting, suitable for user display.
        /// </summary>
        public string CompressionDescriptor => CompressionEnabled ? "(slow export, smaller file sizes)" : "(fastest export, no compression)";

        /// <summary>
        /// User-requested action to change the current export path. Opens an additional dialog for directory selection.
        /// </summary>
        public async Task SelectExportPath()
        {
            var selectDir = await App.Current.DialogService.SelectDirectoryAsync(ExportPath);
            if (selectDir != null)
            {
                Config.ExportPath = selectDir;
                OnPropertyChanged(nameof(ExportPath));
            }
        }
        #endregion
    }
}
