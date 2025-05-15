using BeatmapExporterCore.Utilities;
using System.Reflection;

namespace BeatmapExporterCore.Filters
{
    /// <summary>
    /// Contains reference to all pre-defined filter types.
    /// </summary>
    public static class FilterTypes
    {
        static FilterTypes()
        {
            AllTypes = typeof(FilterTemplate)
                .GetFields(BindingFlags.Public | BindingFlags.Static)
                .Where(p => p.FieldType == typeof(FilterTemplate))
                .Select(p => (FilterTemplate)p.GetValue(null)!)
                .ToList();
        }

        /// <summary>
        /// Generated list of all filter types as pre-defined within this class.
        /// The list is built and stored such that the indicies can be relied on at least throughout the lifetime of the application.
        /// </summary>
        public static List<FilterTemplate> AllTypes { get; }
    }

    /// <summary>
    /// Describes a pre-defined type of filter that is available to be selected (and further configured) by the user.
    /// </summary>
    public class FilterTemplate
    {
        /// <summary>
        /// Delegate function generating a BeatmapFilter for a type of filter from a user input string and the filter negation state at the time of the input.
        /// </summary>
        public delegate BeatmapFilter FilterConstructor(string userInput, bool negate);

        /// <summary>
        /// The type of data that this filter template will collect from the user. 
        /// May be used to determine if there is a superior interface available than text input.
        /// </summary>
        public enum Input { RawText, Played, Gamemode, Status, Collection };

        private FilterTemplate(string shortName, string fullName, string normalInput, string negatedInput, string detail, Input inputType, FilterConstructor constructor)
        {
            ShortName = shortName;
            FullName = fullName;
            NormalInputDescriptor = normalInput;
            NegatedInputDescriptor = negatedInput;
            FilterDetail = detail;
            InputType = inputType;
            Constructor = constructor;
        }

        /// <summary>
        /// Filter name which may be used for finding filter types from user input.
        /// </summary>
        public string ShortName { get; }

        /// <summary>
        /// Full filter name, suitable for use in user-viewed outputs.
        /// </summary>
        public string FullName { get; }

        /// <summary>
        /// Defines the type of input the user should be providing for this filter, ex. 'maximum'. 
        /// </summary>
        public string NormalInputDescriptor { get; }

        /// <summary>
        /// Defines the type of input the user should provide if this filter is negated, ex. 'minimum'.
        /// </summary>
        public string NegatedInputDescriptor { get; }

        /// <summary>
        /// A detailed description of the filter's functionality, suitable for display to the user directly.
        /// </summary>
        public string FilterDetail { get; }

        /// <summary>
        /// The Input type needed from the user to create a filter of this type. Use RawText if not creating a more precise input type.
        /// This is additional info useful for creating a better interface, may be ignored (ex. CLI).
        /// </summary>
        public Input InputType { get; }

        /// <summary>
        /// Function which constructs a filter of this type from a user input.
        /// </summary>
        public FilterConstructor Constructor { get; }

        /// <summary>
        /// A string briefly defining the type of input the user should provide, using the full filter name and appropriate input descriptor.
        /// </summary>
        public string InputDescription(bool negated) => $"{FullName} {(negated ? NegatedInputDescriptor : NormalInputDescriptor)}";

        // All BeatmapExporter-supported filters are defined below.

        #region Filters
        public static FilterTemplate StarRating = new(
            "stars",
            "Beatmap star rating/difficulty",
            "minimum",
            "maximum",
            "Selects beatmaps by their in-game star rating.\nFor example, input '6.3' to only export beatmaps 6.3 stars or harder, or negate this filter to only export beatmaps 6.3 stars or easier.",
            Input.RawText,
            (input, negate) =>
            {
                // 6.3
                if (!float.TryParse(input, out float starRating))
                    throw new ArgumentException($"Invalid star rating");

                return new(input, negate,
                    b => b.StarRating >= starRating,
                    StarRating!);
            });

        public static FilterTemplate Length = new(
            "length",
            "Song length (seconds)",
            "longer than",
            "shorter than",
            "Selects beatmaps which are longer/shorter than a given number of seconds.\nFor example, input '90' to only export beatmaps 90 seconds or more, or negate this filter to only export beatmaps 90 seconds or less.",
            Input.RawText,
            (input, negate) =>
            {
                // 90
                if (!int.TryParse(input, out int duration))
                    throw new ArgumentException("Invalid map length/duration");
                int millis = duration * 1000;

                return new(input, negate,
                    b => b.Length >= millis,
                    Length!);
            });

