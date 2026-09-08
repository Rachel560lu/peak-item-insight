using System;
using System.Diagnostics;
using System.IO;
using BepInEx;

namespace PeakItemInsight.Diagnostics;

internal static class SessionTrace
{
    private static readonly int Pid = Process.GetCurrentProcess().Id;
    private static string? _path;
    public static string ScreenshotPath => Path.ChangeExtension(_path, ".png");
    public static void Start()
    {
        var directory = Path.Combine(Paths.BepInExRootPath, "InsightDiagnostics");
        Directory.CreateDirectory(directory);
        _path = Path.Combine(directory, $"session-{Pid}-{DateTime.UtcNow:yyyyMMddTHHmmssfff}.log");
        Write("START", $"version={Plugin.PluginVersion} mvid={typeof(Plugin).Module.ModuleVersionId} assembly={typeof(Plugin).Assembly.Location} processStart={Process.GetCurrentProcess().StartTime.ToUniversalTime():O}");
    }

    public static void Write(string stage, string message = "")
    {
        if (_path == null) return;
        try { File.AppendAllText(_path, $"{DateTime.UtcNow:O} pid={Pid} {stage} {message}{Environment.NewLine}"); }
        catch (IOException) { /* Diagnostics must not interrupt gameplay. */ }
        catch (UnauthorizedAccessException) { }
    }
}
