using BeatmapExporterCore.Exporters.Lazer;
using BeatmapExporterCore.Exporters;
using BeatmapExporterCore.Filters;
using System.Text;

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
         
        public void ExportBeatmaps()
        {
            Exporter.SetupExport();
            int attempted = 0, exported = 0;
            int count = Exporter.SelectedBeatmapSetCount;
            Console.WriteLine($"Selected {Exporter.SelectedBeatmapSetCount} beatmap sets for export.");
            foreach (var mapset in Exporter.SelectedBeatmapSets)
            {
                string? filename = null;
                attempted++;
                try
                {
                    Exporter.ExportBeatmap(mapset, out filename);
                    exported++;
                    Console.WriteLine($"Exported beatmap set ({attempted}/{count}): {filename}");
                } catch (Exception e)
                {
                    Console.WriteLine($"Unable to export {filename} :: ${e.Message}");
                }
            };
            Console.WriteLine($"Exported {exported}/{count} beatmaps to {Configuration.FullPath}.");
        }

        public void ExportAudioFiles()
        {
            Exporter.SetupExport();
            Console.WriteLine($"Exporting audio from {Exporter.SelectedBeatmapSetCount} beatmap sets as .mp3 files.");
            if (Exporter.TranscodeAvailable)
                Console.WriteLine("This operation will take longer if many selected beatmaps are not in .mp3 format.");
            else
                Console.WriteLine("FFmpeg runtime not found. Beatmaps that use other audio formats than .mp3 will be skipped.\nMake sure ffmpeg.exe is located on the system PATH or placed in the directory with this BeatmapExporter.exe to enable transcoding.");

            int attempted = 0, exportedAudio = 0;
            foreach (var mapset in Exporter.SelectedBeatmapSets)
            {
                var allAudio = Exporter.ExtractAudio(mapset);
                
                foreach (var audioExport in allAudio)
                {
                    attempted++;
                    string audioFile = audioExport.AudioFile.AudioFile;
                    var transcode = audioExport.TranscodeFrom != null;
                    var transcodeNotice = transcode ? $"(transcode required from {audioExport.TranscodeFrom})" : "";
                    try
                    {
                        Console.WriteLine($"({attempted}/?) Exporting {audioExport.OutputFilename}{transcodeNotice}");
                        if (transcode && !Exporter.TranscodeAvailable)
                        {
                            Console.WriteLine($"Beatmap has non-mp3 audio: {audioFile}. FFmpeg not loaded, skipping.");
                            continue;
                        }

                        void metadataFailure(Exception e) => Console.WriteLine($"Unable to set metadata for {audioExport.OutputFilename} :: {e.Message}\nExporting will continue.");
                        Exporter.ExportAudio(audioExport, metadataFailure).Wait();
                        exportedAudio++;

                    } catch (TranscodeException te)
                    {
                        Console.WriteLine($"Unable to transcode audio: {audioFile}. An error occured :: {te.Message}");
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine($"Unable to export audio: {audioFile} :: {e.Message}");
                    }
                }
            }
            Console.WriteLine($"Exported {exportedAudio}/{attempted} audio files from {Exporter.SelectedBeatmapCount} beatmaps to {Configuration.FullPath}.");
        }

        public void ExportBackgroundFiles()
        {
            Exporter.SetupExport();
            Console.WriteLine($"Exporting beatmap background images from {Exporter.SelectedBeatmapSetCount}.");

            int attempted = 0, exported = 0;
            foreach (var mapset in Exporter.SelectedBeatmapSets)
            {
                var allImages = Exporter.ExtractBackgrounds(mapset);

                foreach (var imageExport in allImages)
                {
                    attempted++;
                    var backgroundFile = imageExport.BackgroundFile.BackgroundFile;

                    try
                    {
                        Console.WriteLine($"({attempted}/?) Exporting {imageExport.OutputFilename}");
                        Exporter.ExportBackground(imageExport);
                        exported++;
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine($"Unable to export background image {backgroundFile} :: {e.Message}");
                    }
                }
            }
            Console.WriteLine($"Exported {exported}/{attempted} background files from {Exporter.SelectedBeatmapCount} beatmaps to {Configuration.FullPath}.");
        }

        public void DisplaySelectedBeatmaps()
        {
            foreach (var map in Exporter.SelectedBeatmapSets)
            {
                Console.WriteLine(map.DiffSummary());
            }
        }

        public void DisplayCollections()
        {
            Console.Write("osu! collections:\n\n");
            foreach (var (name, (index, maps)) in Exporter.Collections)
            {
                Console.WriteLine($"#{index}: {name} ({maps.Count} beatmaps)");
            }
            Console.Write("\nThe collection names as shown here can be used with the \"collection\" beatmap filter.\n");
        }

        public void ExportConfiguration()
        {
            while (true)
            {
                StringBuilder settings = new();
                settings
                    .Append("\n--- Advanced export settings ---\n* indicates a setting that has been changed.\n")
                    .Append("\n1. ");

                bool exportBeatmaps = Configuration.ExportFormat == ExportFormat.Beatmap;
                var formatId = (int)Configuration.ExportFormat + 1;
                var edited = exportBeatmaps ? "" : "*";

                settings
                    .Append($"Type {formatId}: {Configuration.ExportFormat.Descriptor()}{edited}")
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
                        Configuration.ExportFormat = Configuration.ExportFormat.Next();
                        break;
                    case 2:
                        Console.Write($"\nPath selected must be valid for your platform or export will fail! Be careful of invalid filename characters on Windows.\nAudio exports will automatically export to an 'mp3' folder at this location.\nDefault export path: {Configuration.DefaultExportPath}\nCurrent export path: {Configuration.ExportPath}\nNew export path: ");
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
- Below 6.3 stars (negation example, works for all filters): !stars 6.3
- Longer than 1:30 (90 seconds): length 90
- 180BPM and above: bpm 180
- Beatmaps added in the last 7 days: since 7
- Beatmaps added in the last 5 hours: since 5:00
- Beatmaps ranked in the last 30 days: ranked 30
- Specific beatmap ID (comma-separated): id 1
- Mapped by RLC or Nathan (comma-separated): author RLC, Nathan
- Specific artists (comma-separated): artist Camellia, nanahira
- Tags include ""touhou"": tag touhou
- Specific gamemodes: mode osu/mania/ctb/taiko
- Beatmap status: status graveyard/leaderboard/ranked/approved/qualified/loved
- Beatmap played in the last 30 days: played 30
- Beatmap has ever been played: everplayed yes
- Contained in a specific collection called ""songs"": collection songs
- Contained in a specific collection labeled #1 in the collection list: collection #1
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
                        BeatmapFilter? filter;
                        try
                        {
                            filter = new FilterParser(input).Parse();
                        } catch (ArgumentException ae)
                        {
                            Console.WriteLine($"Filter input error: {ae.Message}");
                            break;
                        }
                        if (filter is not null)
                        {
                            filters.Add(filter);
                            static void collectionFailure(string filter) => Console.WriteLine($"Unable to find collection: {filter}.");
                            Exporter.UpdateSelectedBeatmaps(collectionFailure);
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
