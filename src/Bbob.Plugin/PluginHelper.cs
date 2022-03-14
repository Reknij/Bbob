
using System.Text.Json;

namespace Bbob.Plugin;
public static class PluginHelper
{
    /// <summary>
    /// Path of target theme folder.
    /// </summary>
    /// <value></value>
    public static string ThemePath { get; set; } = "null";

    /// <summary>
    /// Config of Bbob cli.
    /// </summary>
    /// <returns></returns>
    public static ConfigJson ConfigBbob { get; set; } = new ConfigJson();
    private static PluginJson? _ExecutingPlugin;

    /// <summary>
    /// Information of executing plugin.
    /// </summary>
    /// <value></value>
    public static PluginJson ExecutingPlugin
    {
        get => _ExecutingPlugin ?? throw new NullReferenceException("Executing plugin is null.");
        set => _ExecutingPlugin = value;
    }
    private static string? _currentDirectory;
    private static string? _baseDirectory;

    /// <summary>
    /// Current path of executing Bbob cli.
    /// </summary>
    /// <value></value>
    public static string CurrentDirectory
    {
        get => _currentDirectory ?? throw new NullReferenceException("current directory is null.");
        set => _currentDirectory = value;
    }

    /// <summary>
    /// Base path of Bbob cli.
    /// </summary>
    /// <value></value>
    public static string BaseDirectory
    {
        get => _baseDirectory ?? throw new NullReferenceException("base directory is null.");
        set => _baseDirectory = value;
    }

    /// <summary>
    /// Bbob command distribution path.
    /// </summary>
    /// <returns></returns>
    public static string DistributionDirectory => Path.Combine(CurrentDirectory, ConfigBbob.distributionPath);
    public static class HelperDelegates
    {
        public delegate void ModifyObjectDelegate<T>(ref T? obj);
    }
    static Dictionary<string, object?> pluginsObject = new Dictionary<string, object?>();
    static Dictionary<string, object> metas = new Dictionary<string, object>();

    /// <summary>
    /// Register object with target name to PluginHelper.
    /// </summary>
    /// <param name="name">Name of object</param>
    /// <param name="obj">Instance of object</param>
    public static void registerObject(string name, object? obj)
    {
        if (pluginsObject.ContainsKey(name)) pluginsObject[name] = obj;
        else pluginsObject.Add(name, obj);
    }

    /// <summary>
    /// Check is exists object or not.
    /// </summary>
    /// <param name="name">Name of object</param>
    /// <returns>True if exists, otherwise false.</returns>
    public static bool existsObject(string name)
    {
        return pluginsObject.ContainsKey(name);
    }

    /// <summary>
    /// Check is exists object and is not null or not.
    /// </summary>
    /// <param name="name">Name of object</param>
    /// <returns>True if exists and not null, otherwise false.</returns>
    public static bool existsObjectNoNull(string name)
    {
        return pluginsObject.ContainsKey(name) && pluginsObject[name] != null;
    }

    /// <summary>
    /// Check is exists object and is not null or not.
    /// </summary>
    /// <param name="name">Name of object</param>
    /// <typeparam name="T">Type of object</typeparam>
    /// <returns>True if exists and not null, otherwise false.</returns>
    public static bool existsObjectNoNull<T>(string name)
    {
        return pluginsObject.ContainsKey(name) && pluginsObject[name]?.GetType() == typeof(T);
    }

    /// <summary>
    /// Get register object from PluginHelper
    /// </summary>
    /// <param name="name">Name of object</param>
    /// <param name="value">Found object. May be null.</param>
    /// <typeparam name="T">Type of object</typeparam>
    /// <returns>True if object found, otherwise false.</returns>
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

    /// <summary>
    /// Get register object from PluginHelper and it not null. If it is null will throw exception. Please confirm it is not null.
    /// </summary>
    /// <param name="name">Name of object</param>
    /// <typeparam name="T">Type of object</typeparam>
    /// <returns>Object instance</returns>
    public static T getRegisteredObjectNoNull<T>(string name)
    {
        bool exists = getRegisteredObject<T>(name, out T? a);
        if (exists && a != null)
        {
            return a;
        }
        throw new KeyNotFoundException($"Object name {name} " + (!exists ? "is not exists!" : "value is null!"));
    }

    /// <summary>
    /// Clear all register object from PluginHelper. You should not use it.
    /// </summary>
    public static void clearAllObject()
    {
        pluginsObject.Clear();
    }

    /// <summary>
    /// Get the target register object from PluginHelper to modify.
    /// </summary>
    /// <param name="name">Name of target object</param>
    /// <param name="modifyObjectDelegate">Function to modify object.</param>
    /// <typeparam name="T">Type of target object</typeparam>
    /// <returns>True if found object, otherwise false.</returns>
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

    /// <summary>
    /// Unregister target object from PluginHelper.
    /// </summary>
    /// <param name="name">Name of object</param>
    /// <returns>True if remove success, otherwise false.</returns>
    public static bool unregisterObject(string name)
    {
        return pluginsObject.Remove(name);
    }

    /// <summary>
    /// Get target plugin config with target type.
    /// </summary>
    /// <param name="pluginName">Name of target plugin</param>
    /// <param name="config">Result of target plugin config.</param>
    /// <typeparam name="T">Type of instance result.</typeparam>
    /// <returns>True if success get config, otherwise false.</returns>
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

