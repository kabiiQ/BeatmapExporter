using BeatmapExporter.Exporters.Lazer;
using BeatmapExporterCore.Exporters;
using System.Linq;

namespace BeatmapExporterGUI.ViewModels.HomePage
{
    /// <summary>
    /// Page displayed when a lazer database is loaded, displaying basic database stats.
    /// </summary>
    public class LoadedViewModel : ViewModelBase
    {
        public LoadedViewModel()
        {
        }

        /// <summary>
        /// The LazerExporter instance currently loaded.
        /// </summary>
        public LazerExporter Lazer => Exporter.Lazer!;

        /// <summary>
        /// Reference to the filters currently applied to this LazerExporter.
        /// </summary>
        public int Filters => Lazer.Configuration.Filters.Count();

        /// <summary>
        /// The export mode currently selected on this LazerExporter.
        /// </summary>
        public string ExportMode => Lazer.Configuration.ExportFormat.UnitName();
    }
}
