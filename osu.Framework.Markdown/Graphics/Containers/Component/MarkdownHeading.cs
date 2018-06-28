using System;
using System.Collections.Generic;
using System.Text;
using Markdig.Syntax;
using osu.Framework.Graphics.Sprites;

namespace osu.Framework.Markdown.Graphics.Containers.Component
{
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
                        Text = "  " + Text;
                        this.TextSize = 38;
                        break;
                    case 3:
                        Text = "        " + Text;
                        this.TextSize = 21;
                        break;
                    case 4:
                        this.TextSize = 21;
                        break;
                    case 5:
                        this.TextSize = 10;
                        break;

                    default:
                        this.TextSize = 3;
                        return;
                }
            }
        }
    }
}
