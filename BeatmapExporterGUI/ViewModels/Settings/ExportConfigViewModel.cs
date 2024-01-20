using BeatmapExporter.Exporters;
using BeatmapExporterCore.Exporters;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BeatmapExporterGUI.ViewModels.Settings
{
    public class ExportConfigViewModel : ViewModelBase
    {
        public ExportConfigViewModel()
        {
            ExportModes = BuildExportModes();
        }

        private ExporterConfiguration Config => Exporter.Configuration!;

        // export format in UI will be kept 1:1 to ExportFormat enum's int equivalent
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

        private IEnumerable<string> BuildExportModes()
        {
            var exportFormats = Enum.GetValues(typeof(ExportFormat));
            foreach (ExportFormat format in exportFormats)
            {
                yield return format.UnitName();
            }
        }
    }
}
