
using static DOSGameCollection.LoadGamesDataService;
using System.IO;
using System.Diagnostics;

namespace DOSGameCollection;

public class TopForm : Form
{
    private ListBox? gameListBox;
    private Button? runButton;
    private Button? refreshButton; // Added refresh button
    private TableLayoutPanel? gameDetailsTableLayoutPanel; // For "Name" label and TextBox
    private Label? gameNameLabel;
    private TextBox? gameNameTextBox;
    private PictureBox? boxArtPictureBox;
    private TextBox? synopsisTextBox; // Added for game synopsis
    private List<GameConfiguration> _loadedGameConfigs = new();
    private AppConfigService _appConfigService;

    public TopForm()
    {
        InitializeComponent();
        _appConfigService = new AppConfigService();
        this.Load += TopForm_Load; // Event for when the form loads
    }

    private void InitializeComponent()
    {

        this.Text = "DOSGameCollection";
        this.Name = "TopForm";
        this.ClientSize = new System.Drawing.Size(800, 450);
        this.MinimumSize = new System.Drawing.Size(400, 450);

        TableLayoutPanel tableLayoutPanel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            CellBorderStyle = TableLayoutPanelCellBorderStyle.Single, // Keep for visual debugging
            ColumnCount = 2
        };

        // Define Column Styles
        tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 200F));
        tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));

        // Define Row Styles
        // RowStyles will be implicitly SizeType.Percent, 100F for the single row after removing the button row.
        // If you had multiple rows below where the buttons were, you'd adjust them here.
        // For a single remaining row that fills, no explicit RowStyle is strictly needed unless you want AutoSize or Absolute.
        // tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100F)); // Row for game list and details
        // For simplicity with one main content row, we can let it default or explicitly set it if more rows are added later.
        // For now, let's assume it will take up all available space.

        // --- Refresh Button Setup ---
        refreshButton = new Button
        {
            Text = "Refresh",
            Anchor = AnchorStyles.Left, // Align to the left
            AutoSize = true,
            Margin = new Padding(5, 5, 5, 3) // Margin (left, top, right, bottom)
        };
        refreshButton.Click += RefreshButton_Click;

        // --- Game ListBox Setup ---
        gameListBox = new ListBox();
        // Initialize gameDetailsTableLayoutPanel for Name label and TextBox
        gameDetailsTableLayoutPanel = new TableLayoutPanel
        {
            Dock = DockStyle.Top, // Anchor to the top of the cell
            ColumnCount = 2,
            RowCount = 3, // Increased row count for PictureBox and Run button
            AutoSize = true,
            Margin = new Padding(0, 5, 5, 5) // Added margin (top, right, bottom, left)
        };
        gameDetailsTableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize)); // For Label
        gameDetailsTableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F)); // For TextBox

        // Initialize Run Button (moved here)
        runButton = new Button
        {
            Text = "Run Game",
            Anchor = AnchorStyles.Left, // Anchor to the left
            AutoSize = true,
            Margin = new Padding(0, 0, 0, 5), // Add some bottom margin
            Enabled = false // Initially disabled
        };
        runButton.Click += RunButton_Click; // Add click event handler

        // Define Row Styles for gameDetailsTableLayoutPanel (order matters for visual layout)
        gameDetailsTableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // Row 0: Run button
        gameDetailsTableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // Row 1: Name (label and textbox)
        gameDetailsTableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 250F)); // Row 2: Box Art


        gameNameLabel = new Label
        {
            Text = "Name:",
            Anchor = AnchorStyles.Left,
            TextAlign = ContentAlignment.MiddleLeft,
            AutoSize = true,
            Margin = new Padding(0, 0, 3, 0) // Margin to the right of the label
        };
        gameNameTextBox = new TextBox
        {
            Dock = DockStyle.Fill,
            ReadOnly = true
        };

        // Add Run button to row 0
        gameDetailsTableLayoutPanel.Controls.Add(runButton, 0, 0);
        gameDetailsTableLayoutPanel.SetColumnSpan(runButton, 2); // Span button across both columns
        // Add Name label and textbox to row 1
        gameDetailsTableLayoutPanel.Controls.Add(gameNameLabel, 0, 1);
        gameDetailsTableLayoutPanel.Controls.Add(gameNameTextBox, 1, 1);

        // --- Synopsis TextBox Setup ---
        synopsisTextBox = new TextBox
        {
            Dock = DockStyle.Fill,
            Multiline = true,
            ReadOnly = true,
            ScrollBars = ScrollBars.Vertical,
            Text = "Lorem Ipsum", // Hardcoded synopsis for now
            Margin = new Padding(0, 0, 3, 0) // Right margin
        };

        // Initialize PictureBox for Box Art
        boxArtPictureBox = new PictureBox
        {
            Dock = DockStyle.Fill, // Or AnchorStyles.Top, AnchorStyles.Left if fixed size
            SizeMode = PictureBoxSizeMode.Zoom, // Or StretchImage, Normal, etc.
            BorderStyle = BorderStyle.FixedSingle, // Optional: for visibility
            BackColor = Color.Black,
            Margin = new Padding(3, 0, 0, 0) // Left margin
        };

        // --- Media Panel (for Synopsis and Box Art) ---
        TableLayoutPanel mediaPanel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1
        };
        mediaPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F)); // Synopsis
        mediaPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F)); // Box Art
        mediaPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100F)); // Fill available height

        mediaPanel.Controls.Add(synopsisTextBox, 0, 0);
        mediaPanel.Controls.Add(boxArtPictureBox, 1, 0);

        gameDetailsTableLayoutPanel.Controls.Add(mediaPanel, 0, 2); // Add mediaPanel to row 2
        gameDetailsTableLayoutPanel.SetColumnSpan(mediaPanel, 2); // Span mediaPanel across both columns

        // --- Left Column Panel (for Refresh Button and Game ListBox) ---
        TableLayoutPanel leftColumnPanel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2,
            Margin = new Padding(0) // No margin for the panel itself
        };
        leftColumnPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // Row for Refresh button
        leftColumnPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100F)); // Row for GameListBox

        // Add Refresh Button to the leftColumnPanel
        leftColumnPanel.Controls.Add(refreshButton, 0, 0);

        // Configure Game ListBox (moved here to be added to leftColumnPanel)
        gameListBox.Dock = DockStyle.Fill;
        gameListBox.Margin = new Padding(5, 0, 5, 5); // Adjusted margin (L,T,R,B) - top margin handled by button's bottom margin or AutoSize row
        gameListBox.Sorted = true;
        gameListBox.SelectedIndexChanged += GameListBox_SelectedIndexChanged;
        leftColumnPanel.Controls.Add(gameListBox, 0, 1); // Add gameListBox to the second row of leftColumnPanel

        tableLayoutPanel.Controls.Add(leftColumnPanel, 0, 0); // Add leftColumnPanel to column 0, row 0 of main TLP
        tableLayoutPanel.Controls.Add(gameDetailsTableLayoutPanel, 1, 0); // Add to column 1, row 0 of main TLP

        // Add the TableLayoutPanel to the form
        this.Controls.Add(tableLayoutPanel);
    }

    private async void TopForm_Load(object? sender, EventArgs e)
    {
        await _appConfigService.LoadOrCreateConfigurationAsync(this);

        if (string.IsNullOrEmpty(_appConfigService.LibraryPath) || !Directory.Exists(_appConfigService.LibraryPath))
        {
            MessageBox.Show(this, "Game library path is not configured or is invalid. Please restart the application to configure it. Game loading will be skipped.", "Library Path Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            if (refreshButton != null) refreshButton.Enabled = false; // Disable refresh if library path is bad
            return; // Stop further processing in TopForm_Load
        }

        await RefreshGameListAsync();
    }

    private async Task RefreshGameListAsync()
    {
        // Clear existing game list and related UI elements
        if (gameListBox != null)
        {
            gameListBox.Items.Clear();
        }
        if (gameNameTextBox != null)
        {
            gameNameTextBox.Text = string.Empty;
        }
        if (boxArtPictureBox != null && boxArtPictureBox.Image != null)
        {
            boxArtPictureBox.Image.Dispose();
            boxArtPictureBox.Image = null;
        }
        if (synopsisTextBox != null)
        {
            synopsisTextBox.Text = string.Empty;
        }
        if (runButton != null)
        {
            runButton.Enabled = false;
        }
        _loadedGameConfigs.Clear();

        if (string.IsNullOrEmpty(_appConfigService.LibraryPath) || !Directory.Exists(_appConfigService.LibraryPath))
        {
            MessageBox.Show(this, "Game library path is not configured or is invalid. Refresh aborted.", "Library Path Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        using LoadGameListProgressDialog progressDialog = new();
        var progress = new Progress<ProgressReport>(report =>
        {
            progressDialog.HandleProgressReport(report);
            if (report.IsComplete && progressDialog.Visible)
            {
                progressDialog.Close();
            }
        });

        LoadGamesDataService loadGamesDataService = new();
        List<GameConfiguration> gameConfigs = new List<GameConfiguration>();

        try
        {
            if (refreshButton != null) refreshButton.Enabled = false; // Disable button during load

            Task<List<GameConfiguration>> loadingTask = loadGamesDataService.LoadDataAsync(_appConfigService.LibraryPath, progress);
            progressDialog.ShowDialog(this); // Show modally, blocks here until dialog is closed
            gameConfigs = await loadingTask;
        }
        catch (Exception ex) when (ex is ArgumentException || ex is DirectoryNotFoundException || ex is UnauthorizedAccessException || ex is IOException)
        {
            MessageBox.Show(this, $"Error loading games: {ex.Message}", "Loading Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, $"An unexpected error occurred during game loading: {ex.Message}", "Loading Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            if (refreshButton != null) refreshButton.Enabled = true; // Re-enable button
        }

        _loadedGameConfigs = gameConfigs;
        PopulateListBox(_loadedGameConfigs);
    }

    private void PopulateListBox(List<GameConfiguration> gameConfigs)
    {
        if (gameListBox.InvokeRequired)
        {
            gameListBox.Invoke(new MethodInvoker(() => PopulateListBox(gameConfigs)));
        }
        else
        {
            gameListBox.Items.Clear();
            foreach (GameConfiguration config in gameConfigs)
            {
                gameListBox.Items.Add(config);
            }
        }
    }

    private async void GameListBox_SelectedIndexChanged(object? sender, EventArgs e)
    {
        // Enable the Run button only if an item is selected
        if (runButton != null)
        {
            runButton.Enabled = (gameListBox?.SelectedItem != null);
        }

        if (gameNameTextBox != null && gameListBox != null)
        { // General null check for UI elements

            GameConfiguration? selectedGame = gameListBox.SelectedItem as GameConfiguration;

            if (selectedGame != null)
            {
                gameNameTextBox.Text = selectedGame.GameName;

                // Load Synopsis
                if (synopsisTextBox != null)
                {
                    if (File.Exists(selectedGame.SynopsisFilePath))
                    {
                        try
                        {
                            synopsisTextBox.Text = await File.ReadAllTextAsync(selectedGame.SynopsisFilePath);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error reading synopsis file '{selectedGame.SynopsisFilePath}': {ex.Message}");
                            synopsisTextBox.Text = string.Empty; // Clear on error
                        }
                    }
                    else
                    {
                        synopsisTextBox.Text = string.Empty; // File doesn't exist
                    }
                }
            }
            else // No game selected
            {
                gameNameTextBox.Text = string.Empty; // Clear if no selection
                if (synopsisTextBox != null)
                {
                    synopsisTextBox.Text = string.Empty; // Clear synopsis if no selection
                }
            }

            // Handle Box Art
            if (boxArtPictureBox != null)
            {
                // Dispose previous image to free resources
                boxArtPictureBox.Image?.Dispose();
                boxArtPictureBox.Image = null;

                if (selectedGame != null) // Use the already cast selectedGame
                {
                    if (File.Exists(selectedGame.FrontBoxArtPath))
                    {
                        try
                        {
                            boxArtPictureBox.Image = Image.FromFile(selectedGame.FrontBoxArtPath);
                        }
                        catch (Exception ex) // Catch other potential errors like OutOfMemoryException or invalid image format
                        {
                            Console.WriteLine($"Error loading existing box art '{selectedGame.FrontBoxArtPath}': {ex.Message}");
                            boxArtPictureBox.Image = null; // Ensure it's null on error
                        }
                    }
                    // If File.Exists is false, boxArtPictureBox.Image remains null (no error logged for missing file)
                    // Synopsis is handled above, independently of box art.
                }
            }
        }
    }

    private void RunButton_Click(object? sender, EventArgs e)
    {
        if (gameListBox?.SelectedItem is GameConfiguration selectedGame)
        {
            LaunchDosBox(selectedGame);
        }
    }

    private async void RefreshButton_Click(object? sender, EventArgs e)
    {
        await RefreshGameListAsync();
    }
    private void LaunchDosBox(GameConfiguration gameConfig)
    {
        string? dosboxPath = _appConfigService.DosboxExePath;
        if (string.IsNullOrEmpty(dosboxPath) || !File.Exists(dosboxPath))
        {
            MessageBox.Show(this, $"DOSBox executable path is not configured or the configured path is invalid. Please check 'config.txt' in the application directory, or restart the application to be prompted again.", "Launch Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        string gameSpecificMountCPath = gameConfig.MountCPath;
        if (string.IsNullOrEmpty(gameSpecificMountCPath) || !Directory.Exists(gameSpecificMountCPath))
        {
            MessageBox.Show($"Game's C: mount directory '{gameSpecificMountCPath}' (expected 'game-files' inside game folder) not found for '{gameConfig.GameName}'.", "Launch Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        string gameSpecificDosboxConfPath = gameConfig.DosboxConfPath;
        if (string.IsNullOrEmpty(gameSpecificDosboxConfPath) || !File.Exists(gameSpecificDosboxConfPath))
        {
            MessageBox.Show(this, $"Game's DOSBox config file '{gameSpecificDosboxConfPath}' (expected 'dosbox-staging.conf' inside game folder) not found for '{gameConfig.GameName}'.", "Launch Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        List<string> dosboxArgs = new List<string>();

        dosboxArgs.Add("-noconsole");
        dosboxArgs.Add($"-conf \"{gameSpecificDosboxConfPath}\"");
        dosboxArgs.Add($"-c \"MOUNT C '{gameSpecificMountCPath}'\"");

        foreach (string isoPath in gameConfig.IsoImagePaths)
        {
            // Check if the "isos" directory exists for this game if ISOs are listed
            if (!Directory.Exists(gameConfig.IsoBasePath))
            {
                MessageBox.Show(this, $"ISO directory '{gameConfig.IsoBasePath}' not found for game '{gameConfig.GameName}'.", "Launch Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                continue; // Skip this ISO if the base path doesn't exist
            }

            // ISO paths in game.cfg are relative to the "isos" directory within the game's folder
            string fullIsoPathOnHost = Path.Combine(gameConfig.IsoBasePath, isoPath);

            if (File.Exists(fullIsoPathOnHost))
            {
                dosboxArgs.Add($"-c \"IMGMOUNT D '{fullIsoPathOnHost}' -t iso\"");
            }
            else
            {
                MessageBox.Show(this, $"ISO/CUE file '{isoPath}' not found in '{gameConfig.IsoBasePath}' for game '{gameConfig.GameName}'.", "Launch Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        dosboxArgs.Add($"-c \"C:\"");

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
                FileName = dosboxPath, // Use the local variable
                Arguments = arguments,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            Process.Start(startInfo);
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, $"Failed to launch DOSBox: {ex.Message}\n\nCommand: {arguments}", "Launch Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
