using System.Reflection;
using Bbob.Main;
using Bbob.Main.Cli;
using Bbob.Main.PluginManager;
using Bbob.Plugin;
using static Bbob.Main.Cli.List;

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
        int length = arguments.Length;
        if (length >= 3 && arguments[length - 2] == "--config-file")
        {
            string configPath = arguments[length - 1];
            if (!File.Exists(configPath))
            {
                System.Console.WriteLine("Config file is not exists!");
                return;
            }
            Configuration.ConfigManager.ConfigPath = configPath;
            var l = arguments.ToList();
            l.RemoveRange(length - 2, 2);
            arguments = l.ToArray();
            length = arguments.Length;
        }
        int i = 0;
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
                InitializeBbob.Initialize(InitializeBbob.InitializeOptions.All);
                Init init = new Init();
                init.Process();
                break;
            case Commands.Generate.Current:
            case Commands.Generate.CurrentAka:
                InitializeBbob.Initialize(InitializeBbob.InitializeOptions.All);
                Generator generater = new Generator(dist, afp);
                if (generater.Process())
                {
                    if (++i < length)
                        switch (arguments[i])
                        {
                            case Commands.Deploy.BeOption:
                            case Commands.Deploy.BeOptionAka:
                                DeployIt(dist);
                                break;
                            case Commands.Preview.BeOption:
                            case Commands.Preview.BeOptionAka:
                                PreviewIt(dist);
                                break;
                            default:
                                System.Console.WriteLine($"Unknown option '{arguments[i]}'!");
                                return;
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
                        default:
                            if (arguments[i].StartsWith("-"))
                            {
                                System.Console.WriteLine($"Unknown option '{arguments[i]}'!");
                                return;
                            }
                            else --i;
                            break;
                    }
                if (++i < length) filename = arguments[i];
                InitializeBbob.Initialize(InitializeBbob.InitializeOptions.All);
                Creator creater = new Creator(filename, afp, types);
                creater.Process();
                break;
            case Commands.Deploy.Current:
            case Commands.Deploy.CurrentAka:
                InitializeBbob.Initialize(InitializeBbob.InitializeOptions.All);
                DeployIt(dist);
                break;
            case Commands.Preview.Current:
            case Commands.Preview.CurrentAka:
                InitializeBbob.Initialize(InitializeBbob.InitializeOptions.All);
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
            case Commands.EnableAndDisable.Enable:
                {
                    string pluginName = "";
                    bool direct = false;
                    if (++i < length)
                    {
                        switch (arguments[i])
                        {
                            case Commands.EnableAndDisable.Direct:
                            case Commands.EnableAndDisable.DirectAka:
                                direct = true;
                                break;
                            default:
                                if (arguments[i].StartsWith("-"))
                                {
                                    System.Console.WriteLine($"Unknown option '{arguments[i]}'!");
                                    return;
                                }
                                else --i;
                                break;
                        }
                    }
                    if (++i < length)
                    {
                        pluginName = arguments[i];

                    }
                    eodPlugin(EnableAndDisable.Options.enable, pluginName, direct);
                }
                break;
            case Commands.EnableAndDisable.Disable:
                {
                    string pluginName = "";
                    bool direct = false;
                    if (++i < length)
                    {
                        switch (arguments[i])
                        {
                            case Commands.EnableAndDisable.Direct:
                            case Commands.EnableAndDisable.DirectAka:
                                direct = true;
                                break;
                            default:
                                if (arguments[i].StartsWith("-"))
                                {
                                    System.Console.WriteLine($"Unknown option '{arguments[i]}'!");
                                    return;
                                }
                                else --i;
                                break;
                        }
                    }
                    if (++i < length)
                    {
                        pluginName = arguments[i];
                    }
                    eodPlugin(EnableAndDisable.Options.disable, pluginName, direct);
                }
                break;
            case Commands.List.Current:
            case Commands.List.CurrentAka:
                {
                    DataType type = DataType.Plugins;
                    if (++i < length)
                    {
                        switch (arguments[i])
                        {
                            case Commands.List.Plugins:
                            case Commands.List.PluginsAka:
                                type = DataType.Plugins;
                                break;
                            default:
                                System.Console.WriteLine($"Unknown option '{arguments[i]}'!");
                                --i;
                                return;
                        }
                    }
                    else
                    {
                        System.Console.WriteLine("Please enter option!");
                        return;
                    }
                    InitializeBbob.Initialize(InitializeBbob.InitializeOptions.All);
                    List list = new List(type);
                    list.Process();
                }
                break;
            case Commands.Add.Current:
                {
                    Add.Options option = Add.Options.Address;
                    if (++i < length)
                    {
                        switch (arguments[i])
                        {
                            case Commands.Add.Address:
                            case Commands.Add.AddressAka:
                                option = Add.Options.Address;
                                break;
                            case Commands.Add.File:
                            case Commands.Add.FileAka:
                                option = Add.Options.File;
                                break;
                            default:
                                System.Console.WriteLine($"Unknown option '{arguments[i]}'!");
                                return;
                        }
                    }
                    if (++i < length)
                    {
                        string content = arguments[i];
                        bool global = false;
                        if (++i < length)
                        {
                            if (arguments[i] == "-g") global = true;
                            else System.Console.WriteLine($"Unknown option '{arguments[i]}'");
                        }
                        Add install = new Add(content, option, global);
                        install.Process();
                    }
                    else
                    {
                        System.Console.WriteLine("Please enter content!");
                    }
                }
                break;
            case Commands.Remove.Current:
                if (++i < length)
                {
                    string name = arguments[i];
                    bool global = false;
                    if (++i < length)
                    {
                        if (arguments[i] == "-g") global = true;
                        else System.Console.WriteLine($"Unknown option '{arguments[i]}'");
                    }
                    Remove remove = new Remove(name, global);
                    remove.Process();
                }
                else
                {
                    System.Console.WriteLine("Please enter content!");
                }

                break;
            default:
                System.Console.WriteLine($"Unknown command: {arguments[i]}!");
                break;
        }
    }

    private void eodPlugin(EnableAndDisable.Options option, string pluginName, bool direct)
    {
        InitializeBbob.Initialize(InitializeBbob.InitializeOptions.Config);
        EnableAndDisable eod = new EnableAndDisable(option, pluginName, direct);
        eod.Process();
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
        public static class Help
        {
            public const string Current = "--help";
            public const string CurrentAka = "-h";
        }
        public static class Version
        {
            public const string Current = "--version";
            public const string CurrentAka = "-v";
        }
        public static class Init
        {
            public const string Current = "init";
            public const string CurrentAka = "i";
        }
        public static class New
        {
            public const string Current = "new";
            public const string CurrentAka = "n";
            public const string Blog = "--blog";
            public const string BlogAka = "-b";
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
            public const string BeOption = "--deploy";
            public const string BeOptionAka = "-d";
        }

        public static class Preview
        {
            public const string Current = "preview";
            public const string CurrentAka = "p";
            public const string BeOption = "--preview";
            public const string BeOptionAka = "-p";
        }

        public static class ResetConfig
        {
            public const string Current = "reset-config";
            public const string CurrentAka = "rc";
        }
        public static class EnableAndDisable
        {
            public const string Enable = "enable";
            public const string Disable = "disable";
            public const string Direct = "--direct";
            public const string DirectAka = "-d";
        }
        public static class List
        {
            public const string Current = "list";
            public const string CurrentAka = "l";
            public const string Plugins = "--plugins";
            public const string PluginsAka = "-p";
        }

        public static class Add
        {
            public const string Current = "add";
            public const string Address = "--address";
            public const string AddressAka = "-a";
            public const string File = "--file";
            public const string FileAka = "-f";
        }

        public static class Remove
        {
            public const string Current = "remove";
        }
    }
}