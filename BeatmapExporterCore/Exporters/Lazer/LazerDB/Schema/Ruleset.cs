using Realms;

namespace BeatmapExporterCore.Exporters.Lazer.LazerDB.Schema
{
    // Original source file (modified by kabii) Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
    public class Ruleset : RealmObject
    {
        [PrimaryKey]
        public string ShortName { get; set; } = string.Empty;
        [Indexed]
        public int OnlineID { get; set; } = -1;
        public string Name { get; set; } = string.Empty;
        public string InstantiationInfo { get; set; } = string.Empty;
        public int LastAppliedDifficultyVersion { get; set; }
        public bool Available { get; set; }
    }

    public class ModPreset : RealmObject
    {
        [PrimaryKey]
        public Guid ID { get; set; } = Guid.NewGuid();
        public Ruleset Ruleset { get; set; } = null!;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Mods { get; set; } = string.Empty;
        public bool DeletePending { get; set; }
    }
}
