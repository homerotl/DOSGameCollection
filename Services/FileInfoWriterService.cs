using System.Text;

namespace DOSGameCollection.Services;

public static class FileInfoWriterService
{
    /// <summary>
    /// Updates or adds a display name for a given media file in its corresponding "file-info.txt".
    /// </summary>
    /// <param name="mediaFilePath">The full path to the media file (e.g., image, video).</param>
    /// <param name="newDisplayName">The new display name for the file.</param>
    public static async Task UpdateDisplayNameAsync(string mediaFilePath, string newDisplayName)
    {
        string? directory = Path.GetDirectoryName(mediaFilePath);
        if (string.IsNullOrEmpty(directory))
        {
            throw new ArgumentException("Could not determine directory from media file path.", nameof(mediaFilePath));
        }

        string infoFilePath = Path.Combine(directory, "file-info.txt");
        string mediaFileName = Path.GetFileName(mediaFilePath);

        List<string> lines = File.Exists(infoFilePath)
            ? (await File.ReadAllLinesAsync(infoFilePath, Encoding.UTF8)).ToList()
            : [];

        bool entryUpdated = false;
        for (int i = 0; i < lines.Count; i++)
        {
            string line = lines[i];
            // Skip comments and empty lines
            if (string.IsNullOrWhiteSpace(line) || line.TrimStart().StartsWith("#") || line.TrimStart().StartsWith(";"))
            {
                continue;
            }

            var parts = line.Split(new[] { ',' }, 2);
            if (parts.Length == 2 && parts[0].Trim().Equals(mediaFileName, StringComparison.OrdinalIgnoreCase))
            {
                // Found the entry. If the new name is the same as the filename, remove the line.
                if (newDisplayName.Equals(mediaFileName, StringComparison.OrdinalIgnoreCase))
                {
                    lines.RemoveAt(i);
                }
                else
                {
                    lines[i] = $"{mediaFileName},{newDisplayName}";
                }
                entryUpdated = true;
                break;
            }
        }

        // If no entry was found and the new name is not the default, add a new line.
        if (!entryUpdated && !newDisplayName.Equals(mediaFileName, StringComparison.OrdinalIgnoreCase))
        {
            lines.Add($"{mediaFileName},{newDisplayName}");
        }

        // If the file is now effectively empty, delete it. Otherwise, write the changes.
        if (lines.All(string.IsNullOrWhiteSpace))
        {
            if (File.Exists(infoFilePath)) File.Delete(infoFilePath);
        }
        else
        {
            await File.WriteAllLinesAsync(infoFilePath, lines, Encoding.UTF8);
        }
    }
}