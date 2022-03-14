namespace Bbob.Plugin;

public class PluginJson
{
    private string? _name;

    /// <summary>
    /// Name of plugin.
    /// </summary>
    /// <value></value>
    public string name
    {
        get => _name ?? throw new NullReferenceException("Plugin name is null");
        set => _name = value;
    }

    /// <summary>
    /// Description of plugin.
    /// </summary>
    /// <value></value>
    public string description { get; set; }

    /// <summary>
    /// Author of plugin.
    /// </summary>
    /// <value></value>
    public string author { get; set; }

    /// <summary>
    /// Repository url of plugin.
    /// </summary>
    /// <value></value>
    public string? repository { get; set; }

    /// <summary>
    /// Entry class library of plugin.
    /// </summary>
    /// <value></value>
    public string entry { get; set; }
    public PluginJson()
    {
        entry = "MainPlugin.dll";
        description = "No description...";
        author = "Unknown author";
    }
}