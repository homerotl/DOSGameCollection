namespace DOSGameCollection;

public static class GameDataWriterService
{
    /// <summary>
    /// Updates the 'game.name' property in a specified game.cfg file.
    /// If the property exists, it's updated. If not, it's added to the top of the file.
    /// </summary>
    /// <param name="cfgFilePath">The full path to the game.cfg file.</param>
    /// <param name="newName">The new name for the game.</param>
    public static async Task UpdateGameNameAsync(string cfgFilePath, string newName)
    {
        if (!File.Exists(cfgFilePath))
        {
            throw new FileNotFoundException("Game configuration file not found.", cfgFilePath);
        }

        var lines = (await File.ReadAllLinesAsync(cfgFilePath)).ToList();
        bool nameFound = false;
        const string gameNamePrefix = "game.name=";

        for (int i = 0; i < lines.Count; i++)
        {
            if (lines[i].Trim().StartsWith(gameNamePrefix, StringComparison.OrdinalIgnoreCase))
            {
                lines[i] = $"{gameNamePrefix}{newName}";
                nameFound = true;
                break;
            }
        }

        if (!nameFound) { lines.Insert(0, $"{gameNamePrefix}{newName}"); }

        await File.WriteAllLinesAsync(cfgFilePath, lines);
    }
}

