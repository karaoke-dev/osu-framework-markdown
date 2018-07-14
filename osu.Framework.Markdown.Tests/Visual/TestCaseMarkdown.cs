﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using Markdig;
using Markdig.Extensions.AutoIdentifiers;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;
using Markdig.Extensions.Tables;
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
using osu.Framework.Graphics.Effects;

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
                markdownContainer.Text = @"# Header 1
## Header 2
### Header 3
#### Header 4
##### Header 5";
            });

            AddStep("Markdown Seperator", () =>
            {
                markdownContainer.Text = @"# Language

";
            });

            AddStep("Markdown Heading", () =>
            {
                markdownContainer.Text = @"- [1. Blocks](#1-blocks)
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
                markdownContainer.Text = @"> **input**";
            });

            AddStep("Markdown Fenced Code", () =>
            {
                markdownContainer.Text = @"```scriban-html
{{
  x = ""5""   # This assignment will not output anything
  x         # This expression will print 5
  x + 1     # This expression will print 6
}}
```";
            });

            AddStep("Markdown Table", () =>
            {
                markdownContainer.Text =
                    @"|Operator            | Description
|--------------------|------------
| `'left' + <right>` | concatenates left to right string: `""ab"" + ""c"" -> ""abc""`
| `'left' * <right>` | concatenates the left string `right` times: `'a' * 5  -> aaaaa`. left and right and be swapped as long as there is one string and one number.";
            });

             AddStep("Markdown Table (Aligned)", () =>
            {
                markdownContainer.Text =
                    @"| Left-Aligned  | Center Aligned  | Right Aligned |
| :------------ |:---------------:| -----:|
| col 3 is      | some wordy text | $1600 |
| col 2 is      | centered        |   $12 |
| zebra stripes | are neat        |    $1 |";
            });

            AddStep("Markdown Paragraph 1", () =>
            {
                markdownContainer.Text = @"A text enclosed by `{{` and `}}` is a scriban **code block** that will be evaluated by the scriban templating engine.";
            });

            AddStep("Markdown Paragraph 2", () =>
            {
                markdownContainer.Text =
                    @"The greedy mode using the character - (e.g {{- or -}}), removes any whitespace, including newlines Examples with the variable name = ""foo"":";
            });

            AddStep("MarkdownImage", () =>
            {
                markdownContainer.Text = @"![Drag Racing](https://www.wonderplugin.com/videos/demo-image0.jpg)
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
                    const string url = "https://raw.githubusercontent.com/lunet-io/scriban/master/doc/language.md";
                    var httpClient = new HttpClient();
                    markdownContainer.Text = httpClient.GetStringAsync(url).ConfigureAwait(false).GetAwaiter().GetResult();
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
    public class MarkdownContainer : CompositeDrawable
    {

        protected virtual MarkdownPipeline CreateBuilder()
        {
            return new MarkdownPipelineBuilder().UseAutoIdentifiers(AutoIdentifierOptions.GitHub)
                                         .UseEmojiAndSmiley()
                                         .UseAdvancedExtensions().Build();
        }

        public string Text
        {
            set
            {
                var markdownText = value;
                var pipeline = CreateBuilder();
                var document = Markdig.Markdown.Parse(markdownText, pipeline);

                markdownContainer.Clear();
                foreach (var component in document)
                    AddMarkdownComponent(component, markdownContainer, root_layer_index);
            }
        }

        public virtual float Spacing
        {
            get => markdownContainer.Spacing.Y;
            set => markdownContainer.Spacing = new Vector2(value);
        }

        public virtual MarginPadding MarkdownMargin
        {
            get => markdownContainer.Margin;
            set => markdownContainer.Margin = value;
        }

        public virtual MarginPadding MarkdownPadding
        {
            get => markdownContainer.Padding;
            set => markdownContainer.Padding = value;
        }

        private const int root_layer_index = 0;
        private readonly FillFlowContainer markdownContainer;

        public MarkdownContainer()
        {
            InternalChildren = new Drawable[]
            {
                new ScrollContainer
                {
                    ScrollbarOverlapsContent = false,
                    RelativeSizeAxes = Axes.Both,
                    Child = markdownContainer = new FillFlowContainer
                    {
                        AutoSizeAxes = Axes.Y,
                        RelativeSizeAxes = Axes.X,
                        Direction = FillDirection.Vertical,
                    }
                }
            };

            Spacing = 25;
            MarkdownPadding = new MarginPadding { Left = 10, Right = 30 };
            MarkdownMargin = new MarginPadding { Left = 10, Right = 30 };
        }

        protected virtual void AddMarkdownComponent(IMarkdownObject markdownObject, FillFlowContainer container, int layerIndex)
        {
            switch (markdownObject)
            {
                case HeadingBlock headingBlock:
                    container.Add(CreateMarkdownHeading(headingBlock));
                    if (headingBlock.Level < 3)
                        container.Add(CreateMarkdownSeperator());
                    break;
                case ParagraphBlock paragraphBlock:
                    container.Add(CreateMarkdownTextFlowContainer(paragraphBlock, layerIndex));
                    break;
                case QuoteBlock quoteBlock:
                    container.Add(CreateMarkdownQuoteBlock(quoteBlock));
                    break;
                case FencedCodeBlock fencedCodeBlock:
                    container.Add(CreateMarkdownFencedCodeBlock(fencedCodeBlock));
                    break;
                case Table table:
                    container.Add(CreateMarkdownTable(table));
                    break;
                case ListBlock listBlock:
                    var childContainer = CreateChildFillFlowContainer();
                    container.Add(childContainer);
                    foreach (var single in listBlock)
                        AddMarkdownComponent(single, childContainer, layerIndex + 1);
                    break;
                case ListItemBlock listItemBlock:
                    foreach (var single in listItemBlock)
                        AddMarkdownComponent(single, container, layerIndex);
                    break;
                default:
                    container.Add(CreateNotImplementedMarkdown(markdownObject));
                    break;
            }
        }

        protected virtual MarkdownHeading CreateMarkdownHeading(HeadingBlock headingBlock)
        {
            return new MarkdownHeading(headingBlock);
        }

        protected virtual MarkdownTextFlowContainer CreateMarkdownTextFlowContainer(ParagraphBlock paragraphBlock, int layerIndex)
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

            drawableParagraphBlock.AddInlineText(paragraphBlock.Inline);
            return drawableParagraphBlock;
        }

        protected virtual MarkdownQuoteBlock CreateMarkdownQuoteBlock(QuoteBlock quoteBlock)
        {
            return new MarkdownQuoteBlock(quoteBlock);
        }

        protected virtual MarkdownFencedCodeBlock CreateMarkdownFencedCodeBlock(FencedCodeBlock fencedCodeBlock)
        {
            return new MarkdownFencedCodeBlock(fencedCodeBlock);
        }

        protected virtual MarkdownTable CreateMarkdownTable(Table table)
        {
            return new MarkdownTable(table);
        }

        protected virtual FillFlowContainer CreateChildFillFlowContainer()
        {
            return new FillFlowContainer
            {
                Direction = FillDirection.Vertical,
                Spacing = new Vector2(10, 10),
                Padding = new MarginPadding { Left = 25, Right = 5 },
                AutoSizeAxes = Axes.Y,
                RelativeSizeAxes = Axes.X,
            };
        }

        protected virtual MarkdownSeperator CreateMarkdownSeperator()
        {
            return new MarkdownSeperator();
        }

        protected virtual Drawable CreateNotImplementedMarkdown(IMarkdownObject markdownObject)
        {
            return new NotImplementedMarkdown(markdownObject);
        }

    }

    /// <summary>
    /// NotExistMarkdown :
    /// shows the <see cref="IMarkdownObject" /> does not implement in drawable object
    /// </summary>
    public class NotImplementedMarkdown : SpriteText
    {
        public NotImplementedMarkdown(IMarkdownObject markdownObject)
        {
            Colour = new Color4(255, 0, 0, 255);
            TextSize = 21;
            Text = markdownObject?.GetType() + " Not implemented.";
        }
    }

    /// <summary>
    /// MarkdownTable : 
    /// |Operator            | Description
    /// |--------------------|------------
    /// | `<left/> + <right/>` | add left to right number 
    /// | `<left/> - <right/>` | substract right number from left
    /// | `<left/> * <right/>` | multiply left by right number
    /// | `<left/> / <right/>` | divide left by right number
    /// | `<left/> // <right/>`| divide left by right number and round to an integer
    /// | `<left/> % <right/>` | calculates the modulus of left by right
    /// </summary>
    public class MarkdownTable : Container
    {
        private readonly MarkdownTableContainer tableContainer;
        private readonly List<List<MarkdownTableCell>> listContainerArray = new List<List<MarkdownTableCell>>();
        public MarkdownTable(Table table)
        {
            AutoSizeAxes = Axes.Y;
            RelativeSizeAxes = Axes.X;
            Padding = new MarginPadding { Right = 100 };

            foreach (var block in table)
            {
                var tableRow = (TableRow)block;
                List<MarkdownTableCell> rows = new List<MarkdownTableCell>();

                if (tableRow != null)
                    for (int columnIndex = 0; columnIndex < tableRow.Count; columnIndex++)
                    {
                        var columnDimensions = table.ColumnDefinitions[columnIndex];
                        var tableCell = (TableCell)tableRow[columnIndex];
                        if (tableCell != null)
                            rows.Add(new MarkdownTableCell(tableCell, columnDimensions, listContainerArray.Count));
                    }

                listContainerArray.Add(rows);
            }

            Children = new Drawable[]
            {
                tableContainer = new MarkdownTableContainer
                {
                    AutoSizeAxes = Axes.Y,
                    RelativeSizeAxes = Axes.X,
                    Content = listContainerArray.Select(x=>x.Select(y=>(Drawable)y).ToArray()).ToArray(),
                }
            };

            //define max row is 50
            tableContainer.RowDimensions = Enumerable.Repeat(new Dimension(GridSizeMode.AutoSize), 50).ToArray();

            int row = listContainerArray.FirstOrDefault()?.Count ?? 0;

            if (row == 2)
            {
                tableContainer.ColumnDimensions = new[] { new Dimension(GridSizeMode.Relative, 0.3f) };
            }
        }

        protected override void Update()
        {
            tableContainer.RowDimensions = listContainerArray.Select(X => new Dimension(GridSizeMode.Absolute, X.Max(y => y.TextFlowContainer.DrawHeight + 10))).ToArray();
            base.Update();
        }


        private class MarkdownTableContainer : GridContainer
        {
            public new Axes AutoSizeAxes
            {
                get => base.AutoSizeAxes;
                set => base.AutoSizeAxes = value;
            }
        }

        private class MarkdownTableCell : Container
        {
            public MarkdownTextFlowContainer TextFlowContainer => textFlowContainer;
            private readonly MarkdownTextFlowContainer textFlowContainer;

            public MarkdownTableCell(TableCell cell, TableColumnDefinition definition, int rowNumber)
            {
                RelativeSizeAxes = Axes.Both;
                BorderThickness = 1.8f;
                BorderColour = Color4.White;
                Masking = true;

                var backgroundColor = rowNumber % 2 != 0 ? Color4.White : Color4.LightGray;
                var backgroundAlpha = 0.3f;
                if (rowNumber == 0)
                {
                    backgroundColor = Color4.White;
                    backgroundAlpha = 0.4f;
                }

                Children = new Drawable[]
                {
                    new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = backgroundColor,
                        Alpha = backgroundAlpha
                    },
                    textFlowContainer = new MarkdownTextFlowContainer
                    {
                        Margin = new MarginPadding{Left = 5,Right = 5,Top = 5,Bottom = 5}
                    }
                };

                foreach (var block in cell)
                {
                    var single = (ParagraphBlock)block;
                    textFlowContainer.ParagraphBlock = single;
                }

                switch (definition.Alignment)
                {
                    case TableColumnAlign.Center:
                        textFlowContainer.TextAnchor = Anchor.TopCentre;
                        break;

                    case TableColumnAlign.Right:
                        //TODO : make this work
                        //textFlowContainer.TextAnchor = Anchor.TopRight;
                        break;

                    default:
                        textFlowContainer.TextAnchor = Anchor.TopLeft;
                        break;
                }
            }
        }
    }

    /// <summary>
    /// MarkdownFencedCodeBlock :
    /// ```
    /// foo
    /// ```
    /// </summary>
    public class MarkdownFencedCodeBlock : Container
    {
        public MarkdownFencedCodeBlock(FencedCodeBlock fencedCodeBlock)
        {
            AutoSizeAxes = Axes.Y;
            RelativeSizeAxes = Axes.X;

            TextFlowContainer textFlowContainer;
            Children = new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = Color4.Gray,
                    Alpha = 0.5f
                },
                textFlowContainer = new TextFlowContainer
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Margin = new MarginPadding { Left = 10, Right = 10, Top = 10, Bottom = 10 }
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
    public class MarkdownHeading : Container
    {
        public MarkdownHeading(HeadingBlock headingBlock)
        {
            AutoSizeAxes = Axes.Y;
            RelativeSizeAxes = Axes.X;

            MarkdownTextFlowContainer textFlowContainer;

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
            textFlowContainer.AddInlineText(headingBlock.Inline);
        }
    }

    /// <summary>
    /// MarkdownQuoteBlock :
    /// > NOTE: This document does not describe the `liquid` language.
    /// </summary>
    public class MarkdownQuoteBlock : Container
    {
        public MarkdownQuoteBlock(QuoteBlock quoteBlock)
        {
            AutoSizeAxes = Axes.Y;
            RelativeSizeAxes = Axes.X;

            MarkdownTextFlowContainer textFlowContainer;

            Children = new Drawable[]
            {
                new Box
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
                textFlowContainer.ParagraphBlock = paragraphBlock;
        }
    }

    /// <summary>
    /// MarkdownSeperator :
    /// (spacing)
    /// </summary>
    public class MarkdownSeperator : Box
    {
        public MarkdownSeperator()
        {
            RelativeSizeAxes = Axes.X;
            Height = 1;
            Colour = Color4.Gray;
        }
    }

    /// <summary>
    /// Load image from url
    /// </summary>
    public class MarkdownImage : Container
    {
        public MarkdownImage(string url)
        {
            Box background;
            Children = new Drawable[]
            {
                background = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = Color4.LightGray,
                    Alpha = 0.3f
                },
                new DelayedLoadWrapper(
                    new ImageContainer(url)
                    {
                        RelativeSizeAxes = Axes.Both,
                        OnLoadComplete = d =>
                        {
                            background.FadeTo(0,300,Easing.OutQuint);
                            d.FadeInFromZero(300, Easing.OutQuint);
                        },
                    })
            };
        }

        private class ImageContainer : Container
        {
            private readonly string imageUrl;
            private readonly Sprite image;

            public ImageContainer(string url)
            {
                imageUrl = url;
                Children = new Drawable[]
                {
                    image = new Sprite
                    {
                        RelativeSizeAxes = Axes.Both,
                        FillMode = FillMode.Fit,
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre
                    }
                };
            }

            [BackgroundDependencyLoader]
            private void load(TextureStore textures)
            {
                Texture texture = null;
                if (!string.IsNullOrEmpty(imageUrl))
                    texture = textures.Get(imageUrl);

                //TODO : get default texture
                //if (texture == null)
                //    texture = textures.Get(@"Markdown/default-image");

                image.Texture = texture;
            }
        }
    }

    /// <summary>
    /// Markdown text flow container.
    /// </summary>
    public class MarkdownTextFlowContainer : CustomizableTextContainer
    {
        private ParagraphBlock paragraphBlock;

        public ParagraphBlock ParagraphBlock
        {
            get => paragraphBlock;
            set
            {
                paragraphBlock = value;
                Clear();
                AddInlineText(paragraphBlock.Inline);
            }
        }

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

        public MarkdownTextFlowContainer AddInlineText(ContainerInline lnline)
        {
            foreach (var single in lnline)
            {
                if (single is LiteralInline literalInline)
                {
                    var text = literalInline.Content.ToString();
                    if (lnline.GetNext(literalInline) is HtmlInline
                        && lnline.GetPrevious(literalInline) is HtmlInline)
                        AddText(text, t => t.Colour = Color4.MediumPurple);
                    else if (lnline.GetNext(literalInline) is HtmlEntityInline)
                        AddText(text, t => t.Colour = Color4.GreenYellow);
                    else if (literalInline.Parent is LinkInline linkInline)
                    {
                        if (!linkInline.IsImage)
                            AddText(text, t => t.Colour = Color4.DodgerBlue);
                    }
                    else
                        AddText(text);
                }
                else if (single is CodeInline codeInline)
                {
                    AddCodeInLineText(codeInline);
                }
                else if (single is EmphasisInline)
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
                        AddImage(linkInline);
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
                    AddText(single.GetType() + " Not implemented.", t => t.Colour = Color4.Red);
                }

                //generate child
                if (single is ContainerInline containerInline) AddInlineText(containerInline);
            }

            return this;
        }

        protected virtual void AddCodeInLineText(CodeInline codeInline)
        {
             AddText(codeInline.Content, t =>
             {
                 t.Colour = Color4.Orange;
             });
        }

        protected virtual void AddImage(LinkInline linkInline)
        {
            var imageUrl = linkInline.Url;
            //insert a image
            AddImage(new MarkdownImage(imageUrl)
            {
                Width = 300,
                Height = 240,
            });
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
