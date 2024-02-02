using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BeatmapExporterGUI.ViewModels.List
{
    public partial class CollectionListViewModel : ViewModelBase
    {
        public CollectionListViewModel()
        {
            SelectedCollection = null;
            CollectionList = Exporter.Lazer!.Collections.Select(coll => coll.Key).ToList();
        }

        public List<string> CollectionList { get; }

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

        [ObservableProperty]
        private List<string> _BeatmapDetails;
    }
}
