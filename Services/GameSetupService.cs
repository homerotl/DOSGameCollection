using DOSGameCollection.Models;

namespace DOSGameCollection.Services;

/// <summary>
/// Provides services for setting up new games in the library.
/// </summary>
public class GameSetupService
{
    public record DiskSetupResult(List<string> DestinationPaths, bool FilesWereCopied);

    /// <summary>
    /// Sets up a new game by creating its directory structure, copying a default config, and copying the game files.
    /// </summary>
    /// <param name="targetDirectory">The root directory for the new game in the library.</param>
    /// <param name="gameName">The name of the game.</param>
    /// <param name="sourceDirectory">The directory containing the source game files to be copied.</param>    
    /// <param name="progress">An IProgress object to report progress updates.</param>
    public bool SetupNewGameFromFiles(string gameName, string targetDirectory, string sourceDirectory, IProgress<GameSetupProgressReport> progress)
    {
        progress.Report(new GameSetupProgressReport { Message = "Creating directory structure...", Percentage = 0 });

        // 1. Create the required directory structure.
        // Directory.CreateDirectory is safe to call even if the directories already exist.
        string gameFilesRootPath = Path.Combine(targetDirectory, "game-files", "GAME");
        Directory.CreateDirectory(gameFilesRootPath);

        string mediaCapturesPath = Path.Combine(targetDirectory, "media", "captures");
        Directory.CreateDirectory(mediaCapturesPath);

        progress.Report(new GameSetupProgressReport { Message = "Copying configuration templates...", Percentage = 0 });

        // 2. Copy the default dosbox-staging.conf template.
        string templateConfPath = Path.Combine(AppContext.BaseDirectory, "file-templates", "dosbox-staging.conf");
        string targetConfPath = Path.Combine(targetDirectory, "dosbox-staging.conf");
        File.Copy(templateConfPath, targetConfPath, true); // Overwrite if it somehow exists.

        // 3. Create a basic game.cfg file.
        string gameCfgPath = Path.Combine(targetDirectory, "game.cfg");
        string gameCfgContent = $"game.name={gameName}{Environment.NewLine}{Environment.NewLine}[commands]{Environment.NewLine}";
        File.WriteAllText(gameCfgPath, gameCfgContent);

        // Check if the source directory is already the target game-files directory.
        if (Path.GetFullPath(sourceDirectory).Equals(Path.GetFullPath(gameFilesRootPath), StringComparison.OrdinalIgnoreCase))
        {
            progress.Report(new GameSetupProgressReport { Message = "Files are already in the destination. Skipping copy.", Percentage = 100 });
            return false; // Files were not copied
        }

        progress.Report(new GameSetupProgressReport { Message = "Calculating total file size...", Percentage = 0 });
        long totalSize = CalculateDirectorySize(new DirectoryInfo(sourceDirectory));
        long totalBytesCopied = 0;

        // 4. Recursively copy all files and subdirectories from the source to the target game path.
        CopyDirectory(sourceDirectory, gameFilesRootPath, totalSize, ref totalBytesCopied, progress, true);

        progress.Report(new GameSetupProgressReport { Message = "Setup complete.", Percentage = 100 });
        return true; // Files were copied
    }

