using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FFTViewer
{
    static class NoteConstants
    {
        public const float Inc = 1.0594630943593f; //2 ^ (1 / 12)
        public const float Inc2 = Inc * Inc;

        public const float C4Freq = 261.6255653f;
        public const float C4Position = C4Freq / 44100;
    }
}
