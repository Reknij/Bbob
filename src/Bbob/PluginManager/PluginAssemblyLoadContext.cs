using System.Reflection;
using System.Runtime.Loader;
using System.Text.Json;
using Bbob.Plugin;

namespace Bbob.Main.PluginManager;

public class PluginAssemblyLoadContext : AssemblyLoadContext
{
    AssemblyDependencyResolver resolver;
    IPlugin? plugin;
    public IPlugin? Plugin => plugin;

    public PluginJson PluginInfo {get; protected set;}
    public PluginAssemblyLoadContext(string pluginPath, PluginJson info) : base(isCollectible: false)
    {
        resolver = new AssemblyDependencyResolver(pluginPath);
        PluginInfo = info;
        using (FileStream fs = new FileStream(pluginPath, FileMode.Open))
        {
            Assembly assembly = this.LoadFromStream(fs);
            foreach (Type type in assembly.GetTypes())
            {
                if (type.GetInterface("IPlugin") != null)
                {
                    try
                    {
                        object? instance = Activator.CreateInstance(type);
                        this.plugin = (IPlugin?)instance;
                    }
                    catch (System.Exception ex)
                    {
                        Console.WriteLine( ex.ToString());
                    }
                }
                    
            }
        }
    }

    protected override Assembly? Load(AssemblyName assemblyName)
    {
        string? assemblyPath = resolver.ResolveAssemblyToPath(assemblyName);
        Assembly? IPluginAssembly = Assembly.GetAssembly(typeof(IPlugin));

        if (assemblyName.FullName == IPluginAssembly?.FullName) //return null to shared Main application assembly
        {
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