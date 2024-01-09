using System.IO.Compression;
using BeatmapExporterCore.Exporters;

namespace BeatmapExporter.Exporters
{
    public class ExporterConfiguration
    {
        public static readonly string DefaultAudioPath = "mp3";
        private string? exportPath = null;

        /// <summary>
        /// All available modes of exporting.
        /// </summary>
        public enum Format { Beatmap, Audio, Background };

        public ExporterConfiguration(string defaultExportPath)
        {
            DefaultExportPath = defaultExportPath;
        }

        /// <summary>
        /// The beatmap filters currently applied to this exporter.
        /// </summary>
        public List<BeatmapFilter> Filters { get; set; } = new();

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
                    Format.Beatmap => basePath,
                    Format.Audio => Path.Combine(basePath, "mp3"),
                    Format.Background => Path.Combine(basePath, "bg")
                };
            }
            set => exportPath = value;
        }

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
        public Format ExportFormat { get; set; } = Format.Beatmap;
    }
}