        public static FilterTemplate Author = new(
            "author",
            "Beatmap author",
            "is",
            "is NOT",
            "Selects beatmaps by matching the beatmap creator.\nIf multiple mappers are desired, separate author names with a comma (,).\nFor example, 'RLC, Nathan' would export beatmaps by either beatmap creator.\nNegating this filter would exclude specific beatmap authors from export.",
            Input.RawText,
            (input, negate) =>
            {
                // RLC, Nathan
                string[] authors = input.ToLower().CommaSeparatedArg();

                return new(input, negate,
                    b => authors.Contains(b.Metadata.Author.Username.ToLower()),
                    Author!);
            });

        public static FilterTemplate Id = new(
            "id",
            "Beatmap set ID",
            "is",
            "is NOT",
            "Selects beatmaps with a specific beatmap ID.\nIf you want to match multiple IDs, seperate IDs with a comma (,).",
            Input.RawText,
            (input, negate) =>
            {
                // 1, 2, 3
                var beatmapIds = input.CommaSeparatedArg().Select(int.Parse);

                return new(input, negate,
                    b =>
                    {
                        var beatmapSet = b.BeatmapSet;
                        return beatmapSet != null && beatmapIds.Contains(beatmapSet.OnlineID);
                    },
                    Id!);
            });

        public static FilterTemplate BPM = new(
            "bpm",
            "Song BPM",
            "minimum",
            "maximum",
            "Selects beatmaps with songs above/below a given BPM.\nFor example, input '180' to only export beatmaps 180 BPM or above, or negate the filter to only export beatmaps 180 BPM or below.",
            Input.RawText,
            (input, negate) =>
            {
                // 180
                if (!int.TryParse(input, out int bpm))
                    throw new ArgumentException("Invalid BPM");

                return new(input, negate,
                    b => b.BPM >= bpm,
                    BPM!);
            });

        public static FilterTemplate AddedSince = new(
            "since",
            "Beatmap Set added (date)",
            "in the last",
            "before",
            "Selects beatmap sets using the time since they were added to your osu!lazer. This is only as accurate as the date that osu! has recorded.\nFor example, input '8:00' to only export beatmaps added within the last 8 hours." +
            "\nInput '4' to only export beatmaps added within the last 4 days, or negate this filter to only export beatmaps added more than 4 days ago.",
            Input.RawText,
            (input, negate) =>
            {
                // 2:00
                if (!TimeSpan.TryParse(input, out TimeSpan since))
                    throw new ArgumentException("Invalid time interval");

                return new(input, negate,
                    b =>
                    {
                        if (b.BeatmapSet is null) return false; // if map doesn't have a set at all, we can't get the date added
                        var lifetime = DateTime.Now - b.BeatmapSet.DateAdded;
                        return lifetime < since;
                    },
                    AddedSince!);
            });

        public static FilterTemplate RankedSince = new(
            "ranked",
            "Beatmap Set ranked (date)",
            "in the last",
            "before",
            "Selects beatmap sets using the time since they were ranked.\nFor example, input '30' to only export beatmaps ranked in the last 30 days, or negate this filter to export beatmaps ranked more than 30 days ago",
            Input.RawText,
            (input, negate) =>
            {
                // 30
                if (!TimeSpan.TryParse(input, out TimeSpan since))
                    throw new ArgumentException("Invalid time interval");

                return new(input, negate,
                    b =>
                    {
                        var ranked = b.BeatmapSet?.DateRanked;
                        if (ranked == null) return false; // beatmap is not ranked
                        return DateTime.Now - ranked < since;
                    },
                    RankedSince!);
            });

        public static FilterTemplate Artist = new(
            "artist",
            "Song artist",
            "is",
            "is NOT",
            "Selects beatmaps by matching the song artist (as tagged within osu!).\nIf multiple artists are desired, separate artist names with a comma (,).\nFor example, 'Camellia, Nanahira' would export beatmaps by either artist.\nNegating this filter would exclude specific artists from export.",
            Input.RawText,
            (input, negate) =>
            {
                // Camellia, Nanahira
                string[] artists = input.ToLower().CommaSeparatedArg();

                return new(input, negate,
                    b => artists.Contains(b.Metadata.Artist.ToLower()),
                    Artist!);
            });

        public static FilterTemplate Tag = new(
            "tag",
            "Beatmap tags",
            "contain",
            "does NOT contain",
            "Selects beatmaps which have specific tags (as assigned by the beatmap author).\nIf multiple tags are desired, separate tags wth a comma (,).\nFor example, inputting 'touhou' would only export beatmaps with the tag 'touhou'.\nNegating this filter would exclude specific tags from export.",
            Input.RawText,
            (input, negate) =>
            {
                // touhou
                string[] tags = input.ToLower().CommaSeparatedArg();

                return new(input, negate,
                    b =>
                    {
                        string? beatmapTags = b.Metadata.Tags?.ToLower();
                        return beatmapTags != null && tags.Any(t => beatmapTags.Contains(t));
                    },
                    Tag!);
            });

