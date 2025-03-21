using BeatmapExporterCore.Exporters.Lazer.LazerDB;
using BeatmapExporterCore.Utilities;

namespace BeatmapExporterCore.Exporters
{
    /// <summary>
    /// Base class for BeatmapExporterCore exceptions
    /// </summary>
    [Serializable]
    public class ExporterException : Exception
    {
        public ExporterException() { }
        public ExporterException(string message) : base(message) { }
        public ExporterException(string message, Exception inner) : base(message, inner) { }
    }

    /// <summary>
    /// Exception thrown when the osu!lazer database indicates a version mismatch
    /// </summary>
    public class LazerVersionException : ExporterException
    {
        private LazerVersionException(string message, IEnumerable<string> details) : base(message)
        {
            Details = details;
        }

        public IEnumerable<string> Details { get; }

        /// <summary>
        /// Build an exception for a database schema version mismatch with messages for the user depending on if the file is too new/old
        /// </summary>
        public static LazerVersionException Schema(int databaseVersion, string rawMessage) => new LazerVersionException(rawMessage, SchemaDetails(databaseVersion));

        private static IEnumerable<string> SchemaDetails(int fileSchema)
        {
            int exporterSchema = LazerDatabase.LazerSchemaVersion;
            string exporterVersion = ExporterUpdater.FeatureVersion;
            if (fileSchema > exporterSchema)
            {
                // Typical update required scenario 
                yield return $"The osu!lazer database file being loaded is from a newer version of osu!lazer (using database version {fileSchema}) than this version of BeatmapExporter ({exporterVersion}) was built for (database version {exporterSchema}).";
                yield return "You can check GitHub for a newer release, or file an issue there to let me know it needs updating if needed.";
            } else
            {
                // Older version mismatch scenario, suggest causes
                yield return $"The osu!lazer database file being loaded is from an OLDER version of osu!lazer than this version of BeatmapExporter was designed for.";
                yield return "If you are simply trying to export from your game, ensure that the correct database file your game uses is the one being loaded, and that your game is up-to-date.";
                yield return $"If things are correct and you still want to export from this specific database file/backup, you should check the BeatmapExporter GitHub releases for the version that contains the database version {fileSchema} in the title.";
                yield return "Linux users: it seems sometimes the Linux release is a version or two behind other platforms despite having the same release numbers. If your game is up-to-date and you are getting this error then this has likely occured again. You can easily work around this by using the matching version of BeatmapExporter as well.";
                yield return $"You can check {ExporterUpdater.Releases} for the correct release to open the selected database file using version {fileSchema}. It would contain the database version in the title, such as BeatmapExporter X.X.X ({fileSchema})";
            }
        }

        /// <summary>
        /// Build an exception for a database "file format" mismatch, which requires a newer version of Realm
        /// </summary>
        /// <param name="fileFormat"></param>
        /// <returns></returns>
        public static LazerVersionException FileFormat(int fileFormat, string rawMessage) => new LazerVersionException(rawMessage, UpgradeDetails(fileFormat));

        private static IEnumerable<string> UpgradeDetails(int fileFormat)
        {
            yield return $"The osu!lazer database file being loaded is using a different file format ({fileFormat}) than this version of BeatmapExporter was designed for.";
            yield return "Please first ensure you are loading the correct database, then file an issue on GitHub to let me know that there is a problem.";
        }
    }
}
