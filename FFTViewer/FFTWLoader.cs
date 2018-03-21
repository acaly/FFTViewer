using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace FFTViewer
{
    class FFTWLoader
    {
        [DllImport("libfftw3f.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr fftwf_malloc(UIntPtr size);

        public static void Test()
        {
            var x = fftwf_malloc(new UIntPtr(128));
        }
    }
}
