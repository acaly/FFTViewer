﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FFTViewer
{
    interface IAudioReader : IDisposable
    {
        IAudioProvider Provider { get; }
        int BufferLength { get; }
        void Read(float[] buffer);
    }

    interface IAudioPlayControl
    {
        float[] GetSpectrum();
        void Update();

        void Play();
        void Pause();
        void Stop();

        float PositionRatio { get; set; }
    }

    interface IAudioProvider : IDisposable
    {
        int Rate { get; }
        int SourceCount { get; }
        bool IsPlaying { get; }
        IAudioReader GetReader(int sourceIndex, int bufferLength);
        IAudioPlayControl GetPlayControl();
        event Action StateChanged;
    }

    class EmptyPlayControl : IAudioPlayControl
    {
        public float PositionRatio
        {
            get => 0;
            set { }
        }
        
        public float[] GetSpectrum()
        {
            return new[] { 0f, 0f, 0f };
        }

        public void Pause()
        {
        }

        public void Play()
        {
        }

        public void Stop()
        {
        }

        public void Update()
        {
        }
    }

    class PlayControlLabelXProvider : ILabelXProvider
    {
        public PlayControlLabelXProvider(IAudioPlayControl playControl)
        {
            _PlayControl = playControl;
        }

        private readonly IAudioPlayControl _PlayControl;

        public void DrawAll(ILabelXRenderer r)
        {
            r.Draw(System.Drawing.Pens.Red, _PlayControl.PositionRatio);
        }
    }
}
