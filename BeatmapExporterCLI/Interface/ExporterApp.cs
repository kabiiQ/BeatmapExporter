using BeatmapExporter.Exporters;
using BeatmapExporter.Exporters.Lazer;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace BeatmapExporterCLI.Interface
{
    /// <summary>
    /// Class containing the main application logic for CLI flow 
    /// </summary>
    public class ExporterApp
    {
        public ExporterApp(LazerExporterCLI cli)
        {
            CLI = cli;
            Exporter = cli.Exporter;
            Configuration = Exporter.Configuration;
        }

        public LazerExporterCLI CLI { get; }

        public LazerExporter Exporter { get; }

        public ExporterConfiguration Configuration { get; }

        public void StartApplicationLoop()
        {
            while (true)
            {
                ApplicationLoop();
            }
        }

        public static void Exit()
        {
            // keep console open
            Console.Write("\nPress any key to exit.\n");
            Console.ReadKey();
            Environment.Exit(0);
        }

        void ApplicationLoop()
        {
            // output main application menu
            Console.Write($"\n1. Export selected {CLI.ExportFormatUnitName} ({Exporter.SelectedBeatmapSetCount} beatmap sets, {Exporter.SelectedBeatmapCount} beatmaps)\n2. Display selected beatmap sets ({Exporter.SelectedBeatmapSetCount}/{Exporter.TotalBeatmapSetCount} beatmap sets)\n3. Display {Exporter.CollectionCount} beatmap collections\n4. Advanced export settings (.mp3/image export, compression, export location)\n5. Edit beatmap selection/filters\n\n0. Exit\nSelect operation: ");

            string? input = Console.ReadLine();
            if (input is null)
            {
                ExporterApp.Exit();
            }

            if (!int.TryParse(input, out int op) || op is < 0 or > 5)
            { 
                Console.WriteLine("\nInvalid operation selected.");
                return;
            }

            switch (op)
            {
                case 0:
                    Environment.Exit(0);
                    break;
                case 1:
                    switch (Configuration.ExportFormat)
                    {
                        case ExporterConfiguration.Format.Beatmap:
                            CLI.ExportBeatmaps();
                            break;
                        case ExporterConfiguration.Format.Audio:
                            CLI.ExportAudioFiles();
                            break;
                        case ExporterConfiguration.Format.Background:
                            CLI.ExportBackgroundFiles();
                            break;
                    }
                    break;
                case 2:
                    CLI.DisplaySelectedBeatmaps();
                    break;
                case 3:
                    CLI.DisplayCollections();
                    break;
                case 4:
                    CLI.ExportConfiguration();
                    break;
                case 5:
                    CLI.BeatmapFilterSelection();
                    break;
            }
        }
    }
}
