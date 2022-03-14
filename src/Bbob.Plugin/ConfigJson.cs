namespace Bbob.Plugin;
public class ConfigJson
{
    /// <summary>
    /// Name of theme to generate.
    /// </summary>
    /// <value>Default 'default'</value>
    public string theme { get; set; } = "";

    /// <summary>
    /// Author of blog.
    /// </summary>
    /// <value></value>
    public string author { get; set; } = "";

    /// <summary>
    /// Description of blog.
    /// </summary>
    /// <value></value>
    public string description { get; set; } = "";

    /// <summary>
    /// About message of blog.
    /// </summary>
    /// <value></value>
    public string about { get; set; } = "";

    /// <summary>
    /// Name of blog.
    /// </summary>
    /// <value></value>
    public string blogName { get; set; } = "";

    /// <summary>
    /// blog article count one time of js api.
    /// </summary>
    /// <value>Default '10'.</value>
    public int blogCountOneTime { get; set; }

    /// <summary>
    /// Mode of articles how to generate. 'current' will generate all link to .js file. 'next' will generate all link to json file, .js file will not contain link.
    /// </summary>
    /// <value>Default is 'false'. Default generate.</value>
    public string allLink { get; set; } = "";

    /// <summary>
    /// Enable recursion to process articles or not.
    /// </summary>
    /// <value>Default true.</value>
    public bool recursion { get; set; }

    /// <summary>
    /// Domain of blog.
    /// </summary>
    /// <value>Default empty.</value>
    public string domain {get;set;} = "";

    /// <summary>
    /// Base url of blog.
    /// </summary>
    /// <value>Default '/'</value>
    public string baseUrl { get; set; } = "";

    /// <summary>
    /// Port of preview blog.
    /// </summary>
    /// <value>Default '3000'</value>
    public int previewPort { get; set; }

    /// <summary>
    /// Enable compress file or not.
    /// </summary>
    /// <value>Default true.</value>
    public bool compress {get;set;} = true;

    /// <summary>
    /// Use hash name of file or not.
    /// </summary>
    /// <value>Default true.</value>
    public bool useHashName {get;set;} = true;

    /// <summary>
    /// Distribution of command to process.
    /// </summary>
    /// <value>Default './dist/'</value>
    public string distributionPath {get;set;} = "";

    /// <summary>
    /// Plugin disable list.
    /// </summary>
    /// <typeparam name="string"></typeparam>
    /// <returns></returns>
    public List<string> pluginsDisable { get; set; } = new List<string>();

    /// <summary>
    /// Check target plugin is enable or not.
    /// </summary>
    /// <param name="pluginName">Name of target plugin</param>
    /// <returns>True if enable, otherwise false.</returns>
    public bool isPluginEnable(string pluginName) => isPluginEnable(pluginName, out int i);

    /// <summary>
    /// Check target plugin is enable or not, and get index of plugin disable list.
    /// </summary>
    /// <param name="pluginName">Name of target plugin</param>
    /// <param name="index">Index of target plugin in plugin disable list.</param>
    /// <returns></returns>
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

    /// <summary>
    /// Check target plugin is enable or not.
    /// </summary>
    /// <param name="pluginInfo">Information of target plugin.</param>
    /// <returns></returns>
    public bool isPluginEnable(PluginJson pluginInfo)
    {
        return isPluginEnable(pluginInfo.name);
    }
}