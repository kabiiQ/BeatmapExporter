using BeatmapExporterCore.Exporters.Lazer.LazerDB.Schema;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BeatmapExporterGUI.ViewModels.List
{
    /// <summary>
    /// Page for exploring the files within a single beatmap set. Currently displays the beatmap difficulties on top and all files (including difficulty files) on the bottom.
    /// Both diffs and files allow user-selection and singular export.
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

        /// <summary>
        /// The song and mapper name of the beatmap set this explorer represents.
        /// </summary>
        public string SetName { get; private set; } = string.Empty;

        #region Diff Display Settings
        /// <summary>
        /// List of all the displayed difficulties, may change with user selection change.
        /// </summary>
        [ObservableProperty]
        private List<Beatmap> _DisplayedDiffs;

        /// <summary>
        /// The currently selected display option, which is indexed 1:1 to the <see cref="BeatmapSorting.View" /> enum values.
        /// </summary>
        [ObservableProperty]
        private int _SelectedDisplayOption;

        /// <summary>
        /// The string representations for all supported display options. ex. Display all beatmaps
        /// </summary>
        public List<string> DisplayOptionNames => beatmapListView.DisplayOptions.Select(d => d.DiffName()).ToList();

        partial void OnSelectedDisplayOptionChanged(int value)
        {
            Task.Run(() => ApplyDisplaySetting());
        }

        /// <summary>
        /// Updates the displayed beatmap difficulties to match the current <see cref="SelectedDisplayOption" />
        /// </summary>
        /// <returns></returns>
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
        /// <summary>
        /// The string representations for the difficulties listed within this beatmap set, indexed 1:1 to <see cref="DisplayedDiffs" />
        /// </summary>
        [ObservableProperty]
        private List<string> _DiffNames;

        /// <summary>
        /// The index of the currently user-selected difficulty, indexed to both <see cref="DisplayedDiffs" /> and <see cref="DiffNames" />
        /// </summary>
        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(ExportSelectedDifficultyCommand))]
        private int _SelectedDiffIndex;

        /// <summary>
        /// The index of the currently user-selected file, indexed to both <see cref="FileNames" /> and <see cref="BeatmapSet.Files" />
        /// </summary>
        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(ExportSelectedFileCommand))]
        private int _SelectedFileIndex;

        /// <summary>
        /// The string representations for the files listed within this beatmap set. 
        /// </summary>
        public List<string> FileNames { get; private set; }
        #endregion

        #region Selection Export
        /// <summary>
        /// If a beatmap difficulty is currently selected by the user.
        /// </summary>
        public bool CanExportDiff => SelectedDiffIndex != -1;

        /// <summary>
        /// If a beatmap file is currently selected by the user.
        /// </summary>
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
                Exporter.AddSystemMessage($"Failed to export single file {filename} :: {e.Message}", error: true);
            }
        });
        #endregion
    }
}
