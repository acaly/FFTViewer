using MathNet.Numerics;
using MathNet.Numerics.IntegralTransforms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FFTViewer
{
    //TODO make a chain of IDisposable
    unsafe class FFTPlayer
    {
        public FFTPlayer(IAudioReader reader)
        {
            _Reader = reader;
            _RawDataReader = FFTDataReaderFactory.CreateForFormat(reader.Provider.Format);

            int bufferLength = reader.BufferLength;
            Buffer = new float[bufferLength];
            Buffer2 = new float[bufferLength];
            _Buffer3 = new double[bufferLength];

            UIntPtr allocSize = (UIntPtr)(8 * (uint)bufferLength);
            _Input = FFTW.Malloc(allocSize);
            _Output = FFTW.Malloc(allocSize);
            _Plan = FFTW.PlanDft1D(bufferLength, _Input, _Output, -1, 0 /* FFTW_MEASURE */);
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
        public float[] Buffer2;
        private double[] _Buffer3;
        private float _LastTime;
        
        public void Calculate()
        {
            float time = _Reader.GetRawBuffer(out var buffer, out var offset);
            _RawDataReader.ReadToBuffer(buffer, offset, _WindowFunction, _Input);
            FFTW.Execute(_Plan);

            var changetime = time - _LastTime;
            _LastTime = time;

            for (int i = 0; i < _WindowFunction.Length; ++i)
            {
                Buffer[i] = (float)Math.Sqrt(_Output[i].Length());
                if (i < 2)
                {
                    Buffer2[i] = 0;
                    continue;
                }

                var x = (i) / (float)_WindowFunction.Length;
                var freq = x * _Reader.Provider.Format.SampleRate; //TODO NPE
                var period = 1 / freq;
                
                var th = Math.Atan2(_Output[i].Imaginary, _Output[i].Real);
                var tth = th / Math.PI / 2 * period;

                var changeth = th - _Buffer3[i];
                _Buffer3[i] = th;

                if (changetime == 0)
                {
                    Buffer2[i] = 0;
                    continue;
                }

                var expect_changeth = changetime / period * Math.PI * 2;
                var diff = (expect_changeth - changeth) % (Math.PI * 2);
                diff = diff / (Math.PI * 2);
                if (diff > 0.5) diff = 1 - diff;
                diff *= 15;
                if (diff > 1) diff = 1;
                //Buffer2[i] *= (float)(1 - diff);

                Buffer2[i] = (float)diff;
            }
        }
    }
}
