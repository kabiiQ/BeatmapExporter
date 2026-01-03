using BeatmapExporterCore.Exporters.Lazer.LazerDB;
using BeatmapExporterCore.Exporters.Lazer.LazerDB.Schema;
using BeatmapExporterCore.Exporters.Stable.Collections;
using BeatmapExporterCore.Filters;
using BeatmapExporterCore.Utilities;
using Nito.AsyncEx;
using System.IO.Compression;
using System.Text.RegularExpressions;

namespace BeatmapExporterCore.Exporters.Lazer
{
    /// <summary>
    /// Exception used when an mp3 transcode operation encounters any error.
    /// </summary>
    public class TranscodeException : ExporterException
    {
        public TranscodeException(Exception inner) : base(inner.Message) { }
    }

    public class LazerExporter : IBeatmapExporter
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
        
        readonly LazerDatabase lazerDb;
        readonly Transcoder transcoder;

        readonly List<Beatmap> allBeatmapDiffs;

        /// <param name="lazerDb">The lazer database, referenced for opening files later.</param>
        /// <param name="settings">The user's last known <see cref="ClientSettings"/></param>
        /// <param name="beatmapSets">All beatmap sets loaded into memory.</param>
        /// <param name="lazerCollections">All collections into memory.</param>
        /// <param name="skins">All user skins loaded into memory.</param>
        /// <param name="transcoder">The Transcoder instance to use for mp3 conversions</param>
        public LazerExporter(LazerDatabase lazerDb, ClientSettings settings, List<BeatmapSet> beatmapSets, List<BeatmapCollection> lazerCollections, List<Skin> skins, Transcoder transcoder)
        {
            this.lazerDb = lazerDb;
            this.transcoder = transcoder;

            AllBeatmapSets = beatmapSets
                .Where(set => set.Beatmaps.Count > 0)
                .OrderBy(set => set.OnlineID)
                .ToList();
            SelectedBeatmapSets = AllBeatmapSets;
            TotalBeatmapSetCount = AllBeatmapSets.Count;
            SelectedBeatmapSetCount = TotalBeatmapSetCount;

            allBeatmapDiffs = AllBeatmapSets.SelectMany(s => s.Beatmaps).ToList();

            TotalBeatmapCount = allBeatmapDiffs.Count;
            SelectedBeatmapCount = TotalBeatmapCount;

            var colCount = 0;
            Collections = new();
            foreach (var coll in lazerCollections)
            {
                colCount++;
                var colMaps = allBeatmapDiffs
                    .Where(b => coll.BeatmapMD5Hashes.Contains(b.MD5Hash))
                    .ToList();
                Collections[coll.Name] = new MapCollection(colCount, colMaps);
            }
            CollectionCount = colCount;

            Skins = skins
                .Where(s => !s.Protected && s.NamedFiles.Count > 0)
                .OrderBy(s => s.Name)
                .ToList();

            Configuration = new ExporterConfiguration(settings);
            if (settings.AppliedFilters.Count > 0)
            {
                UpdateSelectedBeatmaps();
            }
        }


        /// <summary>
        /// Count of the total beatmap sets discovered.
        /// </summary>
        public int TotalBeatmapSetCount
        {
            get; // Total count does not change
        }

        /// <summary>
        /// Count of the individual beatmap difficulties discovered.
        /// </summary>
        public int TotalBeatmapCount
        {
            get;
        }

        /// <summary>
        /// All beatmap sets, without any filtering.
        /// </summary>
        public List<BeatmapSet> AllBeatmapSets
        {
            get;
        }
        
        /// <summary>
        /// The beatmap sets currently selected, after filters are applied.
        /// </summary>
        public List<BeatmapSet> SelectedBeatmapSets
        {
            get;
            private set;
        }

        /// <summary>
        /// Count of the individual beatmap difficulties currently selected, after filters are applied.
        /// </summary>
        public int SelectedBeatmapCount
        {
            get;
            private set;
        }

