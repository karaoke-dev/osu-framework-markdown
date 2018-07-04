﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using Markdig;
using Markdig.Extensions.AutoIdentifiers;
using Markdig.Syntax;
using Microsoft.CodeAnalysis.Text;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Testing;
using osu.Framework.Graphics.Shapes;
using Markdig.Syntax.Inlines;
using OpenTK;
using OpenTK.Graphics;

namespace osu.Framework.Markdown.Tests.Visual
{
    [System.ComponentModel.Description("frame-based animations")]
    public class TestCaseMarkdown : TestCase
    {
        static void Error(string message)
        {
            Console.WriteLine(message);
            Environment.Exit(1);
        }

        public TestCaseMarkdown()
        {
            var path = "https://github.com/lunet-io/scriban/blob/master/doc/language.md";
            string markdown = null;
            if (path.StartsWith("https:"))
            {
                Uri uri;
                if (!Uri.TryCreate(path, UriKind.Absolute, out uri))
                {
                    Error($"Unable to parse Uri `{path}`");
                    return;
                }
                // Special handling of github URL to access the raw content instead
                if (uri.Host == "github.com")
                {
                    // https://github.com/lunet-io/scriban/blob/master/doc/language.md
                    // https://raw.githubusercontent.com/lunet-io/scriban/master/doc/language.md
                    var newPath = uri.AbsolutePath;
                    var paths = new List<string>(newPath.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries));
                    if (paths.Count < 5 || paths[2] != "blob")
                    {
                        Error($"Invalid github.com URL `{path}`");
                        return;
                    }
                    paths.RemoveAt(2); // remove blob
                    uri = new Uri($"https://raw.githubusercontent.com/{(string.Join("/", paths))}");
                }

                var httpClient = new HttpClient();
                markdown = httpClient.GetStringAsync(uri).ConfigureAwait(false).GetAwaiter().GetResult();
            }
            else
            {
                markdown = File.ReadAllText(path);
            }

            var pipeline = new MarkdownPipelineBuilder().UseAutoIdentifiers(AutoIdentifierOptions.GitHub).Build();
            var doc = Markdig.Markdown.Parse(markdown, pipeline);

            //var headings = doc.Descendants<HeadingBlock>().Take(20).ToList();
            //var headings = doc.Descendants<ParagraphBlock>().Take(20).ToList();
            //var headings = doc.Descendants<QuoteBlock>().Take(20).ToList();

            var container = new MarkdownScrollView()
            {
                RelativeSizeAxes = Axes.Both,
            };

            container.ImportMarkdownDocument(doc);
            

            Add(container);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class MarkdownScrollView : ScrollContainer
    {
        private MarkdownContainer _markdownContainer;

        public MarkdownScrollView()
        {
            ScrollbarOverlapsContent = false;
            Child = _markdownContainer = new MarkdownContainer()
            {
                Padding = new MarginPadding(3),
                AutoSizeAxes = Axes.Y,
                RelativeSizeAxes = Axes.X,
            };
        }

        public void ImportMarkdownDocument(MarkdownDocument document)
        {
            _markdownContainer.ImportMarkdownDocument(document);
        }
    }

    /// <summary>
    /// MarkdownContainer
    /// </summary>
    public class MarkdownContainer : FillFlowContainer
    {
        public MarkdownContainer()
        {
            Direction = FillDirection.Vertical;
            Spacing = new OpenTK.Vector2(20, 20);
            Margin = new MarginPadding(){Left = 20,Right = 20};
        }

        public void ImportMarkdownDocument(MarkdownDocument document)
        {
            foreach (var component in document)
            {
                AddMarkdownComponent(component);
            }
        }

        public void AddMarkdownComponent(IMarkdownObject markdownObject)
        {
            if(markdownObject is HeadingBlock headingBlock)
            {
                Add(new MarkdownHeading(headingBlock));
            }
            else if(markdownObject is ParagraphBlock paragraphBlock)
            {
                Add(new MarkdownSeperator(paragraphBlock));
            }
            else if(markdownObject is QuoteBlock quoteBlock)
            {
                Add(new MarkdownQuoteBlock(quoteBlock));
            }
            else
            {
                Add(new NotExistMarkdown(markdownObject));
            }
        }
    }

    /// <summary>
    /// NotExistMarkdown : 
    /// shows the markdown syntex that not exist in drawable
    /// </summary>
    public class NotExistMarkdown : SpriteText
    {
        public NotExistMarkdown(IMarkdownObject markdownObject)
        {
            Style = markdownObject;
            this.Colour = new Color4(255,0,0,255);
            this.TextSize = 21;
        }

