namespace BeatmapExporterGUI.ViewModels.Settings
{
    public class TextSelectorViewModel : ValueSelectorViewModel
    {
        private string input;

        public TextSelectorViewModel(NewFilterViewModel newFilter) : base(newFilter)
        {
            input = string.Empty;
        }

        /// <summary>
        /// The current user (text) input.
        /// </summary>
        public string Input
        {
            get => input;
            set
            {
                if (!string.IsNullOrWhiteSpace(value))
                {
                    Parent.ConstructFilter(value);
                }
                input = value;
            }
        }
    }
}
