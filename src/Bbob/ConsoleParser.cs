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
                init.Process();
                break;
            case Commands.Generate.Current:
            case Commands.Generate.CurrentAka:
                Generator generater = new Generator(dist, afp);
                if (generater.Process())
                {
                    if (++i < length)
                        switch (arguments[i])
                        {
                            case Commands.Deploy.Current:
                            case Commands.Deploy.CurrentAka:
                                DeployIt(dist);
                                break;
                            case Commands.Preview.Current:
                            case Commands.Preview.CurrentAka:
                                PreviewIt(dist);
                                break;
                            default: break;
                        }
                }
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
                creater.Process();
                break;
            case Commands.Deploy.Current:
            case Commands.Deploy.CurrentAka:
                DeployIt(dist);
                break;
            case Commands.Preview.Current:
            case Commands.Preview.CurrentAka:
                PreviewIt(dist);
                break;
            case Commands.ResetConfig.Current:
            case Commands.ResetConfig.CurrentAka:
                if (++i < length)
                {
                    ResetConfig resetConfig = new ResetConfig(arguments[i]);
                    resetConfig.Process();
                }
                break;

            default:
                System.Console.WriteLine($"Unknown command: {arguments[i]}");
                break;
        }
    }

    private void DeployIt(string dist)
    {
        Deploy deploy = new Deploy(dist);
        deploy.Process();
    }

    private void PreviewIt(string dist)
    {
        var preview = new Bbob.Main.Cli.Preview(dist);
        System.Console.WriteLine("Running preview...");
        preview.Process();
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
        }

        public static class Deploy
        {
            public const string Current = "deploy";
            public const string CurrentAka = "d";
        }

        public static class Preview
        {
            public const string Current = "preview";
            public const string CurrentAka = "p";
        }

        public static class ResetConfig
        {
            public const string Current = "reset-config";
            public const string CurrentAka = "rc";
        }
    }
}