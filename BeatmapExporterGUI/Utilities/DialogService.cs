using Avalonia.Controls;
using Avalonia.Platform.Storage;
using BeatmapExporter.Exporters.Lazer.LazerDB;
using System.Threading.Tasks;

namespace BeatmapExporterGUI.Utilities
{
    /// <summary>
    /// Service class with methods for opening dialogs to the user.
    /// </summary>
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

        /// <summary>
        /// Opens a dialog for the user to select a client.realm file, returning the containing directory.
        /// </summary>
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
            var file = await storage.OpenFilePickerAsync(options);
            var parent = file.Count > 0 ? await file[0].GetParentAsync() : null;
            return parent?.Path.LocalPath;
        }

        /// <summary>
        /// Opens a dialog for the user to select any directory.
        /// </summary>
        public async Task<string?> SelectDirectoryAsync(string? current)
        {
            var storage = mainWindow.StorageProvider;
            var options = new FolderPickerOpenOptions();
            options.Title = "Select export target";
            if (current != null)
                options.SuggestedStartLocation = await storage.TryGetFolderFromPathAsync(current);

            var dir = await storage.OpenFolderPickerAsync(options);
            return dir.Count > 0 ? dir[0].Path.LocalPath : null;
        }
    }
}
