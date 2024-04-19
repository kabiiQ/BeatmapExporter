using BeatmapExporterCore.Utilities;
using Realms;

namespace BeatmapExporterCore.Exporters.Lazer.LazerDB.Schema
{
    // Original source file (modified by kabii) Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
    public class BeatmapMetadata : RealmObject
    {
        public string Title { get; set; } = string.Empty;
        public string TitleUnicode { get; set; } = string.Empty;
        public string Artist { get; set; } = string.Empty;
        public string ArtistUnicode { get; set; } = string.Empty;
        public RealmUser Author { get; set; } = new RealmUser();
        public string Source { get; set; } = string.Empty;
        public string? Tags { get; set; } = string.Empty;
        public int PreviewTime { get; set; }
        public string AudioFile { get; set; } = string.Empty;
        public string BackgroundFile { get; set; } = string.Empty;

        // Author kabii
        /// <summary>
        /// The song artist and title, truncated for filename output
        /// </summary>
        public string OutputName() => $"{Artist.Trunc(30)} - {Title.Trunc(60)}";

        private static string DupeString(int dupeCount) => dupeCount > 0 ? $" ({dupeCount})" : "";

        /// <summary>
        /// The full filename to be used for exporting audio files associated with this beatmap
        /// </summary>
        public string OutputAudioFilename(int dupeCount = 0) =>
            $"{OutputName()}{DupeString(dupeCount)}.mp3"
            .RemoveFilenameCharacters();

        /// <summary>
        /// The full filename to be used for exporting background images associated with this beatmap
        /// </summary>
        public string OutputBackgroundFilename(int beatmapId, int dupeCount = 0)
        {
            var extension = Path.GetExtension(BackgroundFile);
            return 
                $"{OutputName()} ({beatmapId}){DupeString(dupeCount)}{extension}"
                .RemoveFilenameCharacters();
        }
    }
}
