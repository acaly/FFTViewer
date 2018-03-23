using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FFTViewer
{
    class SpectrumCompressor
    {
        public static unsafe float[] CompressInt16(byte[] rawData, int ratio)
        {
            float[] ret = new float[((rawData.Length / 2 - 1) / ratio + 1) * 2];
            for (int i = 0; i < ret.Length / 2; ++i)
            {
                var max = float.MinValue;
                var min = float.MaxValue;
                for (int j = 0; j < ratio; ++j)
                {
                    var index = i * ratio + j;
                    if (index * 2 >= rawData.Length) break;
                    var point = BitConverter.ToInt16(rawData, index * 2) / 32768f;
                    max = Math.Max(max, point);
                    min = Math.Min(min, point);
                }
                ret[i * 2] = min;
                ret[i * 2 + 1] = max;
            }
            return ret;
        }

        public static float[] Compress(float[] data1, float[] data2, int ratio)
        {
            float[] ret = new float[((data1.Length - 1) / ratio + 1) * 2];
            for (int i = 0; i < ret.Length / 2; ++i)
            {
                var max = float.MinValue;
                var min = float.MaxValue;
                for (int j = 0; j < ratio; ++j)
                {
                    var index = i * ratio + j;
                    if (index >= data1.Length) break;
                    max = Math.Max(max, data1[index]);
                    min = Math.Min(min, data1[index]);
                    max = Math.Max(max, data2[index]);
                    min = Math.Min(min, data2[index]);
                }
                ret[i * 2] = min;
                ret[i * 2 + 1] = max;
            }
            return ret;
        }
    }
}
