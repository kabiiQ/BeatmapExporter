// Original source file (modified by kabii) Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
using Realms;

namespace BeatmapExporter.Exporters.Lazer.LazerDB.Schema
{
    public class BeatmapDifficulty : EmbeddedObject
    {
        public float DrainRate { get; set; } = 0.0f;
        public float CircleSize { get; set; } = 0.0f;
        public float OverallDifficulty { get; set; } = 0.0f;
        public float ApproachRate { get; set; } = 0.0f;

        public double SliderMultiplier { get; set; } = 1;
        public double SliderTickRate { get; set; } = 1;
    }
}
