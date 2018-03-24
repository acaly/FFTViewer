using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FFTViewer
{
    class LoopbackCaptureProvider : IAudioProvider
    {
        class Reader : IAudioReader
        {
            public Reader(LoopbackCaptureProvider provider, int len)
            {
                _Provider = provider;
                BufferLength = len;
                _Buffer = new CaptureBuffer(_Provider.Format, len);

                _Provider._Capture.DataAvailable += Capture_DataAvailable;
            }

            private LoopbackCaptureProvider _Provider;
            private CaptureBuffer _Buffer;

            public IAudioProvider Provider => _Provider;
            public int BufferLength { get; private set; }

            public void Dispose()
            {
                if (_Provider != null)
                {
                    _Provider._Capture.DataAvailable -= Capture_DataAvailable;
                    _Provider = null;
                }
            }

            private void Capture_DataAvailable(object sender, WaveInEventArgs e)
            {
                _Buffer.Write(e.Buffer, e.BytesRecorded);
            }

            public unsafe void* GetRawBuffer()
            {
                return _Buffer.Pointer.ToPointer();
            }
        }

        public LoopbackCaptureProvider()
        {
            _Capture = new WasapiLoopbackCapture();
            Format = _Capture.WaveFormat;
            _Capture.StartRecording();
        }

        private WasapiLoopbackCapture _Capture;

        public event Action StateChanged;

        public WaveFormat Format { get; private set; }
        public int SourceCount => 1;
        public bool IsPlaying => true;

        public void Dispose()
        {
            if (_Capture != null)
            {
                _Capture.StopRecording();
                _Capture.Dispose();
                _Capture = null;
            }
        }

        public IAudioPlayControl GetPlayControl()
        {
            return new EmptyPlayControl();
        }

        public IAudioReader GetReader(int sourceIndex, int bufferLength)
        {
            return new Reader(this, bufferLength);
        }
    }
}
