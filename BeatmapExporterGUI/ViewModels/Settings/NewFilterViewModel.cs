using Avalonia.Data;
using BeatmapExporter.Exporters;
using BeatmapExporterCore.Filters;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BeatmapExporterGUI.ViewModels.Settings
{
    /// <summary>
    /// Page component representing a beatmap filter 'builder' operation.
    /// </summary>
    public partial class NewFilterViewModel : ViewModelBase
    {
        private readonly ExportConfigViewModel exportView;
        private readonly ExporterConfiguration exporterConfig;

        public NewFilterViewModel(ExportConfigViewModel exportView)
        {
            this.exportView = exportView;
            this.exporterConfig = exportView.Exporter.Configuration!;

            AvailableFilterTypes = FilterTypes.AllTypes.Select(t => t.FullName);
        }

        #region Filter Type Selection
        /// <summary>
        /// The names of all current filter types, to be displayed in the interface.
        /// </summary>
        public IEnumerable<string> AvailableFilterTypes { get; }

        /// <summary>
        /// Index (in AvailableFilterTypes) of the currently selected type.
        /// </summary>
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(SelectedFilterType), nameof(FilterDescription), nameof(InputDescription), nameof(ValueSelector))]
        private int _SelectedFilterIndex;

        /// <summary>
        /// The currently selected filter type by the user, resolved from the selected drop-down index.
        /// </summary>
        public FilterTemplate SelectedFilterType => FilterTypes.AllTypes[SelectedFilterIndex];

        /// <summary>
        /// The filter details for the currently selected filter type.
        /// </summary>
        public string FilterDescription => SelectedFilterType.FilterDetail;
        #endregion

        #region Filter Argument Input
        /// <summary>
        /// The filter input helper for the currently selected filter type.
        /// </summary>
        public string InputDescription => $"{SelectedFilterType.InputDescription(Negate)}:";

        /// <summary>
        /// Identifies and instanciates the appropriate <see cref="ValueSelectorViewModel" /> for the selected beatmap filter type.
        /// This value will be directly displayed onto the user interface.
        /// </summary>
        public ValueSelectorViewModel ValueSelector => SelectedFilterType.InputType switch
        {
            FilterTemplate.Input.RawText => new TextSelectorViewModel(this),
            FilterTemplate.Input.Gamemode => new DropdownSelectorViewModel(this, new()
            {
                "osu", "mania", "ctb", "taiko"
            }),
            FilterTemplate.Input.Status => new DropdownSelectorViewModel(this, new()
            {
                "graveyard", "leaderboard", "ranked", "approved", "qualified", "loved"
            }),
            FilterTemplate.Input.Collection => new DropdownSelectorViewModel(this, Exporter.Lazer!.Collections.Keys.ToList())
        };

        /// <summary>
        /// A BeatmapFilter generated from the current user input, null if the user input is not valid.
        /// </summary>
        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(SaveFilterCommand))]
        private BeatmapFilter? _CurrentFilter;

        /// <summary>
        /// If the current user input generated a valid BeatmapFilter that can be applied to the beatmap selection.
        /// </summary>
        public bool FilterValid => CurrentFilter != null;

        /// <summary>
        /// Attempts to construct a BeatmapFilter for a given input, propagating ArgumentExceptions for invalid input. 
        /// Updates CurrentFilter on success.
        /// Throws <see cref="DataValidationException"/> on failure.
        /// </summary>
        public void ConstructFilter(string input)
        {
            try
            {
                CurrentFilter = SelectedFilterType.Constructor(input, Negate);
            } catch (ArgumentException ae)
            {
                CurrentFilter = null;
                throw new DataValidationException(ae.Message);
            }
        }

        /// <summary>
        /// The filter negation status selected by the user.
        /// </summary>
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(FilterDescription), nameof(InputDescription))]
        private bool _Negate;

        partial void OnNegateChanged(bool value)
        {
            if (CurrentFilter != null)
            {
                ConstructFilter(CurrentFilter.Input);
            }
        }
        #endregion

        #region Button Commands
        /// <summary>
        /// User input requesting current 'new filter' to be removed
        /// </summary>
        public void CancelCreation() => exportView.CancelFilterBuilder();

        /// <summary>
        /// User input requesting current 'new filter' to be applied as an active beatmap filter
        /// </summary>
        [RelayCommand(CanExecute = nameof(FilterValid))]
        public async Task SaveFilter() => await exportView.ApplyFilterBuilder(CurrentFilter!);
        #endregion
    }
}
