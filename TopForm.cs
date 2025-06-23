using System.Diagnostics;
using System.Reflection;
using DOSGameCollection.Models;
using DOSGameCollection.UI;
using DOSGameCollection.Services;
using LibVLCSharp.Shared;
using LibVLCSharp.WinForms;

namespace DOSGameCollection;

public class TopForm : Form
{
    private ListBox? gameListBox;
    private Button? runButton;
    private Button? manualButton;
    private Button? refreshButton; 
    private Button? editGameDataButton; // Renamed from editGameNameButton
    private Button? saveGameDataButton; // New button
    private TableLayoutPanel? gameDetailsTableLayoutPanel;
    private Label? gameNameLabel;
    private TextBox? gameNameTextBox;
    private TextBox? releaseYearTextBox;
    private ComboBox? parentalRatingComboBox;
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
    private TextBox? runCommandsTextBox; 
    private List<GameConfiguration> _loadedGameConfigs = [];
    private AppConfigService _appConfigService;
    private BoxArtCarouselManager? _boxArtCarouselManager;

    private LibVLC? _libVLC;
    private MediaPlayer? _mediaPlayer;
    private VideoView? _videoView;
    public TopForm()
    {
        Core.Initialize(); // Must be called before using LibVLCSharp
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
                    AppLogger.Log($"Warning: Embedded resource '{resourceName}' not found.");
                }
            }
        }
        catch (Exception ex)
        {
            AppLogger.Log($"Error loading embedded icon: {ex.Message}");
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

        // Help Menu
        ToolStripMenuItem helpMenu = new("&Help");
        helpMenu.Alignment = ToolStripItemAlignment.Right; // Align to the right

        ToolStripMenuItem aboutMenuItem = new("&About");
        aboutMenuItem.Click += AboutMenuItem_Click;

        ToolStripMenuItem consoleLogMenuItem = new("Console &Log");
        consoleLogMenuItem.Click += ConsoleLogMenuItem_Click;

        helpMenu.DropDownItems.Add(consoleLogMenuItem);
        helpMenu.DropDownItems.Add(aboutMenuItem);

        menuStrip.Items.AddRange(new ToolStripItem[] {
            fileMenu,
            settingsMenu,
            helpMenu
        });

        Controls.Add(menuStrip);
        MainMenuStrip = menuStrip;

        TableLayoutPanel tableLayoutPanel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
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
            Margin = new Padding(5, 5, 5, 3), // Margin (left, top, right, bottom)
            Text = symbolFont != null ? "\u21BB" : "Refresh"
        };
        if (symbolFont != null) { refreshButton.Font = symbolFont; }
        refreshButton.Click += RefreshButton_Click;

        // Initialize Run Button
        runButton = new Button
        {
            AutoSize = true,
            Margin = new Padding(0, 0, 5, 5), // Consistent right and bottom margin
            Enabled = false, // Initially disabled
            Text = symbolFont != null ? "\U0001F680" : "Run"
        };
        if (symbolFont != null) { runButton.Font = symbolFont; }
        runButton.Click += RunButton_Click; // Add click event handler

        // Initialize Manual Button
        manualButton = new Button
        {
            AutoSize = true,
            Margin = new Padding(0, 0, 5, 5), // Consistent right and bottom margin
            Enabled = false, // Initially disabled
            Text = symbolFont != null ? "\U0001F56E" : "Manual"
        };
        if (symbolFont != null) { manualButton.Font = symbolFont; }
        manualButton.Click += ManualButton_Click;

        editGameDataButton = new Button
        {
            AutoSize = true,
            Margin = new Padding(0, 0, 5, 5),
            Enabled = false, // Initially disabled
            Text = symbolFont != null ? "\u270E" : "Edit"
        };
        if (symbolFont != null) { editGameDataButton.Font = symbolFont; }
        editGameDataButton.Click += EditGameDataButton_Click;

        saveGameDataButton = new Button
        {
            AutoSize = true,
            Margin = new Padding(0, 0, 5, 5),
            Visible = false, // Initially hidden
            Text = symbolFont != null ? "\uD83D\uDCBE" : "Save" // Floppy disk emoji for save
        };
        if (symbolFont != null) { saveGameDataButton.Font = symbolFont; }
        saveGameDataButton.Click += SaveGameDataButton_Click;

        // Initialize Previous button
        boxArtPreviousButton = new Button
        {
            Size = new Size(30, 25),
            Enabled = false,
            Anchor = AnchorStyles.Left,
            Text = symbolFont != null ? "\u25C0" : "Previous"
        };
        if (symbolFont != null) { boxArtPreviousButton.Font = symbolFont; }

        // Initialize Next button
        boxArtNextButton = new Button
        {
            Size = new Size(30, 25),
            Enabled = false,
            Anchor = AnchorStyles.Right,
            Text = symbolFont != null ? "\u25B6" : "Next"
        };
        if (symbolFont != null) { boxArtNextButton.Font = symbolFont; }

        gameListBox = new ListBox();

        gameDetailsTableLayoutPanel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 4,
            Margin = new Padding(0, 5, 5, 5)
        };
        gameDetailsTableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));

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
        actionButtonsPanel.Controls.Add(editGameDataButton);
        actionButtonsPanel.Controls.Add(saveGameDataButton);

        // --- ToolTips for Action Buttons ---
        ToolTip actionButtonToolTip = new();
        actionButtonToolTip.SetToolTip(runButton, "Launch");
        actionButtonToolTip.SetToolTip(saveGameDataButton, "Save Game Data");
        actionButtonToolTip.SetToolTip(manualButton, "Manual");
        actionButtonToolTip.SetToolTip(refreshButton, "Reload");
        actionButtonToolTip.SetToolTip(editGameDataButton, "Edit Game Data");

        // Define Row Styles for gameDetailsTableLayoutPanel (order matters for visual layout)
        gameDetailsTableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // Row 0: Run button
        gameDetailsTableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // Row 1: Game Name
        gameDetailsTableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 250F)); // Row 2: Media Panel (Synopsis & Box Art)
        gameDetailsTableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100F)); // Row 3: TabControl (fills remaining space)

        // --- Game Name Container Panel (Label and TextBox) ---
        TableLayoutPanel gameNameContainerPanel = new()
        {
            Dock = DockStyle.Fill, // Fill the cell
            ColumnCount = 2,
            RowCount = 4, // Use four rows now
            Margin = new Padding(0, 0, 3, 0) // Right margin
        };
        gameNameContainerPanel.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        gameNameContainerPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        gameNameContainerPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));      // Row for content
        gameNameContainerPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));      // Row for Release Year
        gameNameContainerPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));      // Row for Parental Rating
        gameNameContainerPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100F)); // Spacer row

        gameNameLabel = new Label
        {
            Text = "Game Name",
            Anchor = AnchorStyles.Left,
            TextAlign = ContentAlignment.MiddleLeft,
            AutoSize = true,
            Margin = new Padding(0, 0, 3, 0)
        };

        gameNameTextBox = new TextBox
        {
            Anchor = AnchorStyles.Left | AnchorStyles.Right,
            ReadOnly = true,
        };
        gameNameTextBox.KeyDown += GameNameTextBox_KeyDown;

        gameNameContainerPanel.Controls.Add(gameNameLabel, 0, 0);
        gameNameContainerPanel.Controls.Add(gameNameTextBox, 1, 0);

        // --- New Release Year controls ---
        Label releaseYearLabel = new Label
        {
            Text = "Release Year",
            Anchor = AnchorStyles.Left,
            TextAlign = ContentAlignment.MiddleLeft,
            AutoSize = true,
            Margin = new Padding(0, 3, 3, 0) // Top and right margin
        };

        releaseYearTextBox = new TextBox
        {
            Anchor = AnchorStyles.Left, // Don't stretch, keep it small
            ReadOnly = true,
            Width = 50, // Good for 4 digits + padding
            MaxLength = 4
        };
        releaseYearTextBox.KeyPress += ReleaseYearTextBox_KeyPress;

        gameNameContainerPanel.Controls.Add(releaseYearLabel, 0, 1);
        gameNameContainerPanel.Controls.Add(releaseYearTextBox, 1, 1);

        // --- New Parental Rating controls ---
        Label parentalRatingLabel = new Label
        {
            Text = "Parental Rating",
            Anchor = AnchorStyles.Left,
            TextAlign = ContentAlignment.MiddleLeft,
            AutoSize = true,
            Margin = new Padding(0, 3, 3, 0) // Top and right margin
        };

        parentalRatingComboBox = new ComboBox
        {
            Anchor = AnchorStyles.Left,
            Enabled = false, // Read-only initially
            DropDownStyle = ComboBoxStyle.DropDownList, // Prevent free-form text
            Width = 100
        };
        parentalRatingComboBox.Items.AddRange(new object[] {
            "", "E", "E 10+", "T", "M 17+", "AO 18+", "RP", "RP LM 17+"
        });

        gameNameContainerPanel.Controls.Add(parentalRatingLabel, 0, 2);
        gameNameContainerPanel.Controls.Add(parentalRatingComboBox, 1, 2);

        // --- Run Commands Layout Table ---
        TableLayoutPanel runCommandsLayoutTable = new()
        {
            Dock = DockStyle.Fill, // Fill the cell
            ColumnCount = 1,
            RowCount = 2,
            Margin = new Padding(3, 0, 0, 0) // Left margin
        };
        runCommandsLayoutTable.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // For Label
        runCommandsLayoutTable.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // For TextBox

        Label runCommandsLabel = new Label
        {
            Text = "Run Commands",
            Anchor = AnchorStyles.Left,
            TextAlign = ContentAlignment.MiddleLeft,
            AutoSize = true,
            Margin = new Padding(0, 0, 0, 3) // Bottom margin
        };

        // Initialize runCommandsTextBox here, moved from the tab setup
        runCommandsTextBox = new TextBox
        {
            Dock = DockStyle.Fill,
            Multiline = true,
            ReadOnly = true,
            ScrollBars = ScrollBars.Vertical,
            Font = new Font("Consolas", 9.75F, FontStyle.Regular),
            Height = 75 // Set a fixed height for roughly 4 lines
        };

        runCommandsLayoutTable.Controls.Add(runCommandsLabel, 0, 0);
        runCommandsLayoutTable.Controls.Add(runCommandsTextBox, 0, 1);

        // --- New container for Game Name and Run Commands ---
        TableLayoutPanel gameDataPanel = new()
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 2,
            Margin = new Padding(0)
        };
        gameDataPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
        gameDataPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
        gameDataPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));      // Row for content
        gameDataPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100F)); // Spacer row to push content up

        gameDataPanel.Controls.Add(gameNameContainerPanel, 0, 0);
        gameDataPanel.Controls.Add(runCommandsLayoutTable, 1, 0);

        gameDetailsTableLayoutPanel.Controls.Add(actionButtonsPanel, 0, 0);
        gameDetailsTableLayoutPanel.Controls.Add(gameDataPanel, 0, 1); // Add the new composite panel

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

        // --- LibVLC VideoView Setup ---
        _libVLC = new LibVLC();
        _mediaPlayer = new MediaPlayer(_libVLC);
        _mediaPlayer.Mute = true;
        _videoView = new VideoView
        {
            MediaPlayer = _mediaPlayer,
            Dock = DockStyle.Fill,
            BackColor = Color.Black,
            Visible = false // Initially hidden
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
        boxArtPanel.Controls.Add(_videoView, 0, 0); // Add VideoView to the same cell

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
        _boxArtCarouselManager = new BoxArtCarouselManager(boxArtPictureBox, _videoView, _mediaPlayer, _libVLC, boxArtImageNameLabel, boxArtPreviousButton, boxArtNextButton);
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

        // --- TabControl Setup for additional game details ---
        gameDetailsTabControl = new TabControl
        {
            Dock = DockStyle.Fill, // Fill its cell in gameDetailsTableLayoutPanel
            Margin = new Padding(0, 5, 0, 0) // Add some top margin
        };

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
        diskImagePictureBox = new PictureBox // This is the bug fix you asked about
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
            diskImagesTab, soundtrackTab, installDiscsTab, walkthroughTab, cheatsTab, notesTab
        });
        gameDetailsTableLayoutPanel.Controls.Add(gameDetailsTabControl, 0, 3); // Add TabControl to row 3

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
        if (releaseYearTextBox != null)
        {
            releaseYearTextBox.Text = string.Empty;
        }
        if (parentalRatingComboBox != null)
        {
            parentalRatingComboBox.SelectedIndex = -1;
            parentalRatingComboBox.Enabled = false;
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
        if (editGameDataButton != null)
        {
            editGameDataButton.Enabled = false;
            editGameDataButton.Visible = true;
        }
        if (saveGameDataButton != null) saveGameDataButton.Visible = false;
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
        if (runCommandsTextBox != null)
        {
            runCommandsTextBox.Clear();
            runCommandsTextBox.ReadOnly = true;
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
        if (editGameDataButton != null)
        {
            editGameDataButton.Enabled = (gameListBox?.SelectedItem != null);
            editGameDataButton.Visible = true; // Ensure edit button is visible
        }
        if (saveGameDataButton != null) saveGameDataButton.Visible = false; // Ensure save button is hidden
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
        if (runCommandsTextBox != null)
        {
            runCommandsTextBox.Clear();
            runCommandsTextBox.ReadOnly = true;
        }

        if (gameNameTextBox != null && gameListBox != null)
        { // General null check for UI elements

            gameNameTextBox.ReadOnly = true;
            GameConfiguration? selectedGame = gameListBox.SelectedItem as GameConfiguration;

            if (selectedGame != null)
            {
                gameNameTextBox.Text = selectedGame.GameName;
                if (releaseYearTextBox != null)
                {
                    releaseYearTextBox.Text = selectedGame.ReleaseYear?.ToString() ?? string.Empty;
                    releaseYearTextBox.ReadOnly = true;
                }
                if (parentalRatingComboBox != null)
                {
                    parentalRatingComboBox.SelectedItem = selectedGame.ParentalRating ?? "";
                    parentalRatingComboBox.Enabled = false;
                }

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
                            AppLogger.Log($"Error reading synopsis file '{selectedGame.SynopsisFilePath}': {ex.Message}");
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

                    if (diskImagesDataGridView.Rows.Count > 0)
                    {
                        diskImagesDataGridView.Rows[0].Selected = true;
                    }

                    UpdateIsoImageForSelection();
                }

                // Populate Run Commands TextBox
                if (runCommandsTextBox != null)
                {
                    runCommandsTextBox.Text = string.Join(Environment.NewLine, selectedGame.DosboxCommands);
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
                if (releaseYearTextBox != null)
                {
                    releaseYearTextBox.Text = string.Empty;
                }
                if (parentalRatingComboBox != null)
                {
                    parentalRatingComboBox.SelectedIndex = -1;
                }
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
                if (selectedGame.CaptureImagePaths.Any())
                {
                    boxArtPaths.AddRange(selectedGame.CaptureImagePaths);
                }
                if (selectedGame.VideoPaths.Any())
                {
                    boxArtPaths.AddRange(selectedGame.VideoPaths);
                }
            }
            _boxArtCarouselManager?.LoadImages(boxArtPaths);
        }
    }

    private void RunButton_Click(object? sender, EventArgs e)
    {
        if (gameListBox?.SelectedItem is GameConfiguration selectedGame)
        {
            GameLauncherService.LaunchGame(selectedGame, _appConfigService.DosboxExePath, this);
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
                        if (diskImagePictureBox != null)
                        {
                            diskImagePictureBox.Image = Image.FromFile(selectedDisc.PngFilePath);
                        }
                    }
                    catch (Exception ex)
                    {
                        AppLogger.Log($"Error loading disc image picture '{selectedDisc.PngFilePath}': {ex.Message}");
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
                        if (isoImagePictureBox != null)
                        {
                            isoImagePictureBox.Image = Image.FromFile(selectedDisc.PngFilePath);
                        }
                    }
                    catch (Exception ex)
                    {
                        AppLogger.Log($"Error loading disc image picture '{selectedDisc.PngFilePath}': {ex.Message}");
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

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _boxArtCarouselManager?.Dispose();
            _mediaPlayer?.Dispose();
            _libVLC?.Dispose();
        }
        base.Dispose(disposing);
    }
    
    private void ReleaseYearTextBox_KeyPress(object? sender, KeyPressEventArgs e)
    {
        // Allow only digits and control characters (like backspace)
        if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
        {
            e.Handled = true;
        }
    }

    private void AboutMenuItem_Click(object? sender, EventArgs e)
    {
        using AboutDialog aboutDialog = new();
        aboutDialog.ShowDialog(this);
    }

    private void ConsoleLogMenuItem_Click(object? sender, EventArgs e)
    {
        string allLogs = AppLogger.GetAllLogs();
        using ConsoleLogDialog logDialog = new(allLogs);
        logDialog.ShowDialog(this);
    }

    private void EditGameDataButton_Click(object? sender, EventArgs e)
    {
        if (gameNameTextBox == null || releaseYearTextBox == null || parentalRatingComboBox == null || runCommandsTextBox == null || editGameDataButton == null || saveGameDataButton == null) return;

        gameNameTextBox.ReadOnly = false;
        runCommandsTextBox.ReadOnly = false;
        releaseYearTextBox.ReadOnly = false;
        parentalRatingComboBox.Enabled = true;

        editGameDataButton.Visible = false;
        saveGameDataButton.Visible = true;

        gameNameTextBox.Focus();
        gameNameTextBox.SelectAll();
    }

    private void GameNameTextBox_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Enter)
        {
            e.SuppressKeyPress = true; // Stop the 'ding' sound
            // Do not save on Enter, only suppress the key press to prevent newline
        }
    }

    private async void SaveGameDataButton_Click(object? sender, EventArgs e)
    {
        if (gameListBox?.SelectedItem is not GameConfiguration selectedGame ||
            gameNameTextBox == null ||
            releaseYearTextBox == null ||
            parentalRatingComboBox == null ||
            runCommandsTextBox == null ||
            editGameDataButton == null ||
            saveGameDataButton == null)
        {
            return;
        }

        string newName = gameNameTextBox.Text.Trim();
        var newCommands = runCommandsTextBox.Lines.ToList();
        string yearText = releaseYearTextBox.Text.Trim();
        string? newRating = parentalRatingComboBox.SelectedItem?.ToString();

        int? newYear = null;
        if (!string.IsNullOrEmpty(yearText))
        {
            if (!int.TryParse(yearText, out int parsedYear))
            {
                MessageBox.Show(this, "Release Year must be a valid number.", "Invalid Input", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (parsedYear <= 1900 || parsedYear > DateTime.Now.Year)
            {
                MessageBox.Show(this, $"Release Year must be between 1901 and {DateTime.Now.Year}.", "Invalid Year", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            newYear = parsedYear;
        }

        string originalName = selectedGame.GameName;
        var originalCommands = selectedGame.DosboxCommands;
        var originalRating = selectedGame.ParentalRating;

        // Revert UI state regardless of change
        gameNameTextBox.ReadOnly = true;
        runCommandsTextBox.ReadOnly = true;
        releaseYearTextBox.ReadOnly = true;
        parentalRatingComboBox.Enabled = false;
        editGameDataButton.Visible = true;
        saveGameDataButton.Visible = false;

        // Check if anything actually changed
        bool nameChanged = !string.IsNullOrWhiteSpace(newName) && !newName.Equals(originalName, StringComparison.Ordinal);
        bool commandsChanged = !newCommands.SequenceEqual(originalCommands);
        bool yearChanged = selectedGame.ReleaseYear != newYear;
        bool ratingChanged = originalRating != newRating;

        if (!nameChanged && !commandsChanged && !yearChanged && !ratingChanged)
        {
            // No changes, just revert UI and return
            gameNameTextBox.Text = originalName;
            runCommandsTextBox.Text = string.Join(Environment.NewLine, originalCommands);
            releaseYearTextBox.Text = selectedGame.ReleaseYear?.ToString() ?? string.Empty;
            parentalRatingComboBox.SelectedItem = originalRating ?? "";
            return;
        }

        try
        {
            // Call the consolidated save method
            await GameDataWriterService.UpdateGameDataAsync(selectedGame.ConfigFilePath, newName, newYear, newRating, newCommands);

            // Update in-memory model only if save was successful
            if (nameChanged)
            {
                selectedGame.GameName = newName;
                // Force ListBox item to refresh its text if it's still the selected item
                if (gameListBox.SelectedItem is GameConfiguration currentSelected && currentSelected == selectedGame)
                {
                    gameListBox.Items[gameListBox.SelectedIndex] = selectedGame;
                }
            }
            if (commandsChanged)
            {
                selectedGame.DosboxCommands = newCommands.Where(c => !string.IsNullOrWhiteSpace(c)).ToList();
            }
            if (yearChanged)
            {
                selectedGame.ReleaseYear = newYear;
            }
            if (ratingChanged)
            {
                selectedGame.ParentalRating = newRating;
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, $"Failed to save game data: {ex.Message}", "Save Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            // Revert textboxes to original values on error
            gameNameTextBox.Text = originalName; // Revert on error
            releaseYearTextBox.Text = selectedGame.ReleaseYear?.ToString() ?? string.Empty;
            runCommandsTextBox.Text = string.Join(Environment.NewLine, originalCommands);
            parentalRatingComboBox.SelectedItem = originalRating ?? "";
        }
    }
}