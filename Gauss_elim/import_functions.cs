using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Gauss_elim.NativeMethods{
    public static class GaussAsm
    {
        [DllImport(@"C:\Users\Dominika\source\repos\Gauss_elim\x64\Debug\Gauss_asm.dll", CallingConvention = CallingConvention.Cdecl)]
        public static unsafe extern float gauss_elimination(float* rowN, float* rowNext, float pivElim);


    }

    public static class GaussCpp

    {
        //public string DllPath = @"C:\Users\Dominika\source\repos\Gauss_elim\x64\Debug\Gauss_c++.dll";

        [DllImport(@"C:\Users\Dominika\source\repos\Gauss_elim\x64\Debug\Gauss_c++.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void start_gauss(string input_path, string output_path);

        [DllImport(@"C:\Users\Dominika\source\repos\Gauss_elim\x64\Debug\Gauss_c++.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr create_matrix(string inputPath);

        [DllImport(@"C:\Users\Dominika\source\repos\Gauss_elim\x64\Debug\Gauss_c++.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void destroy_matrix(IntPtr matrixPtr);

        [DllImport(@"C:\Users\Dominika\source\repos\Gauss_elim\x64\Debug\Gauss_c++.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void gauss_step(IntPtr matrixPtr, int nRow, int nCol);

        [DllImport(@"C:\Users\Dominika\source\repos\Gauss_elim\x64\Debug\Gauss_c++.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void save_matrix(IntPtr matrixPtr, string outputPath);


        [DllImport(@"C:\Users\Dominika\source\repos\Gauss_elim\x64\Debug\Gauss_c++.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void apply_pivot(IntPtr matrixPtr, int currentRow);

        [DllImport(@"C:\Users\Dominika\source\repos\Gauss_elim\x64\Debug\Gauss_c++.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int get_rows(IntPtr matrixPtr);

        [DllImport(@"C:\Users\Dominika\source\repos\Gauss_elim\x64\Debug\Gauss_c++.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int get_cols(IntPtr matrixPtr);

        [DllImport(@"C:\Users\Dominika\source\repos\Gauss_elim\x64\Debug\Gauss_c++.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void zero_until_eps(IntPtr matrixPtr, int startRow, int startCol);

        [DllImport(@"C:\Users\Dominika\source\repos\Gauss_elim\x64\Debug\Gauss_c++.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern float get_eps_abs(IntPtr matrixPtr);

        [DllImport(@"C:\Users\Dominika\source\repos\Gauss_elim\x64\Debug\Gauss_c++.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern float get_eps_rel(IntPtr matrixPtr);
    }
}

