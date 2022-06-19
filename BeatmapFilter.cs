using LazerExporter.OsuDB.Schema;

namespace LazerExporter
{
    public class BeatmapFilter
    {
        public delegate bool Filter(Beatmap beatmap);

        public string Description { get; }
        Filter filter;
        bool negated;
        public BeatmapFilter(string description, bool negated, Filter filter)
        {
            this.Description = description;
            this.negated = negated;
            this.filter = filter;
        }

        public bool Includes(Beatmap beatmap) => negated switch
        {
            true => !filter(beatmap),
            false => filter(beatmap)
        };
    }

    public class FilterParser
    {
        bool negate;
        string input;
        string[] args;

        public FilterParser(string input)
        {
            bool negate = false;
            // parse filter negation 
            if(input[0] == '!')
            {
                negate = true;
                input = input.Substring(1);
            }

            this.negate = negate;
            this.input = input;
            this.args = input.Split(" ");
        }

        public BeatmapFilter? Parse()
        {
            // input is already lowercased
            if(args.Length < 2)
            {
                Console.WriteLine("Please include both the filter name and filter conditions.");
                return null;
            }
            return args[0] switch
            {
                "stars" => StarsFilter(),
                "length" => LengthFilter(),
                "author" => AuthorFilter(),
                "id" => IDFilter(),
                "bpm" => BPMFilter(),
                "artist" => ArtistFilter(),
                "tag" => TagFilter(),
                "mode" => GamemodeFilter(),
                _ => null
            };
        }

        string[] CommaSeparatedArg(int commandLength) => input.Substring(commandLength + 1 /* include space */).Split(",").Select(s => s.Trim()).ToArray();

        BeatmapFilter? StarsFilter()
        {
            // stars 6.3
            float starRating;
            if (!float.TryParse(args[1], out starRating))
            {
                Console.WriteLine($"Invalid star rating: {args[1]}");
                return null;
            }
            return new BeatmapFilter(input, negate, b => b.StarRating >= starRating);
        }

        BeatmapFilter? LengthFilter()
        {
            // length 90
            int duration;
            if (!Int32.TryParse(args[1], out duration))
            {
                Console.WriteLine($"Invalid map duration: {args[1]}");
                return null;
            }
            int millis = duration * 1000;
            return new BeatmapFilter(input, negate, b => b.Length >= millis);
        }

        BeatmapFilter? AuthorFilter()
        {
            // author RLC, Nathan
            string[] authors = CommaSeparatedArg(6);
            return new BeatmapFilter(input, negate, b => authors.Contains(b.Metadata.Author.Username.ToLower()));
        }

        BeatmapFilter? IDFilter()
        {
            // id 1, 2, 3
            var beatmapIds = CommaSeparatedArg(2).Select(a => Int32.Parse(a));
            return new BeatmapFilter(input, negate, b =>
            {
                var beatmapSet = b.BeatmapSet;
                return beatmapSet is not null && beatmapIds.Contains(beatmapSet.OnlineID);
            });
        }

        BeatmapFilter? BPMFilter()
        {
            // bpm 180
            int bpm;
            if (!Int32.TryParse(args[1], out bpm))
            {
                Console.WriteLine($"Invalid BPM: {args[1]}");
                return null;
            }
            return new BeatmapFilter(input, negate, b => b.BPM >= bpm);
        }

        BeatmapFilter? ArtistFilter()
        {
            // artist Camellia, Nanahira
            string[] artists = CommaSeparatedArg(6);
            return new BeatmapFilter(input, negate, b => artists.Contains(b.Metadata.Artist.ToLower()));
        }

        BeatmapFilter? TagFilter()
        {
            // tag touhou
            string[] tags = CommaSeparatedArg(3);
            return new BeatmapFilter(input, negate, b =>
            {
                string? beatmapTags = b.Metadata.Tags?.ToLower();
                return beatmapTags is not null && tags.Any(t => beatmapTags.Contains(t));
            });
        }

        BeatmapFilter? GamemodeFilter()
        {
            // mode osu/mania/ctb/taiko
            int? gamemodeId = args[1] switch
            {
                "osu" => 0,
                "taiko" => 1,
                "ctb" => 2,
                "mania" => 3,
                _ => null
            };
            if(gamemodeId is null)
            {
                Console.WriteLine($"Unknown osu! game mode: {args[1]}. Use osu, mania, ctb, or taiko.");
                return null;
            }
            return new BeatmapFilter(input, negate, b => b.Ruleset.OnlineID == gamemodeId);
        }
    }
}
