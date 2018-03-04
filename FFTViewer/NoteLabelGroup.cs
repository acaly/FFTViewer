using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static FFTViewer.NoteConstants;

namespace FFTViewer
{
    class NoteLabelGroup : ILabelXProvider
    {
        public NoteRange Range;

        private void DrawGroup(ILabelXRenderer r, int i)
        {
            Pen p = (i & 1) == 0 ? Pens.Red : Pens.Blue;
            float v = Range.GroupBase(i);
            r.Draw(p, v);
            v *= Inc2;
            r.Draw(p, v);
            v *= Inc2;
            r.Draw(p, v);
            v *= Inc;
            r.Draw(p, v);
            v *= Inc2;
            r.Draw(p, v);
            v *= Inc2;
            r.Draw(p, v);
            v *= Inc2;
            r.Draw(p, v);
        }

        public void DrawAll(ILabelXRenderer r)
        {
            for (int i = 0; i < Range.GroupCount; ++i)
            {
                DrawGroup(r, i);
            }
        }
    }
}
