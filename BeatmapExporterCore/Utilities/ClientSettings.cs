using BeatmapExporterCore.Exporters;
using BeatmapExporterCore.Filters;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace BeatmapExporterCore.Utilities
{
    /// <summary>
    /// Serializable format for a <see cref="BeatmapFilter"/>
    /// For storing user configuration of beatmap filters
    /// </summary>
    public record SerializedBeatmapFilter(string FilterType, string Input, bool Negated)
    {
        public static SerializedBeatmapFilter FromBeatmapFilter(BeatmapFilter filter)
            => new(filter.Template.ShortName, filter.Input, filter.Negated);

        public BeatmapFilter? ToBeatmapFilter()
            => FilterTypes.AllTypes.FirstOrDefault(t => t.ShortName == FilterType)?.Constructor(Input, Negated);
    }

    /// <summary>
    /// Container for user program settings and related utilities for saving to disk
    /// </summary>
    public class ClientSettings
    {
        public static readonly string APPDIR = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "BeatmapExporter");

        private static readonly string configPath = Path.Combine(APPDIR, "lastSettings.json");
        private static readonly JsonSerializerOptions options = new()
        {
            WriteIndented = true
        };

        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        #region Persisted Settings
        public string? DatabasePath { get; set; } = null;

        public string ExportPath { get; set; } = "lazerexport";

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public ExportFormat ExportFormat { get; set; } = ExportFormat.Beatmap;

        public bool MatchAllFilters { get; set; } = true;

        public bool MergeCollections { get; set; } = true;

        public bool MergeCaseInsensitive { get; set; } = true;

        public List<SerializedBeatmapFilter> AppliedFilters { get; set; } = [];
        #endregion

        #region Serialization Methods
        /// <summary>
        /// Load the application last-known user settings.
        /// Any exception encountered during loading will be propagated to caller, as it will likely be displayed to the user but loading should continue.
        /// </summary>
        /// <exception cref="Exception" />
        public static ClientSettings? LoadFromFile()
        {
            if (!Directory.Exists(APPDIR))
            {
                Directory.CreateDirectory(APPDIR);
            }

            if (!File.Exists(configPath))
            {
                return null;
            }

            var json = File.ReadAllText(configPath);
            var settings = JsonSerializer.Deserialize<ClientSettings>(json)!;

            // Perform simple validation of export path with default fallback
            if (settings.ExportPath == null || !Directory.GetParent(settings.ExportPath)!.Exists)
            {
                settings.ExportPath = "lazerexport";
            }

            return settings;
        }

        /// <summary>
        /// Saves this ClientSettings to disk
        /// </summary>
        /// <exception cref="Exception" />
        public void SaveToFile()
        {
            var json = JsonSerializer.Serialize(this, options);
            File.WriteAllText(configPath, json);
        }
        #endregion

        #region Setting Update Utilities
        /// <summary>
        /// Performs a side-effect of saving the settings to disk, where the call will not fail but may cause log file output.
        /// </summary>
        public void TrySave()
        {
            try
            {
                SaveToFile();
            }
            catch (Exception e)
            {
                Logger.Error(e, "Failed to save settings to file");
            }
        }

        /// <summary>
        /// Saves a known-good database path as its parent directory.
        /// </summary>
        public void SaveDatabase(string path)
        {
            DatabasePath = Directory.GetParent(path)!.FullName;
            TrySave();
        }

        /// <summary>
        /// Saves the last used export path.
        /// </summary>
        public void SaveExportPath(string path)
        {
            ExportPath = path;
            TrySave();
        }
        
        /// <summary>
        /// Saves the preference for beatmap filter logic.
        /// </summary>
        public void SaveFilterMode(bool mode)
        {
            MatchAllFilters = mode;
            TrySave();
        }

        /// <summary>
        /// Saves the preference for if collections should be merged in an exported collection.db.
        /// </summary>
        public void SaveMerge(bool shouldMerge)
        {
            MergeCollections = shouldMerge;
            TrySave();
        }

        /// <summary>
        /// Saves the preference for if collections should be merged in a case-insensitive manner.
        /// </summary>
        public void SaveCaseInsensitive(bool caseInsensitive)
        {

           MergeCaseInsensitive = caseInsensitive;
            TrySave();
        }

        /// <summary>
        /// Saves the last used export format.
        /// </summary>
        public void SaveExportFormat(ExportFormat format)
        {
            ExportFormat = format;
            TrySave();
        }

        public void SaveFilters(List<BeatmapFilter> filters)
        {
            AppliedFilters = filters
                .Select(f => SerializedBeatmapFilter.FromBeatmapFilter(f))
                .ToList();
            TrySave();
        }
        #endregion
    }
}
