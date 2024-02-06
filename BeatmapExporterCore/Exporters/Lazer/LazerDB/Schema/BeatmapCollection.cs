using Realms;

namespace BeatmapExporter.Exporters.Lazer.LazerDB.Schema
{
    // Original source file (modified by kabii) Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
    public class BeatmapCollection : RealmObject
    {
        [PrimaryKey]
        public Guid ID { get; set; }

        public string Name { get; set; } = string.Empty;

        public IList<string> BeatmapMD5Hashes { get; } = null!;

        public DateTimeOffset LastModified { get; set; }
    }
}
