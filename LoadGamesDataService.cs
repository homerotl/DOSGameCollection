using DOSGameCollection.Models;

namespace DOSGameCollection;

public class LoadGamesDataService
{
    // This method simulates loading data and reports progress
    // It takes an IProgress<ProgressReport> to allow reporting updates

    public async Task<List<GameConfiguration>> LoadDataAsync(string libraryBasePath, IProgress<ProgressReport>? progress)
    {
        List<GameConfiguration> gameConfigurations = new List<GameConfiguration>();

        // 1. Validate the directory path input to ensure it's not null or empty.
        if (string.IsNullOrWhiteSpace(libraryBasePath))
        {
            throw new ArgumentException("Library base path cannot be null or empty.", nameof(libraryBasePath));
        }

        // 2. Verify that the specified directory actually exists on the file system.
        if (!Directory.Exists(libraryBasePath))
        {
            throw new DirectoryNotFoundException($"The library base directory '{libraryBasePath}' was not found.");
        }

        // 3. Retrieve all subdirectories within the library base path. Each subdirectory is a potential game.
        //    Task.Run is used to offload the synchronous Directory.GetDirectories operation to a thread pool thread,
        //    preventing the UI thread from freezing during potentially long directory scans.
        string[] gameDirectories = null;
        try
        {
            gameDirectories = await Task.Run(() => Directory.GetDirectories(libraryBasePath, "*", SearchOption.TopDirectoryOnly));
        }
        catch (UnauthorizedAccessException ex)
        {
            // Propagate access denial exceptions.
            throw new UnauthorizedAccessException($"Access to library directory '{libraryBasePath}' is denied. {ex.Message}", ex);
        }
        catch (IOException ex)
        {
            // Propagate general I/O errors.
            throw new IOException($"An I/O error occurred while accessing library directory '{libraryBasePath}'. {ex.Message}", ex);
        }

        int totalGamesToProcess = gameDirectories.Length;
        int processedCount = 0;

        // 4. Handle the scenario where no game directories are found.
        if (totalGamesToProcess == 0)
        {
            if (progress != null)
            {
                // Report immediate completion with a specific message indicating no files were found.
                progress.Report(new ProgressReport
                {
                    CurrentStep = 0,
                    TotalSteps = 0,
                    Message = $"No game directories found in '{libraryBasePath}'.",
                    IsComplete = true
                });
            }
            return new List<GameConfiguration>(); // Return an empty list of configurations.
        }

        // 5. Iterate through each discovered game directory, look for "game.cfg", parse its content, and report progress.
        foreach (string gameDirPath in gameDirectories)
        {
            processedCount++;
            string gameDirName = Path.GetFileName(gameDirPath); // Get the name of the game directory
            string cfgFilePath = Path.Combine(gameDirPath, "game.cfg");

            if (!File.Exists(cfgFilePath))
            {
                Console.WriteLine($"Skipping directory {gameDirName}: 'game.cfg' not found.");
                // Optionally report skipped directory
                if (progress != null)
                {
                    progress.Report(new ProgressReport
                    {
                        CurrentStep = processedCount -1,
                        TotalSteps = totalGamesToProcess,
                        Message = $"Skipping {gameDirName}: game.cfg not found.",
                        IsComplete = false
                    });
                }
                await Task.Delay(10); // Small delay if skipping
                continue; // Move to the next directory
            }

            // Report current progress before starting the parsing of the current file.
            if (progress != null)
            {
                progress.Report(new ProgressReport
                {
                    CurrentStep = processedCount - 1, // Use 0-based indexing for internal step count
                    TotalSteps = totalGamesToProcess,
                    Message = $"Processing game: {gameDirName} ({processedCount} of {totalGamesToProcess})",
                    IsComplete = false
                });
            }

            GameConfiguration? gameConfig = null;
            try
            {
                gameConfig = await CfgFileParser.ParseCfgFileAsync(cfgFilePath, gameDirPath);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing game in {gameDirName}: {ex.Message}");
                gameConfig = new GameConfiguration
                {
                    GameName = $"[Error: {gameDirName}]",
                    GameDirectoryPath = gameDirPath // Set directory path for context, even on error
                };
            }

            // Add the successfully parsed or error-placeholder configuration to the list.
            if (gameConfig != null)
            {
                gameConfigurations.Add(gameConfig);
            }

            // Introduce a small artificial delay. This is useful for visually observing progress
            // in very fast operations; remove or adjust in production for performance.
            await Task.Delay(10);
        }

        // 6. Send a final progress report to indicate that the entire operation is complete.
        if (progress != null)
        {
            progress.Report(new ProgressReport
            {
                CurrentStep = totalGamesToProcess,
                TotalSteps = totalGamesToProcess,
                Message = $"Finished processing {processedCount} of {totalGamesToProcess} game(s).",
                IsComplete = true
            });
        }
        return gameConfigurations;
    }
}
