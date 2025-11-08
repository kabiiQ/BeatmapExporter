using BeatmapExporterCore.Filters;
using BeatmapExporterCore.Utilities;
using NLog;
using System.Diagnostics.CodeAnalysis;
using System.IO.Compression;
using System.Runtime.InteropServices;

namespace BeatmapExporterCore.Exporters
{
    public class ExporterConfiguration
    {
        private ClientSettings settings;

        private string? exportPath = null;
        private bool combineFilterMode;
        private ExportFormat exportFormat;
        private bool compressionEnabled;
        private bool mergeCollections;
        private bool caseInsensitiveMerge;

        public ExporterConfiguration(ClientSettings settings)
        {
            ApplySettings(settings);
        }
        
        static ExporterConfiguration()
        {
            NLog.LogManager.Setup().LoadConfiguration(builder =>
            {
                builder.ForLogger().FilterMinLevel(LogLevel.Error).WriteToFile(Path.Combine(ClientSettings.APPDIR, "exporter.error.log"));
            });
        }

        /// <summary>
        /// Applies the settings from a <see cref="ClientSettings" /> profile to this exporter configuration.
        /// </summary>
        /// <param name="settings"></param>
        [MemberNotNull(nameof(settings), nameof(exportFormat), nameof(Filters))]
        public void ApplySettings(ClientSettings settings)
        {
            this.settings = settings;
            exportPath = settings.ExportPath;
            exportFormat = settings.ExportFormat;
            combineFilterMode = settings.MatchAllFilters;
            compressionEnabled = settings.CompressionEnabled;
            mergeCollections = settings.MergeCollections;
            caseInsensitiveMerge = settings.MergeCaseInsensitive;

            Filters = settings.AppliedFilters
                .Select(f => f.ToBeatmapFilter())
                .Where(f => f != null)
                .Select(f => f!)
                .ToList();
        }

        /// <summary>
        /// The beatmap filters currently applied to this exporter.
        /// </summary>
        public List<BeatmapFilter> Filters { get; set; }

        /// <summary>
        /// Notify the user settings container to update the currently persisted filters.
        /// </summary>
        public void SaveFilters() => settings.SaveFilters([.. Filters]);

        /// <summary>
        /// The default export path for this type of exporter.
        /// </summary>
        public string DefaultExportPath { get; } = "lazerexport";

        /// <summary>
        /// The currently set export path, equal to <see cref="DefaultExportPath"/> if not changed by the user.
        /// </summary>
        public string ExportPath
        {
            get
            {
                string basePath = exportPath ?? DefaultExportPath;
                return ExportFormat switch
                {
                    ExportFormat.Beatmap => basePath,
                    ExportFormat.Audio => Path.Combine(basePath, "mp3"),
                    ExportFormat.Background => Path.Combine(basePath, "bg"),
                    ExportFormat.Replay => Path.Combine(basePath, "replay"),
                    ExportFormat.Skins => Path.Combine(basePath, "skins"),
                    ExportFormat.Folder => Path.Combine(basePath, "Songs"),
                    ExportFormat.CollectionDb => Path.Combine(basePath, "collection.db"),
                    _ => throw new InvalidOperationException()
                };
            }
            set
            {
                exportPath = value;
                settings.SaveExportPath(value);
            }
        }

        /// <summary>
        /// The full export path, suitable for feedback to the user.
        /// </summary>
        public string FullPath => Path.GetFullPath(ExportPath);

        /// <summary>
        /// If filters should be applied with AND logic where beatmaps must match all filters.
        /// </summary>
        public bool CombineFilterMode
        {
            get => combineFilterMode;
            set
            {
                combineFilterMode = value;
                settings.SaveFilterMode(value);
            }
        }

        /// <summary>
        /// If compression is an option that can be used with the current exporter state.
        /// </summary>
        /// <returns></returns>
        public bool CompressionAvailable => ExportFormat is ExportFormat.Beatmap or ExportFormat.Skins 
                                            && !RuntimeInformation.IsOSPlatform(OSPlatform.OSX);

        /// <summary>
        /// If compression is enabled for this exporter, disabled by default. Check <see cref="CompressionLevel"/> for actual compression level when creating archives.
        /// </summary>
        public bool CompressionEnabled
        {
            get => compressionEnabled;
            set
            {
                compressionEnabled = value;
                settings.SaveCompression(value);
            }
        }

        /// <summary>
        /// The compression level set for this exporter. 
        /// </summary>
        public CompressionLevel CompressionLevel => CompressionEnabled ? CompressionLevel.SmallestSize : CompressionLevel.NoCompression;

        /// <summary>
        /// If collection.db export should merge with an existing file, enabled by default. If false, output file will always be overwritten instead.
        /// </summary>
        public bool MergeCollections
        {
            get => mergeCollections;
            set
            {
                mergeCollections = value;
                settings.SaveMerge(value);
            }
        }

        /// <summary>
        /// If collection.db export should merge in a case-insensitive manner, merging duplicates.
        /// </summary>
        public bool MergeCaseInsensitive
        {
            get => caseInsensitiveMerge;
            set
            {
                caseInsensitiveMerge = value;
                settings.SaveCaseInsensitive(value);
            }
        }

        /// <summary>
        /// The current export mode.
        /// </summary>
        public ExportFormat ExportFormat
        { 
            get => exportFormat;
            set
            {
                exportFormat = value;
                settings.SaveExportFormat(value);
            }
        }
    }
}
