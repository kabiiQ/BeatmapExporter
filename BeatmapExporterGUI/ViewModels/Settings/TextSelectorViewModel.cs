namespace BeatmapExporterGUI.ViewModels.Settings
{
    /// <summary>
    /// The TextSelector is a ValueSelector that offers no specific suggestions, just a simple text input field.
    /// </summary>
    public class TextSelectorViewModel : ValueSelectorViewModel
    {
        private string input;

        public TextSelectorViewModel(NewFilterViewModel newFilter) : base(newFilter)
        {
            input = string.Empty;
        }

        /// <summary>
        /// The current user (text) input. On update, attempts to construct a new BeatmapFilter, displaying the error to the user if not valid.
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
