namespace DOSGameCollection.UI;

public class BoxArtCarouselManager : IDisposable
{
    private readonly PictureBox _pictureBox;
    private readonly Label _imageNameLabel;
    private readonly Button _previousButton;
    private readonly Button _nextButton;

    private List<string> _mediaPaths = [];
    private int _currentIndex = -1;
    private static readonly string[] VideoExtensions = [".avi", ".mp4", ".mpg", ".mkv"];

    public BoxArtCarouselManager(PictureBox pictureBox, Label imageNameLabel, Button previousButton, Button nextButton)
    {
        _pictureBox = pictureBox;
        _imageNameLabel = imageNameLabel;
        _previousButton = previousButton;
        _nextButton = nextButton;
    }

    public void LoadImages(List<string> paths)
    {
        Clear();
        _mediaPaths = paths;
        if (_mediaPaths.Any())
        {
            _currentIndex = 0;
            DisplayCurrentMedia();
        }
        UpdateControls();
    }

public void GoToNext()
    {
        if (_currentIndex < _mediaPaths.Count - 1)
        {
            _currentIndex++;
            DisplayCurrentMedia();
            UpdateControls();
        }
    }

    public void GoToPrevious()
    {
        if (_currentIndex > 0)
        {
            _currentIndex--;
            DisplayCurrentMedia();
            UpdateControls();
        }
    }

    private void DisplayCurrentMedia()
    {
        if (_currentIndex < 0 || _currentIndex >= _mediaPaths.Count) return;

        string currentPath = _mediaPaths[_currentIndex];
        _imageNameLabel.Text = Path.GetFileName(currentPath);

        _pictureBox.Visible = true;
        _pictureBox.Image?.Dispose();
        _pictureBox.Image = null;

        string extension = Path.GetExtension(currentPath);
        if (!VideoExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase))
        {
            // It's an image, so try to load it.
            try
            {
                _pictureBox.Image = Image.FromFile(currentPath);
            }
            catch (Exception ex)
            {
                AppLogger.Log($"Error loading carousel image '{currentPath}': {ex.Message}");
            }
        }
        // If it's a video, the picture box remains visible but empty.
    }

    private void UpdateControls()
    {
        _previousButton.Enabled = _currentIndex > 0;
        _nextButton.Enabled = _currentIndex < _mediaPaths.Count - 1;
        _imageNameLabel.Visible = _mediaPaths.Any();
    }

    public void Clear()
    {
        _pictureBox.Visible = false;
        _pictureBox.Image?.Dispose();
        _pictureBox.Image = null;
        _imageNameLabel.Text = "";
        _mediaPaths.Clear();
        _currentIndex = -1;
        UpdateControls();
    }

    public void Dispose()
    {
        _pictureBox.Image?.Dispose();
    }
}
