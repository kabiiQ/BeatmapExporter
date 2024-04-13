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
    /// Page for listing all beatmaps. Currently displays all beatmap sets on one side and further explores a selected beatmap further on the other.
    /// </summary>
    public partial class BeatmapListViewModel : ViewModelBase
    {
        private bool hasExported = false; // if this list view has exported beatmaps already

        public BeatmapListViewModel()
        {
            hasExported = false;

            BeatmapSetList = new();
            _DisplayedBeatmapSets = new();

            SelectedSetIndex = -1;
            SelectedDisplayOption = 0;
            SelectedSortOption = 0;

            SortOptions = BeatmapSorting.AllSortOptions.ToList();
            DisplayOptions = BeatmapSorting.AllDisplayOptions.ToList();

            // Applies the initial display setting - loading and displaying beatmaps
            Task.Run(() => ApplyDisplaySetting());
        }

        #region Beatmap Listing/Selection
        /// <summary>
        /// ViewModel for the currently selected beatmap - right hand side of the user interface.
        /// </summary>
        [ObservableProperty]
        private BeatmapExplorerViewModel? _BeatmapExplorer;

        /// <summary>
        /// The currently selected beatmap set by the user.
        /// </summary>
        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(ExportSelectedBeatmapCommand))]
        private int _SelectedSetIndex;

        /// <summary>
        /// When the selected beatmap set is changed by the user, the right hand side "explorer" view is changed to that new beatmap set.
        /// </summary>
        partial void OnSelectedSetIndexChanged(int value)
        {
            if (value != -1)
            {
                var selectedSet = DisplayedBeatmapSets[value];
                BeatmapExplorer = new(this, selectedSet);
            }
        }

        /// <summary>
        /// A list of beatmap sets that should currently be dispalyed to the user
        /// </summary>
        [ObservableProperty]
        private List<BeatmapSet> _DisplayedBeatmapSets;

        partial void OnDisplayedBeatmapSetsChanged(List<BeatmapSet> value)
        {
            Task.Run(() => Exporter.RealmScheduler.Schedule(async () =>
            {
                await ApplySorting();
            }));
        }

        /// <summary>
        /// A list of displayable strings directly representing the current DisplayedBeatmapSet list 
        /// </summary>
        [ObservableProperty]
        private List<string> _BeatmapSetList;
        #endregion

        #region Beatmap Sorting/Viewing
        internal List<BeatmapSorting.SortBy> SortOptions { get; }

        internal List<BeatmapSorting.View> DisplayOptions { get; }

        public List<string> SortOptionNames => SortOptions.Select(s => s.FullName()).ToList();

        [ObservableProperty]
        private int _SelectedSortOption;

        partial void OnSelectedSortOptionChanged(int value)
        {
            Task.Run(() => ApplySorting());
        }

        private async Task ApplySorting() => await Exporter.RealmScheduler.Schedule(() =>
        {
            if (SelectedSortOption != -1)
            {
                var sortBy = (BeatmapSorting.SortBy)SelectedSortOption;

                // Build a stable sort by secondary sorting on beatmap UUID
                int stableComparer(BeatmapSet x, BeatmapSet y)
                {
                    var selectedSort = sortBy.Comparer()(x, y);
                    if (selectedSort == 0)
                    {
                        // requested sort has "equal" beatmap sets, perform secondary sort
                        return x.ID.CompareTo(y.ID);
                    }
                    return selectedSort;
                }

                DisplayedBeatmapSets.Sort(stableComparer);
                BeatmapSetList = DisplayedBeatmapSets.Select(set => set.DiffSummary()).ToList();
            }
        });

        public List<string> DisplayOptionNames => DisplayOptions.Select(d => d.SetName()).ToList();

        [ObservableProperty]
        private int _SelectedDisplayOption;

        partial void OnSelectedDisplayOptionChanged(int value)
        {
            Task.Run(() => ApplyDisplaySetting());
        }

        private async Task ApplyDisplaySetting() => await Exporter.RealmScheduler.Schedule(async () =>
        {
            if (SelectedDisplayOption == (int)BeatmapSorting.View.Selected)
            {
                DisplayedBeatmapSets = Exporter.Lazer!.SelectedBeatmapSets.ToList();
            }
            else
            {
                DisplayedBeatmapSets = Exporter.Lazer!.AllBeatmapSets.ToList();
            }
            await ApplySorting();
        });
        #endregion

        #region Single Beatmap Export
        /// <summary>
        /// Represents whether the directory should be opened for the user. Returns true for the first access only. 
        /// </summary>
        public bool ShouldOpenDirectory
        {
            get
            {
                if (hasExported)
                {
                    return false;
                }
                hasExported = true;
                return true;
            }
        }
        /// <summary>
        /// If a beatmap set is currently selected by the user.
        /// </summary>
        public bool CanExportBeatmapSet => SelectedSetIndex != -1;

        /// <summary>
        /// Export a single user-selected beatmap
        /// </summary>
        [RelayCommand(CanExecute = nameof(CanExportBeatmapSet))]
        public async Task ExportSelectedBeatmap()
        {
            var exportSet = DisplayedBeatmapSets[SelectedSetIndex];
            var lazer = Exporter.Lazer!;
            lazer.SetupExport(ShouldOpenDirectory);
            string? filename = null;
            try
            {
                await Exporter.RealmScheduler.Schedule(() => lazer.ExportBeatmap(exportSet, out filename));
                Exporter.AddSystemMessage($"Single beatmap exported: {lazer.Configuration.FullPath}/{filename}");
            }
            catch (Exception e)
            {
                Exporter.AddSystemMessage($"Failed to export single beatmap {filename} :: {e.Message}", error: true);
            }
        }

        /// <summary>
        /// User-requested input to manually open the export directory.
        /// </summary>
        public void OpenExportDirectory() => Exporter.Lazer!.SetupExport(openDir: true);
        #endregion
    }
}
