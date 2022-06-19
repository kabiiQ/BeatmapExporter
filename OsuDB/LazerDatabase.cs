using LazerExporter.OsuDB.Schema;
using Realms;

namespace LazerExporter.OsuDB
{
    public class LazerDatabase
    {
        const int LazerSchemaVersion = 14;
        string database;
        string filesDirectory;

        private LazerDatabase(string database)
        {
            this.database = database;
            this.filesDirectory = Path.Combine(Path.GetDirectoryName(database)!, "files");
        }

        static string? GetDatabaseFile(string directory)
        {
            string path = Path.Combine(directory, "client.realm");
            return File.Exists(path) ? path : null;
        }

        public static LazerDatabase? Locate(string directory)
        {
            string? dbFile = GetDatabaseFile(directory);
            if (dbFile is null)
            {
                Console.Write("osu! song database not found. Please find and provide your osu! data folder.\nThe folder should contain a \"client.realm\" file and can be opened from in-game.\n\nFolder path: ");
                string? input = Console.ReadLine();
                if(input is not null)
                {
                    dbFile = GetDatabaseFile(input);
                }
            } 
            return dbFile is not null ? new LazerDatabase(dbFile) : null;
        }

        public Realm? Open()
        {
            RealmConfiguration config = new(database)
            {
                IsReadOnly = true,
                SchemaVersion = LazerSchemaVersion,
            };
            config.Schema = new[] { 
                typeof(Beatmap),
                typeof(BeatmapDifficulty),
                typeof(BeatmapMetadata),
                typeof(BeatmapSet),
                typeof(RealmFile),
                typeof(RealmNamedFileUsage),
                typeof(RealmUser),
                typeof(Ruleset)
            };

            try
            {
                return Realm.GetInstance(config);
            } 
            catch (IOException ex)
            {
                Console.WriteLine($"Error opening database: {ex.Message}");
                return null;
            }
        }

        public FileStream? OpenHashedFile(string hash)
        {
            string path = Path.Combine(filesDirectory, hash.Substring(0, 1), hash.Substring(0, 2), hash);
            try
            {
                return File.Open(path, FileMode.Open);
            } catch (IOException ioe)
            {
                Console.WriteLine($"Unable to open file: {hash} :: {ioe.Message}");
                return null;
            }
        }
    }
}
