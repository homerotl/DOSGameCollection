using DOSGameCollection.Models;
using System.Diagnostics;

namespace DOSGameCollection.Services;

public static class GameLauncherService
{
    public static void LaunchGame(GameConfiguration gameConfig, string? dosboxExePath, IWin32Window owner)
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

        foreach (DiscImageInfo isoInfo in gameConfig.IsoImages)
        {
            if (!Directory.Exists(gameConfig.IsoBasePath))
            {
                MessageBox.Show(owner, $"ISO directory '{gameConfig.IsoBasePath}' not found for game '{gameConfig.GameName}'.", "Launch Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                break;
            }

            string fullIsoPathOnHost = Path.Combine(gameConfig.IsoBasePath, isoInfo.ImgFileName);

            if (File.Exists(fullIsoPathOnHost))
            {
                dosboxArgs.Add($"-c \"IMGMOUNT D '{fullIsoPathOnHost}' -t iso\"");
            }
            else
            {
                MessageBox.Show(owner, $"ISO/CUE file '{isoInfo.ImgFileName}' not found in '{gameConfig.IsoBasePath}' for game '{gameConfig.GameName}'.", "Launch Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        dosboxArgs.Add("-c \"C:\"");

        foreach (string command in gameConfig.DosboxCommands)
        {
            dosboxArgs.Add($"-c \"{command}\"");
        }

        dosboxArgs.Add("-c \"EXIT\"");

        string arguments = string.Join(" ", dosboxArgs);

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
}