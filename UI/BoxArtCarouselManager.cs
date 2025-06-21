using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace DOSGameCollection.UI;

public class BoxArtCarouselManager : IDisposable
{
    private readonly PictureBox _pictureBox;
    private readonly Label _imageNameLabel;
    private readonly Button _previousButton;
    private readonly Button _nextButton;

    private List<string> _imagePaths = [];
    private int _currentIndex = -1;

    public BoxArtCarouselManager(PictureBox pictureBox, Label imageNameLabel, Button previousButton, Button nextButton)
    {
        _pictureBox = pictureBox;
        _imageNameLabel = imageNameLabel;
        _previousButton = previousButton;
        _nextButton = nextButton;
    }

    public void LoadImages(List<string> imagePaths)
    {
        _imagePaths = imagePaths ?? [];
        _currentIndex = _imagePaths.Any() ? 0 : -1;
        UpdateDisplay();
    }

    public void Clear()
    {
        _imagePaths.Clear();
        _currentIndex = -1;
        UpdateDisplay();
    }

    public void GoToPrevious()
    {
        if (_currentIndex > 0)
        {
            _currentIndex--;
            UpdateDisplay();
        }
    }

    public void GoToNext()
    {
        if (_currentIndex < _imagePaths.Count - 1)
        {
            _currentIndex++;
            UpdateDisplay();
        }
    }

    private void UpdateDisplay()
    {
        _pictureBox.Image?.Dispose();
        _pictureBox.Image = null;

        if (_currentIndex < 0 || _currentIndex >= _imagePaths.Count)
        {
            _imageNameLabel.Text = "";
            _previousButton.Enabled = false;
            _nextButton.Enabled = false;
            return;
        }

        string imagePath = _imagePaths[_currentIndex];
        _imageNameLabel.Text = Path.GetFileName(imagePath);
        try { _pictureBox.Image = Image.FromFile(imagePath); }
        catch (Exception ex) { Console.WriteLine($"Error loading box art '{imagePath}': {ex.Message}"); _imageNameLabel.Text = "Error loading image"; }

        _previousButton.Enabled = _currentIndex > 0;
        _nextButton.Enabled = _currentIndex < _imagePaths.Count - 1;
    }

    public void Dispose()
    {
        _pictureBox.Image?.Dispose();
        GC.SuppressFinalize(this);
    }
}

