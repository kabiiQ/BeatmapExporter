using BeatmapExporterCore.Utilities;
using Realms;

// Original schema source file Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
namespace BeatmapExporterCore.Exporters.Lazer.LazerDB.Schema
{
    /// <summary>
    /// A realm model containing metadata for a single score.
    /// </summary>
    public class Score : RealmObject
    {
        [PrimaryKey]
        public Guid ID { get; set; }
        public Beatmap? BeatmapInfo { get; set; }
        public string BeatmapHash { get; set; } = string.Empty;
        public IList<RealmNamedFileUsage> Files { get; } = null!;
        public double Accuracy { get; set; }
        public DateTimeOffset Date { get; set; }
        public RealmUser User { get; set; } = null!;
        public string Mods { get; set; } = string.Empty;
        public string Statistics { get; set; } = string.Empty;
        public IList<int> Pauses { get; } = null!;
        public int Rank { get; set; }

        // Author kabii
        /// <summary>
        /// Produces the output-friendly letter rank for this player score
        /// </summary>
        [Ignored]
        public string RankLetter
        {
            get => Rank switch
            {
                -1 => "F",
                0 => "D",
                1 => "C",
                2 => "B",
                3 => "A",
                4 => "S",
                5 => "S+",
                6 => "SS",
                7 => "SS+",
                _ => "_"
            };
        }

        /// <summary>
        /// A string which distinguishes this score replay in a single beatmap set
        /// </summary>
        public string Details()
        {
            var age = DateTime.Now - Date;
            return $"({age.Days}d) {User.Username} {Accuracy:0.00%} {RankLetter} rank on [{BeatmapInfo!.DifficultyName}]";
        }

        /// <summary>
        /// The full filename to be used for exporting this player score replay.
        /// </summary>
        public string OutputReplayFilename() => 
            $"{User.Username} {RankLetter} rank on {BeatmapInfo!.Metadata.OutputName()} [{BeatmapInfo.DifficultyName}] ({Date.LocalDateTime:yyyy-MM-dd HH-mm-ss}).osr"
            .RemoveFilenameCharacters();
    }
}