        private IMarkdownObject _markdownObject;
        public virtual IMarkdownObject Style
        {
            get => _markdownObject;
            set
            {
                _markdownObject = value;
                this.Text = "Does not found : " + _markdownObject?.GetType()?.Name + " ,Name : "+ _markdownObject?.ToString();
            }
        }
    }

    /// <summary>
    /// NotExistMarkdown : 
    /// - [1. Blocks](#1-blocks)
    ///     - [1.1 Code block](#11-code-block)
    ///     - [1.2 Text block](#12-text-block)
    ///     - [1.3 Escape block](#13-escape-block)
    ///     - [1.4 Whitespace control](#14-whitespace-control)
    /// </summary>
    public class MarkdownListBlock : FillFlowContainer
    {

    }

    /// <summary>
    /// MarkdownHeading
    /// </summary>
    public class MarkdownHeading : SpriteText
    {
        public MarkdownHeading()
        {

        }

        public MarkdownHeading(HeadingBlock headingBlock)
        {
            Style = headingBlock;
        }

        private HeadingBlock _headingBlock;
        public virtual HeadingBlock Style
        {
            get => _headingBlock;
            set
            {
                _headingBlock = value;
                var level = _headingBlock.Level;
                Text = _headingBlock.Inline.FirstChild.ToString();

                switch (level)
                {
                    case 1:
                        this.TextSize = 50;
                        break;
                    case 2:
                        this.TextSize = 38;
                        break;
                    case 3:
                        this.TextSize = 21;
                        break;
                    case 4:
                        this.TextSize = 21;
                        break;
                    case 5:
                        this.TextSize = 10;
                        break;
                    default:
                        this.TextSize = 10;
                        return;
                }
            }
        }
    }

    /// <summary>
    /// MarkdownQuoteBlock
    /// > NOTE: This document does not describe the `liquid` language. Check the [`liquid website`](https://shopify.github.io/liquid/) directly.
    /// </summary>
    public class MarkdownQuoteBlock : FillFlowContainer
    {
        private TextFlowContainer _text;
        private Box _quoteBox;
        public MarkdownQuoteBlock()
        {
            Direction = FillDirection.Horizontal;
            AutoSizeAxes = Axes.Y;
            RelativeSizeAxes = Axes.X;
            Spacing = new Vector2(10);
            Children = new Drawable[]
            {
                _quoteBox = new Box()
                {
                    Colour = Color4.Gray,
                    Width = 5,
                    Anchor = Anchor.CentreLeft,
                    Origin = Anchor.CentreLeft,
                },
                _text = new TextFlowContainer()
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                },
            };
        }

        public MarkdownQuoteBlock(QuoteBlock quoteBlock) : this()
        {
            Style = quoteBlock;
        }

        private QuoteBlock _quoteBlock;
        public virtual QuoteBlock Style
        {
            get => _quoteBlock;
            set
            {
                _quoteBlock = value;

                if (_quoteBlock.LastChild is ParagraphBlock ParagraphBlock)
                {
                    string content = "";
                    //TODO : get text from single
                    foreach (var single in ParagraphBlock.Inline)
                    {
                        if (single is LiteralInline literalInline)
                        {
                            content = content + literalInline.Content.ToString();
                        }
                        else if (single is CodeInline codeInline)
                        {
                            content = content + codeInline.Content;
                        }
                        else if (single is EmphasisInline emphasisInline)
                        {
                            foreach (var child in emphasisInline)
                            {
                                content = content + child;
                            }
                        }
                        else
                        {
                            //var content = single.Content;
                            //_text.Text = content.Text.Substring(content.Start, content.Length);
                            content = content + single.GetType();
                        }

                        content = content + " ";
                    }
                    _text.AddText(content);
                }
            }
        }
    }

    /// <summary>
    /// MarkdownSeperator
    /// 
    /// </summary>
    public class MarkdownSeperator : Box
    {
        public MarkdownSeperator(ParagraphBlock ParagraphBlock)
        {
            Style = ParagraphBlock;
            RelativeSizeAxes = Axes.X;
            Height = 1;
            Colour = Color4.Gray;
        }

        private ParagraphBlock _paragraphBlock;
        public virtual ParagraphBlock Style
        {
            get => _paragraphBlock;
            set
            {
                _paragraphBlock = value;
            }
        }
    }
}
