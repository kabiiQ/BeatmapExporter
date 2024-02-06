namespace BeatmapExporterGUI.ViewModels.Settings
{
    public class ValueSelectorViewModel : ViewModelBase
    {
        /// <summary>
        /// Parent class for all possible methods of allowing user input into a beatmap filter builder.
        /// </summary>
        public ValueSelectorViewModel(NewFilterViewModel newFilter)
        {
            Parent = newFilter;
        }

        protected NewFilterViewModel Parent { get; }
    }
}
