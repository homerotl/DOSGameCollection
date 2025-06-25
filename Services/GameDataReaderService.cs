using DOSGameCollection.Models;

namespace DOSGameCollection.Services;

public static class GameDataReaderService
{
    private const string GameNamePrefix = "game.name=";
    private const string GameReleaseYearPrefix = "game.release.year=";
    private const string GameDeveloperPrefix = "game.developer=";
    private const string GamePublisherPrefix = "game.publisher=";
    private const string ParentalRatingPrefix = "game.parental.rating=";
    private const string IsoSectionHeader = "[isos]";
    private const string CommandsSectionHeader = "[commands]";

    private enum ParsingState { None, Isos, Commands }

    /// <summary>
    /// Scans a directory for media files with specified extensions and reads their display names from file-info.txt.
    /// </summary>
    /// <param name="directoryPath">The path to the directory to scan.</param>
    /// <param name="allowedExtensions">An array of allowed file extensions (e.g., ".png", ".mp4").</param>
    /// <returns>A list of MediaFileInfo objects.</returns>
    private static async Task<List<MediaFileInfo>> GetMediaFilesAsync(string directoryPath, string[] allowedExtensions)
    {
        var mediaFiles = new List<MediaFileInfo>();
        if (!Directory.Exists(directoryPath))
        {
            return mediaFiles;
        }

        var fileInfoMap = await ParseDisplayNamesAsync(directoryPath);

        var files = Directory.EnumerateFiles(directoryPath)
                             .Where(f => allowedExtensions.Contains(Path.GetExtension(f).ToLowerInvariant()))
                             .OrderBy(f => f, StringComparer.OrdinalIgnoreCase);

        foreach (var file in files)
        {
            var fileName = Path.GetFileName(file);
            var displayName = fileInfoMap.GetValueOrDefault(fileName, Path.GetFileNameWithoutExtension(fileName));
            mediaFiles.Add(new MediaFileInfo(file, displayName));
        }

        return mediaFiles;
    }

    /// <summary>
    /// Scans a directory for disc image files (.img, .iso, .cue) and reads their display names from file-info.txt.
    /// </summary>
    /// <param name="directoryPath">The path to the directory to scan.</param>
    /// <returns>A list of DiscImageInfo objects.</returns>
    private static async Task<List<DiscImageInfo>> GetDiscImagesAsync(string directoryPath)
    {
        var discImages = new List<DiscImageInfo>();
        if (!Directory.Exists(directoryPath))
        {
            return discImages;
        }

        var fileInfoMap = await ParseDisplayNamesAsync(directoryPath);

        var files = Directory.EnumerateFiles(directoryPath, "*.img")
                             .Concat(Directory.EnumerateFiles(directoryPath, "*.iso"))
                             .Concat(Directory.EnumerateFiles(directoryPath, "*.cue")) // Include .cue for CD images
                             .OrderBy(f => f, StringComparer.OrdinalIgnoreCase);

        foreach (var file in files)
        {
            var fileInfo = new FileInfo(file);
            var fileName = fileInfo.Name;
            var displayName = fileInfoMap.GetValueOrDefault(fileName, Path.GetFileNameWithoutExtension(fileName));
            var pngPath = Path.ChangeExtension(file, ".png");

            discImages.Add(new DiscImageInfo { ImgFileName = file, DisplayName = displayName, PngFilePath = File.Exists(pngPath) ? pngPath : null, FileSizeInBytes = fileInfo.Length });
        }

        return discImages;
    }

