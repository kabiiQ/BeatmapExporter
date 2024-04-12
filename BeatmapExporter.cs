﻿using BeatmapExporter.Exporters;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace BeatmapExporter
{
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
            Console.Write($"\n1. Export selected {config.ExportFormatUnitName} ({exporter.SelectedBeatmapSetCount} beatmap sets, {exporter.SelectedBeatmapCount} beatmaps)\n2. Display selected beatmap sets ({exporter.SelectedBeatmapSetCount}/{exporter.BeatmapSetCount} beatmap sets)\n3. Display {exporter.CollectionCount} beatmap collections\n4. Advanced export settings (.mp3/image/score export, compression, export location)\n5. Edit beatmap selection/filters\n\n0. Exit\nSelect operation: ");

            string? input = Console.ReadLine();
            if (input is null)
            {
                ExporterLoader.Exit();
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
                    switch(config.ExportFormat)
                    {
                        case ExporterConfiguration.Format.Beatmap:
                            exporter.ExportBeatmaps();
                            break;
                        case ExporterConfiguration.Format.Audio:
                            exporter.ExportAudioFiles();
                            break;
                        case ExporterConfiguration.Format.Background:
                            exporter.ExportBackgroundFiles();
                            break;
                        case ExporterConfiguration.Format.Score:
                            exporter.ExportScores();
                            break;
                    }
                    break;
                case 2:
                    exporter.DisplaySelectedBeatmaps();
                    break;
                case 3:
                    exporter.DisplayCollections();
                    break;
                case 4:
                    ExportConfiguration();
                    break;
                case 5:
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
                    .Append("\n1. ");

                bool exportBeatmaps = config.ExportFormat == ExporterConfiguration.Format.Beatmap;
                switch(config.ExportFormat)
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
                    case ExporterConfiguration.Format.Score:
                        settings.Append("Type 4: Only scores will be exported (.osr)*");
                        break;
                }

                settings
                    .Append("\n2. Export path: ")
                    .Append(Path.GetFullPath(config.ExportPath));
                if (config.ExportPath != config.DefaultExportPath)
                    settings.Append('*');

                if(exportBeatmaps)
                {
                    settings.Append("\n3. ");
                    if (config.CompressionEnabled)
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

                switch(op)
                {
                    case 1:
                        switch (config.ExportFormat)
                        {
                            case ExporterConfiguration.Format.Beatmap:
                                config.ExportFormat = ExporterConfiguration.Format.Audio;
                                break;
                            case ExporterConfiguration.Format.Audio:
                                config.ExportFormat = ExporterConfiguration.Format.Background;
                                break;
                            case ExporterConfiguration.Format.Background:
                                config.ExportFormat = ExporterConfiguration.Format.Score;
                                break;
                            case ExporterConfiguration.Format.Score:
                                config.ExportFormat = ExporterConfiguration.Format.Beatmap;
                                break;
                        }
                        break;
                    case 2:
                        Console.Write($"\nPath selected must be valid for your platform or export will fail! Be careful of invalid filename characters on Windows.\nAudio exports will automatically export to a '{ExporterConfiguration.DefaultAudioPath}' folder at this location.\nDefault export path: {config.DefaultExportPath}\nCurrent export path: {config.ExportPath}\nNew export path: ");
                        string? pathInput = Console.ReadLine();
                        if (string.IsNullOrEmpty(pathInput))
                            continue;
                        config.ExportPath = pathInput;
                        Console.WriteLine($"- CHANGED: Export location set to {Path.GetFullPath(config.ExportPath)}");
                        break;
                    case 3:
                        if(config.CompressionEnabled)
                        {
                            Console.WriteLine("- CHANGED: .osz output compression has been disabled.");
                            config.CompressionEnabled = false;
                        }
                        else
                        {
                            Console.WriteLine("- CHANGED: .osz output compression has been enabled.");
                            config.CompressionEnabled = true;
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
                var filters = config.Filters;
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
                            exporter.UpdateSelectedBeatmaps();
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

        void TryRemoveBeatmapFilter(string? idArg)
        {
            var filters = config.Filters;
            if (idArg is null || !int.TryParse(idArg, out int id) || id < 1 || id > filters.Count)
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

        public static void OpenExportDirectory(string directory)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                Process.Start("explorer.exe", directory);
            }
        }
    }
}
