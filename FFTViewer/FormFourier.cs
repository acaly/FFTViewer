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

        private string OpenFile()
        {
            string filename;
            using (OpenFileDialog dialog = new OpenFileDialog())
            {
                dialog.Filter = "*.mp3,*.wav|*.mp3;*.wav";
                if (dialog.ShowDialog() == DialogResult.Cancel)
                {
                    this.Close();
                    return null;
                }
                filename = dialog.FileName;
            }
            Text += " - " + Path.GetFileNameWithoutExtension(filename);
            return filename;
        }

        private void FormFourier_Shown(object sender, EventArgs e)
        {
            //_Provider = new Mp3Provider(OpenFile());
            _Provider = new LoopbackCaptureProvider();
            //_Provider = new WaveInProvider();

            _Reader = _Provider.GetReader(0, FFTLength(_Provider.Format.SampleRate));
            _PlayControl = _Provider.GetPlayControl();
            _Compressed = _PlayControl.GetSpectrum();
            _FFTPlayer = new FFTPlayer(_Reader);

            _RendererSpectrum = new SpectrumRenderer
            {
                Source = () => _Compressed,

                ScaleY = CalcScaleYSpec,

            };
            _ManagerSpectrum = new RenderingManager
            {
                Target = pictureBox1,
                MarginTop = 10,
                MarginBottom = 10,
            };
            _ManagerSpectrum.Layers.Add(_RendererSpectrum);
            _ManagerSpectrum.Layers.Add(new PlayControlLabelXProvider(_PlayControl));

            _ManagerSpectrum.Start(120);
            _ManagerSpectrum.DoubleClick += RendererSpectrum_DoubleClick;

            _Range = new NoteRange
            {
                Base = NoteConstants.C4Position(_Provider.Format.SampleRate),
                OffsetMin = -4,
                OffsetMax = 5,
            };
            _NoteLabel = new NoteLabelGroup { Range = _Range };
            _Recorder = new FFTImageRecorder(600, 800, 3, Color.White, Color.Red, Color.Yellow);
            _RendererFFT = new SpectrumRenderer
            {
                //Source = GetFFTData,
                Source2 = GetFFTData2,

                ScaleX = _Range.CalculateLogScale,
                ScaleY = CalcScaleYFFT,
                FixedRangeY = 10,
            };
            _RendererFFT.DataReceivers.Add(_Recorder);
            _ManagerFFT = new RenderingManager
            {
                Target = pictureBox2,

                MarginLeft = 10,
                MarginRight = 10,
                MarginTop = 0,
                MarginBottom = 0,
            };
            _ManagerFFT.Layers.Add(_Recorder);
            _ManagerFFT.Layers.Add(_RendererFFT);
            _ManagerFFT.Layers.Add(_NoteLabel);

            _Provider.StateChanged += () =>
            {
                _Recorder.Enabled = _Provider.IsPlaying;
            };
            _Recorder.Enabled = _Provider.IsPlaying;

            _ManagerFFT.Start(15);
        }

        private float CalcScaleYFFT(float r)
        {
            const float Min = 0;
            const float Mid = 1E-0f;
            const float Max = 100f;
            const float RLow = 0.1f;
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

        private IAudioProvider _Provider;
        private IAudioReader _Reader;
        private IAudioPlayControl _PlayControl;
        private float[] _Compressed;
        private FFTPlayer _FFTPlayer;
        private SpectrumRenderer _RendererSpectrum;
        private RenderingManager _ManagerSpectrum;
        private SpectrumRenderer _RendererFFT;
        private RenderingManager _ManagerFFT;
        private FFTImageRecorder _Recorder;
        private NoteRange _Range;
        private NoteLabelGroup _NoteLabel;

        private float _ScaleY = 1;
        private int _FFTChannel = 0;
        
        private static int FFTLength(int sampleRate)
        {
            return 8 * (int)(0.020f * sampleRate); //Magic number for performing FFT.
        }

        private float[] GetFFTData()
        {
            _PlayControl.Update();
            _FFTPlayer.Calculate();
            return _FFTPlayer.Buffer;
        }

        private void GetFFTData2(out float[] d1, out float[] d2)
        {
            _PlayControl.Update();
            _FFTPlayer.Calculate();
            d1 = _FFTPlayer.Buffer;
            d2 = _FFTPlayer.Buffer2;
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
            _PlayControl.Play();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            _PlayControl.Pause();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            _PlayControl.Stop();
        }

        private void RendererSpectrum_DoubleClick(float x, float y)
        {
            _PlayControl.PositionRatio = x;
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

        private void FormFourier_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (_Reader != null)
            {
                _Reader.Dispose();
                _Reader = null;
            }
            if (_Provider != null)
            {
                _Provider.Dispose();
                _Provider = null;
            }
        }
    }
}