    public static async Task<GameConfiguration?> ParseCfgFileAsync(string cfgFilePath, string gameDirectoryPath)
    {
        if (!File.Exists(cfgFilePath))
        {
            AppLogger.Log($"Warning: File not found for parsing: {cfgFilePath}");
            return null;
        }

        GameConfiguration config = new()
        {
            GameDirectoryPath = gameDirectoryPath,
            GameName = Path.GetFileName(gameDirectoryPath) // Default name if not in config
        };

        var isoFileNames = new List<string>();

        try
        {
            string[] lines = await File.ReadAllLinesAsync(cfgFilePath);
            ParsingState currentState = ParsingState.None;

            foreach (string line in lines)
            {
                string trimmedLine = line.Trim();

                // Skip empty lines and comments (lines starting with ';' or '#')
                if (string.IsNullOrEmpty(trimmedLine) || trimmedLine.StartsWith(";") || trimmedLine.StartsWith("#"))
                {
                    // Skip without changing state
                    continue;
                }

                if (trimmedLine.Equals(IsoSectionHeader, StringComparison.OrdinalIgnoreCase))
                {
                    currentState = ParsingState.Isos;
                    continue;
                }
                else if (trimmedLine.Equals(CommandsSectionHeader, StringComparison.OrdinalIgnoreCase))
                {
                    currentState = ParsingState.Commands;
                    continue;
                }
                else if (trimmedLine.StartsWith(GameNamePrefix, StringComparison.OrdinalIgnoreCase))
                {
                    config.GameName = trimmedLine.Substring(GameNamePrefix.Length).Trim();
                    currentState = ParsingState.None; // Game name is not part of Isos or Commands sections
                    continue;
                }
                else if (trimmedLine.StartsWith(GameReleaseYearPrefix, StringComparison.OrdinalIgnoreCase))
                {
                    string yearString = trimmedLine.Substring(GameReleaseYearPrefix.Length).Trim();
                    if (int.TryParse(yearString, out int year))
                    {
                        config.ReleaseYear = year;
                    }
                    else { AppLogger.Log($"Warning: Invalid release year format in {cfgFilePath}: '{yearString}'"); }
                    currentState = ParsingState.None;
                    continue;
                }
                else if (trimmedLine.StartsWith(GameDeveloperPrefix, StringComparison.OrdinalIgnoreCase))
                {
                    config.Developer = trimmedLine.Substring(GameDeveloperPrefix.Length).Trim();
                    currentState = ParsingState.None;
                    continue;
                }
                else if (trimmedLine.StartsWith(GamePublisherPrefix, StringComparison.OrdinalIgnoreCase))
                {
                    config.Publisher = trimmedLine.Substring(GamePublisherPrefix.Length).Trim();
                    currentState = ParsingState.None;
                    continue;
                }
                else if (trimmedLine.StartsWith(ParentalRatingPrefix, StringComparison.OrdinalIgnoreCase))
                {
                    string fileValue = trimmedLine.Substring(ParentalRatingPrefix.Length).Trim();
                    if (!string.IsNullOrEmpty(fileValue))
                    {
                        string? displayValue = FormatTools.DecodeRating(fileValue);
                        if (displayValue == null)
                        {
                            AppLogger.Log($"Warning: Unknown parental rating value '{fileValue}' in {cfgFilePath}.");
                        }
                        config.ParentalRating = displayValue; // Assigns null if invalid, which is correct
                    }
                    currentState = ParsingState.None;
                }

                switch (currentState)
                {
                    case ParsingState.Isos:
                        isoFileNames.Add(trimmedLine);
                        break;
                    case ParsingState.Commands:
                        config.DosboxCommands.Add(trimmedLine);
                        break;
                    case ParsingState.None:
                        break;
                }
            }
        }
        catch (UnauthorizedAccessException ex)
        {
            AppLogger.Log($"Error parsing {cfgFilePath}: Access denied. {ex.Message}");
            throw;
        }
        catch (IOException ex)
        {
            AppLogger.Log($"Error parsing {cfgFilePath}: I/O error. {ex.Message}");
            throw;
        }
        catch (Exception ex)
        {
            AppLogger.Log($"An unexpected error occurred while parsing {cfgFilePath}: {ex.Message}");
            throw;
        }

        if (string.IsNullOrEmpty(config.GameName) || config.GameName == "Unnamed Game")
        {
            // Default to the game's directory name if not specified in the file
            config.GameName = Path.GetFileName(gameDirectoryPath);
        }

        // Check for manual.pdf in the game directory
        string potentialManualPath = Path.Combine(gameDirectoryPath, "manual.pdf");
        if (File.Exists(potentialManualPath))
        {
            config.ManualPath = potentialManualPath;
        }

        // Set box art existence flags
        config.HasFrontBoxArt = File.Exists(config.FrontBoxArtPath); // Existing property
        config.HasBackBoxArt = File.Exists(config.BackBoxArtPath);   // Existing property

        // Scan for media files using the new helper
        config.CaptureFiles = await GetMediaFilesAsync(Path.Combine(gameDirectoryPath, "media", "captures"), [".png"]);
        config.VideoFiles = await GetMediaFilesAsync(Path.Combine(gameDirectoryPath, "media", "videos"), [".avi", ".mp4", ".mpg"]);
        config.InsertFiles = await GetMediaFilesAsync(Path.Combine(gameDirectoryPath, "media", "inserts"), [".png", ".pdf"]); // NEW

        // Scan for soundtrack files
        string ostDirectory = Path.Combine(gameDirectoryPath, "media", "ost");
        config.SoundtrackFiles = (await GetMediaFilesAsync(ostDirectory, [".mp3", ".ogg", ".flac"]))
                                 .Concat(await GetMediaFilesAsync(Path.Combine(ostDirectory, "midi"), [".mid"]))
                                 .ToList();
        string soundtrackCoverPath = Path.Combine(ostDirectory, "cover.png");
        if (File.Exists(soundtrackCoverPath))
        {
            config.SoundtrackCoverPath = soundtrackCoverPath;
        }

        // Scan for disc images using the new helper
        config.DiscImages = await GetDiscImagesAsync(Path.Combine(gameDirectoryPath, "disk-images"));
        config.IsoImages = await GetDiscImagesAsync(Path.Combine(gameDirectoryPath, "isos"));

        return config;
    }

    /// <summary>
    /// Parses a "file-info.txt" file in a given directory to extract display names for files.
    /// </summary>
    /// <param name="directoryPath">The directory containing the file-info.txt file.</param>
    /// <returns>A dictionary mapping file names to their display names.</returns>
    private static async Task<Dictionary<string, string>> ParseDisplayNamesAsync(string directoryPath)
    {
        var displayNames = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        string infoFilePath = Path.Combine(directoryPath, "file-info.txt");

        if (!File.Exists(infoFilePath))
        {
            return displayNames;
        }

        try
        {
            string[] infoLines = await File.ReadAllLinesAsync(infoFilePath);
            foreach (var line in infoLines)
            {
                if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#") || line.StartsWith(";")) continue;
                var parts = line.Split(new[] { ',' }, 2);
                if (parts.Length == 2)
                {
                    displayNames[parts[0].Trim()] = parts[1].Trim();
                }
            }
        }
        catch (Exception ex)
        {
            AppLogger.Log($"Error reading display name file '{infoFilePath}': {ex.Message}");
        }

        return displayNames;
    }
}