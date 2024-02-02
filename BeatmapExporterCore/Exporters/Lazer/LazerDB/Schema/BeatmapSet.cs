// Original source file (modified by kabii) Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
using BeatmapExporterCore.Utilities;
using Realms;

namespace BeatmapExporter.Exporters.Lazer.LazerDB.Schema
{
    public class BeatmapSet : RealmObject
    {
        [PrimaryKey]
        public Guid ID { get; set; } = Guid.NewGuid();
        [Indexed]
        public int OnlineID { get; set; } = -1;
        public DateTimeOffset DateAdded { get; set; }
        public DateTimeOffset? DateSubmitted { get; set; }
        public DateTimeOffset? DateRanked { get; set; }
        public IList<Beatmap> Beatmaps { get; } = null!;
        public IList<RealmNamedFileUsage> Files { get; } = null!;
        public int Status { get; set; } = -3;
        public bool DeletePending { get; set; }
        public string Hash { get; set; } = string.Empty;
        public bool Protected { get; set; }

        // Author kabii
        IList<Beatmap>? selected = null;

        [Ignored]
        public IList<Beatmap> SelectedBeatmaps
        {
            get
            {
                return selected switch
                {
                    not null => selected,
                    null => Beatmaps
                };
            }
            set { selected = value; }
        }

        [Ignored]
        public BeatmapMetadata? DiffMetadata => Beatmaps.FirstOrDefault()?.Metadata;

        public string DiffSummary()
        {
            BeatmapMetadata metadata = Beatmaps.First().Metadata;
            var difficulties = SelectedBeatmaps.Select(b => b.StarRating).OrderBy(r => r).Select(r => r.ToString("0.00"));
            string difficultySpread = string.Join(", ", difficulties);

            return
                $"{OnlineID}: {metadata.Artist} - {metadata.Title} ({metadata.Author.Username} - {difficultySpread} stars)";
        }

        public string ArchiveFilename()
        {
            BeatmapMetadata metadata = SelectedBeatmaps.First().Metadata;
            string beatmapId = OnlineID != -1 ? $"{OnlineID} " : "";
            return
                $"{beatmapId}{metadata.Artist.Trunc(30)} - {metadata.Title.Trunc(40)} ({metadata.Author.Username.Trunc(30)}).osz"
                .RemoveFilenameCharacters();
        }
    }
}
