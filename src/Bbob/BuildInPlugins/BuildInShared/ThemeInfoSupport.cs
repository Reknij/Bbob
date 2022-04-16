namespace Bbob.Main.BuildInPlugin.BuildInShared;

public class ThemeInfoSupport
{
    public string? articleBaseUrl { get; set; }

    public string buildArticleUrl(string address)
    {
        if (articleBaseUrl == null) throw new NullReferenceException("articleBaseUrl is null");
        string result;
        if (articleBaseUrl.Contains("{address}")) result = articleBaseUrl.Replace("{address}", address);
        else result = articleBaseUrl + address;
        return result;
    }
}
