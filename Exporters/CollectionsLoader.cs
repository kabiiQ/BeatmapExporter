using osu_database_reader.BinaryFiles;
using osu_database_reader.Components.Beatmaps;

namespace BeatmapExporter.Exporters
{
    public static class CollectionsLoader
    {
        const string CollectionFile = "collection.db";

        public static List<Collection>? Load(string directory)
        {
            try
            {
                string dbPath = Path.Combine(directory, CollectionFile);
                using var dbFile = File.Open(dbPath, FileMode.Open);
                return CollectionDb.Read(dbFile).Collections;
            } catch (Exception e)
            {
                Console.WriteLine($"Unable to load {CollectionFile}: {e.Message}");
                return null;
            }
        }
    }
}
