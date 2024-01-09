using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace BeatmapExporterCore.Utilities
{
    public class PlatformUtil
    {
        /// <summary>
        /// Open an Explorer window to a specific directory - Windows platform only
        /// </summary>
        public static void OpenExportDirectory(string directory)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                Process.Start("explorer.exe", directory);
            }
        }
    }
}
