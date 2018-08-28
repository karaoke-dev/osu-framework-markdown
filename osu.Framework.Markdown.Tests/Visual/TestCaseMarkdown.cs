﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using osu.Framework.Graphics.Colour;

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

            AddStep("Html in line", () =>
            {
                markdownContainer.Text = @"  - [9.3 <code>case</code> and <code>when</code>](#93-case-and-when)";
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

            AddStep("Markdown Svg Tag", () =>
            {
                markdownContainer.Text = @"![](https://img.shields.io/github/stars/pandao/editor.md.svg) ![]";
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

            AddStep("MarkdownOsuWiki", () =>
            {
                try
                {
                    //https://github.com/ppy/osu-wiki/blob/master/wiki/Game_Modes/osu!/en.md
                    const string url = "https://raw.githubusercontent.com/ppy/osu-wiki/master/wiki/Game_Modes/osu!/en.md";
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
            => new MarkdownPipelineBuilder().UseAutoIdentifiers(AutoIdentifierOptions.GitHub)
            .UseEmojiAndSmiley()
            .UseAdvancedExtensions().Build();

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

        public MarginPadding MarkdownMargin
        {
            get => markdownContainer.Margin;
            set => markdownContainer.Margin = value;
        }

        public MarginPadding MarkdownPadding
        {
            get => markdownContainer.Padding;
            set => markdownContainer.Padding = value;
        }

        private const int root_layer_index = 0;
        private FillFlowContainer markdownContainer;

        [BackgroundDependencyLoader]
        private void load()
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
                case HtmlBlock _:
                    //Cannot read Html Syntex in Markdown.
                    break;
                case LinkReferenceDefinitionGroup _:
                    //Link Definition Does not need display.
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
            => new NotImplementedMarkdown(markdownObject);
    }

    /// <summary>
    /// Visualises a message when a <see cref="IMarkdownObject"/> doesn't have a visual implementation.
    /// </summary>
    public class NotImplementedMarkdown : CompositeDrawable
    {
        public NotImplementedMarkdown(IMarkdownObject markdownObject)
        {
            AutoSizeAxes = Axes.Y;
            InternalChild = CreateNotImplementDrawable(markdownObject);
        }

        protected virtual Drawable CreateNotImplementDrawable(IMarkdownObject markdownObject)
        {
            return new SpriteText
            {
                Colour = new Color4(255, 0, 0, 255),
                TextSize = 21,
                Text = markdownObject?.GetType() + " Not implemented."
            };
        }
    }

    /// <summary>
    /// Visualises a markdown table, containing <see cref="MarkdownTableCell"/>s.
    /// </summary>
    public class MarkdownTable : CompositeDrawable
    {
        private readonly MarkdownTableContainer tableContainer;
        private readonly List<List<MarkdownTableCell>> listContainerArray = new List<List<MarkdownTableCell>>();

        public MarkdownTable(Table table)
        {
            AutoSizeAxes = Axes.Y;
            RelativeSizeAxes = Axes.X;
            Padding = new MarginPadding { Right = 100 };
            Margin = new MarginPadding { Right = 100 };

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
                            rows.Add(CreateMarkdownTableCell(tableCell, columnDimensions, listContainerArray.Count));
                    }

                listContainerArray.Add(rows);
            }

            InternalChild = tableContainer = new MarkdownTableContainer
            {
                AutoSizeAxes = Axes.Y,
                RelativeSizeAxes = Axes.X,
                Content = listContainerArray.Select(x => x.Select(y => (Drawable)y).ToArray()).ToArray(),
            };
        }

        protected virtual MarkdownTableCell CreateMarkdownTableCell(TableCell cell, TableColumnDefinition definition, int rowNumber) =>
            new MarkdownTableCell(cell, definition, rowNumber);

        
        private Vector2 lastDrawSize;
        protected override void Update()
        {
            if (lastDrawSize != DrawSize)
            {
                lastDrawSize = DrawSize;
                updateColumnDefinitions();
                updateRowDefinitions();
            }
            base.Update();
        }
        

        /*
        public override bool Invalidate(Invalidation invalidation = Invalidation.All, Drawable source = null, bool shallPropagate = true)
        {
            if ((invalidation & (Invalidation.DrawSize | Invalidation.RequiredParentSizeToFit)) > 0)
            {
                updateColumnDefinitions();
                updateRowDefinitions();
            }

            return base.Invalidate(invalidation, source, shallPropagate);
        }
        */

        private void updateColumnDefinitions()
        {
            if(!listContainerArray.Any())
                return;

            var totalColumn = listContainerArray.Max(x => x.Count);
            var totalRows = listContainerArray.Count;

            var listcolumnMaxWidth = new float[totalColumn];

            for (int row = 0; row < totalRows; row++)
            {
                for (int column = 0; column < totalColumn; column++)
                {
                    var colimnTextTotalWidth = listContainerArray[row][column].TextFlowContainer.TotalTextWidth();

                    //get max width
                    listcolumnMaxWidth[column] = Math.Max(listcolumnMaxWidth[column], colimnTextTotalWidth);
                }
            }

            listcolumnMaxWidth = listcolumnMaxWidth.Select(x => x + 20).ToArray();

            var columnDimensions = new Dimension[totalColumn];

            //if max width < DrawWidth, means set absolute value to each column
            if (listcolumnMaxWidth.Sum() < DrawWidth - Margin.Right)
            {
                //not relative , define value instead
                tableContainer.RelativeSizeAxes = Axes.None;
                for (int column = 0; column < totalColumn; column++)
                {
                    columnDimensions[column] = new Dimension(GridSizeMode.Absolute, listcolumnMaxWidth[column]);
                }
            }
            else
            {
                //set to relative
                tableContainer.RelativeSizeAxes = Axes.X;
                var totalWidth = listcolumnMaxWidth.Sum();
                for (int column = 0; column < totalColumn; column++)
                {
                    columnDimensions[column] = new Dimension(GridSizeMode.Relative, listcolumnMaxWidth[column] / totalWidth);
                }
            }
            tableContainer.ColumnDimensions = columnDimensions;
        }

        private void updateRowDefinitions()
        {
            if (!listContainerArray.Any())
                return;

            tableContainer.RowDimensions = listContainerArray
                .Select(x => new Dimension(GridSizeMode.Absolute, x.Max(y => y.TextFlowContainer.DrawHeight + 10))).ToArray();
        }

        private class MarkdownTableContainer : GridContainer
        {
            public new Axes AutoSizeAxes
            {
                get => base.AutoSizeAxes;
                set => base.AutoSizeAxes = value;
            }
        }

        public class MarkdownTableCell : CompositeDrawable
        {
            public MarkdownTextFlowContainer TextFlowContainer => textFlowContainer;
            private readonly MarkdownTextFlowContainer textFlowContainer;

            protected virtual MarkdownTextFlowContainer CreateMarkdownTextFlowContainer() =>
                new MarkdownTextFlowContainer
                {
                    Padding = new MarginPadding { Left = 5, Right = 5, Top = 5, Bottom = 0 }
                };

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

                InternalChildren = new Drawable[]
                {
                    new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = backgroundColor,
                        Alpha = backgroundAlpha
                    },
                    textFlowContainer = CreateMarkdownTextFlowContainer()
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
                        textFlowContainer.TextAnchor = Anchor.TopRight;
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
    public class MarkdownFencedCodeBlock : CompositeDrawable
    {
        public MarkdownFencedCodeBlock(FencedCodeBlock fencedCodeBlock)
        {
            AutoSizeAxes = Axes.Y;
            RelativeSizeAxes = Axes.X;

            TextFlowContainer textFlowContainer;
            InternalChildren = new Drawable[]
            {
                CreateBackground(),
                textFlowContainer = CreateTextArea(),
            };

            var lines = fencedCodeBlock.Lines.Lines.Take(fencedCodeBlock.Lines.Count);
            foreach (var sligneLine in lines)
            {
                var lineString = sligneLine.ToString();
                textFlowContainer.AddParagraph(lineString);
            }
        }

        protected virtual Drawable CreateBackground()
        {
            return new Box
            {
                RelativeSizeAxes = Axes.Both,
                Colour = Color4.Gray,
                Alpha = 0.5f
            };
        }

        protected virtual TextFlowContainer CreateTextArea()
        {
            return new TextFlowContainer
            {
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Margin = new MarginPadding { Left = 10, Right = 10, Top = 10, Bottom = 10 }
            };
        }
    }

    /// <summary>
    /// MarkdownHeading :
    /// #Heading1
    /// ##Heading2
    /// ###Heading3
    /// ###3Heading4
    /// </summary>
    public class MarkdownHeading : CompositeDrawable
    {
        public MarkdownHeading(HeadingBlock headingBlock)
        {
            AutoSizeAxes = Axes.Y;
            RelativeSizeAxes = Axes.X;

            MarkdownTextFlowContainer textFlowContainer;

            InternalChildren = new Drawable[]
            {
                textFlowContainer = CreateMarkdownTextFlowContainer()
            };

            var level = headingBlock.Level;
            textFlowContainer.Scale = new Vector2(GetFontSizeByLevel(level));
            textFlowContainer.AddInlineText(headingBlock.Inline);
        }

        protected virtual MarkdownTextFlowContainer CreateMarkdownTextFlowContainer() =>
            new MarkdownTextFlowContainer();

        protected float GetFontSizeByLevel(int level)
        {
            switch (level)
            {
                case 1:
                    return 2.7f;
                case 2:
                    return 2;
                case 3:
                    return 1.5f;
                case 4:
                    return 1.3f;
                default:
                    return 1;
            }
        }
    }

    /// <summary>
    /// MarkdownQuoteBlock :
    /// > NOTE: This document does not describe the `liquid` language.
    /// </summary>
    public class MarkdownQuoteBlock : CompositeDrawable
    {
        public MarkdownQuoteBlock(QuoteBlock quoteBlock)
        {
            AutoSizeAxes = Axes.Y;
            RelativeSizeAxes = Axes.X;

            MarkdownTextFlowContainer textFlowContainer;

            InternalChildren = new Drawable[]
            {
                CreateBackground(),
                textFlowContainer = CreateMarkdownTextFlowContainer()
            };

            if (quoteBlock.LastChild is ParagraphBlock paragraphBlock)
                textFlowContainer.ParagraphBlock = paragraphBlock;
        }

        protected virtual Drawable CreateBackground()
        {
            return new Box
            {
                Colour = Color4.Gray,
                Width = 5,
                Anchor = Anchor.CentreLeft,
                Origin = Anchor.CentreLeft,
                RelativeSizeAxes = Axes.Y
            };
        }

        protected virtual MarkdownTextFlowContainer CreateMarkdownTextFlowContainer()
        {
            return new MarkdownTextFlowContainer
            {
                Margin = new MarginPadding { Left = 20 }
            };
        }
    }

    /// <summary>
    /// MarkdownSeperator :
    /// (spacing)
    /// </summary>
    public class MarkdownSeperator : CompositeDrawable
    {
        public MarkdownSeperator()
        {
            AutoSizeAxes = Axes.Y;
            RelativeSizeAxes = Axes.X;
            InternalChild = CreateSeperator();
        }

        protected virtual Drawable CreateSeperator()
        {
            return new Box
            {
                RelativeSizeAxes = Axes.X,
                Colour = Color4.Gray,
                Height = 1,
            };
        }
    }

    /// <summary>
    /// Load image from url
    /// </summary>
    public class MarkdownImage : CompositeDrawable
    {
        private readonly Box background;
        public MarkdownImage(string url)
        {
            InternalChildren = new Drawable[]
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
                            if(d is ImageContainer imageContainer)
                                EffectLoadImageComplete(imageContainer);
                        },
                    })
            };
        }

        protected virtual void EffectLoadImageComplete(ImageContainer imageContainer)
        {
            var rowImageSize = imageContainer.Image?.Texture?.Size ?? new Vector2();
            //Resize to image's row size
            this.ResizeWidthTo(rowImageSize.X, 700, Easing.OutQuint);
            this.ResizeHeightTo(rowImageSize.Y, 700, Easing.OutQuint);

            //Hide background image
            background.FadeTo(0, 300, Easing.OutQuint);
            imageContainer.FadeInFromZero(300, Easing.OutQuint);
        }

        protected class ImageContainer : CompositeDrawable
        {
            private readonly string imageUrl;
            private readonly Sprite image;

            public Sprite Image => image;

            public ImageContainer(string url)
            {
                imageUrl = url;
                InternalChildren = new Drawable[]
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
                    else if (literalInline.Parent is EmphasisInline emphasisInline)
                    {
                        if (emphasisInline.IsDouble)
                        {
                            switch (emphasisInline.DelimiterChar)
                            {
                                case '*':
                                    AddBoldText(text, literalInline);
                                    break;
                                default:
                                    AddDefalutLiteralInlineText(text, literalInline);
                                    break;
                            }
                        }
                        else
                        {
                            AddDefalutLiteralInlineText(text, literalInline);
                        }
                    }
                    else if (literalInline.Parent is LinkInline linkInline)
                    {
                        if (!linkInline.IsImage)
                            AddLinkText(text, literalInline);
                    }
                    else
                        AddText(text);
                }
                else if (single is CodeInline codeInline)
                {
                    AddCodeInLineText(codeInline);
                }
                else if (single is LinkInline linkInline)
                {
                    if (linkInline.IsImage)
                    {
                        AddImage(linkInline);
                    }
                }
                else if (single is HtmlInline || single is HtmlEntityInline || single is EmphasisInline)
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

        protected virtual void AddBoldText(string text, LiteralInline literalInline)
        {
            //TODO : make real "Bold text"
            AddDrawable(new SpriteText
            {
                Text = text,
                Colour = Color4.LightGray
            }.WithEffect(new GlowEffect
            {
                BlurSigma = new Vector2(1f),
                Strength = 2f,
                Colour = ColourInfo.GradientHorizontal(new Color4(1.2f, 1.2f, 1.2f, 1f), new Color4(1.2f, 1.2f, 1.2f, 1f)),
            }));
        }

        protected virtual void AddLinkText(string text, LiteralInline literalInline)
        {
            //TODO Add Link Text
            //var linkText = (literalInline.Parent as LinkInline)?.Url;
            AddText(text, t => t.Colour = Color4.DodgerBlue);
        }

        protected virtual void AddDefalutLiteralInlineText(string text, LiteralInline literalInline)
        {
            AddText(text);
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
                Width = 40,
                Height = 40,
            });
        }

        protected IEnumerable<SpriteText> AddDrawable(Drawable drawable)
        {
            var imageIndex = AddPlaceholder(drawable);
            return base.AddText("[" + imageIndex + "]");
        }

        public bool IsChangeLine()
        {
            if (FlowingChildren.Any())
            {
                var fortRowX = FlowingChildren.FirstOrDefault()?.BoundingBox.Size.X;
                return FlowingChildren.Any(x => x.BoundingBox.X != fortRowX);
            }
            return true;
        }

        public float TotalTextWidth()
        {
            return FlowingChildren.Sum(x => x.BoundingBox.Size.X);
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
