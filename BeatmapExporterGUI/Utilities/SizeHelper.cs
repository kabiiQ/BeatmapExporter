using Avalonia.Controls.ApplicationLifetimes;

namespace BeatmapExporterGUI.Utilities
{
    internal class SizeHelper
    {
        public static double RemainingHeight(int reserved) => (App.Current.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)?.MainWindow?.ClientSize.Height - reserved ?? 1024;
    }
}
