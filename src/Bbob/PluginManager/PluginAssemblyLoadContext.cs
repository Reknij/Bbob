using System.Reflection;
using System.Runtime.Loader;
using System.Text.Json;
using Bbob.Plugin;
using ConsoleHelper = Bbob.Shared.SharedLib.ConsoleHelper;

namespace Bbob.Main.PluginManager;

public class PluginAssemblyLoadContext : AssemblyLoadContext
{
    AssemblyDependencyResolver resolver;
    IPlugin? plugin;
    public IPlugin Plugin => plugin ?? throw new NullReferenceException("Plugin is null in PluginAssemblyLoadCOntext");
    public bool havePlugin = false;
    public string Warning = string.Empty;

    public PluginJson PluginInfo { get; protected set; }
    public PluginAssemblyLoadContext(string pluginPath, PluginJson info) : base(isCollectible: false)
    {
        resolver = new AssemblyDependencyResolver(pluginPath);
        PluginInfo = info;
        using (FileStream fs = new FileStream(pluginPath, FileMode.Open))
        {
            Assembly assembly = this.LoadFromStream(fs);
            foreach (Type type in assembly.GetTypes())
            {
                if (type.GetInterface("IPlugin") == typeof(IPlugin))
                {
                    try
                    {
                        object? instance = Activator.CreateInstance(type);
                        this.plugin = (IPlugin?)instance;
                        if (instance != null) havePlugin = true;
                    }
                    catch (System.Exception ex)
                    {
                        ConsoleHelper.printError($"Initialize plugin <{info.name}> throw error:\n{ex}");
                    }
                }

            }
        }
    }

    protected override Assembly? Load(AssemblyName assemblyName)
    {
        string? assemblyPath = resolver.ResolveAssemblyToPath(assemblyName);
        Assembly IPluginAssembly = Assembly.GetAssembly(typeof(IPlugin)) ?? throw new NullReferenceException("Can't get assembly IPlugin!");
        if (assemblyName.Name == IPluginAssembly.GetName().Name) //return null to shared Main application assembly
        {
            Version now = IPluginAssembly.GetName().Version ?? new Version(0, 0, 0, 0);
            Version tar = assemblyName.Version ?? new Version(0, 0, 0, 0);
            if (tar != now)
            {
                if (tar.Major > now.Major || (tar.Major == now.Major && tar.Minor > now.Minor))
                {
                    Warning = $"Plugin <{PluginInfo.name}> interface version is newer than version now. Please update Bbob-Cli first, otherwise plugin will not working.";
                }
                else if (tar.Major < now.Major || (tar.Major == now.Major && tar.Minor < now.Minor))
                {
                    Warning = $"Plugin <{PluginInfo.name}> interface version is older than version now. Please update plugin first, otherwise plugin will not working.";
                }
                if (assemblyPath != null) return LoadFromAssemblyPath(assemblyPath);
            }
            return null;
        }
        if (assemblyPath != null)
        {
            return LoadFromAssemblyPath(assemblyPath);
        }

        return null;
    }

    protected override IntPtr LoadUnmanagedDll(string unmanagedDllName)
    {
        string? libraryPath = resolver.ResolveUnmanagedDllToPath(unmanagedDllName);
        if (libraryPath != null)
        {
            return LoadUnmanagedDllFromPath(libraryPath);
        }

        return IntPtr.Zero;
    }
}