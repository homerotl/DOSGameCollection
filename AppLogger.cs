using System;
using System.Collections.Generic;

namespace DOSGameCollection;

public static class AppLogger
{
    private static readonly List<string> _logMessages = new();
    private static readonly object _lock = new();

    public static void Log(string message)
    {
        lock (_lock)
        {
            _logMessages.Add($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}");
        }
    }

    public static string GetAllLogs()
    {
        lock (_lock)
        {
            return string.Join(Environment.NewLine, _logMessages);
        }
    }
}