        /// <summary>
        /// Count of the beatmap sets currently selected, after filters are applied.
        /// </summary>
        public int SelectedBeatmapSetCount
        {
            get;
            private set;
        }

        /// <summary>
        /// All discovered collections. The dictionary key represents the collection's name as chosen by the user.
        /// </summary>
        public Dictionary<string, MapCollection> Collections
        {
            get;
        }

        /// <summary>
        /// Count of all collections discovered
        /// </summary>
        public int CollectionCount
        {
            get;
        }

        /// <summary>
        /// All player skins.
        /// </summary>
        public List<Skin> Skins
        {
            get;
        }

        /// <summary>
        /// The (mutable) exporter configuration, containing beatmap filter rules and export settings
        /// </summary>
        public ExporterConfiguration Configuration
        {
            get;
        }

        /// <summary>
        /// If audio file transcoding is available
        /// </summary>
        public bool TranscodeAvailable => transcoder.Available;

        public record struct FilterDetail(int Id, string Description, int DiffCount);

        /// <summary>
        /// Returns a list of 'FilterDetail' containers with information about applied filters
        /// </summary>
        public IEnumerable<FilterDetail> Filters() => Configuration.Filters.Select((filter, i) =>
        {
            int diffCount = allBeatmapDiffs.Count(diff => filter.Includes(diff));
            return new FilterDetail(i + 1, filter.Description, diffCount);
        });

        /// <summary>
        /// Creates the export directory and opens the folder (for Windows platform)
        /// </summary>
        public void SetupExport(bool openDir = true)
        {
            string path;
            if (Configuration.ExportFormat == ExportFormat.CollectionDb)
            {
                var parent = Directory.GetParent(Configuration.ExportPath)!;
                path = parent.FullName;
            } else
            {
                path = Configuration.ExportPath;
            }
            Directory.CreateDirectory(path);
            if (openDir)
            {
                try
                {
                    PlatformUtil.Open(path);
                }
                catch (Exception e)
                {
                    Logger.Error(e, "Error opening export directory");
                }
            }
        }

        /// <summary>
        /// Export a single BeatmapSet, with only the 'selected' diffs 
        /// </summary>
        /// <param name="mapset">The BeatmapSet to export</param>
        /// <param name="filename">The output filename that will be used. Will be set regardless of success of export and should be used for user feedback.</param>
        /// <exception cref="IOException">The BeatmapSet export was unsuccessful and an error should be noted to the user.</exception>
        public void ExportBeatmap(BeatmapSet mapset, out string filename)
        {
            // get excluded diff hashes - every file will be exported except for the undesired difficulty files
            var excluded = mapset.ExcludedDiffHashes;

            Stream? export = null;
            try
            {
                filename = mapset.ArchiveFilename();
                string exportPath = Path.Combine(Configuration.ExportPath, filename);
                export = File.Open(exportPath, FileMode.CreateNew);

                using ZipArchive osz = new(export, ZipArchiveMode.Create, true);
                foreach (var namedFile in mapset.NamedFiles)
                {
                    string hash = namedFile.File.Hash;
                    if (excluded.Contains(hash))
                        continue;
                    var entry = osz.CreateEntry(namedFile.Filename, Configuration.CompressionLevel);
                    using var entryStream = entry.Open();
                    // open the actual difficulty/audio/image file from the lazer file store
                    using var file = lazerDb.OpenHashedFile(hash);
                    file?.CopyTo(entryStream);
                }
            }
            finally
            {
                export?.Dispose();
            }
        }
        
        public record struct AudioExportTask(BeatmapSet OriginSet, BeatmapMetadata AudioFile, string? TranscodeFrom, string OutputFilename);

