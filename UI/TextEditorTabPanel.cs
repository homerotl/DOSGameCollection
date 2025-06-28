using System.ComponentModel;

namespace DOSGameCollection.UI;

public class TextEditorTabPanel : UserControl
{
    private TextBox? contentTextBox;
    private Button? editButton;
    private Button? saveButton;
    private Button? cancelButton;

    private string? _filePath;
    private string? _originalContent;

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public string? FilePath
    {
        get => _filePath;
        set
        {
            _filePath = value;
            // When FilePath is set, we are loading a new game.
            // Ensure we are not in edit mode from a previous selection.
            if (IsEditing)
            {
                CancelEditMode(suppressEvent: true);
            }
            LoadContentAsync();
        }
    }

    public bool IsEditing => saveButton?.Visible ?? false;

    public event EventHandler? EditModeStarted;
    public event EventHandler? EditModeEnded;

    public TextEditorTabPanel()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        var symbolFont = FormatTools.GetSymbolFont();

        TableLayoutPanel mainPanel = new()
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2
        };
        mainPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));      // For buttons
        mainPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100F)); // For textbox

        FlowLayoutPanel buttonsPanel = new()
        {
            FlowDirection = FlowDirection.RightToLeft,
            Dock = DockStyle.Fill,
            AutoSize = true,
            Margin = new Padding(0, 5, 0, 0)
        };

        editButton = new Button
        {
            AutoSize = true,
            Margin = new Padding(0, 0, 5, 5),
            Enabled = false,
            Text = symbolFont != null ? "\u270E" : "Edit"
        };
        if (symbolFont != null) { editButton.Font = symbolFont; }
        editButton.Click += EditButton_Click;

        saveButton = new Button
        {
            AutoSize = true,
            Margin = new Padding(0, 0, 5, 5),
            Visible = false,
            Text = symbolFont != null ? "\uD83D\uDCBE" : "Save"
        };
        if (symbolFont != null) { saveButton.Font = symbolFont; }
        saveButton.Click += SaveButton_Click;

        cancelButton = new Button
        {
            AutoSize = true,
            Margin = new Padding(0, 0, 5, 5),
            Visible = false,
            Text = symbolFont != null ? "\U0001F5D9" : "Cancel"
        };
        if (symbolFont != null) { cancelButton.Font = symbolFont; }
        cancelButton.Click += CancelButton_Click;

        ToolTip toolTip = new();
        toolTip.SetToolTip(editButton, "Edit");
        toolTip.SetToolTip(saveButton, "Save");
        toolTip.SetToolTip(cancelButton, "Cancel");

        buttonsPanel.Controls.Add(editButton);   // This will be on the far right.
        buttonsPanel.Controls.Add(cancelButton); // This will be to the left of the edit button.
        buttonsPanel.Controls.Add(saveButton);   // This will be to the left of the cancel button.

        contentTextBox = new TextBox
        {
            Dock = DockStyle.Fill,
            Multiline = true,
            ReadOnly = true,
            ScrollBars = ScrollBars.Vertical,
            Font = new Font("Consolas", 9.75F, FontStyle.Regular),
        };

        mainPanel.Controls.AddRange([buttonsPanel, contentTextBox]);
        Controls.Add(mainPanel);
    }

    private async void LoadContentAsync()
    {
        if (contentTextBox == null || editButton == null) return;

        contentTextBox.Text = string.Empty;
        _originalContent = string.Empty;
        editButton.Enabled = false;

        if (string.IsNullOrEmpty(_filePath))
        {
            return;
        }

        // The file might not exist, which is fine. The user can create it by editing.
        editButton.Enabled = true;

        if (File.Exists(_filePath))
        {
            try
            {
                _originalContent = await File.ReadAllTextAsync(_filePath);
                contentTextBox.Text = _originalContent;
            }
            catch (Exception ex)
            {
                AppLogger.Log($"Error reading text file '{_filePath}': {ex.Message}");
                contentTextBox.Text = $"Error loading content: {ex.Message}";
                editButton.Enabled = false;
            }
        }
    }

    public void EnterEditMode()
    {
        if (contentTextBox == null || editButton == null || saveButton == null || cancelButton == null) return;

        _originalContent = contentTextBox.Text;
        contentTextBox.ReadOnly = false;
        editButton.Visible = false;
        saveButton.Visible = true;
        cancelButton.Visible = true;
        contentTextBox.Focus();

        EditModeStarted?.Invoke(this, EventArgs.Empty);
    }

    public void CancelEditMode(bool suppressEvent = false)
    {
        if (contentTextBox == null || editButton == null || saveButton == null || cancelButton == null) return;

        contentTextBox.ReadOnly = true;
        editButton.Visible = true;
        saveButton.Visible = false;
        cancelButton.Visible = false;
        contentTextBox.Text = _originalContent;

        if (!suppressEvent)
        {
            EditModeEnded?.Invoke(this, EventArgs.Empty);
        }
    }

    private void EditButton_Click(object? sender, EventArgs e)
    {
        EnterEditMode();
    }

    private async void SaveButton_Click(object? sender, EventArgs e)
    {
        if (contentTextBox == null || editButton == null || saveButton == null || cancelButton == null || string.IsNullOrEmpty(_filePath)) return;

        // Revert UI state first
        contentTextBox.ReadOnly = true;
        editButton.Visible = true;
        saveButton.Visible = false;
        cancelButton.Visible = false;

        string newContent = contentTextBox.Text;

        try
        {
            // Only write if content has changed or if the file doesn't exist and there's new content.
            bool fileExists = File.Exists(_filePath);
            if (newContent != _originalContent || (!fileExists && !string.IsNullOrEmpty(newContent)))
            {
                // Ensure the directory exists before writing the file.
                string? directory = Path.GetDirectoryName(_filePath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }
                await File.WriteAllTextAsync(_filePath, newContent);
                _originalContent = newContent; // Update original content on successful save
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, $"Failed to save file '{Path.GetFileName(_filePath)}': {ex.Message}", "Save Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            contentTextBox.Text = _originalContent; // Revert textbox to original content on error
        }
        finally
        {
            EditModeEnded?.Invoke(this, EventArgs.Empty);
        }
    }

    private void CancelButton_Click(object? sender, EventArgs e)
    {
        CancelEditMode();
    }

    public void Clear()
    {
        if (IsEditing)
        {
            CancelEditMode(suppressEvent: true);
        }
        if (contentTextBox != null)
        {
            contentTextBox.Text = string.Empty;
        }
        _filePath = null;
        _originalContent = string.Empty;
        if (editButton != null)
        {
            editButton.Enabled = false;
        }
    }

    public void HandleKeyDown(KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Escape && IsEditing)
        {
            CancelEditMode();
            e.SuppressKeyPress = true;
        }
    }
}
