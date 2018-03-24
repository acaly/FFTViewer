using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace FFTViewer
{
    class CaptureBuffer
    {
        //TODO make the internal buffer larger to avoid memmove every time.
        public CaptureBuffer(WaveFormat format, int length)
        {
            int bufferLen = length * format.BitsPerSample / 8 * format.Channels;
            _Data = new byte[bufferLen];
        }

        private byte[] _Data;

        public void Write(byte[] buffer, int length)
        {
            Array.ConstrainedCopy(_Data, length, _Data, 0, _Data.Length - length);
            Array.Copy(buffer, 0, _Data, _Data.Length - length, length);
        }

        public void Read(out byte[] data, out int offset)
        {
            data = _Data;
            offset = 0;
        }
    }
}
