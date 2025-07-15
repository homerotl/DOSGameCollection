using DOSGameCollection.Models;
using DOSGameCollection.Services;

namespace DOSGameCollection.UI;

public class DiskImageTabPanel : UserControl
{
    private readonly DataGridView _dataGridView;
    private readonly PictureBox _pictureBox;
    private readonly ContextMenuStrip _pictureContextMenu;
    private readonly Label _imageNotAvailableLabel;
    private Image? _cdIcon;
    private Image? _floppyIcon;
    public event Action<string, string>? DisplayNameUpdated;
    public event Action<DiscImageInfo>? DiscImageUpdated;
    public event Action<string>? PictureDeleteRequested;

    public DiskImageTabPanel()
    {
        LoadIcons();

        // Main layout panel
        TableLayoutPanel mainPanel = new()
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1
        };

        mainPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
        mainPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
        mainPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

        // Initialize DataGridView
        _dataGridView = new DataGridView
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

        var typeColumn = new DataGridViewImageColumn { HeaderText = "Type", Name = "Type", AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells, ReadOnly = true, ImageLayout = DataGridViewImageCellLayout.Normal };
        _dataGridView.Columns.Add(typeColumn);

        // Column 2: Name (Editable)
        _dataGridView.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Name", Name = "Name", FillWeight = 100, ReadOnly = false });

        // Column 3: Size (Read-only) 
        _dataGridView.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Size", Name = "FileSize", AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells, ReadOnly = true, DefaultCellStyle = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleRight } });


        _dataGridView.SelectionChanged += DataGridView_SelectionChanged;
        _dataGridView.CellValueChanged += DataGridView_CellValueChanged;

        // --- Panel for PictureBox and Label ---
        Panel imageDisplayPanel = new()
        {
            Dock = DockStyle.Fill,
            Margin = new Padding(3)
        };

        // Initialize PictureBox
        _pictureBox = new PictureBox
        {
            Dock = DockStyle.Fill,
            SizeMode = PictureBoxSizeMode.Zoom,
            BorderStyle = BorderStyle.FixedSingle,
            BackColor = Color.Black
        };

        // --- Context Menu for PictureBox ---
        _pictureContextMenu = new ContextMenuStrip();
        var deleteMenuItem = new ToolStripMenuItem("Delete picture");
        deleteMenuItem.Click += DeletePictureMenuItem_Click;
        _pictureContextMenu.Items.Add(deleteMenuItem);

        _pictureBox.ContextMenuStrip = _pictureContextMenu;

        _imageNotAvailableLabel = new Label
        {
            Text = "Image not available.\nDrag and drop an image to include it.",
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleCenter,
            Visible = false,
            ForeColor = SystemColors.GrayText,
            BackColor = Color.Black
        };

        // Enable Drag and Drop on the panel containing the PictureBox
        imageDisplayPanel.AllowDrop = true;
        imageDisplayPanel.DragEnter += ImageDisplayPanel_DragEnter;
        imageDisplayPanel.DragDrop += ImageDisplayPanel_DragDrop;

        mainPanel.Controls.Add(_dataGridView, 0, 0);
        mainPanel.Controls.Add(imageDisplayPanel, 1, 0);
        imageDisplayPanel.Controls.Add(_imageNotAvailableLabel);
        imageDisplayPanel.Controls.Add(_pictureBox);

