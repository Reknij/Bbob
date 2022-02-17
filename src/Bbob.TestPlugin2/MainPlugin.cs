using Bbob.Plugin;

namespace Bbob.TestPlugin2;
public class MainPlugin : IPlugin
{
    public void GenerateCommand(string filePath, string distribution, GenerationStage stage)
    {
        switch (stage)
        {
            case GenerationStage.Process:
                PluginHelper.modifyRegisteredObject<string>("markdown", (ref string? markdown) =>
            {
                if (markdown == null) return;
                markdown = markdown.Replace("paragraph", "ppap");
            });
                break;
            case GenerationStage.FinalProcess:
                if (PluginHelper.getRegisteredObject<Func<Type, object>>("getYamlObject", out var getYamlObject) && getYamlObject != null)
                {
                    yamlObject yo = getYamlObject(typeof(yamlObject)) as yamlObject ?? throw new NullReferenceException();
                    PluginHelper.getPluginJsonConfig<jsonObject>("TestPlugin2", out var jo);
                    if (jo == null) throw new NullReferenceException();
                    string n = $"{yo.newClassName}-{yo.msg}-{jo.append}-{jo.year}";
                    PluginHelper.modifyRegisteredObject<string>("contentParsed", (ref string? html) =>
                    {
                        if (html == null) return;
                        html = html.Replace("bbob", n);
                    });

                }
                break;
            default:
                break;
        }
    }
    public void GenerateCommandPart2()
    {

    }

    public class yamlObject
    {
        public string? newClassName { get; set; }
        public string? msg { get; set; }
    }

    public class jsonObject
    {
        public string? append { get; set; }
        public int year { get; set; }
    }
}
