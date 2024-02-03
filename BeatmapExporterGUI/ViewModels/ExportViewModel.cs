using BeatmapExporter.Exporters.Lazer;
using BeatmapExporter.Exporters.Lazer.LazerDB.Schema;
using BeatmapExporterCore.Exporters;
using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace BeatmapExporterGUI.ViewModels
{
    /// <summary>
    /// Page following an export task throughout and displaying progress to the user
    /// </summary>
    public partial class ExportViewModel : ViewModelBase
    {
        private readonly LazerExporter lazer;
        private readonly OuterViewModel outer;

        public ExportViewModel(OuterViewModel outer)
        {
            this.lazer = Exporter.Lazer!;
            this.outer = outer;
            Exported = new();

            TaskTitle = $"Exporting {lazer.Configuration.ExportFormat.UnitName()}";
            TotalSetCount = lazer.SelectedBeatmapSetCount;
            Progress = 0;
            Description = string.Empty;
            ActiveExport = false;
        }

        /// <summary>
        /// Description of the general export operation, displayed to the user.
        /// </summary>
        public string TaskTitle { get; }

        /// <summary>
        /// Total number of beatmap sets selected for export.
        /// </summary>
        public int TotalSetCount { get; }

        /// <summary>
        /// The progress towards TotalSetCount. Only beatmap sets, does not account for how many files are in each set.
        /// </summary>
        [ObservableProperty]
        private int _Progress;

        /// <summary>
        /// Export operation status displayed to the user. 
        /// </summary>
        [ObservableProperty]
        private string _Description;

        /// <summary>
        /// Reference to the export cancel command to allow this page to cancel export started from the MenuRow export command
        /// </summary>
        public ICommand ExportCancelCommand => outer.MenuRow.ExportCancelCommand;

        /// <summary>
        /// A collection of <see cref="ExportOperation" /> structs each representing one attempted export by this operation
        /// </summary>
        public ObservableCollection<ExportOperation> Exported { get; }

        /// <summary>
        /// If there is an export currently active. Used to disable navigation away until export is complete/cancelled.
        /// </summary>
        public bool ActiveExport { get; private set; }

        private void AddExport(bool success, string description) => Exported.Insert(0, new(success, description));

        /// <summary>
        /// Identifies the requested export format and begins the export operation represented by this ExportView. 
        /// </summary>
        public async Task StartExport(CancellationToken token)
        {
            Func<CancellationToken, Task> operation = lazer.Configuration.ExportFormat switch
            {
                ExportFormat.Beatmap => ExportBeatmaps,
                ExportFormat.Audio => ExportAudioFiles,
                ExportFormat.Background => ExportBackgrounds
            };

            ActiveExport = true;
            await operation(token);
            ActiveExport = false;
        }

        /// <summary>
        /// Exports all selected beatmaps, updating the user viewable statuses with each attempted export.
        /// </summary>
        private async Task ExportBeatmaps(CancellationToken token)
        {
            lazer.SetupExport();
            int exported = 0;
            Description = $"{TotalSetCount} beatmap sets ({lazer.SelectedBeatmapCount} diffs) are selected for export.";
            foreach (var mapset in lazer.SelectedBeatmapSets)
            {
                if (token.IsCancellationRequested)
                    break;
                string? filename = null;
                try
                {
                    await Exporter.RealmScheduler.Schedule(() => lazer.ExportBeatmap(mapset, out filename));
                    exported++;
                    AddExport(true, $"Exported beatmap set ({Progress + 1}/{TotalSetCount}): {filename}");
                } catch (Exception e)
                {
                    AddExport(false, $"Unable to export {filename} :: {e.Message}");
                }
                Progress++;
            }
            var status = $"Exported {exported}/{TotalSetCount} beatmaps to {lazer.Configuration.FullPath}.";
            Exporter.AddSystemMessage(status);
            Description = status;
        }

        private record struct ExportProgress(int Discovered, int Success);

        /// <summary>
        /// Exports audio files from all selected beatmaps, updating the export description with the export status.
        /// </summary>
        private async Task ExportAudioFiles(CancellationToken token)
        {
            lazer.SetupExport();
            var transcodeInfo = lazer.TranscodeAvailable ? "This operation will take longer if many selected beatmaps are not in .mp3 format."
                : "FFmpeg runtime not found. Beatmaps that use other audio formats than .mp3 will be skipped.\nMake sure ffmpeg.exe is located on the system PATH or placed in the directory with this BeatmapExporter.exe to enable transcoding.";
            Description = $"Exporting audio from {TotalSetCount} beatmap sets as .mp3 files.\n{transcodeInfo}";

            int exportedAudio = 0, discovered = 0;
            foreach (var mapset in lazer.SelectedBeatmapSets)
            {
                if (token.IsCancellationRequested)
                    break;

                await Exporter.RealmScheduler.Schedule(async () =>
                {
                    var (stepDiscover, stepSuccess) = await ExportMapsetAudio(mapset, discovered);
                    // These states are used for UI info/progress, it seems better to update them async and be potentially desynced rather than waiting for slow exports just to maintain order
                    discovered += stepDiscover;
                    exportedAudio += stepSuccess;
                    Progress++;
                });
            }
            Description = $"Exported {exportedAudio}/{discovered} audio files from {TotalSetCount} beatmaps to {lazer.Configuration.FullPath}.";
        }

        /// <summary>
        /// Identifies selected beatmap's audio files and exports, updating the user viewable statuses with each attempted export.
        /// </summary>
        private async Task<ExportProgress> ExportMapsetAudio(BeatmapSet mapset, int totalDiscovered)
        {
            var allAudio = lazer.ExtractAudio(mapset);

            int exportedAudio = 0, discovered = 0;
            foreach (var audioExport in allAudio)
            {
                discovered++; // count of audio files discovered by this 
                totalDiscovered++; // count of all audio files discovered across any other calls
                string audioFile = audioExport.AudioFile.AudioFile;
                var transcode = audioExport.TranscodeFrom != null;
                var transcodeNotice = transcode ? $"(transcode required from {audioExport.TranscodeFrom})" : "";
                try
                {
                    if (transcode && !lazer.TranscodeAvailable)
                    {
                        AddExport(false, $"Non-mp3 audio {audioExport.OutputFilename} found and FFmpeg is not loaded, this audio will be skipped.");
                        continue;
                    }
                    AddExport(true, $"({totalDiscovered}/?) Exporting {audioExport.OutputFilename}{transcodeNotice}");

                    void metadataFailure(Exception e) => AddExport(false, $"Unable to set metadata for {audioExport.OutputFilename} :: {e.Message}. Exporting will continue.");
                    await lazer.ExportAudio(audioExport, metadataFailure);
                    exportedAudio++;
                }
                catch (TranscodeException te)
                {
                    AddExport(false, $"Unable to transcode audio: {audioFile}. An error occured :: {te.Message}");
                }
                catch (Exception e)
                {
                    AddExport(false, $"Unable to export audio: {audioFile} :: {e.Message}");
                }
            }
            return new(discovered, exportedAudio);
        }

        private async Task ExportBackgrounds(CancellationToken token)
        {
            lazer.SetupExport();
            Description = $"Exporting beatmap background images from {TotalSetCount} beatmap sets.";

            int discovered = 0, exportedBackgrounds = 0;
            foreach (var mapset in lazer.SelectedBeatmapSets)
            {
                if (token.IsCancellationRequested)
                    break;

                await Exporter.RealmScheduler.Schedule(() =>
                {
                    var (stepDiscover, stepSuccess) = ExportMapsetBackgrounds(mapset, discovered);
                    // These states are used for UI info/progress, it seems better to update them async and be potentially desynced rather than waiting for slow exports just to maintain order
                    discovered += stepDiscover;
                    exportedBackgrounds += stepSuccess;
                    Progress++;
                });
            }
            Description = $"Exported {exportedBackgrounds}/{discovered} background files from {TotalSetCount} beatmaps to {lazer.Configuration.FullPath}.";
        }

        private ExportProgress ExportMapsetBackgrounds(BeatmapSet mapset, int totalDiscovered)
        {
            var allImages = lazer.ExtractBackgrounds(mapset);

            int exportedBackgrounds = 0, discovered = 0;
            foreach (var imageExport in allImages)
            {
                discovered++;
                totalDiscovered++;
                var backgroundFile = imageExport.BackgroundFile.BackgroundFile;

                try
                {
                    lazer.ExportBackground(imageExport);
                    exportedBackgrounds++;
                    AddExport(true, $"({totalDiscovered}/?) Exported background image {imageExport.OutputFilename}.");
                }
                catch (Exception e)
                {
                    AddExport(false, $"Unable to export background image {backgroundFile} :: {e.Message}");
                }
            }
            return new(discovered, exportedBackgrounds);
        }
    }

    public record struct ExportOperation(bool Success, string Description)
    {
        public readonly string Color => Success ? "" : "Red";

        public readonly string Detail => Success ? Description : "(!)" + Description;
    }
}
