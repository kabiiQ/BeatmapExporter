using BeatmapExporter.Exporters;
using BeatmapExporter.Exporters.Lazer;
using BeatmapExporter.Exporters.Lazer.LazerDB.Schema;
using BeatmapExporterCore.Exporters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace BeatmapExporterCLI.Interface
{
    /// <summary>
    /// Class wrapping a LazerExporter into a CLI interface, outputting progress and all data to console
    /// Contains methods that are used to produce CLI-specific outputs
    /// </summary>
    public class LazerExporterCLI
    {
        public LazerExporterCLI(LazerExporter exporter)
        {
            Exporter = exporter;
        }

        public LazerExporter Exporter { get; }

        public ExporterConfiguration Configuration => Exporter.Configuration;

        public string ExportFormatUnitName => Configuration.ExportFormat switch
        {
            ExporterConfiguration.Format.Beatmap => "osu! beatmaps (.osz)",
            ExporterConfiguration.Format.Audio => "audio (.mp3)",
            ExporterConfiguration.Format.Background => "beatmap backgrounds",
            _ => throw new NotImplementedException()
        };

        public void ExportBeatmaps()
        {

        }

        public void ExportAudioFiles()
        {

        }

        public void ExportBackgroundFiles()
        {

        }

        public void DisplaySelectedBeatmaps()
        {

        }

        public void DisplayCollections()
        {

        }

        public void ExportConfiguration()
        {
            while (true)
            {
                StringBuilder settings = new();
                settings
                    .Append("\n--- Advanced export settings ---\n* indicates a setting that has been changed.\n")
                    .Append("\n1. ");

                bool exportBeatmaps = Configuration.ExportFormat == ExporterConfiguration.Format.Beatmap;
                switch (Configuration.ExportFormat)
                {
                    case ExporterConfiguration.Format.Beatmap:
                        settings.Append("Type 1: Beatmaps will be exported in osu! archive format (.osz)");
                        break;
                    case ExporterConfiguration.Format.Audio:
                        settings.Append("Type 2: Beatmap audio files will be renamed, tagged and exported (.mp3 format)*");
                        break;
                    case ExporterConfiguration.Format.Background:
                        settings.Append("Type 3: Only beatmap background images will be exported (original format)*");
                        break;
                }

                settings
                    .Append("\n2. Export path: ")
                    .Append(Path.GetFullPath(Configuration.ExportPath));
                if (Configuration.ExportPath != Configuration.DefaultExportPath)
                    settings.Append('*');

                if (exportBeatmaps)
                {
                    settings.Append("\n3. ");
                    if (Configuration.CompressionEnabled)
                        settings.Append(".osz compression is enabled (slow export, smaller file sizes)*");
                    else
                        settings.Append(".osz compression is disabled (fastest export)");
                }

                settings.Append("\n\nEdit setting # (Blank to save settings): ");

                Console.Write(settings.ToString());
                string? input = Console.ReadLine();
                if (string.IsNullOrEmpty(input) || !int.TryParse(input, out int op) || op < 1 || op > (exportBeatmaps ? 4 : 2))
                {
                    Console.Write("\nInvalid operation selected.\n");
                    return;
                }

                switch (op)
                {
                    case 1:
                        switch (Configuration.ExportFormat)
                        {
                            case ExporterConfiguration.Format.Beatmap:
                                Configuration.ExportFormat = ExporterConfiguration.Format.Audio;
                                break;
                            case ExporterConfiguration.Format.Audio:
                                Configuration.ExportFormat = ExporterConfiguration.Format.Background;
                                break;
                            case ExporterConfiguration.Format.Background:
                                Configuration.ExportFormat = ExporterConfiguration.Format.Beatmap;
                                break;
                        }
                        break;
                    case 2:
                        Console.Write($"\nPath selected must be valid for your platform or export will fail! Be careful of invalid filename characters on Windows.\nAudio exports will automatically export to a '{ExporterConfiguration.DefaultAudioPath}' folder at this location.\nDefault export path: {Configuration.DefaultExportPath}\nCurrent export path: {Configuration.ExportPath}\nNew export path: ");
                        string? pathInput = Console.ReadLine();
                        if (string.IsNullOrEmpty(pathInput))
                            continue;
                        Configuration.ExportPath = pathInput;
                        Console.WriteLine($"- CHANGED: Export location set to {Path.GetFullPath(Configuration.ExportPath)}");
                        break;
                    case 3:
                        if (Configuration.CompressionEnabled)
                        {
                            Console.WriteLine("- CHANGED: .osz output compression has been disabled.");
                            Configuration.CompressionEnabled = false;
                        }
                        else
                        {
                            Console.WriteLine("- CHANGED: .osz output compression has been enabled.");
                            Configuration.CompressionEnabled = true;
                        }
                        break;
                }
            }
        }

        #region Beatmap Filters
        public void BeatmapFilterSelection()
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
- Beatmaps added in the last 7 days: since 7
- Beatmaps added in the last 5 hours: since 5:00
- Specific beatmap ID (comma-separated): id 1
- Mapped by RLC or Nathan (comma-separated): author RLC, Nathan
- Specific artists (comma-separated): artist Camellia, nanahira
- Tags include ""touhou"": tag touhou
- Specific gamemodes: mode osu/mania/ctb/taiko
- Beatmap status: graveyard/leaderboard/ranked/approved/qualified/loved
- Contained in a specific collection called ""songs"": collection songs
- Contained in a specific collection labeled #1 in the collection list: collection #1
- Contained in ANY collection: collection -all 
- Remove a specific filter (using line number from list above): remove 1
- Remove all filters: reset
Back to export menu: exit
"
);

            while (true)
            {
                var filters = Configuration.Filters;
                if (filters.Count > 0)
                {
                    Console.Write("----------------------\nCurrent beatmap filters:\n\n");
                    Console.Write(FilterDetail());
                    Console.Write($"\nMatched beatmap sets: {Exporter.SelectedBeatmapSetCount}/{Exporter.TotalBeatmapSetCount}\n\n");
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
                        string? idArg = command.ElementAtOrDefault(1);
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
                            Exporter.UpdateSelectedBeatmaps();
                            Console.Write("\nFilter added.\n\n");
                        }
                        else
                        {
                            Console.WriteLine($"Invalid filter '{command[0]}'.");
                        }
                        break;
                }
            }
        }

        public string FilterDetail()
        {
            var filterInfo = Exporter.Filters().Select(filter => $"{filter.Id}. {filter.Description} ({filter.DiffCount} beatmaps)");
            return string.Join("\n", filterInfo);
        }

        void TryRemoveBeatmapFilter(string? idArg)
        {
            var filters = Configuration.Filters;
            if (idArg is null || !int.TryParse(idArg, out int id) || id < 1 || id > filters.Count)
            {
                Console.WriteLine($"Not an existing rule ID: {idArg}");
                return;
            }

            filters.RemoveAt(id - 1);
            Console.WriteLine("Filter removed.");
            Exporter.UpdateSelectedBeatmaps();
            return;
        }

        void ResetBeatmapFilters()
        {
            Configuration.Filters.Clear();
            Exporter.UpdateSelectedBeatmaps();
        }
        #endregion
    }
}
