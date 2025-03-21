using BeatmapExporterCLI.Interface;
using BeatmapExporterCore.Exporters;
using BeatmapExporterCore.Exporters.Lazer;
using BeatmapExporterCore.Exporters.Lazer.LazerDB;
using BeatmapExporterCore.Exporters.Lazer.LazerDB.Schema;
using Realms;

namespace BeatmapExporterCLI.Data
{
    public static class LazerLoader
    {
        /// <summary>
        /// Attempt to locate and load the lazer database. May prompt user for the database path.
        /// </summary>
        public static ExporterApp Load(string? userDir)
        {
            // osu!lazer has been selected at this point. 
            // load the osu!lazer database here, can operate on lazer-specific objects
            // assume default lazer directory, prompting user if not found (or specified as arg)
            Console.Write($" --- kabii's Lazer Exporter ---\n\nNow checking default osu!lazer storage locations. You can run this application with your lazer storage location as an argument if you have it stored somewhere different.\n\n");

            var checkDirs = LazerDatabase.CheckDirectories(userDir);
            string? dbFile = null;
            foreach (var dir in checkDirs)
            {
                // check each provided or default lazer directory
                Console.WriteLine($"Checking directory: {dir}");
                dbFile = LazerDatabase.GetDatabaseFile(dir);
                if (dbFile is null)
                {
                    Console.WriteLine($"osu!lazer database not found at {dir}");
                } else
                {
                    break; // database found, do not check more locations
                }
            }

            if (dbFile is null)
            {
                // fallback: prompt user for directory to check
                Console.Write("osu! song database not found. Please find and provide your osu!lazer data folder.\nThe folder should contain a \"client.realm\" file and can be opened from in-game.\n\nFolder path: ");
                string? input = Console.ReadLine();
                if (input is not null)
                {
                    dbFile = LazerDatabase.GetDatabaseFile(input);
                }
            }

            if (dbFile is null)
            {
                // failed to find lazer database
                Console.WriteLine("osu! database not found in default location or selected.");
                ExporterApp.Exit();
            }
            LazerDatabase database = new LazerDatabase(dbFile);

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
                if (e is LazerVersionException version)
                {
                    foreach (var message in version.Details)
                    {
                        Console.Write($"\n(!) {message}\n");
                    }
                }
                else
                {
                    Console.WriteLine("\nThis is an abnormal error, and you may need to open a GitHub issue for further assistance.");
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
    }
}
