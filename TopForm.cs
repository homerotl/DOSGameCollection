
using System.Diagnostics;
using System.Reflection;
using DOSGameCollection.Models;
using DOSGameCollection.UI;

namespace DOSGameCollection;

public class TopForm : Form
{
    private ListBox? gameListBox;
    private Button? runButton;
    private Button? manualButton; // Added for game manual
    private Button? refreshButton; // Added refresh button
    private TableLayoutPanel? gameDetailsTableLayoutPanel; // For "Name" label and TextBox
    private Label? gameNameLabel;
    private TextBox? gameNameTextBox;
    private PictureBox? boxArtPictureBox;
    private Button? boxArtPreviousButton;
    private Label? boxArtImageNameLabel;
    private Button? boxArtNextButton;
    private TextBox? synopsisTextBox; // Added for game synopsis
    private MenuStrip? menuStrip; // Added for MenuStrip
    private TabControl? gameDetailsTabControl; // For additional game details
    private DataGridView? diskImagesDataGridView; // For displaying data for CD-ROM images
    private PictureBox? isoImagePictureBox; // For showing an image of the selected CD-ROM image
    private DataGridView? installDiscsDataGridView; // For floppy disk images
    private PictureBox? diskImagePictureBox; // For showing an image of the selected install disc
    private DataGridView? runCommandsDataGridView; // For displaying DOSBox commands on the "Run Commands" tab
    private List<GameConfiguration> _loadedGameConfigs = [];
    private AppConfigService _appConfigService;
    private BoxArtCarouselManager? _boxArtCarouselManager;

    public TopForm()
    {
        InitializeComponent();
        _appConfigService = new AppConfigService();
        this.Load += TopForm_Load; // Event for when the form loads
    }

