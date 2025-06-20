namespace DOSGameCollection;

public static class CfgFileParser
{
    private const string GameNamePrefix = "game.name=";
    private const string IsoSectionHeader = "[isos]";
    private const string CommandsSectionHeader = "[commands]";

    private enum ParsingState { None, Isos, Commands }

    public static async Task<GameConfiguration?> ParseCfgFileAsync(string cfgFilePath, string gameDirectoryPath)
    {
        if (!File.Exists(cfgFilePath))
        {
            Console.WriteLine($"Warning: File not found for parsing: {cfgFilePath}");
            return null;
        }

        // GameDirectoryPath is required by GameConfiguration constructor.
        // ConfigFilePath, MountCPath, and DosboxConfPath properties in GameConfiguration
        // will be derived from this gameDirectoryPath.
        GameConfiguration config = new GameConfiguration
        {
            GameDirectoryPath = gameDirectoryPath
        };

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

                switch (currentState)
                {
                    case ParsingState.Isos:
                        config.IsoImagePaths.Add(trimmedLine);
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
            Console.WriteLine($"Error parsing {cfgFilePath}: Access denied. {ex.Message}");
            throw;
        }
        catch (IOException ex)
        {
            Console.WriteLine($"Error parsing {cfgFilePath}: I/O error. {ex.Message}");
            throw;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An unexpected error occurred while parsing {cfgFilePath}: {ex.Message}");
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

        return config;
    }
}