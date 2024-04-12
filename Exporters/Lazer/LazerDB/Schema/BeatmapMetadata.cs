﻿// Original source file (modified by kabii) Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
using Realms;

namespace BeatmapExporter.Exporters.Lazer.LazerDB.Schema
{
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
        private string OutputName(int beatmapId) => $"{Artist.Trunc(30)} - {Title.Trunc(60)} ({beatmapId})";

        public string OutputAudioFilename(int beatmapId) =>
            $"{OutputName(beatmapId)}.mp3"
            .RemoveFilenameCharacters();

        public string OutputBackgroundFilename(int beatmapId)
        {
            var backgroundName = BackgroundFile.Trunc(120);
            if (BackgroundFile != backgroundName)
                // restore file extension if truncated
                backgroundName = backgroundName + Path.GetExtension(BackgroundFile);
            return 
                $"{OutputName(beatmapId)} {backgroundName}"
                .RemoveFilenameCharacters();
        }
        
        //GetDisplayTitle() step in (from lazer's BeatmapMetadataExtensions) to stay consistent with lazer's naming
        public string Display()
        {
            string author = string.IsNullOrEmpty(this.Author.Username) ? string.Empty : $" ({this.Author.Username})";

            string artist = string.IsNullOrEmpty(this.Artist) ? "unknown artist" : this.Artist;
            string title = string.IsNullOrEmpty(this.Title) ? "unknown title" : this.Title;

            return $"{artist} - {title}{author}".Trim();
        }
    }
}
