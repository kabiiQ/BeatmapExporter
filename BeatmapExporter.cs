using BeatmapExporter.Exporters;
using System.Text;

namespace BeatmapExporter
{
    public static class StringExt
    {
        public static string Trunc(this string str, int len) => string.IsNullOrEmpty(str) ? str : str.Length <= len ? str : str[..len];
    }

    public class BeatmapExporter
    {
        readonly IBeatmapExporter exporter;
        readonly ExporterConfiguration config;

        public BeatmapExporter(IBeatmapExporter exporter)
        {
            this.exporter = exporter;
            this.config = exporter.Configuration;
        }

        public void StartApplicationLoop()
        {
            while (true)
            {
                ApplicationLoop();
            }
        }

        void ApplicationLoop()
        {
            // output main application menu
            Console.Write($"\n1. Export selected beatmaps ({exporter.SelectedBeatmapSetCount} beatmap sets, {exporter.SelectedBeatmapCount} beatmaps)\n2. Display selected beatmap sets ({exporter.SelectedBeatmapSetCount}/{exporter.BeatmapSetCount} beatmap sets)\n3. Advanced export settings (compression, export as zip, export location)\n4. Edit beatmap selection/filters\n\n0. Exit\nSelect operation: ");

            string? input = Console.ReadLine();
            if (input is null)
            {
                ExporterLoader.Exit();
            }

            if (!int.TryParse(input, out int op) || op is < 0 or > 4)
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
                    exporter.ExportBeatmaps();
                    break;
                case 2:
                    exporter.DisplaySelectedBeatmaps();
                    break;
                case 3:
                    ExportConfiguration();
                    break;
                case 4:
                    BeatmapFilterSelection();
                    break;
            }
        }

        void ExportConfiguration()
        {
            while(true)
            {
                StringBuilder settings = new();
                settings
                    .Append("\n--- Advanced export settings ---\n* indicates a setting that has been changed.\n")
                    .Append("\n1. Export path: ")
                    .Append(Path.GetFullPath(config.ExportPath));
                if (config.ExportPath != config.DefaultExportPath)
                    settings.Append('*');

                settings.Append("\n2. ");
                if (config.CompressionEnabled)
                    settings.Append(".osz compression is enabled (slow export, smaller file sizes)*");
                else
                    settings.Append(".osz compression is disabled (fastest export)");

                settings.Append("\n3. ");
                if (config.ExportSingleArchive)
                {
                    string compressed = config.CompressionEnabled ? "compressed*" : "uncompressed";
                    settings.Append($"Exporting as a single {compressed}(#2) .zip archive.*");
                }
                else
                {
                    settings.Append("Exporting as individual .osz files.");
                }

                settings.Append("\n\nEdit setting # (Blank to save settings): ");

                Console.Write(settings.ToString());
                string? input = Console.ReadLine();
                if(string.IsNullOrEmpty(input) || !int.TryParse(input, out int op) || op is < 1 or > 3)
                {
                    Console.Write("\nInvalid operation selected.\n");
                    return;
                }

                switch(op)
                {
                    case 1:
                        Console.Write($"\nPath selected must be valid for your platform or export will fail! Be careful of invalid filename characters on Windows.\nDefault export path: {config.DefaultExportPath}\nCurrent export path: {config.ExportPath}\nNew export path: ");
                        string? pathInput = Console.ReadLine();
                        if (string.IsNullOrEmpty(pathInput))
                            continue;
                        config.ExportPath = pathInput;
                        Console.WriteLine($"Export location set to {Path.GetFullPath(config.ExportPath)}");
                        break;
                    case 2:
                        if(config.CompressionEnabled)
                        {
                            Console.WriteLine(".osz and .zip output compression has been disabled.");
                            config.CompressionEnabled = false;
                        }
                        else
                        {
                            Console.WriteLine(".osz and .zip output compression has been enabled.");
                            config.CompressionEnabled = true;
                        }
                        break;
                    case 3:
                        if(config.ExportSingleArchive)
                        {
                            Console.WriteLine("Beatmaps will be exported as individual .osz files.");
                            config.ExportSingleArchive = false;
                        } 
                        else
                        {
                            Console.WriteLine("Beatmaps will be exported as a single .zip archive.");
                            config.ExportSingleArchive = true;
                        }
                        break;
                }
            }
        }

        void BeatmapFilterSelection()
        {
            Console.Write("\n--- Beatmap Selection ---\n");

            Console.Write(
    @"Only beatmaps which match ALL ACTIVE FILTERS will be exported.
Prefixing the filter with ""!"" will negate the filter, if you want to use a ""less than"" filter. ""!"" can be used with all filters, though an example is only shown for star rating.

Examples:
- To only export beatmaps 6.3 stars and above: stars 6.3
- Below 6.3 stars (using negation): !stars 6.3
- Longer than 1:30 (90 seconds): length 90
- 180BPM and above: bpm 180
- Specific beatmap ID (comma-separated): id 1
- Mapped by RLC or Nathan (comma-separated): author RLC, Nathan
- Specific artists (comma-separated): artist Camellia, nanahira
- Tags include ""touhou"": tag touhou
- Specific gamemodes: mode osu/mania/ctb/taiko
- Beatmap status: graveyard/leaderboard/ranked/approved/qualified/loved
- Remove a specific filter (using line number from list above): remove 1
- Remove all filters: reset
Back to export menu: exit"
);

            var filters = config.Filters;
            while (true)
            {
                if (filters.Count > 0)
                {
                    Console.Write("----------------------\nCurrent beatmap filters:\n\n");
                    Console.Write(exporter.FilterDetail());
                    Console.Write($"\nMatched beatmap sets: {exporter.SelectedBeatmapSetCount}/{exporter.BeatmapSetCount}\n\n");
                }
                else
                {
                    Console.Write("\n\nThere are no active beatmap filters. ALL beatmaps currently selected for export.\n\n");
                }

                // start filter selection ui mode
                Console.Write("Select filter (Blank to save selection): ");

                string? input = Console.ReadLine()?.ToLower();
                if (string.IsNullOrEmpty(input))
                {
                    return;
                }

                string[] command = input.Split(" ");
                // check for filter "remove" operations, otherwise pass to parse filter 
                switch (command[0])
                {
                    case "remove":
                        string idArg = command[1];
                        TryRemoveBeatmapFilter(idArg);
                        break;
                    case "reset":
                        ResetBeatmapFilters();
                        break;
                    case "exit":
                        return;
                    default:
                        // parse as new filter
                        BeatmapFilter? filter = new FilterParser(input).Parse();
                        if (filter is not null)
                        {
                            filters.Add(filter);
                            exporter.UpdateSelectedBeatmaps();
                            Console.Write("\nFilter added.\n\n");
                        }
                        else
                        {
                            Console.WriteLine($"Unknown filter type {command[0]}.");
                        }
                        break;
                }
            }
        }

        void TryRemoveBeatmapFilter(string idArg)
        {
            var filters = config.Filters;
            if (!int.TryParse(idArg, out int id) || id < 1 || id > filters.Count)
            {
                Console.WriteLine($"Not an existing rule ID: {idArg}");
                return;
            }

            filters.RemoveAt(id - 1);
            Console.WriteLine("Filter removed.");
            exporter.UpdateSelectedBeatmaps();
            return;
        }

        void ResetBeatmapFilters()
        {
            config.Filters.Clear();
            exporter.UpdateSelectedBeatmaps();
        }
    }
}
