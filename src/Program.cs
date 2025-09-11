using System;
using Avalonia;

public class Program
{
    public const string AppName = "CodeEditorApp";
    public static int Main(string[] args)
    {
        DebugWriter.Clean();
        DebugWriter.Initialize(Console.Out, "log_Main_console");
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

