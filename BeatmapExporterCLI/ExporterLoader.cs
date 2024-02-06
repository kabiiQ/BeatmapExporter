using BeatmapExporterCLI.Data;
using BeatmapExporterCLI.Interface;
using BeatmapExporterCore.Utilities;

namespace BeatmapExporterCLI
{
    internal class ExporterLoader
    {
        static async Task Main(string[] args) 
        {
            // effectively just block for CLI update check - we want the update notification to appear first
            // rather than interrupt later output
            var update = await ExporterUpdater.CheckNewerVersionAvailable();
            if (update.HasValue)
            {
                Console.WriteLine($"UPDATE AVAILABLE for BeatmapExporter: ({update.Value.Current} -> {update.Value.New})\n{ExporterUpdater.Latest}\n");
            }

            // currently only load lazer, can add interface for selecting osu stable here later
            ExporterApp exporter = LazerLoader.Load(args.FirstOrDefault());

            exporter.StartApplicationLoop();

            ExporterApp.Exit();
        }
    }
}