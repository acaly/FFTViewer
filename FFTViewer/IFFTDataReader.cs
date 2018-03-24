using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FFTViewer
{
    interface IFFTDataReader
    {
        unsafe void ReadToBuffer(int n, void* src, float[] window, FFTW.Complex* dest);
    }

    class FFTDataReaderFloat : IFFTDataReader
    {
        public FFTDataReaderFloat(int numChannels, int channel)
        {
            ChannelCount = numChannels;
            Channel = channel;
        }

        private readonly int Channel;
        private readonly int ChannelCount;

        public unsafe void ReadToBuffer(int n, void* src, float[] window, FFTW.Complex* dest)
        {
            float* srcf = (float*)src + Channel;
            for (int i = 0, j = 0; i < n; i += 1, j += ChannelCount)
            {
                dest[i].Real = srcf[j] * window[i] / 100;
                dest[i].Imaginary = 0;
            }
        }
    }

    class FFTDataReaderInt16 : IFFTDataReader
    {
        public FFTDataReaderInt16(int numChannels, int channel)
        {
            ChannelCount = numChannels;
            Channel = channel;
        }

        private readonly int Channel;
        private readonly int ChannelCount;

        public unsafe void ReadToBuffer(int n, void* src, float[] window, FFTW.Complex* dest)
        {
            short* srcf = (short*)src + Channel;
            for (int i = 0, j = 0; i < n; i += 1, j += ChannelCount)
            {
                dest[i].Real = srcf[j] * window[i] / 32768f / 100; //TODO why this affects the result?
            }
        }
    }

    class FFTDataReaderFactory
    {
        public static IFFTDataReader CreateForFormat(WaveFormat format)
        {
            switch (format.Encoding)
            {
                case WaveFormatEncoding.IeeeFloat:
                    return new FFTDataReaderFloat(format.Channels, 0);
                case WaveFormatEncoding.Pcm:
                    return new FFTDataReaderInt16(format.Channels, 0);
            }
            throw new NotSupportedException("Not supported format: " + format.Encoding.ToString());
        }
    }
}
