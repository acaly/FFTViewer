using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace FFTViewer
{
    class CaptureBuffer<T>
    {
        private static readonly int SizeOfT = Marshal.SizeOf<T>();

        public CaptureBuffer(int length)
        {
            _Data = new T[length];
            _PinnedHandle = GCHandle.Alloc(_Data, GCHandleType.Pinned);
            _Pointer = _PinnedHandle.AddrOfPinnedObject();
        }

        ~CaptureBuffer()
        {
            _PinnedHandle.Free();
        }

        private T[] _Data;
        private GCHandle _PinnedHandle;
        private IntPtr _Pointer;

        public IntPtr Pointer => _Pointer;

        public void Write(byte[] buffer, int offset, int length)
        {
            int start = 0, copyLen = _Data.Length;
            if (length / SizeOfT < copyLen)
            {
                start = copyLen - length / SizeOfT;
                copyLen = length / SizeOfT;
                Array.ConstrainedCopy(_Data, length / SizeOfT, _Data, 0, start);
            }
            Buffer.BlockCopy(buffer, offset + length - copyLen * SizeOfT, _Data, start * SizeOfT, copyLen * SizeOfT);
        }
    }
}
