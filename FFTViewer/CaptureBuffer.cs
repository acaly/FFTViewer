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
            _PinnedHandle = GCHandle.Alloc(_Data, GCHandleType.Pinned);
            _Pointer = _PinnedHandle.AddrOfPinnedObject();
        }

        ~CaptureBuffer()
        {
            _PinnedHandle.Free();
        }

        private byte[] _Data;
        private GCHandle _PinnedHandle;
        private IntPtr _Pointer;

        public IntPtr Pointer => _Pointer;

        public void Write(byte[] buffer, int length)
        {
            Array.ConstrainedCopy(_Data, length, _Data, 0, _Data.Length - length);
            Array.Copy(buffer, 0, _Data, _Data.Length - length, length);
        }
    }
}
