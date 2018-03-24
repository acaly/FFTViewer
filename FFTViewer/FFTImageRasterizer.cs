using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace FFTViewer
{
    class FFTImageRasterizer
    {
        public FFTImageRasterizer(int width, Color c0, Color c)
        {
            _Buffer = new int[width];
            _C0 = c0;
            _C = c;
        }

        private Color _C0, _C;

        private int[] _Buffer;

        private bool _HasLast;
        private float _LastPixelX;
        private float _LastPixelY;

        private bool _HasCurrent;
        private float _CurrentPixelX;
        private float _CurrentPixelY;

        public void Begin()
        {
            _HasLast = false;
        }

        public void End()
        {
            if (_HasCurrent)
            {
                //DrawPoint(_Buffer.Length, _LastPixelY);
            }
        }

        public void DrawPoint(float x, float y)
        {
            if (!_HasLast)
            {
                _HasLast = true;
                _LastPixelX = x;
                _LastPixelY = y;
                _HasCurrent = false;

                for (int i = 0; i <= x; ++i)
                {
                    _Buffer[i] = _C0.ToArgb();
                }
                return;
            }

            if (!_HasCurrent)
            {
                _HasCurrent = true;
                _CurrentPixelX = x;
                _CurrentPixelY = y;
            }
            else
            {
                _CurrentPixelX = x;
                _CurrentPixelY = Math.Max(y, _CurrentPixelY);
            }

            if (Math.Ceiling(_LastPixelX) < Math.Floor(_CurrentPixelX))
            {
                for (int i = (int)Math.Ceiling(_LastPixelX); i <= Math.Floor(_CurrentPixelX); ++i)
                {
                    if (i < 0)
                    {
                        continue;
                    }
                    if (i >= _Buffer.Length)
                    {
                        break;
                    }
                    float pos = (i - _LastPixelX) / (_CurrentPixelX - _LastPixelX);
                    float drawY = _LastPixelY * (1 - pos) + _CurrentPixelY * pos;
                    _Buffer[i] = GetColor(drawY);
                }

                _LastPixelX = _CurrentPixelX;
                if (_LastPixelX == Math.Floor(_LastPixelX)) _LastPixelX += 1;
                _LastPixelY = _CurrentPixelY;

                _HasCurrent = false;
            }
        }

        private int GetColor(float val)
        {
            var x = val;
            if (x > 1) x = 1;
            if (x < 0) x = 0;
            var y = 1 - x;
            var r = _C0.R * y + _C.R * x;
            var g = _C0.G * y + _C.G * x;
            var b = _C0.B * y + _C.B * x;
            return Color.FromArgb((int)r, (int)g, (int)b).ToArgb();
        }

        public void CopyTo(IntPtr ptr)
        {
            Marshal.Copy(_Buffer, 0, ptr, _Buffer.Length);
        }
    }
}
