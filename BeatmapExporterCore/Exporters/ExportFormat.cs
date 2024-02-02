namespace BeatmapExporterCore.Exporters
{
    /// <summary>
    /// All available modes of exporting.
    /// </summary>
    public enum ExportFormat { Beatmap, Audio, Background };

    public static class ExportFormatExtensions
    {
        /// <summary>
        /// A string describing what this export format targets. Simple, for inlining, for example: "beatmap backgrounds"
        /// </summary>
        public static string UnitName(this ExportFormat format) => format switch
        {
            ExportFormat.Beatmap => "osu! beatmaps (.osz)",
            ExportFormat.Audio => "audio (.mp3)",
            ExportFormat.Background => "beatmap backgrounds",
            _ => throw new NotImplementedException()
        };

        /// <summary>
        /// A string describing the actions that the export mode will perform.
        /// </summary>
        public static string Descriptor(this ExportFormat format) => format switch
        {
            ExportFormat.Beatmap => "Beatmaps will be exported in osu! archive format (.osz).",
            ExportFormat.Audio => "Beatmap audio files will be renamed, tagged and exported (.mp3 format).",
            ExportFormat.Background => "Only beatmap background images will be exported (original format).",
            _ => throw new NotImplementedException()
        };

        public static ExportFormat Next(this ExportFormat format)
        {
            var max = Enum.GetValues(typeof(ExportFormat)).Length;
            var iNext = ((int)format + 1) % max;
            return (ExportFormat)iNext;
        }
    }

    public static class ExportFormats
    {
        public static IEnumerable<ExportFormat> All() => (ExportFormat[])Enum.GetValues(typeof(ExportFormat));
    }
}
