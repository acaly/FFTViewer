using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FFTViewer
{
    interface IFFTDataReceiver
    {
        void WriteData(RectangleF rect, PointF[] points, float[] channel2);
    }
}
