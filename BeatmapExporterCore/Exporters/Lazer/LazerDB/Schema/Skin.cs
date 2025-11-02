using BeatmapExporterCore.Utilities;
using Realms;

namespace BeatmapExporterCore.Exporters.Lazer.LazerDB.Schema;

// Original source file (modified by kabii) Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
public class Skin : RealmObject
{
    [PrimaryKey]
    public Guid ID { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Creator { get; set; } = string.Empty;
    public string InstantiationInfo { get; set; } = string.Empty;
    public string Hash { get; set; } = string.Empty;
    public bool Protected { get; set; }
    public IList<RealmNamedFileUsage> Files { get; } = null!;
    public bool DeletePending { get; set; }
    
    // Author kabii
    /// <summary>
    /// Returns all files within a skin that provide a filename
    /// </summary>
    [Ignored]
    public IList<RealmNamedFileUsage> NamedFiles => Files.Where(f => !string.IsNullOrWhiteSpace(f.Filename)).ToList();
    
    /// <summary>
    /// The full filename to be used for exporting this skin
    /// </summary>
    public string OutputFilename()
    {
        string creator = string.IsNullOrEmpty(Creator) ? "" : $" ({Creator})";
        return
            $"{Name}{creator}.osk"
            .RemoveFilenameCharacters();
    }
}