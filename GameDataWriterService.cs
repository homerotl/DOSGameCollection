namespace DOSGameCollection;

public static class GameDataWriterService
{
    /// <summary>
    /// Updates the 'game.name' property in a specified game.cfg file.
    /// And updates the [commands] section in a specified game.cfg file.
    /// If properties/sections exist, they are updated. If not, they are added.
    /// </summary>
    /// <param name="cfgFilePath">The full path to the game.cfg file.</param>
    /// <param name="newName">The new name for the game.</param>
    /// <param name="newYear">The new release year for the game.</param>
    /// <param name="newCommands">The new list of commands to write.</param>
    public static async Task UpdateGameDataAsync(string cfgFilePath, string newName, int? newYear, IEnumerable<string> newCommands)
    {
        if (!File.Exists(cfgFilePath))
        {
            throw new FileNotFoundException("Game configuration file not found.", cfgFilePath);
        }

        var lines = (await File.ReadAllLinesAsync(cfgFilePath)).ToList();

        // --- Update game.name and find its index ---
        int nameIndex = -1;
        const string gameNamePrefix = "game.name=";
        for (int i = 0; i < lines.Count; i++)
        {
            if (lines[i].TrimStart().StartsWith(gameNamePrefix, StringComparison.OrdinalIgnoreCase))
            {
                lines[i] = $"{gameNamePrefix}{newName}";
                nameIndex = i;
                break;
            }
        }
        if (nameIndex == -1)
        {
            lines.Insert(0, $"{gameNamePrefix}{newName}");
            nameIndex = 0;
        }

        // --- Update game.release.year ---
        int yearIndex = -1;
        const string yearPrefix = "game.release.year=";
        for (int i = 0; i < lines.Count; i++)
        {
            if (lines[i].TrimStart().StartsWith(yearPrefix, StringComparison.OrdinalIgnoreCase))
            {
                yearIndex = i;
                break;
            }
        }

        if (newYear.HasValue)
        {
            string yearLine = $"{yearPrefix}{newYear.Value}";
            if (yearIndex != -1)
            {
                lines[yearIndex] = yearLine;
            }
            else
            {
                // Insert after game.name
                lines.Insert(nameIndex + 1, yearLine);
            }
        }
        else // newYear is null, so remove the line if it exists
        {
            if (yearIndex != -1)
            {
                lines.RemoveAt(yearIndex);
            }
        }

        // --- Update [commands] section ---
        const string commandsHeader = "[commands]";

        int commandsHeaderIndex = -1;
        for (int i = 0; i < lines.Count; i++)
        {
            if (lines[i].TrimStart().Equals(commandsHeader, StringComparison.OrdinalIgnoreCase))
            {
                commandsHeaderIndex = i;
                break;
            }
        }

        // Filter out empty or whitespace-only commands
        var validNewCommands = newCommands.Where(c => !string.IsNullOrWhiteSpace(c)).ToList();

        if (commandsHeaderIndex == -1)
        {
            // Add commands section at the end if not found
            lines.Add(string.Empty);
            lines.Add(commandsHeader);
            lines.AddRange(validNewCommands);
        }
        else
        {
            // Remove existing commands in the section
            int firstCommandIndex = commandsHeaderIndex + 1;
            int commandsToRemoveCount = 0;

            // Find the end of the commands section
            // It ends at the next section header or end of file.
            for (int i = firstCommandIndex; i < lines.Count; i++)
            {
                string trimmedLine = lines[i].TrimStart();
                if (string.IsNullOrEmpty(trimmedLine) || trimmedLine.StartsWith("[", StringComparison.Ordinal) || trimmedLine.StartsWith(";", StringComparison.Ordinal) || trimmedLine.StartsWith("#", StringComparison.Ordinal))
                {
                    break; // End of commands section or start of new section/comment
                }
                commandsToRemoveCount++;
            }

            if (commandsToRemoveCount > 0) { lines.RemoveRange(firstCommandIndex, commandsToRemoveCount); }
            lines.InsertRange(firstCommandIndex, validNewCommands);
        }

        await File.WriteAllLinesAsync(cfgFilePath, lines);
    }
}
