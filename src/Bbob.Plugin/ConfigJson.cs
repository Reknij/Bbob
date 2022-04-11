namespace Bbob.Plugin;

/// <summary>
/// Object of config.
/// </summary>
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
    public string domain { get; set; } = "";

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
    public bool compress { get; set; } = true;

    /// <summary>
    /// Use hash name of file or not.
    /// </summary>
    /// <value>Default true.</value>
    public bool useHashName { get; set; } = true;

    /// <summary>
    /// Distribution of command to process.
    /// </summary>
    /// <value>Default './dist/'</value>
    public string distributionPath { get; set; } = "";

    /// <summary>
    /// Plugin disable list.
    /// </summary>
    /// <returns></returns>
    public HashSet<string> pluginsDisable { get; set; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Check target plugin is enable or not, and get index of plugin disable list.
    /// </summary>
    /// <param name="pluginName">Name of target plugin</param>
    /// <returns></returns>
    public bool isPluginEnable(string pluginName)
    {
        return !pluginsDisable.Contains(pluginName);
    }

    /// <summary>
    /// Check target plugin is enable or not.
    /// </summary>
    /// <param name="pluginInfo">Information of target plugin.</param>
    /// <returns></returns>
    public bool isPluginEnable(PluginJson pluginInfo) => isPluginEnable(pluginInfo.name);

    /// <summary>
    /// True if all build-in plugin is enable, otherwise false.
    /// </summary>
    /// <returns></returns>
    public bool isAllBuildInPluginEnable()
    {
        return !pluginsDisable.Contains("*B");
    }

    /// <summary>
    /// True if all third plugin is enable, otherwise false.
    /// </summary>
    /// <returns></returns>
    public bool isAllThirdPluginEnable()
    {
        return !pluginsDisable.Contains("*T");
    }
}