using BeatmapExporterCore.Exporters.Lazer;
using BeatmapExporterCore.Exporters;
using System.Diagnostics.CodeAnalysis;

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

        /// <summary>
        /// Reference to the CLI-specific component container 
        /// </summary>
        public LazerExporterCLI CLI { get; }

        /// <summary>
        /// Reference to the general lazer exporter component container
        /// </summary>
        public LazerExporter Exporter { get; }

        /// <summary>
        /// Reference to the current configuration for export for the user
        /// </summary>
        public ExporterConfiguration Configuration { get; }
            
        /// <summary>
        /// Begins the infinite loop for the main application I/O flow.
        /// </summary>
        public void StartApplicationLoop()
        {
            while (true)
            {
                ApplicationLoop();
            }
        }

        /// <summary>
        /// Exits the program after blocking for user acknowledgement. 
        /// </summary>
        [DoesNotReturn]
        public static void Exit()
        {
            // keep console open
            Console.Write("\nPress any key to exit.\n");
            Console.ReadKey();
            Environment.Exit(0);
        }

        /// <summary>
        /// Primary CLI user interaction flow.
        /// </summary>
        void ApplicationLoop()
        {
            // output main application menu
            Console.Write($"\n1. Export selected {Configuration.ExportFormat.UnitName()} ({Exporter.SelectedBeatmapSetCount} beatmap sets, {Exporter.SelectedBeatmapCount} beatmaps)\n2. Display selected beatmap sets ({Exporter.SelectedBeatmapSetCount}/{Exporter.TotalBeatmapSetCount} beatmap sets)\n3. Display {Exporter.CollectionCount} beatmap collections\n4. Advanced export settings (.mp3/image export, compression, export location)\n5. Edit beatmap selection/filters\n\n0. Exit\nSelect operation: ");

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
                        case ExportFormat.Beatmap:
                            CLI.ExportBeatmaps();
                            break;
                        case ExportFormat.Audio:
                            CLI.ExportAudioFiles();
                            break;
                        case ExportFormat.Background:
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
