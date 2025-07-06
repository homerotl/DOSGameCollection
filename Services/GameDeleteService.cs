using DOSGameCollection.Models;
using Microsoft.VisualBasic.FileIO;
using System;
using System.IO;
using System.Threading.Tasks;

namespace DOSGameCollection.Services
{
    /// <summary>
    /// Provides services for deleting games from the library.
    /// </summary>
    public class GameDeleteService
    {
        /// <summary>
        /// Deletes a game's directory and all its contents, sending them to the Recycle Bin.
        /// </summary>
        /// <param name="gameDirectoryPath">The root directory of the game to delete.</param>
        /// <param name="progress">An IProgress object to report progress updates.</param>
        public async Task DeleteGameAsync(string gameDirectoryPath, IProgress<ProgressReport> progress)
        {
            await Task.Run(() =>
            {
                if (!Directory.Exists(gameDirectoryPath))
                {
                    progress.Report(new ProgressReport { IsComplete = true, Message = "Directory not found." });
                    return;
                }

                try
                {
                    progress.Report(new ProgressReport { Message = "Deleting game files to Recycle Bin...", TotalSteps = 1, CurrentStep = 0 });
                    FileSystem.DeleteDirectory(gameDirectoryPath, UIOption.OnlyErrorDialogs, RecycleOption.SendToRecycleBin);
                    progress.Report(new ProgressReport { Message = "Deletion complete.", IsComplete = true, TotalSteps = 1, CurrentStep = 1 });
                }
                catch (Exception ex)
                {
                    throw new IOException($"Failed to delete directory '{gameDirectoryPath}'. Please ensure no files are in use. {ex.Message}", ex);
                }
            });
        }
    }
}

