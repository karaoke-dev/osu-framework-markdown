using System;
using System.Collections.Generic;
using System.Text;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;

namespace osu.Framework.Markdown.Graphics.Containers.Component
{
    public class MarkdownQuoteBlock : Container
    {
        private SpriteText _text;
        private Container _quoteContainer;
        private Box _quoteBox;
        public MarkdownQuoteBlock()
        {
            Add(_quoteContainer = new Container()
            {
                Children = new Drawable[]
                {
                    _text = new SpriteText()
                    {

                    },
                    _quoteBox = new Box()
                    {

                    }
                }
            });
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
                    _text.Text = content;
                }
            }
        }
    }
}
