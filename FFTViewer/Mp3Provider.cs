using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
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

                _FrameLength = _Provider._RawFormat.BitsPerSample / 8 * _Provider._RawFormat.Channels;
                _MaxOffset = _Provider._RawData.Length - _FrameLength * _BufferLength;
            }

            private readonly Mp3Provider _Provider;
            private readonly int _Index;
            private readonly int _BufferLength;

            private readonly int _FrameLength;
            private readonly int _MaxOffset;

            public int BufferLength => _BufferLength;
            public IAudioProvider Provider => _Provider;

            public void Dispose()
            {
            }

            public unsafe void* GetRawBuffer()
            {
                int frame = (int)(_Provider._Timer.TimeMs / 1000 * 44100);
                int offset = frame * _FrameLength;
                if (offset > _MaxOffset)
                {
                    offset = _MaxOffset;
                }
                return (_Provider._PinnedPtr + offset).ToPointer();
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

            _RawData = convertedData;
            _PinnedHandle = GCHandle.Alloc(_RawData, GCHandleType.Pinned);
            _PinnedPtr = _PinnedHandle.AddrOfPinnedObject();

            _RawFormat = f;
            _TotalTimeMs = convertedData.Length * 1000L / f.AverageBytesPerSecond;

            _Timer = new PlayerTimer(_TotalTimeMs);
            _Timer.StateChanged += Timer_StateChanged;

            _WavePlayer = new WavePlayer(_RawData, _RawFormat, _TotalTimeMs);
        }

        ~Mp3Provider()
        {
            DoDispose();
        }

        private void Timer_StateChanged()
        {
            StateChanged?.Invoke();
        }

        private bool _Disposed = false;
        
        private byte[] _RawData;
        private GCHandle _PinnedHandle;
        private IntPtr _PinnedPtr;

        private WaveFormat _RawFormat;
        private long _TotalTimeMs;

        private PlayerTimer _Timer;
        private WavePlayer _WavePlayer;

        public WaveFormat Format => _RawFormat;
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

        private void DoDispose()
        {
            if (_PinnedHandle.IsAllocated)
            {
                _PinnedHandle.Free();
            }
        }

        public void Dispose()
        {
            if (!_Disposed)
            {
                _Disposed = true;
                DoDispose();
                GC.SuppressFinalize(this);
            }
        }

        public void Update()
        {
            _Timer.Update();
        }

        public float[] GetSpectrum()
        {
            return SpectrumCompressor.CompressInt16(_RawData, 8000);
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
