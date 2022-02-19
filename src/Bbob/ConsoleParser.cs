using Bbob.Main;
using Bbob.Main.Cli;
using Bbob.Main.PluginManager;
using Bbob.Plugin;

namespace Bbob.Main;

class ConsoleParser
{
    string[] arguments;
    public ConsoleParser(string[] _arguments)
    {
        arguments = _arguments;
    }

    public void Parse()
    {
        string dist = Path.Combine(Environment.CurrentDirectory, "dist");
        int i = 0;
        int length = arguments.Length;
        if (length == 0)
        {
            System.Console.WriteLine("Please enter commands!");
            return;
        }
        string afp = Path.Combine(Environment.CurrentDirectory, "articles");
        switch (arguments[i])
        {
            case Commands.Init.Current:
            case Commands.Init.CurrentAka:
                Init init = new Init();
                if (init.Process()) System.Console.WriteLine("Init done.");
                else System.Console.WriteLine("Init failed.");
                break;
            case Commands.Generate.Current:
            case Commands.Generate.CurrentAka:
                Generator generater = new Generator(dist, afp);
                if (generater.Process())
                {
                    System.Console.WriteLine("Generate success.");
                    if (++i < length)
                        switch (arguments[i])
                        {
                            case Commands.Generate.Deploy:
                            case Commands.Generate.DeployAka:
                                DeployIt(dist, false);
                                break;
                            default: break;
                        }
                }
                else System.Console.WriteLine("Generate failed.");
                break;

            case Commands.New.Current:
            case Commands.New.CurrentAka:
                NewTypes types = NewTypes.blog;
                string? filename = null;
                if (++i < length)
                    switch (arguments[i])
                    {
                        case Commands.New.Blog:
                        case Commands.New.BlogAka:
                            types = NewTypes.blog;
                            break;
                        default: break;
                    }
                if (++i < length) filename = arguments[i];
                Creator creater = new Creator(filename, afp, types);
                if (creater.Process()) System.Console.WriteLine($"Created {filename} success.");
                else System.Console.WriteLine($"Create {filename} failed.");
                break;
            case Commands.Deploy.Current:
            case Commands.Deploy.CurrentAka:
                DeployIt(dist);
                break;

            default:
                System.Console.WriteLine($"Unknown command: {arguments[i]}");
                break;
        }
    }

    private void DeployIt(string dist, bool load = true)
    {
        Deploy deploy = new Deploy(dist, load);
        if (deploy.Process()) System.Console.WriteLine("Run deploy done.");
        else System.Console.WriteLine("Run deploy failed.");
    }

    static class Commands
    {
        public static class Init
        {
            public const string Current = "init";
            public const string CurrentAka = "i";
        }
        public static class New
        {
            public const string Current = "new";
            public const string CurrentAka = "n";
            public const string Blog = "blog";
            public const string BlogAka = "b";
        }
        public static class Generate
        {
            public const string Current = "generate";
            public const string CurrentAka = "g";
            public const string Deploy = "deploy";
            public const string DeployAka = "d";
        }

        public static class Deploy
        {
            public const string Current = "deploy";
            public const string CurrentAka = "d";
        }
    }
}