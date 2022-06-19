using LazerExporter.OsuDB;
using LazerExporter.OsuDB.Schema;
using System.Diagnostics;
using System.IO.Compression;
using System.Runtime.InteropServices;

namespace LazerExporter
{
    public static class StringExt
    {
        public static string Trunc(this string str, int len) => String.IsNullOrEmpty(str) ? str : str.Length <= len ? str : str[..len];
    }

    public class LazerExporter
    {
        LazerDatabase lazerDb;
        
        readonly List<BeatmapSet> beatmaps; // entire set of beatmap sets 
        readonly int beatmapCount;

        // active filters, user state
        List<BeatmapFilter> filters = new();
        int selectedCount;

        public LazerExporter(LazerDatabase lazerDb, List<BeatmapSet> beatmaps)
        {
            this.lazerDb = lazerDb;
            var nonEmpty = beatmaps.Where(set => set.Beatmaps.Count > 0).OrderBy(set => set.OnlineID).ToList();
            this.beatmaps = nonEmpty;

            int count = beatmaps.Select(b => b.Beatmaps.Count).Sum();
            this.beatmapCount = count;
            this.selectedCount = count;
        }
        
        public void StartApplicationLoop()
        {
            while(true)
            {
                ApplicationLoop();
            }
        }

        void ApplicationLoop()
        {
            // output main application menu
            int selectedSets = SelectedBeatmaps().Count();
            Console.Write($"\n1. Export selected beatmaps ({selectedSets} beatmap sets, {selectedCount} beatmaps)\n2. Display selected beatmaps ({selectedCount}/{beatmapCount} beatmaps)\n3. Edit beatmap selection/filters\n\n0. Exit\nSelect operation: ");

            string? input = Console.ReadLine();
            if(input is null)
            {
                LazerLoader.Exit();
            }

            if (!Int32.TryParse(input, out int op) || op is < 0 or > 3)
            {
                Console.WriteLine("\nInvalid operation selected.");
                return;
            }

            switch (op)
            {
                case 0:
                    LazerLoader.Exit();
                    break;
                case 1:
                    ExportBeatmaps();
                    break;
                case 2:
                    DisplaySelectedBeatmaps();
                    break;
                case 3:
                    BeatmapFilterSelection();
                    break;
            }
        }

        IEnumerable<BeatmapSet> SelectedBeatmaps() =>
            from beatmap in beatmaps
            where beatmap.SelectedBeatmaps.Count > 0
            select beatmap;

        void DisplaySelectedBeatmaps()
        {
            // display ALL currently selected beatmaps
            var selectedBeatmaps = SelectedBeatmaps().ToList();
            foreach (var map in selectedBeatmaps)
            {
                Console.WriteLine(map.Display());
            }
        }

        void BeatmapFilterSelection()
        {
            Console.WriteLine("\n--- Beatmap Selection ---");

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
- Specific gamemode only: mode osu/mania/ctb/taiko
- Remove a specific filter (using line number from list above): remove 1
- Remove all filters: reset
Back to export menu: exit"
);
            while (true)
            {
                if (filters.Count > 0)
                {
                    var filterInfo = filters.Select((f, i) =>
                    {
                        int includedCount = beatmaps.SelectMany(set => set.Beatmaps).Count(b => f.Includes(b));
                        return $"{i + 1}. {f.Description} ({includedCount} matches)";
                    });
                    Console.Write("----------------------\nCurrent beatmap filters:\n\n");
                    Console.Write(String.Join("\n", filterInfo));
                    Console.WriteLine();
                    Console.WriteLine($"Matched beatmaps: {selectedCount}/{beatmapCount}\n");
                }
                else
                {
                    Console.Write("\nThere are no active beatmap filters. ALL beatmaps currently selected for export.\n\n");
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
                        if(filter is not null)
                        {
                            filters.Add(filter);
                            UpdateSelectedBeatmaps();
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
            if (!Int32.TryParse(idArg, out int id) || (id < 1 || id > filters.Count))
            {
                Console.WriteLine($"Not an existing rule ID: {idArg}");
                return;
            }

            filters.RemoveAt(id - 1);
            Console.WriteLine("Filter removed.");
            UpdateSelectedBeatmaps();
            return;
        }

        void ResetBeatmapFilters()
        {
            filters.Clear();
            UpdateSelectedBeatmaps();
        }

        void ExportBeatmaps()
        {
            // perform export operation for currently selected beatmaps 
            var selected = SelectedBeatmaps().ToList();
            int selectedForExport = selected.Count;

            // produce set of excluded file hashes of difficulty files that should be skipped
            // these are difficulties in original beatmap but not 'selected'
            // when doing file export, we do not want to care about what difficulty file, audio file, etc we are exporting 
            var excludedHashes =
                from set in selected // get all sets that contain at least one selected difficulty
                from map in set.Beatmaps // get each difficulty (regardless of filtering) from those sets
                where !set.SelectedBeatmaps.Contains(map) // get difficulties that will be filtered for export
                select map.Hash;
            var excluded = excludedHashes.ToList();

            string exportDir = "lazerexport";
            Directory.CreateDirectory(exportDir);
            Console.WriteLine($"Selected {selectedForExport} beatmap sets for export.");

            if(RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                Process.Start("explorer.exe", exportDir);
            }

            // export all named files, excluding filtered difficulties into .osz archive
            int attempted = 0;
            int exported = 0;

            foreach (var mapset in selected)
            {
                attempted++;
                string filename = mapset.ArchiveFilename();
                Console.WriteLine($"Exporting beatmap ({exported}/{selectedForExport}): {filename}");
                try
                {
                    using FileStream export = File.Open(Path.Combine(exportDir, filename), FileMode.CreateNew);
                    using ZipArchive osz = new(export, ZipArchiveMode.Create, true);
                    foreach (var namedFile in mapset.Files)
                    {
                        string hash = namedFile.File.Hash;
                        if (excluded.Contains(hash))
                            continue;
                        var entry = osz.CreateEntry(namedFile.Filename, CompressionLevel.NoCompression);
                        using var entryStream = entry.Open();
                        // open the actual difficulty/audio/image file from the lazer file store
                        using var file = lazerDb.OpenHashedFile(hash);
                        file?.CopyTo(entryStream);
                    }
                    exported++;
                } catch (IOException ioe)
                {
                    Console.WriteLine($"Unable to export {filename} :: {ioe.Message}");
                }
            }
            Console.WriteLine($"Exported {exported}/{selectedForExport} beatmaps to {Path.GetFullPath(exportDir)}.");
        }

        void UpdateSelectedBeatmaps()
        {
            int selectedCount = 0;
            foreach (var set in beatmaps)
            {
                var filteredMaps =
                    from map in set.Beatmaps
                    where filters.All(f => f.Includes(map))
                    select map;
                var selected = filteredMaps.ToList();

                set.SelectedBeatmaps = selected;
                selectedCount += selected.Count;
            }
            this.selectedCount = selectedCount;
        }
    }
}
