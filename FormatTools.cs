namespace DOSGameCollection;

using System.Drawing;
using System.Linq;

public static class FormatTools
{
    private static string SYMBOL_FONT = "Segoe UI Symbol";

    private static readonly Lazy<bool> _segoeUiSymbolExists = new(() =>
    {
        // Check if symbol font is installed on the system.
        return FontFamily.Families.Any(f => f.Name.Equals(SYMBOL_FONT, StringComparison.OrdinalIgnoreCase));
    });

    public static bool SegoeUiSymbolExists => _segoeUiSymbolExists.Value;

    public static Font? GetSymbolFont(float size = 9F, FontStyle style = FontStyle.Regular)
    {
        if (SegoeUiSymbolExists)
        {
            return new Font(SYMBOL_FONT, size, style);
        }
        return null; // Return null if the font is not available
    }

    public static string FormatFileSize(long bytes)
    {
        if (bytes >= 1024 * 1024 * 1024) // Gigabytes
        {
            return $"{(double)bytes / (1024 * 1024 * 1024):F2} GB";
        }
        if (bytes >= 1024 * 1024) // Megabytes
        {
            return $"{(double)bytes / (1024 * 1024):F2} MB";
        }
        return bytes >= 1024 ? $"{bytes / 1024} KB" : $"{bytes} B"; // Kilobytes or Bytes
    }
}

