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
        string dist = Path.Combine(Environment.CurrentDirectory, Configuration.ConfigManager.MainConfig.distributionPath);
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
        string url = $"http://localhost:{CliShared.GetAvailablePort(Configuration.ConfigManager.MainConfig.previewPort)}";

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
                                PreviewIt(dist, url);
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
                {
                    Func<bool> check = () =>
                    {
                        switch (arguments[i])
                        {
                            case Commands.Preview.Host.Current:
                            case Commands.Preview.Host.CurrentAka:
                                if (++i < length) url = arguments[i];
                                else
                                {
                                    System.Console.WriteLine("Please enter your host value");
                                    return false;
                                }
                                break;
                            default:
                                break;
                        }
                        return true;
                    };
                    if (++i < length)
                    {
                        check();
                    }
                    PreviewIt(dist, url);
                }
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
                    string content = string.Empty;
                    bool global = false;
                    bool replace = false;
                    Func<string, bool> checkArgument = (argument) =>
                    {
                        switch (argument)
                        {
                            case Commands.Add.Address:
                            case Commands.Add.AddressAka:
                            case Commands.Add.File:
                            case Commands.Add.FileAka:
                                option = argument == Commands.Add.Address ? Add.Options.Address : Add.Options.File;
                                if (++i < length) content = arguments[i];
                                else
                                {
                                    System.Console.WriteLine("No exists argument <content>!");
                                    return false;
                                }
                                break;
                            case Commands.Add.Global:
                            case Commands.Add.GlobalAka:
                                global = true;
                                break;
                            case Commands.Add.Replace:
                            case Commands.Add.ReplaceAka:
                                replace = true;
                                break;
                            default:
                                System.Console.WriteLine($"Unknown option '{argument}'!");
                                return false;
                        }
                        return true;
                    };
                    while (++i < length)
                    {
                        if (!checkArgument(arguments[i])) return;
                    }
                    if (string.IsNullOrWhiteSpace(content))
                    {
                        System.Console.WriteLine("Please enter the <content> of your want add.");
                        return;
                    }
                    Add install = new Add(content, option, global, replace);
                    install.Process();
                }
                break;
            case Commands.Remove.Current:
                {
                    string name = string.Empty;
                    bool global = false;
                    Func<string, bool> checkArgument = (argument) =>
                    {
                        switch (argument)
                        {
                            case Commands.Remove.Global:
                            case Commands.Remove.GlobalAka:
                                global = true;
                                break;
                            default:
                                name = argument;
                                break;
                        }
                        return true;
                    };
                    while (++i < length)
                    {
                        if (!checkArgument(arguments[i])) return;
                    }
                    if (string.IsNullOrWhiteSpace(name))
                    {
                        System.Console.WriteLine("Please enter the <name> of your want remove.");
                        return;
                    }
                    Remove remove = new Remove(name, global);
                    remove.Process();
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

    private void PreviewIt(string dist, string url)
    {
        var preview = new Bbob.Main.Cli.Preview(dist, url);
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
            public static class Host
            {
                public const string Current = "--url";
                public const string CurrentAka = "-u";
            }
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
            public const string Global = "--global";
            public const string GlobalAka = "-g";
            public const string Replace = "--replace";
            public const string ReplaceAka = "-r";
        }

        public static class Remove
        {
            public const string Current = "remove";
            public const string Global = Add.Global;
            public const string GlobalAka = Add.GlobalAka;
        }
    }
}