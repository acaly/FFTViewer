using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FFTViewer
{
    class SpectrumCompressor
    {
        public static float[] Compress(Mp3Reader r, int ratio)
        {
            float[] ret = new float[((r.DataL.Length - 1) / ratio + 1) * 2];
            for (int i = 0; i < ret.Length / 2; ++i)
            {
                var max = float.MinValue;
                var min = float.MaxValue;
                for (int j = 0; j < ratio; ++j)
                {
                    var index = i * ratio + j;
                    if (index >= r.DataL.Length) break;
                    max = Math.Max(max, r.DataL[index]);
                    min = Math.Min(min, r.DataL[index]);
                    max = Math.Max(max, r.DataR[index]);
                    min = Math.Min(min, r.DataR[index]);
                }
                ret[i * 2] = min;
                ret[i * 2 + 1] = max;
            }
            return ret;
        }
    }
}
