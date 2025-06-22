using LibVLCSharp.Shared;
using LibVLCSharp.WinForms;

namespace DOSGameCollection.UI;

public class BoxArtCarouselManager : IDisposable
{
    private readonly PictureBox _pictureBox;
    private readonly VideoView _videoView;
    private readonly MediaPlayer _mediaPlayer;
    private readonly LibVLC _libVLC;
    private readonly Label _imageNameLabel;
    private readonly Button _previousButton;
    private readonly Button _nextButton;

    private List<string> _mediaPaths = [];
    private int _currentIndex = -1;

    public BoxArtCarouselManager(PictureBox pictureBox, VideoView videoView, MediaPlayer mediaPlayer, LibVLC libVLC, Label imageNameLabel, Button previousButton, Button nextButton)
    {
        _pictureBox = pictureBox;
        _videoView = videoView;
        _mediaPlayer = mediaPlayer;
        _libVLC = libVLC;
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

        if (Path.GetExtension(currentPath).Equals(".avi", StringComparison.OrdinalIgnoreCase))
        {
            _pictureBox.Visible = false;
            _pictureBox.Image?.Dispose();
            _pictureBox.Image = null;

            _videoView.Visible = true;
            
            using var media = new Media(_libVLC, new Uri(currentPath));
            _mediaPlayer.Play(media);
            _mediaPlayer.EndReached += MediaPlayer_EndReached;
        }
        else
        {
            _mediaPlayer.Stop();
            _mediaPlayer.EndReached -= MediaPlayer_EndReached;
            _videoView.Visible = false;
            
            _pictureBox.Visible = true;
            _pictureBox.Image?.Dispose();
            _pictureBox.Image = Image.FromFile(currentPath);
        }
    }

    private void MediaPlayer_EndReached(object? sender, EventArgs e) => _mediaPlayer?.Play();

    private void UpdateControls()
    {
        _previousButton.Enabled = _currentIndex > 0;
        _nextButton.Enabled = _currentIndex < _mediaPaths.Count - 1;
        _imageNameLabel.Visible = _mediaPaths.Any();
    }

    public void Clear()
    {
        _mediaPlayer.Stop();
        _mediaPlayer.EndReached -= MediaPlayer_EndReached;
        _videoView.Visible = false;
        _pictureBox.Visible = true;
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
        if (_mediaPlayer != null) _mediaPlayer.EndReached -= MediaPlayer_EndReached;
    }
}

