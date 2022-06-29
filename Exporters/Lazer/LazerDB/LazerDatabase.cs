using BeatmapExporter.Exporters.Lazer.LazerDB.Schema;
using Realms;

namespace BeatmapExporter.Exporters.Lazer.LazerDB
{
    public class LazerDatabase
    {
        const int LazerSchemaVersion = 14;
        string database;
        string filesDirectory;

        private LazerDatabase(string database)
        {
            this.database = database;
            filesDirectory = Path.Combine(Path.GetDirectoryName(database)!, "files");
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
                if (input is not null)
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

        string HashedFilePath(string hash) => Path.Combine(filesDirectory, hash.Substring(0, 1), hash.Substring(0, 2), hash);

        public FileStream? OpenHashedFile(string hash)
        {
            try
            {
                string path = HashedFilePath(hash);
                return File.Open(path, FileMode.Open);
            }
            catch (IOException ioe)
            {
                Console.WriteLine($"Unable to open file: {hash} :: {ioe.Message}");
                return null;
            }
        }

        public FileStream? OpenNamedFile(BeatmapSet set, string filename)
        {
            // get named file from specific beatmap - check if it exists in this beatmap
            string? fileHash = set.Files.FirstOrDefault(f => f.Filename == filename)?.File?.Hash;
            if(fileHash is null)
            {
                Console.WriteLine($"File {filename} not found in beatmap {set.ArchiveFilename()}");
                return null;
            }
            try
            {
                string path = HashedFilePath(fileHash);
                return File.Open(path, FileMode.Open);
            }
            catch (IOException ioe)
            {
                Console.WriteLine($"Unable to open file: {filename} from beatmap {set.ArchiveFilename()} :: {ioe.Message}");
                return null;
            }
        }
    }
}