        public static FilterTemplate Gamemode = new(
            "gamemode",
            "Beatmap gamemode",
            "is",
            "is NOT",
            "Selects beatmaps which are created for a specific gamemode.\nNegating this filter would exclude those gamemode beatmaps from export.",
            Input.Gamemode,
            (input, negate) =>
            {
                // osu/mania/ctb/taiko
                int? gamemodeId = input.ToLower() switch
                {
                    "osu" => 0,
                    "taiko" => 1,
                    "ctb" => 2,
                    "mania" => 3,
                    _ => null
                };
                if (gamemodeId == null)
                    throw new ArgumentException($"Unknown osu! game mode. Use osu, mania, ctb, or taiko.");

                return new(input, negate,
                    b => b.Ruleset.OnlineID == gamemodeId,
                    Gamemode!);
            });

        public static FilterTemplate OnlineStatus = new(
            "status",
            "Beatmap status (online status)",
            "is",
            "is NOT",
            "Selects beatmaps with a specific osu! online status.\nNegating this filter would exclude maps with that status from export.",
            Input.Status,
            (input, negate) =>
            {
                // graveyard/leaderboard/ranked/approved/qualified/loved
                int[]? statusId = input.ToLower() switch
                {
                    var s when s.StartsWith("graveyard") || s == "unknown" => new[] { -3 },
                    var s when s.StartsWith("leaderboard") => new[] { 1, 2, 3, 4 },
                    var s when s.StartsWith("rank") => new[] { 1 },
                    var s when s.StartsWith("approve") => new[] { 2 },
                    var s when s.StartsWith("qualif") => new[] { 3 },
                    var s when s.StartsWith("love") => new[] { 4 },
                    _ => null
                };
                if (statusId == null)
                    throw new ArgumentException("Unknown beatmap status. Use graveyard, leaderboard, ranked, approved, qualified, or loved.");

                return new(input, negate,
                    b => statusId.Contains(b.Status),
                    OnlineStatus!);
            });

        public static FilterTemplate PlayedSince = new(
            "played",
            "Beatmap played (date)",
            "in the last",
            "not since",
            "Selected beatmaps using the time that you last played them.\nFor example, input 12:00 to only export beatmaps that you played within the last 12 hours."
            + "\nInput '30' to only export beatmaps you have played within the last 30 days, or negate this filter to export all beatmaps which you have not played in the last 30 days.\nUse the \"Beatmap played ever\" filter instead if you want to select all played/unplayed beatmaps.",
            Input.RawText,
            (input, negate) =>
            {
                // 12:00 
                if (!TimeSpan.TryParse(input, out TimeSpan since))
                    throw new ArgumentException("Invalid time interval");

                return new(input, negate,
                    b =>
                    {
                        if (b.LastPlayed == null) return false; // beatmap has never been played
                        var playedAgo = DateTime.Now - b.LastPlayed;
                        return playedAgo < since;
                    },
                    PlayedSince!);
            });

        public static FilterTemplate Played = new(
            "everplayed",
            "Beatmap played ever",
            "",
            "(negated)",
            "Selects beatmaps that have been played at least once (yes), or only unplayed beatmaps (no)."
            + "\nThe \"yes, with all diffs\" option will export ALL difficulties of a beatmap set if any difficulty has been played.",
            Input.Played,
            (input, negate) =>
            {
                var lower = input.ToLower();
                // Override flag - exports all difficulties if any were played
                if (lower.Contains("set") || lower.Contains("diff"))
                {
                    return new(input, negate,
                        b =>
                        {
                            var beatmapSet = b.BeatmapSet;
                            return beatmapSet != null && beatmapSet.Beatmaps.Any(b => b.LastPlayed != null);
                        },
                        Played!);
                }
                else
                {
                    bool? played = lower switch
                    {
                        "yes" => true,
                        "no" => false,
                        _ => null
                    };
                    if (played == null)
                        throw new ArgumentException("Invalid beatmap played selection. Use \"yes\" for played beatmaps, or \"no\" for unplayed beatmaps.");

                    return new(input, negate,
                        b => (b.LastPlayed != null) == played,
                        Played!);
                }
            });

        public static FilterTemplate Collections = new(
            "collection",
            "Collection",
            "",
            ", NOT in",
            "Selects beatmaps contained within a specific collection.",
            Input.Collection,
            (input, negate) =>
            {
                // name1, name2
                string[] collections = input.CommaSeparatedArg();
                // builds a placeholder filter that will be re-built if/where the user's collections are available
                return new(input, negate, collections);
            });
        #endregion
    }
}
