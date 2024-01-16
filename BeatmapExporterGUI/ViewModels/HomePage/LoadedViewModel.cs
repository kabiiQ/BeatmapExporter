using BeatmapExporter.Exporters.Lazer;
using System.Linq;

namespace BeatmapExporterGUI.ViewModels.HomePage
{
    public class LoadedViewModel : ViewModelBase
    {
        public LoadedViewModel()
        {
        }

        private LazerExporter Lazer => Exporter.Lazer!;

        public int BeatmapSets => Lazer.TotalBeatmapSetCount;

        public int BeatmapDiffs => Lazer.TotalBeatmapCount;

        public int Collections => Lazer.CollectionCount;

        public int SetsSelected => Lazer.SelectedBeatmapSetCount;

        public int DiffsSelected => Lazer.SelectedBeatmapCount;

        public int Filters => Lazer.Configuration.Filters.Count();

        public string ExportMode => Lazer.ExportFormatUnitName;
    }
}