    private void InitializeComponent()
    {
        this.Text = $"DOSGameCollection - build {BuildInfo.BuildVersion}";
        this.Name = "TopForm";
        this.ClientSize = new System.Drawing.Size(800, 600); // Updated height
        this.MinimumSize = new System.Drawing.Size(800, 600); // Updated minimum width and height

        // --- Set Form Icon ---
        try
        {
            // Get the current assembly
            Assembly assembly = Assembly.GetExecutingAssembly();
            // Get the resource name (usually Namespace.Filename.Extension)
            // Assuming your namespace is DOSGameCollection and icon is appicon.ico in root
            string resourceName = "DOSGameCollection.appicon.ico"; // Adjust if namespace or filename differs

            using (Stream? iconStream = assembly.GetManifestResourceStream(resourceName))
            {
                if (iconStream != null)
                {
                    this.Icon = new System.Drawing.Icon(iconStream);
                }
                else
                {
                    Console.WriteLine($"Warning: Embedded resource '{resourceName}' not found.");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading embedded icon: {ex.Message}");
            // Optionally handle the error, e.g., log it or show a message
        }

        // --- MenuStrip Setup ---
        menuStrip = new MenuStrip
        {
            Dock = DockStyle.Top // Explicitly dock MenuStrip to the top
        };

        // File Menu
        ToolStripMenuItem fileMenu = new("&File");
        ToolStripMenuItem exitMenuItem = new("E&xit");
        exitMenuItem.Click += ExitMenuItem_Click;
        fileMenu.DropDownItems.Add(exitMenuItem);

        // Settings Menu
        ToolStripMenuItem settingsMenu = new("&Settings");
        ToolStripMenuItem setDosboxLocationMenuItem = new("Set &DOSBox location...");
        setDosboxLocationMenuItem.Click += SetDosboxLocationMenuItem_Click;

        ToolStripMenuItem setGameLibraryLocationMenuItem = new("Set game &library location...");
        setGameLibraryLocationMenuItem.Click += SetGameLibraryLocationMenuItem_Click;

        settingsMenu.DropDownItems.Add(setDosboxLocationMenuItem);
        settingsMenu.DropDownItems.Add(setGameLibraryLocationMenuItem);

        menuStrip.Items.AddRange(new ToolStripItem[] {
            fileMenu,
            settingsMenu
        });

        Controls.Add(menuStrip);
        MainMenuStrip = menuStrip;

        TableLayoutPanel tableLayoutPanel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            CellBorderStyle = TableLayoutPanelCellBorderStyle.Single, // Keep for visual debugging
            ColumnCount = 2
        };

        // Define Column Styles
        tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 200F));
        tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));

        var symbolFont = FormatTools.GetSymbolFont();

        // Button initialization

        // Initialize the refresh button
        refreshButton = new Button
        {
            Anchor = AnchorStyles.Left, // Align to the left
            AutoSize = true,
            Margin = new Padding(5, 5, 5, 3) // Margin (left, top, right, bottom)
        };
        refreshButton.Click += RefreshButton_Click;

        // Initialize Run Button
        runButton = new Button
        {
            AutoSize = true,
            Margin = new Padding(0, 0, 5, 5), // Consistent right and bottom margin
            Enabled = false // Initially disabled
        };
        runButton.Click += RunButton_Click; // Add click event handler

        // Initialize Manual Button
        manualButton = new Button
        {
            AutoSize = true,
            Margin = new Padding(0, 0, 5, 5), // Consistent right and bottom margin
            Enabled = false // Initially disabled
        };
        manualButton.Click += ManualButton_Click;

        // Initialize Previous button
        boxArtPreviousButton = new Button
        {
            Size = new Size(30, 25),
            Enabled = false,
            Anchor = AnchorStyles.Left
        };

        // Initialize Next button
        boxArtNextButton = new Button
        {
            Size = new Size(30, 25),
            Enabled = false,
            Anchor = AnchorStyles.Right
        };

        if (symbolFont != null)
        {
            refreshButton.Font = symbolFont;
            refreshButton.Text = "\u21BB"; // Unicode for Clockwise Open Circle Arrow

            runButton.Font = symbolFont;
            runButton.Text = "\U0001F680"; // Unicode for Rocket

            manualButton.Font = symbolFont;
            manualButton.Text = "\U0001F56E"; // Unicode for Rolled-up Newspaper (used as book/manual symbol)

            boxArtPreviousButton.Font = symbolFont;
            boxArtPreviousButton.Text = "\u25C0"; // Unicode character for "Previous"

            boxArtNextButton.Font = symbolFont;
            boxArtNextButton.Text = "\u25B6"; // Unicode character for "Next"
        }
        else
        {
            refreshButton.Text = "Refresh";
            runButton.Text = "Run";
            manualButton.Text = "Manual";
            boxArtPreviousButton.Text = "Previous";
            boxArtPreviousButton.Text = "Next";
        }




        // Game ListBox Setup
        gameListBox = new ListBox();

        // Initialize gameDetailsTableLayoutPanel for Name label and TextBox
        gameDetailsTableLayoutPanel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill, // Fill the cell in the main TableLayoutPanel
            ColumnCount = 2,
            RowCount = 4, // Increased row count for TabControl
            // AutoSize = true, // Removed: Dock.Fill will manage size
            Margin = new Padding(0, 5, 5, 5) // Added margin (top, right, bottom, left)
        };
        gameDetailsTableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize)); // For Label
        gameDetailsTableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F)); // For TextBox

        // --- Action Buttons Panel (Run and Manual) ---
        FlowLayoutPanel actionButtonsPanel = new FlowLayoutPanel
        {
            FlowDirection = FlowDirection.LeftToRight,
            Dock = DockStyle.Top, // Ensure it uses the available width of its cell
            AutoSize = true,
            Margin = new Padding(0, 0, 0, 5) // Bottom margin for the panel
        };

        actionButtonsPanel.Controls.Add(runButton);
        actionButtonsPanel.Controls.Add(manualButton);

        // --- ToolTips for Action Buttons ---
        ToolTip actionButtonToolTip = new ToolTip();
        actionButtonToolTip.SetToolTip(runButton, "Launch");
        actionButtonToolTip.SetToolTip(manualButton, "Manual");
        actionButtonToolTip.SetToolTip(refreshButton, "Reload");

        // Define Row Styles for gameDetailsTableLayoutPanel (order matters for visual layout)
        gameDetailsTableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // Row 0: Run button
        gameDetailsTableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // Row 1: Name (label and textbox)
        gameDetailsTableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 250F)); // Row 2: Media Panel (Synopsis & Box Art)
        gameDetailsTableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100F)); // Row 3: TabControl (fills remaining space)

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

        gameDetailsTableLayoutPanel.Controls.Add(actionButtonsPanel, 0, 0);
        gameDetailsTableLayoutPanel.SetColumnSpan(actionButtonsPanel, 2); // Span panel across both columns

        gameDetailsTableLayoutPanel.Controls.Add(gameNameLabel, 0, 1);
        gameDetailsTableLayoutPanel.Controls.Add(gameNameTextBox, 1, 1);

        // --- Synopsis TextBox Setup ---
        synopsisTextBox = new TextBox
        {
            Dock = DockStyle.Fill,
            Multiline = true,
            ReadOnly = true,
            ScrollBars = ScrollBars.Vertical,
            Margin = new Padding(3, 0, 0, 0) // Left margin (will be on the right)
        };

        // Initialize PictureBox for Box Art
        boxArtPictureBox = new PictureBox
        {
            Dock = DockStyle.Fill,
            SizeMode = PictureBoxSizeMode.Zoom,
            BorderStyle = BorderStyle.FixedSingle,
            BackColor = Color.Black
            // Margin is handled by its container
        };

        // --- Box Art Panel with Carousel Controls ---
        TableLayoutPanel boxArtPanel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            RowCount = 2,
            ColumnCount = 1,
            Margin = new Padding(0, 0, 3, 0) // Right margin for the whole panel
        };
        boxArtPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100F)); // Image
        boxArtPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));    // Controls
        boxArtPanel.Controls.Add(boxArtPictureBox, 0, 0);

        // --- Carousel Controls Panel ---
        TableLayoutPanel carouselControlsPanel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 3,
            RowCount = 1,
            Height = 30,
            Margin = new Padding(0, 3, 0, 0) // Top margin
        };
        carouselControlsPanel.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize)); // Previous button
        carouselControlsPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F)); // Label
        carouselControlsPanel.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize)); // Next button
        carouselControlsPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

        boxArtImageNameLabel = new Label { Text = "", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleCenter };

        boxArtPreviousButton.Click += BoxArtPreviousButton_Click;
        boxArtNextButton.Click += BoxArtNextButton_Click;

        carouselControlsPanel.Controls.Add(boxArtPreviousButton, 0, 0);
        carouselControlsPanel.Controls.Add(boxArtImageNameLabel, 1, 0);
        carouselControlsPanel.Controls.Add(boxArtNextButton, 2, 0);

        boxArtPanel.Controls.Add(carouselControlsPanel, 0, 1);

        // Initialize the Box Art Carousel Manager
        _boxArtCarouselManager = new BoxArtCarouselManager(boxArtPictureBox, boxArtImageNameLabel, boxArtPreviousButton, boxArtNextButton);
        // --- Media Panel (for Synopsis and Box Art) ---
        TableLayoutPanel mediaPanel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1
        };
        mediaPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F)); // Column 0: Box Art
        mediaPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F)); // Column 1: Synopsis
        mediaPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100F)); // Fill available height

        mediaPanel.Controls.Add(boxArtPanel, 0, 0); // Add the new composite panel
        mediaPanel.Controls.Add(synopsisTextBox, 1, 0); // Synopsis text box on the right

        gameDetailsTableLayoutPanel.Controls.Add(mediaPanel, 0, 2); // Add mediaPanel to row 2
        gameDetailsTableLayoutPanel.SetColumnSpan(mediaPanel, 2); // Span mediaPanel across both columns

        // --- TabControl Setup for additional game details ---
        gameDetailsTabControl = new TabControl
        {
            Dock = DockStyle.Fill, // Fill its cell in gameDetailsTableLayoutPanel
            Margin = new Padding(0, 5, 0, 0) // Add some top margin
        };

        // Create and add TabPages
        TabPage runCommandsTab = new TabPage("Run Commands"); // New tab
        runCommandsDataGridView = new DataGridView
        {
            Dock = DockStyle.Fill,
            Margin = new Padding(3), // Add some padding within the tab page
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            AllowUserToResizeRows = false,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            BackgroundColor = SystemColors.Window,
            BorderStyle = BorderStyle.None,
            CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal,
            ColumnHeadersVisible = false,
            RowHeadersVisible = false,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            ReadOnly = true
        };
        runCommandsDataGridView.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Command", Name = "Command", FillWeight = 100 });
        runCommandsTab.Controls.Add(runCommandsDataGridView);
        // Existing tabs
        TabPage diskImagesTab = new TabPage("CD-ROM images");

        // --- Layout Panel for CD-ROM Images Tab ---
        TableLayoutPanel isoImagesPanel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1
        };
        isoImagesPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
        isoImagesPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
        isoImagesPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

        diskImagesDataGridView = new DataGridView
        {
            Dock = DockStyle.Fill,
            Margin = new Padding(3),
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            AllowUserToResizeRows = false,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            BackgroundColor = SystemColors.Window,
            BorderStyle = BorderStyle.None,
            CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal,
            ColumnHeadersVisible = false,
            RowHeadersVisible = false,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            ReadOnly = true
        };

        isoImagesPanel.Controls.Add(diskImagesDataGridView, 0, 0);

        // PictureBox for the selected CD-ROM image
        isoImagePictureBox = new PictureBox
        {
            Dock = DockStyle.Fill,
            Margin = new Padding(3),
            SizeMode = PictureBoxSizeMode.Zoom,
            BorderStyle = BorderStyle.FixedSingle,
            BackColor = Color.Black
        };
        isoImagesPanel.Controls.Add(isoImagePictureBox, 1, 0);

        diskImagesTab.Controls.Add(isoImagesPanel);

        // Modify columns for diskImagesDataGridView
        diskImagesDataGridView.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Name", Name = "DisplayName", FillWeight = 70 });
        diskImagesDataGridView.Columns.Add(new DataGridViewTextBoxColumn
        {
            HeaderText = "Size",
            Name = "FileSize",
            AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
            DefaultCellStyle = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleRight }
        });
        diskImagesDataGridView.SelectionChanged += DiskImagesDataGridView_SelectionChanged;


        TabPage soundtrackTab = new TabPage("Soundtrack");
        TabPage installDiscsTab = new TabPage("Install discs");

        // --- Layout Panel for Install Discs Tab ---
        TableLayoutPanel installDiscsPanel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1
        };
        installDiscsPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
        installDiscsPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
        installDiscsPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

        // DataGridView for install disc images
        installDiscsDataGridView = new DataGridView
        {
            Dock = DockStyle.Fill,
            Margin = new Padding(3),
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            AllowUserToResizeRows = false,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            BackgroundColor = SystemColors.Window,
            BorderStyle = BorderStyle.None,
            CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal,
            ColumnHeadersVisible = false,
            RowHeadersVisible = false,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            ReadOnly = true
        };
        installDiscsDataGridView.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Name", Name = "DisplayName", FillWeight = 70 });
        installDiscsDataGridView.Columns.Add(new DataGridViewTextBoxColumn
        {
            HeaderText = "Size",
            Name = "FileSize",
            AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
            DefaultCellStyle = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleRight }
        });

        installDiscsDataGridView.SelectionChanged += InstallDiscsDataGridView_SelectionChanged;
        installDiscsPanel.Controls.Add(installDiscsDataGridView, 0, 0);

        // PictureBox for the selected install disc image
        diskImagePictureBox = new PictureBox
        {
            Dock = DockStyle.Fill,
            Margin = new Padding(3),
            SizeMode = PictureBoxSizeMode.Zoom,
            BorderStyle = BorderStyle.FixedSingle,
            BackColor = Color.Black
        };
        installDiscsPanel.Controls.Add(diskImagePictureBox, 1, 0);

        installDiscsTab.Controls.Add(installDiscsPanel);

        TabPage walkthroughTab = new TabPage("Walkthrough");
        TabPage cheatsTab = new TabPage("Cheats");
        TabPage notesTab = new TabPage("Notes");



        gameDetailsTabControl.TabPages.AddRange(new TabPage[] {
            runCommandsTab, diskImagesTab, soundtrackTab, installDiscsTab, walkthroughTab, cheatsTab, notesTab
        });
        gameDetailsTableLayoutPanel.Controls.Add(gameDetailsTabControl, 0, 3); // Add TabControl to row 3
        gameDetailsTableLayoutPanel.SetColumnSpan(gameDetailsTabControl, 2); // Span TabControl across both columns

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

        // Add the main TableLayoutPanel directly to the Form's controls
        this.Controls.Add(tableLayoutPanel);

        this.Controls.Add(menuStrip);
    }

    private async void TopForm_Load(object? sender, EventArgs e)
    {
        await _appConfigService.LoadOrCreateConfigurationAsync(this);

        if (string.IsNullOrEmpty(_appConfigService.LibraryPath) || !Directory.Exists(_appConfigService.LibraryPath))
        {
            MessageBox.Show(this, "Game library path is not configured or is invalid. Please set it via 'Settings > Set game library location...'. Game loading will be skipped.", "Library Path Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
        _boxArtCarouselManager?.Clear();
        if (synopsisTextBox != null)
        {
            synopsisTextBox.Text = string.Empty;
        }
        if (runButton != null)
        {
            runButton.Enabled = false;
        }
        if (manualButton != null)
        {
            manualButton.Enabled = false;
        }
        if (diskImagesDataGridView != null)
        {
            diskImagesDataGridView.Rows.Clear();
        }
        if (isoImagePictureBox != null && isoImagePictureBox.Image != null)
        {
            isoImagePictureBox.Image.Dispose();
            isoImagePictureBox.Image = null;
        }
        if (installDiscsDataGridView != null)
        {
            installDiscsDataGridView.Rows.Clear();
        }
        if (diskImagePictureBox != null && diskImagePictureBox.Image != null)
        {
            diskImagePictureBox.Image.Dispose();
            diskImagePictureBox.Image = null;
        }
        if (runCommandsDataGridView != null)
        {
            runCommandsDataGridView.Rows.Clear();
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
        if (gameListBox == null) return;

        if (gameListBox.InvokeRequired)
        {
            gameListBox.Invoke(new System.Windows.Forms.MethodInvoker(() => PopulateListBox(gameConfigs)));
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
        if (manualButton != null)
        {
            manualButton.Enabled = false; // Disable by default, enable if manual exists
        }
        if (diskImagesDataGridView != null)
        {
            diskImagesDataGridView.Rows.Clear();
        }
        if (isoImagePictureBox != null)
        {
            isoImagePictureBox.Image?.Dispose();
            isoImagePictureBox.Image = null;
        }

        if (installDiscsDataGridView != null)
        {
            installDiscsDataGridView.Rows.Clear();
        }
        if (diskImagePictureBox != null)
        {
            diskImagePictureBox.Image?.Dispose();
            diskImagePictureBox.Image = null;
        }
        if (runCommandsDataGridView != null)
        {
            runCommandsDataGridView.Rows.Clear();
        }

        if (gameNameTextBox != null && gameListBox != null)
        { // General null check for UI elements

            GameConfiguration? selectedGame = gameListBox.SelectedItem as GameConfiguration;

            if (selectedGame != null)
            {
                gameNameTextBox.Text = selectedGame.GameName;

                if (manualButton != null && !string.IsNullOrEmpty(selectedGame.ManualPath) && File.Exists(selectedGame.ManualPath))
                {
                    manualButton.Enabled = true;
                }

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

                // Populate Disk Images ListBox
                if (diskImagesDataGridView != null)
                {
                    foreach (DiscImageInfo isoInfo in selectedGame.IsoImages)
                    {
                        var rowIndex = diskImagesDataGridView.Rows.Add(isoInfo.ToString(), FormatTools.FormatFileSize(isoInfo.FileSizeInBytes));
                        diskImagesDataGridView.Rows[rowIndex].Tag = isoInfo;
                    }
                }

                if (diskImagesDataGridView != null)
                {
                    if (diskImagesDataGridView.Rows.Count > 0)
                    {
                        diskImagesDataGridView.Rows[0].Selected = true;
                    }
                    UpdateIsoImageForSelection();
                }

                // Populate Run Commands DataGridView
                if (runCommandsDataGridView != null)
                {
                    foreach (string command in selectedGame.DosboxCommands)
                    {
                        runCommandsDataGridView.Rows.Add(command);
                    }
                }

                // Populate Install Discs ListBox
                if (installDiscsDataGridView != null)
                {
                    foreach (DiscImageInfo discInfo in selectedGame.DiscImages)
                    {
                        var rowIndex = installDiscsDataGridView.Rows.Add(discInfo.ToString(), FormatTools.FormatFileSize(discInfo.FileSizeInBytes));
                        installDiscsDataGridView.Rows[rowIndex].Tag = discInfo;
                    }

                    // Trigger selection changed to load the image for the first item
                    // Select the first row and ensure its image is loaded.
                    if (installDiscsDataGridView.Rows.Count > 0)
                    {
                        installDiscsDataGridView.Rows[0].Selected = true;
                    }
                    UpdateInstallDiscImageForSelection();
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

            // Handle Box Art Carousel via manager
            var boxArtPaths = new List<string>();
            if (selectedGame != null)
            {
                if (selectedGame.HasFrontBoxArt)
                {
                    boxArtPaths.Add(selectedGame.FrontBoxArtPath);
                }
                if (selectedGame.HasBackBoxArt)
                {
                    boxArtPaths.Add(selectedGame.BackBoxArtPath);
                }
            }
            _boxArtCarouselManager?.LoadImages(boxArtPaths);
        }
    }

    private void RunButton_Click(object? sender, EventArgs e)
    {
        if (gameListBox?.SelectedItem is GameConfiguration selectedGame)
        {
            LaunchDosBox(selectedGame);
        }
    }

    private void ManualButton_Click(object? sender, EventArgs e)
    {
        if (gameListBox?.SelectedItem is GameConfiguration selectedGame &&
            !string.IsNullOrEmpty(selectedGame.ManualPath) &&
            File.Exists(selectedGame.ManualPath))
        {
            try
            {
                ProcessStartInfo psi = new ProcessStartInfo
                {
                    FileName = selectedGame.ManualPath,
                    UseShellExecute = true // Important for opening with default app
                };
                Process.Start(psi);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, $"Could not open manual '{selectedGame.ManualPath}'.\nError: {ex.Message}", "Error Opening Manual", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        else
        {
            MessageBox.Show(this, "No manual found or manual path is invalid for the selected game.", "Manual Not Available", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            if (manualButton != null)
            {
                manualButton.Enabled = false; // Ensure it's disabled if something went wrong
            }
        }
    }


    private async void RefreshButton_Click(object? sender, EventArgs e)
    {
        await RefreshGameListAsync();
    }

    private async void SetDosboxLocationMenuItem_Click(object? sender, EventArgs e)
    {
        bool pathUpdated = await _appConfigService.ManuallySetDosboxPathAsync(this);
        if (pathUpdated)
        {
            await _appConfigService.SaveConfigurationAsync(this);
            MessageBox.Show(this, "DOSBox location has been updated.", "DOSBox Location Saved", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }

    private async void SetGameLibraryLocationMenuItem_Click(object? sender, EventArgs e)
    {
        bool pathUpdated = await _appConfigService.ManuallySetLibraryPathAsync(this);
        if (pathUpdated)
        {
            await _appConfigService.SaveConfigurationAsync(this);
            MessageBox.Show(this, "Game library location has been updated. Refreshing game list...", "Library Location Saved", MessageBoxButtons.OK, MessageBoxIcon.Information);
            await RefreshGameListAsync();
        }
        // Optional: else, show a message that the operation was cancelled or no path was chosen.
    }


    private void ExitMenuItem_Click(object? sender, EventArgs e)
    {
        Application.Exit();
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

        foreach (DiscImageInfo isoInfo in gameConfig.IsoImages)
        {
            // Check if the "isos" directory exists for this game if ISOs are listed
            if (!Directory.Exists(gameConfig.IsoBasePath))
            {
                MessageBox.Show(this, $"ISO directory '{gameConfig.IsoBasePath}' not found for game '{gameConfig.GameName}'.", "Launch Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                break; // No point in checking other ISOs if the directory is missing
            }

            // ISO paths in game.cfg are relative to the "isos" directory within the game's folder
            string fullIsoPathOnHost = Path.Combine(gameConfig.IsoBasePath, isoInfo.ImgFileName);

            if (File.Exists(fullIsoPathOnHost))
            {
                dosboxArgs.Add($"-c \"IMGMOUNT D '{fullIsoPathOnHost}' -t iso\"");
            }
            else
            {
                MessageBox.Show(this, $"ISO/CUE file '{isoInfo.ImgFileName}' not found in '{gameConfig.IsoBasePath}' for game '{gameConfig.GameName}'.", "Launch Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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

    private void UpdateInstallDiscImageForSelection()
    {
        // Clear previous image
        if (diskImagePictureBox != null)
        {
            diskImagePictureBox.Image?.Dispose();
            diskImagePictureBox.Image = null;
        }

        if (installDiscsDataGridView?.SelectedRows.Count > 0)
        {
            var selectedRow = installDiscsDataGridView.SelectedRows[0];
            if (selectedRow.Tag is DiscImageInfo selectedDisc)
            {
                if (!string.IsNullOrEmpty(selectedDisc.PngFilePath) && File.Exists(selectedDisc.PngFilePath))
                {
                    try
                    {
                        // Load the new image. The PictureBox's SizeMode is already set to Zoom, which maintains aspect ratio.
                        diskImagePictureBox.Image = Image.FromFile(selectedDisc.PngFilePath);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error loading disc image picture '{selectedDisc.PngFilePath}': {ex.Message}");
                    }
                }
            }
        }
    }
    private void InstallDiscsDataGridView_SelectionChanged(object? sender, EventArgs e)
    {
        UpdateInstallDiscImageForSelection();
    }

    private void UpdateIsoImageForSelection()
    {
        // Clear previous image
        if (isoImagePictureBox != null)
        {
            isoImagePictureBox.Image?.Dispose();
            isoImagePictureBox.Image = null;
        }

        if (diskImagesDataGridView?.SelectedRows.Count > 0)
        {
            var selectedRow = diskImagesDataGridView.SelectedRows[0];
            if (selectedRow.Tag is DiscImageInfo selectedDisc)
            {
                if (!string.IsNullOrEmpty(selectedDisc.PngFilePath) && File.Exists(selectedDisc.PngFilePath))
                {
                    try
                    {
                        isoImagePictureBox.Image = Image.FromFile(selectedDisc.PngFilePath);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error loading disc image picture '{selectedDisc.PngFilePath}': {ex.Message}");
                    }
                }
            }
        }
    }

    private void DiskImagesDataGridView_SelectionChanged(object? sender, EventArgs e)
    {
        UpdateIsoImageForSelection();
    }
    
    private void BoxArtPreviousButton_Click(object? sender, EventArgs e)
    {
        _boxArtCarouselManager?.GoToPrevious();
    }

    private void BoxArtNextButton_Click(object? sender, EventArgs e)
    {
        _boxArtCarouselManager?.GoToNext();
    }

    /// <summary>
    /// Clean up any resources being used.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose(bool disposing)
    {
        if (disposing) {
            _boxArtCarouselManager?.Dispose();
        }
        base.Dispose(disposing);
    }
}