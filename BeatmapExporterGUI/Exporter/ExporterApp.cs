using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using BeatmapExporter.Exporters;
using BeatmapExporter.Exporters.Lazer;
using BeatmapExporter.Exporters.Lazer.LazerDB;
using BeatmapExporter.Exporters.Lazer.LazerDB.Schema;
using BeatmapExporterCore.Utilities;
using BeatmapExporterGUI.Utilities;
using CommunityToolkit.Mvvm.ComponentModel;
using Realms;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;

namespace BeatmapExporterGUI.Exporter
{
    public class ExporterApp : ObservableObject
    {
        public ExporterApp()
        {
            var cts = new CancellationTokenSource();
            RealmScheduler = new RealmTaskScheduler(cts.Token);
            RealmScheduler.Start();

            SystemMessages = new();
        }

        public static void Exit()
        {
            if (Application.Current!.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.Shutdown();
            }
        }

        public static void OpenLatestRelease() => ProcessHelper.OpenUrl(ExporterUpdater.Latest);

        /// <summary>
        /// Attempt to load an osu!lazer database into this app
        /// </summary>
        /// <param name="directory">A user-specified directory to override the database search</param>
        /// <returns>If the database load was a success else the user should be notified to try again</returns>
        public bool LoadDatabase(string? directory)
        {
            directory ??= LazerDatabase.DefaultInstallDirectory();

            AddSystemMessage($"Checking directory: {directory}");
            AddSystemMessage("Run this application with your osu!lazer storage directory as an argument if this is not your osu! data location.");

            string? dbFile = LazerDatabase.GetDatabaseFile(directory);
            if (dbFile == null)
            {
                AddSystemMessage("osu! song database not found. Please find and provide your osu!lazer data folder.", error: true);
                AddSystemMessage("The folder should contain a \"client.realm\" file and can be opened from in-game to locate it.");
                return false;
            }

            var database = new LazerDatabase(dbFile);
            Realm? realm;
            try
            {
                realm = database!.Open();
                if (realm is null)
                    throw new IOException("Unable to open osu! database.");
            } catch (Exception e)
            {
                AddSystemMessage($"Error opening database: {e.Message}", error: true);
                if (e is LazerVersionException)
                {
                    AddSystemMessage("The osu!lazer database structure has updated since the last BeatmapExporter update.");
                    AddSystemMessage("You can check GitHub for a new release, or file an issue there to let me know it needs updating if it's been a few days.");
                }
                return false;
            }

            AddSystemMessage("osu! database opened successfully. Loading beatmaps...");

            // load beatmaps into memory for filtering/export later
            List<BeatmapSet> beatmaps = realm!.All<BeatmapSet>().ToList();

            AddSystemMessage("Loading osu!lazer collections...");
            List<BeatmapCollection> collections = realm.All<BeatmapCollection>().ToList();

            // replace any current exporter for this ExporterApp instance with the newly loaded database
            Lazer = new(database, beatmaps, collections);
            AddSystemMessage($"Loaded osu!lazer database: {dbFile}");
            return true;
        }

        public void Unload() => Lazer = null;

        public LazerExporter? Lazer { get; private set; }

        public ExporterConfiguration? Configuration => Lazer?.Configuration;

        public RealmTaskScheduler RealmScheduler { get; }

        public record struct Message(bool IsError, string Content, DateTime Timestamp)
        {
            public override readonly string? ToString() => $"{(IsError ? "(!)" : "")}{Timestamp:HH:mm} - {Content}";
        }

        public ObservableCollection<Message> SystemMessages { get; }

        public void AddSystemMessage(string message, bool error = false) => SystemMessages.Insert(0, new(error, message, DateTime.Now));
    }
}
