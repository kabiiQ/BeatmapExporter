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
        public DateTimeOffset Date { get; set; }
        public RealmUser User { get; set; } = null!;
        public int Rank { get; set; }

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

        public string OutputReplayFilename() => 
            $"{User.Username} {RankLetter} rank on {BeatmapInfo!.Metadata.OutputName()} [{BeatmapInfo.DifficultyName}] ({Date.LocalDateTime:yyyy-MM-dd_HH-mm}).osr"
            .RemoveFilenameCharacters();
    }
}
