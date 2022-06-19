using LazerExporter.OsuDB;
using LazerExporter.OsuDB.Schema;
using Realms;
using System.Runtime.InteropServices;

namespace LazerExporter
{
    internal class LazerLoader
    {
        static void Main(string[] args)
        {

            // assume default lazer directory, prompting user if not found (or specified as arg)
            string? directory;
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                // default install location: %appdata%/osu
                string appdata = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                directory = args.FirstOrDefault() ?? Path.Combine(appdata, "osu");
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                directory = "~/.osu/Songs";
            }
            else
            {
                directory = "~/.local/share/osu";
            }

            Console.Write($" --- kabii's Lazer Exporter ---\n\nChecking directory: {directory}\nRun this application with your osu!lazer storage directory as an argument if this is not your osu! data location.\n");

            // load beatmap information into memory
            LazerDatabase? database = LazerDatabase.Locate(directory);
            if(database is null)
            {
                Console.WriteLine("osu! database not found in default location or selected.");
                Exit();
            }

            Realm? realm = database!.Open();
            if(realm is null)
            {
                Console.WriteLine("\nUnable to open osu! database.");
                Exit();
            }

            Console.Write("\nosu! database opened successfully.\nLoading beatmaps...\n");

            // load beatmaps into memory for filtering/export later
            List<BeatmapSet> beatmaps = realm!.All<BeatmapSet>().ToList();

            // start console i/o loop
            LazerExporter exporter = new(database, beatmaps);
            exporter.StartApplicationLoop();

            Exit();
        }

        public static void Exit()
        {
            // keep console open
            Console.Write("\nPress any key to exit.\n");
            Console.ReadKey();
            Environment.Exit(0);
        }
    }
}