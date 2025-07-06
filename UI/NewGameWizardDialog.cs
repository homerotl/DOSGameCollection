using DOSGameCollection.Services;
using DOSGameCollection.Models;

namespace DOSGameCollection.UI;

public class NewGameWizardDialog : Form
{
    // Public properties for results
    public string GameName { get; private set; } = "";
    public string GameDirectory { get; private set; } = "";
    public List<string> DisketteImagePaths { get; private set; } = new();
    public List<string> CopiedDisketteImagePaths { get; private set; } = new();
    public List<string> CdRomImagePaths { get; private set; } = new();
    public List<string> CopiedCdRomImagePaths { get; private set; } = new();
    public string? SourceDirectory { get; private set; }
    public GameConfiguration? NewGameConfiguration { get; private set; }

    // Wizard state
    private Panel? _currentPanel;
    private bool _isInitialized = false;

    // Panels
    private Panel _step1Panel;
    private Panel _step2FilesPanel;
    private Panel _step3FilesPanel;
    private Panel _step2DiskettesPanel;
    private Panel _step3DiskettesPanel;
    private Panel _step2CdRomsPanel;
    private Panel _step3CdRomsPanel;


    // Common controls
    private Button _cancelButton;
    private Button _nextButton;
    private Button _backButton;
    private Label _errorLabel;

    // Step 1 controls
    private TextBox _gameNameTextBox;
    private TextBox _gameDirectoryTextBox;
    private Button _browseDirectoryButton;
    private RadioButton _installFromFilesRadioButton;
    private RadioButton _installFromDiskettesRadioButton;
    private RadioButton _installFromCdRomRadioButton;
    private Label _instructionsLabel;

    // Step 2 controls
    private TextBox _sourceDirectoryTextBox;
    private Button _browseSourceDirectoryButton;

    // Step 2 Diskettes controls
    private ListBox _disketteImagesListBox;
    private Button _addDisketteImagesButton;
    private Button _moveDisketteUpButton;
    private Button _moveDisketteDownButton;
    private Button _deleteDisketteButton;

    // Step 2 CD-ROMs controls
    private ListBox _cdRomImagesListBox;
    private Button _addCdRomImagesButton;
    private Button _moveCdRomUpButton;
    private Button _moveCdRomDownButton;
    private Button _deleteCdRomButton;

    // Step 3 controls
    private TextBox _reviewGameNameTextBox;
    private TextBox _reviewGameDirectoryTextBox;
    private TextBox _reviewSourceDirectoryTextBox;
    private ProgressBar _disketteProgressBar;
    private Label _disketteProgressLabel;
    private Panel _disketteProgressPanel;
    private TextBox _reviewInstructionsTextBox;
    private ProgressBar _progressBar;
    private Label _progressLabel;
    private Panel _progressPanel;

    // Step 3 Diskettes controls
    private TextBox _reviewDisketteGameNameTextBox;
    private TextBox _reviewDisketteGameDirectoryTextBox;
    private ListBox _reviewDisketteImagesListBox;
    private TextBox _reviewDisketteInstructionsTextBox;

    // Step 3 CD-ROMs controls
    private TextBox _reviewCdRomGameNameTextBox;
    private TextBox _reviewCdRomGameDirectoryTextBox;
    private ListBox _reviewCdRomImagesListBox;
    private TextBox _reviewCdRomInstructionsTextBox;
    private ProgressBar _cdRomProgressBar;
    private Label _cdRomProgressLabel;
    private Panel _cdRomProgressPanel;

    private readonly IEnumerable<string> _existingGameNames;
    private readonly string _libraryPath;

