using DOSGameCollection.Models;

namespace DOSGameCollection.UI;

public class DiscImageTabPanel : TableLayoutPanel
{
    private readonly DataGridView _dataGridView;
    private readonly PictureBox _pictureBox;
    private readonly Label _imageNotAvailableLabel;

    public DiscImageTabPanel()
    {
        // Initialize the TableLayoutPanel
        Dock = DockStyle.Fill;
        ColumnCount = 2;
        RowCount = 1;
        ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
        ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
        RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

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
            ReadOnly = true,
            MultiSelect = false
        };
        _dataGridView.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Name", Name = "DisplayName", FillWeight = 70 });
        _dataGridView.Columns.Add(new DataGridViewTextBoxColumn
        {
            HeaderText = "Size",
            Name = "FileSize",
            AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
            DefaultCellStyle = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleRight }
        });
        _dataGridView.SelectionChanged += DataGridView_SelectionChanged;

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

        _imageNotAvailableLabel = new Label
        {
            Text = "Image not available",
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleCenter,
            Visible = false,
            ForeColor = SystemColors.GrayText,
            BackColor = Color.Black
        };

        Controls.Add(_dataGridView, 0, 0);
        Controls.Add(imageDisplayPanel, 1, 0);
        imageDisplayPanel.Controls.Add(_imageNotAvailableLabel);
        imageDisplayPanel.Controls.Add(_pictureBox);
    }

    public void Populate(IEnumerable<DiscImageInfo> discImages)
    {
        _dataGridView.Rows.Clear();

        foreach (var discInfo in discImages)
        {
            var rowIndex = _dataGridView.Rows.Add(discInfo.ToString(), FormatTools.FormatFileSize(discInfo.FileSizeInBytes));
            _dataGridView.Rows[rowIndex].Tag = discInfo;
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
                    _pictureBox.Image = Image.FromFile(selectedDisc.PngFilePath);
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
}