    /// <summary>
    /// Sets up the basic structure for a new game to be installed from diskettes.
    /// </summary>
    /// <param name="gameName">The name of the game.</param>
    /// <param name="targetDirectory">The root directory for the new game in the library.</param>
    /// <param name="sourceDiskettePaths">A list of paths to the source diskette image files.</param>
    /// <param name="progress">An IProgress object to report progress updates.</param>
    /// <returns>A result object containing the destination paths and a flag indicating if files were copied.</returns>
    public DiskSetupResult SetupNewGameFromDiskettes(string gameName, string targetDirectory, IEnumerable<string> sourceDiskettePaths, IProgress<GameSetupProgressReport> progress)
    {
        progress.Report(new GameSetupProgressReport { Message = "Creating directory structure...", Percentage = 0 });

        // 1. Create the required directory structure.
        Directory.CreateDirectory(Path.Combine(targetDirectory, "game-files"));
        Directory.CreateDirectory(Path.Combine(targetDirectory, "media", "captures"));
        string diskImagesPath = Path.Combine(targetDirectory, "disk-images");
        Directory.CreateDirectory(diskImagesPath);

        progress.Report(new GameSetupProgressReport { Message = "Copying configuration templates...", Percentage = 5 });

        // 2. Copy the default dosbox-staging.conf template.
        string templateConfPath = Path.Combine(AppContext.BaseDirectory, "file-templates", "dosbox-staging.conf");
        string targetConfPath = Path.Combine(targetDirectory, "dosbox-staging.conf");
        File.Copy(templateConfPath, targetConfPath, true);

        // 3. Create a basic game.cfg file.
        string gameCfgPath = Path.Combine(targetDirectory, "game.cfg");
        string gameCfgContent = $"game.name={gameName}{Environment.NewLine}{Environment.NewLine}[commands]{Environment.NewLine}";
        File.WriteAllText(gameCfgPath, gameCfgContent);

        // Check if all source files are already in the target directory.
        bool allFilesAreInDestination = sourceDiskettePaths.All(p =>
            Path.GetFullPath(Path.GetDirectoryName(p) ?? "")
                .Equals(Path.GetFullPath(diskImagesPath), StringComparison.OrdinalIgnoreCase)
        );

        if (allFilesAreInDestination)
        {
            progress.Report(new GameSetupProgressReport { Message = "Diskette images are already in the destination. Skipping copy.", Percentage = 100 });
            return new DiskSetupResult(sourceDiskettePaths.ToList(), false);
        }

        // 4. Copy disk images
        var destinationDiskettePaths = new List<string>();
        var sourceFiles = sourceDiskettePaths.Select(p => new FileInfo(p)).ToList();
        long totalSize = sourceFiles.Sum(f => f.Length);
        long totalBytesCopied = 0;

        foreach (var sourceFile in sourceFiles)
        {
            string destinationFile = Path.Combine(diskImagesPath, sourceFile.Name);
            File.Copy(sourceFile.FullName, destinationFile, true);
            destinationDiskettePaths.Add(destinationFile);

            totalBytesCopied += sourceFile.Length;
            int percentage = (totalSize > 0) ? (int)((double)totalBytesCopied * 100 / totalSize) : 0;

            progress.Report(new GameSetupProgressReport
            {
                Message = $"Copying: {sourceFile.Name}",
                // Scale copy progress from 10% to 100% of the overall operation
                Percentage = Math.Clamp(10 + percentage * 90 / 100, 10, 100)
            });
        }

        progress.Report(new GameSetupProgressReport { Message = "Setup complete.", Percentage = 100 });
        return new DiskSetupResult(destinationDiskettePaths, true);
    }

    private long CalculateDirectorySize(DirectoryInfo dir)
    {
        long size = dir.GetFiles().Sum(fi => fi.Length);
        size += dir.GetDirectories().Sum(di => CalculateDirectorySize(di));
        return size;
    }

    private void CopyDirectory(string sourceDir, string destinationDir, long totalSize, ref long totalBytesCopied, IProgress<GameSetupProgressReport> progress, bool convertToUppercase = false)
    {
        var dir = new DirectoryInfo(sourceDir);
        if (!dir.Exists) throw new DirectoryNotFoundException($"Source directory not found: {dir.FullName}");

        Directory.CreateDirectory(destinationDir);

        foreach (FileInfo file in dir.GetFiles())
        {
            string targetFileName = convertToUppercase ? file.Name.ToUpperInvariant() : file.Name;
            string targetFilePath = Path.Combine(destinationDir, targetFileName);
            file.CopyTo(targetFilePath, true);

            totalBytesCopied += file.Length;
            int percentage = (totalSize > 0) ? (int)((double)totalBytesCopied * 100 / totalSize) : 0;

            progress.Report(new GameSetupProgressReport
            {
                Message = $"Copying: {file.Name}",
                Percentage = Math.Clamp(percentage, 0, 100)
            });
        }

        foreach (DirectoryInfo subDir in dir.GetDirectories())
        {
            string targetSubDirName = convertToUppercase ? subDir.Name.ToUpperInvariant() : subDir.Name;
            CopyDirectory(subDir.FullName, Path.Combine(destinationDir, targetSubDirName), totalSize, ref totalBytesCopied, progress, convertToUppercase);
        }
    }
    
