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

    public static async Task<GameConfiguration?> ParseCfgFileAsync(string cfgFilePath, string gameDirectoryPath)
    {
        if (!File.Exists(cfgFilePath))
        {
            AppLogger.Log($"Warning: File not found for parsing: {cfgFilePath}");
            return null;
        }

        GameConfiguration config = new()
        {
            GameDirectoryPath = gameDirectoryPath
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

        // Check for box art
        config.HasFrontBoxArt = File.Exists(config.FrontBoxArtPath);
        config.HasBackBoxArt = File.Exists(config.BackBoxArtPath);

        // Scan for capture images (.png files)
        string capturesDirectory = Path.Combine(gameDirectoryPath, "media", "captures");
        if (Directory.Exists(capturesDirectory))
        {
            try
            {
                var displayNames = await ParseDisplayNamesAsync(capturesDirectory);
                var captureFiles = Directory.EnumerateFiles(capturesDirectory, "*.png")
                                            .OrderBy(f => f, StringComparer.OrdinalIgnoreCase);

                foreach (var filePath in captureFiles)
                {
                    string fileName = Path.GetFileName(filePath);
                    displayNames.TryGetValue(fileName, out var displayName);
                    config.CaptureFiles.Add(new MediaFileInfo(filePath, displayName ?? fileName));
                }
            }
            catch (Exception ex)
            {
                AppLogger.Log($"Error scanning for captures in '{capturesDirectory}': {ex.Message}");
            }
        }

        // Scan for videos
        string videosDirectory = Path.Combine(gameDirectoryPath, "media", "videos");
        if (Directory.Exists(videosDirectory))
        {
            try
            {
                var displayNames = await ParseDisplayNamesAsync(videosDirectory);
                var allowedVideoExtensions = new[] { "*.avi", "*.mp4", "*.mpg" };
                var videoFiles = allowedVideoExtensions
                                        .SelectMany(ext => Directory.EnumerateFiles(videosDirectory, ext, SearchOption.TopDirectoryOnly))
                                        .OrderBy(f => f, StringComparer.OrdinalIgnoreCase);

                foreach (var filePath in videoFiles)
                {
                    string fileName = Path.GetFileName(filePath);
                    displayNames.TryGetValue(fileName, out var displayName);
                    config.VideoFiles.Add(new MediaFileInfo(filePath, displayName ?? fileName));
                }
            }
            catch (Exception ex)
            {
                AppLogger.Log($"Error scanning for videos in '{videosDirectory}': {ex.Message}");
            }
        }

        // Process collected ISO paths
        string isoDirectory = Path.Combine(gameDirectoryPath, "isos");
        if (Directory.Exists(isoDirectory) && isoFileNames.Any())
        {
            // Parse file-info.txt for ISOs
            var isoDisplayNames = await ParseDisplayNamesAsync(isoDirectory);

            // Process each ISO file from the config
            foreach (var isoFileName in isoFileNames)
            {
                long actualFileSize = 0; // Declare here to be accessible later

                string fullIsoPath = Path.Combine(isoDirectory, isoFileName);
                if (File.Exists(fullIsoPath))
                {
                    string fileExtension = Path.GetExtension(isoFileName);

                    if (fileExtension.Equals(".cue", StringComparison.OrdinalIgnoreCase))
                    {
                        string binFilePath = Path.ChangeExtension(fullIsoPath, ".bin");
                        if (File.Exists(binFilePath))
                        {
                            actualFileSize = new FileInfo(binFilePath).Length;
                        }
                        else
                        {
                            AppLogger.Log($"Warning: .bin file not found for '{isoFileName}' at '{binFilePath}'. File size will be reported as 0.");
                        }
                    }
                    else // Assume .iso or other direct image file
                    {
                        actualFileSize = new FileInfo(fullIsoPath).Length;
                    }

                    string pngFilePath = Path.ChangeExtension(fullIsoPath, ".png");
                    isoDisplayNames.TryGetValue(isoFileName, out var displayName);

                    config.IsoImages.Add(new DiscImageInfo
                    {
                        ImgFileName = isoFileName,
                        FileSizeInBytes = actualFileSize,
                        PngFilePath = File.Exists(pngFilePath) ? pngFilePath : null,
                        DisplayName = displayName
                    });
                }
            }
        }

        // Scan for floppy disk images (.img files)
        string diskImagesDirectory = Path.Combine(gameDirectoryPath, "disk-images");
        if (Directory.Exists(diskImagesDirectory))
        {
            try
            {
                var displayNames = await ParseDisplayNamesAsync(diskImagesDirectory);

                var imgFiles = Directory.EnumerateFiles(diskImagesDirectory, "*.img")
                                        .OrderBy(f => f, StringComparer.OrdinalIgnoreCase);

                foreach (var imgFilePath in imgFiles)
                {
                    string imgFileName = Path.GetFileName(imgFilePath);
                    string pngFilePath = Path.ChangeExtension(imgFilePath, ".png");
                    var fileInfo = new FileInfo(imgFilePath);

                    displayNames.TryGetValue(imgFileName, out var displayName);

                    config.DiscImages.Add(new DiscImageInfo
                    {
                        ImgFileName = imgFileName,
                        FileSizeInBytes = fileInfo.Length,
                        PngFilePath = File.Exists(pngFilePath) ? pngFilePath : null,
                        DisplayName = displayName
                    });
                }
            }
            catch (Exception ex)
            {
                // Log this error but don't prevent the game from loading
                AppLogger.Log($"Error scanning for floppy disk images in '{diskImagesDirectory}': {ex.Message}");
            }
        }

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