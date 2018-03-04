using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FFTViewer
{
    class WavePlayer
    {
        public WavePlayer(Mp3Reader r)
        {
            _Device = new WaveOut();
            _Provider = new RawSourceWaveStream(r.RawData, 0, r.RawData.Length, r.RawFormat);
            _Device.Init(_Provider);
            _TotalMs = r.TotalTimeMs;
        }

        private WaveOut _Device;
        private RawSourceWaveStream _Provider;
        private float _TotalMs;

        public void SetPosition(float posRatio)
        {
            _Provider.CurrentTime = TimeSpan.FromMilliseconds(posRatio * _TotalMs);
        }

        public void Pause()
        {
            _Device.Pause();
        }

        public void Play()
        {
            _Device.Play();
        }

        public void Stop()
        {
            _Device.Stop();
            _Provider.CurrentTime = TimeSpan.FromSeconds(0);
        }
    }
}