        /// <summary>
        /// Analyze a beatmap set, producing AudioExportTask containers for each unique audio track.
        /// </summary>
        public IEnumerable<AudioExportTask> ExtractAudio(BeatmapSet mapset)
        {
            // perform export of songs as .mp3 files
            // get any beatmap diffs from this set with different audio files
            // typically 1 'audio.mp3' only by convention. could also be multiple across different difficulties
            var uniqueMetadata = mapset
                .SelectedBeatmaps
                .Select(b => b.Metadata)
                .DistinctBy(m => m.AudioFile)
                .ToList();

            int beatmapAudioCount = 0;
            foreach (var metadata in uniqueMetadata)
            {
                // transcode if audio is not in .mp3 format
                string extension = Path.GetExtension(metadata.AudioFile);
                string? transcodeFrom = null;
                if (!extension.Equals(".mp3", StringComparison.OrdinalIgnoreCase))
                {
                    transcodeFrom = extension;
                }

                // produce more meaningful filename than 'audio.mp3' 
                string outputFilename = metadata.OutputAudioFilename(beatmapAudioCount);

                beatmapAudioCount++;
                yield return new AudioExportTask(mapset, metadata, transcodeFrom, outputFilename);
            }
        }

        /// <summary>
        /// Export a single audio file from a beatmap.
        /// If the FFMpeg runtime is available, non-mp3 audio files will have a transcode attempted into mp3 format. 
        /// </summary>
        /// <param name="export">An AudioExportTask container, as produced by <see cref="ExportAudio(AudioExportTask, Action{Exception})"/></param>
        /// <param name="metadataFailure">An optional callback to notify users on metadata assignment failure. As metadata is non-critical, export will always continue.</param>
        /// <exception cref="TranscodeException">Audio transcode is required for this file and has failed.</exception>
        /// <exception cref="Exception">General audio file export has failed.</exception>
        public void ExportAudio(AudioExportTask export, Action<Exception>? metadataFailure)
        {            
            var (mapset, metadata, transcodeFrom, outputFilename) = export;
            string outputFile = Path.Combine(Configuration.ExportPath, outputFilename);

            using FileStream? audio = lazerDb.OpenNamedFile(mapset, metadata.AudioFile);
            if (audio is null)
            {
                throw new IOException($"Audio file {metadata.AudioFile} not found in beatmap {mapset.ArchiveFilename()}.");
            }
                
            // Create physical .mp3 file, either through transcoding or simple copying
            if (transcodeFrom != null)
            {
                // transcoder (FFmpeg) not available, skipping. 
                if (!TranscodeAvailable)
                {
                    return;
                }
                try
                {
                    AsyncContext.Run(() =>
                    {
                        transcoder.TranscodeMP3(audio, outputFile);
                    });
                    // await Task.Run(() => transcoder.TranscodeMP3(audio, outputFile)); -> changes thread context, breaking realm access later
                    // transcoder.TranscodeMP3(audio, outputFile); -> blocks main thread, breaking UI updates until completion
                }
                catch (Exception e)
                {
                    throw new TranscodeException(e);
                }
            }
            else
            {
                using FileStream output = File.Open(outputFile, FileMode.CreateNew);
                audio.CopyTo(output);
            }

            // set mp3 tags 
            try
            {
                var mp3 = TagLib.File.Create(outputFile);
                if (string.IsNullOrEmpty(mp3.Tag.Title))
                    mp3.Tag.Title = metadata.TitleUnicode;
                if (mp3.Tag.Performers.Length == 0)
                    mp3.Tag.Performers = new[] { metadata.ArtistUnicode };
                if (string.IsNullOrEmpty(mp3.Tag.Description))
                    mp3.Tag.Description = metadata.Tags;
                mp3.Tag.Comment = $"{mapset.OnlineID} {metadata.Tags}";

                // set beatmap background as album cover 
                if (mp3.Tag.Pictures.Length == 0 && metadata.BackgroundFile is not null)
                {
                    using FileStream? bg = lazerDb.OpenNamedFile(mapset, metadata.BackgroundFile);
                    if (bg is not null)
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
            }
            catch (Exception e)
            {
                metadataFailure?.Invoke(e); // Callback to notify of metadata failure, but export will continue
            }
        }

        public record struct BackgroundExportTask(BeatmapSet OriginSet, BeatmapMetadata BackgroundFile, string OutputFilename);

        /// <summary>
        /// Analyze a BeatmapSet, produding BeatmaepExportTask containers for each unique audio track.
        /// </summary>
        public IEnumerable<BackgroundExportTask> ExtractBackgrounds(BeatmapSet mapset)
        {
            // get beatmap diffs from this set with different background image filenames
            var uniqueMetadata = mapset
                .SelectedBeatmaps
                .Select(b => b.Metadata)
                .Where(m => m.BackgroundFile != null)
                .GroupBy(m => m.BackgroundFile)
                .Select(g => g.First())
                .ToList();

            int beatmapBackgroundCount = 0;
            foreach (var metadata in uniqueMetadata)
            {
                // get output filename for background including original background name
                string outputFilename = metadata.OutputBackgroundFilename(mapset.OnlineID, beatmapBackgroundCount);
                beatmapBackgroundCount++;
                yield return new BackgroundExportTask(mapset, metadata, outputFilename);
            }
        }

        /// <summary>
        /// Export a single background image from a beatmap.
        /// </summary>
        /// <param name="export">A BackgroundExportTask container, as produced by <see cref="ExportBackground(BackgroundExportTask)"/></param>
        /// <exception cref="Exception">General background file export has failed.</exception>
        public void ExportBackground(BackgroundExportTask export)
        {
            var (mapset, metadata, outputFilename) = export;
            string outputFile = Path.Combine(Configuration.ExportPath, outputFilename);

            using FileStream? background = lazerDb.OpenNamedFile(mapset, metadata.BackgroundFile);
            if (background is null)
                throw new IOException($"Background file {metadata.BackgroundFile} not found in beatmap {mapset.ArchiveFilename()}");

            using FileStream output = File.Open(outputFile, FileMode.CreateNew);
            background.CopyTo(output);
        }

        public IEnumerable<Score> GetSelectedReplays() => SelectedBeatmapSets
            .SelectMany(s => s.SelectedBeatmaps)
            .SelectMany(b => b.Scores);

        /// <summary>
        /// Export a player score replay 
        /// </summary>
        /// <param name="score">The player score to export</param>
        /// <param name="filename">The output filename that will be used. Will be set regardless of success of export and should be used for user feedback.</param>
        /// <exception cref="IOException">The Score export was unsuccessful and an error should be noted to the user.</exception>
        public void ExportReplay(Score score, out string filename)
        {
            filename = score.OutputReplayFilename();
            string outputFile = Path.Combine(Configuration.ExportPath, filename);

            string? replayFile = score.Files.FirstOrDefault()?.File?.Hash;
            if (replayFile == null)
                throw new IOException($"Replay file for {outputFile} does not exist.");

            using FileStream replay = lazerDb.OpenHashedFile(replayFile);
            using FileStream output = File.Open(outputFile, FileMode.CreateNew);
            replay.CopyTo(output);
        }
        
        /// <summary>
        /// Export a single osu!lazer skin
        /// </summary>
        /// <param name="skin">The Skin to export</param>
        /// <param name="filename">The output filename that will be used. Will be set regardless of success of export and should be used for user feedback.</param>
        /// /// <exception cref="IOException">The Skin export was unsuccessful and an error should be noted to the user.</exception>
        public void ExportSkin(Skin skin, out string filename)
        {
            Stream? export = null;
            try
            {
                filename = skin.OutputFilename();
                if (skin.Protected || skin.NamedFiles.Count == 0)
                    throw new IOException("This skin can not be exported");
                string exportPath = Path.Combine(Configuration.ExportPath, filename);
                export = File.Open(exportPath, FileMode.CreateNew);

                using ZipArchive osk = new(export, ZipArchiveMode.Create, true);
                // Export all files in the skin as-is, very similar operation to a beatmap export
                foreach (var namedFile in skin.NamedFiles)
                {
                    string hash = namedFile.File.Hash;
                    var entry = osk.CreateEntry(namedFile.Filename, Configuration.CompressionLevel);
                    using var entryStream = entry.Open();
                    using var file = lazerDb.OpenHashedFile(hash);
                    file.CopyTo(entryStream);
                }
            }
            finally
            {
                export?.Dispose();
            }
        }

        /// <summary>
        /// Export a single BeatmapSet as a "folder" for use with osu! stable, with only the 'selected' diffs.
        /// </summary>
        /// <param name="mapset">The BeatmapSet to export.</param>
        /// <param name="filename">The output directory that will be used. Will be set regardless of success of export and should be used for user feedback.</param>
        /// <exception cref="IOException">The BeatmapSet export was unsuccessful and an error should be noted to the user.</exception>
        public void ExportBeatmapFolder(BeatmapSet mapset, out string dirName)
        {
            var excluded = mapset.ExcludedDiffHashes;

            dirName = mapset.SongFolderName();
            string mapDir = Path.Combine(Configuration.ExportPath, dirName);
            foreach (var namedFile in mapset.NamedFiles)
            {
                string hash = namedFile.File.Hash;
                if (excluded.Contains(hash))
                    continue;
                using var file = lazerDb.OpenHashedFile(hash);

                string outputFile = Path.Combine(mapDir, namedFile.Filename);
                // mkdirs - beatmap set may contain sub directories for storyboard files etc
                var dir = Path.GetDirectoryName(outputFile);
                if (dir != null) 
                    Directory.CreateDirectory(dir);

                using FileStream output = File.Create(outputFile);
                file?.CopyTo(output);
            }
        }

        /// <summary>
        /// Export a single RealmNamedFileUsage, using its original filename.
        /// </summary>
        public void ExportSingleFile(RealmNamedFileUsage fileUsage)
        {
            var filename = Path.GetFileName(fileUsage.Filename); // exporting a single file with its original filename (but no sub-directories that were in the beatmap structure)
            string exportPath = Path.Combine(Configuration.ExportPath, filename);

            using var export = File.Open(exportPath, FileMode.CreateNew);
            using var file = lazerDb.OpenHashedFile(fileUsage.File.Hash);
            file.CopyTo(export);
        }

        public record struct CollectionMergeStep(string Name, int OriginalDiffs, int IncludedDiffs);

        /// <summary>
        /// Return the export target CollectionDb, opening an existing file or creating a clean instance
        /// </summary>
        /// <returns></returns>
        private CollectionDb OpenCollectionDb()
        {
            if (Configuration.MergeCollections && File.Exists(Configuration.ExportPath))
            {
                // Attempt to open an existing collection.db file for merging
                return CollectionDb.Open(Configuration.ExportPath, Configuration.MergeCaseInsensitive);
            } else
            {
                // Create a clean new collection db for export
                return new CollectionDb();
            }
        }

        /// <summary>
        /// Perform a merge of any existing data in the CollectionDb object with the selected beatmaps in osu!lazer collections
        /// </summary>
        private IEnumerable<CollectionMergeStep> MergeCollections(CollectionDb collectionDb)
        {
            foreach (var (name, maps) in Collections)
            {
                // With each lazer collection, apply beatmap filters again for this specific export as selected beatmap diffs are not cached
                var collMaps = maps.Beatmaps
                    .Where(b => Configuration.Filters.All(f => f.Includes(b)))
                    .Select(b => b.MD5Hash)
                    .ToList();

                collectionDb.MergeCollection(name, collMaps);
                yield return new CollectionMergeStep(name, maps.Beatmaps.Count, collMaps.Count);
            }
        }

        /// <summary>
        /// Perform export of the 'selected' beatmaps which also belong to osu!lazer collections into an osu! stable collection.db file.
        /// </summary>
        public List<CollectionMergeStep> ExportCollectionDb()
        {
            // Open or create collection.db file
            var collectionDb = OpenCollectionDb();

            // Merge collections with selected beatmaps
            var mergeSteps = MergeCollections(collectionDb).ToList();

            // Perform actual collection.db file export
            collectionDb.ExportFile(Configuration.ExportPath);

            return mergeSteps;
        }

        private readonly Regex idCollection = new("#([0-9]+)", RegexOptions.Compiled);

        /// <summary>
        /// Update the set of 'selected' beatmaps by applying all filters from this exporter's Configuration.Filters.
        /// </summary>
        /// <param name="collectionFailure">An optional callback to notify users on a collection filter mismatch.</param>
        public void UpdateSelectedBeatmaps(Action<string>? collectionFailure = null)
        {
            List<string> collFilters = new();
            List<BeatmapFilter> beatmapFilters = new();
            bool negateColl = false;
            foreach (var filter in Configuration.Filters)
            {
                // Validate collection filter requests
                if (filter.Collections is not null)
                {
                    List<string> filteredCollections = new();
                    foreach (var requestedFilter in filter.Collections)
                    {
                        string? targetCollection = null;
                        var match = idCollection.Match(requestedFilter);
                        if (match.Success)
                        {
                            // Find any collection filters that are requested by index
                            var collectionId = int.Parse(match.Groups[1].Value);
                            targetCollection = Collections.FirstOrDefault(c => c.Value.CollectionID == collectionId).Key;
                        }
                        else
                        {
                            var exists = Collections.ContainsKey(requestedFilter);
                            if (exists)
                            {
                                targetCollection = requestedFilter;
                            }
                        }
                        if(targetCollection != null)
                        {
                            filteredCollections.Add(targetCollection);
                        }
                        else
                        {
                            collectionFailure?.Invoke(requestedFilter);
                        }
                    }
                    collFilters.AddRange(filteredCollections);
                    negateColl = filter.Negated;
                }

                else // this filter is not a collection filter
                    beatmapFilters.Add(filter);
            }

            // re-build 'collection' filters to optimize/cache iteration of beatmaps/collections for these
            if (collFilters.Count > 0)
            {
                // build list of beatmap ids from selected filters
                var includedHashes = Collections
                    .Where(c => collFilters.Any(c => c == "-all") switch
                    {
                        true => true,
                        false => collFilters.Any(filter => string.Equals(filter, c.Key, StringComparison.OrdinalIgnoreCase))
                    })
                    .SelectMany(c => c.Value.Beatmaps.Select(b => b.ID))
                    .ToList();

                string desc = string.Join(", ", collFilters);
                BeatmapFilter collFilter = new(desc, negateColl,
                    b => includedHashes.Contains(b.ID),
                    FilterTemplate.Collections);

                // with placeholder collection filters removed, add re-built filter 
                beatmapFilters.Add(collFilter);
            }

            // Apply rebuilt collection filter
            Configuration.Filters = new(beatmapFilters);
            Configuration.SaveFilters();

            // compute and cache 'selected' beatmaps based on current filters
            int selectedCount = 0;
            int selectedSetCount = 0;
            List<BeatmapSet> selectedSets = new();
            foreach (var set in AllBeatmapSets)
            {
                var filteredMaps = set.Beatmaps
                    .Where(map =>
                    {
                        if (Configuration.CombineFilterMode)
                        {
                            // Combine filter mode enabled, AND logic
                            return Configuration.Filters.All(f => f.Includes(map));
                        }
                        else
                        {
                            // OR logic
                            return Configuration.Filters.Any(f => f.Includes(map)); // OR logic
                        }
                    });

                var selected = filteredMaps.ToList();

                set.SelectedBeatmaps = selected;
                selectedCount += selected.Count;

                // after filtering beatmaps, "selected beatmap sets" will only include sets that STILL have at least 1 beatmap 
                if (selected.Count > 0)
                {
                    selectedSetCount++;
                    selectedSets.Add(set);
                }
            }

            SelectedBeatmapSets = selectedSets;
            SelectedBeatmapSetCount = selectedSetCount;
            SelectedBeatmapCount = selectedCount;
        }
    }
}
