using BeatmapExporter.Exporters.Lazer;
using BeatmapExporter.Exporters.Lazer.LazerDB;
using BeatmapExporter.Exporters.Lazer.LazerDB.Schema;
using BeatmapExporterCLI.Interface;
using Realms;
using Remotion.Linq.Parsing;

namespace BeatmapExporterCLI.Data
{
    public static class LazerLoader
    {
        public static ExporterApp Load(string? directory)
        {
            // osu!lazer has been selected at this point. 
            // load the osu!lazer database here, can operate on lazer-specific objects
            // assume default lazer directory, prompting user if not found (or specified as arg)
            directory ??= LazerDatabase.DefaultInstallDirectory();

            Console.Write($" --- kabii's Lazer Exporter ---\n\nChecking directory: {directory}\nRun this application with your osu!lazer storage directory as an argument if this is not your osu! data location.\n");

            // load beatmap information into memory
            LazerDatabase? database = Locate(directory);
            if (database is null)
            {
                Console.WriteLine("osu! database not found in default location or selected.");
                ExporterApp.Exit();
            }

            Realm? realm = null;
            try
            {
                realm = database!.Open();
                if (realm is null)
                    throw new IOException("Unable to open osu! database.");
            }
            catch (Exception e)
            {
                Console.WriteLine($"\nError opening database: {e.Message}");
                if (e is LazerVersionException)
                {
                    Console.WriteLine("The osu!lazer database structure has updated since the last BeatmapExporter update.");
                    Console.WriteLine("\nYou can check https://github.com/kabiiQ/BeatmapExporter/releases for a new release, or file an issue there to let me know it needs updating if it's been a few days.");
                }
                ExporterApp.Exit();
            }

            Console.Write("\nosu! database opened successfully.\nLoading beatmaps...\n");

            // load beatmaps into memory for filtering/export later
            List<BeatmapSet> beatmaps = realm!.All<BeatmapSet>().ToList();

            Console.WriteLine("Loading osu!lazer collections...");
            List<BeatmapCollection> collections = realm.All<BeatmapCollection>().ToList();

            // start console i/o loop
            LazerExporter exporter = new(database, beatmaps, collections);
            LazerExporterCLI cli = new(exporter);
            return new ExporterApp(cli);
        }

        public static LazerDatabase? Locate(string directory)
        {
            string? dbFile = LazerDatabase.GetDatabaseFile(directory);
            if (dbFile is null)
            {
                Console.Write("osu! song database not found. Please find and provide your osu!lazer data folder.\nThe folder should contain a \"client.realm\" file and can be opened from in-game.\n\nFolder path: ");
                string? input = Console.ReadLine();
                if (input is not null)
                {
                    dbFile = LazerDatabase.GetDatabaseFile(input);
                }
            }
            return dbFile is not null ? new LazerDatabase(dbFile) : null;
        }
    }
}
