﻿using System;
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
                    this.Close();
                    return;
                }
                filename = dialog.FileName;
            }

            Text += " - " + Path.GetFileNameWithoutExtension(filename);

            _Provider = new LoopbackCaptureProvider();//new Mp3Provider(filename);
            _Reader = _Provider.GetReader(0, FFTLength);
            _PlayControl = _Provider.GetPlayControl();
            _Compressed = _PlayControl.GetSpectrum();
            _FFTPlayer = new FFTPlayer(_Reader);

            _Provider.StateChanged += () =>
            {
                _Recorder.Enabled = _Provider.IsPlaying;
            };

            _RendererSpectrum = new Renderer
            {
                Target = pictureBox1,
                Source = () => _Compressed,

                LabelsXForeground = new PlayControlLabelXProvider(_PlayControl),

                ScaleY = CalcScaleYSpec,

                MarginTop = 10,
                MarginBottom = 10,
            };
            _RendererSpectrum.Start(120);
            _RendererSpectrum.DoubleClick += RendererSpectrum_DoubleClick;

            _Range = new NoteRange
            {
                Base = NoteConstants.C4Position(_Provider.Rate),
                OffsetMin = -4,
                OffsetMax = 5,
            };
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
            _RendererFFT.Start(15);
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
        private Renderer _RendererSpectrum;
        private Renderer _RendererFFT;
        private FFTImageRecorder _Recorder;
        private NoteRange _Range;
        private NoteLabelGroup _NoteLabel;

        private float _ScaleY = 1;
        private int _FFTChannel = 0;

        private const int FFTLength = 16384; //Magic number for performing FFT.

        private float[] GetFFTData()
        {
            _PlayControl.Update();
            _FFTPlayer.Calculate();
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
