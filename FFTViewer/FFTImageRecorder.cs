using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace FFTViewer
{
    class FFTImageRecorder
    {
        public FFTImageRecorder(int imageWidth, int imageHeight, float ratioY, Color c0, Color c)
        {
            _Image = new Bitmap(imageWidth, imageHeight, PixelFormat.Format32bppArgb);
            _Rasterizer = new FFTImageRasterizer(imageWidth, c0, c);
            _LockedData = _Image.LockBits(new Rectangle(0, 0, imageWidth, imageHeight),
                ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
            RatioY = ratioY;
        }
        
        private Bitmap _Image;
        private BitmapData _LockedData;
        private int _PositionY;
        private FFTImageRasterizer _Rasterizer;
        private bool _WrapImage = false;
        public float RatioY;

        public bool Enabled = true;

        public void Write(RectangleF rect, PointF[] points)
        {
            if (!Enabled)
            {
                return;
            }
            if (_Image == null)
            {
                throw new InvalidOperationException();
            }
            
            _Rasterizer.Begin();
            for (int i = 0; i < points.Length; ++i)
            {
                var point = points[i];
                var x = (point.X - rect.Left) / rect.Width * _Image.Width;
                var y = (rect.Bottom - point.Y) / rect.Height;
                _Rasterizer.DrawPoint(x, y);
            }
            _Rasterizer.End();

            var ptr = IntPtr.Add(_LockedData.Scan0, _LockedData.Stride * _PositionY);
            _PositionY += 1;
            if (_PositionY >= _Image.Height)
            {
                _PositionY = 0;
                _WrapImage = true;
            }
            
            _Rasterizer.CopyTo(ptr);
        }

        private RectangleF DrawBitmapSection(Graphics g, RectangleF rect, int start, int end)
        {
            var absHeight = Math.Abs(rect.Height);
            float available = ((end - start) * RatioY) / absHeight;
            RectangleF src, dest, empty;
            if (available < 1)
            {
                src = new RectangleF(0, start, _Image.Width, end);
                dest = new RectangleF(rect.Left, rect.Top + rect.Height * (1 - available),
                    rect.Width, rect.Height * available);
                empty = new RectangleF(rect.Left, rect.Top, rect.Width, rect.Height * (1 - available));
            }
            else
            {
                var covered = absHeight / RatioY;
                src = new RectangleF(0, end - covered, _Image.Width, covered);
                dest = rect;
                empty = new RectangleF(rect.Left, rect.Top, rect.Width, 0);
            }
            g.DrawImage(_Image, dest, src, GraphicsUnit.Pixel);
            return empty;
        }

        public void DrawBitmap(Graphics g, RectangleF rect)
        {
            //Try to be fast
            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
            g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighSpeed;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighSpeed;

            if (_PositionY == 0) return;

            _Image.UnlockBits(_LockedData);

            try
            {
                var empty = DrawBitmapSection(g, rect, 0, _PositionY);
                if (_WrapImage)
                {
                    DrawBitmapSection(g, empty, _PositionY, _Image.Height);
                }
            }
            finally
            {
                _LockedData = _Image.LockBits(new Rectangle(0, 0, _Image.Width, _Image.Height),
                    ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
            }
        }

        public Bitmap GetBitmap()
        {
            if (_Image == null)
            {
                throw new InvalidOperationException();
            }
            _Image.UnlockBits(_LockedData);
            var ret = _Image;
            _Image = null;
            _LockedData = null;
            return ret;
        }
    }
}
