using MathNet.Numerics;
using MathNet.Numerics.IntegralTransforms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FFTViewer
{
    class FFTPlayer
    {
        public FFTPlayer(IAudioReader reader)
        {
            _Reader = reader;

            int bufferLength = reader.BufferLength;
            _ReadBuffer = new Complex32[bufferLength];
            Buffer = new float[bufferLength];

            _WindowFunction = new float[bufferLength];
            float sigma = bufferLength / 2 / WindowRange;
            int offset = bufferLength / 2;
            for (int i = 0; i < bufferLength; ++i)
            {
                float x = (i - offset);
                _WindowFunction[i] = (float)Math.Exp(-x * x / 2 / sigma / sigma);
            }
        }

        private const float WindowRange = 3.0f;

        private readonly IAudioReader _Reader;
        private Complex32[] _ReadBuffer;
        private float[] _WindowFunction;
        public float[] Buffer;

        public void Calculate()
        {
            _Reader.Read(Buffer); //TODO do not use this buffer. Fix after changing to FFTW

            for (int i = 0; i < _ReadBuffer.Length; ++i)
            {
                _ReadBuffer[i] = new Complex32(Buffer[i] * _WindowFunction[i], 0);
            }

            Fourier.Forward(_ReadBuffer);

            for (int i = 0; i < _ReadBuffer.Length; ++i)
            {
                Buffer[i] = (float)_ReadBuffer[i].Magnitude;
            }
        }
    }
}
