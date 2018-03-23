using MathNet.Numerics;
using MathNet.Numerics.IntegralTransforms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FFTViewer
{
    unsafe class FFTPlayer
    {
        public FFTPlayer(IAudioReader reader)
        {
            _Reader = reader;
            _RawDataReader = FFTDataReaderFactory.CreateForFormat(reader.Provider.Format);

            int bufferLength = reader.BufferLength;
            Buffer = new float[bufferLength];

            UIntPtr allocSize = (UIntPtr)(8 * (uint)bufferLength);
            _Input = FFTW.Malloc(allocSize);
            _Output = FFTW.Malloc(allocSize);
            _Plan = FFTW.PlanDft1D(bufferLength, _Input, _Output, 1, 0 /* FFTW_MEASURE */);
            for (int i = 0; i < bufferLength; ++i)
            {
                _Input[i].Imaginary = 0; //Clear only imaginary
            }

            _WindowFunction = new float[bufferLength];
            float sigma = bufferLength / 2 / WindowRange;
            int offset = bufferLength / 2;
            for (int i = 0; i < bufferLength; ++i)
            {
                float x = (i - offset);
                _WindowFunction[i] = (float)Math.Exp(-x * x / 2 / sigma / sigma);
            }
        }

        ~FFTPlayer()
        {
            //TODO use IDisposable
            FFTW.DestroyPlan(_Plan);
            FFTW.Free(_Input);
            FFTW.Free(_Output);
        }

        private const float WindowRange = 4;

        private readonly IAudioReader _Reader;
        private readonly IFFTDataReader _RawDataReader;

        private IntPtr _Plan;
        private FFTW.Complex* _Input;
        private FFTW.Complex* _Output;
        private float[] _WindowFunction;
        public float[] Buffer;

        public void Calculate()
        {
            _RawDataReader.ReadToBuffer(_WindowFunction.Length, _Reader.GetRawBuffer(), _WindowFunction, _Input);
            FFTW.Execute(_Plan);

            for (int i = 0; i < _WindowFunction.Length; ++i)
            {
                Buffer[i] = (float)Math.Sqrt(_Output[i].Length());
            }
        }
    }
}
