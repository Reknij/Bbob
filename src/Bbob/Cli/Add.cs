using SharpCompress.Common;
using SharpCompress.Readers;
using ConsoleHelper = Bbob.Shared.SharedLib.ConsoleHelper;

namespace Bbob.Main.Cli;

public class Add : Command
{
    public new static string Name => "add";
    public new static string Help => "Add the theme or plugin. Auto detect.\n" +
    "<option>:\n" +
    "--address | -a : Add from address. <content> is address.\n" +
    "--file | -f : Add from local path. <content> is local file path.\n" +
    "--list | -l : Add from list file `addlist.txt` in current path.\n" +
    "--global | -g : Add <content> to global directory.\n" +
    "--replace | -r : Replace <content> no overwrite.\n\n" +
    "Use:\n" +
    "// add <option> [content]";

    public List<string> Contents { get; set; } = new();
    public bool Replace { get; set; } = false;
    private bool isList { get; set; } = false;
    private bool isGlobal = false;
    public bool Global
    {
        get => isGlobal;
        set
        {
            isGlobal = value;
            if (isGlobal)
            {
                DownloadPath.Plugins = Path.Combine(AppContext.BaseDirectory, "plugins");
                DownloadPath.Themes = Path.Combine(AppContext.BaseDirectory, "themes");
            }
            else
            {
                DownloadPath.Plugins = Path.Combine(Environment.CurrentDirectory, "plugins");
                DownloadPath.Themes = Path.Combine(Environment.CurrentDirectory, "themes");
            }
        }
    }
    public static class DownloadPath
    {
        public static string Plugins = Path.Combine(Environment.CurrentDirectory, "plugins");
        public static string Themes = Path.Combine(Environment.CurrentDirectory, "themes");
        public readonly static string Temp = Path.Combine(AppContext.BaseDirectory, "temp");
    }
    public enum Options
    {
        Address,
        File
    }
    public Options Option { get; set; } = Options.Address;
    public Add(string content, Options option, bool Global, bool Replace, bool isList) : base(false)
    {
        this.Replace = Replace;
        this.Option = option;
        this.Global = Global;
        this.isList = isList;

        var addlistPath = Path.Combine(Environment.CurrentDirectory, "addlist.txt");
        if (this.isList)
        {
            if (File.Exists(addlistPath)) this.Contents = File.ReadAllLines(addlistPath).ToList();
            for (int i = 0; i < this.Contents.Count; i++)
            {
                if (string.IsNullOrWhiteSpace(this.Contents[i])) this.Contents.RemoveAt(i);
            }
        }
        else this.Contents.Add(content);
    }

    public override bool Process()
    {
        const string SUCCESS = "Success: ";
        const string FAILED = "Failed: ";
        if (Directory.Exists(DownloadPath.Temp)) Shared.SharedLib.DirectoryHelper.DeleteDirectory(DownloadPath.Temp);
        Directory.CreateDirectory(DownloadPath.Temp);
        string name = "";
        string tempFilePath = "";
        string addStatus = "Added";
        CliShared.TextType type;

        if (Contents.Count == 0)
        {
            ConsoleHelper.printError($"{FAILED}Please make sure `addlist.txt` is exists! You are using `addlist.txt` file to add from list.");
            return false;
        }

        foreach (var content in Contents)
        {
            if (Option == Options.Address)
            {
                Uri address;
                string? filename = null;
                try
                {
                    address = new Uri(content);
                    filename = Path.GetFileNameWithoutExtension(address.LocalPath);
                }
                catch (System.Exception)
                {
                    ConsoleHelper.printError($"{FAILED}Please make sure content is url!");
                    return false;
                }
                type = CliShared.isPluginOrThemeName(filename, out name);
                if (type == CliShared.TextType.None)
                {
                    ConsoleHelper.printError($"{FAILED}Can't add because it no theme or plugin.");
                    return false;
                }
                tempFilePath = Path.Combine(DownloadPath.Temp, filename + Path.GetRandomFileName());
                HttpClient client = new HttpClient();
                try
                {
                    var t = client.GetStreamAsync(address);
                    ConsoleHelper.print($"Downloading {name}", color: ConsoleColor.DarkCyan);
                    using (var s = t.Result)
                    {
                        using (var f = File.OpenWrite(tempFilePath))
                        {
                            s.CopyTo(f);
                        }
                    }
                }
                catch (System.Exception)
                {
                    ConsoleHelper.printError("Error downloading plugin!");
                    ConsoleHelper.printError($"{FAILED}Please make sure it is valid address!");
                    return false;
                }
            }
            else if (Option == Options.File)
            {
                if (!File.Exists(content))
                {
                    ConsoleHelper.printError($"{FAILED}File is not exists!");
                    return false;
                }
                try
                {
                    name = Path.GetFileNameWithoutExtension(content);
                    tempFilePath = content;
                    type = CliShared.isPluginOrThemeName(name, out name);
                    if (type == CliShared.TextType.None)
                    {
                        ConsoleHelper.printError($"{FAILED}Can't add because it no theme or plugin.");
                        return false;
                    }
                }
                catch (System.Exception)
                {
                    ConsoleHelper.printError($"{FAILED}Please make sure your enter the valid path!");
                    return false;
                }
            }
            else return false;

            if (!isValidFilePath(name))
            {
                ConsoleHelper.printError($"{FAILED}It is invalid: {name}");
                return false;
            }
            string downloadPath = Path.Combine(type == CliShared.TextType.Theme ? DownloadPath.Themes : DownloadPath.Plugins, name.ToUpper());
            if (Directory.Exists(downloadPath))
            {
                if (Replace)
                {
                    Shared.SharedLib.DirectoryHelper.DeleteDirectory(downloadPath);
                    addStatus = "Replace";
                }
                else addStatus = "Overwrite";
            }
            getcontentFromFile(tempFilePath, downloadPath, false);

            string p = Global ? "global" : "current";
            string top = type == CliShared.TextType.Theme ? "theme" : "plugin";
            ConsoleHelper.printSuccess($"{SUCCESS}{addStatus} {top} {name} to {p} directory path.");
        }
        Shared.SharedLib.DirectoryHelper.DeleteDirectory(DownloadPath.Temp);
        return true;
    }

    private void getcontentFromFile(string tempFilePath, string downloadPath, bool overrideIt)
    {
        try
        {
            Directory.CreateDirectory(downloadPath);
            using (Stream stream = File.OpenRead(tempFilePath))
            using (var reader = ReaderFactory.Open(stream))
            {
                while (reader.MoveToNextEntry())
                {
                    if (!reader.Entry.IsDirectory)
                    {
                        reader.WriteEntryToDirectory(downloadPath, new ExtractionOptions()
                        {
                            ExtractFullPath = true,
                            Overwrite = true
                        });
                    }
                }
            }
        }
        catch (System.Exception ex)
        {
            ConsoleHelper.printError("Uncompress target file error:\n" + ex.Message);
        }
    }

    private bool isValidFilePath(string filePath)
    {
        foreach (char c in Path.GetInvalidFileNameChars())
        {
            if (filePath.Contains(c)) return false;
        }
        return true;
    }
}