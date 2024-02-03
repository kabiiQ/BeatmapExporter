using Avalonia.Controls.ApplicationLifetimes;

namespace BeatmapExporterGUI.Utilities
{
    internal class SizeHelper
    {
        /// <summary>
        /// Computes a remaining pixel height for controls using the current window height.
        /// </summary>
        public static double RemainingHeight(int reserved) => (App.Current.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)?.MainWindow?.ClientSize.Height - reserved ?? 1024;
    }
}
