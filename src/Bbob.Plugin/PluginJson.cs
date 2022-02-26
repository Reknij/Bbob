namespace Bbob.Plugin;

public class PluginJson
{
    private string? _name;
    public string name
    {
        get => _name ?? throw new NullReferenceException("Plugin name is null");
        set => _name = value;
    }
    public string description { get; set; }
    public string author { get; set; }
    public string? repository { get; set; }
    public string entry { get; set; }
    public PluginJson()
    {
        entry = "MainPlugin.dll";
        description = "No description...";
        author = "Unknown author";
    }
}