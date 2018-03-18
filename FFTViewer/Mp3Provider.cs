using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FFTViewer
{
    class Mp3Provider : IAudioProvider, IAudioPlayControl
    {
        private class Mp3Reader : IAudioReader
        {
            public Mp3Reader(Mp3Provider provider, int index, int bufferLength)
            {
                _Provider = provider;
                _Index = index;
                _BufferLength = bufferLength;
            }

            private readonly Mp3Provider _Provider;
            private readonly int _Index;
            private readonly int _BufferLength;

            public int BufferLength => _BufferLength;
            public IAudioProvider Provider => _Provider;

            public void Dispose()
            {
            }

            public void Read(float[] buffer)
            {
                int frame = (int)(_Provider._Timer.TimeMs / 1000 * 44100);
                switch (_Index)
                {
                    case 0:
                        for (int i = 0; i < _BufferLength; ++i)
                        {
                            buffer[i] = _Provider._DataL[frame + i];
                        }
                        break;
                    case 1:
                        for (int i = 0; i < _BufferLength; ++i)
                        {
                            buffer[i] = _Provider._DataR[frame + i];
                        }
                        break;
                    case 2:
                        for (int i = 0; i < _BufferLength; ++i)
                        {
                            buffer[i] = _Provider._DataL[frame + i] - _Provider._DataR[frame + i];
                        }
                        break;
                }
            }
        }

        public Mp3Provider(string filename)
        {
            byte[] convertedData;
            int size;
            WaveFormat f = new WaveFormat(44100, 16, 2);
            using (Mp3FileReader r = new Mp3FileReader(filename))
            {
                using (var conv = new WaveFormatConversionStream(f, r))
                {
                    int len = (int)conv.Length;
                    size = len / 4;
                    convertedData = new byte[len];
                    conv.Read(convertedData, 0, len);
                }
            }

            _DataL = new float[size];
            _DataR = new float[size];
            using (var ms = new MemoryStream(convertedData))
            {
                using (var br = new BinaryReader(ms))
                {
                    for (int i = 0; i < size; ++i)
                    {
                        _DataL[i] = br.ReadInt16() / (float)Int16.MaxValue;
                        _DataR[i] = br.ReadInt16() / (float)Int16.MaxValue;
                    }
                }
            }

            _RawData = convertedData;
            _RawFormat = f;
            _TotalTimeMs = convertedData.Length * 1000L / f.AverageBytesPerSecond;

            _Timer = new PlayerTimer(_TotalTimeMs);
            _Timer.StateChanged += Timer_StateChanged;

            _WavePlayer = new WavePlayer(_RawData, _RawFormat, _TotalTimeMs);
        }

        private void Timer_StateChanged()
        {
            StateChanged?.Invoke();
        }

        private float[] _DataL, _DataR;

        private byte[] _RawData;
        private WaveFormat _RawFormat;
        private long _TotalTimeMs;

        private PlayerTimer _Timer;
        private WavePlayer _WavePlayer;

        public int Rate => 44100;
        public int SourceCount => 3;
        public bool IsPlaying => _Timer.IsRunning;

        public float PositionRatio
        {
            get => _Timer.TimeMs / _Timer.TotalTimeMs;
            set
            {
                _Timer.SetPosition(value * _Timer.TotalTimeMs);
                _WavePlayer.SetPosition(value);
            }
        }

        public IAudioReader GetReader(int sourceIndex, int bufferLength)
        {
            return new Mp3Reader(this, sourceIndex, bufferLength);
        }

        public void Dispose()
        {
        }

        public void Update()
        {
            _Timer.Update();
        }

        public float[] GetSpectrum()
        {
            return SpectrumCompressor.Compress(_DataL, _DataR, 8000);
        }

        public void Play()
        {
            _Timer.Start();
            _WavePlayer.Play();
        }

        public void Pause()
        {
            _Timer.Pause();
            _WavePlayer.Pause();
        }

        public void Stop()
        {
            _Timer.Stop();
            _WavePlayer.Stop();
        }

        public IAudioPlayControl GetPlayControl()
        {
            return this;
        }

        public event Action StateChanged;
    }
}
