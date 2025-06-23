namespace DOSGameCollection.Services;

public static class GameDataWriterService
{
    /// <summary>
    /// Updates multiple data points in a specified game.cfg file in a single operation.
    /// </summary>
    /// <param name="cfgFilePath">The full path to the game.cfg file.</param>
    /// <param name="newName">The new name for the game.</param>
    /// <param name="newYear">The new release year for the game.</param>
    /// <param name="newRating">The new parental rating for the game.</param>
    /// <param name="newCommands">The new list of commands to write.</param>
    public static async Task UpdateGameDataAsync(string cfgFilePath, string newName, int? newYear, string? newRating, IEnumerable<string> newCommands)
    {
        if (!File.Exists(cfgFilePath))
        {
            throw new FileNotFoundException("Game configuration file not found.", cfgFilePath);
        }

        List<string> lines = (await File.ReadAllLinesAsync(cfgFilePath)).ToList();

        // Convert UI rating to file format
        string? ratingForFile = FormatTools.EncodeRating(newRating);

        // Update simple key-value properties
        UpdateOrAddProperty(lines, "game.name=", newName);
        UpdateOrAddProperty(lines, "game.release.year=", newYear?.ToString());
        UpdateOrAddProperty(lines, "game.parental.rating=", ratingForFile);

        // Update the [commands] section
        UpdateCommandsSection(lines, newCommands);

        await File.WriteAllLinesAsync(cfgFilePath, lines);
    }

    private static void UpdateOrAddProperty(List<string> lines, string prefix, string? value)
    {
        int propertyIndex = lines.FindIndex(l => l.Trim().StartsWith(prefix, StringComparison.OrdinalIgnoreCase));

        if (string.IsNullOrEmpty(value))
        {
            // If the value is null or empty, remove the line from the file if it exists.
            if (propertyIndex != -1)
            {
                lines.RemoveAt(propertyIndex);
            }
        }
        else
        {
            // If the value is provided, update the existing line or add a new one.
            if (propertyIndex != -1)
            {
                lines[propertyIndex] = $"{prefix}{value}";
            }
            else
            {
                lines.Insert(0, $"{prefix}{value}"); // Add to top of file if not found.
            }
        }
    }

    private static void UpdateCommandsSection(List<string> lines, IEnumerable<string> newCommands)
    {
        const string commandsHeader = "[commands]";
        int commandsHeaderIndex = lines.FindIndex(l => l.Trim().Equals(commandsHeader, StringComparison.OrdinalIgnoreCase));
        var validNewCommands = newCommands.Where(c => !string.IsNullOrWhiteSpace(c)).ToList();

        if (commandsHeaderIndex == -1)
        {
            // If section doesn't exist, add it to the end.
            lines.Add(string.Empty);
            lines.Add(commandsHeader);
            lines.AddRange(validNewCommands);
        }
        else
        {
            // If section exists, find its end and replace the content.
            int firstCommandIndex = commandsHeaderIndex + 1;
            int commandsToRemoveCount = 0;
            for (int i = firstCommandIndex; i < lines.Count; i++)
            {
                if (string.IsNullOrWhiteSpace(lines[i]) || lines[i].Trim().StartsWith("[")) { break; }
                commandsToRemoveCount++;
            }
            if (commandsToRemoveCount > 0) { lines.RemoveRange(firstCommandIndex, commandsToRemoveCount); }
            lines.InsertRange(firstCommandIndex, validNewCommands);
        }
    }
}
