using System.Diagnostics;
using System.Reflection;
using DOSGameCollection.Models;
using DOSGameCollection.UI;
using DOSGameCollection.Services;

namespace DOSGameCollection;

public class TopForm : Form
{
    private ListBox? gameListBox;
    private Button? runButton;
    private Button? manualButton;
    private Button? refreshButton; 
    private Button? editGameDataButton;
    private Button? saveGameDataButton;
    private Button? cancelGameDataButton;
    private TableLayoutPanel? rightColumnPanel;
    private Label? gameNameLabel;
    private TextBox? gameNameTextBox;
    private TextBox? releaseYearTextBox;
    private ComboBox? parentalRatingComboBox;
    private TextBox? developerTextBox;
    private TextBox? publisherTextBox;
    private MenuStrip? menuStrip; 
    private TabControl? extraInformationTabControl;
    private MediaTabPanel? mediaTabPanel;
    private MediaTabPanel? insertsTabPanel;
    private DiscImageTabPanel? isoImagesTabPanel;
    private DiscImageTabPanel? floppyDisksTabPanel;
    private DataGridView? soundtrackDataGridView;
    private PictureBox? soundtrackCoverPictureBox;
    private TextBox? runCommandsTextBox; 
    private TextEditorTabPanel? synopsisTabPanel;
    private TextEditorTabPanel? notesTabPanel;
    private List<GameConfiguration> _loadedGameConfigs = [];
    private readonly AppConfigService _appConfigService;
    public TopForm()
    {
        InitializeComponent();
        _appConfigService = new AppConfigService();
        Load += TopForm_Load;
        KeyPreview = true; // Allows the form to preview key events before the focused control.
        KeyDown += TopForm_KeyDown;
    }

