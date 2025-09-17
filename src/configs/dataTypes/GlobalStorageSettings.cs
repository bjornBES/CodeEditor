
public class GlobalStorageSettings : Settings<GlobalStorageSettings>
{
    public string DefaultTheme { get; set; }
    public List<string> RecentWorkspaces = new List<string>();
    public List<string> RecentFolders = new List<string>();
    public List<string> RecentFiles = new List<string>();
}