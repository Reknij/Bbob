using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;

namespace Bbob.Main.Cli;

public class Preview : Command
{
    string distribution;
    public Preview(string distribution)
    {
        this.distribution = distribution;
    }
    public override bool Process()
    {
        if (!Directory.Exists(distribution))
        {
            System.Console.WriteLine("Distribution not exists!");
            return false;
        }
        if (Directory.GetFiles(distribution, "*", SearchOption.AllDirectories).Length == 0)
        {
            System.Console.WriteLine("Distribution is not exists any files!");
            return false;
        }
        StartPreview();
        return true;
    }

    public void StartPreview()
    {
        var config = Configuration.ConfigManager.GetConfigManager().MainConfig;
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