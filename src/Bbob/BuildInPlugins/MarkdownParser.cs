using System.Dynamic;
using System.Text;
using Bbob.Plugin;
using Markdig;
using Markdig.Extensions.Yaml;
using Markdig.Helpers;
using Markdig.Parsers;
using Markdig.Renderers;
using Markdig.Renderers.Html;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;

namespace Bbob.Main.BuildInPlugin;
public class MarkdownParser : IPlugin
{
    MarkdownPipelineBuilder? builder = new MarkdownPipelineBuilder();
    SharpYaml.Serialization.Serializer serializer = new SharpYaml.Serialization.Serializer(new SharpYaml.Serialization.SerializerSettings()
    {
        IgnoreUnmatchedProperties = true
    });
    MarkdownPipeline pipeline;
    public MarkdownParser()
    {
        builder.UseYamlFrontMatter();
        builder.Extensions.AddIfNotAlready<InsertAttributeExtension>(new InsertAttributeExtension("bbob"));
        pipeline = builder.Build();
    }
    public void NewCommand(string filePath, ref string content, NewTypes types = NewTypes.blog)
    {
        switch (types)
        {
            case NewTypes.blog:
                newBlog(filePath, ref content);
                break;

            default:
                break;
        }
    }
    public void GenerateCommand(string filePath, GenerationStage stage)
    {
        switch (stage)
        {
            case GenerationStage.Initialize: parseFile(filePath); break;
            case GenerationStage.Parse: parseMarkdownToHtml(); break;

            default: break;
        }
    }
    private void parseFile(string filePath)
    {
        if (Path.GetExtension(filePath) != ".md") return;

        using (StreamReader sr = new StreamReader(filePath))
        {
            string source = sr.ReadToEnd();

            var doc = Markdown.Parse(source, pipeline);
            string mdString = source;
            YamlFrontMatterBlock? yamlFrontMatterBlock = doc.Descendants<YamlFrontMatterBlock>().FirstOrDefault();
            if (yamlFrontMatterBlock != null)
            {
                string yamlString = source.Substring(yamlFrontMatterBlock.Span.Start, yamlFrontMatterBlock.Span.Length);
                mdString = source.Replace(yamlString, "");
                mdString = mdString.Remove(0, 1); //remove first character '/n'
                PluginHelper.registerObject("markdown", mdString);
                yamlString = yamlString.Remove(0, 3);
                yamlString = yamlString.Remove(yamlString.Length - 3, 3);
                parseLinkInfo(yamlString, filePath);
                doc.Remove(yamlFrontMatterBlock);
            }
            else PluginHelper.ExecutingCommandResult = new CommandResult("Front matter is null or can't parse.", CommandOperation.Skip);
        }
    }

    private void parseLinkInfo(string plain, string filePath)
    {
        try
        {
            Dictionary<string, object> yaml = serializer.Deserialize<Dictionary<string, object>>(plain);
            dynamic article = new ExpandoObject();
            foreach (var y in yaml)
            {
                if (y.Value != null)
                {
                    ((IDictionary<String, Object>)article).Add(y.Key, y.Value);
                }
            }
            PluginHelper.registerObject("article", article);
        }
        catch (System.Exception ex)
        {
            PluginHelper.ExecutingCommandResult = new CommandResult($"Parse front matter error: {ex.Message}", CommandOperation.Skip);
        }
    }
    private void parseMarkdownToHtml()
    {
        try
        {
            if (PluginHelper.getRegisteredObject<string>("markdown", out string? value) && value != null)
            {
                var result = Markdown.Parse(value, pipeline);
                var headingBlocks = result.Descendants<HeadingBlock>();
                Toc toc = new Toc();
                int tocCount = 1;
                foreach (var headingBlock in headingBlocks)
                {
                    if (headingBlock.Inline?.FirstChild != null && headingBlock.Inline.FirstChild is LiteralInline li)
                    {
                        headingBlock.GetAttributes().Id = $"toc-{tocCount++}";
                        string name = li.Content.ToString();
                        string id = headingBlock.GetAttributes().Id ?? throw new NullReferenceException("Get toc id null!");
                        toc.Add(headingBlock.Level, name, id);
                    }
                }
                PluginHelper.modifyRegisteredObject<dynamic>("article", (ref dynamic? article) =>
                {
                    if (article == null) return;
                    article.toc = toc.ToHtml();
                    article.contentParsed = $"<div id='bbob-markdown-content'>{result.ToHtml()}</div>";
                });
            }
        }
        catch (System.Exception ex)
        {
            PluginHelper.ExecutingCommandResult = new CommandResult($"Parse markdown error: {ex.Message}", CommandOperation.Skip);
        }
    }

