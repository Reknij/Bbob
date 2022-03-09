using Bbob.Plugin;

namespace Bbob.Main.BuildInPlugin;

public class InitializeBbob: IPlugin
{
    readonly string InitDoneFile = Path.Combine(PluginHelper.CurrentDirectory, "BbobInitDone");
    public bool IsInitDone => File.Exists(InitDoneFile);
    public void InitCommand()
    {
        PluginHelper.getPluginJsonConfig<MyConfig>(out MyConfig? config);
        if (config == null) config = new MyConfig();

        InitializeFiles(config);
        if (config.IsAllEnable()) PluginHelper.printConsole("Initialize done...");
        else if (config.IsAllDisable()) PluginHelper.printConsole("Nothing initialized, all disabled...");
    }

    public void GenerateCommand(string filePath, GenerationStage stage)
    {
        if (!IsInitDone) this.InitCommand();
    }

    private void InitializeFiles(MyConfig config)
    {
        if (config.gitignore.enable)
        {
            PluginHelper.printConsole("Initialize gitignore file.");
            string gitignore = Path.Combine(PluginHelper.CurrentDirectory, ".gitignore");
            using (FileStream fs = File.OpenWrite(gitignore))
            {
                StreamWriter sw = new StreamWriter(fs);
                sw.WriteLine("# Generate by InitializeBbob build-in plugin.");
                sw.WriteLine("themes/");
                sw.WriteLine("dist/");
                sw.WriteLine(".deploy_git/");
                sw.WriteLine("plugins/");
                sw.WriteLine("BbobInitDone");
                WriteBbob(sw);
                if (config.gitignore.onlyDataFiles)
                {
                    sw.WriteLine("*.exe");
                    sw.WriteLine("*.dll");
                }
                sw.Close();
            }
        }
        File.Create(InitDoneFile).Close();
    }
    
    private void WriteBbob(StreamWriter sw)
    {
        sw.WriteLine("JSApi/");
        sw.WriteLine("Bbob.deps.json");
        sw.WriteLine("Bbob");
        sw.WriteLine("Bbob.exe");
    }

    public class MyConfig
    {
        public class Gitignore
        {
            public bool enable {get;set;} = true;
            public bool onlyDataFiles{get;set;} = true;
        }
        public Gitignore gitignore{get;set;} = new Gitignore();
        public bool IsAllEnable()
        {
            return gitignore.enable;
        }
        public bool IsAllDisable()
        {
            return !gitignore.enable;
        }
    }
}