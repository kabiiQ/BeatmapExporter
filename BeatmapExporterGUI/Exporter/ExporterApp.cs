using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using BeatmapExporterCore.Exporters;
using BeatmapExporterCore.Exporters.Lazer;
using BeatmapExporterCore.Exporters.Lazer.LazerDB;
using BeatmapExporterCore.Exporters.Lazer.LazerDB.Schema;
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

        /// <summary>
        /// Exits the AvaloniaUI application
        /// </summary>
        public static void Exit()
        {
            if (Application.Current!.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.Shutdown();
            }
        }

        /// <summary>
        /// Opens the latest BeatmapExporter release in web browser
        /// </summary>
        public static void OpenLatestRelease() => PlatformUtil.OpenUrl(ExporterUpdater.Latest);

        /// <summary>
        /// Attempt to load an osu!lazer database into this app
        /// </summary>
        /// <param name="directory">A user-specified directory to override the database search</param>
        /// <returns>If the database load was a success else the user should be notified to try again</returns>
        public bool LoadDatabase(string? userDir)
        {
            // load the osu!lazer database here, check default directories
            var checkDirs = LazerDatabase.CheckDirectories(userDir);
            AddSystemMessage("Now checking default osu!lazer storage locations.");
            AddSystemMessage("You can run this application with your lazer storage location as an argument if you have it stored somewhere different.");

            string? dbFile = null;
            foreach (var dir in checkDirs)
            {
                // check each provided or default lazer directory
                AddSystemMessage($"Checking directory: {dir}");
                dbFile = LazerDatabase.GetDatabaseFile(dir);
                if (dbFile is null)
                {
                    AddSystemMessage($"osu! song database not found at {dir}.", error: true);
                } else
                {
                    break; // database found, do not check more locations
                }
            }

            // simply not loading anything on failure for the GUI
            // the user will be able to use a standard button for selecting the database if nothing is loaded 
            if (dbFile is null)
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
                if (e is LazerVersionException version)
                {
                    foreach (var message in version.Details)
                    {
                        AddSystemMessage(message, error: true);
                    }
                }
                else
                {
                    AddSystemMessage("This is an abnormal error, and you may need to open a GitHub issue for further assistance.", error: true);
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

        /// <summary>
        /// Unloads the LazerExporter instance, allowing the user to load a database.
        /// </summary>
        public void Unload() => Lazer = null;

        /// <summary>
        /// The currently loaded LazerExporter instance.
        /// </summary>
        public LazerExporter? Lazer { get; private set; }

        /// <summary>
        /// The ExporterConfiguration contained within the currently loaded LazerExporter instance.
        /// </summary>
        public ExporterConfiguration? Configuration => Lazer?.Configuration;

        /// <summary>
        /// The single-thread task scheduler that should be used for all Realm database access.
        /// </summary>
        public RealmTaskScheduler RealmScheduler { get; }

        /// <summary>
        /// Struct representing a single BeatmapExporter system message.
        /// </summary>
        public record struct Message(bool IsError, string Content, DateTime Timestamp)
        {
            public override readonly string? ToString() => $"{(IsError ? "(!)" : "")}{Timestamp:HH:mm} - {Content}";
        }

        /// <summary>
        /// All recorded Message instances for this application 
        /// </summary>
        public ObservableCollection<Message> SystemMessages { get; }

        /// <summary>
        /// Adds a new system message to be displayed to the user
        /// </summary>
        /// <param name="message">The message content</param>
        /// <param name="error">If this message represents an error. May be formatted differently for the user.</param>
        public void AddSystemMessage(string message, bool error = false) => SystemMessages.Insert(0, new(error, message, DateTime.Now));
    }
}
