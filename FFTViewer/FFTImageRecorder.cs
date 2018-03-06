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
            _LineBuffer = new int[imageWidth];
            _LockedData = _Image.LockBits(new Rectangle(0, 0, imageWidth, imageHeight),
                ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
            RatioY = ratioY;
            _C0 = c0;
            _C = c;
        }
        
        private Bitmap _Image;
        private BitmapData _LockedData;
        private int _PositionY;
        private int[] _LineBuffer; //int32 argb values
        private bool _WrapImage = false;
        public float RatioY;
        private Color _C0, _C;

        public bool Enabled;

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

            float pixelWidth = rect.Width / _Image.Width;
            float pixelEnd = rect.Left + pixelWidth;
            int lineBufferPosition = 0;
            NewPixel();
            for (int i = 0; i < points.Length; ++i)
            {
                var point = points[i];
                if (point.X <= rect.Left) continue;
                if (point.X >= rect.Right)
                {
                    break;
                }
                while (point.X > pixelEnd)
                {
                    pixelEnd += pixelWidth;
                    _LineBuffer[lineBufferPosition++] = FinishPixel().ToArgb();
                    NewPixel();
                }
                AddPixelData((rect.Bottom - point.Y) / rect.Height);
            }
            if (lineBufferPosition < _LineBuffer.Length)
            {
                _LineBuffer[lineBufferPosition] = FinishPixel().ToArgb();
            }

            var ptr = IntPtr.Add(_LockedData.Scan0, _LockedData.Stride * _PositionY);
            _PositionY += 1;
            if (_PositionY >= _Image.Height)
            {
                _PositionY = 0;
                _WrapImage = true;
            }

            Marshal.Copy(_LineBuffer, 0, ptr, _LineBuffer.Length);
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

        private float _PixelAccumulate;
        private float _LastPixelValue;

        private void InitPixel()
        {
            _LastPixelValue = 0;
        }

        private void NewPixel()
        {
            _PixelAccumulate = -1;
        }

        private void AddPixelData(float val)
        {
            if (val > 1) val = 1;
            if (val < 0) val = 0;
            //Max
            if (_PixelAccumulate < val) _PixelAccumulate = val;
        }

        private Color FinishPixel()
        {
            if (_PixelAccumulate < 0)
            {
                _PixelAccumulate = _LastPixelValue;
            }
            else
            {
                _LastPixelValue = _PixelAccumulate;
            }
            return Interpolate(_PixelAccumulate);
        }

        private Color Interpolate(float val)
        {
            var x = val;
            var y = 1 - val;
            var r = _C0.R * y + _C.R * x;
            var g = _C0.G * y + _C.G * x;
            var b = _C0.B * y + _C.B * x;
            return Color.FromArgb((int)r, (int)g, (int)b);
        }
    }
}