    /// <summary>
    /// Sets up the basic structure for a new game to be installed from CD-ROM images.
    /// </summary>
    /// <param name="gameName">The name of the game.</param>
    /// <param name="targetDirectory">The root directory for the new game in the library.</param>
    /// <param name="sourceCdRomPaths">A list of paths to the source CD-ROM image files (.iso, .cue).</param>
    /// <param name="progress">An IProgress object to report progress updates.</param>
    /// <returns>A result object containing the destination paths and a flag indicating if files were copied.</returns>
    public DiskSetupResult SetupNewGameFromCdRoms(string gameName, string targetDirectory, IEnumerable<string> sourceCdRomPaths, IProgress<GameSetupProgressReport> progress)
    {
        progress.Report(new GameSetupProgressReport { Message = "Creating directory structure...", Percentage = 0 });

        // 1. Create the required directory structure.
        Directory.CreateDirectory(Path.Combine(targetDirectory, "game-files"));
        Directory.CreateDirectory(Path.Combine(targetDirectory, "media", "captures"));
        string isosPath = Path.Combine(targetDirectory, "isos");
        Directory.CreateDirectory(isosPath);

        progress.Report(new GameSetupProgressReport { Message = "Copying configuration templates...", Percentage = 5 });

        // 2. Copy the default dosbox-staging.conf template.
        string templateConfPath = Path.Combine(AppContext.BaseDirectory, "file-templates", "dosbox-staging.conf");
        string targetConfPath = Path.Combine(targetDirectory, "dosbox-staging.conf");
        File.Copy(templateConfPath, targetConfPath, true);

        // 3. Create a basic game.cfg file.
        string gameCfgPath = Path.Combine(targetDirectory, "game.cfg");
        string gameCfgContent = $"game.name={gameName}{Environment.NewLine}{Environment.NewLine}[commands]{Environment.NewLine}";
        File.WriteAllText(gameCfgPath, gameCfgContent);

        // Check if all source files are already in the target directory.
        bool allFilesAreInDestination = sourceCdRomPaths.All(p =>
            Path.GetFullPath(Path.GetDirectoryName(p) ?? "")
                .Equals(Path.GetFullPath(isosPath), StringComparison.OrdinalIgnoreCase)
        );

        if (allFilesAreInDestination)
        {
            progress.Report(new GameSetupProgressReport { Message = "CD-ROM images are already in the destination. Skipping copy.", Percentage = 100 });
            return new DiskSetupResult(sourceCdRomPaths.ToList(), false);
        }

        // 4. Copy CD-ROM images, including .bin files for .cue sheets
        var destinationCdRomPaths = new List<string>();
        var sourceFilesToCopy = new List<FileInfo>();

        foreach (var path in sourceCdRomPaths)
        {
            var fileInfo = new FileInfo(path);
            sourceFilesToCopy.Add(fileInfo);
            if (fileInfo.Extension.Equals(".cue", StringComparison.OrdinalIgnoreCase) && File.Exists(Path.ChangeExtension(path, ".bin")))
            {
                sourceFilesToCopy.Add(new FileInfo(Path.ChangeExtension(path, ".bin")));
            }
        }

        long totalSize = sourceFilesToCopy.Sum(f => f.Length);
        long totalBytesCopied = 0;

        foreach (var sourceFile in sourceFilesToCopy)
        {
            string destinationFile = Path.Combine(isosPath, sourceFile.Name);
            File.Copy(sourceFile.FullName, destinationFile, true);
            if (sourceCdRomPaths.Contains(sourceFile.FullName, StringComparer.OrdinalIgnoreCase))
            {
                destinationCdRomPaths.Add(destinationFile);
            }
            totalBytesCopied += sourceFile.Length;
            int percentage = (totalSize > 0) ? (int)((double)totalBytesCopied * 100 / totalSize) : 0;

            progress.Report(new GameSetupProgressReport
            {
                Message = $"Copying: {sourceFile.Name}",
                // Scale copy progress from 10% to 100% of the overall operation
                Percentage = Math.Clamp(10 + percentage * 90 / 100, 10, 100)
            });
        }

        progress.Report(new GameSetupProgressReport { Message = "Setup complete.", Percentage = 100 });
        return new DiskSetupResult(destinationCdRomPaths, true);
    }
}
