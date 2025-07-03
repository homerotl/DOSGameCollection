namespace DOSGameCollection;

using System.Collections.Generic;
using System.Linq;
using System.Reflection;

public static class FormatTools
{

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

    public static Image? LoadImageFromResource(string resourceName)
    {
        Assembly assembly = Assembly.GetExecutingAssembly();
        using (Stream? imageStream = assembly.GetManifestResourceStream(resourceName))
        {
            if (imageStream != null)
            {
                return Image.FromStream(imageStream);
            }
        }
        AppLogger.Log($"Warning: Could not load embedded resource '{resourceName}'.");
        return null;
    }

    public static Icon? LoadIconFromResource(string resourceName)
    {
        Assembly assembly = Assembly.GetExecutingAssembly();
        using (Stream? iconStream = assembly.GetManifestResourceStream(resourceName))
        {
            if (iconStream != null)
            {
                return new Icon(iconStream);
            }
        }
        AppLogger.Log($"Warning: Could not load embedded resource '{resourceName}'.");
        return null;
    }
}
