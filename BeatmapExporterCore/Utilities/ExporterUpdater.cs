namespace BeatmapExporterCore.Utilities
{
    public static class ExporterUpdater
    {
        public const string Version = "2.0.0";

        public const string Project = "https://github.com/kabiiQ/BeatmapExporter";
        public const string Releases = $"{Project}/releases";
        public const string Latest = $"{Releases}/latest";
        public const string VersionDoc = "https://raw.githubusercontent.com/kabiiQ/BeatmapExporter/main/VERSION";

        public record struct Update(string Current, string New);

        /// <summary>
        /// Checks the project GitHub repo's VERSION file and compares the contents against the current version string.
        /// This method should be called, and the user notified if an update is available.
        /// </summary>
        /// <returns>A struct with the current and available version numbers, or null if there is no update available.</returns>
        public static async Task<Update?> CheckNewerVersionAvailable()
        {
            try
            {
                var client = new HttpClient()
                {
                    Timeout = TimeSpan.FromSeconds(5)
                };
                var latest = await client.GetStringAsync(VersionDoc);
                if (latest != Version)
                {
                    return new Update(Version, latest);
                }
            }
            catch (Exception) { } // not critical error, don't bother user
            return null;
        }
    }
}
