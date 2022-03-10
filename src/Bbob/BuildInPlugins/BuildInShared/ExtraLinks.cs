using Bbob.Plugin;

namespace Bbob.Main.BuildInPlugin;

public class ExtraLinks: IPlugin
{
    public void GenerateCommand(string filePath, GenerationStage stage)
    {
        if (stage != GenerationStage.Initialize) return;

        PluginHelper.registerObject("extraLinks", new Dictionary<string, string>());
    }

    public void CommandComplete(Commands cmd)
    {
        if (cmd != Commands.GenerateCommand) return;
        Dictionary<string, string> extraLinks = PluginHelper.getRegisteredObjectNoNull<Dictionary<string, string>>("extraLinks");
        List<ExtraLink> e = new List<ExtraLink>();
        foreach (var item in extraLinks)
        {
            e.Add(new ExtraLink(item.Key, item.Value));
        }
        PluginHelper.registerMeta("extraLinks", e);
    }

    private class ExtraLink
    {
        public string name{get;set;}
        public string address{get;set;}
        public ExtraLink(string n, string a) => (name, address) = (n, a);
    }
}