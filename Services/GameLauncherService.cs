using DOSGameCollection.Models;
using System.Diagnostics;

namespace DOSGameCollection.Services;

public static class GameLauncherService
{
    public static void LaunchGame(GameConfiguration gameConfig, string? dosboxExePath, List<string> commandsToRun, IWin32Window owner)
    {
        if (string.IsNullOrEmpty(dosboxExePath) || !File.Exists(dosboxExePath))
        {
            MessageBox.Show(owner, $"DOSBox executable path is not configured or the configured path is invalid. Please check the settings.", "Launch Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        if (string.IsNullOrEmpty(gameConfig.MountCPath) || !Directory.Exists(gameConfig.MountCPath))
        {
            MessageBox.Show(owner, $"Game's C: mount directory '{gameConfig.MountCPath}' (expected 'game-files' inside game folder) not found for '{gameConfig.GameName}'.", "Launch Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        if (string.IsNullOrEmpty(gameConfig.DosboxConfPath) || !File.Exists(gameConfig.DosboxConfPath))
        {
            MessageBox.Show(owner, $"Game's DOSBox config file '{gameConfig.DosboxConfPath}' (expected 'dosbox-staging.conf' inside game folder) not found for '{gameConfig.GameName}'.", "Launch Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        List<string> dosboxArgs = new List<string>
        {
            "-noconsole",
            $"-conf \"{gameConfig.DosboxConfPath}\"",
            $"-c \"MOUNT C '{gameConfig.MountCPath}'\""
        };

        if (gameConfig.IsoImages.Any())
        {
            if (!Directory.Exists(gameConfig.IsoBasePath))
            {
                MessageBox.Show(owner, $"ISO directory '{gameConfig.IsoBasePath}' not found for game '{gameConfig.GameName}'.", "Launch Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            else
            {
                var validIsoPaths = gameConfig.IsoImages
                    .Where(isoInfo =>
                    {
                        bool exists = File.Exists(isoInfo.FilePath);
                        if (!exists)
                        {
                            MessageBox.Show(owner, $"ISO/CUE file '{Path.GetFileName(isoInfo.FilePath)}' not found at the expected path '{isoInfo.FilePath}'.", "Launch Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }
                        return exists;
                    })
                    .Select(isoInfo => $"'{isoInfo.FilePath}'");

                if (validIsoPaths.Any()) dosboxArgs.Add($"-c \"IMGMOUNT D {string.Join(" ", validIsoPaths)} -t iso\"");
            }
        }

        dosboxArgs.Add("-c \"C:\"");

        foreach (string command in commandsToRun)
        {
            dosboxArgs.Add($"-c \"{command}\"");
        }

        dosboxArgs.Add("-c \"EXIT\"");

        string arguments = string.Join(" ", dosboxArgs);

        AppLogger.Log($"Launching DOSBox with command: {dosboxExePath} {arguments}");

        try
        {
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = dosboxExePath,
                Arguments = arguments,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            Process.Start(startInfo);
        }
        catch (Exception ex)
        {
            MessageBox.Show(owner, $"Failed to launch DOSBox: {ex.Message}\n\nCommand: {arguments}", "Launch Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private static Process? LaunchDosboxForMediaInstallation(string? dosboxExePath, string dosboxConfPath, string mountCPath, IEnumerable<string> imagePaths, char driveLetter, string imageType, IWin32Window owner)
    {
        if (string.IsNullOrEmpty(dosboxExePath) || !File.Exists(dosboxExePath))
        {
            MessageBox.Show(owner, "DOSBox executable path is not configured or the configured path is invalid. Please check the settings.", "Launch Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return null;
        }

        var dosboxArgs = new List<string>
        {
            "-noconsole",
            $"-conf \"{dosboxConfPath}\"",
            $"-c \"MOUNT C '{mountCPath}'\""
        };

        if (imagePaths.Any())
        {
            // Build the IMGMOUNT command with all disk images quoted
            var quotedImagePaths = imagePaths.Select(p => $"'{p}'");
            string imgMountCommand = $"IMGMOUNT {driveLetter} {string.Join(" ", quotedImagePaths)} -t {imageType}";
            dosboxArgs.Add($"-c \"{imgMountCommand}\"");
        }

        dosboxArgs.Add("-c \"C:\"");

        string arguments = string.Join(" ", dosboxArgs);

        AppLogger.Log($"Launching DOSBox for media installation with command: {dosboxExePath} {arguments}");

        try
        {
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = dosboxExePath,
                Arguments = arguments,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            return Process.Start(startInfo);
        }
        catch (Exception ex)
        {
            MessageBox.Show(owner, $"Failed to launch DOSBox for installation: {ex.Message}\n\nCommand: {arguments}", "Launch Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return null;
        }
    }

    public static Process? LaunchDosboxForDisketteInstallation(string? dosboxExePath, string dosboxConfPath, string mountCPath, IEnumerable<string> disketteImagePaths, IWin32Window owner)
    {
        return LaunchDosboxForMediaInstallation(dosboxExePath, dosboxConfPath, mountCPath, disketteImagePaths, 'A', "floppy", owner);
    }

    public static Process? LaunchDosboxForCdRomInstallation(string? dosboxExePath, string dosboxConfPath, string mountCPath, IEnumerable<string> cdRomImagePaths, IWin32Window owner)
    {
        return LaunchDosboxForMediaInstallation(dosboxExePath, dosboxConfPath, mountCPath, cdRomImagePaths, 'D', "iso", owner);
    }
}