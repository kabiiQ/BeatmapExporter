using BeatmapExporter.Exporters.Lazer.LazerDB.Schema;

namespace BeatmapExporterCore.Exporters
{
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
        /// A description for this filter, possibly the exact user input.
        /// </summary>
        public string Description { get; }

        /// <summary>
        /// A list of collection names iff this is a placeholder collection filter.
        /// </summary>
        public string[]? Collections { get; }

        /// <summary>
        /// If this filter should be negated, as selected by the user.
        /// </summary>
        public bool Negated { get; }

        /// <summary>
        /// Creates a standard beatmap filter.
        /// </summary>
        public BeatmapFilter(string description, bool negated, LazerFilter lazerFilter)
        {
            Description = description;
            Negated = negated;
            this.lazerFilter = lazerFilter;
            Collections = null;
        }

        /// <summary>
        /// Creates a placeholder filter for collections that will be re-built if collections are available.
        /// </summary>
        public BeatmapFilter(string description, bool negated, string[] collections) : this(description, negated, b => true)
        {
            Collections = collections;
        }

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
