using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using Markdig;
using Markdig.Extensions.AutoIdentifiers;
using Markdig.Syntax;
using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Framework.Markdown.Graphics.Containers;
using osu.Framework.Markdown.Graphics.Containers.Component;
using osu.Framework.Testing;

namespace osu.Framework.Markdown.Tests.Component
{
    /// <summary>
    /// Base testcase
    /// T is used to filter specific type of object from markdown
    /// </summary>
    /// <typeparam name="T"></typeparam>
    //public abstract class TestCaseMarkdownComponent<T> : TestCase where T : ContainerBlock
    //{
    //    protected MarkdownContainer MarkdownContainer;
    //    protected List<T> Markdowns;

    //    static void Error(string message)
    //    {
    //        Console.WriteLine(message);
    //        //Environment.Exit(1);
    //    }

    //    public TestCaseMarkdownComponent()
    //    {
           
    //    }

    //    public virtual void InitializeComponentView()
    //    {
    //        MarkdownContainer = new MarkdownContainer()
    //        {
    //            AutoSizeAxes = Axes.Both,
    //        };

    //        foreach (var head in headings)
    //        {
    //            container.Add(new MarkdownQuoteBlock(head));
    //        }

    //        Add(container);
    //    }

    //    public virtual void InitializeMarkdown()
    //    {
    //        var path = "https://github.com/lunet-io/scriban/blob/master/doc/language.md";
    //        string markdown = null;
    //        if (path.StartsWith("https:"))
    //        {
    //            Uri uri;
    //            if (!Uri.TryCreate(path, UriKind.Absolute, out uri))
    //            {
    //                Error($"Unable to parse Uri `{path}`");
    //                return;
    //            }
    //            // Special handling of github URL to access the raw content instead
    //            if (uri.Host == "github.com")
    //            {
    //                // https://github.com/lunet-io/scriban/blob/master/doc/language.md
    //                // https://raw.githubusercontent.com/lunet-io/scriban/master/doc/language.md
    //                var newPath = uri.AbsolutePath;
    //                var paths = new List<string>(newPath.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries));
    //                if (paths.Count < 5 || paths[2] != "blob")
    //                {
    //                    Error($"Invalid github.com URL `{path}`");
    //                    return;
    //                }
    //                paths.RemoveAt(2); // remove blob
    //                uri = new Uri($"https://raw.githubusercontent.com/{(string.Join("/", paths))}");
    //            }

    //            var httpClient = new HttpClient();
    //            markdown = httpClient.GetStringAsync(uri).ConfigureAwait(false).GetAwaiter().GetResult();
    //        }
    //        else
    //        {
    //            markdown = File.ReadAllText(path);
    //        }

    //        var pipeline = new MarkdownPipelineBuilder().UseAutoIdentifiers(AutoIdentifierOptions.GitHub).Build();
    //        var doc = Markdig.Markdown.Parse(markdown, pipeline);

    //        //var headings = doc.Descendants<HeadingBlock>().Take(20).ToList();
    //        //var headings = doc.Descendants<ParagraphBlock>().Take(20).ToList();
    //        Markdowns = doc.Descendants<T>().Take(20).ToList();
    //    }
    //}
}
