using System;
using System.Collections.Generic;
using System.Text;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;

namespace osu.Framework.Markdown.Graphics.Containers
{
    public class MarkdownScrollView : ScrollContainer
    {
        private MarkdownContainer _markdownContainer;

        public MarkdownScrollView()
        {
            _markdownContainer = new MarkdownContainer()
            {
                AutoSizeAxes = Axes.Both
            };
            base.Add(_markdownContainer);
        }

        public override void Add(Drawable drawable)
        {
            _markdownContainer.Add(drawable);
        }
    }
}
