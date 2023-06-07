using BeatmapExporter.Exporters.Lazer.LazerDB.Schema;

namespace BeatmapExporter.Exporters
{
    public record struct MapCollection(int CollectionID, List<Beatmap> Beatmaps);

    public interface IBeatmapExporter
    {

        int BeatmapSetCount { get; }
        int BeatmapCount { get; }
        int SelectedBeatmapSetCount { get; }
        int SelectedBeatmapCount { get; }
        int CollectionCount { get; }
        ExporterConfiguration Configuration { get; }

        string FilterDetail();
        void UpdateSelectedBeatmaps();
        void DisplaySelectedBeatmaps();
        void ExportBeatmaps();
        void ExportAudioFiles();
        void DisplayCollections();
    }
}
