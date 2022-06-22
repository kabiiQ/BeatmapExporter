using BeatmapExporter.Exporters.Lazer.LazerDB;
using BeatmapExporter.Exporters.Lazer.LazerDB.Schema;
using System.Diagnostics;
using System.IO.Compression;
using System.Runtime.InteropServices;

namespace BeatmapExporter.Exporters.Lazer
{
    public class LazerExporter : IBeatmapExporter
    {
        readonly LazerDatabase lazerDb;
        readonly ExporterConfiguration config;

        readonly List<BeatmapSet> beatmapSets;
        readonly int beatmapCount;

        int selectedBeatmapCount; // internally maintained count of selected beatmaps
        List<BeatmapSet> selectedBeatmapSets;

        public LazerExporter(LazerDatabase lazerDb, List<BeatmapSet> beatmapSets)
        {
            this.lazerDb = lazerDb;

            var nonEmpty = beatmapSets.Where(set => set.Beatmaps.Count > 0).OrderBy(set => set.OnlineID).ToList();
            this.beatmapSets = nonEmpty;
            this.selectedBeatmapSets = nonEmpty;

            int count = nonEmpty.Select(b => b.Beatmaps.Count).Sum();
            this.beatmapCount = count;
            this.selectedBeatmapCount = count;

            this.config = new ExporterConfiguration("lazerexport", "lazerexport.zip");
        }


        public int BeatmapSetCount
        {
            get => beatmapSets.Count;
        }

        public int BeatmapCount
        {
            get => beatmapCount;
        }

        public int SelectedBeatmapSetCount
        {
            get => selectedBeatmapSets.Count;
        }

        public int SelectedBeatmapCount
        {
            get => selectedBeatmapCount;
        }

        public ExporterConfiguration Configuration
        {
            get => config;
        }

        IEnumerable<String> filterInfo() => config.Filters.Select((f, i) =>
        {
            int includedCount = beatmapSets.SelectMany(set => set.Beatmaps).Count(b => f.Includes(b));
            return $"{i + 1}. {f.Description} ({includedCount} beatmaps)";
        });

        public string FilterDetail() => string.Join("\n", filterInfo());

        public void DisplaySelectedBeatmaps()
        {
            // display ALL currently selected beatmaps
            foreach (var map in selectedBeatmapSets)
            {
                Console.WriteLine(map.Display());
            }
        }

        public void ExportBeatmaps()
        {
            // perform export operation for currently selected beatmaps 
            int selectedSetCount = selectedBeatmapSets.Count;
            // produce set of excluded file hashes of difficulty files that should be skipped
            // these are difficulties in original beatmap but not 'selected'
            // when doing file export, we do not want to care about what difficulty file, audio file, etc we are exporting 
            var excludedHashes =
                from set in selectedBeatmapSets // get all sets that contain at least one selected difficulty
                from map in set.Beatmaps // get each difficulty (regardless of filtering) from those sets
                where !set.SelectedBeatmaps.Contains(map) // get difficulties that will be filtered for export
                select map.Hash;
            var excluded = excludedHashes.ToList();

            string exportDir = "lazerexport";
            Directory.CreateDirectory(exportDir);
            Console.WriteLine($"Selected {selectedSetCount} beatmap sets for export.");

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                Process.Start("explorer.exe", exportDir);
            }

            // if requested, produce a single .zip archive of the export
            FileStream? exportZip = null;
            ZipArchive? exportArchive = null;
            if (config.ExportSingleArchive)
            {
                try
                {
                    exportZip = File.Open(Path.Combine(exportDir, config.ExportArchivePath), FileMode.CreateNew); // lazerexport.zip
                    exportArchive = new ZipArchive(exportZip, ZipArchiveMode.Create, true);
                } 
                catch (IOException io)
                {
                    Console.WriteLine($"Unable to create export archive {Path.GetFullPath(config.ExportArchivePath)} :: {io.Message}");
                    return;
                }
            }

            // export all named files, excluding filtered difficulties into .osz archive
            int attempted = 0;
            int exported = 0;
            foreach (var mapset in selectedBeatmapSets)
            {
                attempted++;
                string filename = mapset.ArchiveFilename();
                Console.WriteLine($"Exporting beatmap set ({attempted}/{selectedSetCount}): {filename}");

                MemoryStream? memStream = null;
                Stream? export = null;
                try
                {
                    if (exportArchive is not null)
                    {
                        // if exporting to an archive, create the .osz file in memory rather than on disk
                        memStream = new MemoryStream(5000000);
                        export = memStream;
                    }
                    else
                    {
                        string exportPath = Path.Combine(exportDir, filename);
                        export = File.Open(exportPath, FileMode.CreateNew);
                    }

                    using ZipArchive osz = new(export, ZipArchiveMode.Create, true);
                    foreach (var namedFile in mapset.Files)
                    {
                        string hash = namedFile.File.Hash;
                        if (excluded.Contains(hash))
                            continue;
                        var entry = osz.CreateEntry(namedFile.Filename, config.CompressionLevel);
                        using var entryStream = entry.Open();
                        // open the actual difficulty/audio/image file from the lazer file store
                        using var file = lazerDb.OpenHashedFile(hash);
                        file?.CopyTo(entryStream);
                    }
                    exported++;

                    if(memStream is not null)
                    {
                        // if exporting to an archive, copy the .osz archive into the .zip 
                        var oszEntry = exportArchive!.CreateEntry(filename, config.CompressionLevel);
                        using var entryStream = oszEntry.Open();
                        memStream.Seek(0, SeekOrigin.Begin);
                        memStream.CopyTo(entryStream);
                    }
                }
                catch (IOException ioe)
                {
                    Console.WriteLine($"Unable to export {filename} :: {ioe.Message}");
                }
                finally
                {
                    export?.Dispose();
                }
            }
            exportArchive?.Dispose();
            exportZip?.Dispose();

            string location = exportArchive is not null ? Path.GetFullPath(config.ExportArchivePath) : Path.GetFullPath(exportDir);
            Console.WriteLine($"Exported {exported}/{selectedSetCount} beatmaps to {location}.");
        }

        public void UpdateSelectedBeatmaps()
        {
            int selectedCount = 0;
            List<BeatmapSet> selectedSets = new();
            foreach (var set in beatmapSets)
            {
                var filteredMaps =
                    from map in set.Beatmaps
                    where config.Filters.All(f => f.Includes(map))
                    select map;
                var selected = filteredMaps.ToList();

                set.SelectedBeatmaps = selected;
                selectedCount += selected.Count;

                // after filtering beatmaps, "selected beatmap sets" will only include sets that STILL have at least 1 beatmap 
                if (selected.Count > 0)
                {
                    selectedSets.Add(set);
                }
            }

            this.selectedBeatmapSets = selectedSets;
            this.selectedBeatmapCount = selectedCount;
        }
    }
}
