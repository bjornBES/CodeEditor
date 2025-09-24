using System;
using Avalonia;
using lib.debug;

public class Program
{
    public const string AppName = "CodeEditorApp";
    public static int Main(string[] args)
    {
        DebugWriter.Clean();
        DebugWriter.Initialize(Console.Out);
        DebugWriter.AddModule("Main", "log_Main_console", "Main");
        DebugWriter.AddModulesToLog("Main", "Commands", "KeybindingManager", "Window", "Explorer", "Top palette", "Editor");
        DebugWriter.AddModule("AvaloniaEdit", "log_AvaloniaEdit_console", "AvaloniaEdit");
        DebugWriter.WriteLine("Main", "Hello world");
        {
            DebugWriter.WriteLine("Main", "Hello");
        }
        return BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
    }
    static AppBuilder BuildAvaloniaApp() => AppBuilder.Configure<App>()
    .UsePlatformDetect()
    .LogToTrace();
}

