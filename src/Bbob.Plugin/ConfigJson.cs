namespace Bbob.Plugin;
public class ConfigJson
{
    public string theme { get; set; } = "";
    public string author { get; set; } = "";
    public string description { get; set; } = "";
    public string about { get; set; } = "";
    public string blogName { get; set; } = "";

    public int blogCountOneTime { get; set; }
    public string allLink { get; set; } = "";
    public bool recursion { get; set; }
    public string domain {get;set;} = "";
    public string baseUrl { get; set; } = "";
    public int previewPort { get; set; }
    public bool compress {get;set;} = true;

    public List<string> pluginsDisable { get; set; } = new List<string>();
    public bool isPluginEnable(string pluginName) => isPluginEnable(pluginName, out int i);
    public bool isPluginEnable(string pluginName, out int index)
    {
        index = -1;
        for (int i = 0; i < pluginsDisable.Count; i++)
        {
            if (pluginsDisable[i].ToUpper() == pluginName.ToUpper())
            {
                index = i;
                return false;
            }
        }
        return true;
    }
    public bool isPluginEnable(PluginJson pluginInfo)
    {
        return isPluginEnable(pluginInfo.name);
    }
}