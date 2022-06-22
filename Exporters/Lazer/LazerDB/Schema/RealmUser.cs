// Original source file (modified by kabii) Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
using Realms;

namespace BeatmapExporter.Exporters.Lazer.LazerDB.Schema
{
    public class RealmUser : EmbeddedObject
    {
        public int OnlineID { get; set; } = 1;
        public string Username { get; set; } = string.Empty;
    }
}