    public NewGameWizardDialog(IEnumerable<string> existingGameNames, string libraryPath)
    {
        _existingGameNames = existingGameNames ?? Enumerable.Empty<string>();
        _libraryPath = libraryPath;
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        Text = "New Game Wizard";
        ClientSize = new Size(500, 400);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        StartPosition = FormStartPosition.CenterParent;
        ShowInTaskbar = false;
        MaximizeBox = false;
        MinimizeBox = false;


        // --- Main Layout ---
        var wizardLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(10),
            ColumnCount = 1,
            RowCount = 3
        };
        wizardLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F)); // For the step panel
        wizardLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));      // For the error label
        wizardLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));      // For the buttons


        // --- Create and add step panels ---
        _step1Panel = CreateStep1Panel();
        _step2FilesPanel = CreateStep2FilesPanel();
        _step3FilesPanel = CreateStep3FilesPanel();
        _step2DiskettesPanel = CreateStep2DiskettesPanel();
        _step3DiskettesPanel = CreateStep3DiskettesPanel();
        _step2CdRomsPanel = CreateStep2CdRomsPanel();
        _step3CdRomsPanel = CreateStep3CdRomsPanel();

        wizardLayout.Controls.Add(_step1Panel, 0, 0);
        wizardLayout.Controls.Add(_step2FilesPanel, 0, 0);
        wizardLayout.Controls.Add(_step3FilesPanel, 0, 0);
        wizardLayout.Controls.Add(_step2DiskettesPanel, 0, 0);
        wizardLayout.Controls.Add(_step3DiskettesPanel, 0, 0);
        wizardLayout.Controls.Add(_step2CdRomsPanel, 0, 0);
        wizardLayout.Controls.Add(_step3CdRomsPanel, 0, 0);

        _step1Panel.Visible = true;
        _step2FilesPanel.Visible = false;
        _step3FilesPanel.Visible = false;
        _step2DiskettesPanel.Visible = false;
        _step3DiskettesPanel.Visible = false;
        _step2CdRomsPanel.Visible = false;
        _step3CdRomsPanel.Visible = false;
        _currentPanel = _step1Panel;

        // --- Error Label ---
        _errorLabel = new Label
        {
            Text = "",
            ForeColor = Color.Red,
            Dock = DockStyle.Fill,
            Margin = new Padding(0, 5, 0, 5),
            Visible = false
        };

        wizardLayout.Controls.Add(_errorLabel, 0, 1);

        // --- Bottom Buttons ---
        var buttonsPanel = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 4, RowCount = 1 };
        buttonsPanel.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize)); // Back
        buttonsPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F)); // Spacer
        buttonsPanel.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize)); // Next
        buttonsPanel.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize)); // Cancel

        _backButton = new Button { Text = "< Back", Enabled = false, AutoSize = true };
        _nextButton = new Button { Text = "Next >", Enabled = false, AutoSize = true };
        _cancelButton = new Button { Text = "Cancel", DialogResult = DialogResult.Cancel, AutoSize = true };

        _nextButton.Click += NextButton_Click;
        _backButton.Click += BackButton_Click;

        buttonsPanel.Controls.Add(_backButton, 0, 0);
        buttonsPanel.Controls.Add(_nextButton, 2, 0);
        buttonsPanel.Controls.Add(_cancelButton, 3, 0);
        wizardLayout.Controls.Add(buttonsPanel, 0, 2);

        Controls.Add(wizardLayout);
        CancelButton = _cancelButton;
        AcceptButton = _nextButton;

        UpdateWizardState();
        _isInitialized = true;
    }

    private void BrowseDirectoryButton_Click(object? sender, EventArgs e)
    {
        using var folderDialog = new FolderBrowserDialog
        {
            Description = "Select or Create Game Directory",
            ShowNewFolderButton = true,
            SelectedPath = _libraryPath // Start at the library root
        };

        // Pre-select a potential new game directory based on the game name
        string gameName = _gameNameTextBox.Text.Trim();
        if (!string.IsNullOrEmpty(gameName) && !string.IsNullOrEmpty(_libraryPath))
        {
            folderDialog.SelectedPath = Path.Combine(_libraryPath, gameName);
        }

        if (folderDialog.ShowDialog(this) == DialogResult.OK)
        {
            _gameDirectoryTextBox.Text = folderDialog.SelectedPath;
        }
    }

    private void ValidateForm()
    {
        if (!_isInitialized) return;

        if (_currentPanel == _step1Panel)
        {
            // Step 1 Validation
            string gameName = _gameNameTextBox.Text.Trim();
            if (string.IsNullOrWhiteSpace(gameName))
            {
                _errorLabel.Text = "Game name cannot be empty.";
                _errorLabel.Visible = true;
                _nextButton.Enabled = false;
                return;
            }
            if (_existingGameNames.Contains(gameName, StringComparer.OrdinalIgnoreCase))
            {
                _errorLabel.Text = "A game with this name already exists.";
                _errorLabel.Visible = true;
                _nextButton.Enabled = false;
                return;
            }

            if (string.IsNullOrWhiteSpace(_gameDirectoryTextBox.Text))
            {
                _errorLabel.Text = "Game directory must be selected.";
                _errorLabel.Visible = true;
                _nextButton.Enabled = false;
                return;
            }
        }
        else if (_currentPanel == _step2FilesPanel)
        {
            // Step 2 Validation
            if (_installFromFilesRadioButton.Checked)
            {
                if (string.IsNullOrWhiteSpace(_sourceDirectoryTextBox.Text))
                {
                    _errorLabel.Text = "Source directory must be selected.";
                    _errorLabel.Visible = true;
                    _nextButton.Enabled = false;
                    return;
                }
            }
            // Other installation types might not need this validation
        }
        else if (_currentPanel == _step2DiskettesPanel)
        {
            // Step 2 Diskettes Validation
            if (_disketteImagesListBox.Items.Count == 0)
            {
                _errorLabel.Text = "At least one diskette image must be added.";
                _errorLabel.Visible = true;
                _nextButton.Enabled = false;
                return;
            }
        }
        else if (_currentPanel == _step2CdRomsPanel)
        {
            // Step 2 CD-ROMs Validation
            if (_cdRomImagesListBox.Items.Count == 0)
            {
                _errorLabel.Text = "At least one CD-ROM image must be added.";
                _errorLabel.Visible = true;
                _nextButton.Enabled = false;
                return;
            }
        }

        // All valid
        _errorLabel.Text = "";
        _errorLabel.Visible = false;
        _nextButton.Enabled = true; // Enabled if no validation rule failed
    }

    private void BackButton_Click(object? sender, EventArgs e)
    {
        if (_currentPanel == null) return;

        var previousPanel = GetPreviousPanel();
        if (previousPanel == null) return;

        _currentPanel.Visible = false;
        _currentPanel = previousPanel;
        _currentPanel.Visible = true;

        UpdateWizardState();
    }

    private async void NextButton_Click(object? sender, EventArgs e)
    {
        if (_currentPanel == null) return;

        var nextPanel = GetNextPanel();

        if (nextPanel == null)
        {
            // This happens for unimplemented paths like CD-ROM or if we are on the final step.
            // If we are on the final step for "From Files", we execute the setup logic.
            if (_currentPanel == _step3FilesPanel)
            {
                await ExecuteGameSetup();
            }
            else if (_currentPanel == _step3DiskettesPanel)
            {
                await ExecuteDisketteGameSetup();
            }
            else if (_currentPanel == _step3CdRomsPanel)
            {
                await ExecuteCdRomGameSetup();
            }
            else
            {
                MessageBox.Show(this, "This installation method is not yet implemented beyond this point.", "Not Implemented", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
        else
        {
            _currentPanel.Visible = false;
            _currentPanel = nextPanel;
            _currentPanel.Visible = true;

            UpdateWizardState();
        }
    }

    private async Task ExecuteCdRomGameSetup()
    {
        _backButton.Enabled = false;
        _nextButton.Enabled = false;
        _cancelButton.Enabled = false;
        _cdRomProgressPanel.Visible = true;

        var progress = new Progress<GameSetupProgressReport>(report =>
        {
            if (this.IsDisposed) return;
            _cdRomProgressLabel.Text = report.Message;
            _cdRomProgressBar.Value = Math.Clamp(report.Percentage, 0, 100);
        });

        try
        {
            var setupService = new GameSetupService();

            GameName = _gameNameTextBox.Text.Trim();
            GameDirectory = _gameDirectoryTextBox.Text;
            CdRomImagePaths = _cdRomImagesListBox.Items.Cast<string>().ToList();

            CopiedCdRomImagePaths = await Task.Run(() =>
                setupService.SetupNewGameFromCdRoms(GameName, GameDirectory, CdRomImagePaths, progress)
            );

            string newCfgPath = Path.Combine(GameDirectory, "game.cfg");
            NewGameConfiguration = await GameDataReaderService.ParseCfgFileAsync(newCfgPath, GameDirectory);

            DialogResult = DialogResult.OK;
        }
        catch (Exception ex)
        {
            if (this.IsDisposed) return;
            MessageBox.Show(this, $"An error occurred during game setup: {ex.Message}", "Setup Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            _backButton.Enabled = true;
            _nextButton.Enabled = true;
            _cancelButton.Enabled = true;
            _cdRomProgressPanel.Visible = false;
        }
    }

    private async Task ExecuteDisketteGameSetup()
    {
        _backButton.Enabled = false;
        _nextButton.Enabled = false;
        _cancelButton.Enabled = false;
        _disketteProgressPanel.Visible = true;

        var progress = new Progress<GameSetupProgressReport>(report =>
        {
            if (this.IsDisposed) return;
            _disketteProgressLabel.Text = report.Message;
            _disketteProgressBar.Value = Math.Clamp(report.Percentage, 0, 100);
        });

        try
        {
            var setupService = new GameSetupService();

            GameName = _gameNameTextBox.Text.Trim();
            GameDirectory = _gameDirectoryTextBox.Text;
            DisketteImagePaths = _disketteImagesListBox.Items.Cast<string>().ToList();

            CopiedDisketteImagePaths = await Task.Run(() =>
                setupService.SetupNewGameFromDiskettes(GameName, GameDirectory, DisketteImagePaths, progress)
            );

            // At this point, the game is parsable.
            string newCfgPath = Path.Combine(GameDirectory, "game.cfg");
            NewGameConfiguration = await GameDataReaderService.ParseCfgFileAsync(newCfgPath, GameDirectory);

            // Signal success and the wizard will close. The main form will handle the rest.
            DialogResult = DialogResult.OK;
        }
        catch (Exception ex)
        {
            if (this.IsDisposed)
            {
                AppLogger.Log($"Error during diskette setup after dialog was closed: {ex.Message}");
                return;
            }

            MessageBox.Show(this, $"An error occurred during game setup: {ex.Message}", "Setup Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            _backButton.Enabled = true;
            _nextButton.Enabled = true;
            _cancelButton.Enabled = true;
            _disketteProgressPanel.Visible = false;
        }
    }

    private async Task ExecuteGameSetup()
    {
        // "Finish" button was clicked on the last step
        _backButton.Enabled = false;
        _nextButton.Enabled = false;
        _cancelButton.Enabled = false;
        _progressPanel.Visible = true;

        var progress = new Progress<GameSetupProgressReport>(report =>
        {
            _progressLabel.Text = report.Message;
            _progressBar.Value = Math.Clamp(report.Percentage, 0, 100);
        });

        try
        {
            var setupService = new GameSetupService();

            // Set public properties from the controls on the previous steps
            GameName = _gameNameTextBox.Text.Trim();
            GameDirectory = _gameDirectoryTextBox.Text;
            SourceDirectory = _sourceDirectoryTextBox.Text;

            // Perform the setup operations
            await Task.Run(() => setupService.SetupNewGameFromFiles(GameName, GameDirectory, SourceDirectory!, progress));

            // Parse the newly created game to add it to the list
            string newCfgPath = Path.Combine(GameDirectory, "game.cfg");
            NewGameConfiguration = await GameDataReaderService.ParseCfgFileAsync(newCfgPath, GameDirectory);

            MessageBox.Show(this, "Game setup completed successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            DialogResult = DialogResult.OK;
        }
        catch (Exception ex)
        {
            // Show a detailed error message and keep the wizard open
            MessageBox.Show(this, $"An error occurred during game setup:\n\n{ex.Message}", "Setup Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            _backButton.Enabled = true;
            _nextButton.Enabled = true;
            _cancelButton.Enabled = true;
            _progressPanel.Visible = false;
        }
    }

    private Panel? GetNextPanel()
    {
        if (_currentPanel == _step1Panel)
        {
            if (_installFromFilesRadioButton.Checked) return _step2FilesPanel;
            if (_installFromDiskettesRadioButton.Checked) return _step2DiskettesPanel;
            if (_installFromCdRomRadioButton.Checked) return _step2CdRomsPanel;
        }
        else if (_currentPanel == _step2FilesPanel)
        {
            return _step3FilesPanel;
        }
        else if (_currentPanel == _step2DiskettesPanel)
        {
            return _step3DiskettesPanel;
        }
        else if (_currentPanel == _step2CdRomsPanel)
        {
            return _step3CdRomsPanel;
        }

        // Return null if there is no next panel (end of the line for this path)
        return null;
    }

    private Panel? GetPreviousPanel()
    {
        if (_currentPanel == _step3FilesPanel) return _step2FilesPanel;
        if (_currentPanel == _step2FilesPanel) return _step1Panel;
        if (_currentPanel == _step2DiskettesPanel) return _step1Panel;
        if (_currentPanel == _step3DiskettesPanel) return _step2DiskettesPanel;
        if (_currentPanel == _step2CdRomsPanel) return _step1Panel;
        if (_currentPanel == _step3CdRomsPanel) return _step2CdRomsPanel;

        return null;
    }

private Panel CreateStep2CdRomsPanel()
    {
        var step2Panel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 4
        };
        step2Panel.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // Top Label
        step2Panel.RowStyles.Add(new RowStyle(SizeType.Percent, 100F)); // ListBox and buttons
        step2Panel.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // Add button
        step2Panel.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // Bottom instructions

        // --- Top Instructions ---
        var topLabel = new Label
        {
            Text = "Select the installation CD-ROM images (.iso or .cue).",
            Dock = DockStyle.Fill,
            AutoSize = true,
            Margin = new Padding(0, 0, 0, 10)
        };

        // --- Main Content (ListBox and order buttons) ---
        var contentPanel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1
        };
        contentPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        contentPanel.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));

        _cdRomImagesListBox = new ListBox { Dock = DockStyle.Fill, SelectionMode = SelectionMode.One };
        _cdRomImagesListBox.SelectedIndexChanged += CdRomImagesListBox_SelectedIndexChanged;

        var orderButtonsPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.TopDown,
            AutoSize = true,
            Margin = new Padding(5, 0, 0, 0)
        };

        _moveCdRomUpButton = new Button { Text = "▲", Enabled = false, Size = new Size(30, 30), Font = new Font("Segoe UI", 9F) };
        _moveCdRomDownButton = new Button { Text = "▼", Enabled = false, Size = new Size(30, 30), Font = new Font("Segoe UI", 9F) };
        _deleteCdRomButton = new Button { Text = "X", Enabled = false, Size = new Size(30, 30), ForeColor = Color.Red, Font = new Font("Segoe UI", 9F, FontStyle.Bold) };

        _moveCdRomUpButton.Click += MoveCdRomUpButton_Click;
        _moveCdRomDownButton.Click += MoveCdRomDownButton_Click;
        _deleteCdRomButton.Click += DeleteCdRomButton_Click;

        orderButtonsPanel.Controls.AddRange(new Control[] { _moveCdRomUpButton, _moveCdRomDownButton, _deleteCdRomButton });

        contentPanel.Controls.Add(_cdRomImagesListBox, 0, 0);
        contentPanel.Controls.Add(orderButtonsPanel, 1, 0);

        // --- Add Button ---
        _addCdRomImagesButton = new Button { Text = "Add CD-ROM images...", AutoSize = true, Anchor = AnchorStyles.Left, Margin = new Padding(0, 5, 0, 5) };
        _addCdRomImagesButton.Click += AddCdRomImagesButton_Click;

        // --- Bottom Instructions ---
        var bottomInstructions = new TextBox
        {
            Text = "Add CD-ROM image files to the list and ensure they are in the correct order. These images will be mounted on drive d: and during installation you will be able to swap disk images with CTRL-F4.",
            Dock = DockStyle.Fill,
            Multiline = true,
            ReadOnly = true,
            BorderStyle = BorderStyle.None,
            BackColor = SystemColors.Control,
            Margin = new Padding(0, 10, 0, 0)
        };

        // --- Add Controls to Panel ---
        step2Panel.Controls.Add(topLabel, 0, 0);
        step2Panel.Controls.Add(contentPanel, 0, 1);
        step2Panel.Controls.Add(_addCdRomImagesButton, 0, 2);
        step2Panel.Controls.Add(bottomInstructions, 0, 3);

        return step2Panel;
    }

    private void CdRomImagesListBox_SelectedIndexChanged(object? sender, EventArgs e)
    {
        int selectedIndex = _cdRomImagesListBox.SelectedIndex;
        bool isItemSelected = selectedIndex != -1;

        _deleteCdRomButton.Enabled = isItemSelected;
        _moveCdRomUpButton.Enabled = isItemSelected && selectedIndex > 0;
        _moveCdRomDownButton.Enabled = isItemSelected && selectedIndex < _cdRomImagesListBox.Items.Count - 1;
    }

    private void AddCdRomImagesButton_Click(object? sender, EventArgs e)
    {
        using var openFileDialog = new OpenFileDialog
        {
            Title = "Select CD-ROM Images",
            Filter = "CD Images (*.iso;*.cue)|*.iso;*.cue|All files (*.*)|*.*",
            Multiselect = true
        };

        if (openFileDialog.ShowDialog(this) == DialogResult.OK)
        {
            var currentItems = _cdRomImagesListBox.Items.Cast<string>().ToHashSet(StringComparer.OrdinalIgnoreCase);
            var newItems = openFileDialog.FileNames.Where(f => !currentItems.Contains(f));
            _cdRomImagesListBox.Items.AddRange(newItems.ToArray());
            ValidateForm();
        }
    }

    private void MoveCdRomUpButton_Click(object? sender, EventArgs e)
    {
        int selectedIndex = _cdRomImagesListBox.SelectedIndex;
        if (selectedIndex > 0)
        {
            object item = _cdRomImagesListBox.SelectedItem;
            _cdRomImagesListBox.Items.RemoveAt(selectedIndex);
            _cdRomImagesListBox.Items.Insert(selectedIndex - 1, item);
            _cdRomImagesListBox.SelectedIndex = selectedIndex - 1;
        }
    }

    private void MoveCdRomDownButton_Click(object? sender, EventArgs e)
    {
        int selectedIndex = _cdRomImagesListBox.SelectedIndex;
        if (selectedIndex != -1 && selectedIndex < _cdRomImagesListBox.Items.Count - 1)
        {
            object item = _cdRomImagesListBox.SelectedItem;
            _cdRomImagesListBox.Items.RemoveAt(selectedIndex);
            _cdRomImagesListBox.Items.Insert(selectedIndex + 1, item);
            _cdRomImagesListBox.SelectedIndex = selectedIndex + 1;
        }
    }

    private void DeleteCdRomButton_Click(object? sender, EventArgs e)
    {
        int selectedIndex = _cdRomImagesListBox.SelectedIndex;
        if (selectedIndex != -1)
        {
            _cdRomImagesListBox.Items.RemoveAt(selectedIndex);
            ValidateForm();
        }
    }

    private bool IsFinalStep(Panel? panel)
    {
        // Define which panels are considered "final" steps where "Next" becomes "Confirm" or "Finish"
        return panel == _step3FilesPanel || panel == _step3DiskettesPanel || panel == _step3CdRomsPanel;
        // When other paths are implemented, they will be added here, e.g.:
        // || panel == _step3DiskettesPanel
    }

    private void BrowseSourceDirectoryButton_Click(object? sender, EventArgs e)
    {
        using var folderDialog = new FolderBrowserDialog
        {
            Description = "Select the directory containing the game files",
            ShowNewFolderButton = false
        };

        if (folderDialog.ShowDialog(this) == DialogResult.OK)
        {
            _sourceDirectoryTextBox.Text = folderDialog.SelectedPath;
        }
    }

    private void UpdateWizardState()
    {
        _backButton.Enabled = GetPreviousPanel() != null;

        if (IsFinalStep(_currentPanel))
        {
            _nextButton.Text = "Confirm";
        }
        else
        {
            _nextButton.Text = "Next >";
        }

        // If we are on the review screen, populate it with the latest data.
        if (_currentPanel == _step3FilesPanel)
        {
            PopulateReviewScreen();
        }
        else if (_currentPanel == _step3DiskettesPanel)
        {
            PopulateDisketteReviewScreen();
        }
        else if (_currentPanel == _step3CdRomsPanel)
        {
            PopulateCdRomReviewScreen();
        }

        ValidateForm();

    }

private void PopulateCdRomReviewScreen()
    {
        string gameName = _gameNameTextBox.Text.Trim();
        string targetDir = _gameDirectoryTextBox.Text;
        string relativeTargetDir = Path.GetRelativePath(_libraryPath, targetDir);

        _reviewCdRomGameNameTextBox.Text = gameName;
        _reviewCdRomGameDirectoryTextBox.Text = relativeTargetDir;

        _reviewCdRomImagesListBox.Items.Clear();
        foreach (var item in _cdRomImagesListBox.Items)
        {
            _reviewCdRomImagesListBox.Items.Add(Path.GetFileName(item.ToString()));
        }

        _reviewCdRomInstructionsTextBox.Text = "When you press Confirm, we will: " +
                                               "1) Create the necesary folders in the selected game directory, " +
                                               "2) Create a basic game.cfg file, " +
                                               "3) Copy a dosbox-staging.conf template file to the target directory and " +
                                               "4) We will launch DOSBox while mounting the directory game-files as the C: drive and your CD-ROM image(s) on drive D:. " +
                                               "Once you are at the DOSBox DOS prompt proceed to install from drive D: " +
                                               "When you are done you can type EXIT and within the application you should change the commands needed to run the game.";
    }

    private Panel CreateStep3CdRomsPanel()
    {
        var step3Panel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(5),
            ColumnCount = 2,
            RowCount = 6 // Title, Name, Target Dir, CD-ROMs, Instructions, Progress
        };
        step3Panel.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        step3Panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        step3Panel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        step3Panel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        step3Panel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        step3Panel.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));  // ListBox
        step3Panel.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));  // Instructions
        step3Panel.RowStyles.Add(new RowStyle(SizeType.AutoSize));      // Progress

        var reviewLabel = new Label { Text = "Review and Confirm", Dock = DockStyle.Top, Font = new Font(Font, FontStyle.Bold), AutoSize = true, Padding = new Padding(0, 0, 0, 10) };
        step3Panel.Controls.Add(reviewLabel, 0, 0);
        step3Panel.SetColumnSpan(reviewLabel, 2);

        // --- Game Name ---
        var nameLabel = new Label { Text = "Game Name:", Anchor = AnchorStyles.Left, AutoSize = true, Margin = new Padding(0, 6, 5, 0) };
        _reviewCdRomGameNameTextBox = new TextBox { Dock = DockStyle.Fill, ReadOnly = true, Margin = new Padding(0, 3, 0, 3) };
        step3Panel.Controls.Add(nameLabel, 0, 1);
        step3Panel.Controls.Add(_reviewCdRomGameNameTextBox, 1, 1);

        // --- Game Target Directory ---
        var targetDirLabel = new Label { Text = "Target Directory:", Anchor = AnchorStyles.Left, AutoSize = true, Margin = new Padding(0, 6, 5, 0) };
        _reviewCdRomGameDirectoryTextBox = new TextBox { Dock = DockStyle.Fill, ReadOnly = true, Margin = new Padding(0, 3, 0, 3) };
        step3Panel.Controls.Add(targetDirLabel, 0, 2);
        step3Panel.Controls.Add(_reviewCdRomGameDirectoryTextBox, 1, 2);

        // --- CD-ROM Images ---
        var cdRomsLabel = new Label { Text = "CD-ROM Images:", Anchor = AnchorStyles.Top | AnchorStyles.Left, AutoSize = true, Margin = new Padding(0, 6, 5, 0) };
        _reviewCdRomImagesListBox = new ListBox { Dock = DockStyle.Fill, Margin = new Padding(0, 3, 0, 3), SelectionMode = SelectionMode.None, BackColor = SystemColors.Control };
        step3Panel.Controls.Add(cdRomsLabel, 0, 3);
        step3Panel.Controls.Add(_reviewCdRomImagesListBox, 1, 3);

        // --- Instructions ---
        _reviewCdRomInstructionsTextBox = new TextBox { Multiline = true, ReadOnly = true, Dock = DockStyle.Fill, Margin = new Padding(0, 15, 0, 0), BorderStyle = BorderStyle.None, BackColor = SystemColors.Control };
        step3Panel.Controls.Add(_reviewCdRomInstructionsTextBox, 0, 4);
        step3Panel.SetColumnSpan(_reviewCdRomInstructionsTextBox, 2);

        // --- Progress Panel ---
        _cdRomProgressPanel = new Panel { Dock = DockStyle.Fill, Visible = false, Margin = new Padding(0, 10, 0, 0) };
        var progressLayout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2 };
        progressLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        progressLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        _cdRomProgressLabel = new Label { Text = "Starting...", Dock = DockStyle.Fill, AutoSize = true, Margin = new Padding(0, 0, 0, 3) };
        _cdRomProgressBar = new ProgressBar { Dock = DockStyle.Fill, Minimum = 0, Maximum = 100 };

        progressLayout.Controls.Add(_cdRomProgressLabel, 0, 0);
        progressLayout.Controls.Add(_cdRomProgressBar, 0, 1);
        _cdRomProgressPanel.Controls.Add(progressLayout);
        step3Panel.Controls.Add(_cdRomProgressPanel, 0, 5);
        step3Panel.SetColumnSpan(_cdRomProgressPanel, 2);

        return step3Panel;
    }

    private void PopulateDisketteReviewScreen()
    {
        string gameName = _gameNameTextBox.Text.Trim();
        string targetDir = _gameDirectoryTextBox.Text;
        string relativeTargetDir = Path.GetRelativePath(_libraryPath, targetDir);

        _reviewDisketteGameNameTextBox.Text = gameName;
        _reviewDisketteGameDirectoryTextBox.Text = relativeTargetDir;

        _reviewDisketteImagesListBox.Items.Clear();
        foreach (var item in _disketteImagesListBox.Items)
        {
            // Add just the filename for a cleaner look in the review
            _reviewDisketteImagesListBox.Items.Add(Path.GetFileName(item.ToString()));
        }

        _reviewDisketteInstructionsTextBox.Text = "After pressing the Confirm button we will: " +
                                                  "1) Create the necesary game directory structue, " +
                                                  "2) Copy a default version of the DOSBox Staging configuration file. " +
                                                  "3) Create a basic game.cfg file. " +
                                                  "4) Copy your disk images to the <game-directory>\\disk-images directory and " +
                                                  "5) Launch DOSBox with the disk images mounted on the drive a: and it will be up to you to install the game from the diskettes. " +
                                                  "When you are done, type 'exit' to close DOSBox and complete the configuration with the commands needed to launch the game.";
    }

    private Panel CreateStep3DiskettesPanel()
    {
        var step3Panel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(5),
            ColumnCount = 2,
            RowCount = 6 // Title, Name, Target Dir, Diskettes, Instructions, Progress
        };
        step3Panel.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        step3Panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        step3Panel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        step3Panel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        step3Panel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        step3Panel.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));  // ListBox
        step3Panel.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));  // Instructions
        step3Panel.RowStyles.Add(new RowStyle(SizeType.AutoSize));      // Progress

        var reviewLabel = new Label
        {
            Text = "Review and Confirm",
            Dock = DockStyle.Top,
            Font = new Font(Font, FontStyle.Bold),
            AutoSize = true,
            Padding = new Padding(0, 0, 0, 10)
        };
        step3Panel.Controls.Add(reviewLabel, 0, 0);
        step3Panel.SetColumnSpan(reviewLabel, 2);

        // --- Game Name ---
        var nameLabel = new Label { Text = "Game Name:", Anchor = AnchorStyles.Left, AutoSize = true, Margin = new Padding(0, 6, 5, 0) };
        _reviewDisketteGameNameTextBox = new TextBox { Dock = DockStyle.Fill, ReadOnly = true, Margin = new Padding(0, 3, 0, 3) };
        step3Panel.Controls.Add(nameLabel, 0, 1);
        step3Panel.Controls.Add(_reviewDisketteGameNameTextBox, 1, 1);

        // --- Game Target Directory ---
        var targetDirLabel = new Label { Text = "Target Directory:", Anchor = AnchorStyles.Left, AutoSize = true, Margin = new Padding(0, 6, 5, 0) };
        _reviewDisketteGameDirectoryTextBox = new TextBox { Dock = DockStyle.Fill, ReadOnly = true, Margin = new Padding(0, 3, 0, 3) };
        step3Panel.Controls.Add(targetDirLabel, 0, 2);
        step3Panel.Controls.Add(_reviewDisketteGameDirectoryTextBox, 1, 2);

        // --- Diskette Images ---
        var diskettesLabel = new Label { Text = "Diskette Images:", Anchor = AnchorStyles.Top | AnchorStyles.Left, AutoSize = true, Margin = new Padding(0, 6, 5, 0) };
        _reviewDisketteImagesListBox = new ListBox { Dock = DockStyle.Fill, Margin = new Padding(0, 3, 0, 3), SelectionMode = SelectionMode.None, BackColor = SystemColors.Control };
        step3Panel.Controls.Add(diskettesLabel, 0, 3);
        step3Panel.Controls.Add(_reviewDisketteImagesListBox, 1, 3);

        // --- Instructions ---
        _reviewDisketteInstructionsTextBox = new TextBox
        {
            Multiline = true,
            ReadOnly = true,
            Dock = DockStyle.Fill,
            Margin = new Padding(0, 15, 0, 0),
            BorderStyle = BorderStyle.None,
            BackColor = SystemColors.Control
        };
        step3Panel.Controls.Add(_reviewDisketteInstructionsTextBox, 0, 4);
        step3Panel.SetColumnSpan(_reviewDisketteInstructionsTextBox, 2);

        // --- Progress Panel ---
        _disketteProgressPanel = new Panel { Dock = DockStyle.Fill, Visible = false, Margin = new Padding(0, 10, 0, 0) };
        var progressLayout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2 };
        progressLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        progressLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        _disketteProgressLabel = new Label
        {
            Text = "Starting...",
            Dock = DockStyle.Fill,
            AutoSize = true,
            Margin = new Padding(0, 0, 0, 3)
        };

        _disketteProgressBar = new ProgressBar { Dock = DockStyle.Fill, Minimum = 0, Maximum = 100 };

        progressLayout.Controls.Add(_disketteProgressLabel, 0, 0);
        progressLayout.Controls.Add(_disketteProgressBar, 0, 1);
        _disketteProgressPanel.Controls.Add(progressLayout);
        step3Panel.Controls.Add(_disketteProgressPanel, 0, 5);
        step3Panel.SetColumnSpan(_disketteProgressPanel, 2);

        return step3Panel;
    }

    private Panel CreateStep1Panel()
    {
        var step1Panel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 3
        };
        step1Panel.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        step1Panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        step1Panel.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // Game Name
        step1Panel.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // Directory
        step1Panel.RowStyles.Add(new RowStyle(SizeType.Percent, 100F)); // Setup Type

        // --- Game Name ---
        var gameNameLabel = new Label { Text = "Game Name:", Anchor = AnchorStyles.Left, AutoSize = true, Margin = new Padding(0, 6, 5, 0) };
        _gameNameTextBox = new TextBox { Dock = DockStyle.Fill, MaxLength = 100 };
        _gameNameTextBox.Validated += (s, e) => ValidateForm();

        // --- Game Directory ---
        var gameDirectoryLabel = new Label { Text = "Game Directory:", Anchor = AnchorStyles.Left, AutoSize = true, Margin = new Padding(0, 6, 5, 0) };
        var directoryPanel = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1, AutoSize = true };
        directoryPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        directoryPanel.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        _gameDirectoryTextBox = new TextBox { Dock = DockStyle.Fill, ReadOnly = true };
        _gameDirectoryTextBox.TextChanged += (s, e) => ValidateForm();

        _browseDirectoryButton = new Button { Text = "Browse...", Margin = new Padding(5, 0, 0, 0), AutoSize = true };
        _browseDirectoryButton.Click += BrowseDirectoryButton_Click;
        directoryPanel.Controls.Add(_gameDirectoryTextBox, 0, 0);
        directoryPanel.Controls.Add(_browseDirectoryButton, 1, 0);

        // --- Setup Type ---
        var setupTypeGroupBox = new GroupBox { Text = "Setup Type", Dock = DockStyle.Fill, Margin = new Padding(0, 10, 0, 10) };
        var setupContentPanel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1
        };
        setupContentPanel.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        setupContentPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));

        var radioButtonsPanel = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown, Padding = new Padding(10), AutoSize = true };
        _installFromFilesRadioButton = new RadioButton { Text = "Install from files", Checked = true, AutoSize = true, Margin = new Padding(3, 3, 3, 10) };
        _installFromDiskettesRadioButton = new RadioButton { Text = "Install from diskettes", AutoSize = true, Margin = new Padding(3, 3, 3, 10) };
        _installFromCdRomRadioButton = new RadioButton { Text = "Install from CD-ROMs", AutoSize = true };

        _installFromFilesRadioButton.CheckedChanged += OnSetupTypeChanged;
        _installFromDiskettesRadioButton.CheckedChanged += OnSetupTypeChanged;
        _installFromCdRomRadioButton.CheckedChanged += OnSetupTypeChanged;

        radioButtonsPanel.Controls.AddRange(new Control[] { _installFromFilesRadioButton, _installFromDiskettesRadioButton, _installFromCdRomRadioButton });

        _instructionsLabel = new Label
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(10),
            BorderStyle = BorderStyle.Fixed3D,
            BackColor = SystemColors.ControlLight
        };
        setupContentPanel.Controls.Add(radioButtonsPanel, 0, 0);
        setupContentPanel.Controls.Add(_instructionsLabel, 1, 0);
        setupTypeGroupBox.Controls.Add(setupContentPanel);

        // --- Add Controls to Panel ---
        step1Panel.Controls.Add(gameNameLabel, 0, 0);
        step1Panel.Controls.Add(_gameNameTextBox, 1, 0);
        step1Panel.Controls.Add(gameDirectoryLabel, 0, 1);
        step1Panel.Controls.Add(directoryPanel, 1, 1);
        step1Panel.Controls.Add(setupTypeGroupBox, 0, 2);
        step1Panel.SetColumnSpan(setupTypeGroupBox, 2);

        UpdateInstructionsText();

        return step1Panel;
    }

    private Panel CreateStep2FilesPanel()
    {
        var step2Panel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 3 // Label, Directory selection, Spacer
        };
        step2Panel.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        step2Panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        step2Panel.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // Instructions
        step2Panel.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // Directory
        step2Panel.RowStyles.Add(new RowStyle(SizeType.Percent, 100F)); // Spacer

        // --- Instructions ---
        var instructionsLabel = new Label
        {
            Text = "Select the directory containing the game's installation files.",
            Dock = DockStyle.Fill,
            AutoSize = true,
            Margin = new Padding(0, 0, 0, 15)
        };

        // --- Source Directory ---
        var sourceDirectoryLabel = new Label { Text = "Source Directory:", Anchor = AnchorStyles.Left, AutoSize = true, Margin = new Padding(0, 6, 5, 0) };
        var directoryPanel = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1, AutoSize = true };
        directoryPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        directoryPanel.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        _sourceDirectoryTextBox = new TextBox { Dock = DockStyle.Fill, ReadOnly = true };
        _sourceDirectoryTextBox.TextChanged += (s, e) => ValidateForm();
        _browseSourceDirectoryButton = new Button { Text = "Browse...", Margin = new Padding(5, 0, 0, 0), AutoSize = true };
        _browseSourceDirectoryButton.Click += BrowseSourceDirectoryButton_Click;
        directoryPanel.Controls.Add(_sourceDirectoryTextBox, 0, 0);
        directoryPanel.Controls.Add(_browseSourceDirectoryButton, 1, 0);

        // --- Add Controls to Panel ---
        step2Panel.Controls.Add(instructionsLabel, 0, 0);
        step2Panel.SetColumnSpan(instructionsLabel, 2);
        step2Panel.Controls.Add(sourceDirectoryLabel, 0, 1);
        step2Panel.Controls.Add(directoryPanel, 1, 1);

        return step2Panel;
    }

    private Panel CreateStep3FilesPanel()
    {
        var step3Panel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(5),
            ColumnCount = 2,
            RowCount = 6 // Title, Name, Target Dir, Source Dir, Instructions, Progress
        };
        step3Panel.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        step3Panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        step3Panel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        step3Panel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        step3Panel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        step3Panel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        step3Panel.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        step3Panel.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        var reviewLabel = new Label
        {
            Text = "Review and Confirm",
            Dock = DockStyle.Top,
            Font = new Font(Font, FontStyle.Bold),
            AutoSize = true,
            Padding = new Padding(0, 0, 0, 10)
        };

        step3Panel.Controls.Add(reviewLabel, 0, 0);
        step3Panel.SetColumnSpan(reviewLabel, 2);

        // --- Game Name ---
        var nameLabel = new Label { Text = "Game Name:", Anchor = AnchorStyles.Left, AutoSize = true, Margin = new Padding(0, 6, 5, 0) };
        _reviewGameNameTextBox = new TextBox { Dock = DockStyle.Fill, ReadOnly = true, Margin = new Padding(0, 3, 0, 3) };
        step3Panel.Controls.Add(nameLabel, 0, 1);
        step3Panel.Controls.Add(_reviewGameNameTextBox, 1, 1);

        // --- Game Target Directory ---
        var targetDirLabel = new Label { Text = "Target Directory:", Anchor = AnchorStyles.Left, AutoSize = true, Margin = new Padding(0, 6, 5, 0) };
        _reviewGameDirectoryTextBox = new TextBox { Dock = DockStyle.Fill, ReadOnly = true, Margin = new Padding(0, 3, 0, 3) };
        step3Panel.Controls.Add(targetDirLabel, 0, 2);
        step3Panel.Controls.Add(_reviewGameDirectoryTextBox, 1, 2);

        // --- Game Source Directory ---
        var sourceDirLabel = new Label { Text = "Source Directory:", Anchor = AnchorStyles.Left, AutoSize = true, Margin = new Padding(0, 6, 5, 0) };
        _reviewSourceDirectoryTextBox = new TextBox { Dock = DockStyle.Fill, ReadOnly = true, Margin = new Padding(0, 3, 0, 3) };
        step3Panel.Controls.Add(sourceDirLabel, 0, 3);
        step3Panel.Controls.Add(_reviewSourceDirectoryTextBox, 1, 3);

        // --- Instructions ---
        _reviewInstructionsTextBox = new TextBox
        {
            Multiline = true,
            ReadOnly = true,
            Dock = DockStyle.Fill,
            Margin = new Padding(0, 15, 0, 0),
            BorderStyle = BorderStyle.None,
            BackColor = SystemColors.Control
        };
        step3Panel.Controls.Add(_reviewInstructionsTextBox, 0, 4);
        step3Panel.SetColumnSpan(_reviewInstructionsTextBox, 2);

        // --- Progress Panel ---
        _progressPanel = new Panel { Dock = DockStyle.Fill, Visible = false, Margin = new Padding(0, 10, 0, 0) };
        var progressLayout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2 };
        progressLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        progressLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        _progressLabel = new Label
        {
            Text = "Starting...",
            Dock = DockStyle.Fill,
            AutoSize = true,
            Margin = new Padding(0, 0, 0, 3)
        };

        _progressBar = new ProgressBar { Dock = DockStyle.Fill, Minimum = 0, Maximum = 100 };

        progressLayout.Controls.Add(_progressLabel, 0, 0);
        progressLayout.Controls.Add(_progressBar, 0, 1);
        _progressPanel.Controls.Add(progressLayout);
        step3Panel.Controls.Add(_progressPanel, 0, 5);
        step3Panel.SetColumnSpan(_progressPanel, 2);

        return step3Panel;
    }

    private void OnSetupTypeChanged(object? sender, EventArgs e)
    {
        if (sender is RadioButton { Checked: true })
        {
            UpdateInstructionsText();
        }
    }

    private void UpdateInstructionsText()
    {
        if (_installFromFilesRadioButton.Checked)
        {
            _instructionsLabel.Text = "Use this option if your game files are ready to be copied into a directory like C:\\games\\mygame. All files and directories will be copied into the library game structure.";
        }
        else if (_installFromDiskettesRadioButton.Checked)
        {
            _instructionsLabel.Text = "With this option you will be able to mount one or more diskette images and we will open a DOSBox session where you can go to drive a: and start the installation. You will be able to swap diskettes with CTRL-F4.";
        }
        else if (_installFromCdRomRadioButton.Checked)
        {
            _instructionsLabel.Text = "Use this option to install your game from one or more CD-ROM disk images. You will be asked to select the location of your disk images. Those will be copied to the library game structure and after that we will open a DOSBox session where you can go to drive d: and start the installation. You will be able to swap disk images with CTRL-F4.";
        }
    }
    private void PopulateReviewScreen()
    {
        string gameName = _gameNameTextBox.Text.Trim();
        string sourceDir = _sourceDirectoryTextBox.Text;
        string targetDir = _gameDirectoryTextBox.Text;
        string relativeTargetDir = Path.GetRelativePath(_libraryPath, targetDir);

        _reviewGameNameTextBox.Text = gameName;
        _reviewSourceDirectoryTextBox.Text = sourceDir;
        _reviewGameDirectoryTextBox.Text = relativeTargetDir;

        _reviewInstructionsTextBox.Text = "After you press Confirm, we will: " +
                                          "1) Create the basic folder structure for your game. " +
                                          "2)  Add a basic dosbox-staging.conf file, which you can tune later. " +
                                          "3) Copy all the files inside the source directory, including any subdirectories into <game-directory>\\game-files\\game. " +
                                          "After this, you need to edit your new game details and add the necesary commands to start the game. " +
                                          "After this, you should be able to launch your game.";
    }
    
    private Panel CreateStep2DiskettesPanel()
    {
        var step2Panel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 4
        };
        step2Panel.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // Top Label
        step2Panel.RowStyles.Add(new RowStyle(SizeType.Percent, 100F)); // ListBox and buttons
        step2Panel.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // Add button
        step2Panel.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // Bottom instructions

        // --- Top Instructions ---
        var topLabel = new Label
        {
            Text = "Select the installation disk images.",
            Dock = DockStyle.Fill,
            AutoSize = true,
            Margin = new Padding(0, 0, 0, 10)
        };

        // --- Main Content (ListBox and order buttons) ---
        var contentPanel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1
        };
        contentPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        contentPanel.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));

        _disketteImagesListBox = new ListBox { Dock = DockStyle.Fill, SelectionMode = SelectionMode.One };
        _disketteImagesListBox.SelectedIndexChanged += DisketteImagesListBox_SelectedIndexChanged;

        var orderButtonsPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.TopDown,
            AutoSize = true,
            Margin = new Padding(5, 0, 0, 0)
        };

        _moveDisketteUpButton = new Button { Text = "▲", Enabled = false, Size = new Size(30, 30), Font = new Font("Segoe UI", 9F) };
        _moveDisketteDownButton = new Button { Text = "▼", Enabled = false, Size = new Size(30, 30), Font = new Font("Segoe UI", 9F) };
        _deleteDisketteButton = new Button { Text = "X", Enabled = false, Size = new Size(30, 30), ForeColor = Color.Red, Font = new Font("Segoe UI", 9F, FontStyle.Bold) };

        _moveDisketteUpButton.Click += MoveDisketteUpButton_Click;
        _moveDisketteDownButton.Click += MoveDisketteDownButton_Click;
        _deleteDisketteButton.Click += DeleteDisketteButton_Click;

        orderButtonsPanel.Controls.AddRange(new Control[] { _moveDisketteUpButton, _moveDisketteDownButton, _deleteDisketteButton });

        contentPanel.Controls.Add(_disketteImagesListBox, 0, 0);
        contentPanel.Controls.Add(orderButtonsPanel, 1, 0);

        // --- Add Button ---
        _addDisketteImagesButton = new Button { Text = "Add disk images...", AutoSize = true, Anchor = AnchorStyles.Left, Margin = new Padding(0, 5, 0, 5) };
        _addDisketteImagesButton.Click += AddDisketteImagesButton_Click;

        // --- Bottom Instructions ---
        var bottomInstructions = new TextBox
        {
            Text = "Add diskette image files to the list and ensure they are in the correct order. These images will be mounted on drive a: and during installation you will be able to swap disk images with CTRL-F4.",
            Dock = DockStyle.Fill,
            Multiline = true,
            ReadOnly = true,
            BorderStyle = BorderStyle.None,
            BackColor = SystemColors.Control,
            Margin = new Padding(0, 10, 0, 0)
        };

        // --- Add Controls to Panel ---
        step2Panel.Controls.Add(topLabel, 0, 0);
        step2Panel.Controls.Add(contentPanel, 0, 1);
        step2Panel.Controls.Add(_addDisketteImagesButton, 0, 2);
        step2Panel.Controls.Add(bottomInstructions, 0, 3);

        return step2Panel;
    }

    private void DisketteImagesListBox_SelectedIndexChanged(object? sender, EventArgs e)
    {
        int selectedIndex = _disketteImagesListBox.SelectedIndex;
        bool isItemSelected = selectedIndex != -1;

        _deleteDisketteButton.Enabled = isItemSelected;
        _moveDisketteUpButton.Enabled = isItemSelected && selectedIndex > 0;
        _moveDisketteDownButton.Enabled = isItemSelected && selectedIndex < _disketteImagesListBox.Items.Count - 1;
    }

    private void AddDisketteImagesButton_Click(object? sender, EventArgs e)
    {
        using var openFileDialog = new OpenFileDialog
        {
            Title = "Select Diskette Images",
            Filter = "Diskette Images (*.img)|*.img|All files (*.*)|*.*",
            Multiselect = true
        };

        if (openFileDialog.ShowDialog(this) == DialogResult.OK)
        {
            // Get existing items to prevent duplicates
            var currentItems = _disketteImagesListBox.Items.Cast<string>().ToHashSet(StringComparer.OrdinalIgnoreCase);
            var newItems = openFileDialog.FileNames.Where(f => !currentItems.Contains(f));
            _disketteImagesListBox.Items.AddRange(newItems.ToArray());
            ValidateForm(); // Re-validate to enable the Next button if needed
        }
    }

    private void MoveDisketteUpButton_Click(object? sender, EventArgs e)
    {
        int selectedIndex = _disketteImagesListBox.SelectedIndex;
        if (selectedIndex > 0)
        {
            object item = _disketteImagesListBox.SelectedItem;
            _disketteImagesListBox.Items.RemoveAt(selectedIndex);
            _disketteImagesListBox.Items.Insert(selectedIndex - 1, item);
            _disketteImagesListBox.SelectedIndex = selectedIndex - 1;
        }
    }

    private void MoveDisketteDownButton_Click(object? sender, EventArgs e)
    {
        int selectedIndex = _disketteImagesListBox.SelectedIndex;
        if (selectedIndex != -1 && selectedIndex < _disketteImagesListBox.Items.Count - 1)
        {
            object item = _disketteImagesListBox.SelectedItem;
            _disketteImagesListBox.Items.RemoveAt(selectedIndex);
            _disketteImagesListBox.Items.Insert(selectedIndex + 1, item);
            _disketteImagesListBox.SelectedIndex = selectedIndex + 1;
        }
    }

    private void DeleteDisketteButton_Click(object? sender, EventArgs e)
    {
        int selectedIndex = _disketteImagesListBox.SelectedIndex;
        if (selectedIndex != -1)
        {
            _disketteImagesListBox.Items.RemoveAt(selectedIndex);
            ValidateForm(); // Re-validate in case the list is now empty
        }
    }
}