    private void InitializeComponent()
    {
        Text = "DOSGameCollection";
        Name = "TopForm";
        ClientSize = new System.Drawing.Size(800, 600); 
        MinimumSize = new System.Drawing.Size(800, 600); 

        // --- Set Form Icon ---
        try
        {
            Assembly assembly = Assembly.GetExecutingAssembly();

            string resourceName = "DOSGameCollection.appicon.ico"; 

            using (Stream? iconStream = assembly.GetManifestResourceStream(resourceName))
            {
                if (iconStream != null)
                {
                    Icon = new System.Drawing.Icon(iconStream);
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
        helpMenu.Alignment = ToolStripItemAlignment.Right;

        ToolStripMenuItem aboutMenuItem = new("&About");
        aboutMenuItem.Click += AboutMenuItem_Click;

        ToolStripMenuItem consoleLogMenuItem = new("Console &Log");
        consoleLogMenuItem.Click += ConsoleLogMenuItem_Click;

        helpMenu.DropDownItems.Add(consoleLogMenuItem);
        helpMenu.DropDownItems.Add(aboutMenuItem);

        menuStrip.Items.AddRange([
            fileMenu,
            settingsMenu,
            helpMenu
        ]);

        Controls.Add(menuStrip);
        MainMenuStrip = menuStrip;

        TableLayoutPanel mainLayoutPanel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2
        };

        mainLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 200F));
        mainLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));

        var symbolFont = FormatTools.GetSymbolFont();

        // Button initialization

        refreshButton = new Button
        {
            Anchor = AnchorStyles.Left,
            AutoSize = true,
            Margin = new Padding(5, 5, 5, 3),
            Text = symbolFont != null ? "\u21BB" : "Refresh"
        };
        if (symbolFont != null) { refreshButton.Font = symbolFont; }
        refreshButton.Click += RefreshButton_Click;

        runButton = new Button
        {
            AutoSize = true,
            Margin = new Padding(0, 0, 5, 5), 
            Enabled = false,
            Text = symbolFont != null ? "\U0001F680" : "Run"
        };
        if (symbolFont != null) { runButton.Font = symbolFont; }
        runButton.Click += RunButton_Click; 

        manualButton = new Button
        {
            AutoSize = true,
            Margin = new Padding(0, 0, 5, 5),
            Enabled = false,
            Text = symbolFont != null ? "\U0001F56E" : "Manual"
        };
        if (symbolFont != null) { manualButton.Font = symbolFont; }
        manualButton.Click += ManualButton_Click;

        editGameDataButton = new Button
        {
            AutoSize = true,
            Margin = new Padding(0, 0, 5, 5),
            Enabled = false, 
            Text = symbolFont != null ? "\u270E" : "Edit"
        };
        if (symbolFont != null) { editGameDataButton.Font = symbolFont; }
        editGameDataButton.Click += EditGameDataButton_Click;

        saveGameDataButton = new Button
        {
            AutoSize = true,
            Margin = new Padding(0, 0, 5, 5),
            Visible = false, 
            Text = symbolFont != null ? "\uD83D\uDCBE" : "Save"
        };
        if (symbolFont != null) { saveGameDataButton.Font = symbolFont; }
        saveGameDataButton.Click += SaveGameDataButton_Click;

        cancelGameDataButton = new Button
        {
            AutoSize = true,
            Margin = new Padding(0, 0, 5, 5),
            Visible = false,
            Text = symbolFont != null ? "\U0001F5D9" : "Cancel"
        };
        if (symbolFont != null) { cancelGameDataButton.Font = symbolFont; }
        cancelGameDataButton.Click += CancelGameDataButton_Click;

        gameListBox = new ListBox();

        rightColumnPanel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 3,
            Margin = new Padding(0, 5, 5, 5)
        };
        rightColumnPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));

        // --- Action Buttons Panel 
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
        actionButtonsPanel.Controls.Add(cancelGameDataButton);

        // --- ToolTips for Action Buttons ---
        ToolTip actionButtonToolTip = new();
        actionButtonToolTip.SetToolTip(runButton, "Launch");
        actionButtonToolTip.SetToolTip(saveGameDataButton, "Save Game Data");
        actionButtonToolTip.SetToolTip(cancelGameDataButton, "Cancel");
        actionButtonToolTip.SetToolTip(manualButton, "Manual");
        actionButtonToolTip.SetToolTip(refreshButton, "Reload");
        actionButtonToolTip.SetToolTip(editGameDataButton, "Edit Game Data");

        // Define Row Styles for gameDetailsTableLayoutPanel
        rightColumnPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // Row 0: Action buttons
        rightColumnPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // Row 1: Game configuration and details
        rightColumnPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100F)); // Row 2: TabControl (fills remaining space)

        // --- Game Name Container Panel (Label and TextBox) ---
        TableLayoutPanel gamePropsTablePanel = new()
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            ColumnCount = 2,
            RowCount = 6,
            Margin = new Padding(0, 0, 3, 0) // Right margin
        };
        gamePropsTablePanel.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        gamePropsTablePanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));

        gamePropsTablePanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));      // Row for Game name
        gamePropsTablePanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));      // Row for Release Year
        gamePropsTablePanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));      // Row for Parental Rating
        gamePropsTablePanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));      // Row for Developer
        gamePropsTablePanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));      // Row for Publisher
        gamePropsTablePanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100F)); // Spacer row

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
            MaxLength = 100
        };
        gameNameTextBox.KeyDown += GameNameTextBox_KeyDown;
        gameNameTextBox.KeyPress += GameNameTextBox_KeyPress;

        gamePropsTablePanel.Controls.Add(gameNameLabel, 0, 0);
        gamePropsTablePanel.Controls.Add(gameNameTextBox, 1, 0);

        // --- New Release Year controls ---
        Label releaseYearLabel = new()
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

        gamePropsTablePanel.Controls.Add(releaseYearLabel, 0, 1);
        gamePropsTablePanel.Controls.Add(releaseYearTextBox, 1, 1);

        // --- New Parental Rating controls ---
        Label parentalRatingLabel = new()
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

        gamePropsTablePanel.Controls.Add(parentalRatingLabel, 0, 2);
        gamePropsTablePanel.Controls.Add(parentalRatingComboBox, 1, 2);

        // --- New Developer controls ---
        Label developerLabel = new()
        {
            Text = "Developer",
            Anchor = AnchorStyles.Left,
            TextAlign = ContentAlignment.MiddleLeft,
            AutoSize = true,
            Margin = new Padding(0, 3, 3, 0) // Top and right margin
        };

        developerTextBox = new TextBox
        {
            Anchor = AnchorStyles.Left | AnchorStyles.Right,
            ReadOnly = true,
            MaxLength = 100
        };
        developerTextBox.KeyPress += GameNameTextBox_KeyPress;

        gamePropsTablePanel.Controls.Add(developerLabel, 0, 3);
        gamePropsTablePanel.Controls.Add(developerTextBox, 1, 3);

        // --- New Publisher controls ---
        Label publisherLabel = new()
        {
            Text = "Publisher",
            Anchor = AnchorStyles.Left,
            TextAlign = ContentAlignment.MiddleLeft,
            AutoSize = true,
            Margin = new Padding(0, 3, 3, 0) // Top and right margin
        };

        publisherTextBox = new TextBox
        {
            Anchor = AnchorStyles.Left | AnchorStyles.Right,
            ReadOnly = true,
            MaxLength = 100
        };
        publisherTextBox.KeyPress += GameNameTextBox_KeyPress;

        gamePropsTablePanel.Controls.Add(publisherLabel, 0, 4);
        gamePropsTablePanel.Controls.Add(publisherTextBox, 1, 4);

        // --- Run Commands Layout Table ---
        TableLayoutPanel runCommandsPanel = new()
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            ColumnCount = 1,
            RowCount = 2,
            Margin = new Padding(3, 0, 0, 0) // Left margin
        };
        runCommandsPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // For Label
        runCommandsPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // For TextBox

        Label runCommandsLabel = new()
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

        runCommandsPanel.Controls.Add(runCommandsLabel, 0, 0);
        runCommandsPanel.Controls.Add(runCommandsTextBox, 0, 1);

        TableLayoutPanel gameConfigPanel = new() // Information saved on game.cfg
        {
            Dock = DockStyle.Fill,
            AutoSize = true,
            ColumnCount = 2,
            RowCount = 1,
            Margin = new Padding(0)
        };
        gameConfigPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
        gameConfigPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));

        gameConfigPanel.Controls.Add(gamePropsTablePanel, 0, 0);
        gameConfigPanel.Controls.Add(runCommandsPanel, 1, 0);

        rightColumnPanel.Controls.Add(actionButtonsPanel, 0, 0);
        rightColumnPanel.Controls.Add(gameConfigPanel, 0, 1); // Add the new composite panel

        // --- TabControl Setup for additional game details ---
        extraInformationTabControl = new TabControl
        {
            Dock = DockStyle.Fill,
            Margin = new Padding(0, 5, 0, 0),
            Enabled = false // Initially disabled
        };

        TabPage mediaTab = new("Media");
        mediaTabPanel = new MediaTabPanel { Dock = DockStyle.Fill };
        mediaTab.Controls.Add(mediaTabPanel);

        TabPage insertsTab = new("Inserts");
        insertsTabPanel = new MediaTabPanel { Dock = DockStyle.Fill };
        insertsTab.Controls.Add(insertsTabPanel);

        TabPage isoImagesTab = new("CD-ROM");
        isoImagesTabPanel = new DiscImageTabPanel { Dock = DockStyle.Fill };
        isoImagesTab.Controls.Add(isoImagesTabPanel);

        TabPage soundtrackTab = new("Soundtrack");

        // --- Layout Panel for Soundtrack Tab ---
        TableLayoutPanel soundtrackPanel = new()
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1
        };
        soundtrackPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
        soundtrackPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
        soundtrackPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

        // --- DataGridView for Soundtrack List ---
        soundtrackDataGridView = new DataGridView
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
            ReadOnly = true,
            MultiSelect = false
        };

        var musicTypeColumn = new DataGridViewTextBoxColumn { HeaderText = "Type", Name = "Type", AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells };
        if (symbolFont != null) { musicTypeColumn.DefaultCellStyle.Font = FormatTools.GetSymbolFont(10F); }
        soundtrackDataGridView.Columns.Add(musicTypeColumn);
        soundtrackDataGridView.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Name", Name = "Name", FillWeight = 100 });
        var soundtrackLinkColumn = new DataGridViewTextBoxColumn { HeaderText = "Link", Name = "Link", AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells };
        if (symbolFont != null) { soundtrackLinkColumn.DefaultCellStyle.Font = FormatTools.GetSymbolFont(10F); }
        soundtrackDataGridView.Columns.Add(soundtrackLinkColumn);
        soundtrackDataGridView.CellClick += SoundtrackDataGridView_CellClick;
        soundtrackDataGridView.CellMouseEnter += SoundtrackDataGridView_CellMouseEnter;
        soundtrackDataGridView.CellMouseLeave += SoundtrackDataGridView_CellMouseLeave;

        // --- PictureBox for Soundtrack Cover ---
        soundtrackCoverPictureBox = new PictureBox
        {
            Dock = DockStyle.Fill,
            Margin = new Padding(3),
            SizeMode = PictureBoxSizeMode.Zoom,
            BorderStyle = BorderStyle.FixedSingle,
            BackColor = Color.Black
        };

        soundtrackPanel.Controls.Add(soundtrackDataGridView, 0, 0);
        soundtrackPanel.Controls.Add(soundtrackCoverPictureBox, 1, 0);
        soundtrackTab.Controls.Add(soundtrackPanel);

        TabPage floppyDisksTab = new("Floppy disks");
        floppyDisksTabPanel = new DiscImageTabPanel { Dock = DockStyle.Fill };
        floppyDisksTab.Controls.Add(floppyDisksTabPanel);

        TabPage walkthroughTab = new TabPage("Walkthrough");
        TabPage cheatsTab = new TabPage("Cheats");
        TabPage notesTab = new TabPage("Notes");

        TabPage synopsisTab = new("Synopsis");
        synopsisTabPanel = new TextEditorTabPanel
        {
            Dock = DockStyle.Fill
        };
        synopsisTabPanel.EditModeStarted += HandleEditModeStarted;
        synopsisTab.Controls.Add(synopsisTabPanel);

        notesTabPanel = new TextEditorTabPanel
        {
            Dock = DockStyle.Fill
        };
        notesTabPanel.EditModeStarted += HandleEditModeStarted;
        notesTab.Controls.Add(notesTabPanel);


        extraInformationTabControl.TabPages.AddRange([
            mediaTab, synopsisTab, insertsTab, isoImagesTab, soundtrackTab, floppyDisksTab, walkthroughTab, cheatsTab, notesTab
        ]);
        rightColumnPanel.Controls.Add(extraInformationTabControl, 0, 2);

        // --- Left Column Panel (for Refresh Button and Game ListBox) ---
        TableLayoutPanel leftColumnPanel = new()
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

        mainLayoutPanel.Controls.Add(leftColumnPanel, 0, 0); // Add leftColumnPanel to column 0, row 0 of main TLP
        mainLayoutPanel.Controls.Add(rightColumnPanel, 1, 0); // Add to column 1, row 0 of main TLP

        // Add the main TableLayoutPanel directly to the Form's controls
        Controls.Add(mainLayoutPanel);

        Controls.Add(menuStrip);
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
        if (developerTextBox != null)
        {
            developerTextBox.Text = string.Empty;
        }
        if (publisherTextBox != null)
        {
            publisherTextBox.Text = string.Empty;
        }
        mediaTabPanel?.Clear();
        insertsTabPanel?.Clear();
        synopsisTabPanel?.Clear();
        notesTabPanel?.Clear();
        isoImagesTabPanel?.Clear();
        floppyDisksTabPanel?.Clear();
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
        if (cancelGameDataButton != null) cancelGameDataButton.Visible = false;
        if (runCommandsTextBox != null)
        {
            runCommandsTextBox.Clear();
            runCommandsTextBox.ReadOnly = true;
        }
        if (soundtrackDataGridView != null)
        {
            soundtrackDataGridView.Rows.Clear();
        }
        if (soundtrackCoverPictureBox != null && soundtrackCoverPictureBox.Image != null)
        {
            soundtrackCoverPictureBox.Image.Dispose();
            soundtrackCoverPictureBox.Image = null;
        }
        if (extraInformationTabControl != null)
        {
            extraInformationTabControl.Enabled = false;
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
        List<GameConfiguration> gameConfigs = [];

        try
        {
            if (refreshButton != null) refreshButton.Enabled = false;

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
            if (refreshButton != null) refreshButton.Enabled = true;
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
        if (runButton != null)
        {
            runButton.Enabled = (gameListBox?.SelectedItem != null);
        }
        if (manualButton != null)
        {
            manualButton.Enabled = false;
        }
        if (editGameDataButton != null)
        {
            editGameDataButton.Enabled = (gameListBox?.SelectedItem != null);
            editGameDataButton.Visible = true;
        }
        if (saveGameDataButton != null) saveGameDataButton.Visible = false;
        if (cancelGameDataButton != null) cancelGameDataButton.Visible = false;
        isoImagesTabPanel?.Clear();
        floppyDisksTabPanel?.Clear();
        if (runCommandsTextBox != null)
        {
            runCommandsTextBox.Clear();
            runCommandsTextBox.ReadOnly = true;
        }
        synopsisTabPanel?.Clear();
        notesTabPanel?.Clear();
        if (soundtrackDataGridView != null)
        {
            soundtrackDataGridView.Rows.Clear();
        }
        if (soundtrackCoverPictureBox != null)
        {
            soundtrackCoverPictureBox.Image?.Dispose();
            soundtrackCoverPictureBox.Image = null;
        }
        if (extraInformationTabControl != null)
        {
            extraInformationTabControl.Enabled = false; // Disable tabs if no game is selected or selection is cleared
        }

        if (gameNameTextBox != null && gameListBox != null)
        {
            gameNameTextBox.ReadOnly = true;
            GameConfiguration? selectedGame = gameListBox.SelectedItem as GameConfiguration;

            if (selectedGame != null)
            {
                if (extraInformationTabControl != null)
                {
                    extraInformationTabControl.Enabled = true; // Enable tabs when a game is selected
                }
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
                if (developerTextBox != null)
                {
                    developerTextBox.Text = selectedGame.Developer ?? string.Empty;
                }
                if (publisherTextBox != null)
                {
                    publisherTextBox.Text = selectedGame.Publisher ?? string.Empty;
                }

                // Populate Media Tab
                var mediaItems = new List<MediaTabPanel.MediaItem>();
                if (selectedGame.HasFrontBoxArt)
                    mediaItems.Add(new(selectedGame.FrontBoxArtPath, "Front Box Art", MediaTabPanel.MediaType.Image));
                if (selectedGame.HasBackBoxArt)
                    mediaItems.Add(new(selectedGame.BackBoxArtPath, "Back Box Art", MediaTabPanel.MediaType.Image));
                mediaItems.AddRange(selectedGame.CaptureFiles.Select(f => new MediaTabPanel.MediaItem(f.FilePath, f.DisplayName, MediaTabPanel.MediaType.Image)));
                mediaItems.AddRange(selectedGame.VideoFiles.Select(f => new MediaTabPanel.MediaItem(f.FilePath, f.DisplayName, MediaTabPanel.MediaType.Video)));
                mediaTabPanel?.Populate(mediaItems);

                // Populate Inserts Tab
                var insertItems = selectedGame.InsertFiles.Select(f => new MediaTabPanel.MediaItem(f.FilePath, f.DisplayName,
                    Path.GetExtension(f.FilePath).Equals(".pdf", StringComparison.OrdinalIgnoreCase) ? MediaTabPanel.MediaType.Pdf : MediaTabPanel.MediaType.Image
                ));
                insertsTabPanel?.Populate(insertItems);

                PopulateSoundtrackTab(selectedGame);

                isoImagesTabPanel?.Populate(selectedGame.IsoImages);
                floppyDisksTabPanel?.Populate(selectedGame.DiscImages);

                // Load Synopsis and Notes
                if (synopsisTabPanel != null)
                {
                    synopsisTabPanel.FilePath = selectedGame.SynopsisFilePath;
                }
                if (notesTabPanel != null)
                {
                    notesTabPanel.FilePath = Path.Combine(selectedGame.GameDirectoryPath, "notes.txt");
                }

                if (manualButton != null && !string.IsNullOrEmpty(selectedGame.ManualPath) && File.Exists(selectedGame.ManualPath))
                {
                    manualButton.Enabled = true;
                }

                if (runCommandsTextBox != null)
                {
                    runCommandsTextBox.Text = string.Join(Environment.NewLine, selectedGame.DosboxCommands);
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
                if (developerTextBox != null)
                {
                    developerTextBox.Text = string.Empty;
                }
                if (publisherTextBox != null)
                {
                    publisherTextBox.Text = string.Empty;
                }
                mediaTabPanel?.Clear();
                insertsTabPanel?.Clear();
                synopsisTabPanel?.Clear();
                notesTabPanel?.Clear();
                isoImagesTabPanel?.Clear();
                floppyDisksTabPanel?.Clear();
                if (extraInformationTabControl != null)
                {
                    extraInformationTabControl.Enabled = false; // Ensure tabs are disabled if selection is cleared
                }
            }
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
    }

    private void ExitMenuItem_Click(object? sender, EventArgs e)
    {
        Application.Exit();
    }

    private void PopulateSoundtrackTab(GameConfiguration game)
    {
        if (soundtrackDataGridView == null || soundtrackCoverPictureBox == null) return;

        // Clear previous state
        soundtrackDataGridView.Rows.Clear();
        soundtrackCoverPictureBox.Image?.Dispose();
        soundtrackCoverPictureBox.Image = null;

        // Load cover art if it exists
        if (!string.IsNullOrEmpty(game.SoundtrackCoverPath) && File.Exists(game.SoundtrackCoverPath))
        {
            try
            {
                soundtrackCoverPictureBox.Image = Image.FromFile(game.SoundtrackCoverPath);
            }
            catch (Exception ex)
            {
                AppLogger.Log($"Error loading soundtrack cover image '{game.SoundtrackCoverPath}': {ex.Message}");
            }
        }

        // Populate the grid with soundtrack files
        foreach (var track in game.SoundtrackFiles)
        {
            // 🎵 for music
            string typeDisplay = FormatTools.SegoeUiSymbolExists ? "\U0001F3B5" : "Music";
            string linkSymbol = FormatTools.SegoeUiSymbolExists ? "\U0001F517" : "Open";

            var rowIndex = soundtrackDataGridView.Rows.Add(typeDisplay, track.DisplayName, linkSymbol);
            var row = soundtrackDataGridView.Rows[rowIndex];
            row.Tag = track; // Store the MediaFileInfo object
            row.Cells[2].ToolTipText = "Open";
        }
    }

    private void SoundtrackDataGridView_CellClick(object? sender, DataGridViewCellEventArgs e)
    {
        // We only care about clicks on the "Link" column (index 2). Ignore headers too.
        if (e.RowIndex < 0 || e.ColumnIndex != 2) return;

        if (soundtrackDataGridView?.Rows[e.RowIndex].Tag is MediaFileInfo trackInfo)
        {
            if (!string.IsNullOrEmpty(trackInfo.FilePath) && File.Exists(trackInfo.FilePath))
            {
                try
                {
                    ProcessStartInfo psi = new()
                    {
                        FileName = trackInfo.FilePath,
                        UseShellExecute = true // Use the default OS application
                    };
                    Process.Start(psi);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(this, $"Could not open media file '{trackInfo.FilePath}'.\nError: {ex.Message}", "Error Opening File", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
    }

    private void SoundtrackDataGridView_CellMouseEnter(object? sender, DataGridViewCellEventArgs e)
    {
        if (soundtrackDataGridView == null) return;

        // Change cursor to hand only when over the "Link" column (index 2) in a valid row.
        if (e.RowIndex >= 0 && e.ColumnIndex == 2)
        {
            soundtrackDataGridView.Cursor = Cursors.Hand;
        }
        else
        {
            soundtrackDataGridView.Cursor = Cursors.Default;
        }
    }

    private void SoundtrackDataGridView_CellMouseLeave(object? sender, DataGridViewCellEventArgs e)
    {
        if (soundtrackDataGridView != null)
        {
            soundtrackDataGridView.Cursor = Cursors.Default;
        }
    }
    
    private void GameNameTextBox_KeyPress(object? sender, KeyPressEventArgs e)
    {
        // Allow control characters like backspace
        if (char.IsControl(e.KeyChar))
        {
            return;
        }
        // Block non-ASCII characters
        if (e.KeyChar > 127)
        {
            e.Handled = true;
        }
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

    private void CancelGameDataButton_Click(object? sender, EventArgs e)
    {
        CancelGameDataEdit();
    }

    private void EditGameDataButton_Click(object? sender, EventArgs e)
    {
        // If synopsis or notes are being edited, cancel that edit first.
        if (synopsisTabPanel?.IsEditing == true)
        {
            synopsisTabPanel.CancelEditMode();
        }
        if (notesTabPanel?.IsEditing == true)
        {
            notesTabPanel.CancelEditMode();
        }

        if (gameNameTextBox == null || releaseYearTextBox == null || parentalRatingComboBox == null ||
            developerTextBox == null || publisherTextBox == null || runCommandsTextBox == null || cancelGameDataButton == null ||
            editGameDataButton == null || saveGameDataButton == null) return;


        gameNameTextBox.ReadOnly = false;
        runCommandsTextBox.ReadOnly = false;
        releaseYearTextBox.ReadOnly = false;
        parentalRatingComboBox.Enabled = true;
        developerTextBox.ReadOnly = false;
        publisherTextBox.ReadOnly = false;

        editGameDataButton.Visible = false;
        saveGameDataButton.Visible = true;
        cancelGameDataButton.Visible = true;

        gameNameTextBox.Focus();
        gameNameTextBox.SelectAll();
    }

    private void GameNameTextBox_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Enter)
        {
            e.SuppressKeyPress = true; // Stop the 'ding' sound
        }
    }

    private async void SaveGameDataButton_Click(object? sender, EventArgs e)
    {
        if (gameListBox?.SelectedItem is not GameConfiguration selectedGame ||
            gameNameTextBox == null ||
            releaseYearTextBox == null ||
            parentalRatingComboBox == null ||
            developerTextBox == null ||
            publisherTextBox == null ||
            runCommandsTextBox == null ||
            editGameDataButton == null || cancelGameDataButton == null ||
            saveGameDataButton == null)
        {
            return;
        }

        string newName = gameNameTextBox.Text.Trim();
        var newCommands = runCommandsTextBox.Lines.ToList();
        string yearText = releaseYearTextBox.Text.Trim();
        string? newRating = parentalRatingComboBox.SelectedItem?.ToString();
        string newDeveloper = developerTextBox.Text.Trim();
        string newPublisher = publisherTextBox.Text.Trim();

        // --- VALIDATION ---
        if (string.IsNullOrWhiteSpace(newName))
        {
            MessageBox.Show(this, "Game Name cannot be empty.", "Invalid Input", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }
        if (newName.Any(c => c > 127))
        {
            MessageBox.Show(this, "Game Name can only contain ASCII characters.", "Invalid Input", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }
        if (newDeveloper.Any(c => c > 127))
        {
            MessageBox.Show(this, "Developer can only contain ASCII characters.", "Invalid Input", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }
        if (newPublisher.Any(c => c > 127))
        {
            MessageBox.Show(this, "Publisher can only contain ASCII characters.", "Invalid Input", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

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
        var originalDeveloper = selectedGame.Developer;
        var originalPublisher = selectedGame.Publisher;

        // Revert UI state regardless of change
        gameNameTextBox.ReadOnly = true;
        runCommandsTextBox.ReadOnly = true;
        releaseYearTextBox.ReadOnly = true;
        parentalRatingComboBox.Enabled = false;
        developerTextBox.ReadOnly = true;
        publisherTextBox.ReadOnly = true;
        editGameDataButton.Visible = true;
        saveGameDataButton.Visible = false;
        cancelGameDataButton.Visible = false;

        // Check if anything actually changed
        bool nameChanged = !string.IsNullOrWhiteSpace(newName) && !newName.Equals(originalName, StringComparison.Ordinal);
        bool commandsChanged = !newCommands.SequenceEqual(originalCommands);
        bool yearChanged = selectedGame.ReleaseYear != newYear;
        bool ratingChanged = originalRating != newRating;
        bool developerChanged = originalDeveloper != newDeveloper;
        bool publisherChanged = originalPublisher != newPublisher;

        if (!nameChanged && !commandsChanged && !yearChanged && !ratingChanged && !developerChanged && !publisherChanged)
        {
            // No changes, just revert UI and return
            gameNameTextBox.Text = originalName;
            runCommandsTextBox.Text = string.Join(Environment.NewLine, originalCommands);
            releaseYearTextBox.Text = selectedGame.ReleaseYear?.ToString() ?? string.Empty;
            parentalRatingComboBox.SelectedItem = originalRating ?? "";
            developerTextBox.Text = originalDeveloper ?? "";
            publisherTextBox.Text = originalPublisher ?? "";
            return;
        }

        try
        {
            await GameDataWriterService.UpdateGameDataAsync(selectedGame.ConfigFilePath, newName, newYear, newRating, newDeveloper, newPublisher, newCommands);

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
            if (developerChanged)
            {
                selectedGame.Developer = newDeveloper;
            }
            if (publisherChanged)
            {
                selectedGame.Publisher = newPublisher;
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
            developerTextBox.Text = originalDeveloper ?? "";
            publisherTextBox.Text = originalPublisher ?? "";
        }
    }

    private void TopForm_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Escape)
        {
            // Check if in game data edit mode first, as it's the "highest" level edit.
            if (saveGameDataButton?.Visible == true)
            {
                CancelGameDataEdit();
                e.SuppressKeyPress = true;
                return; // Exit after handling
            }

            // If not in game data edit, check the text panels.
            // The HandleKeyDown method will suppress the key if it's in edit mode.
            synopsisTabPanel?.HandleKeyDown(e);
            notesTabPanel?.HandleKeyDown(e);
        }
    }

    private void HandleEditModeStarted(object? sender, EventArgs e)
    {
        if (saveGameDataButton?.Visible == true)
        {
            CancelGameDataEdit();
        }
    }

    /// <summary>
    /// Cancels the game data edit mode, reverting all changes and UI state.
    /// </summary>
    private void CancelGameDataEdit()
    {
        if (gameListBox?.SelectedItem is not GameConfiguration selectedGame ||
            gameNameTextBox == null || releaseYearTextBox == null || parentalRatingComboBox == null ||
            cancelGameDataButton == null ||
            developerTextBox == null || publisherTextBox == null || runCommandsTextBox == null ||
            editGameDataButton == null || saveGameDataButton == null)
        {
            return;
        }

        // Revert UI state to read-only
        gameNameTextBox.ReadOnly = true;
        runCommandsTextBox.ReadOnly = true;
        releaseYearTextBox.ReadOnly = true;
        parentalRatingComboBox.Enabled = false;
        developerTextBox.ReadOnly = true;
        publisherTextBox.ReadOnly = true;
        editGameDataButton.Visible = true;
        saveGameDataButton.Visible = false;
        cancelGameDataButton.Visible = false;

        // Revert UI fields to their original values from the in-memory model
        gameNameTextBox.Text = selectedGame.GameName;
        releaseYearTextBox.Text = selectedGame.ReleaseYear?.ToString() ?? string.Empty;
        parentalRatingComboBox.SelectedItem = selectedGame.ParentalRating ?? "";
        developerTextBox.Text = selectedGame.Developer ?? string.Empty;
        publisherTextBox.Text = selectedGame.Publisher ?? string.Empty;
        runCommandsTextBox.Text = string.Join(Environment.NewLine, selectedGame.DosboxCommands);
    }
}