    /// <summary>
    /// Get executing plugin config with target type.
    /// </summary>
    /// <param name="config">Result of plugin config.</param>
    /// <typeparam name="T">Target type you want parse config.</typeparam>
    /// <returns>True if success get config, otherwise false.</returns>
    public static bool getPluginJsonConfig<T>(out T? config) =>
    getPluginJsonConfig<T>(ExecutingPlugin.name ?? throw new NullReferenceException("Executing plugin name is null."), out config);

    /// <summary>
    /// Save object to target plugin config. It is json file.
    /// </summary>
    /// <param name="pluginName">Target plugin name</param>
    /// <param name="config">Object config</param>
    /// <typeparam name="T">Type of object config</typeparam>
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

    /// <summary>
    /// Save object to executing plugin config. It is json file.
    /// </summary>
    /// <param name="config">Object config</param>
    /// <typeparam name="T">Type of object config</typeparam>
    public static void savePluginJsonConfig<T>(T config) =>
    savePluginJsonConfig<T>(ExecutingPlugin.name, config);

    /// <summary>
    /// Check plugin config is exists or not.
    /// </summary>
    /// <param name="pluginName">Name of plugin</param>
    /// <returns>True if exists, otherwise false.</returns>
    public static bool isPluginJsonConfigExists(string pluginName)
    {
        string configsDirectory = Path.Combine(CurrentDirectory, "configs");
        string pluginConfigJson = Path.Combine(configsDirectory, $"{pluginName}.config.json");
        return File.Exists(pluginConfigJson);
    }

    /// <summary>
    /// Check executing plugin config is exists or not.
    /// </summary>
    /// <returns>True if exists, otherwise false.</returns>
    public static bool isPluginJsonConfigExists() => isPluginJsonConfigExists(ExecutingPlugin.name);

    /// <summary>
    /// Print message to console of Bbob.
    /// </summary>
    /// <param name="msg">Message object. Will use ToString() function.</param>
    public static void printConsole(object msg)
    {
        System.Console.WriteLine($"[{ExecutingPlugin.name}]: {msg.ToString()}");
    }

    /// <summary>
    /// Register meta with name of executing plugin.
    /// </summary>
    /// <param name="meta">Meta object</param>
    public static void registerMeta(object meta)
    {
        registerMeta(ExecutingPlugin.name, meta);
    }

    /// <summary>
    /// Register meta with target name.
    /// </summary>
    /// <param name="metaName">Name of meta</param>
    /// <param name="meta">Meta object</param>
    public static void registerMeta(string metaName, object meta)
    {
        if (metas.ContainsKey(metaName)) metas.Remove(metaName);
        metas.Add(metaName, meta);
    }

    /// <summary>
    /// Unregister target meta name is plugin name.
    /// </summary>
    /// <returns>True if exists and removed, otherwise false.</returns>
    public static bool unregisterMeta() => unregisterMeta(ExecutingPlugin.name);

    /// <summary>
    /// Unregister target meta.
    /// </summary>
    /// <param name="metaName">Name of meta</param>
    /// <returns>True if exists and removed, otherwise false.</returns>
    public static bool unregisterMeta(string metaName)
    {
        if (metas.ContainsKey(metaName))
        {
            metas.Remove(metaName);
            return true;
        }
        return false;
    }

    /// <summary>
    /// Get theme.json with target type.
    /// </summary>
    /// <typeparam name="T">Target type to parse theme.json.</typeparam>
    /// <returns>Type instance of theme.json.</returns>
    public static T? getThemeInfo<T>()
    {
        using (FileStream fs = File.OpenRead(Path.Combine(ThemePath, "theme.json")))
        {
            return JsonSerializer.Deserialize<T>(fs);
        }
    }

    /// <summary>
    /// Check plugin is done or not.
    /// </summary>
    /// <param name="names">Name of plugins.</param>
    /// <returns>True if done, otherwise false.</returns>
    public static bool isTargetPluginDone(string[] names)
    {
        foreach (var name in names)
        {
            if (!isTargetPluginDone(name)) return false;
        }
        return true;
    }

    /// <summary>
    /// Check plugin is done or not.
    /// </summary>
    /// <param name="name">Name of plugin.</param>
    /// <returns>True if done, otherwise false.</returns>
    public static bool isTargetPluginDone(string name)
    {
        return _pluginsDone.Contains(name);
    }

    /// <summary>
    /// Check plugin is enable and is done or not.
    /// </summary>
    /// <param name="name"></param>
    /// <returns>True if enable and done, otherwise false.</returns>
    public static bool isTargetPluginEnableAndDone(string name)
    {
        return isTargetPluginEnable(name) && isTargetPluginDone(name);
    }

    /// <summary>
    /// Check plugin is enable or disable.
    /// </summary>
    /// <param name="name"></param>
    /// <returns>True if enable, otherwise false.</returns>
    public static bool isTargetPluginEnable(string name) => ConfigBbob.isPluginEnable(name);

    /// <summary>
    /// Metas of all plugins register. You should not use it.
    /// </summary>
    /// <returns></returns>
    public static Dictionary<string, object> _getAllMetas() => metas;

    /// <summary>
    /// HashSet of plugins done. You should not use it.
    /// </summary>
    /// <typeparam name="string"></typeparam>
    /// <returns></returns>
    public static HashSet<string> _pluginsDone = new HashSet<string>();

    /// <summary>
    /// Bbob will check the command operation in command result to determine execution next.
    /// </summary>
    /// <value></value>
    public static CommandResult ExecutingCommandResult { get; set; }
}