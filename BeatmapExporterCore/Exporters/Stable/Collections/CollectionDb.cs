using osu.Shared.Serialization;

namespace BeatmapExporterCore.Exporters.Stable.Collections
{
    using Collection = HashSet<string>;

    public class CollectionDb
    {
        private static readonly int baseVersion = 20250122;

        /// <summary>
        /// Construct a brand new CollectionDb with no existing base
        /// </summary>
        public CollectionDb()
        {
            Version = baseVersion;
            Collections = new();
        }

        /// <summary>
        /// Construct a CollectionDb from an existing database
        /// </summary>
        private CollectionDb(int osuVersion, bool ignoreCase)
        {
            Version = osuVersion;
            // Create collection ignoring case if coming from existing db to allow natural merge of duplicate stable/lazer collections
            var comparer = ignoreCase ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal;
            Collections = new(comparer);
        }

        /// <summary>
        /// The osu! version of the collection database
        /// </summary>
        public int Version { get; }

        /// <summary>
        /// All the collections to be stored in this collection database
        /// Dictionary (collection name, beatmap hashes)
        /// </summary>
        public Dictionary<string, Collection> Collections { get; }

        public void MergeCollection(string name, List<string> hashes)
        {
            var coll = Collections.GetValueOrDefault(name, new());
            coll.UnionWith(hashes);
            Collections[name] = coll;
        }

        /// <summary>
        /// Opens a collection.db file at the provided path and parses the osu! stable binary format
        /// </summary>
        /// <exception cref="IOException">The collection could not be opened and a warning should be noted to the user.</exception>
        public static CollectionDb Open(string path, bool ignoreCase)
        {
            using var file = File.Open(path, FileMode.Open);
            using var reader = new SerializationReader(file);

            var dbVersion = reader.ReadInt32();
            var count = reader.ReadInt32();
 
            var collections = new CollectionDb(dbVersion, ignoreCase);
            for (int i = 0; i < count; i++)
            {
                // Parse each collection
                var name = reader.ReadString();

                // Parse each beatmap diff hash
                var diffCount = reader.ReadInt32();
                var hashes = Enumerable
                    .Range(0, diffCount)
                    .Select(_ => reader.ReadString())
                    .ToList();

                collections.MergeCollection(name, hashes);
            }

            return collections;
        }

        /// <summary>
        /// Writes this instance to a collection.db file at the provided path
        /// </summary>
        /// <exception cref="=IOException">The collection.db export was unsuccessful and an error should be reported to the user.</exception>
        public void ExportFile(string path)
        {
            using var file = File.Open(path, FileMode.Create);
            using var writer = new SerializationWriter(file);

            writer.Write(Version);
            writer.Write(Collections.Count);

            foreach (var (name, hashes) in Collections)
            {
                writer.Write(name);
                writer.Write(hashes.Count);
                foreach (var hash in hashes)
                {
                    writer.Write(hash);
                }
            }
        }
    }
}
