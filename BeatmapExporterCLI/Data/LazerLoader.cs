using BeatmapExporterCLI.Interface;
using BeatmapExporterCore.Exporters;
using BeatmapExporterCore.Exporters.Lazer;
using BeatmapExporterCore.Exporters.Lazer.LazerDB;
using BeatmapExporterCore.Exporters.Lazer.LazerDB.Schema;
using BeatmapExporterCore.Utilities;
using Realms;

namespace BeatmapExporterCLI.Data
{
    public static class LazerLoader
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Attempt to locate and load the lazer database. May prompt user for the database path.
        /// </summary>
        public static ExporterApp Load(string? userDir)
        {
            // osu!lazer has been selected at this point. 
            // load the osu!lazer database here, can operate on lazer-specific objects
            Console.Write(" --- BeatmapExporter for osu!lazer ---\n\n");
            Console.Write($"BeatmapExporter application data and error logs located in {ClientSettings.APPDIR}\n");

            ClientSettings settings;
            try
            {
                // Load any previous user settings from file, or use defaults if this file does not exist.
                settings = ClientSettings.LoadFromFile();
            } catch (Exception e)
            {
                Console.Write($"\nUnable to load application settings: {e.Message}\n");
                Console.Write("Loading will continue with default settings.\n\n");
                settings = new();
            }
            
            // Attempt to load FFmpeg transcoder
            var transcoder = new Transcoder();
            Console.Write(transcoder.Available
                ? "FFmpeg successfully loaded! .mp3 export for beatmaps that use other audio formats will be available.\n\n"
                : "FFmpeg not found. Conversion to .mp3 for beatmap audio export will not be available. This is not required and you can ignore this warning if you are not trying to output mp3 files.\n\n");
            

            Console.Write("Now checking known osu!lazer storage locations.\n\n");
            List<string?> userDirs = [userDir, settings.DatabasePath];
            var checkDirs = userDirs.Concat(LazerDatabase.GetDefaultDirectories());
            string? dbFile = null;
            foreach (var dir in checkDirs)
            {
                // check each provided or default lazer directory
                if (dir is null) continue;
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
                Logger.Error(e, "Error opening database");
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

            Console.Write("\nosu! database opened successfully.\nLoading content...\n");
            settings.SaveDatabase(dbFile);

            // load beatmaps into memory for filtering/export later
            List<BeatmapSet> beatmaps = realm!.All<BeatmapSet>().ToList();
            List<BeatmapCollection> collections = realm.All<BeatmapCollection>().ToList();
            List<Skin> skins = realm.All<Skin>().ToList();
            Console.WriteLine($"Load complete. Found {beatmaps.Count} beatmaps, {collections.Count} collections, {skins.Count} skins.");

            // start console i/o loop
            LazerExporter exporter = new(database, settings, beatmaps, collections, skins, transcoder);
            LazerExporterCLI cli = new(exporter);
            return new ExporterApp(cli);
        }
    }
}
