using BeatmapExporter.Exporters.Lazer.LazerDB;
using BeatmapExporter.Exporters.Lazer.LazerDB.Schema;
using Realms;
using System.Runtime.InteropServices;

namespace BeatmapExporter.Exporters.Lazer
{
    public static class LazerLoader
    {
        public static BeatmapExporter Load(string? directory)
        {

            // osu!lazer has been selected at this point. 
            // load the osu!lazer database here, can operate on lazer-specific objects
            // assume default lazer directory, prompting user if not found (or specified as arg)
            if(directory is null)
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    // default install location: %appdata%/osu
                    string appdata = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                    directory = Path.Combine(appdata, "osu");
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    directory = "~/.osu/Songs";
                }
                else
                {
                    directory = "~/.local/share/osu";
                }
            }

            Console.Write($" --- kabii's Lazer Exporter ---\n\nChecking directory: {directory}\nRun this application with your osu!lazer storage directory as an argument if this is not your osu! data location.\n");

            // load beatmap information into memory
            LazerDatabase? database = LazerDatabase.Locate(directory);
            if (database is null)
            {
                Console.WriteLine("osu! database not found in default location or selected.");
                ExporterLoader.Exit();
            }

            Realm? realm = database!.Open();
            if (realm is null)
            {
                Console.WriteLine("\nUnable to open osu! database.");
                ExporterLoader.Exit();
            }

            Console.Write("\nosu! database opened successfully.\nLoading beatmaps...\n");

            // load beatmaps into memory for filtering/export later
            List<BeatmapSet> beatmaps = realm!.All<BeatmapSet>().ToList();

            // start console i/o loop
            LazerExporter lazerExporter = new(database, beatmaps);
            return new BeatmapExporter(lazerExporter);
        }
    }
}
