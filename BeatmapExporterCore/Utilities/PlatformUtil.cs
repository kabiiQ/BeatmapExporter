using System.Diagnostics;

namespace BeatmapExporterCore.Utilities
{
    public class PlatformUtil
    {
        /// <summary>
        /// Open a path with the system shell
        /// </summary>
        public static void Open(string file)
        {
            Process.Start(new ProcessStartInfo()
            {
                FileName = file,
                UseShellExecute = true,
                Verb = "open"
            });
        }
    }
}
