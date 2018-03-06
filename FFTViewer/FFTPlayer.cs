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
        public FFTPlayer(Mp3Reader reader)
        {
            _Reader = reader;
        }

        private readonly Mp3Reader _Reader;
        private Complex32[] _ReadBuffer;
        public float[] Buffer;

        public void Calculate(int channel, int offset, int len)
        {
            if (_ReadBuffer == null || _ReadBuffer.Length < len)
            {
                _ReadBuffer = new Complex32[len];
                Buffer = new float[len];
            }

            float[] data = channel == 0 ? _Reader.DataL : channel == 1 ? _Reader.DataR : _Reader.DataDiff;
            
            if (offset + len > data.Length)
            {
                offset = data.Length - len;
            }

            for (int i = 0; i < len; ++i)
            {
                _ReadBuffer[i] = new Complex32(data[i + offset], 0);
            }

            Fourier.Forward(_ReadBuffer);
            for (int i = 0; i < len; ++i)
            {
                Buffer[i] = (float)_ReadBuffer[i].Magnitude;
            }
        }
    }
}
