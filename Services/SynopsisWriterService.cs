namespace DOSGameCollection.Services;

public static class SynopsisWriterService
{
    /// <summary>
    /// Represents the result of a synopsis save operation.
    /// </summary>
    public class SynopsisSaveResult
    {
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }
        public string? OriginalContent { get; set; } // Content of the file *before* the save attempt
        public bool ContentChanged { get; set; } // Indicates if the new content was different from the original
    }

    /// <summary>
    /// Attempts to save the new synopsis content to the specified file.
    /// Reads the existing content, compares it, and writes only if changed.
    /// Handles file I/O errors internally and provides a structured result.
    /// </summary>
    /// <param name="synopsisFilePath">The full path to the synopsis.txt file.</param>
    /// <param name="newSynopsis">The new text content for the synopsis.</param>
    /// <returns>A <see cref="SynopsisSaveResult"/> indicating success, error message, and original content.</returns>
    public static async Task<SynopsisSaveResult> TrySaveSynopsisAsync(string synopsisFilePath, string newSynopsis)
    {
        string? originalContent = null;
        bool contentChanged = false;

        try
        {
            // 1. Read original content (if file exists)
            if (File.Exists(synopsisFilePath))
            {
                originalContent = await File.ReadAllTextAsync(synopsisFilePath);
            }
            else
            {
                originalContent = string.Empty; // Treat non-existent file as empty content for comparison
            }

            // 2. Compare new content with original
            contentChanged = !newSynopsis.Equals(originalContent, StringComparison.Ordinal);

            if (!contentChanged)
            {
                // No changes, no need to write
                return new SynopsisSaveResult { Success = true, ContentChanged = false, OriginalContent = originalContent };
            }

            // 3. If content changed, proceed to write
            string? directory = Path.GetDirectoryName(synopsisFilePath);
            if (directory != null)
            {
                Directory.CreateDirectory(directory);
            }
            await File.WriteAllTextAsync(synopsisFilePath, newSynopsis);

            return new SynopsisSaveResult
            {
                Success = true,
                ContentChanged = true,
                OriginalContent = originalContent // Provide original content for potential revert if UI needs it later
            };
        }
        catch (Exception ex)
        {
            AppLogger.Log($"Error saving synopsis file '{synopsisFilePath}': {ex.Message}");
            return new SynopsisSaveResult
            {
                Success = false,
                ErrorMessage = $"Failed to save synopsis: {ex.Message}",
                OriginalContent = originalContent, // Still provide original content for UI to revert
                ContentChanged = contentChanged // Indicate if a change was detected before the error
            };
        }
    }
}
