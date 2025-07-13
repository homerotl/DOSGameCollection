using System.Diagnostics;
using System.Reflection;
using DOSGameCollection.Models;
using DOSGameCollection.UI;
using DOSGameCollection.Services;

namespace DOSGameCollection;

public class TopForm : Form
{
    private ListBox? gameListBox;
    private Button? playGameButton;
    private Button? dosPromptButton;
    private Button? manualButton;
    private Button? dosboxConfigButton;
    private Button? gameConfigButton;
    private Button? refreshButton;
    private Button? openGameFolderButton;
    private Button? editGameDataButton;
    private Button? saveGameDataButton;
    private Button? newGameButton;
    private Button? deleteGameButton;

    private Button? cancelGameDataButton;
    private TableLayoutPanel? rightColumnPanel;
    private TextBox? gameNameTextBox;
    private TextBox? releaseYearTextBox;
    private ComboBox? parentalRatingComboBox;
    private TextBox? developerTextBox;
    private TextBox? publisherTextBox;
    private MenuStrip? menuStrip;
    private TabControl? extraInformationTabControl;
    private MediaTabPanel? mediaTabPanel;
    private MediaTabPanel? insertsTabPanel;
    private MediaTabPanel? soundtrackTabPanel;
    private DiscImageTabPanel? isoImagesTabPanel;
    private DiscImageTabPanel? floppyDisksTabPanel;
    private TextBox? runCommandsTextBox;
    private TextBox? setupCommandsTextBox;
    private TextEditorTabPanel? synopsisTabPanel;
    private TextEditorTabPanel? notesTabPanel;
    private TextEditorTabPanel? cheatsTabPanel;
    private TextEditorTabPanel? walkthroughTabPanel;
    private List<GameConfiguration> loadedGameConfigs = [];
    private readonly AppConfigService appConfigService;
    public TopForm()
    {
        InitializeComponent();
        appConfigService = new AppConfigService();
        Load += TopForm_Load;
        KeyPreview = true; // Allows the form to preview key events before the focused control.
        KeyDown += TopForm_KeyDown;
    }

    private void InitializeComponent()
    {
        Text = "DOSGameCollection";
        Name = "TopForm";
        ClientSize = new System.Drawing.Size(1200, 675);
        MinimumSize = new System.Drawing.Size(800, 600);

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

        TableLayoutPanel mainLayoutPanel = new()
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2
        };

        mainLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 250F));
        mainLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));

        // Collection buttom initialization
        refreshButton = new Button
        { Anchor = AnchorStyles.Left, Size = new Size(35, 35), Margin = new Padding(5), };
        refreshButton.Click += RefreshButton_Click;

        newGameButton = new Button { Anchor = AnchorStyles.Left, Size = new Size(35, 35), Margin = new Padding(5) };
        newGameButton.Click += NewGameButton_Click;

        deleteGameButton = new Button { Anchor = AnchorStyles.Left, Size = new Size(35, 35), Margin = new Padding(5), Enabled = false };
        deleteGameButton.Click += DeleteGameButton_Click;

        // Game action button initialization
        playGameButton = new Button
        { Anchor = AnchorStyles.Left, Size = new Size(35, 35), Margin = new Padding(5), Enabled = false };
        playGameButton.Click += RunButton_Click;

        dosPromptButton = new Button
        { Anchor = AnchorStyles.Left, Size = new Size(35, 35), Margin = new Padding(5), Enabled = false };
        dosPromptButton.Click += DosPromptButton_Click;

        manualButton = new Button
        { Anchor = AnchorStyles.Left, Size = new Size(35, 35), Margin = new Padding(5), Enabled = false };
        manualButton.Click += ManualButton_Click;

        dosboxConfigButton = new Button
        { Anchor = AnchorStyles.Left, Size = new Size(35, 35), Margin = new Padding(5), Enabled = false };
        dosboxConfigButton.Click += DosboxConfigButton_Click;

        openGameFolderButton = new Button
        { Anchor = AnchorStyles.Left, Size = new Size(35, 35), Margin = new Padding(5), Enabled = false };
        openGameFolderButton.Click += OpenGameFolderButton_Click;

        gameConfigButton = new Button
        { Anchor = AnchorStyles.Left, Size = new Size(35, 35), Margin = new Padding(5), Enabled = false };
        gameConfigButton.Click += GameConfigButton_Click;

        // Edit action button initialization
        editGameDataButton = new Button
        { Anchor = AnchorStyles.Left, Size = new Size(35, 35), Margin = new Padding(5), Enabled = false };
        editGameDataButton.Click += EditGameDataButton_Click;

        cancelGameDataButton = new Button
        { Anchor = AnchorStyles.Left, Size = new Size(35, 35), Margin = new Padding(5), Visible = false };
        cancelGameDataButton.Click += CancelGameDataButton_Click;

        saveGameDataButton = new Button
        { Anchor = AnchorStyles.Left, Size = new Size(35, 35), Margin = new Padding(5), Visible = false };
        saveGameDataButton.Click += SaveGameDataButton_Click;

        // Load embedded resources
        Assembly assembly = Assembly.GetExecutingAssembly();

        // Collection button actions
        refreshButton.Image = FormatTools.LoadImageFromResource("DOSGameCollection.Resources.icons.refresh.png");
        newGameButton.Image = FormatTools.LoadImageFromResource("DOSGameCollection.Resources.icons.new.png");
        deleteGameButton.Image = FormatTools.LoadImageFromResource("DOSGameCollection.Resources.icons.delete.png");

        // Game actions
        playGameButton.Image = FormatTools.LoadImageFromResource("DOSGameCollection.Resources.icons.play.png");
        dosPromptButton.Image = FormatTools.LoadImageFromResource("DOSGameCollection.Resources.icons.prompt.png");
        manualButton.Image = FormatTools.LoadImageFromResource("DOSGameCollection.Resources.icons.manual.png");
        dosboxConfigButton.Image = FormatTools.LoadImageFromResource("DOSGameCollection.Resources.icons.dosbox-stg.png");
        openGameFolderButton.Image = FormatTools.LoadImageFromResource("DOSGameCollection.Resources.icons.folder.png");
        gameConfigButton.Image = FormatTools.LoadImageFromResource("DOSGameCollection.Resources.icons.settings.png");

        // Edit actions
        editGameDataButton.Image = FormatTools.LoadImageFromResource("DOSGameCollection.Resources.icons.edit.png");
        cancelGameDataButton.Image = FormatTools.LoadImageFromResource("DOSGameCollection.Resources.icons.cancel.png");
        saveGameDataButton.Image = FormatTools.LoadImageFromResource("DOSGameCollection.Resources.icons.ok.png");

        // App icon
        Icon = FormatTools.LoadIconFromResource("DOSGameCollection.appicon.ico");

        gameListBox = new ListBox();

        rightColumnPanel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 3,
            Margin = new Padding(0)
        };
        rightColumnPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));

        // --- Action Buttons Panel 
        TableLayoutPanel actionButtonsPanel = new()
        {
            ColumnCount = 2,
            RowCount = 1,
            Dock = DockStyle.Top,
            AutoSize = true,
            Margin = new Padding(0)
            //BackColor = Color.Red // For debugging layout
        };
        actionButtonsPanel.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));      // Left-aligned buttons
        actionButtonsPanel.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));      // Right-aligned buttons

        FlowLayoutPanel leftActionButtons = new()
        {
            AutoSize = true,
            Dock = DockStyle.Left,
            FlowDirection = FlowDirection.LeftToRight,
            Margin = new Padding(0)
            // BackColor = Color.Blue for debugging layout
        };
        leftActionButtons.Controls.Add(playGameButton);
        leftActionButtons.Controls.Add(dosPromptButton);
        leftActionButtons.Controls.Add(gameConfigButton);
        leftActionButtons.Controls.Add(manualButton);
        leftActionButtons.Controls.Add(dosboxConfigButton);
        leftActionButtons.Controls.Add(openGameFolderButton);

        FlowLayoutPanel rightActionButtons = new()
        {
            AutoSize = true,
            Dock = DockStyle.Right,
            Margin = new Padding(0)
            // BackColor = Color.Green for debugging layout
        };
        rightActionButtons.Controls.Add(cancelGameDataButton);
        rightActionButtons.Controls.Add(saveGameDataButton);
        rightActionButtons.Controls.Add(editGameDataButton);

        actionButtonsPanel.Controls.Add(leftActionButtons, 0, 0);
        actionButtonsPanel.Controls.Add(rightActionButtons, 1, 0);

        // --- ToolTips for Action Buttons ---
        ToolTip actionButtonToolTip = new();
        actionButtonToolTip.SetToolTip(refreshButton, "Refresh Game List");
        actionButtonToolTip.SetToolTip(newGameButton, "New Game");
        actionButtonToolTip.SetToolTip(deleteGameButton, "Delete Selected Game");

        actionButtonToolTip.SetToolTip(playGameButton, "Play Game");
        actionButtonToolTip.SetToolTip(dosPromptButton, "DOS Prompt");
        actionButtonToolTip.SetToolTip(gameConfigButton, "Run Game Setup");
        actionButtonToolTip.SetToolTip(manualButton, "Game Manual");
        actionButtonToolTip.SetToolTip(dosboxConfigButton, "Open DOSBox Config");
        actionButtonToolTip.SetToolTip(openGameFolderButton, "Open game folder");

        actionButtonToolTip.SetToolTip(saveGameDataButton, "Save");
        actionButtonToolTip.SetToolTip(cancelGameDataButton, "Cancel");
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

        Label gameNameLabel = new Label
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
            Font = new Font("Consolas", 8F, FontStyle.Regular),
            Height = 60 // Set a fixed height for roughly 4 lines
        };

        runCommandsPanel.Controls.Add(runCommandsLabel, 0, 0);
        runCommandsPanel.Controls.Add(runCommandsTextBox, 0, 1);

        // --- Setup Commands Layout Table ---
        TableLayoutPanel setupCommandsPanel = new()
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            ColumnCount = 1,
            RowCount = 2,
            Margin = new Padding(3, 3, 0, 0) // Left and Top margin
        };
        setupCommandsPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // For Label
        setupCommandsPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // For TextBox

        Label setupCommandsLabel = new()
        {
            Text = "Setup Commands",
            Anchor = AnchorStyles.Left,
            TextAlign = ContentAlignment.MiddleLeft,
            AutoSize = true,
            Margin = new Padding(0, 0, 0, 3) // Bottom margin
        };

        setupCommandsTextBox = new TextBox
        {
            Dock = DockStyle.Fill,
            Multiline = true,
            ReadOnly = true,
            ScrollBars = ScrollBars.Vertical,
            Font = new Font("Consolas", 8F, FontStyle.Regular),
            Height = 60 // Set a fixed height for roughly 4 lines
        };

        setupCommandsPanel.Controls.Add(setupCommandsLabel, 0, 0);
        setupCommandsPanel.Controls.Add(setupCommandsTextBox, 0, 1);

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

        // --- Container for both command panels ---
        TableLayoutPanel allCommandsPanel = new()
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            ColumnCount = 1,
            RowCount = 2
        };
        allCommandsPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
        allCommandsPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));

        allCommandsPanel.Controls.Add(runCommandsPanel, 0, 0);
        allCommandsPanel.Controls.Add(setupCommandsPanel, 0, 1);

        gameConfigPanel.Controls.Add(gamePropsTablePanel, 0, 0);
        gameConfigPanel.Controls.Add(allCommandsPanel, 1, 0);

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
        mediaTabPanel.DisplayNameUpdated += MediaDisplayNameUpdated;
        mediaTab.Controls.Add(mediaTabPanel);

        TabPage insertsTab = new("Inserts");
        insertsTabPanel = new MediaTabPanel { Dock = DockStyle.Fill };
        insertsTabPanel.DisplayNameUpdated += MediaDisplayNameUpdated;
        insertsTab.Controls.Add(insertsTabPanel);

        TabPage isoImagesTab = new("CD-ROM");
        isoImagesTabPanel = new DiscImageTabPanel { Dock = DockStyle.Fill };
        isoImagesTabPanel.DisplayNameUpdated += MediaDisplayNameUpdated;
        isoImagesTabPanel.DiscImageUpdated += DiscImageUpdated;
        isoImagesTab.Controls.Add(isoImagesTabPanel);

        TabPage soundtrackTab = new("Soundtrack");

        soundtrackTabPanel = new MediaTabPanel { Dock = DockStyle.Fill };
        soundtrackTabPanel.DisplayNameUpdated += MediaDisplayNameUpdated;
        soundtrackTab.Controls.Add(soundtrackTabPanel);

        TabPage floppyDisksTab = new("Floppy disks");
        floppyDisksTabPanel = new DiscImageTabPanel { Dock = DockStyle.Fill };
        floppyDisksTabPanel.DisplayNameUpdated += MediaDisplayNameUpdated;
        floppyDisksTabPanel.DiscImageUpdated += DiscImageUpdated;
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

        cheatsTabPanel = new TextEditorTabPanel
        {
            Dock = DockStyle.Fill
        };
        cheatsTabPanel.EditModeStarted += HandleEditModeStarted;
        cheatsTab.Controls.Add(cheatsTabPanel);

        walkthroughTabPanel = new TextEditorTabPanel
        {
            Dock = DockStyle.Fill
        };
        walkthroughTabPanel.EditModeStarted += HandleEditModeStarted;
        walkthroughTab.Controls.Add(walkthroughTabPanel);


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
            Margin = new Padding(0), // No margin for the panel itself
            //BackColor = Color.Red // For debugging layout
        };
        leftColumnPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // Row for Refresh button
        leftColumnPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100F)); // Row for GameListBox

        // Add Refresh Button to the leftColumnPanel
        // --- Top-left buttons panel (Refresh, New) ---
        FlowLayoutPanel topLeftButtonsPanel = new()
        {
            AutoSize = true,
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.LeftToRight,
            Margin = new Padding(0),
            Padding = new Padding(0)
        };
        topLeftButtonsPanel.Controls.Add(refreshButton);
        topLeftButtonsPanel.Controls.Add(newGameButton);
        topLeftButtonsPanel.Controls.Add(deleteGameButton);

        leftColumnPanel.Controls.Add(topLeftButtonsPanel, 0, 0);

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

    private void DosPromptButton_Click(object? sender, EventArgs e)
    {
        if (gameListBox?.SelectedItem is GameConfiguration selectedGame)
        {
            GameLauncherService.LaunchDosPrompt(selectedGame, appConfigService.DosboxExePath, this);
        }
    }

     private void NewGameButton_Click(object? sender, EventArgs e)
    {
        var existingNames = loadedGameConfigs.Select(c => c.GameName);
        var libraryPath = appConfigService.LibraryPath ?? "";

        using NewGameWizardDialog newGameDialog = new(existingNames, libraryPath);
        if (newGameDialog.ShowDialog(this) == DialogResult.OK)
        {
            if (newGameDialog.NewGameConfiguration != null && gameListBox != null)
            {
                var newGame = newGameDialog.NewGameConfiguration;
                // Add the new game to our in-memory list and the UI listbox.
                loadedGameConfigs.Add(newGame);
                gameListBox.Items.Add(newGame);
                gameListBox.SelectedItem = newGame; // Select the new game to show its details.

                // If the wizard was for a diskette install, launch the installer now.
                if (newGameDialog.CopiedDisketteImagePaths.Any())
                {
                    MessageBox.Show(this, "Game entry created. Now launching DOSBox for installation. When finished, type 'exit' in DOSBox.", "Installation Step 2", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    var dosboxExePath = appConfigService.DosboxExePath;
                    var dosboxConfPath = Path.Combine(newGame.GameDirectoryPath, "dosbox-staging.conf");
                    var mountCPath = Path.Combine(newGame.GameDirectoryPath, "game-files");

                    GameLauncherService.LaunchDosboxForDisketteInstallation(
                        dosboxExePath,
                        dosboxConfPath,
                        mountCPath,
                        newGameDialog.CopiedDisketteImagePaths,
                        this
                    );
                }
                else if (newGameDialog.CopiedCdRomImagePaths.Any())
                {
                    MessageBox.Show(this, "Game entry created. Now launching DOSBox for installation. When finished, type 'exit' in DOSBox.", "Installation Step 2", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    var dosboxExePath = appConfigService.DosboxExePath;
                    var dosboxConfPath = Path.Combine(newGame.GameDirectoryPath, "dosbox-staging.conf");
                    var mountCPath = Path.Combine(newGame.GameDirectoryPath, "game-files");

                    GameLauncherService.LaunchDosboxForCdRomInstallation(
                        dosboxExePath,
                        dosboxConfPath,
                        mountCPath,
                        newGameDialog.CopiedCdRomImagePaths,
                        this
                    );
                }
            }
        }
    }

private async void DeleteGameButton_Click(object? sender, EventArgs e)
    {
        if (gameListBox?.SelectedItem is not GameConfiguration selectedGame)
        {
            return;
        }

        // Disable the button immediately to prevent double-clicks while the confirmation is open.
        if (deleteGameButton != null) deleteGameButton.Enabled = false;

        var confirmResult = MessageBox.Show(this,
            "You are about to delete the folder for this game and all the files within it. Are you sure?",
            "Confirm Deletion",
            MessageBoxButtons.OKCancel,
            MessageBoxIcon.Warning);

        if (confirmResult != DialogResult.OK)
        {
            // Re-enable the button if the user cancels the operation.
            if (deleteGameButton != null)
            {
                deleteGameButton.Enabled = true;
            }
            return;
        }

        using LoadGameListProgressDialog progressDialog = new();
        progressDialog.Text = "Deleting Game"; // Customize dialog title
        var progress = new Progress<ProgressReport>(report =>
        {
            progressDialog.HandleProgressReport(report);
            if (report.IsComplete && progressDialog.Visible)
            {
                progressDialog.Close();
            }
        });

        var deleteService = new GameDeleteService();
        bool deleteSucceeded = false;
        try
        {
            // The delete button is already disabled. Just disable refresh.
            if (refreshButton != null) refreshButton.Enabled = false;

            Task deleteTask = deleteService.DeleteGameAsync(selectedGame.GameDirectoryPath, progress);
            progressDialog.ShowDialog(this);
            await deleteTask;
            deleteSucceeded = true;
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, $"Error deleting game: {ex.Message}", "Deletion Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            if (refreshButton != null) refreshButton.Enabled = true;
            // If deletion failed, the selected item is still present, so re-enable the delete button.
            // If it succeeded, the list selection will change, and the button state will be updated by the event handler.
            if (!deleteSucceeded && deleteGameButton != null)
            {
                deleteGameButton.Enabled = true;
            }
        }

        if (deleteSucceeded)
        {
            loadedGameConfigs.Remove(selectedGame);
            gameListBox.Items.Remove(selectedGame);
        }
    }

    private async void TopForm_Load(object? sender, EventArgs e)
    {
        await appConfigService.LoadOrCreateConfigurationAsync(this);

        if (string.IsNullOrEmpty(appConfigService.LibraryPath) || !Directory.Exists(appConfigService.LibraryPath))
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
        cheatsTabPanel?.Clear();
        walkthroughTabPanel?.Clear();
        isoImagesTabPanel?.Clear();
        floppyDisksTabPanel?.Clear();
        soundtrackTabPanel?.Clear();
        if (playGameButton != null)
        {
            playGameButton.Enabled = false;
        }
        if (dosPromptButton != null)
        {
            dosPromptButton.Enabled = false;
        }
        if (manualButton != null)
        {
            manualButton.Enabled = false;
        }
        if (dosboxConfigButton != null)
        {
            dosboxConfigButton.Enabled = false;
        }
        if (openGameFolderButton != null)
        {
            openGameFolderButton.Enabled = false;
        }
        if (gameConfigButton != null)
        {
            gameConfigButton.Enabled = false;
        }
        if (editGameDataButton != null)
        {
            editGameDataButton.Enabled = false;
            editGameDataButton.Visible = true;
        }
        if (deleteGameButton != null)
        {
            deleteGameButton.Enabled = false;
        }
        if (saveGameDataButton != null) saveGameDataButton.Visible = false;
        if (cancelGameDataButton != null) cancelGameDataButton.Visible = false;
        if (runCommandsTextBox != null)
        {
            runCommandsTextBox.Clear();
            runCommandsTextBox.ReadOnly = true;
        }
        if (setupCommandsTextBox != null)
        {
            setupCommandsTextBox.Clear();
            setupCommandsTextBox.ReadOnly = true;
        }
        if (extraInformationTabControl != null)
        {
            extraInformationTabControl.Enabled = false;
        }
        loadedGameConfigs.Clear();

        if (string.IsNullOrEmpty(appConfigService.LibraryPath) || !Directory.Exists(appConfigService.LibraryPath))
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

            Task<List<GameConfiguration>> loadingTask = loadGamesDataService.LoadDataAsync(appConfigService.LibraryPath, progress);
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

        loadedGameConfigs = gameConfigs;
        PopulateListBox(loadedGameConfigs);
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
        if (playGameButton != null) playGameButton.Enabled = false;
        if (gameConfigButton != null) gameConfigButton.Enabled = false;
        if (dosPromptButton != null)
        {
            dosPromptButton.Enabled = false;
        }
        if (manualButton != null)
        {
            manualButton.Enabled = false;
        }
        if (dosboxConfigButton != null)
        {
            dosboxConfigButton.Enabled = false;
        }
        if (openGameFolderButton != null)
        {
            openGameFolderButton.Enabled = false;
        }
        if (editGameDataButton != null)
        {
            editGameDataButton.Enabled = (gameListBox?.SelectedItem != null);
            editGameDataButton.Visible = true;
        }
        if (deleteGameButton != null)
        {
            deleteGameButton.Enabled = (gameListBox?.SelectedItem != null);
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
        if (setupCommandsTextBox != null)
        {
            setupCommandsTextBox.Clear();
            setupCommandsTextBox.ReadOnly = true;
        }
        synopsisTabPanel?.Clear();
        notesTabPanel?.Clear();
        cheatsTabPanel?.Clear();
        walkthroughTabPanel?.Clear();
        soundtrackTabPanel?.Clear();
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

                // Read display names from file-info.txt in the game's root for box art
                var gameDirDisplayNames = await GameDataReaderService.ParseDisplayNamesAsync(selectedGame.GameDirectoryPath);

                // Populate Media Tab
                var mediaItems = new List<MediaTabPanel.MediaItem>();
                if (selectedGame.HasFrontBoxArt)
                {
                    var displayName = gameDirDisplayNames.GetValueOrDefault("front.png", "Front Box Art");
                    mediaItems.Add(new(selectedGame.FrontBoxArtPath, displayName, MediaTabPanel.MediaType.Image));
                }
                if (selectedGame.HasBackBoxArt)
                {
                    var displayName = gameDirDisplayNames.GetValueOrDefault("back.png", "Back Box Art");
                    mediaItems.Add(new(selectedGame.BackBoxArtPath, displayName, MediaTabPanel.MediaType.Image));
                }
                mediaItems.AddRange(selectedGame.CaptureFiles.Select(f => new MediaTabPanel.MediaItem(f.FilePath, f.DisplayName, MediaTabPanel.MediaType.Image)));
                mediaItems.AddRange(selectedGame.VideoFiles.Select(f => new MediaTabPanel.MediaItem(f.FilePath, f.DisplayName, MediaTabPanel.MediaType.Video)));
                mediaTabPanel?.Populate(mediaItems);

                // Populate Inserts Tab
                var insertItems = selectedGame.InsertFiles.Select(f => new MediaTabPanel.MediaItem(f.FilePath, f.DisplayName,
                    Path.GetExtension(f.FilePath).Equals(".pdf", StringComparison.OrdinalIgnoreCase) ? MediaTabPanel.MediaType.Pdf : MediaTabPanel.MediaType.Image
                ));
                insertsTabPanel?.Populate(insertItems);

                // Populate Soundtrack Tab
                var soundtrackItems = selectedGame.SoundtrackFiles.Select(f => new MediaTabPanel.MediaItem(f.FilePath, f.DisplayName, MediaTabPanel.MediaType.Audio));
                soundtrackTabPanel?.Populate(soundtrackItems, selectedGame.SoundtrackCoverPath);

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
                if (cheatsTabPanel != null)
                {
                    // The FilePath will either be the one found by the scanner, or a path to a new file.
                    // The component handles creating it on first save.
                    cheatsTabPanel.FilePath = selectedGame.CheatsFilePath ?? Path.Combine(selectedGame.GameDirectoryPath, "cheats-and-secrets.txt");
                }
                if (walkthroughTabPanel != null)
                {
                    // The FilePath will either be the one found by the scanner, or a path to a new file.
                    // The component handles creating it on first save.
                    walkthroughTabPanel.FilePath = selectedGame.WalkthroughFilePath ?? Path.Combine(selectedGame.GameDirectoryPath, "walkthrough.txt");
                }
                if (manualButton != null && !string.IsNullOrEmpty(selectedGame.ManualPath) && File.Exists(selectedGame.ManualPath))
                {
                    manualButton.Enabled = true;
                }
                if (dosboxConfigButton != null)
                {
                    string configPath = Path.Combine(selectedGame.GameDirectoryPath, "dosbox-staging.conf");
                    dosboxConfigButton.Enabled = File.Exists(configPath);
                }
                if (openGameFolderButton != null)
                {
                    openGameFolderButton.Enabled = Directory.Exists(selectedGame.GameDirectoryPath);
                }
                if (playGameButton != null)
                {
                    playGameButton.Enabled = selectedGame.DosboxCommands.Any();
                }
                if (dosPromptButton != null)
                {
                    dosPromptButton.Enabled = true;
                }
                if (gameConfigButton != null)
                {
                    gameConfigButton.Enabled = selectedGame.SetupCommands.Any();
                }
                if (runCommandsTextBox != null)
                {
                    runCommandsTextBox.Text = string.Join(Environment.NewLine, selectedGame.DosboxCommands);
                }
                if (setupCommandsTextBox != null)
                {
                    setupCommandsTextBox.Text = string.Join(Environment.NewLine, selectedGame.SetupCommands);
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
                cheatsTabPanel?.Clear();
                walkthroughTabPanel?.Clear();
                isoImagesTabPanel?.Clear();
                floppyDisksTabPanel?.Clear();
                soundtrackTabPanel?.Clear();
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
            GameLauncherService.LaunchGame(selectedGame, appConfigService.DosboxExePath, selectedGame.DosboxCommands, this);
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

    private void DosboxConfigButton_Click(object? sender, EventArgs e)
    {
        if (gameListBox?.SelectedItem is GameConfiguration selectedGame)
        {
            string configPath = Path.Combine(selectedGame.GameDirectoryPath, "dosbox-staging.conf");

            if (File.Exists(configPath))
            {
                try
                {
                    ProcessStartInfo psi = new(configPath)
                    {
                        UseShellExecute = true // Use the default OS application for .conf files
                    };
                    Process.Start(psi);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(this, $"Could not open the DOSBox configuration file.\nError: {ex.Message}", "Error Opening File", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                MessageBox.Show(this, "The DOSBox configuration file (dosbox-staging.conf) was not found for this game.", "File Not Found", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
    }

    private void OpenGameFolderButton_Click(object? sender, EventArgs e)
    {
        if (gameListBox?.SelectedItem is GameConfiguration selectedGame)
        {
            string gamePath = selectedGame.GameDirectoryPath;
            if (Directory.Exists(gamePath))
            {
                try
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = gamePath,
                        UseShellExecute = true
                    });
                }
                catch (Exception ex)
                {
                    MessageBox.Show(this, $"Could not open the game folder.\nError: {ex.Message}", "Error Opening Folder", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                MessageBox.Show(this, "The game directory was not found.", "Directory Not Found", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
    }

    private void GameConfigButton_Click(object? sender, EventArgs e)
    {
        if (gameListBox?.SelectedItem is GameConfiguration selectedGame && selectedGame.SetupCommands.Any())
        {
            // Assuming GameLauncherService.LaunchGame is updated to take a list of commands
            GameLauncherService.LaunchGame(selectedGame, appConfigService.DosboxExePath, selectedGame.SetupCommands, this);
        }
    }

    private async void RefreshButton_Click(object? sender, EventArgs e)
    {
        await RefreshGameListAsync();
    }

    private async void SetDosboxLocationMenuItem_Click(object? sender, EventArgs e)
    {
        bool pathUpdated = await appConfigService.ManuallySetDosboxPathAsync(this);
        if (pathUpdated)
        {
            await appConfigService.SaveConfigurationAsync(this);
            MessageBox.Show(this, "DOSBox location has been updated.", "DOSBox Location Saved", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }

    private async void SetGameLibraryLocationMenuItem_Click(object? sender, EventArgs e)
    {
        bool pathUpdated = await appConfigService.ManuallySetLibraryPathAsync(this);
        if (pathUpdated)
        {
            await appConfigService.SaveConfigurationAsync(this);
            MessageBox.Show(this, "Game library location has been updated. Refreshing game list...", "Library Location Saved", MessageBoxButtons.OK, MessageBoxIcon.Information);
            await RefreshGameListAsync();
        }
    }

    private void ExitMenuItem_Click(object? sender, EventArgs e)
    {
        Application.Exit();
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
        if (cheatsTabPanel?.IsEditing == true)
        {
            cheatsTabPanel.CancelEditMode();
        }
        if (walkthroughTabPanel?.IsEditing == true)
        {
            walkthroughTabPanel.CancelEditMode();
        }

        if (gameNameTextBox == null || releaseYearTextBox == null || parentalRatingComboBox == null ||
            developerTextBox == null || publisherTextBox == null || runCommandsTextBox == null || cancelGameDataButton == null ||
            editGameDataButton == null || saveGameDataButton == null || setupCommandsTextBox == null) return;

        gameNameTextBox.ReadOnly = false;
        runCommandsTextBox.ReadOnly = false;
        releaseYearTextBox.ReadOnly = false;
        setupCommandsTextBox.ReadOnly = false;
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
            setupCommandsTextBox == null ||
            editGameDataButton == null || cancelGameDataButton == null ||
            saveGameDataButton == null)
        {
            return;
        }

        string newName = gameNameTextBox.Text.Trim();
        var newCommands = runCommandsTextBox.Lines.ToList();
        var newSetupCommands = setupCommandsTextBox.Lines.ToList();
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
        var originalSetupCommands = selectedGame.SetupCommands;
        var originalRating = selectedGame.ParentalRating;
        var originalDeveloper = selectedGame.Developer;
        var originalPublisher = selectedGame.Publisher;

        // Revert UI state regardless of change
        gameNameTextBox.ReadOnly = true;
        runCommandsTextBox.ReadOnly = true;
        releaseYearTextBox.ReadOnly = true;
        setupCommandsTextBox.ReadOnly = true;
        parentalRatingComboBox.Enabled = false;
        developerTextBox.ReadOnly = true;
        publisherTextBox.ReadOnly = true;
        editGameDataButton.Visible = true;
        saveGameDataButton.Visible = false;
        cancelGameDataButton.Visible = false;

        // Check if anything actually changed
        bool nameChanged = !string.IsNullOrWhiteSpace(newName) && !newName.Equals(originalName, StringComparison.Ordinal);
        bool commandsChanged = !newCommands.SequenceEqual(originalCommands);
        bool setupCommandsChanged = !newSetupCommands.SequenceEqual(originalSetupCommands);
        bool yearChanged = selectedGame.ReleaseYear != newYear;
        bool ratingChanged = originalRating != newRating;
        bool developerChanged = originalDeveloper != newDeveloper;
        bool publisherChanged = originalPublisher != newPublisher;

        if (!nameChanged && !commandsChanged && !yearChanged && !ratingChanged && !developerChanged && !publisherChanged && !setupCommandsChanged)
        {
            // No changes, just revert UI and return
            gameNameTextBox.Text = originalName;
            runCommandsTextBox.Text = string.Join(Environment.NewLine, originalCommands);
            setupCommandsTextBox.Text = string.Join(Environment.NewLine, originalSetupCommands);
            releaseYearTextBox.Text = selectedGame.ReleaseYear?.ToString() ?? string.Empty;
            parentalRatingComboBox.SelectedItem = originalRating ?? "";
            developerTextBox.Text = originalDeveloper ?? "";
            publisherTextBox.Text = originalPublisher ?? "";
            return;
        }

        try
        {
            await GameDataWriterService.UpdateGameDataAsync(selectedGame.ConfigFilePath, newName, newYear, newRating, newDeveloper, newPublisher, newCommands, newSetupCommands);

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
            if (setupCommandsChanged)
            {
                selectedGame.SetupCommands = newSetupCommands.Where(c => !string.IsNullOrWhiteSpace(c)).ToList();
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
            // After all updates, re-evaluate button states
            if (playGameButton != null)
            {
                playGameButton.Enabled = selectedGame.DosboxCommands.Any();
            }
            if (gameConfigButton != null)
            {
                gameConfigButton.Enabled = selectedGame.SetupCommands.Any();
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, $"Failed to save game data: {ex.Message}", "Save Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            // Revert textboxes to original values on error
            gameNameTextBox.Text = originalName; // Revert on error
            releaseYearTextBox.Text = selectedGame.ReleaseYear?.ToString() ?? string.Empty;
            runCommandsTextBox.Text = string.Join(Environment.NewLine, originalCommands);
            setupCommandsTextBox.Text = string.Join(Environment.NewLine, originalSetupCommands);
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
            cheatsTabPanel?.HandleKeyDown(e);
            walkthroughTabPanel?.HandleKeyDown(e);
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
            developerTextBox == null || publisherTextBox == null || runCommandsTextBox == null || setupCommandsTextBox == null ||
            editGameDataButton == null || saveGameDataButton == null)
        {
            return;
        }

        // Revert UI state to read-only
        gameNameTextBox.ReadOnly = true;
        runCommandsTextBox.ReadOnly = true;
        releaseYearTextBox.ReadOnly = true;
        setupCommandsTextBox.ReadOnly = true;
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
        setupCommandsTextBox.Text = string.Join(Environment.NewLine, selectedGame.SetupCommands);

        // Re-evaluate button states based on the original (reverted) data
        if (playGameButton != null)
        {
            playGameButton.Enabled = selectedGame.DosboxCommands.Any();
        }
        if (gameConfigButton != null)
        {
            gameConfigButton.Enabled = selectedGame.SetupCommands.Any();
        }
    }
    
    /// <summary>
    /// Handles the event raised when a media file's display name is updated in a MediaTabPanel.
    /// This method updates the in-memory GameConfiguration model to reflect the change.
    /// </summary>
    /// <param name="filePath">The full path of the file that was updated.</param>
    /// <param name="newDisplayName">The new display name for the file.</param>
    private void MediaDisplayNameUpdated(string filePath, string newDisplayName)
    {
        if (gameListBox?.SelectedItem is not GameConfiguration selectedGame)
        {
            return;
        }

        // Helper function to find and update the item in a list.
        // Since records are immutable, we replace the old record with a new one.
        bool UpdateInList(List<MediaFileInfo> list)
        {
            var index = list.FindIndex(f => f.FilePath.Equals(filePath, StringComparison.OrdinalIgnoreCase));
            if (index != -1)
            {
                list[index] = list[index] with { DisplayName = newDisplayName };
                return true;
            }
            return false;
        }

        bool UpdateDiscInList(List<DiscImageInfo> list)
        {
            var index = list.FindIndex(d => d.FilePath.Equals(filePath, StringComparison.OrdinalIgnoreCase));
            if (index != -1)
            {
                list[index] = list[index] with { DisplayName = newDisplayName };
                return true;
            }
            return false;
        }

        // Attempt to find and update the media item in all relevant lists.
        if (UpdateInList(selectedGame.CaptureFiles) || 
            UpdateInList(selectedGame.VideoFiles) || 
            UpdateInList(selectedGame.InsertFiles) || 
            UpdateInList(selectedGame.SoundtrackFiles) ||
            UpdateDiscInList(selectedGame.IsoImages) ||
            UpdateDiscInList(selectedGame.DiscImages)) { }
    }

    private void DiscImageUpdated(DiscImageInfo updatedDiscInfo)
    {
        if (gameListBox?.SelectedItem is not GameConfiguration selectedGame)
        {
            return;
        }

        // Helper function to find and update the item in a list.
        // Since records are immutable, we replace the old record with a new one.
        bool UpdateInList(List<DiscImageInfo> list)
        {
            // The key is the FilePath.
            var index = list.FindIndex(d => d.FilePath.Equals(updatedDiscInfo.FilePath, StringComparison.OrdinalIgnoreCase));
            if (index != -1)
            {
                list[index] = updatedDiscInfo; // Replace the old record with the new one.
                return true;
            }
            return false;
        }

        // Attempt to find and update the media item in both relevant lists.
        if (UpdateInList(selectedGame.IsoImages) || UpdateInList(selectedGame.DiscImages)) { }
    }
}