    private void newBlog(string filePath, ref string content)
    {
        string dateTime = Shared.SharedLib.DateTimeHelper.GetDateTimeNowString();
        string articleName = Path.GetFileNameWithoutExtension(filePath);
        content = $"---\ntitle: {articleName}\ndate: {dateTime}\n---\n# Start writing!";
    }

    private class Toc
    {
        private int lastLevel;
        private string html;
        const string endList = "</ul>";
        TocNumber tocNumber = new TocNumber();
        public Toc()
        {
            lastLevel = 0;
            html = string.Empty;
        }
        public void Add(int currentLevel, string text, string id)
        {
            calcTocNumber(currentLevel);
            string number = $"<span class=\"toc-number\">{tocNumber}</span>";
            string span = $"<span class=\"toc-text\">{text}</span>";
            string a = $"<a href=\"#{id}\">{number}\n{span}</a>";
            string li = $"<li class=\"toc-item toc-item-level-{currentLevel}\">{a}\n</li>\n";
            if (currentLevel > lastLevel)
            {
                html += $"<ul class=\"toc-list\">{li}\n";
            }
            else if (currentLevel == lastLevel)
            {
                html += li;
            }
            else if (currentLevel < lastLevel)
            {
                html += $"</uL>{li}";
            }
            lastLevel = currentLevel;
        }
        public string ToHtml()
        {
            return string.IsNullOrEmpty(html) ? $"<ul>{endList}" : html + endList;
        }
        private void calcTocNumber(int currentLevel)
        {
            if (currentLevel == lastLevel || lastLevel == 0)
            {
                tocNumber.Current++;
            }
            else if (currentLevel > lastLevel)
            {
                tocNumber.Child = new TocNumber(1, tocNumber);
                tocNumber = tocNumber.Child;
            }
            else if (currentLevel < lastLevel)
            {
                if (tocNumber.Parent != null)
                {
                    tocNumber = tocNumber.Parent;
                }
                tocNumber.Child = null;
                tocNumber.Current++;
            }
        }

        private class TocNumber
        {
            public int Current { get; set; }
            public TocNumber? Parent { get; set; }
            public TocNumber? Child { get; set; }

            public TocNumber(int current = 0, TocNumber? parent = null, TocNumber? child = null)
            {
                Current = current;
                Parent = parent;
                Child = child;
            }

            public override string ToString()
            {
                string text = $"{this.Current}.";
                var p = Parent;
                while (p != null)
                {
                    text = $"{p.Current}.{text}";
                    p = p.Parent;
                }
                return text;
            }
        }
    }

    public class LinkInfo
    {
        public string? title { get; set; }
        public string? date { get; set; }

        public string[]? categories { get; set; }
        public string[]? tags { get; set; }
        public string? address { get; set; }
        public LinkInfo(string filePath, string? title = null, string? date = null, string[]? categories = null, string[]? tags = null, string? address = null)
        {
            this.title = title;
            this.date = date;
            this.categories = categories;
            this.tags = tags;
            this.address = address;
        }

        public LinkInfo()
        {
            title = date = address = null;
            this.categories = this.tags = null;
            //all must be null when deserializing yaml, otherwise an error will be thrown.
        }
    }

    private class InsertAttributeExtension : Markdig.IMarkdownExtension
    {
        string header;
        public InsertAttributeExtension(string header)
        {
            this.header = header;
        }
        public void Setup(MarkdownPipelineBuilder pipeline)
        {
            pipeline.BlockParsers.ForEach((BlockParser blockParser) =>
            {
                blockParser.Closed += (BlockProcessor processor, Block block) =>
                {
                    switch (block)
                    {
                        case HeadingBlock headingBlock:
                            headingBlock.GetAttributes().AddClass($"{header}-h{headingBlock.Level}");
                            break;
                        case ParagraphBlock paragraphBlock:
                            paragraphBlock.GetAttributes().AddClass($"{header}-p");
                            break;
                        case ListBlock listBlock:
                            string listName = listBlock.IsOrdered ? "ordered" : "unordered";
                            listBlock.GetAttributes().AddClass($"{header}-list-{listName}");
                            break;
                        case CodeBlock codeBlock:
                            codeBlock.GetAttributes().AddClass($"{header}-code");
                            break;
                        case QuoteBlock quoteBlock:
                            quoteBlock.GetAttributes().AddClass($"{header}-quote");
                            break;

                        default: break;
                    }
                };
            });
        }

        public void Setup(MarkdownPipeline pipeline, IMarkdownRenderer renderer) { }
    }

}