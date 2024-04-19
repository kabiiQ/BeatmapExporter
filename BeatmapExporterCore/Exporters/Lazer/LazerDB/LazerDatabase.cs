using BeatmapExporterCore.Exporters.Lazer.LazerDB.Schema;
using Realms;
using Realms.Exceptions;
using System.Runtime.InteropServices;

namespace BeatmapExporterCore.Exporters.Lazer.LazerDB
{
    /// <summary>
    /// Exception thrown when the osu!lazer database indicates a version mismatch
    /// </summary>
    public class LazerVersionException : ExporterException { }

    public class LazerDatabase
    {
        public const int LazerSchemaVersion = 40;
        public const string FirstLazerVersion = "2023.1222.0";

        readonly string database;
        readonly string filesDirectory;

        public LazerDatabase(string database)
        {
            this.database = database;
            filesDirectory = Path.Combine(Path.GetDirectoryName(database)!, "files");
        }

        /// <param name="directory">The directory to check for an existing lazer database</param>
        /// <returns>The file path of the lazer database file, or null if not found</returns>
        public static string? GetDatabaseFile(string directory)
        {
            string path = Path.Combine(directory, "client.realm");
            return File.Exists(path) ? path : null;
        }

        /// <summary>
        /// Attempt to determine the lazer database directory.
        /// </summary>
        public static string DefaultInstallDirectory()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                // default install location: %appdata%/osu
                return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "osu");
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Library/Application Support/osu");
            }
            else
            {
                return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".local/share/osu");
            }
        }

        /// <summary>
        /// Loads the osu!lazer database schema and opens the database file 
        /// </summary>
        /// <returns>The Realm instance with the lazer database loaded</returns>
        /// <exception cref="IOException">The database could not be opened</exception>
        /// <exception cref="LazerVersionException">The issue was detected specifically as being a version mismatch and the user should be notified</exception>
        public Realm Open()
        {
            RealmConfiguration config = new(database)
            {
                IsReadOnly = true,
                SchemaVersion = LazerSchemaVersion,
                Schema = new[] {
                    typeof(Beatmap),
                    typeof(BeatmapCollection),
                    typeof(BeatmapDifficulty),
                    typeof(BeatmapMetadata),
                    typeof(BeatmapSet),
                    typeof(BeatmapUserSettings),
                    typeof(RealmFile),
                    typeof(RealmNamedFileUsage),
                    typeof(RealmUser),
                    typeof(Ruleset),
                    typeof(ModPreset),
                    typeof(Score)
            }
            };

            try
            {
                return Realm.GetInstance(config);
            }
            catch (RealmException re)
            {
                if(re.Message.Contains("does not equal last set version"))
                {
                    throw new LazerVersionException();
                }
                throw new IOException(re.Message);
            }
        }

        /// <summary>
        /// Returns the path to a lazer file hash within the lazer directory structure 
        /// </summary>
        /// <param name="hash">The hashed lazer file name</param>
        /// <returns>The file path on disk</returns>
        string HashedFilePath(string hash) => Path.Combine(filesDirectory, hash[..1], hash[..2], hash);

        /// <summary>
        /// Opens a file from a lazer file hash
        /// </summary>
        /// <param name="hash">The hashed lazer file name</param>
        /// <exception cref="IOException">The file could not be opened</exception>
        public FileStream OpenHashedFile(string hash)
        {
            try
            {
                string path = HashedFilePath(hash);
                return File.Open(path, FileMode.Open);
            } catch(Exception e)
            {
                throw new IOException($"Unable to open file: {hash}", e);
            } 
        }

        /// <summary>
        /// Opens a file from a BeatmapSet by filename
        /// </summary>
        /// <param name="set">The BeatmapSet containing the file</param>
        /// <param name="filename">The filename to open from the BeatmapSet</param>
        /// <returns>A FileStream to the file content, null if the file was not found in the beatmap</returns>
        public FileStream? OpenNamedFile(BeatmapSet set, string filename)
        {
            // get named file from specific beatmap - check if it exists in this beatmap
            string? fileHash = set.Files.FirstOrDefault(f => f.Filename == filename)?.File?.Hash;
            if(fileHash is null)
            {
                return null;
            }
            string path = HashedFilePath(fileHash);
            try
            {
                return File.Open(path, FileMode.Open); // Throws IOException
            } catch(Exception e)
            {
                throw new IOException($"Unable to open file: {filename} from beatmap {set.ArchiveFilename()}", e);
            }
        }
    }
}
