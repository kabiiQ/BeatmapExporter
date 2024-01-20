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
    /// Follows a single export task throughout
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
        }

        public string TaskTitle { get; }

        public int TotalSetCount { get; }

        /// <summary>
        /// The progress towards TotalSetCount. Only beatmap sets, does not account for how many files are in each set.
        /// </summary>
        [ObservableProperty]
        private int _Progress;

        [ObservableProperty]
        private string _Description;

        public ICommand ExportCancelCommand => outer.MenuRow.ExportCancelCommand;

        public ObservableCollection<ExportOperation> Exported { get; }

        private void AddExport(bool success, string description) => Exported.Insert(0, new(success, description));

        public async Task StartExport(CancellationToken token)
        {
            Func<CancellationToken, Task> operation = lazer.Configuration.ExportFormat switch
            {
                ExportFormat.Beatmap => ExportBeatmaps,
                ExportFormat.Audio => ExportAudioFiles,
                ExportFormat.Background => ExportBackgrounds
            };

            await operation(token);
        }

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

                await Exporter.RealmScheduler.Schedule(() => ExportMapsetAudio(mapset, ref exportedAudio, ref discovered));
            }
            Description = $"Exported {exportedAudio}/{discovered} audio files from {TotalSetCount} beatmaps to {lazer.Configuration.FullPath}.";
        }

        private void ExportMapsetAudio(BeatmapSet mapset, ref int exportedAudio, ref int discovered)
        {
            var allAudio = lazer.ExtractAudio(mapset);

            foreach (var audioExport in allAudio)
            {
                discovered++;
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
                    AddExport(true, $"({discovered}/?) Exporting {audioExport.OutputFilename}{transcodeNotice}");

                    void metadataFailure(Exception e) => AddExport(false, $"Unable to set metadata for {audioExport.OutputFilename} :: {e.Message}. Exporting will continue.");
                    lazer.ExportAudio(audioExport, metadataFailure);
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
            Progress++;
        }

        private async Task ExportBackgrounds(CancellationToken token)
        {
            lazer.SetupExport();
            Description = $"Exporting beatmap background images from {TotalSetCount} beatmap sets.";

            int discovered = 0, exported = 0;
            foreach (var mapset in lazer.SelectedBeatmapSets)
            {
                if (token.IsCancellationRequested)
                    break;

                var allImages = lazer.ExtractBackgrounds(mapset);

                foreach (var imageExport in allImages)
                {
                    discovered++;
                    var backgroundFile = imageExport.BackgroundFile.BackgroundFile;

                    try
                    {
                        await Exporter.RealmScheduler.Schedule(() => lazer.ExportBackground(imageExport));
                        exported++;
                        AddExport(true, $"({discovered}/?) Exported background image {imageExport.OutputFilename}.");
                    }
                    catch (Exception e)
                    {
                        AddExport(false, $"Unable to export background image {backgroundFile} :: {e.Message}");
                    }
                }
                Progress++;
            }
            Description = $"Exported {exported}/{discovered} background files from {TotalSetCount} beatmaps to {lazer.Configuration.FullPath}.";
        }
    }

    public record struct ExportOperation(bool Success, string Description)
    {
        public readonly string Color => Success ? "" : "Red";

        public readonly string Detail => Success ? Description : "(!)" + Description;
    }
}
