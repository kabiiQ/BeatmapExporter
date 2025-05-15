using BeatmapExporterCore.Filters;
using System.IO.Compression;

namespace BeatmapExporterCore.Exporters
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
                    ExportFormat.Replay => Path.Combine(basePath, "replay"),
                    ExportFormat.Folder => Path.Combine(basePath, "Songs"),
                    ExportFormat.CollectionDb => Path.Combine(basePath, "collection.db"),
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
        /// If filters should be applied with AND logic where beatmaps must match all filters.
        /// </summary>
        public bool CombineFilterMode { get; set; } = true;

        /// <summary>
        /// If compression is enabled for this exporter, disabled by default. Check <see cref="CompressionLevel"/> for actual compression level when creating archives.
        /// </summary>
        public bool CompressionEnabled { get; set; } = false;

        /// <summary>
        /// The compression level set for this exporter. 
        /// </summary>
        public CompressionLevel CompressionLevel => CompressionEnabled ? CompressionLevel.SmallestSize : CompressionLevel.NoCompression;

        /// <summary>
        /// If collection.db export should merge with an existing file, enabled by default. If false, output file will always be overwritten instead.
        /// </summary>
        public bool MergeCollections { get; set; } = true;

        /// <summary>
        /// If collection.db export should merge in a case-insensitive manner, merging duplicates.
        /// </summary>
        public bool MergeCaseInsensitive { get; set; } = true;

        /// <summary>
        /// The current export mode.
        /// </summary>
        public ExportFormat ExportFormat { get; set; } = ExportFormat.Beatmap;
    }
}
