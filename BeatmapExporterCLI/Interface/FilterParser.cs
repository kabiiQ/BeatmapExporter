using BeatmapExporterCore.Filters;

namespace BeatmapExporterCLI.Interface
{
    /// <summary>
    /// FilterParser converts user CLI input into BeatmapFilter objects
    /// </summary>
    public class FilterParser
    {
        readonly bool negate;
        readonly string input;
        readonly string[] args;

        public FilterParser(string input)
        {
            bool negate = false;
            // parse filter negation 
            if (input[0] == '!')
            {
                negate = true;
                input = input[1..];
            }

            this.negate = negate;
            this.input = input;
            this.args = input.Split(" ");
        }

        public BeatmapFilter? Parse()
        {
            // input is already lowercased
            if (args.Length < 2)
            {
                Console.WriteLine("Please include both the filter name and filter conditions.");
                return null;
            }
            // Look for matching command using filter short names
            var template = FilterTypes.AllTypes.FirstOrDefault(t => t.ShortName == args[0]);
            // Remove command from input and construct BeatmapFilter 
            var noCommand = string.Join(" ", args[1..]);
            return template?.Constructor(noCommand, negate);
        }
    }
}
