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
    public string baseUrl { get; set; } = "";
    public int previewPort { get; set; }

    public string[] buildInPlugins { get; set; } = Array.Empty<string>();
}