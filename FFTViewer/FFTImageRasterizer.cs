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
        public FFTImageRasterizer(int width, Color c0, Color c1, Color c2)
        {
            _Buffer = new int[width];
            _C0 = c0;
            _C1 = c1;
            _C2 = c2;
        }

        private Color _C0, _C1, _C2;

        private int[] _Buffer;

        private bool _HasLast;
        private float _LastPixelX;
        private float _LastPixelY;
        private float _LastPixelZ;

        private bool _HasCurrent;
        private float _CurrentPixelX;
        private float _CurrentPixelY;
        private float _CurrentPixelZ;

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

        public void DrawPoint(float x, float y, float ex)
        {
            if (!_HasLast)
            {
                _HasLast = true;
                _LastPixelX = x;
                _LastPixelY = y;
                _LastPixelZ = ex;
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
                _CurrentPixelZ = ex;
            }
            else
            {
                _CurrentPixelX = x;
                _CurrentPixelY = Math.Max(y, _CurrentPixelY);
                _CurrentPixelZ = ex; //TODO how to update z?
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
                    float drawZ = _LastPixelZ * (1 - pos) + _CurrentPixelZ * pos;
                    _Buffer[i] = GetColor(drawY, drawZ);
                }

                _LastPixelX = _CurrentPixelX;
                if (_LastPixelX == Math.Floor(_LastPixelX)) _LastPixelX += 1;
                _LastPixelY = _CurrentPixelY;
                _LastPixelZ = _CurrentPixelZ;

                _HasCurrent = false;
            }
        }

        private static Color InterceptColor(Color c1, Color c2, float x)
        {
            if (x > 1) x = 1;
            if (x < 0) x = 0;
            var y = 1 - x;
            var r = c1.R * y + c2.R * x;
            var g = c1.G * y + c2.G * x;
            var b = c1.B * y + c2.B * x;
            return Color.FromArgb((int)r, (int)g, (int)b);
        }

        private int GetColor(float val, float val2)
        {
            return InterceptColor(_C0, InterceptColor(_C1, _C2, val2), val).ToArgb();
        }

        public void CopyTo(IntPtr ptr)
        {
            Marshal.Copy(_Buffer, 0, ptr, _Buffer.Length);
        }
    }
}
