using BeatmapExporterCore.Exporters;
using BeatmapExporterCore.Filters;
using System.IO.Compression;

namespace BeatmapExporter.Exporters
{
    public class ExporterConfiguration
    {
        private string? exportPath = null;

        public ExporterConfiguration(string defaultExportPath)
        {
            DefaultExportPath = defaultExportPath;
            Filters = new();
        }

        /// <summary>
        /// The beatmap filters currently applied to this exporter.
        /// </summary>
        public List<BeatmapFilter> Filters { get; set; }

        /// <summary>
        /// The default export path for this type of exporter.
        /// </summary>
        public string DefaultExportPath { get; }

        /// <summary>
        /// The currently set export path, equal to <see cref="DefaultExportPath"/> if not changed by the user.
        /// </summary>
        public string ExportPath
        {
            get
            {
                string basePath = exportPath is not null ? exportPath : DefaultExportPath;
                return ExportFormat switch
                {
                    ExportFormat.Beatmap => basePath,
                    ExportFormat.Audio => Path.Combine(basePath, "mp3"),
                    ExportFormat.Background => Path.Combine(basePath, "bg"),
                    _ => throw new InvalidOperationException()
                };
            }
            set => exportPath = value;
        }

        /// <summary>
        /// The full export path, suitable for feedback to the user.
        /// </summary>
        public string FullPath => Path.GetFullPath(ExportPath);

        /// <summary>
        /// If compression is enabled for this exporter, disabled by default. Check <see cref="CompressionLevel"/> for actual compression level when creating archives.
        /// </summary>
        public bool CompressionEnabled { get; set; } = false;

        /// <summary>
        /// The compression level set for this exporter. 
        /// </summary>
        public CompressionLevel CompressionLevel => CompressionEnabled ? CompressionLevel.SmallestSize : CompressionLevel.NoCompression;

        /// <summary>
        /// The current export mode.
        /// </summary>
        public ExportFormat ExportFormat { get; set; } = ExportFormat.Beatmap;
    }
}
