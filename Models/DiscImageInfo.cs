namespace DOSGameCollection.Models;

public class DiscImageInfo
{
    public required string ImgFileName { get; set; }
    public string? PngFilePath { get; set; } // Full path to the .png file

    public override string ToString()
    {
        return ImgFileName;
    }
}