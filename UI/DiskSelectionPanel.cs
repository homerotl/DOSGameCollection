using System.ComponentModel;
using DOSGameCollection.Services;

namespace DOSGameCollection.UI;

public class DiskSelectionPanel : UserControl
{
    private readonly ListBox _listBox;
    private readonly Button _addButton;
    private readonly Button _moveUpButton;
    private readonly Button _moveDownButton;
    private readonly Button _deleteButton;

    private string _addButtonText = "Add...";

    // Public properties for customization
    [Category("Behavior"), Description("The title of the file open dialog.")]
    public string AddDialogTitle { get; set; } = "Select Files";

    private bool ShouldSerializeAddDialogTitle() => AddDialogTitle != "Select Files";
    private void ResetAddDialogTitle() => AddDialogTitle = "Select Files";

    [Category("Behavior"), Description("The text displayed on the 'Add' button.")]
    public string AddButtonText
    {
        get => _addButtonText;
        set
        {
            _addButtonText = value;
            if (_addButton != null)
            {
                _addButton.Text = value;
            }
        }
    }

    private bool ShouldSerializeAddButtonText() => _addButtonText != "Add...";
    private void ResetAddButtonText() => AddButtonText = "Add...";

    [Category("Behavior"), Description("The file filter for the file open dialog.")]
    public string FileFilter { get; set; } = "All files (*.*)|*.*";

    private bool ShouldSerializeFileFilter() => FileFilter != "All files (*.*)|*.*";
    private void ResetFileFilter() => FileFilter = "All files (*.*)|*.*";

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public IEnumerable<string> SelectedFilePaths => _listBox.Items.Cast<FileListItem>().Select(i => i.FilePath);

    // Event to notify parent when the list changes (for validation purposes)
    public event EventHandler? ListChanged;

    public DiskSelectionPanel()
    {
        _listBox = new ListBox();
        _addButton = new Button();
        _moveUpButton = new Button();
        _moveDownButton = new Button();
        _deleteButton = new Button();
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        // --- Main Content (ListBox and order buttons) ---
        var contentPanel = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            ColumnCount = 2,
            //BackColor = Color.Red, // For debugging layout
            AutoSize = true,
            RowCount = 1
        };
        contentPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        contentPanel.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));

        _listBox.Dock = DockStyle.Top;
        _listBox.SelectionMode = SelectionMode.One;
        _listBox.DisplayMember = "FileName";
        _listBox.SelectedIndexChanged += ListBox_SelectedIndexChanged;
        _listBox.Height = 120;

        var orderButtonsPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Top,
            FlowDirection = FlowDirection.TopDown,
            AutoSize = true,
            //BackColor = Color.Yellow,
            Margin = new Padding(5, 0, 0, 0)
        };

        _moveUpButton.Text = "▲";
        _moveUpButton.Enabled = false;
        _moveUpButton.Size = new Size(30, 30);
        _moveUpButton.Font = new Font("Segoe UI", 9F);
        _moveUpButton.Click += MoveUpButton_Click;

        _moveDownButton.Text = "▼";
        _moveDownButton.Enabled = false;
        _moveDownButton.Size = new Size(30, 30);
        _moveDownButton.Font = new Font("Segoe UI", 9F);
        _moveDownButton.Click += MoveDownButton_Click;

        _deleteButton.Text = "X";
        _deleteButton.Enabled = false;
        _deleteButton.Size = new Size(30, 30);
        _deleteButton.ForeColor = Color.Red;
        _deleteButton.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        _deleteButton.Click += DeleteButton_Click;

        orderButtonsPanel.Controls.AddRange([_moveUpButton, _moveDownButton, _deleteButton]);

        contentPanel.Controls.Add(_listBox, 0, 0);
        contentPanel.Controls.Add(orderButtonsPanel, 1, 0);

        // --- Add Button ---
        _addButton.AutoSize = true;
        _addButton.Anchor = AnchorStyles.Left;
        _addButton.Margin = new Padding(0, 5, 0, 5);
        _addButton.Click += AddButton_Click;

        // --- Main Layout ---
        var mainLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            ColumnCount = 1,
            //BackColor = Color.Blue, // For debugging layout
            RowCount = 2,
            AutoSize = true
        };
        mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        mainLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        mainLayout.Controls.Add(contentPanel, 0, 0);
        mainLayout.Controls.Add(_addButton, 0, 1);

        Controls.Add(mainLayout);
    }

    private void AddButton_Click(object? sender, EventArgs e)
    {
        using var openFileDialog = new OpenFileDialog
        {
            Title = AddDialogTitle,
            Filter = FileFilter,
            Multiselect = true
        };

        // Pre-select the last used source path if it's valid.
        if (!string.IsNullOrEmpty(AppConfigService.LastNewGameSourcePath) && Directory.Exists(AppConfigService.LastNewGameSourcePath))
        {
            openFileDialog.InitialDirectory = AppConfigService.LastNewGameSourcePath;
        }

        if (openFileDialog.ShowDialog(this) == DialogResult.OK)
        {
            var currentItems = _listBox.Items.Cast<FileListItem>().Select(i => i.FilePath).ToHashSet(StringComparer.OrdinalIgnoreCase);
            var newItems = openFileDialog.FileNames
                .Where(f => !currentItems.Contains(f))
                .Select(f => new FileListItem(f));
            _listBox.Items.AddRange([.. newItems]);
            ListChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    private void ListBox_SelectedIndexChanged(object? sender, EventArgs e)
    {
        int selectedIndex = _listBox.SelectedIndex;
        bool isItemSelected = selectedIndex != -1;
        _deleteButton.Enabled = isItemSelected;
        _moveUpButton.Enabled = isItemSelected && selectedIndex > 0;
        _moveDownButton.Enabled = isItemSelected && selectedIndex < _listBox.Items.Count - 1;
    }

    private void MoveUpButton_Click(object? sender, EventArgs e) { MoveItem(-1); }
    private void MoveDownButton_Click(object? sender, EventArgs e) { MoveItem(1); }
    private void DeleteButton_Click(object? sender, EventArgs e)
    {
        if (_listBox.SelectedIndex != -1)
        {
            _listBox.Items.RemoveAt(_listBox.SelectedIndex);
            ListChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    private void MoveItem(int direction)
    {
        int selectedIndex = _listBox.SelectedIndex;
        if (selectedIndex < 0) return;
        int newIndex = selectedIndex + direction;
        if (newIndex < 0 || newIndex >= _listBox.Items.Count) return;
        object item = _listBox.SelectedItem;
        _listBox.Items.RemoveAt(selectedIndex);
        _listBox.Items.Insert(newIndex, item);
        _listBox.SelectedIndex = newIndex;
    }

    private record FileListItem(string FilePath)
    {
        public string FileName => Path.GetFileName(FilePath);
    }
}