using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FFTViewer
{
    class CaptureBuffer
    {
        public CaptureBuffer(int length)
        {
            _Data = new float[length];
        }

        public float[] _Data { get; private set; }

        public void Write(byte[] buffer, int offset, int length)
        {
            int start = 0, copyLen = _Data.Length;
            if (length / 4 < copyLen)
            {
                start = copyLen - length / 4;
                copyLen = length / 4;
                Array.ConstrainedCopy(_Data, length / 4, _Data, 0, start);
            }
            Buffer.BlockCopy(buffer, offset + length - copyLen * 4, _Data, start * 4, copyLen * 4);
        }
    }
}
