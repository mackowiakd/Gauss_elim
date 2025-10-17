using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Gauss_elim.NativeMethods
{
    public static class NativeMethods
    {
        [DllImport(@"C:\Users\Dominika\source\repos\Gauss_elim\x64\Debug\Gauss_asm.dll", CallingConvention = CallingConvention.Cdecl)]
        public static unsafe extern float gauss_elimination(float* rowN, float* rowNext, float* pivElim, int* offsIdxSize);

        [DllImport(@"C:\Users\Dominika\source\repos\Gauss_elim\x64\Debug\Gauss_c++.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void start_gauss(string input_path, string output_path);


        //jak zaimportowac ta funkcje cpp z tej drugiej biblioteki dll
    }
}

