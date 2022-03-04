using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;

namespace Bbob.Main.Cli;

public class Preview : Command
{
    public new static string Name => "preview";
    public new static string Help => "Preview the blog from distribution (folder name 'dist').\n"+
    "Use:\n"+
    "// preview\n"+
    "// p";

    string distribution;
    public Preview(string distribution)
    {
        this.distribution = distribution;
    }
    public override bool Process()
    {
        const string SUCCESS = "Success preview: ";
        const string FAILED = "Failed preview: ";
        if (!Directory.Exists(distribution))
        {
            System.Console.WriteLine($"{FAILED}Distribution not exists!"); 
            return false;
        }
        if (Directory.GetFiles(distribution, "*", SearchOption.AllDirectories).Length == 0)
        {
            System.Console.WriteLine($"{FAILED}Distribution is not exists any files!");
            return false;
        }
        StartPreview();
        System.Console.WriteLine($"{SUCCESS}Preview has been run.");
        return true;
    }

    public void StartPreview()
    {
        var config = Configuration.ConfigManager.MainConfig;
        string url = $"http://localhost:{config.previewPort}";
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
        app.UseFileServer(new FileServerOptions()
        {
            FileProvider = new PhysicalFileProvider(distribution),

        });
        System.Console.WriteLine($"Preview running at {url}");
        System.Console.WriteLine("Ctrl + C to stop preview.");
        app.Run(url);
    }
}