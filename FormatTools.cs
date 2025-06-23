namespace DOSGameCollection;

using System.Drawing;
using System.Collections.Generic;
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

    private static readonly Dictionary<string, string> RatingDisplayToFileMap = new()
    {
        { "", "" }, // Handle empty selection
        { "E", "E" },
        { "E 10+", "E10" },
        { "T", "T" },
        { "M 17+", "M17" },
        { "AO 18+", "AO18" },
        { "RP", "RP" },
        { "RP LM 17+", "RPLM17" }
    };

    private static readonly Dictionary<string, string> RatingFileToDisplayMap =
        RatingDisplayToFileMap.ToDictionary(kvp => kvp.Value, kvp => kvp.Key, StringComparer.OrdinalIgnoreCase);

    public static string? EncodeRating(string? displayValue)
    {
        if (displayValue == null) return null;
        return RatingDisplayToFileMap.TryGetValue(displayValue, out var fileValue) ? fileValue : null;
    }

    public static string? DecodeRating(string? fileValue)
    {
        if (fileValue == null) return null;
        return RatingFileToDisplayMap.TryGetValue(fileValue, out var displayValue) ? displayValue : null;
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
