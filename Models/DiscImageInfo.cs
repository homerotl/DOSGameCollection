namespace DOSGameCollection.Models;

public class DiscImageInfo
{
    public required string ImgFileName { get; set; }
    public string? PngFilePath { get; set; } // Full path to the .png file
    public string? DisplayName { get; set; } // Optional friendly nam
// e for display

    public override string ToString()
    {
        // If DisplayName is available, use it; otherwise, fall back to the filename.
        return !string.IsNullOrEmpty(DisplayName) ? DisplayName : ImgFileName;
    }
}