        Controls.Add(mainPanel);

    }

    public void Populate(IEnumerable<DiscImageInfo> discImages)
    {
        _dataGridView.Rows.Clear();

        foreach (var discInfo in discImages)
        {
            Image? typeIcon = GetIconForFile(discInfo.FilePath);

            if (typeIcon == null) continue;

            var rowIndex = _dataGridView.Rows.Add(
                typeIcon,
                discInfo.DisplayName,
                FormatTools.FormatFileSize(discInfo.FileSizeInBytes)
            );
            var row = _dataGridView.Rows[rowIndex];
            row.Tag = discInfo;
        }

        if (_dataGridView.Rows.Count > 0)
        {
            _dataGridView.Rows[0].Selected = true;
        }

        // Manually update the image for the initial selection. The SelectionChanged event
        // is not reliably triggered to update the UI when the control is not yet visible.
        UpdateImageForSelection();
    }

    public void Clear()
    {
        _dataGridView.Rows.Clear();
        _pictureBox.Image?.Dispose();
        _pictureBox.Image = null;
        _pictureBox.Visible = false;
        _imageNotAvailableLabel.Visible = false;
    }

    private void UpdateImageForSelection()
    {
        // Reset state
        _pictureBox.Image?.Dispose();
        _pictureBox.Image = null;
        _pictureBox.Visible = false;
        _imageNotAvailableLabel.Visible = false;

        if (_dataGridView.SelectedRows.Count > 0 && _dataGridView.SelectedRows[0].Tag is DiscImageInfo selectedDisc)
        {
            if (!string.IsNullOrEmpty(selectedDisc.PngFilePath) && File.Exists(selectedDisc.PngFilePath))
            {
                try
                {
                    // Load the image via a memory stream to avoid locking the file on disk.
                    // Then, create a new Bitmap from the stream-based image to allow the stream to be closed.
                    using var fileStream = new FileStream(selectedDisc.PngFilePath, FileMode.Open, FileAccess.Read);
                    using var memoryStream = new MemoryStream();
                    fileStream.CopyTo(memoryStream);
                    memoryStream.Position = 0; // Rewind the stream before reading
                    using var tempImage = Image.FromStream(memoryStream);
                    _pictureBox.Image = new Bitmap(tempImage);
                }
                catch (Exception ex)
                {
                    AppLogger.Log($"Error loading disc image picture '{selectedDisc.PngFilePath}': {ex.Message}");
                    _pictureBox.Image = null; // Ensure image is null on error
                }
            }

            if (_pictureBox.Image != null)
            {
                _pictureBox.Visible = true;
            }
            else
            {
                _imageNotAvailableLabel.Visible = true;
            }
        }
    }

    private void DataGridView_SelectionChanged(object? sender, EventArgs e)
    {
        UpdateImageForSelection();
    }

    private async void DataGridView_CellValueChanged(object? sender, DataGridViewCellEventArgs e)
    {
        // We only care about changes in the "Name" column (index 1)
        if (e.RowIndex < 0 || e.ColumnIndex != 1)
        {
            return;
        }

        var row = _dataGridView.Rows[e.RowIndex];
        if (row.Tag is not DiscImageInfo discInfo)
        {
            return;
        }

        string? newDisplayName = row.Cells[e.ColumnIndex].Value as string;
        string mediaFileName = Path.GetFileName(discInfo.FilePath);

        if (string.IsNullOrWhiteSpace(newDisplayName))
        {
            newDisplayName = mediaFileName;
        }

        if (newDisplayName.Equals(discInfo.DisplayName, StringComparison.Ordinal))
        {
            return;
        }

        await FileInfoWriterService.UpdateDisplayNameAsync(discInfo.FilePath, newDisplayName);

        row.Cells[e.ColumnIndex].Value = newDisplayName;
        row.Tag = discInfo with { DisplayName = newDisplayName };
        DisplayNameUpdated?.Invoke(discInfo.FilePath, newDisplayName);
    }

    private void LoadIcons()
    {
        _cdIcon = FormatTools.LoadImageFromResource("DOSGameCollection.Resources.icons.cd_20.png");
        _floppyIcon = FormatTools.LoadImageFromResource("DOSGameCollection.Resources.icons.floppy_20.png");
    }
    
     private Image? GetIconForFile(string filePath)
    {
        string extension = Path.GetExtension(filePath).ToLowerInvariant();
        return extension switch
        {
            ".iso" or ".cue" => _cdIcon,
            ".img" => _floppyIcon,
            _ => null // Or a default icon if you have one
        };
    }

    private void ImageDisplayPanel_DragEnter(object? sender, DragEventArgs e)
    {
        if (e.Data != null && e.Data.GetDataPresent(DataFormats.FileDrop))
        {
            var files = (string[]?)e.Data.GetData(DataFormats.FileDrop);
            if (files != null && files.Length == 1 && Path.GetExtension(files[0]).Equals(".png", StringComparison.OrdinalIgnoreCase))
            {
                e.Effect = DragDropEffects.Copy;
                return;
            }
        }
        e.Effect = DragDropEffects.None;
    }

    private void ImageDisplayPanel_DragDrop(object? sender, DragEventArgs e)
    {
        if (e.Data == null) return;

        var files = (string[]?)e.Data.GetData(DataFormats.FileDrop);
        if (files == null || files.Length != 1) return;

        string sourcePngPath = files[0];

        if (_dataGridView.SelectedRows.Count == 0 || _dataGridView.SelectedRows[0].Tag is not DiscImageInfo selectedDisc)
        {
            MessageBox.Show(this, "Please select a disk from the list before dragging an image.", "No Disk Selected", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        try
        {
            string discImageDirectory = Path.GetDirectoryName(selectedDisc.FilePath) ?? "";
            if (string.IsNullOrEmpty(discImageDirectory) || !Directory.Exists(discImageDirectory))
            {
                MessageBox.Show(this, "The directory for the selected disk image could not be found.", "Directory Not Found", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            string targetFileName = Path.ChangeExtension(Path.GetFileName(selectedDisc.FilePath), ".png");
            string targetPngPath = Path.Combine(discImageDirectory, targetFileName);

            File.Copy(sourcePngPath, targetPngPath, true);

            // Update the model in the DataGridView's Tag
            var updatedDiscInfo = selectedDisc with { PngFilePath = targetPngPath };
            _dataGridView.SelectedRows[0].Tag = updatedDiscInfo;

            // Refresh the PictureBox display using the new info in the Tag
            UpdateImageForSelection();

            // Notify the parent form (TopForm) to update its master list
            DiscImageUpdated?.Invoke(updatedDiscInfo);
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, $"Failed to copy the image file.\n\nError: {ex.Message}", "File Copy Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            AppLogger.Log($"Error during drag-drop image copy: {ex.Message}");
        }
    }

    private void DeletePictureMenuItem_Click(object? sender, EventArgs e)
    {
        // The context menu is on the PictureBox, but the relevant data is in the DataGridView's selected row.
        if (_dataGridView.SelectedRows.Count > 0 && _dataGridView.SelectedRows[0].Tag is DiscImageInfo selectedDisc)
        {
            if (!string.IsNullOrEmpty(selectedDisc.PngFilePath) && File.Exists(selectedDisc.PngFilePath))
            {
                PictureDeleteRequested?.Invoke(selectedDisc.PngFilePath);
            }
        }
    }
}