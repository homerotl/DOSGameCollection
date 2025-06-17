
using static DOSGameCollection.LoadGamesDataService;
using System.Diagnostics;

namespace DOSGameCollection;

public class TopForm : Form
{
    private ListBox? gameListBox;
    private Button? runButton;
    private FlowLayoutPanel? buttonFlowLayoutPanel;
    private TableLayoutPanel? gameDetailsTableLayoutPanel; // For "Name" label and TextBox
    private Label? gameNameLabel;
    private TextBox? gameNameTextBox;
    private PictureBox? boxArtPictureBox;
    private List<GameConfiguration> _loadedGameConfigs;
    private readonly string _dosboxExePath = @"C:\Emulation\pc\DOSBox-Staging\v0.82.1\dosbox.exe"; // <--- SET YOUR DOSBOX.EXE PATH HERE

    // Base directory for the game library
    private readonly string _gameLibraryDirectory = @"C:\DOSGameCollection\library"; // <-- SET YOUR GAME LIBRARY PATH HERE

    public TopForm()
    {
        InitializeComponent();
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
            // CellBorderStyle = TableLayoutPanelCellBorderStyle.Single, // Keep for visual debugging
            ColumnCount = 2,
            RowCount = 2 // Increased row count for the new FlowLayoutPanel
        };

