using System.Text;

namespace DOSGameCollection.Services;

public static class GameDataWriterService
{
    private const string GameNamePrefix = "game.name=";
    private const string GameReleaseYearPrefix = "game.release.year=";
    private const string GameDeveloperPrefix = "game.developer=";
    private const string GamePublisherPrefix = "game.publisher=";
    private const string ParentalRatingPrefix = "game.parental.rating=";
    private const string CommandsSectionHeader = "[commands]";
    private const string SetupCommandsSectionHeader = "[setup-commands]";

    public static async Task UpdateGameDataAsync(
        string configFilePath,
        string newName,
        int? newYear,
        string? newRating,
        string newDeveloper,
        string newPublisher,
        List<string> newCommands,
        List<string> newSetupCommands)
    {
        string[] originalLines = File.Exists(configFilePath) ? await File.ReadAllLinesAsync(configFilePath) : [];
        var newLines = new List<string>();

        var propertiesWritten = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        bool inCommandsSection = false;
        bool inSetupCommandsSection = false;

        foreach (var line in originalLines)
        {
            var trimmedLine = line.Trim();

            // Handle section transitions
            if (trimmedLine.StartsWith("["))
            {
                inCommandsSection = trimmedLine.Equals(CommandsSectionHeader, StringComparison.OrdinalIgnoreCase);
                inSetupCommandsSection = trimmedLine.Equals(SetupCommandsSectionHeader, StringComparison.OrdinalIgnoreCase);

                if (inCommandsSection)
                {
                    newLines.Add(CommandsSectionHeader);
                    newLines.AddRange(newCommands.Where(c => !string.IsNullOrWhiteSpace(c)));
                    propertiesWritten.Add(CommandsSectionHeader);
                }
                else if (inSetupCommandsSection)
                {
                    newLines.Add(SetupCommandsSectionHeader);
                    newLines.AddRange(newSetupCommands.Where(c => !string.IsNullOrWhiteSpace(c)));
                    propertiesWritten.Add(SetupCommandsSectionHeader);
                }
                else
                {
                    // Preserve other sections
                    newLines.Add(line);
                }
                continue;
            }

            // Skip lines within sections we're replacing
            if (inCommandsSection || inSetupCommandsSection)
            {
                continue;
            }

            // Handle properties by replacing or removing them
            if (UpdateProperty(line, GameNamePrefix, newName, newLines, propertiesWritten)) continue;
            if (UpdateProperty(line, GameReleaseYearPrefix, newYear?.ToString(), newLines, propertiesWritten)) continue;
            if (UpdateProperty(line, GameDeveloperPrefix, newDeveloper, newLines, propertiesWritten)) continue;
            if (UpdateProperty(line, GamePublisherPrefix, newPublisher, newLines, propertiesWritten)) continue;
            if (UpdateProperty(line, ParentalRatingPrefix, FormatTools.EncodeRating(newRating), newLines, propertiesWritten)) continue;

            // Preserve comments, blank lines, etc.
            newLines.Add(line);
        }

        // Add any properties or sections that were not found in the original file
        AppendIfMissing(GameNamePrefix, newName, newLines, propertiesWritten);
        AppendIfMissing(GameReleaseYearPrefix, newYear?.ToString(), newLines, propertiesWritten);
        AppendIfMissing(GameDeveloperPrefix, newDeveloper, newLines, propertiesWritten);
        AppendIfMissing(GamePublisherPrefix, newPublisher, newLines, propertiesWritten);
        AppendIfMissing(ParentalRatingPrefix, FormatTools.EncodeRating(newRating), newLines, propertiesWritten);

        if (!propertiesWritten.Contains(CommandsSectionHeader) && newCommands.Any(c => !string.IsNullOrWhiteSpace(c)))
        {
            if (newLines.Any() && !string.IsNullOrWhiteSpace(newLines.Last())) newLines.Add("");
            newLines.Add(CommandsSectionHeader);
            newLines.AddRange(newCommands.Where(c => !string.IsNullOrWhiteSpace(c)));
        }
        if (!propertiesWritten.Contains(SetupCommandsSectionHeader) && newSetupCommands.Any(c => !string.IsNullOrWhiteSpace(c)))
        {
            if (newLines.Any() && !string.IsNullOrWhiteSpace(newLines.Last())) newLines.Add("");
            newLines.Add(SetupCommandsSectionHeader);
            newLines.AddRange(newSetupCommands.Where(c => !string.IsNullOrWhiteSpace(c)));
        }

        await File.WriteAllLinesAsync(configFilePath, newLines, Encoding.UTF8);
    }

    private static bool UpdateProperty(string line, string prefix, string? newValue, List<string> newLines, HashSet<string> written)
    {
        if (line.Trim().StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
        {
            if (!written.Contains(prefix))
            {
                if (!string.IsNullOrEmpty(newValue))
                {
                    newLines.Add($"{prefix}{newValue}");
                }
                // Mark as written even if we remove it (newValue is null/empty)
                written.Add(prefix);
            }
            return true; // Line handled (either replaced, removed, or skipped as duplicate)
        }
        return false;
    }

    private static void AppendIfMissing(string prefix, string? value, List<string> newLines, HashSet<string> written)
    {
        if (!written.Contains(prefix) && !string.IsNullOrEmpty(value))
        {
            newLines.Add($"{prefix}{value}");
        }
    }
}
