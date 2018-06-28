using System;
using System.Collections.Generic;
using System.Text;
using Markdig.Syntax;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Shapes;

namespace osu.Framework.Markdown.Graphics.Containers.Component
{
    public class MarkdownSeperator : Box
    {
        public MarkdownSeperator()
        {
            RelativeSizeAxes = Axes.X;
        }

        public MarkdownSeperator(ParagraphBlock ParagraphBlock)
        {
            //Style = ParagraphBlock;
            Width = 200;
            Height = 1;
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
