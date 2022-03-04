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
        foreach (var attr in Attribute.GetCustomAttributes(context.main.GetType()))
        {
            if (attr is PluginCondition condition && (condition.ConditionType & ConditionType.StatusCheck) != 0)
            {
                PluginContext? other = plugins.Find((item) => item.info.name == condition.PluginName);
                if (other == null) continue;
                var otherPcr = GetPluginContextRef(other);
                switch (condition.PluginOrder)
                {
                    default:
                    case PluginOrder.Any: break;
                    case PluginOrder.AfterMe:
                        c.nexts.Add(otherPcr);
                        otherPcr.previous.Add(c);
                        break;
                    case PluginOrder.BeforeMe:
                        c.previous.Add(otherPcr);
                        otherPcr.nexts.Add(c);
                        break;
                }
            }
        }
                
        return c;
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