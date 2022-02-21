namespace Bbob.Plugin;

public class PluginJson
{
    public string? name{get;set;}
    public string description{get;set;}
    public string author{get;set;}
    public string? repository{get;set;}
    public string entry{get;set;}
    public PluginJson()
    {
        entry = "MainPlugin.dll";
        description = "No description...";
        author = "Unknown author";
    }
}