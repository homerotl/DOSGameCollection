namespace DOSGameCollection.Models;

public class GameConfiguration
{
    public string GameName { get; set; } = "Unnamed Game"; // Default name
    public int? ReleaseYear { get; set; }
    public string? Developer { get; set; }
    public string? Publisher { get; set; }
    public List<DiscImageInfo> IsoImages { get; set; } = [];
    public List<string> DosboxCommands { get; set; } = [];
    public List<DiscImageInfo> DiscImages { get; set; } = [];
    public List<MediaFileInfo> CaptureFiles { get; set; } = [];
    public List<MediaFileInfo> VideoFiles { get; set; } = [];
    // Add these two properties to your GameConfiguration class
    public List<MediaFileInfo> SoundtrackFiles { get; set; } = [];
    public string? SoundtrackCoverPath { get; set; }

    public string? ParentalRating { get; set; }
    public required string GameDirectoryPath { get; set; }

    public string ConfigFilePath => Path.Combine(GameDirectoryPath, "game.cfg");

    public string MountCPath => Path.Combine(GameDirectoryPath, "game-files");

    public string IsoBasePath => Path.Combine(GameDirectoryPath, "isos");

    public string FrontBoxArtPath => Path.Combine(GameDirectoryPath, "media", "box-art", "front.png");
    public bool HasFrontBoxArt { get; set; }

    public string BackBoxArtPath => Path.Combine(GameDirectoryPath, "media", "box-art", "back.png");
    public bool HasBackBoxArt { get; set; }

    public string SynopsisFilePath => Path.Combine(GameDirectoryPath, "media", "synopsis.txt");

    public string DosboxConfPath => Path.Combine(GameDirectoryPath, "dosbox-staging.conf");

    public string? ManualPath { get; set; } // Path to the game's manual, if it exists

    public override string ToString()
    {
        return GameName;
    }
}