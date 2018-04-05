using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FFTViewer
{
    class SpectrumRenderer : IRenderingLayer
    {
        public delegate void GetTwoChannelDataDelegate(out float[] d1, out float[] d2);

        private PointF[] _PointBuffer;

        public float? FixedRangeY = null;

        public Func<float[]> Source;
        public GetTwoChannelDataDelegate Source2;
        public Func<float, float> ScaleX;
        public Func<float, float> ScaleY;

        public ILabelXProvider LabelsXForeground, LabelsXBackground;

        public FFTImageRecorder ImageRecorder;

        private void EnsureBuffer(int len)
        {
            if (_PointBuffer == null || _PointBuffer.Length != len)
            {
                _PointBuffer = new PointF[len];
            }
        }

        private float DefaultScale(float r)
        {
            return r;
        }

        private void CalculatePoints(float[] val, double x0, double xstep, double y0, double ystep)
        {
            for (int i = 0; i < _PointBuffer.Length; ++i)
            {
                //TODO draw less points (at high frequency range)
                _PointBuffer[i] = new PointF(
                    (float)(x0 + xstep * val.Length * ScaleX(i / (float)val.Length)),
                    (float)(y0 + ystep * ScaleY(val[i])));
            }
        }

        private class LabelXRenderer : ILabelXRenderer
        {
            public Graphics g;
            public float x0, x1;
            public float y0, y1;
            public Func<float, float> scaleX;

            public void Draw(Pen pen, float val)
            {
                val = scaleX(val);
                var x = x0 * (1 - val) + x1 * val;
                g.DrawLine(pen, x, y0, x, y1);
            }
        }

        public void ResizeBuffer(int size)
        {
            for (int i = 0; i < size; ++i)
            {
                var pos = ScaleX(i / (float)size);
                if (pos > 1)
                {
                    EnsureBuffer(i);
                    return;
                }
            }
            EnsureBuffer(size);
        }

        private void RenderInternal(ScaledGraphics g, float[] val, float[] val2 = null)
        {
            if (val.Length < 2) return;

            if (ScaleX == null) ScaleX = DefaultScale;
            if (ScaleY == null) ScaleY = DefaultScale;

            if (_PointBuffer == null || _PointBuffer.Length == 0)
            {
                ResizeBuffer(val.Length);
            }
            
            var left = g.Rect.X;
            var top = g.Rect.Y;
            var width = g.Rect.Width;
            var height = g.Rect.Height;
            
            var y0 = top;
            var ystep = height;
            
            CalculatePoints(val, left, width / (val.Length - 1), y0, ystep);
            
            //Record
            var recorderRect = new RectangleF(left, top, width, height);
            ImageRecorder?.Write(recorderRect, _PointBuffer, val2);
            ImageRecorder?.DrawBitmap(g.Target, recorderRect);
            
            var labelXRenderer = new LabelXRenderer
            {
                g = g.Target,
                x0 = left,
                x1 = left + width,
                y0 = top,
                y1 = top + height,
                scaleX = ScaleX,
            };

            //X axis
            g.Target.DrawLine(Pens.Gray, left, y0 + ystep, left + width, y0 + ystep);

            //Background label
            LabelsXBackground?.DrawAll(labelXRenderer);

            //Draw
            try
            {
                //In case of NaN/Inf
                g.Target.DrawLines(Pens.Black, _PointBuffer);
            }
            catch
            {
            }

            //Foreground label
            LabelsXForeground?.DrawAll(labelXRenderer);
        }
        
        public void Draw(ScaledGraphics g)
        {
            if (Source2 != null)
            {
                Source2(out var d1, out var d2);
                RenderInternal(g, d1, d2);
            }
            else
            {
                RenderInternal(g, Source());
            }
        }
    }
}
