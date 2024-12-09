using Realms;

namespace BeatmapExporterCore.Exporters.Lazer.LazerDB.Schema
{
    // Original source file (modified by kabii) Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
    public class Beatmap : RealmObject
    {
        [PrimaryKey]
        public Guid ID { get; set; } = Guid.NewGuid();

        public string DifficultyName { get; set; } = string.Empty;

        public Ruleset Ruleset { get; set; } = null!;

        public BeatmapDifficulty Difficulty { get; set; } = null!;

        public BeatmapMetadata Metadata { get; set; } = null!;
        
        [Backlink(nameof(Score.BeatmapInfo))]
        public IQueryable<Score> Scores { get; } = null!;

        public BeatmapUserSettings UserSettings { get; set; } = null!;

        public BeatmapSet? BeatmapSet { get; set; }

        public int Status { get; set; }

        [Indexed]
        public int OnlineID { get; set; } = -1;

        public double Length { get; set; }

        public double BPM { get; set; }

        public string Hash { get; set; } = string.Empty;

        public double StarRating { get; set; } = -1;

        [Indexed]
        public string MD5Hash { get; set; } = string.Empty;

        public string OnlineMD5Hash { get; set; } = string.Empty;

        public DateTimeOffset? LastLocalUpdate { get; set; }

        public DateTimeOffset? LastOnlineUpdate { get; set; }

        public bool Hidden { get; set; }

        public DateTimeOffset? LastPlayed { get; set; }

        public int BeatDivisor { get; set; }

        public double? EditorTimestamp { get; set; }

        // Author kabii
        /// <summary>
        /// Assesses Beatmap equality based on the beatmap UUID
        /// </summary>
        public override bool Equals(object? obj)
        {
            if (obj == null || !GetType().Equals(obj.GetType()))
            {
                return false;
            }
            else
            {
                Beatmap map = (Beatmap)obj;
                return ID == map.ID;
            }
        }

        /// <summary>
        /// A string which describes this beatmap's difficulty details in two lines
        /// </summary>
        /// <returns></returns>
        public string Details()
        {
            int lengthSeconds = (int)Length / 1000;
            var diffDetail = $"{lengthSeconds} seconds - {BPM:0}BPM AR{Difficulty.ApproachRate} CS{Difficulty.CircleSize} HP{Difficulty.DrainRate} OD{Difficulty.OverallDifficulty}";
            return $"{Ruleset.ShortName}: {StarRating:0.00} stars by {Metadata.Author.Username} [{DifficultyName}]\n{diffDetail}";
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(base.GetHashCode(), ID);
        }
    }
}
