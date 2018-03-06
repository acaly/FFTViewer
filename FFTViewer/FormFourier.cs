using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FFTViewer
{
    public partial class FormFourier : Form
    {
        public FormFourier()
        {
            InitializeComponent();
        }

        private void FormFourier_Shown(object sender, EventArgs e)
        {
            string filename;
            using (OpenFileDialog dialog = new OpenFileDialog())
            {
                dialog.Filter = "*.mp3|*.mp3";
                if (dialog.ShowDialog() == DialogResult.Cancel)
                {
                    Environment.Exit(0);
                }
                filename = dialog.FileName;
            }

            Text += " - " + Path.GetFileNameWithoutExtension(filename);

            _Reader = new Mp3Reader(filename);
            _Compressed = SpectrumCompressor.Compress(_Reader, 2000);
            _FFTPlayer = new FFTPlayer(_Reader);

            _Clock = new PlayerTimer { TotalTimeMs = _Reader.TotalTimeMs };
            _Clock.StateChanged += () =>
            {
                _Recorder.Enabled = _Clock.IsRunning;
            };

            _RendererSpectrum = new Renderer
            {
                Target = pictureBox1,
                Source = () => _Compressed,

                LabelsXForeground = _Clock,

                ScaleY = CalcScaleYSpec,

                MarginTop = 10,
                MarginBottom = 10,
            };
            _RendererSpectrum.Start(120);
            _RendererSpectrum.DoubleClick += RendererSpectrum_DoubleClick;

            _Range = new NoteRange { Base = NoteConstants.C4Position, OffsetMin = -4, OffsetMax = 5 };
            _NoteLabel = new NoteLabelGroup { Range = _Range };
            _Recorder = new FFTImageRecorder(600, 800, 3, Color.White, Color.Green);
            _RendererFFT = new Renderer
            {
                Target = pictureBox2,
                Source = GetFFTData,

                ScaleX = _Range.CalculateLogScale,
                ScaleY = CalcScaleYFFT,
                FixedRangeY = 10,

                MarginLeft = 10,
                MarginRight = 10,
                MarginTop = 0,
                MarginBottom = 0,

                LabelsXBackground = _NoteLabel,

                ImageRecorder = _Recorder,
            };
            _RendererFFT.Start(50);

            _WavePlayer = new WavePlayer(_Reader);
        }

        private float CalcScaleYFFT(float r)
        {
            const float Min = 0;
            const float Mid = 1E-0f;
            const float Max = 100f;
            const float RLow = 0.2f;
            float ret;
            if (r < Min)
            {
                return 1;
            }
            if (r < Mid)
            {
                ret = RLow * (r / Mid);
            }
            else
            {
                ret = (float)((Math.Log(r) - Math.Log(Mid)) / (Math.Log(Max) - Math.Log(Mid)));
                ret = ret * (1 - RLow) + RLow;
            }
            return 1 - ret * _ScaleY;
        }

        private float CalcScaleYSpec(float val)
        {
            if (val > 1) return 0;
            if (val < -1) return 1;
            return (1 - val) / 2;
        }

        private Mp3Reader _Reader;
        private float[] _Compressed;
        private FFTPlayer _FFTPlayer;
        private Renderer _RendererSpectrum;
        private Renderer _RendererFFT;
        private FFTImageRecorder _Recorder;
        private WavePlayer _WavePlayer;
        private PlayerTimer _Clock;
        private NoteRange _Range;
        private NoteLabelGroup _NoteLabel;

        private float _ScaleY = 1;
        private int _FFTChannel = 0;

        private const int FFTLength = 8192; //Magic number for performing FFT.

        private float[] GetFFTData()
        {
            _FFTPlayer.Calculate(_FFTChannel, (int)((_Clock.TimeMs) / 1000f * 44100), FFTLength);
            return _FFTPlayer.Buffer;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            _ScaleY *= 2;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            _ScaleY /= 2;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            _WavePlayer.Play();
            _Clock.Start();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            _WavePlayer.Pause();
            _Clock.Pause();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            _WavePlayer.Stop();
            _Clock.Stop();
        }

        private void RendererSpectrum_DoubleClick(float x, float y)
        {
            _WavePlayer.SetPosition(x);
            _Clock.SetPosition(x * _Reader.TotalTimeMs);
        }

        private void button6_Click(object sender, EventArgs e)
        {
            _Range.Base *= NoteConstants.Inc2;
        }

        private void button7_Click(object sender, EventArgs e)
        {
            _Range.Base *= NoteConstants.Inc;
        }

        private void button8_Click(object sender, EventArgs e)
        {
            _Range.Base /= NoteConstants.Inc;
        }

        private void button9_Click(object sender, EventArgs e)
        {
            _Range.Base /= NoteConstants.Inc2;
        }

        private void button10_Click(object sender, EventArgs e)
        {
            _Recorder.RatioY *= 1.1f;
        }

        private void button11_Click(object sender, EventArgs e)
        {
            _Recorder.RatioY /= 1.1f;
        }

        private void button12_Click(object sender, EventArgs e)
        {
            if (button12.Text == "L")
            {
                _FFTChannel = 1;
                button12.Text = "R";
            }
            else if (button12.Text == "R")
            {
                _FFTChannel = 2;
                button12.Text = "x";
            }
            else
            {
                _FFTChannel = 0;
                button12.Text = "L";
            }
        }
    }
}
