using BeatmapExporter.Exporters.Lazer;
using BeatmapExporterCore.Exporters;
using System.Linq;

namespace BeatmapExporterGUI.ViewModels.HomePage
{
    public class LoadedViewModel : ViewModelBase
    {
        public LoadedViewModel()
        {
        }

        public LazerExporter Lazer => Exporter.Lazer!;

        public int Filters => Lazer.Configuration.Filters.Count();

        public string ExportMode => Lazer.Configuration.ExportFormat.UnitName();
    }
}
