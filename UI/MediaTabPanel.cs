using System.Diagnostics;
using DOSGameCollection.Services;

namespace DOSGameCollection.UI;

public class MediaTabPanel : UserControl
{
    public record MediaItem(string FilePath, string DisplayName, MediaType Type);
    public enum MediaType { Image, Video, Pdf, Audio, Other }

    private DataGridView _mediaDataGridView;
    private PictureBox _mediaDisplayPictureBox;
    private Label _previewNotAvailableLabel;
    private string? _permanentCoverPath;
    public event Action<string, string>? DisplayNameUpdated;

    public MediaTabPanel()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        // --- Layout Panel for Media Tab ---
        TableLayoutPanel mediaTabPanel = new()
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1
        };
        mediaTabPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
        mediaTabPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
        mediaTabPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

        // --- DataGridView for Media List ---
        _mediaDataGridView = new DataGridView
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
            ReadOnly = false,
            MultiSelect = false
        };

        var symbolFont = FormatTools.GetSymbolFont(10F);

        // Column 1: Type (Image/Video Icon)
        var mediaTypeColumn = new DataGridViewTextBoxColumn { HeaderText = "Type", Name = "Type", AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells, ReadOnly = true };
        if (symbolFont != null)
        {
            mediaTypeColumn.DefaultCellStyle.Font = symbolFont;
        }
        _mediaDataGridView.Columns.Add(mediaTypeColumn);
        // Column 2: Name
        _mediaDataGridView.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Name", Name = "Name", FillWeight = 100, ReadOnly = false });
        // Column 3: Link (Clickable)
        var linkColumn = new DataGridViewTextBoxColumn { HeaderText = "Link", Name = "Link", AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells, ReadOnly = true };
        if (symbolFont != null)
        {
            linkColumn.DefaultCellStyle.Font = symbolFont;
        }
        _mediaDataGridView.Columns.Add(linkColumn);

        _mediaDataGridView.SelectionChanged += MediaDataGridView_SelectionChanged;
        _mediaDataGridView.CellClick += MediaDataGridView_CellClick;
        _mediaDataGridView.CellMouseEnter += MediaDataGridView_CellMouseEnter;
        _mediaDataGridView.CellMouseLeave += MediaDataGridView_CellMouseLeave;
        _mediaDataGridView.CellValueChanged += MediaDataGridView_CellValueChanged;

        // --- Panel for PictureBox ---
        Panel mediaDisplayPanel = new() { Dock = DockStyle.Fill };
        _mediaDisplayPictureBox = new PictureBox
        {
            Dock = DockStyle.Fill,
            SizeMode = PictureBoxSizeMode.Zoom,
            BorderStyle = BorderStyle.FixedSingle,
            BackColor = Color.Black
        };

        _previewNotAvailableLabel = new Label
        {
            Text = "Preview not available",
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleCenter,
            Visible = false,
            ForeColor = SystemColors.GrayText,
            BackColor = Color.Black
        };

        // Add the label first so it's behind the PictureBox
        mediaDisplayPanel.Controls.Add(_previewNotAvailableLabel);
        mediaDisplayPanel.Controls.Add(_mediaDisplayPictureBox);

        mediaTabPanel.Controls.Add(_mediaDataGridView, 0, 0);
        mediaTabPanel.Controls.Add(mediaDisplayPanel, 1, 0);
        Controls.Add(mediaTabPanel);
    }

    public void Populate(IEnumerable<MediaItem> mediaItems, string? coverImagePath = null)
    {
        _mediaDataGridView.Rows.Clear();
        ClearDisplay();

        _permanentCoverPath = coverImagePath;

        // Display permanent cover if provided
        if (!string.IsNullOrEmpty(_permanentCoverPath) && File.Exists(_permanentCoverPath))
        {
            _mediaDisplayPictureBox.Visible = true;
            try
            {
                _mediaDisplayPictureBox.Image = Image.FromFile(_permanentCoverPath);
            }
            catch (Exception ex)
            {
                AppLogger.Log($"Error loading cover image '{_permanentCoverPath}': {ex.Message}");
            }
        }

        foreach (var item in mediaItems)
        {
            string typeDisplay = FormatTools.SegoeUiSymbolExists ? item.Type switch
            {
                MediaType.Image => "\U0001F5BC", // 🖼️
                MediaType.Video => "\U0001F39E", // 🎞️
                MediaType.Pdf => "\U0001F4C4", // 📄
                MediaType.Audio => "\U0001F3B5", // 🎵
                _ => "❓"
            } : item.Type.ToString();

            string linkSymbol = FormatTools.SegoeUiSymbolExists ? "\U0001F517" : "Open";

            var rowIndex = _mediaDataGridView.Rows.Add(typeDisplay, item.DisplayName, linkSymbol);
            var row = _mediaDataGridView.Rows[rowIndex];
            row.Tag = item;
            row.Cells[2].ToolTipText = "Open"; // Set tooltip for the link column
        }

        if (_mediaDataGridView.Rows.Count > 0)
        {
            _mediaDataGridView.ClearSelection();
            _mediaDataGridView.Rows[0].Selected = true;
        }
    }

    public void Clear()
    {
        _mediaDataGridView.Rows.Clear();
        ClearDisplay();
        _permanentCoverPath = null;
    }

    private void ClearDisplay()
    {
        _mediaDisplayPictureBox.Visible = false;
        _mediaDisplayPictureBox.Image?.Dispose();
        _mediaDisplayPictureBox.Image = null;
        _previewNotAvailableLabel.Visible = false;
    }

    private void MediaDataGridView_SelectionChanged(object? sender, EventArgs e)
    {
        // If a permanent cover is being displayed, don't change the image based on selection.
        if (!string.IsNullOrEmpty(_permanentCoverPath))
        {
            return;
        }

        ClearDisplay();

        if (_mediaDataGridView.SelectedRows.Count == 0 || _mediaDataGridView.SelectedRows[0].Tag is not MediaItem mediaItem) return;

        if (mediaItem.Type == MediaType.Image)
        {
            _mediaDisplayPictureBox.Visible = true;
            try
            {
                _mediaDisplayPictureBox.Image = Image.FromFile(mediaItem.FilePath);
            }
            catch (Exception ex)
            {
                AppLogger.Log($"Error loading media image '{mediaItem.FilePath}': {ex.Message}");
            }
        }
        else if (mediaItem.Type == MediaType.Pdf || mediaItem.Type == MediaType.Video)
        {
            _previewNotAvailableLabel.Visible = true;
        }
    }

    private void MediaDataGridView_CellClick(object? sender, DataGridViewCellEventArgs e)
    {
        if (e.RowIndex < 0 || e.ColumnIndex != 2 || _mediaDataGridView.Rows[e.RowIndex].Tag is not MediaItem mediaItem) return;

        if (!string.IsNullOrEmpty(mediaItem.FilePath) && File.Exists(mediaItem.FilePath))
        {
            try { Process.Start(new ProcessStartInfo(mediaItem.FilePath) { UseShellExecute = true }); }
            catch (Exception ex) { MessageBox.Show(this, $"Could not open media file '{mediaItem.FilePath}'.\nError: {ex.Message}", "Error Opening File", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        }
    }

    private void MediaDataGridView_CellMouseEnter(object? sender, DataGridViewCellEventArgs e) => _mediaDataGridView.Cursor = e.RowIndex >= 0 && e.ColumnIndex == 2 ? Cursors.Hand : Cursors.Default;

    private void MediaDataGridView_CellMouseLeave(object? sender, DataGridViewCellEventArgs e) => _mediaDataGridView.Cursor = Cursors.Default;
    
    private async void MediaDataGridView_CellValueChanged(object? sender, DataGridViewCellEventArgs e)
    {
        // We only care about changes in the "Name" column (index 1)
        if (e.RowIndex < 0 || e.ColumnIndex != 1)
        {
            return;
        }

        var row = _mediaDataGridView.Rows[e.RowIndex];
        if (row.Tag is not MediaItem mediaItem)
        {
            return;
        }

        string? newDisplayName = row.Cells[e.ColumnIndex].Value as string;
        string mediaFileName = Path.GetFileName(mediaItem.FilePath);

        // If the new name is blank, revert to the filename.
        if (string.IsNullOrWhiteSpace(newDisplayName))
        {
            newDisplayName = mediaFileName;
        }

        // If the name hasn't actually changed, do nothing.
        if (newDisplayName.Equals(mediaItem.DisplayName, StringComparison.Ordinal))
        {
            return;
        }

        await FileInfoWriterService.UpdateDisplayNameAsync(mediaItem.FilePath, newDisplayName);

        // Update the cell and tag to reflect the final name (e.g., if it was reverted to filename)
        row.Cells[e.ColumnIndex].Value = newDisplayName;
        row.Tag = mediaItem with { DisplayName = newDisplayName };
        DisplayNameUpdated?.Invoke(mediaItem.FilePath, newDisplayName);
    }
}
