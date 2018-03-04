using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FFTViewer
{
    interface ILabelXRenderer
    {
        void Draw(Pen pen, float x);
    }

    interface ILabelXProvider
    {
        void DrawAll(ILabelXRenderer r);
    }
}
