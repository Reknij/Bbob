using Bbob.Plugin;

namespace Bbob.Main.PluginManager;

public record class PluginContext(IPlugin main, PluginJson info);
public class PluginContextRef
{
    public HashSet<PluginContextRef> previous = new();
    public PluginContext current;
    public HashSet<PluginContextRef> nexts = new();

    public PluginContextRef(PluginContext current)
    {
        this.current = current;
    }

    public override bool Equals(object? other)
    {
        if (other is PluginContextRef v)
        {
            return current.info.name == v.current.info.name;
        }
        return false;
    }

    public override string ToString()
    {
        return current.info.name;
    }

    public override int GetHashCode()
    {
        return current.info.name.GetHashCode();
    }
}

public class PluginRelation
{
    List<PluginContext> plugins;
    HashSet<PluginContextRef> pcrs = new HashSet<PluginContextRef>();
    public PluginRelation(List<PluginContext> plugins)
    {
        this.plugins = plugins;
    }

    public List<PluginContext> ProcessRelation()
    {
        foreach (var pluginContext in plugins)
        {
            if (getPcrFromPcrs(pluginContext)) continue;
            var pcr = GetPluginContextRef(pluginContext);
            pcrs.Add(pcr);
        }
        HashSet<PluginContext> pluginsProcessed = new HashSet<PluginContext>();
        foreach (var pcr in pcrs)
        {
            addToProcessed(pluginsProcessed, pcr);
        }
        return pluginsProcessed.ToList();
    }

    private void addToProcessed(HashSet<PluginContext> pcList, PluginContextRef pcr)
    {
        if (pcList.Contains(pcr.current)) return;
        foreach (var prev in pcr.previous)
        {
            addToProcessed(pcList, prev);
        }
        pcList.Add(pcr.current);
        foreach (var next in pcr.nexts)
        {
            addToProcessed(pcList, next);
        }
    }

    private PluginContextRef GetPluginContextRef(PluginContext context)
    {
        if (getPcrFromPcrs(context, out var pcr) && pcr != null) return pcr;
        var c = new PluginContextRef(context);
        pcrs.Add(c);
        var attrs = Attribute.GetCustomAttributes(context.main.GetType());
        HashSet<string> readyPluginNames = new HashSet<string>();
        foreach (var attr in attrs)
        {
            if (attr is PluginCondition condition && (condition.ConditionType & ConditionType.OrderCheck) != 0)
            {
                List<PluginContext> others = new List<PluginContext>();
                if (condition.PluginName == "*")
                {
                    foreach (var p in plugins)
                    {
                        if (isTargetRequirePlugin(p, context)) continue;
                        if (p.info.name == context.info.name) continue;
                        if (!readyPluginNames.Contains(p.info.name)) others.Add(p);
                    }
                }
                else
                {
                    PluginContext? o = plugins.Find((item) => item.info.name == condition.PluginName);
                    if (o != null && !readyPluginNames.Contains(o.info.name)) others.Add(o);
                }

                foreach (var other in others)
                {
                    var otherPcr = GetPluginContextRef(other);
                    switch (condition.PluginOrder)
                    {
                        default:
                        case PluginOrder.Any: break;
                        case PluginOrder.AfterMe:
                            if (otherPcr.nexts.Contains(c)) break;
                            c.nexts.Add(otherPcr);
                            otherPcr.previous.Add(c);
                            break;
                        case PluginOrder.BeforeMe:
                            if (otherPcr.previous.Contains(c)) break;
                            c.previous.Add(otherPcr);
                            otherPcr.nexts.Add(c);
                            break;
                    }
                    readyPluginNames.Add(other.info.name);
                }
            }
        }

        return c;
    }

    private bool isTargetRequirePlugin(PluginContext targetPlugin, PluginContext requirePlugin)
    {
        var attrs = Attribute.GetCustomAttributes(targetPlugin.main.GetType());
        foreach (var attr in attrs)
        {
            if (attr is PluginCondition condition && (condition.ConditionType & ConditionType.OrderCheck) != 0)
            {
                if (condition.PluginName == requirePlugin.info.name) return true;
            }
        }
        return false;
    }
    private bool getPcrFromPcrs(PluginContext c) => getPcrFromPcrs(c, out var a);
    public bool getPcrFromPcrs(PluginContext c, out PluginContextRef? target)
    {
        foreach (var pcr in pcrs)
        {
            if (pcr.current.info.name == c.info.name)
            {
                target = pcr;
                return true;
            }
        }
        target = null;
        return false;
    }
}