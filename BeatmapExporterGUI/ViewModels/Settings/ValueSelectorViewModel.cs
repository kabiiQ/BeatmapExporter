namespace BeatmapExporterGUI.ViewModels.Settings
{
    public class ValueSelectorViewModel : ViewModelBase
    {
        public ValueSelectorViewModel(NewFilterViewModel newFilter)
        {
            Parent = newFilter;
        }

        protected NewFilterViewModel Parent { get; }
    }
}
