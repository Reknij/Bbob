using System.Text.Json.Serialization;

namespace Bbob.Plugin;

public class LinkInfo
{
    public string? title { get; set; }
    public string? date { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string[]? categories { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string[]? tags { get; set; }
    public string? address { get; set; }

    private string? filePath { get; set; }
    public string getFilePath() => filePath ?? throw new NullReferenceException("filePath is null");
    public void setFilePath(string value) => filePath = value;
    public LinkInfo(string filePath, string? title = null, string? date = null, string[]? categories = null, string[]? tags = null, string? address = null)
    {
        this.title = title;
        this.date = date;
        this.categories = categories;
        this.tags = tags;
        this.address = address;
        this.filePath = filePath;
    }

    public LinkInfo()
    {
        title = date = address = null;
        this.filePath = string.Empty;
        this.categories = this.tags = null;
        //all must be null when deserializing yaml, otherwise an error will be thrown.
    }
}