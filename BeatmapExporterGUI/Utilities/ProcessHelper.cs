using System.Diagnostics;
using System.Runtime.InteropServices;

namespace BeatmapExporterGUI.Utilities
{
    /// <summary>
    /// Helper methods for starting processes on the system.
    /// </summary>
    public static class ProcessHelper
    {
        /// <summary>
        /// Attempts to open a URL in the system default browser.
        /// </summary>
        public static void OpenUrl(string url)
        {
            try
            {
                Process.Start(url);
            } catch
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    url = url.Replace("&", "^&");
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = url,
                        UseShellExecute = true
                    });
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    Process.Start("xdg-open", url);
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    Process.Start("open", url);
                }
                else
                {
                    throw;
                }
            }
        }
    }
}
