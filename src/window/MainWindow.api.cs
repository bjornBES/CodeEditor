
using System.ComponentModel;
using System.Reactive;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using NativeFileDialogSharp;
using ReactiveUI;
using Tmds.DBus.Protocol;
using lib.debug;

public partial class MainWindow : Window
{
    string OpenFileDialog()
    {
        string path = OpenDialog(DialogType.OpenFile);
        if (path == null)
        {
            return null;
        }
        CodeEditor.LoadFile(path);
        return path;
    }
    void SaveFile()
    {
        CodeEditor.SaveFile();
    }
    void SaveFileAs()
    {
        string path = OpenDialog(DialogType.OpenFile);
        if (path == null)
        {
            return;
        }
        CodeEditor.SaveFile(path);
    }
    void IndentDocument()
    {
        CodeEditor.IndentDocument();
    }
}