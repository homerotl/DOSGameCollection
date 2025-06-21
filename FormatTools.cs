namespace DOSGameCollection;

public static class FormatTools
{
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

