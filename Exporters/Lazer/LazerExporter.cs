using BeatmapExporter.Exporters.Lazer.LazerDB;
using BeatmapExporter.Exporters.Lazer.LazerDB.Schema;
using System.IO.Compression;

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
            
        readonly Transcoder transcoder;

        readonly Dictionary<string, List<Beatmap>>? collections;
        List<string> selectedFromCollections;

        public LazerExporter(LazerDatabase lazerDb, List<BeatmapSet> beatmapSets, List<BeatmapCollection>? lazerCollections)
        {
            this.lazerDb = lazerDb;

            var nonEmpty = beatmapSets.Where(set => set.Beatmaps.Count > 0).OrderBy(set => set.OnlineID).ToList();
            this.beatmapSets = nonEmpty;
            this.selectedBeatmapSets = nonEmpty;

            var allBeatmaps = nonEmpty.SelectMany(s => s.Beatmaps).ToList();

            int count = allBeatmaps.Count;
            this.beatmapCount = count;
            this.selectedBeatmapCount = count;

            this.selectedFromCollections = new();

            this.config = new ExporterConfiguration("lazerexport", "lazerexport.zip");

            if(lazerCollections != null)
            {
                collections = new();
                foreach (var coll in lazerCollections)
                {
                    var collMaps = allBeatmaps
                        .Where(b => coll.BeatmapMD5Hashes.Contains(b.MD5Hash))
                        .ToList();
                    collections[coll.Name] = collMaps;
                }
            } 
            else
            {
                Console.WriteLine("Collection filtering and information will not be available.");
                collections = null;
            }

            this.transcoder = new Transcoder();
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

        public int CollectionCount
        {
            get => collections?.Count ?? 0;
        }

        public ExporterConfiguration Configuration
        {
            get => config;
        }

        public List<string> SelectedFromCollections
        {
            get => selectedFromCollections;
        }

        IEnumerable<String> FilterInfo() => config.Filters.Select((f, i) =>
        {
            int includedCount = beatmapSets.SelectMany(set => set.Beatmaps).Count(b => f.Includes(b));
            return $"{i + 1}. {f.Description} ({includedCount} beatmaps)";
        });

        public string FilterDetail() => string.Join("\n", FilterInfo());

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
            // produce set of excluded file hashes of difficulty files that should be skipped
            // these are difficulties in original beatmap but not 'selected'
            // when doing file export, we do not want to care about what difficulty file, audio file, etc we are exporting 
            var excludedHashes =
                from set in selectedBeatmapSets // get all sets that contain at least one selected difficulty
                from map in set.Beatmaps // get each difficulty (regardless of filtering) from those sets
                where !set.SelectedBeatmaps.Contains(map) // get difficulties that will be filtered for export
                select map.Hash;
            var excluded = excludedHashes.ToList();

            string exportDir = config.ExportPath;
            Directory.CreateDirectory(exportDir);
            Console.WriteLine($"Selected {SelectedBeatmapSetCount} beatmap sets for export.");

            BeatmapExporter.OpenExportDirectory(exportDir);

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
                Console.WriteLine($"Exporting beatmap set ({attempted}/{SelectedBeatmapSetCount}): {filename}");

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
            Console.WriteLine($"Exported {exported}/{SelectedBeatmapSetCount} beatmaps to {location}.");
        }

        public void ExportAudioFiles()
        {
            // perform export of songs as .mp3 files
            string exportDir = config.AudioExportPath;
            Directory.CreateDirectory(exportDir);

            Console.WriteLine($"Exporting audio from {selectedBeatmapSets.Count} beatmap sets to as .mp3 files.");
            if (transcoder.Available)
                Console.WriteLine("This operation will take longer if many selected beatmaps are not in .mp3 format.");
            else
                Console.WriteLine("FFmpeg runtime not found. Beatmaps that use other audio formats than .mp3 will be skipped.\nMake sure ffmpeg.exe is located on the system PATH or placed in the directory with this BeatmapExporter.exe to enable transcoding.");

            BeatmapExporter.OpenExportDirectory(exportDir);

            int exportedAudioFiles = 0;
            int attempted = 0;
            foreach (var mapset in selectedBeatmapSets)
            {
                // get any beatmap diffs from this set with different audio files
                // typically 1 'audio.mp3' only by convention. could also be multiple across different difficulties
                var uniqueMetadata = mapset
                    .SelectedBeatmaps
                    .Select(b => b.Metadata)
                    .GroupBy(m => m.AudioFile)
                    .Select(g => g.First())
                    .ToList();

                foreach (var metadata in uniqueMetadata)
                {
                    try
                    {
                        // transcode if audio is not in .mp3 format
                        string extension = Path.GetExtension(metadata.AudioFile);
                        bool transcode = extension.ToLower() != ".mp3";
                        string transcodeNotice = transcode ? $" (transcode required from {extension})" : "";

                        // produce more meaningful filename than 'audio.mp3' 
                        string outputFilename = metadata.OutputAudioFilename(mapset.OnlineID);
                        string outputFile = Path.Combine(exportDir, outputFilename);

                        attempted++;
                        Console.WriteLine($"({attempted}/?) Exporting {outputFilename}{transcodeNotice}");

                        using FileStream? audio = lazerDb.OpenNamedFile(mapset, metadata.AudioFile);
                        if (audio is null)
                            continue;

                        if (transcode)
                        {
                            // transcoder (FFmpeg) not available, skipping. 
                            if (!transcoder.Available)
                            {
                                Console.WriteLine($"Beatmap has non-mp3 audio: {metadata.AudioFile}. FFmpeg not loaded, skipping.");
                                continue;
                            }
                            try
                            {
                                transcoder.TranscodeMP3(audio, outputFile);
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine($"Unable to transcode audio: {metadata.AudioFile}. An error occured :: {e.Message}");
                                continue;
                            }
                        }
                        else
                        {
                            using FileStream output = File.Open(outputFile, FileMode.CreateNew);
                            audio.CopyTo(output);
                        }

                        // set mp3 tags 
                        var mp3 = TagLib.File.Create(outputFile);
                        if (string.IsNullOrEmpty(mp3.Tag.Title))
                            mp3.Tag.Title = metadata.TitleUnicode;
                        if(mp3.Tag.Performers.Count() == 0) 
                            mp3.Tag.Performers = new[] { metadata.ArtistUnicode };
                        if(string.IsNullOrEmpty(mp3.Tag.Description))
                            mp3.Tag.Description = metadata.Tags;
                        mp3.Tag.Comment = $"{mapset.OnlineID} {metadata.Tags}";

                        // set beatmap background as album cover 
                        if(mp3.Tag.Pictures.Count() == 0 && metadata.BackgroundFile is not null)
                        {
                            using FileStream? bg = lazerDb.OpenNamedFile(mapset, metadata.BackgroundFile);
                            if(bg is not null)
                            {
                                using MemoryStream ms = new();
                                bg.CopyTo(ms);
                                byte[] image = ms.ToArray();

                                var cover = new TagLib.Id3v2.AttachmentFrame
                                {
                                    Type = TagLib.PictureType.FrontCover,
                                    Description = "Background",
                                    MimeType = System.Net.Mime.MediaTypeNames.Image.Jpeg,
                                    Data = image,
                                };
                                mp3.Tag.Pictures = new[] { cover };
                            }
                        }

                        mp3.Save();
                        exportedAudioFiles++;
                    } 
                    catch (IOException e)
                    {
                        Console.WriteLine($"Unable to export beatmap :: {e.Message}");
                    }
                }
            }

            string location = Path.GetFullPath(exportDir);
            Console.WriteLine($"Exported {exportedAudioFiles}/{attempted} audio files from {SelectedBeatmapCount} beatmaps to {location}.");
        }

        public void DisplayCollections()
        {
            if(collections is not null)
            {
                Console.Write("osu! collections:\n\n");
                foreach (var (name, maps) in collections)
                {
                    Console.WriteLine($"{name} ({maps.Count} beatmaps)");
                }
            } 
            else
            {
                Console.WriteLine("Your osu!lazer collection database was not able to be loaded. Collection information and filtering is not available.");
            }
            Console.Write("\nThe collection names as shown here can be used with the \"collection\" beatmap filter.\n");
        }

        public void UpdateSelectedBeatmaps()
        {
            List<string> collFilters = new();
            List<BeatmapFilter> beatmapFilters = new();
            bool negateColl = false;
            foreach (var filter in config.Filters)
            {
                if (filter.Collections is not null)
                {
                    collFilters.AddRange(filter.Collections);
                    negateColl = filter.Negate;
                }
                else
                    beatmapFilters.Add(filter);
            }

            Console.WriteLine($"collection filters: {collFilters.Count}");
            // re-build 'collection' filters to optimize/cache iteration of beatmaps/collections for these
            if (collections is not null && collFilters.Count > 0)
            {
                // build list of beatmap ids from selected filters
                var includedHashes = collections
                    .Where(c => collFilters.Any(c => c == "-all") switch
                    {
                        true => true,
                        false => collFilters.Any(selected => string.Equals(selected, c.Key, StringComparison.OrdinalIgnoreCase))
                    })
                    .SelectMany(c => c.Value.Select(b => b.ID))
                    .ToList();

                string desc = string.Join(", ", collFilters);
                BeatmapFilter collFilter = new($"Collection filter: {(negateColl ? "NOT in " : "")}{desc}", negateColl,
                    b => includedHashes.Contains(b.ID));

                // with placeholder collection filters removed, add re-built filter 
                beatmapFilters.Add(collFilter);
            }

            // collection filter will either be rebuilt above or removed (if collections are not available)
            config.Filters = beatmapFilters;

            // compute and cache 'selected' beatmaps based on current filters
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
