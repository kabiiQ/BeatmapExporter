using BeatmapExporter.Exporters.Lazer.LazerDB.Schema;

namespace BeatmapExporterCore.Filters
{
    /// <summary>
    /// Represents a single active beatmap filter, as configured by the user.
    /// </summary>
    public class BeatmapFilter
    {
        /// <summary>
        /// A beatmap selection filter predicate, before any negation or editing.
        /// This component of a BeatmapFilter checks lazer beatmaps specifically.
        /// </summary>
        /// <param name="beatmap">A lazer beatmap that will be checked for inclusion by this filter.</param>
        /// <returns>If this filter includes the specific beatmap.</returns>
        public delegate bool LazerFilter(Beatmap beatmap);

        readonly LazerFilter lazerFilter;

        /// <summary>
        /// Creates a standard beatmap filter.
        /// </summary>
        public BeatmapFilter(string input, bool negated, LazerFilter lazerFilter, FilterTemplate template)
        {
            Input = input;
            Negated = negated;
            this.lazerFilter = lazerFilter;
            Collections = null;
            Template = template;
        }

        /// <summary>
        /// Creates a placeholder filter for collections that will be re-built if collections are available.
        /// </summary>
        public BeatmapFilter(string input, bool negated, string[] collections) : this(input, negated, b => true, FilterTemplate.Collections)
        {
            Collections = collections;
        }

        /// <summary>
        /// The original user input, used as an argument for this filter.
        /// </summary>
        public string Input { get; }

        /// <summary>
        /// A list of collection names iff this is a placeholder collection filter.
        /// </summary>
        public string[]? Collections { get; }

        /// <summary>
        /// If this filter should be negated, as selected by the user.
        /// </summary>
        public bool Negated { get; }

        /// <summary>
        /// The FilterTemplate this BeatmapFilter orignates from. 
        /// </summary>
        public FilterTemplate Template { get; }

        /// <summary>
        /// A description for this filter, possibly containing the user input.
        /// </summary>
        public string Description => $"{Template.InputDescription(Negated)}: {Input}";

        /// <summary>
        /// Determines if this filter contains a specific beatmap.
        /// </summary>
        public bool Includes(Beatmap beatmap) => Negated switch
        {
            true => !lazerFilter(beatmap),
            false => lazerFilter(beatmap)
        };
    }
}
