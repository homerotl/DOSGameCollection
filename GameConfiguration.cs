namespace DOSGameCollection;

/// <summary>
/// Represents the full configuration details parsed from a single .cfg file.
/// </summary>
public class GameConfiguration
{
    public string GameName { get; set; } = "Unnamed Game"; // Default name
    public List<string> IsoImagePaths { get; set; } = new List<string>();
    public List<string> DosboxCommands { get; set; } = new List<string>();
    
    /// <summary>
    /// The root directory path for this specific game.
    /// Example: "C:\DOSGameCollection\library\MyGame"
    /// </summary>
    public required string GameDirectoryPath { get; set; }

    /// <summary>
    /// Gets the full path to the game's .cfg file (e.g., game.cfg).
    /// </summary>
    public string ConfigFilePath => Path.Combine(GameDirectoryPath, "game.cfg");

    /// <summary>
    /// Gets the full path to the game's C: drive mount point (the "game-files" directory).
    /// </summary>
    public string MountCPath => Path.Combine(GameDirectoryPath, "game-files");

    /// <summary>
    /// Gets the full path to the game's "isos" directory, where CD images are stored.
    /// </summary>
    public string IsoBasePath => Path.Combine(GameDirectoryPath, "isos");

    /// <summary>
    /// Gets the full path to the game's "front.png" box art, if it exists.
    /// </summary>
    public string FrontBoxArtPath => Path.Combine(GameDirectoryPath, "media", "box-art", "front.png");
    /// <summary>
    /// Gets the full path to the game's DOSBox configuration file (dosbox-staging.conf).
    /// </summary>
    public string DosboxConfPath => Path.Combine(GameDirectoryPath, "dosbox-staging.conf");
    /// <summary>
    /// Provides a string representation of the object, suitable for display in a ListBox.
    /// </summary>
    /// <returns>The GameName property.</returns>
    public override string ToString()
    {
        return GameName;
    }
}