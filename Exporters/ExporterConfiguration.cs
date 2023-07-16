using System.IO.Compression;

namespace BeatmapExporter.Exporters
{
    public class ExporterConfiguration
    {
        public static readonly string DefaultAudioPath = "mp3";

        public enum Format { Audio, Beatmap };  

        private readonly string defaultExportPath;
        private string? exportPath = null;

        public ExporterConfiguration(string defaultExportPath)
        {
            this.defaultExportPath = defaultExportPath;
        }

        public List<BeatmapFilter> Filters { get; set; } = new();

        public string DefaultExportPath
        {
            get => defaultExportPath;
        }
        public string ExportPath
        {
            get
            {
                string basePath = exportPath is not null ? exportPath : DefaultExportPath;
                return ExportFormat == Format.Audio ? Path.Combine(basePath, "mp3") : basePath;
            }
            set => exportPath = value;
        }

        public bool CompressionEnabled { get; set; } = false;
        public CompressionLevel CompressionLevel
        {
            get => CompressionEnabled ? CompressionLevel.SmallestSize : CompressionLevel.NoCompression;
        }

        public Format ExportFormat { get; set; } = Format.Beatmap;

        public string ExportFormatUnitName => ExportFormat switch
        {
            Format.Audio => "audio (.mp3)",
            Format.Beatmap => "osu! beatmaps (.osz)",
            _ => throw new NotImplementedException()
        };
    }
}
