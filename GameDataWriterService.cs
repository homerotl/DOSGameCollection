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

    /// <summary>
    /// Updates the [commands] section in a specified game.cfg file.
    /// If the section exists, its contents are replaced. If not, the section is added to the end of the file.
    /// </summary>
    /// <param name="cfgFilePath">The full path to the game.cfg file.</param>
    /// <param name="newCommands">The new list of commands to write.</param>
    public static async Task UpdateGameCommandsAsync(string cfgFilePath, IEnumerable<string> newCommands)
    {
        if (!File.Exists(cfgFilePath))
        {
            throw new FileNotFoundException("Game configuration file not found.", cfgFilePath);
        }

        var lines = (await File.ReadAllLinesAsync(cfgFilePath)).ToList();
        const string commandsHeader = "[commands]";

        int commandsHeaderIndex = -1;
        for (int i = 0; i < lines.Count; i++)
        {
            if (lines[i].Trim().Equals(commandsHeader, StringComparison.OrdinalIgnoreCase))
            {
                commandsHeaderIndex = i;
                break;
            }
        }

        var validNewCommands = newCommands.Where(c => !string.IsNullOrWhiteSpace(c)).ToList();

        if (commandsHeaderIndex == -1)
        {
            lines.Add(string.Empty);
            lines.Add(commandsHeader);
            lines.AddRange(validNewCommands);
        }
        else
        {
            int firstCommandIndex = commandsHeaderIndex + 1;
            int commandsToRemoveCount = 0;

            for (int i = firstCommandIndex; i < lines.Count; i++)
            {
                if (string.IsNullOrEmpty(lines[i].Trim())) { break; }
                commandsToRemoveCount++;
            }

            if (commandsToRemoveCount > 0) { lines.RemoveRange(firstCommandIndex, commandsToRemoveCount); }
            lines.InsertRange(firstCommandIndex, validNewCommands);
        }

        await File.WriteAllLinesAsync(cfgFilePath, lines);
    }
}
