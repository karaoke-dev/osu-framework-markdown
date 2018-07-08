using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using Markdig;
using Markdig.Extensions.AutoIdentifiers;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;
using Microsoft.CodeAnalysis.Text;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Testing;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Animations;
using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Graphics.Textures;
using osu.Framework.Allocation;

namespace osu.Framework.Markdown.Tests.Visual
{
    [System.ComponentModel.Description("markdown reader")]
    public class TestCaseMarkdown : TestCase
    {
        public TestCaseMarkdown()
        {
            MarkdownContainer markdownContainer;
            Add(markdownContainer = new MarkdownContainer
            {
                RelativeSizeAxes = Axes.Both,
            });

            AddStep("Markdown Heading", () =>
            {
                markdownContainer.MarkdownText = @"# Header 1
                ## Header 2
                ### Header 3
                #### Header 4
                ##### Header 5";
            });

            AddStep("Markdown Seperator", () =>
            {
                markdownContainer.MarkdownText = @"# Language

";
            });

            AddStep("Markdown Heading", () =>
            {
                markdownContainer.MarkdownText = @"- [1. Blocks](#1-blocks)
  - [1.1 Code block](#11-code-block)
  - [1.2 Text block](#12-text-block)
  - [1.3 Escape block](#13-escape-block)
  - [1.4 Whitespace control](#14-whitespace-control)
- [2 Comments](#2-comments)
- [3 Literals](#3-literals)
  - [3.1 Strings](#31-strings)
  - [3.2 Numbers](#32-numbers)
  - [3.3 Boolean](#33-boolean)
  - [3.4 null](#34-null)";
            });

            AddStep("Markdown Quote", () =>
            {
                markdownContainer.MarkdownText = @"> **input**";
            });

            AddStep("Markdown Fenced Code", () =>
            {
                markdownContainer.MarkdownText = @"```scriban-html
{{
  x = ""5""   # This assignment will not output anything
  x         # This expression will print 5
  x + 1     # This expression will print 6
}}
```";
            });

            AddStep("Markdown Paragraph 1", () =>
            {
                markdownContainer.MarkdownText = @"A text enclosed by `{{` and `}}` is a scriban **code block** that will be evaluated by the scriban templating engine.";
            });

            AddStep("Markdown Paragraph 2", () =>
            {
                markdownContainer.MarkdownText =
                    @"The greedy mode using the character - (e.g {{- or -}}), removes any whitespace, including newlines Examples with the variable name = ""foo"":";
            });

            AddStep("MarkdownImage", () =>
            {
                markdownContainer.MarkdownText = @"![Drag Racing](https://www.wonderplugin.com/videos/demo-image0.jpg)
![Drag Racing](https://www.wonderplugin.com/videos/demo-image0.jpg)
![Drag Racing](https://www.wonderplugin.com/videos/demo-image0.jpg)
![Drag Racing](https://www.wonderplugin.com/videos/demo-image0.jpg)
![Drag Racing](https://www.wonderplugin.com/videos/demo-image0.jpg)";
            });

            AddStep("MarkdownFromInternet", () =>
            {
                try
                {
                    //test readme in https://github.com/lunet-io/scriban/blob/master/doc/language.md#92-if-expression-else-else-if-expression
                    var url = "https://raw.githubusercontent.com/lunet-io/scriban/master/doc/language.md";
                    var httpClient = new HttpClient();
                    markdownContainer.MarkdownText = httpClient.GetStringAsync(url).ConfigureAwait(false).GetAwaiter().GetResult();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }

            });
        }
    }

    /// <summary>
    /// Contains all the markdown component <see cref="IMarkdownObject" /> in <see cref="MarkdownDocument" />
    /// </summary>
    public class MarkdownContainer : ScrollContainer
    {
        public MarkdownDocument MarkdownDocument
        {
            get => document;
            set
            {
                document = value;
                //clear all exist markdown object and re-create them
                markdownContainer.Clear();
                foreach (var component in document)
                    AddMarkdownComponent(component, markdownContainer, RootLayerIndex);
            }
        }

        public string MarkdownText
        {
            set
            {
                var markdownText = value;
                var pipeline = new MarkdownPipelineBuilder().UseAutoIdentifiers(AutoIdentifierOptions.GitHub).Build();
                MarkdownDocument = Markdig.Markdown.Parse(markdownText, pipeline);
            }
        }

        private const int RootLayerIndex = 0;
        private const int SeperatorPx = 25;
        private MarkdownDocument document;
        private readonly FillFlowContainer markdownContainer;

        public MarkdownContainer()
        {
            ScrollbarOverlapsContent = false;
            Child =  markdownContainer = new FillFlowContainer
            {
                Padding = new MarginPadding{Left = 10, Right = 30},
                Margin = new MarginPadding { Left = 10, Right = 30 },
                AutoSizeAxes = Axes.Y,
                RelativeSizeAxes = Axes.X,
                Direction = FillDirection.Vertical,
                Spacing = new Vector2(SeperatorPx)
            };
        }

        protected void AddMarkdownComponent(IMarkdownObject markdownObject, FillFlowContainer container, int layerIndex)
        {
            if (markdownObject is HeadingBlock headingBlock)
            {
                container.Add(new MarkdownHeading(headingBlock));
            }
            else if (markdownObject is LiteralInline literalInline)
            {
                container.Add(new MarkdownSeperator(literalInline));
            }
            else if (markdownObject is ParagraphBlock paragraphBlock)
            {
                var drawableParagraphBlock = new MarkdownTextFlowContainer();
                switch (layerIndex)
                {
                    case 1:
                        drawableParagraphBlock.AddText("@ ", t => t.Colour = Color4.DarkGray);
                        break;
                    case 2:
                        drawableParagraphBlock.AddText("# ", t => t.Colour = Color4.DarkGray);
                        break;
                    case 3:
                    case 4:
                        drawableParagraphBlock.AddText("+ ", t => t.Colour = Color4.DarkGray);
                        break;
                }

                drawableParagraphBlock = ParagraphBlockHelper.GeneratePartial(drawableParagraphBlock, paragraphBlock.Inline);
                container.Add(drawableParagraphBlock);
            }
            else if (markdownObject is QuoteBlock quoteBlock)
            {
                container.Add(new MarkdownQuoteBlock(quoteBlock));
            }
            else if (markdownObject is FencedCodeBlock fencedCodeBlock)
            {
                container.Add(new MarkdownFencedCodeBlock(fencedCodeBlock));
            }
            else if (markdownObject is ListBlock listBlock)
            {
                var childContainer = new FillFlowContainer()
                {
                    Direction = FillDirection.Vertical,
                    Spacing = new Vector2(10, 10),
                    Padding = new MarginPadding() { Left = 25, Right = 5 },
                    AutoSizeAxes = Axes.Y,
                    RelativeSizeAxes = Axes.X,
                };
                container.Add(childContainer);
                foreach (var single in listBlock)
                {
                    AddMarkdownComponent(single, childContainer, layerIndex + 1);
                }
            }
            else if (markdownObject is ListItemBlock listItemBlock)
            {
                foreach (var single in listItemBlock)
                {
                    AddMarkdownComponent(single, container, layerIndex);
                }
            }
            else
            {
                container.Add(new NotExistingMarkdown(markdownObject));
            }

            //show seperator line
            if (markdownObject is LeafBlock leafBlock && !(markdownObject is ParagraphBlock))
            {
                if (leafBlock.Inline != null)
                {
                    container.Add(new MarkdownSeperator(null));
                }
            }
        }
    }

    /// <summary>
    /// NotExistMarkdown :
    /// shows the <see cref="IMarkdownObject" /> does not implement in drawable object
    /// </summary>
    internal class NotExistingMarkdown : SpriteText
    {
        public NotExistingMarkdown(IMarkdownObject markdownObject)
        {
            Colour = new Color4(255, 0, 0, 255);
            TextSize = 21;
            Text = markdownObject?.GetType() + " Not implemented.";
        }
    }

    /// <summary>
    /// MarkdownFencedCodeBlock :
    /// ```
    /// foo
    /// ```
    /// </summary>
    internal class MarkdownFencedCodeBlock : Container
    {
        private readonly MarkdownTextFlowContainer textFlowContainer;

        public MarkdownFencedCodeBlock(FencedCodeBlock fencedCodeBlock)
        {
            AutoSizeAxes = Axes.Y;
            RelativeSizeAxes = Axes.X;
            Children = new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = Color4.Gray,
                    Alpha = 0.5f
                },
                textFlowContainer = new MarkdownTextFlowContainer
                {
                    Margin = new MarginPadding { Left = 10, Right = 10, Top = 10, Bottom = 10 },
                }
            };

            var lines = fencedCodeBlock.Lines.Lines.Take(fencedCodeBlock.Lines.Count);
            foreach (var sligneLine in lines)
            {
                var lineString = sligneLine.ToString();
                textFlowContainer.AddParagraph(lineString);
            }
        }
    }

    /// <summary>
    /// MarkdownHeading :
    /// #Heading1
    /// ##Heading2
    /// ###Heading3
    /// ###3Heading4
    /// </summary>
    internal class MarkdownHeading : Container
    {
        private readonly MarkdownTextFlowContainer textFlowContainer;

        public MarkdownHeading(HeadingBlock headingBlock)
        {
            AutoSizeAxes = Axes.Y;
            RelativeSizeAxes = Axes.X;

            Children = new Drawable[]
            {
                textFlowContainer = new MarkdownTextFlowContainer()
            };

            var level = headingBlock.Level;
            Vector2 scale = new Vector2(1);

            switch (level)
            {
                case 1:
                    scale = new Vector2(2.7f);
                    break;
                case 2:
                    scale = new Vector2(2);
                    break;
                case 3:
                    scale = new Vector2(1.5f);
                    break;
                case 4:
                    scale = new Vector2(1.3f);
                    break;
            }

            textFlowContainer.Scale = scale;
            textFlowContainer = ParagraphBlockHelper.GeneratePartial(textFlowContainer, headingBlock.Inline);
        }
    }

    /// <summary>
    /// MarkdownQuoteBlock :
    /// > NOTE: This document does not describe the `liquid` language.
    /// </summary>
    internal class MarkdownQuoteBlock : Container
    {
        private readonly MarkdownTextFlowContainer textFlowContainer;
        private Box quoteBox;

        public MarkdownQuoteBlock(QuoteBlock quoteBlock)
        {
            AutoSizeAxes = Axes.Y;
            RelativeSizeAxes = Axes.X;

            Children = new Drawable[]
            {
                quoteBox = new Box
                {
                    Colour = Color4.Gray,
                    Width = 5,
                    Anchor = Anchor.CentreLeft,
                    Origin = Anchor.CentreLeft,
                    RelativeSizeAxes = Axes.Y
                },
                textFlowContainer = new MarkdownTextFlowContainer
                {
                    Margin = new MarginPadding { Left = 20 }
                }
            };

            if (quoteBlock.LastChild is ParagraphBlock paragraphBlock)
                textFlowContainer = ParagraphBlockHelper.GeneratePartial(textFlowContainer, paragraphBlock.Inline);
        }
    }

    /// <summary>
    /// MarkdownSeperator :
    /// (spacing)
    /// </summary>
    internal class MarkdownSeperator : Box
    {
        public MarkdownSeperator(LiteralInline literalInline)
        {
            RelativeSizeAxes = Axes.X;
            Height = 1;
            Colour = Color4.Gray;
        }
    }

    /// <summary>
    /// Fill <see cref="Inline" /> into <see cref="TextFlowContainer" />
    /// </summary>
    internal static class ParagraphBlockHelper
    {
        public static MarkdownTextFlowContainer GenerateText(ParagraphBlock paragraphBlock)
        {
            var textFlowContainer = new MarkdownTextFlowContainer();
            GeneratePartial(textFlowContainer, paragraphBlock.Inline);
            return textFlowContainer;
        }

        public static MarkdownTextFlowContainer GeneratePartial(MarkdownTextFlowContainer textFlowContainer, ContainerInline lnline)
        {
            foreach (var single in lnline)
            {
                if (single is LiteralInline literalInline)
                {
                    var text = literalInline.Content.ToString();
                    if (lnline.GetNext(literalInline) is HtmlInline
                        && lnline.GetPrevious(literalInline) is HtmlInline htmlInline)
                        textFlowContainer.AddText(text, t => t.Colour = Color4.MediumPurple);
                    else if (lnline.GetNext(literalInline) is HtmlEntityInline htmlEntityInline)
                        textFlowContainer.AddText(text, t => t.Colour = Color4.GreenYellow);
                    else if (literalInline.Parent is LinkInline linkInline)
                    {
                        if (!linkInline.IsImage)
                            textFlowContainer.AddText(text, t => t.Colour = Color4.DodgerBlue);
                    }
                    else
                        textFlowContainer.AddText(text);
                }
                else if (single is CodeInline codeInline)
                {
                    textFlowContainer.AddText(codeInline.Content, t => t.Colour = Color4.Orange);
                }
                else if (single is EmphasisInline emphasisInline)
                {
                    //foreach (var child in emphasisInline)
                    //{
                    //    textFlowContainer.AddText(child.ToString());
                    //}
                }
                else if (single is LinkInline linkInline)
                {
                    if (linkInline.IsImage)
                    {
                        var imageUrl = linkInline.Url;
                        //insert a image
                        textFlowContainer.AddImage(new MarkdownImage(imageUrl)
                        {
                            Width = 300,
                            Height = 300,
                        });
                    }
                }
                else if (single is HtmlInline || single is HtmlEntityInline)
                {
                    //DO nothing
                }
                else if (single is LineBreakInline)
                {
                    //IDK what is this but just ignore
                }
                else
                {
                    textFlowContainer.AddText(single.GetType() + " Not implemented.", t => t.Colour = Color4.Red);
                }

                //generate child
                if (single is ContainerInline containerInline) GeneratePartial(textFlowContainer, containerInline);
            }

            return textFlowContainer;
        }
    }

    /// <summary>
    /// Load image from url
    /// </summary>
    internal class MarkdownImage : Container
    {
        private readonly string imageUrl;

        public MarkdownImage(string url)
        {
            imageUrl = url;
        }

        [BackgroundDependencyLoader]
        private void load(TextureStore textures)
        {
            Texture texture = null;
            if (!string.IsNullOrEmpty(imageUrl))
                texture = textures.Get(imageUrl);

            //TODO : get default texture
            //if (texture == null)
            //    texture = textures.Get(@"Online/avatar-guest");

            Add(new Sprite
            {
                RelativeSizeAxes = Axes.Both,
                Texture = texture,
                FillMode = FillMode.Fit,
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre
            });
        }
    }

    /// <summary>
    /// Markdown text flow container.
    /// </summary>
    internal class MarkdownTextFlowContainer : CustomizableTextContainer
    {
        public MarkdownTextFlowContainer()
        {
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;
        }

        public IEnumerable<SpriteText> AddImage(MarkdownImage image)
        {
            var imageIndex = AddPlaceholder(image);
            return base.AddText("[" + imageIndex + "]");
        }

        public new IEnumerable<SpriteText> AddText(string text, Action<SpriteText> creationParameters = null)
        {
            text = text.Replace("[", "[[").Replace("]", "]]");
            return base.AddText(text, creationParameters);
        }

        public new IEnumerable<SpriteText> AddParagraph(string text, Action<SpriteText> creationParameters = null)
        {
            text = text.Replace("[", "[[").Replace("]", "]]");
            return base.AddParagraph(text, creationParameters);
        }
    }

    /// <summary>
    /// List extension
    /// </summary>
    internal static class ListExtension
    {
        public static T GetNext<T>(this IEnumerable<T> guidList, T current)
        {
            return guidList.SkipWhile(i => !i.Equals(current)).Skip(1).FirstOrDefault();
        }

        public static T GetPrevious<T>(this IEnumerable<T> guidList, T current)
        {
            return guidList.TakeWhile(i => !i.Equals(current)).LastOrDefault();
        }
    }
}
