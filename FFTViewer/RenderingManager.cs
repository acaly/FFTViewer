using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Timer = System.Timers.Timer;

namespace FFTViewer
{
    class RenderingManager
    {
        private Timer _Timer;

        public Control Target;
        public float MarginLeft, MarginRight, MarginTop, MarginBottom;

        public event Action<float, float> DoubleClick;

        public readonly List<IRenderingLayer> Layers = new List<IRenderingLayer>();

        public void Start(int interval)
        {
            Target.FindForm().FormClosed += Renderer_FormClosed;
            Target.Paint += Target_Paint;
            Target.Resize += Target_Resize;
            Target.DoubleClick += Target_DoubleClick;

            if (interval != 0)
            {
                _Timer = new Timer(interval);
                _Timer.Elapsed += Timer_Elapsed;
                _Timer.Start();
            }

            Target.Invalidate();
        }

        private void Renderer_FormClosed(object sender, FormClosedEventArgs e)
        {
            Stop();
        }

        private void Target_DoubleClick(object sender, EventArgs e)
        {
            var region = Target.ClientRectangle;
            var left = region.Left + MarginLeft;
            var top = region.Top + MarginTop;
            var width = region.Width - MarginLeft - MarginRight;
            var height = region.Height - MarginTop - MarginBottom;

            var p = Target.PointToClient(Control.MousePosition);
            var x = (p.X - left) / width;
            var y = (p.Y - top) / height;

            if (x >= 0 && x <= 1 && y >= 0 && y <= 1)
            {
                DoubleClick?.Invoke(x, y);
            }
        }

        private void Target_Resize(object sender, EventArgs e)
        {
            Target.Invalidate();
        }

        private void Timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                Target.Invoke((Action)Target.Invalidate);
            }
            catch
            {
            }
        }

        public void Stop()
        {
            if (_Timer != null)
            {
                _Timer.Stop();
                _Timer.Dispose();
                _Timer = null;
            }
        }

        private void Target_Paint(object sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            g.Clear(Color.White);

            var scaled = new ScaledGraphics { Target = g };

            var region = Target.ClientRectangle;
            scaled.Rect.X = region.Left + MarginLeft;
            scaled.Rect.Y = region.Top + MarginTop;
            scaled.Rect.Width = region.Width - MarginLeft - MarginRight;
            scaled.Rect.Height = region.Height - MarginTop - MarginBottom;
            
            foreach (var layer in Layers)
            {
                layer.Draw(scaled);
            }
        }
    }
}
