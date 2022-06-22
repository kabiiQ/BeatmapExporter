// Original source file (modified by kabii) Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
using Realms;

namespace BeatmapExporter.Exporters.Lazer.LazerDB.Schema
{
    public class Ruleset : RealmObject
    {
        [PrimaryKey]
        public string ShortName { get; set; } = string.Empty;
        [Indexed]
        public int OnlineID { get; set; } = -1;
        public string Name { get; set; } = string.Empty;
        public string InstantiationInfo { get; set; } = string.Empty;
        public bool Available { get; set; }
    }
}
