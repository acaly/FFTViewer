using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FFTViewer
{
    class NoteLabelGroup : LabelXLayer
    {
        public NoteRange Range;

        private void DrawGroup(int i, float distanct)
        {
            Pen p = (i & 1) == 0 ? Pens.Red : Pens.Blue;
            var v = i * distanct;
            var inc = distanct / 12;
            var inc2 = inc * 2;
            DrawLine(p, v);
            v += inc2;
            DrawLine(p, v);
            v += inc2;
            DrawLine(p, v);
            v += inc;
            DrawLine(p, v);
            v += inc2;
            DrawLine(p, v);
            v += inc2;
            DrawLine(p, v);
            v += inc2;
            DrawLine(p, v);
        }

        public override void DrawAll()
        {
            var distance = 1.0f / (Range.GroupCount - 1);
            for (int i = 0; i < Range.GroupCount; ++i)
            {
                DrawGroup(i, distance);
            }
        }
    }
}
