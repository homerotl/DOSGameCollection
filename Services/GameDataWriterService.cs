namespace DOSGameCollection.Services;

public static class GameDataWriterService
{
    /// <summary>
    /// Updates multiple data points in a specified game.cfg file in a single operation.
    /// </summary>
    /// <param name="cfgFilePath">The full path to the game.cfg file.</param>
    /// <param name="newName">The new name for the game.</param>
    /// <param name="newYear">The new release year.</param>
    /// <param name="newRating">The new parental rating.</param>
    /// <param name="newDeveloper">The new developer.</param>
    /// <param name="newPublisher">The new publisher.</param>
    /// <param name="newCommands">The new list of commands to write.</param>
    public static async Task UpdateGameDataAsync(string cfgFilePath, string newName, int? newYear, string? newRating, string? newDeveloper, string? newPublisher, IEnumerable<string> newCommands)
    {
        if (!File.Exists(cfgFilePath))
        {
            throw new FileNotFoundException("Game configuration file not found.", cfgFilePath);
        }

        var lines = (await File.ReadAllLinesAsync(cfgFilePath)).ToList();

        // Remove all existing game.* properties. We will re-add them in order.
        lines.RemoveAll(l => l.TrimStart().StartsWith("game.", StringComparison.OrdinalIgnoreCase));

        // Prepare the new properties to be inserted at the top.
        var newProperties = new List<string>();
        newProperties.Add($"game.name={newName}");

        string? ratingForFile = FormatTools.EncodeRating(newRating);
        if (!string.IsNullOrEmpty(ratingForFile)) newProperties.Add($"game.parental.rating={ratingForFile}");
        if (!string.IsNullOrEmpty(newPublisher)) newProperties.Add($"game.publisher={newPublisher}");
        if (!string.IsNullOrEmpty(newDeveloper)) newProperties.Add($"game.developer={newDeveloper}");
        if (newYear.HasValue) newProperties.Add($"game.release.year={newYear.Value}");

        // Find the first non-empty, non-comment line to insert a blank line before if needed.
        int firstContentIndex = lines.FindIndex(l => !string.IsNullOrWhiteSpace(l) && !l.TrimStart().StartsWith("#") && !l.TrimStart().StartsWith(";"));
        if (firstContentIndex != -1 && newProperties.Any())
        {
            lines.Insert(firstContentIndex, string.Empty);
        }

        // Insert the new properties at the beginning of the file content.
        lines.InsertRange(0, newProperties);

        // Update the [commands] section
        UpdateCommandsSection(lines, newCommands);

        await File.WriteAllLinesAsync(cfgFilePath, lines);
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
