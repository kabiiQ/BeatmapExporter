using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BeatmapExporterGUI.ViewModels.List
{
    /// <summary>
    /// Page for listing all beatmap collections. Currently lists on the left half, allowing user selection for inner beatmap listing on the right side.
    /// </summary>
    public partial class CollectionListViewModel : ViewModelBase
    {
        public CollectionListViewModel()
        {
            SelectedCollection = null; // initially do not select any collection
            _BeatmapDetails = new();

            CollectionList = Exporter.Lazer!.Collections.Select(coll => coll.Key).ToList();
        }

        /// <summary>
        /// A list of the names of all known beatmap collections.
        /// </summary>
        public List<string> CollectionList { get; }

        /// <summary>
        /// The name of the currently user-selected beatmap collection.
        /// </summary>
        [ObservableProperty]
        private string? _SelectedCollection;

        partial void OnSelectedCollectionChanged(string? value)
        {
            if (value == null) return;
            var selectedColl = Exporter.Lazer!.Collections[value];
            Task.Run(() => Exporter.RealmScheduler.Schedule(() =>
            {
                BeatmapDetails = selectedColl.Beatmaps.Select(b => b.Details()).ToList();
            }));
        }

        /// <summary>
        /// List of strings containing user-displayable descriptions for all the beatmaps within the currently selected collection.
        /// </summary>
        [ObservableProperty]
        private List<string> _BeatmapDetails;
    }
}
