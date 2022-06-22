using System.IO.Compression;

namespace BeatmapExporter.Exporters
{
    public class ExporterConfiguration
    {
        private readonly string defaultExportPath;
        private string? exportPath = null;

        private readonly string defaultArchivePath;

        public ExporterConfiguration(string defaultExportPath, string defaultArchivePath)
        {
            this.defaultExportPath = defaultExportPath;
            this.defaultArchivePath = defaultArchivePath;
        }

        public List<BeatmapFilter> Filters { get; set; } = new();

        public string DefaultExportPath
        {
            get => defaultExportPath;
        }
        public string ExportPath
        {
            get => exportPath is not null ? exportPath : DefaultExportPath;
            set => exportPath = value;
        }

        public bool CompressionEnabled { get; set; } = false;
        public CompressionLevel CompressionLevel
        {
            get => CompressionEnabled ? CompressionLevel.SmallestSize : CompressionLevel.NoCompression;
        }

        public bool ExportSingleArchive { get; set; } = false;
        public string ExportArchivePath
        {
            get => defaultArchivePath;
        }
    }
}
