using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FFTViewer
{
    abstract class LabelXLayer : IRenderingLayer
    {
        public ScaledGraphics g;

        protected void DrawLine(Pen pen, float x)
        {
            var transformedX = g.Rect.X + g.Rect.Width * x;
            g.Target.DrawLine(pen, transformedX, g.Rect.Top, transformedX, g.Rect.Bottom);
        }

        public abstract void DrawAll();

        public void Draw(ScaledGraphics g)
        {
            this.g = g;
            DrawAll();
        }
    }
}
