﻿using Avalonia.Controls;
using Avalonia.Platform.Storage;
using BeatmapExporter.Exporters.Lazer.LazerDB;
using System.Threading.Tasks;

namespace BeatmapExporterGUI.Utilities
{
    public class DialogService
    {
        private readonly Window mainWindow;
        private readonly FilePickerFileType realmFilter;

        public DialogService(Window mainWindow)
        {
            this.mainWindow = mainWindow;

            realmFilter = new("osu!lazer Realm")
            {
                Patterns = new[] { "client.realm" },
                MimeTypes = new[] { "application/octet-stream" }
            };
        }

        public async Task<string?> SelectDatabaseDialogAsync()
        {
            var storage = mainWindow.StorageProvider;
            var start = await storage.TryGetFolderFromPathAsync(LazerDatabase.DefaultInstallDirectory());
            var options = new FilePickerOpenOptions()
            {
                Title = "Select osu! database",
                AllowMultiple = false,
                FileTypeFilter = new[] { realmFilter },
                SuggestedStartLocation = start
            };
            var file = await mainWindow.StorageProvider.OpenFilePickerAsync(options);
            var parent = file.Count > 0 ? await file[0].GetParentAsync() : null;
            return parent?.Path.AbsolutePath;
        }
    }
}