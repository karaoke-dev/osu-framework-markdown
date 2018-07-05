using System;
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
            else if(markdownObject is LiteralInline literalInline)
            {
                Add(new MarkdownSeperator(literalInline));
            }
            else if(markdownObject is ParagraphBlock paragraphBlock)
            {
                var drawableParagraphBlock = ParagraphBlockHelper.GenerateText(paragraphBlock);
                drawableParagraphBlock.RelativeSizeAxes = Axes.X;
                drawableParagraphBlock.AutoSizeAxes = Axes.Y;
                Add(drawableParagraphBlock);
                
            }
            else if(markdownObject is QuoteBlock quoteBlock)
            {
                Add(new MarkdownQuoteBlock(quoteBlock));
            }
            else if(markdownObject is ListBlock listBlock)
            {
                 Add(new MarkdownListBlock(listBlock));
            }
            else
            {
                Add(new NotExistMarkdown(markdownObject));
            }

            //show child object
            if (markdownObject is LeafBlock leafBlock)
            {
                if (leafBlock.Inline != null)
                {
                    foreach (var single in leafBlock.Inline)
                    {
                        AddMarkdownComponent(single);
                    }
                }
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
        public MarkdownListBlock(ListBlock listBlock)
        {
            Direction = FillDirection.Vertical;
            AutoSizeAxes = Axes.Y;
            RelativeSizeAxes = Axes.X;
            Style = listBlock;
        }

        private ListBlock _listBlock;
        public virtual ListBlock Style
        {
            get => _listBlock;
            set
            {
                _listBlock = value;

                int rootLayerIndex = 0;
                CreateLayer(_listBlock,rootLayerIndex);
                void CreateLayer(ListBlock listBlock,int layerIndex)
                {
                    foreach (var singleBlock in listBlock)
                    {
                        //TODO : singleBlock has two child
                        //[0] : 1. Blocks
                        //[1] : 1.1 Code block
                        //      1.2 Text block
                        //      1.3 Escape block
                        //      1.4 Whitespace control

                        if (singleBlock is ListItemBlock listitemBlock)
                        {
                            foreach (var block in listitemBlock)
                            {
                                if (block is ParagraphBlock paragraphBlock)
                                {
                                    var drawableParagraphBlock = ParagraphBlockHelper.GenerateText(paragraphBlock);
                                    drawableParagraphBlock.Margin = new MarginPadding(){Left = 20 * layerIndex};
                                    drawableParagraphBlock.RelativeSizeAxes = Axes.X;
                                    drawableParagraphBlock.AutoSizeAxes = Axes.Y;
                                    Add(drawableParagraphBlock);
                                }
                                else if(block is ListBlock listBlock2)
                                {
                                    CreateLayer(listBlock2, layerIndex + 1);
                                }
                            }
                        }
                    }
                   
                }
            }
        }
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
    public class MarkdownQuoteBlock : Container
    {
        private TextFlowContainer _text;
        private Box _quoteBox;
        public MarkdownQuoteBlock()
        {
            //Direction = FillDirection.Horizontal;
            //Spacing = new Vector2(10);
            AutoSizeAxes = Axes.Y;
            RelativeSizeAxes = Axes.X;
            Children = new Drawable[]
            {
                _quoteBox = new Box()
                {
                    Colour = Color4.Gray,
                    Width = 5,
                    Anchor = Anchor.CentreLeft,
                    Origin = Anchor.CentreLeft,
                    RelativeSizeAxes = Axes.Y,
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
                    _text = ParagraphBlockHelper.GenerateText(ParagraphBlock);
                    _text.Margin = new MarginPadding(){Left = 20};
                    _text.RelativeSizeAxes = Axes.X;
                    _text.AutoSizeAxes = Axes.Y;
                    Add(_text);
                }
            }
        }
    }

    #region MarkdownSeperator.cs

    /// <summary>
    /// MarkdownSeperator
    /// 
    /// </summary>
    public class MarkdownSeperator : Box
    {
        public MarkdownSeperator(LiteralInline ParagraphBlock)
        {
            Style = ParagraphBlock;
            RelativeSizeAxes = Axes.X;
            Height = 1;
            Colour = Color4.Gray;
        }

        private LiteralInline _literalInLine;
        public virtual LiteralInline Style
        {
            get => _literalInLine;
            set
            {
                _literalInLine = value;
            }
        }
    }

    #endregion

    #region ParagraphBlockHelper.cs

    public static class ParagraphBlockHelper
    {
        public static TextFlowContainer GenerateText(ParagraphBlock paragraphBlock)
        {
             TextFlowContainer TextFlowContainer = new TextFlowContainer();
             foreach (var single in paragraphBlock.Inline)
             {
                if (single is LiteralInline literalInline)
                {
                    TextFlowContainer.AddText(literalInline.Content.ToString());
                }
                else if (single is CodeInline codeInline)
                {
                    TextFlowContainer.AddText(codeInline.Content);
                }
                else if (single is EmphasisInline emphasisInline)
                {
                    foreach (var child in emphasisInline)
                    {
                        TextFlowContainer.AddText(child.ToString());
                    }
                }
                else if(single is LinkInline linkInline)
                {
                    var url = linkInline.Url;
                    if (linkInline.FirstChild is CodeInline codeInline2)
                    {
                        TextFlowContainer.AddParagraph(codeInline2.Content, t => t.Colour = Color4.LightBlue);
                    }
                    else if(linkInline.FirstChild is LiteralInline literalInline2)
                    {
                        TextFlowContainer.AddParagraph(literalInline2.Content.ToString(), t => t.Colour = Color4.LightBlue);
                    }
                    else
                    {
                        TextFlowContainer.AddText(single.GetType() + " does not containe" 
                            + linkInline.FirstChild.GetType(), t => t.Colour = Color4.Red);
                    }
                }
                else
                {
                    TextFlowContainer.AddText(single.GetType().ToString(), t => t.Colour = Color4.Red);
                }
            }
            return TextFlowContainer;
        }
    }

    #endregion

    
}