        // Define Column Styles
        tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 200F));
        tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));

        // Define Row Styles
        tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // Row for buttons
        tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100F)); // Row for game list and details

        // Initialize FlowLayoutPanel for buttons
        buttonFlowLayoutPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            AutoSize = true,
            FlowDirection = FlowDirection.LeftToRight,
            Padding = new Padding(5)
        };
        tableLayoutPanel.Controls.Add(buttonFlowLayoutPanel, 0, 0);
        tableLayoutPanel.SetColumnSpan(buttonFlowLayoutPanel, 2); // Span across both columns

        // Initialize Run Button
        runButton = new Button
        {
            Text = "Run",
            AutoSize = true,
            Margin = new Padding(3),
            Enabled = false // Initially disabled
        };
        runButton.Click += RunButton_Click; // Add click event handler
        buttonFlowLayoutPanel.Controls.Add(runButton);

        // --- Game ListBox Setup ---
        gameListBox = new ListBox();
        // Initialize gameDetailsTableLayoutPanel for Name label and TextBox
        gameDetailsTableLayoutPanel = new TableLayoutPanel
        {
            Dock = DockStyle.Top, // Anchor to the top of the cell
            ColumnCount = 2,
            RowCount = 2, // Increased row count for PictureBox
            AutoSize = true,
            Margin = new Padding(0, 5, 5, 5) // Added margin (top, right, bottom, left)
        };
        gameDetailsTableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize)); // For Label
        gameDetailsTableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F)); // For TextBox
        // Define Row Styles for gameDetailsTableLayoutPanel
        gameDetailsTableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // Row for Name
        gameDetailsTableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 250F)); // Row for Box Art with fixed height


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
        gameDetailsTableLayoutPanel.Controls.Add(gameNameLabel, 0, 0);
        gameDetailsTableLayoutPanel.Controls.Add(gameNameTextBox, 1, 0);

        // Initialize PictureBox for Box Art
        boxArtPictureBox = new PictureBox
        {
            Dock = DockStyle.Fill, // Or AnchorStyles.Top, AnchorStyles.Left if fixed size
            SizeMode = PictureBoxSizeMode.Zoom, // Or StretchImage, Normal, etc.
            BorderStyle = BorderStyle.FixedSingle, // Optional: for visibility
            BackColor = Color.Black, // Set background color to black
            // Height = 400, // Height will be determined by the TableLayoutPanel row
        };
        gameDetailsTableLayoutPanel.Controls.Add(boxArtPictureBox, 0, 1); // Add to row 1
        gameDetailsTableLayoutPanel.SetColumnSpan(boxArtPictureBox, 2); // Span across both columns in the details panel

        // --- Game ListBox Setup ---
        gameListBox.Dock = DockStyle.Fill;
        gameListBox.Margin = new Padding(5); // Margin around the list box
        gameListBox.Sorted = true; // Ensure items are sorted alphabetically
        gameListBox.SelectedIndexChanged += GameListBox_SelectedIndexChanged; // Add event for selection change
        tableLayoutPanel.Controls.Add(gameListBox, 0, 1); // Add gameListBox to column 0, row 1 of main TLP
        
        // Add gameDetailsTableLayoutPanel directly to the main tableLayoutPanel
        tableLayoutPanel.Controls.Add(gameDetailsTableLayoutPanel, 1, 1); // Add to column 1, row 1 of main TLP

        // Add the TableLayoutPanel to the form
        this.Controls.Add(tableLayoutPanel);
    }

    private async void TopForm_Load(object? sender, EventArgs e)
    {
        using LoadGameListProgressDialog progressDialog = new();
        // Create a Progress<T> object.
        // The constructor takes an Action<T> which will be executed on the UI thread
        // whenever progress.Report() is called from the background.
        var progress = new Progress<ProgressReport>(report =>
        {
            // This code runs on the UI thread
            progressDialog.HandleProgressReport(report);

            // Optionally, close the dialog when the task indicates completion
            if (report.IsComplete)
            {
                progressDialog.Close();
            }
        });

        // Instantiate your data service
        LoadGamesDataService loadGamesDataService = new();

        try
        {
            // LoadDataAsync is already an async method.
            // Start the loading task.
            Task<List<GameConfiguration>> loadingTask = loadGamesDataService.LoadDataAsync(_gameLibraryDirectory, progress);
            progressDialog.ShowDialog(this);
            _loadedGameConfigs = await loadingTask;
        }
        catch (ArgumentException ex)
        {
            MessageBox.Show($"Error: Invalid directory path. {ex.Message}", "Loading Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }
        catch (DirectoryNotFoundException ex)
        {
            MessageBox.Show($"Error: Directory not found. {ex.Message}", "Loading Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }
        catch (UnauthorizedAccessException ex)
        {
            MessageBox.Show($"Error: Access denied to directory. Please check permissions. {ex.Message}", "Loading Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }
        catch (IOException ex)
        {
            MessageBox.Show($"Error: An I/O error occurred. {ex.Message}", "Loading Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"An unexpected error occurred during data loading: {ex.Message}", "Loading Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

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

    private void GameListBox_SelectedIndexChanged(object? sender, EventArgs e)
    {
        // Enable the Run button only if an item is selected
        if (runButton != null)
        {
            runButton.Enabled = (gameListBox?.SelectedItem != null);
        }

        if (gameNameTextBox != null && gameListBox != null)
        {
            if (gameListBox.SelectedItem is GameConfiguration selectedGame)
            {
                gameNameTextBox.Text = selectedGame.GameName;
            }
            else
            {
                gameNameTextBox.Text = string.Empty; // Clear if no selection
            }

            // Handle Box Art
            if (boxArtPictureBox != null)
            {
                // Dispose previous image to free resources
                boxArtPictureBox.Image?.Dispose();
                boxArtPictureBox.Image = null;

                if (gameListBox?.SelectedItem is GameConfiguration selectedGameWithArt)
                {
                    if (File.Exists(selectedGameWithArt.FrontBoxArtPath))
                    {
                        try
                        {
                            boxArtPictureBox.Image = Image.FromFile(selectedGameWithArt.FrontBoxArtPath);
                        }
                        catch (Exception ex) { Console.WriteLine($"Error loading box art: {ex.Message}"); /* Handle error, e.g., show placeholder */ }
                    }
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
    private void LaunchDosBox(GameConfiguration gameConfig)
    {
        if (string.IsNullOrEmpty(_dosboxExePath) || !File.Exists(_dosboxExePath))
        {
            MessageBox.Show("DOSBox executable path is not set or not found. Please check _dosboxExePath in TopForm.cs", "Launch Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
            MessageBox.Show($"Game's DOSBox config file '{gameSpecificDosboxConfPath}' (expected 'dosbox-staging.conf' inside game folder) not found for '{gameConfig.GameName}'.", "Launch Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                MessageBox.Show($"ISO directory '{gameConfig.IsoBasePath}' not found for game '{gameConfig.GameName}'.", "Launch Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
                MessageBox.Show($"ISO/CUE file '{isoPath}' not found in '{gameConfig.IsoBasePath}' for game '{gameConfig.GameName}'.", "Launch Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
                FileName = _dosboxExePath,
                Arguments = arguments,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            Process.Start(startInfo);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Failed to launch DOSBox: {ex.Message}\n\nCommand: {arguments}", "Launch Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
