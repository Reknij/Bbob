
using System.Text.Json;

namespace Bbob.Plugin;
public static class PluginHelper
{
    public static string ThemePath { get; set; } = "null";
    public static ConfigJson ConfigBbob {get;set;} = new ConfigJson();
    private static PluginJson? _ExecutingPlugin;
    public static PluginJson ExecutingPlugin
    {
        get => _ExecutingPlugin ?? throw new NullReferenceException("Executing plugin is null.");
        set => _ExecutingPlugin = value;
    }
    private static string? _currentDirectory;
    private static string? _baseDirectory;
    public static string CurrentDirectory
    {
        get => _currentDirectory ?? throw new NullReferenceException("current directory is null.");
        set => _currentDirectory = value;
    }
    public static string BaseDirectory
    {
        get => _baseDirectory ?? throw new NullReferenceException("base directory is null.");
        set => _baseDirectory = value;
    }
    public static class HelperDelegates
    {
        public delegate void ModifyObjectDelegate<T>(ref T? obj);
    }
    static Dictionary<string, object?> pluginsObject = new Dictionary<string, object?>();
    static Dictionary<string, object> metas = new Dictionary<string, object>();

    public static void registerObject(string name, object? obj)
    {
        if (pluginsObject.ContainsKey(name)) pluginsObject[name] = obj;
        else pluginsObject.Add(name, obj);
    }
    public static bool existsObject(string name)
    {
        return pluginsObject.ContainsKey(name);
    }
    public static bool existsObjectNoNull(string name)
    {
        return pluginsObject.ContainsKey(name) && pluginsObject[name] != null;
    }
    public static bool existsObjectNoNull<T>(string name)
    {
        return pluginsObject.ContainsKey(name) && pluginsObject[name]?.GetType() == typeof(T);
    }
    public static bool getRegisteredObject<T>(string name, out T? value)
    {
        if (pluginsObject.TryGetValue(name, out object? v) && v is T)
        {
            value = (T)v;
            return true;
        }
        value = default(T);
        return false;
    }
    public static T getRegisteredObjectNoNull<T>(string name)
    {
        bool exists = getRegisteredObject<T>(name, out T? a);
        if (exists && a != null)
        {
            return a;
        }
        throw new KeyNotFoundException($"Object name {name} " + (!exists ? "is not exists!" : "value is null!"));
    }

    public static void clearAllObject()
    {
        pluginsObject.Clear();
    }

    public static bool modifyRegisteredObject<T>(string name, HelperDelegates.ModifyObjectDelegate<T> modifyObjectDelegate)
    {
        if (pluginsObject.TryGetValue(name, out object? value) && value is T)
        {
            T? obj = (T?)value;
            modifyObjectDelegate?.Invoke(ref obj);
            pluginsObject[name] = obj;

            return true;
        }
        return false;
    }
    public static bool unregisterObject(string name)
    {
        return pluginsObject.Remove(name);
    }
    public static bool getPluginJsonConfig<T>(string pluginName, out T? config)
    {
        string configsDirectory = Path.Combine(CurrentDirectory, "configs");

        string pluginConfigJson = Path.Combine(configsDirectory, $"{pluginName}.config.json");
        if (File.Exists(pluginConfigJson))
        {
            try
            {
                using (FileStream fs = new FileStream(pluginConfigJson, FileMode.Open, FileAccess.Read))
                {
                    config = JsonSerializer.Deserialize<T>(fs);
                    return true;
                }
            }
            catch (System.Exception ex)
            {
                System.Console.WriteLine($"Get plugin config error:\n{ex.ToString()}");
            }
        }
        config = default(T);
        return false;
    }
    public static bool getPluginJsonConfig<T>(out T? config) =>
    getPluginJsonConfig<T>(ExecutingPlugin.name ?? throw new NullReferenceException("Executing plugin name is null."), out config);

    public static void savePluginJsonConfig<T>(string pluginName, T config)
    {
        try
        {
            string configsDirectory = Path.Combine(CurrentDirectory, "configs");
            string pluginConfigJson = Path.Combine(configsDirectory, $"{pluginName}.config.json");
            if (File.Exists(pluginConfigJson)) File.Delete(pluginConfigJson);
            using (FileStream fs = File.OpenWrite(pluginConfigJson))
            {
                JsonSerializer.Serialize<T>(fs, config);
            }
        }
        catch (System.Exception ex)
        {
            System.Console.WriteLine($"Save plugin config error:\n{ex.ToString()}");
        }
    }
    public static void savePluginJsonConfig<T>(T config) =>
    savePluginJsonConfig<T>(ExecutingPlugin.name, config);
    public static void printConsole(string msg)
    {
        System.Console.WriteLine($"[{ExecutingPlugin.name}]: {msg}");
    }

    public static void registerMeta(object meta)
    {
        registerMeta(ExecutingPlugin.name, meta);
    }
    public static void registerMeta(string metaName, object meta)
    {
        if (metas.ContainsKey(metaName)) metas.Remove(metaName);
        metas.Add(metaName, meta);
    }
    public static bool unregisterMeta() => unregisterMeta(ExecutingPlugin.name);
    public static bool unregisterMeta(string metaName)
    {
        if (metas.ContainsKey(metaName))
        {
            metas.Remove(metaName);
            return true;
        }
        return false;
    }
    public static T? getThemeInfo<T>()
    {
        using (FileStream fs = File.OpenRead(Path.Combine(ThemePath, "theme.json")))
        {
            return JsonSerializer.Deserialize<T>(fs);
        }
    }
    public static Dictionary<string, object> _getAllMetas() => metas;

    public static sortArticlesDelegate? sortArticles { get; set; }
    public static sortCategoriesDelegate? sortCategories { get; set; }
    public static sortTagsDelegate? sortTags { get; set; }
    public static CommandResult ExecutingCommandResult { get; set; }
}