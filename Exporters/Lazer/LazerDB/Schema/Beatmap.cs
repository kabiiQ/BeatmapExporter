// Original source file (modified by kabii) Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.

using Newtonsoft.Json;
using Realms;

namespace BeatmapExporter.Exporters.Lazer.LazerDB.Schema
{
    public class Beatmap : RealmObject
    {
        [PrimaryKey]
        public Guid ID { get; set; } = Guid.NewGuid();

        public string DifficultyName { get; set; } = string.Empty;

        public Ruleset Ruleset { get; set; } = null!;

        public BeatmapDifficulty Difficulty { get; set; } = null!;

        public BeatmapMetadata Metadata { get; set; } = null!;
        
        [JsonIgnore]
        [Backlink(nameof(ScoreInfo.BeatmapInfo))]
        public IQueryable<ScoreInfo> Scores { get; } = null!;

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

        public double AudioLeadIn { get; set; }

        public float StackLeniency { get; set; } = 0.7f;

        public bool SpecialStyle { get; set; }

        public bool LetterboxInBreaks { get; set; }

        public bool WidescreenStoryboard { get; set; }

        public bool EpilepsyWarning { get; set; }

        public bool SamplesMatchPlaybackRate { get; set; }

        public DateTimeOffset? LastPlayed { get; set; }

        public double DistanceSpacing { get; set; }

        public int BeatDivisor { get; set; }

        public int GridSize { get; set; }

        public double TimelineZoom { get; set; }

        public double? EditorTimestamp { get; set; }

        public int CountdownOffset { get; set; }

        // Author kabii
        public override bool Equals(object obj)
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

        public override int GetHashCode()
        {
            return HashCode.Combine(base.GetHashCode(), ID);
        }
        
        //GetDisplayTitle() step in (from lazer's BeatmapInfoExtensions) to stay consistent with lazer's naming
        public string Display()
        {
            return $"{this.Metadata.Display()} {GetVersionString()}".Trim();
        }
        //GetVersionString() step in (from lazer's BeatmapInfoExtensions)
        private string GetVersionString() => string.IsNullOrEmpty(this.DifficultyName) ? string.Empty : $"[{this.DifficultyName}]";
    }
}
