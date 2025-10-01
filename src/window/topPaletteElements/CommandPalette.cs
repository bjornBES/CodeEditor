
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;

public class CommandPalette : TopPaletteElement
{
    public override string ElementName { get; protected set; }
    public List<CommandEntry> CommandEntries{ get; set; }

    StackPanel mainPanel;

    StackPanel commandList;

    TextBox inputBox;
    const int MaxCommandCount = 15;

    public CommandPalette()
    {
        CommandEntries = new List<CommandEntry>();
        ElementName = "cmd";

        InitializeComponent();
    }

    public void InitializeComponent()
    {
        Background = Brushes.Black;

        mainPanel = new StackPanel()
        {
            Orientation = Orientation.Vertical,
        };

        commandList = new StackPanel()
        {
            Orientation = Orientation.Vertical,
        };

        inputBox = new TextBox();
        mainPanel.Children.Add(inputBox);
        mainPanel.Children.Add(commandList);
        Children.Add(mainPanel);
    }

    public override void OpenElement()
    {
        CommandEntries.Clear();
        commandList.Children.Clear();
        inputBox.Focus();

        List<CommandEntry> entries = CommandManager.commandEntries;
        foreach (CommandEntry entry in entries)
        {
            Button button = new Button()
            {
                Content = entry.DisplayName,
                Padding = new Thickness(0, 0)
            };
            button.Click += (s, e) =>
            {
                CommandManager.ExecuteCommandGetArgs(entry.CommandId);
                ClosePalette.Invoke();
            };
            commandList.Children.Add(button);
            CommandEntries.Add(entry);
            if (CommandEntries.Count == MaxCommandCount)
            {
                break;
            }
        }
    }
}