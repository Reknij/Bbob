using Bbob.Plugin;

namespace Bbob.TestPlugin1;

public class MainPlugin : IPlugin
{

    public void GenerateCommand(string filePath, string distribution, GenerationStage stage)
    {
        switch (stage)
        {
            case GenerationStage.Process:
                PluginHelper.modifyRegisteredObject<string>("markdown", (ref string? value) =>
                        {
                            if (value == null) return;
                            value = value.Replace("title", "ttat");

                        });
                PluginHelper.registerObject("injectData", "HelloWorld!");
                break;
            case GenerationStage.FinalProcess:
                PluginHelper.modifyRegisteredObject<string>("contentParsed", (ref string? value) =>
                        {
                            if (value == null) return;
                            value = value.Replace("class", "id");
                        });
                break;

            default:
                break;
        }


    }
}
