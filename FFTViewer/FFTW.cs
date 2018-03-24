using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace FFTViewer
{
    unsafe static class FFTW
    {
        public struct Complex
        {
            public float Real, Imaginary;

            public float Length()
            {
                //We're going to take log later. No need to do sqrt.
                return Real * Real + Imaginary * Imaginary;
            }
        }

        [DllImport("libfftw3f.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "fftwf_malloc")]
        public static extern Complex* Malloc(UIntPtr size);

        [DllImport("libfftw3f.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "fftwf_free")]
        public static extern void Free(Complex* ptr);

        [DllImport("libfftw3f.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "fftwf_plan_dft_1d")]
        public static extern IntPtr PlanDft1D(int n0, Complex* pIn, Complex* pOut, int direction, uint flags);

        [DllImport("libfftw3f.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "fftwf_destroy_plan")]
        public static extern void DestroyPlan(IntPtr plan);

        [DllImport("libfftw3f.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "fftwf_execute")]
        public static extern void Execute(IntPtr plan);
    }
}
