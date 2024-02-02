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

        public LazerExporter Lazer => Exporter.Lazer!;

        protected ExporterConfiguration Config => Exporter.Configuration!;

        #region Beatmap Filters
        [ObservableProperty]
        private IEnumerable<string> _BeatmapFilters;

        /// <summary>
        /// Update current beatmap filter list (computation must be done on Realm thread)
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

        public string SelectionSummary => $"Beatmap sets selected: {Lazer.SelectedBeatmapSetCount}/{Lazer.TotalBeatmapSetCount}\n\nBeatmap diffs selected: {Lazer.SelectedBeatmapCount}/{Lazer.TotalBeatmapCount}";

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(RemoveSelectedFilterCommand))]
        private int _SelectedFilterIndex;

        public bool IsFilterSelected => SelectedFilterIndex != -1;

        public bool IsResettable => Config.Filters.Count > 0;

        [RelayCommand(CanExecute = nameof(IsFilterSelected))]
        private async Task RemoveSelectedFilter()
        {
            Config.Filters.RemoveAt(SelectedFilterIndex);
            await UpdateBeatmapFilters();
        }

        [RelayCommand(CanExecute = nameof(IsResettable))]
        private async Task ResetFilters()
        {
            Config.Filters.Clear();
            await UpdateBeatmapFilters();
        }

        public IAsyncRelayCommand ExportBeatmapsCommand => outerViewModel.MenuRow.ExportCommand;

        [RelayCommand]
        private void ListBeatmaps() => outerViewModel.ListBeatmaps();

        [ObservableProperty]
        private NewFilterViewModel? _CurrentFilterCreationControl;

        public void CreateFilterBuilder() => CurrentFilterCreationControl = new(this);

        public void CancelFilterBuilder() => CurrentFilterCreationControl = null;

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

        public bool IsBeatmapExport => Config.ExportFormat == ExportFormat.Beatmap;

        /// <summary>
        /// Descriptor string for the currently selected export format 
        /// </summary>
        public string ModeDescriptor
        {
            get => Config.ExportFormat.Descriptor();
        }

        public string ExportPath => Config.FullPath;

        public bool CompressionEnabled
        {
            get => Config.CompressionEnabled;
            set
            {
                Config.CompressionEnabled = value;
                OnPropertyChanged(nameof(CompressionDescriptor));
            }
        }

        public string CompressionDescriptor => CompressionEnabled ? "(slow export, smaller file sizes)" : "(fastest export, no compression)";

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
