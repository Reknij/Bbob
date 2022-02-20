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
                PluginHelper.modifyRegisteredObject<dynamic>("article", (ref dynamic? article) =>
                {
                    if (article == null) return;
                    PluginHelper.getPluginJsonConfig<jsonObject>("TestPlugin2", out var jo);
                    if (jo == null)
                    {
                        PluginHelper.printConsole("plugin config is null.");
                        return;
                    }
                    string n = $"{article.newClassName}-{article.msg}-{jo.append}-{jo.year}";

                    article.contentParsed = article.contentParsed.Replace("bbob", n);
                });
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
