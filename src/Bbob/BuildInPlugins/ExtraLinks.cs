using Bbob.Plugin;

namespace Bbob.Main.BuildInPlugin;

[PluginJson(name = "ExtraLinks", version = "1.0.0", author = "Jinker")]
public class ExtraLinks: IPlugin
{
    bool hasGenerate = false;
    public void GenerateCommand(string filePath, GenerationStage stage)
    {
        if (stage != GenerationStage.Initialize) return;
        hasGenerate = true;
        PluginHelper.registerObject("extraLinks", new Dictionary<string, string>());
    }

    public Action? CommandComplete(Commands cmd)
    {
        if (cmd != Commands.GenerateCommand || !hasGenerate) return null;
        Dictionary<string, string> extraLinks = PluginHelper.getRegisteredObjectNotNull<Dictionary<string, string>>("extraLinks");
        List<ExtraLink> e = new List<ExtraLink>();
        foreach (var item in extraLinks)
        {
            e.Add(new ExtraLink(item.Key, item.Value));
        }
        PluginHelper.registerMeta("extraLinks", e);

        return null;
    }

    private class ExtraLink
    {
        public string name{get;set;}
        public string address{get;set;}
        public ExtraLink(string n, string a) => (name, address) = (n, a);
    }
}