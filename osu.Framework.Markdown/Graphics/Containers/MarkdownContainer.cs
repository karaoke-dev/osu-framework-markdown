using System;
using System.Collections.Generic;
using System.Text;
using osu.Framework.Graphics.Containers;

namespace osu.Framework.Markdown.Graphics.Containers
{
    public class MarkdownContainer : FillFlowContainer
    {
        public MarkdownContainer()
        {
            Direction = FillDirection.Vertical;
            Spacing = new OpenTK.Vector2(20, 20);
        }
    }
}
