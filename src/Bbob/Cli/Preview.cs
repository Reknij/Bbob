using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using ConsoleHelper = Bbob.Shared.SharedLib.ConsoleHelper;

namespace Bbob.Main.Cli;

public class Preview : Command
{
    public new static string Name => "preview";
    public new static string Help => "Preview the blog from distribution (folder name 'dist').\n" +
    "Use:\n" +
    "// preview\n" +
    "// p";

    string distribution;
    private string url = string.Empty;
    public string Url
    {
        get => url;
        set
        {
            if (Shared.SharedLib.UrlHelper.IsValid(value))
            {
                var result = Regex.Match(value, @".+\:([0-9]+)");
                if (result.Success && int.TryParse(result.Result("$1"), out int port) && (port < 1024 || port > 49151))
                {
                    ConsoleHelper.printWarning("Port in url is invalid, value must 1024 - 49151! Auto set to default port.");
                    value = Regex.Replace(value, @"(.+\:)([0-9]+)", @$"$1 {CliShared.GetAvailablePort(Configuration.ConfigManager.MainConfig.previewPort)}").Replace(" ", "");
                }
                url = value;
            }
            else
            {
                string u = $"http://localhost:{CliShared.GetAvailablePort(Configuration.ConfigManager.MainConfig.previewPort)}";
                ConsoleHelper.printWarning($"Host is invalid! Auto set to default url '{u}'");
                url = u;
            }
            if (url.Length > 0 && url.Last() == '/') url = url.Remove(url.Length - 1, 1);
        }
    }
    public Preview(string distribution, string url)
    {
        this.distribution = distribution;
        this.Url = url;
    }
    public override bool Process()
    {
        const string SUCCESS = "Success preview: ";
        const string FAILED = "Failed preview: ";
        if (!Directory.Exists(distribution))
        {
            ConsoleHelper.printError($"{FAILED}Distribution not exists!");
            return false;
        }
        if (Directory.GetFiles(distribution, "*", SearchOption.AllDirectories).Length == 0)
        {
            ConsoleHelper.printError($"{FAILED}Distribution is not exists any files!");
            return false;
        }
        ConsoleHelper.printWarning("Starting preview server...");
        bool result = StartPreview();
        if (result) ConsoleHelper.printSuccess($"{SUCCESS}Preview has been run.");
        else ConsoleHelper.printError($"{FAILED}Preview has stopped.");
        return result;
    }

    public bool StartPreview()
    {
        var config = Configuration.ConfigManager.MainConfig;
        var builder = WebApplication.CreateBuilder();
        builder.Logging.ClearProviders();
        var app = builder.Build();
        app.Use(async (context, next) =>
        {
            await next();
            if (context.Response.StatusCode == 404)
            {
                context.Request.Path = "/";
                await next();
            }
        });
        app.UsePathBase(config.baseUrl);
        app.UseFileServer(new FileServerOptions()
        {
            FileProvider = new PhysicalFileProvider(distribution),

        });
        ConsoleHelper.printSuccess($"Preview running at {Url}{config.baseUrl}");
        ConsoleHelper.print("Ctrl + C to stop preview.", color:ConsoleColor.Cyan);
        try
        {
            app.Run(Url);
        }
        catch (System.Exception ex)
        {
            ConsoleHelper.printError("Run preview error:\n" + ex.Message);
            return false;
        }
        return true;
    }
}