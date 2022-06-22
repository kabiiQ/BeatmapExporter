using BeatmapExporter.Exporters.Lazer;

namespace BeatmapExporter
{
    internal class ExporterLoader
    {
        static void Main(string[] args)
        {
            // currently only load lazer, can add interface for selecting osu stable here later
            BeatmapExporter exporter = LazerLoader.Load(args.FirstOrDefault());
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