using BeatmapExporter.Exporters.Lazer.LazerDB.Schema;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BeatmapExporterGUI.ViewModels.List
{
    /// <summary>
    /// View for listing all beatmaps. Currently displays all beatmap sets on one side and further explores a selected beatmap further on the other.
    /// </summary>
    public partial class BeatmapExplorerViewModel : ViewModelBase
    {
        private readonly BeatmapListViewModel beatmapListView;
        private readonly BeatmapSet selectedSet;

        public BeatmapExplorerViewModel(BeatmapListViewModel parent, BeatmapSet set)
        {
            beatmapListView = parent;
            selectedSet = set;

            SelectedDisplayOption = parent.SelectedDisplayOption; // inherit all/selected option from parent list
            // Do not select files for export by default
            SelectedDiffIndex = -1;
            SelectedFileIndex = -1;

            _DisplayedDiffs = new();
            _DiffNames = new();
            FileNames = new();

            Task.Run(() => Exporter.RealmScheduler.Schedule(async () =>
            {
                var metadata = set.DiffMetadata;

                SetName = $"{metadata?.Title} ({metadata?.Author.Username})";
                OnPropertyChanged(nameof(SetName));

                FileNames = set.Files.Select(f => f.Filename).ToList();
                OnPropertyChanged(nameof(FileNames));

                await ApplyDisplaySetting();
            }));
        }

        public string SetName { get; private set; } = string.Empty;

        #region Diff Display Settings
        [ObservableProperty]
        private List<Beatmap> _DisplayedDiffs;

        [ObservableProperty]
        private int _SelectedDisplayOption;

        public List<string> DisplayOptionNames => beatmapListView.DisplayOptions.Select(d => d.DiffName()).ToList();

        partial void OnSelectedDisplayOptionChanged(int value)
        {
            Task.Run(() => ApplyDisplaySetting());
        }

        private async Task ApplyDisplaySetting() => await Exporter.RealmScheduler.Schedule(() =>
        {
            if (SelectedDisplayOption == (int)BeatmapSorting.View.Selected)
            {
                DisplayedDiffs = selectedSet.SelectedBeatmaps.ToList();
            }
            else
            {
                DisplayedDiffs = selectedSet.Beatmaps.ToList();
            }
            DiffNames = DisplayedDiffs.Select(d => d.Details()).ToList();
        });
        #endregion

        #region File Display/Selection
        [ObservableProperty]
        private List<string> _DiffNames;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(ExportSelectedDifficultyCommand))]
        private int _SelectedDiffIndex;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(ExportSelectedFileCommand))]
        private int _SelectedFileIndex;

        public List<string> FileNames { get; private set; }
        #endregion

        #region Selection Export
        public bool CanExportDiff => SelectedDiffIndex != -1;

        public bool CanExportFile => SelectedFileIndex != -1;

        /// <summary>
        /// Exports a single user-selected beatmap difficulty from within a beatmap set
        /// The exported set includes all beatmap files except any other difficulties 
        /// </summary>
        [RelayCommand(CanExecute = nameof(CanExportDiff))]
        private async Task ExportSelectedDifficulty() => await Exporter.RealmScheduler.Schedule(async () =>
        {
            var selectedDiff = DisplayedDiffs[SelectedDiffIndex];

            // Temporarly mark only UI-selected diff as 'selected' internally
            var initialSelection = selectedSet.SelectedBeatmaps.ToList();
            selectedSet.SelectedBeatmaps = new List<Beatmap>()
            {
                selectedDiff
            };

            try
            {
                await beatmapListView.ExportSelectedBeatmap();
            }
            finally
            {
                // restore initial difficulty selection
                selectedSet.SelectedBeatmaps = initialSelection;
            }
        });

        /// <summary>
        /// Exports a single user-selected file from within a beatmap set
        /// </summary>
        [RelayCommand(CanExecute = nameof(CanExportFile))]
        private async Task ExportSelectedFile() => await Exporter.RealmScheduler.Schedule(() =>
        {
            var lazer = Exporter.Lazer!;
            lazer.SetupExport(beatmapListView.ShouldOpenDirectory);

            string? filename = null;
            try
            {
                var selectedFile = selectedSet.Files[SelectedFileIndex];
                filename = selectedFile.Filename;
                lazer.ExportSingleFile(selectedSet, selectedFile);
                Exporter.AddSystemMessage($"Single file exported: {lazer.Configuration.FullPath}/{filename}");
            }
            catch (Exception e)
            {
                Exporter.AddSystemMessage($"Failed to export single file {filename} :: {e.Message}");
            }
        });
        #endregion
    }
}
