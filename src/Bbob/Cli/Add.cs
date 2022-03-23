using SharpCompress.Common;
using SharpCompress.Readers;

namespace Bbob.Main.Cli;

public class Add : Command
{
    public new static string Name => "add";
    public new static string Help => "Add the theme or plugin. Auto detect.\n" +
    "<option>:\n" +
    "--address | -a : Add from address. <content> is address.\n" +
    "--file | -f : Add from local path. <content> is local file path.\n" +
    "--global | -g : Add <content> to global directory.\n" +
    "--replace | -r : Replace <content> no overwrite.\n\n" +
    "Use:\n" +
    "// add <option> <content>";

    public string Content { get; set; } = "";
    public bool Replace { get; set; } = false;
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
    public Add(string content, Options option, bool Global, bool Replace = false) : base(false)
    {
        this.Content = content;
        this.Replace = Replace;
        this.Option = option;
        this.Global = Global;
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
        if (Option == Options.Address)
        {
            Uri address;
            try
            {
                address = new Uri(Content);
                name = Path.GetFileNameWithoutExtension(address.LocalPath);
            }
            catch (System.Exception)
            {
                System.Console.WriteLine($"{FAILED}Please make sure content is url!");
                return false;
            }
            type = CliShared.isPluginOrThemeName(name, out name);
            if (type == CliShared.TextType.None)
            {
                System.Console.WriteLine($"{FAILED}Can't add because it no theme or plugin.");
                return false;
            }
            tempFilePath = Path.Combine(DownloadPath.Temp, Path.GetRandomFileName());
            HttpClient client = new HttpClient();
            try
            {
                var t = client.GetStreamAsync(address);
                System.Console.WriteLine("Downloading...");
                using (var s = t.Result)
                {
                    System.Console.WriteLine("Downloaded, installing...");
                    using (var f = File.OpenWrite(tempFilePath))
                    {
                        s.CopyTo(f);
                    }
                }
            }
            catch (System.Exception)
            {
                System.Console.WriteLine("Error downloading plugin!");
                System.Console.WriteLine($"{FAILED}Please make sure it is valid address!");
                return false;
            }
        }
        else if (Option == Options.File)
        {
            if (!File.Exists(Content))
            {
                System.Console.WriteLine($"{FAILED}File is not exists!");
                return false;
            }
            try
            {
                name = Path.GetFileNameWithoutExtension(Content);
                tempFilePath = Content;
                type = CliShared.isPluginOrThemeName(name, out name);
                if (type == CliShared.TextType.None)
                {
                    System.Console.WriteLine($"{FAILED}Can't add because it no theme or plugin.");
                    return false;
                }
            }
            catch (System.Exception)
            {
                System.Console.WriteLine($"{FAILED}Please make sure your enter the valid path!");
                return false;
            }
        }
        else return false;

        if (!isValidFilePath(name))
        {
            System.Console.WriteLine($"{FAILED}It is invalid: {name}");
            return false;
        }
        string downloadPath = Path.Combine(type == CliShared.TextType.Theme ? DownloadPath.Themes : DownloadPath.Plugins, name);
        if (Directory.Exists(downloadPath))
        {
            if (Replace)
            {
                Shared.SharedLib.DirectoryHelper.DeleteDirectory(downloadPath);
                addStatus = "Replace";
            }
            else addStatus = "Overwrite";
        }
        getContentFromFile(tempFilePath, downloadPath, false);

        Shared.SharedLib.DirectoryHelper.DeleteDirectory(DownloadPath.Temp);
        string p = Global ? "global" : "current";
        System.Console.WriteLine($"{SUCCESS}{addStatus} {name} to {p} directory path.!");
        return true;
    }

    private void getContentFromFile(string tempFilePath, string downloadPath, bool overrideIt)
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
            System.Console.WriteLine("Uncompress target file error:\n" + ex.Message);
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