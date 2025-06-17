using System.IO;
using System.Windows.Forms; // Required for IWin32Window, MessageBox, OpenFileDialog

namespace DOSGameCollection
{
    public class AppConfigService
    {
        private const string ConfigFileName = "config.txt";
        private const string DosboxPathKey = "dosbox-path=";
        private const string LibraryPathKey = "library=";

        public string? DosboxExePath { get; private set; }
        public string? LibraryPath { get; private set; }

        private readonly string _configFilePath;

        public AppConfigService()
        {
            _configFilePath = Path.Combine(Application.StartupPath, ConfigFileName);
        }

        public async Task LoadOrCreateConfigurationAsync(IWin32Window? owner = null)
        {
            // Initialize properties to null to ensure fresh load or prompt
            bool pathConfigured = false;
            DosboxExePath = null;
            LibraryPath = null;

            if (File.Exists(_configFilePath)) // Try to load existing configuration
            {
                try
                {
                    string[] lines = await File.ReadAllLinesAsync(_configFilePath);
                    foreach (string line in lines)
                    {
                        if (line.StartsWith(DosboxPathKey, StringComparison.OrdinalIgnoreCase))
                        {
                            string pathValue = line.Substring(DosboxPathKey.Length).Trim();
                            if (File.Exists(pathValue))
                            {
                                DosboxExePath = pathValue;
                                pathConfigured = true;
                            }
                            else
                            {
                                MessageBox.Show(owner, $"The DOSBox path '{pathValue}' found in '{ConfigFileName}' is invalid.", "Invalid Config: DOSBox Path", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            }
                        }
                        else if (line.StartsWith(LibraryPathKey, StringComparison.OrdinalIgnoreCase))
                        {
                            string pathValue = line.Substring(LibraryPathKey.Length).Trim();
                            if (Directory.Exists(pathValue))
                            {
                                LibraryPath = pathValue;
                            }
                            else
                            {
                                MessageBox.Show(owner, $"The Library path '{pathValue}' found in '{ConfigFileName}' is invalid.", "Invalid Config: Library Path", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(owner, $"Error reading '{ConfigFileName}': {ex.Message}. Please re-select the DOSBox executable.", "Config Read Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    // Proceed to prompt
                }
            }

            if (string.IsNullOrEmpty(DosboxExePath))
            {
                await PromptUserForDosboxPathAsync(owner);
            }

            // Prompt for Library path if not loaded or invalid
            if (string.IsNullOrEmpty(LibraryPath))
            {
                await PromptUserForLibraryPathAsync(owner);
            }

            await SaveConfigurationAsync(owner);
        }

        private async Task PromptUserForDosboxPathAsync(IWin32Window? owner = null)
        {

            MessageBox.Show(owner, $"DOSBox executable path is not configured or is invalid. Please locate your DOSBox executable (e.g., dosbox.exe, dosbox-staging.exe).", "DOSBox Configuration Needed", MessageBoxButtons.OK, MessageBoxIcon.Information);

            using OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Title = "Select DOSBox Executable",
                Filter = "Executable files (*.exe)|*.exe|All files (*.*)|*.*",
                CheckFileExists = true,
                CheckPathExists = true
            };

            if (openFileDialog.ShowDialog(owner) == DialogResult.OK)
            {
                DosboxExePath = openFileDialog.FileName;
            }
            else
            {
                MessageBox.Show(owner, "DOSBox executable was not selected. Game launching will be disabled until DOSBox is configured.", "DOSBox Not Configured", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                DosboxExePath = null; // Ensure it's null if user cancels
            }
        }
        
         private async Task PromptUserForLibraryPathAsync(IWin32Window? owner = null)
        {
            MessageBox.Show(owner, $"Game library path is not configured or is invalid. Please select your game library directory.", "Game Library Configuration Needed", MessageBoxButtons.OK, MessageBoxIcon.Information);

            using FolderBrowserDialog folderDialog = new FolderBrowserDialog
            {
                Description = "Select Game Library Directory",
                ShowNewFolderButton = true
            };

            if (folderDialog.ShowDialog(owner) == DialogResult.OK)
            {
                LibraryPath = folderDialog.SelectedPath;
            }
            else
            {
                MessageBox.Show(owner, "Game library directory was not selected. Game loading will be unavailable.", "Library Not Configured", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                LibraryPath = null; // Ensure it's null if user cancels
            }
            // Save will be handled by LoadOrCreateConfigurationAsync
        }

        private async Task SaveConfigurationAsync(IWin32Window? owner = null)
        {
            List<string> linesToSave = new List<string>();
            if (!string.IsNullOrEmpty(DosboxExePath))
            {
                linesToSave.Add($"{DosboxPathKey}{DosboxExePath}");
            }
            if (!string.IsNullOrEmpty(LibraryPath))
            {
                linesToSave.Add($"{LibraryPathKey}{LibraryPath}");
            }

            try
            {
                await File.WriteAllLinesAsync(_configFilePath, linesToSave);
                // Optionally, show a generic "Configuration saved" message if needed,
                // but individual prompts already give feedback.
            }
            catch (Exception ex)
            {
                MessageBox.Show(owner, $"Failed to save configuration to '{ConfigFileName}': {ex.Message}", "Save Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
         }
    }
}
