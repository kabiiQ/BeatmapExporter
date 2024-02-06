using BeatmapExporter.Exporters.Lazer.LazerDB.Schema;

namespace BeatmapExporter.Exporters
{
    /// <summary>
    /// Container representing a single discovered osu! collection.
    /// </summary>
    public record struct MapCollection(int CollectionID, List<Beatmap> Beatmaps);

    /// <summary>
    /// Interface representing a beatmap exporter (i.e. lazer or stable)
    /// Empty while not currently in use with 2.0 refactoring.
    /// </summary>
    public interface IBeatmapExporter { }
}
