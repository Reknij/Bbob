using System.Reflection;
using Bbob.Main;
using Bbob.Main.Cli;
using Bbob.Main.Configuration;
using Bbob.Main.PluginManager;
using Bbob.Plugin;
using static Bbob.Main.Cli.List;
using ConsoleHelper = Bbob.Shared.SharedLib.ConsoleHelper;

namespace Bbob.Main;

class ConsoleParser
{
    string[] arguments;
    public ConsoleParser(string[] _arguments)
    {
        arguments = _arguments;
    }

    public bool requireInitialize()
    {
        var files = new string[]{
            "config.json",
            "BbobInitDone"
        };
        foreach (var file in files)
        {
            var path = Path.Combine(Environment.CurrentDirectory, file);
            if (!File.Exists(path)) return true;
        }
        return false;
    }

    public void Parse()
    {
        int length = arguments.Length;
        if (length >= 3 && arguments[length - 2] == "--config-file")
        {
            string configPath = arguments[length - 1];
            if (!File.Exists(configPath))
            {
                ConsoleHelper.printError("Config file is not exists!");
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
            ConsoleHelper.printError("Please enter commands!");
            return;
        }
        string afp = Path.Combine(Environment.CurrentDirectory, "articles");
        Func<Action, bool> isHelp = (act) =>
        {
            if (++i < length && (arguments[i] == Commands.Help.Current || arguments[i] == Commands.Help.CurrentAka))
            {
                act();
                return true;
            }
            --i;
            return false;
        };
        if (arguments[0] != Commands.Init.Current && arguments[0] != Commands.Init.CurrentAka && requireInitialize())
        {
            ConsoleHelper.printWarning($"Please ensure initialize first! Example: `bbob init`");
            return;
        }
        switch (arguments[i])
        {
            case Commands.Init.Current:
            case Commands.Init.CurrentAka:
                if (isHelp(printHelp<Init>)) return;
                InitializeBbob.Initialize(InitializeBbob.InitializeOptions.All);
                Init init = new Init();
                init.Process();
                break;
            case Commands.Generate.Current:
            case Commands.Generate.CurrentAka:
                if (isHelp(printHelp<Generator>)) return;
                {
                    InitializeBbob.Initialize(InitializeBbob.InitializeOptions.All);
                    string dist = Path.Combine(Environment.CurrentDirectory, Configuration.ConfigManager.MainConfig.distributionPath);
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
                                    string url = $"http://localhost:{CliShared.GetAvailablePort(Configuration.ConfigManager.MainConfig.previewPort)}";
                                    PreviewIt(dist, url);
                                    break;
                                default:
                                    ConsoleHelper.printWarning($"Unknown option '{arguments[i]}'!");
                                    return;
                            }
                    }
                    break;
                }

            case Commands.New.Current:
            case Commands.New.CurrentAka:
                if (isHelp(printHelp<Creator>)) return;
                TurnMessageShow(TurnOption.Theme, false);
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
                                ConsoleHelper.printWarning($"Unknown option '{arguments[i]}'!");
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
            case Commands.Dev.Current:
                if (isHelp(printHelp<Dev>)) return;
                {
                    var isConcise = false;
                    if (++i < length)
                        switch (arguments[i])
                        {
                            case Commands.Dev.Concise.Current:
                                isConcise = true;
                                break;
                            default:
                                ConsoleHelper.printWarning($"Unknown option '{arguments[i]}'!");
                                return;
                        }

                    Dev dev = new Dev()
                    {
                        isConcise = isConcise,
                    };
                    Generator generater = new Generator(dev.Distribution, dev.ArticlesFolder);
                    if (dev.Process())
                    {
                        generater.Process();
                        dev.ProcessAfterGenerated();
                    }

                    break;
                }
            case Commands.Deploy.Current:
            case Commands.Deploy.CurrentAka:
                if (isHelp(printHelp<Deploy>)) return;
                {
                    TurnMessageShow(TurnOption.Theme, false);
                    InitializeBbob.Initialize(InitializeBbob.InitializeOptions.All);
                    string dist = Path.Combine(Environment.CurrentDirectory, Configuration.ConfigManager.MainConfig.distributionPath);
                    DeployIt(dist);
                    break;
                }
            case Commands.Preview.Current:
            case Commands.Preview.CurrentAka:
                if (isHelp(printHelp<Preview>)) return;
                TurnMessageShow(TurnOption.Plugin | TurnOption.Theme, false);
                InitializeBbob.Initialize(InitializeBbob.InitializeOptions.Config);
                {
                    string url = $"http://localhost:{CliShared.GetAvailablePort(Configuration.ConfigManager.MainConfig.previewPort)}";
                    Func<bool> check = () =>
                    {
                        switch (arguments[i])
                        {
                            case Commands.Preview.Url.Current:
                            case Commands.Preview.Url.CurrentAka:
                                if (++i < length) url = arguments[i];
                                else
                                {
                                    ConsoleHelper.printWarning("Please enter your host value");
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
                    string dist = Path.Combine(Environment.CurrentDirectory, Configuration.ConfigManager.MainConfig.distributionPath);
                    PreviewIt(dist, url);
                }
                break;
            case Commands.ResetConfig.Current:
            case Commands.ResetConfig.CurrentAka:
                if (isHelp(printHelp<ResetConfig>)) return;
                TurnMessageShow(TurnOption.All, false);
                if (++i < length)
                {
                    ResetConfig resetConfig = new ResetConfig(arguments[i]);
                    resetConfig.Process();
                }
                break;
            case Commands.EnableAndDisable.Enable:
                if (isHelp(printHelp<EnableAndDisable>)) return;
                {
                    TurnMessageShow(TurnOption.All, false);
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
                                    ConsoleHelper.printWarning($"Unknown option '{arguments[i]}'!");
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
                if (isHelp(printHelp<EnableAndDisable>)) return;
                {
                    TurnMessageShow(TurnOption.All, false);
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
                                    ConsoleHelper.printWarning($"Unknown option '{arguments[i]}'!");
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
                if (isHelp(printHelp<List>)) return;
                {
                    TurnMessageShow(TurnOption.All, false);
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
                                ConsoleHelper.printWarning($"Unknown option '{arguments[i]}'!");
                                --i;
                                return;
                        }
                    }
                    else
                    {
                        ConsoleHelper.printError("Please enter option!");
                        return;
                    }
                    InitializeBbob.Initialize(InitializeBbob.InitializeOptions.All);
                    List list = new List(type);
                    list.Process();
                }
                break;
            case Commands.Add.Current:
                if (isHelp(printHelp<Add>)) return;
                {
                    TurnMessageShow(TurnOption.All, false);
                    Add.Options? option = null;
                    string content = string.Empty;
                    bool global = false;
                    bool replace = false;
                    bool isList = false;
                    Func<string, bool> checkArgument = (argument) =>
                    {
                        switch (argument)
                        {
                            case Commands.Add.Address:
                            case Commands.Add.AddressAka:
                                if (option != null) break;
                                option = Add.Options.Address;
                                break;
                            case Commands.Add.File:
                            case Commands.Add.FileAka:
                                if (option != null) break;
                                option = Add.Options.File;
                                break;
                            case Commands.Add.Global:
                            case Commands.Add.GlobalAka:
                                global = true;
                                break;
                            case Commands.Add.Replace:
                            case Commands.Add.ReplaceAka:
                                replace = true;
                                break;
                            case Commands.Add.List:
                            case Commands.Add.ListAka:
                                isList = true;
                                break;
                            default:
                                content = argument;
                                return false;
                        }
                        return true;
                    };
                    while (++i < length)
                    {
                        if (!checkArgument(arguments[i])) return;
                    }
                    if (option == null)
                    {
                        ConsoleHelper.printError("Please enter option '--address' or '--file'!");
                        return;
                    }
                    if (!isList && string.IsNullOrWhiteSpace(content))
                    {
                        ConsoleHelper.printError("Please enter the <content> of your want add.");
                        return;
                    }
                    Add install = new Add(content, option.Value, global, replace, isList);
                    install.Process();
                }
                break;
            case Commands.Remove.Current:
                if (isHelp(printHelp<Remove>)) return;
                {
                    TurnMessageShow(TurnOption.All, false);
                    string name = string.Empty;
                    bool global = false;
                    bool isAllPlugin = false;
                    bool isAllTheme = false;
                    Func<string, bool> checkArgument = (argument) =>
                    {
                        switch (argument)
                        {
                            case Commands.Remove.Global:
                            case Commands.Remove.GlobalAka:
                                global = true;
                                break;
                            case Commands.Remove.AllPlugin:
                                isAllPlugin = true;
                                break;
                            case Commands.Remove.AllTheme:
                                isAllTheme = true;
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
                    if (!isAllPlugin && !isAllTheme && string.IsNullOrWhiteSpace(name))
                    {
                        ConsoleHelper.printError("Please enter the <name> of your want remove.");
                        return;
                    }
                    Remove remove = new Remove(name, global, isAllPlugin, isAllTheme);
                    remove.Process();
                }
                break;
            case Commands.Run.Current:
                if (isHelp(printHelp<Run>)) return;
                {
                    TurnMessageShow(TurnOption.All, false);
                    InitializeBbob.Initialize(InitializeBbob.InitializeOptions.All);
                    string name = string.Empty;
                    string command = name;
                    bool isGlobal = false;
                    string[] args = Array.Empty<string>();
                    if (++i < length)
                    {
                        name = arguments[i];
                        if (name == "-g" || name == "--global") isGlobal = true;
                        if (++i < length)
                        {
                            command = arguments[i];
                            if (++i < length) Array.Copy(arguments, i, args = new string[length - i], 0, args.Length);
                        }
                        Run run = new Run(name, command, args, isGlobal);
                        run.Process();
                    }
                    else ConsoleHelper.printError("Please enter the name!");
                    break;
                }
            default:
                ConsoleHelper.printWarning($"Unknown command: {arguments[i]}!");
                break;
        }
    }

    private enum TurnOption
    {
        Plugin = 2,
        Theme = 4,
        Config = 6,
        All = Plugin | Theme | Config
    }
    private void TurnMessageShow(TurnOption turnOff, bool isOn = true)
    {
        if ((turnOff & TurnOption.Plugin) != 0) PluginSystem.ShowLoadedMessage = isOn;
        if ((turnOff & TurnOption.Theme) != 0) ThemeProcessor.ShowLoadedMessage = isOn;
        if ((turnOff & TurnOption.Config) != 0) Configuration.ConfigManager.ShowLoadedMessage = isOn;
    }

    private void printHelp<T>() where T : Command
    {
        Type type = typeof(T);
        string Name = (string?)type.GetProperty("Name", BindingFlags.Public | BindingFlags.Static)?.GetValue(null) ?? "";
        string Help = (string?)type.GetProperty("Help", BindingFlags.Public | BindingFlags.Static)?.GetValue(null) ?? "";

        System.Console.WriteLine($"Command '{Name}': {Help}");
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
        preview.Process();
    }

    public static class Commands
    {
        public static class Help
        {
            public const string Current = "--help";
            public const string CurrentAka = "-h";
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
        public static class Dev
        {
            public const string Current = "dev";
            public static class Concise
            {
                public const string Current = "--concise";
            }
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
            public static class Url
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
            public const string List = "--list";
            public const string ListAka = "-l";
        }

        public static class Remove
        {
            public const string Current = "remove";
            public const string Global = Add.Global;
            public const string GlobalAka = Add.GlobalAka;
            public const string AllPlugin = "--all-plugin";
            public const string AllTheme = "--all-theme";
        }

        public static class Run
        {
            public const string Current = "run";
        }
    }
}