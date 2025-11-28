using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;


namespace Gauss_elim.NativeMethods{


    public static class import_func
    {
        // --- KONFIGURACJA ŚCIEŻEK ---
        // Definiujemy stałe wewnątrz klasy. Dzięki temu są widoczne dla DllImport.

#if DEBUG
        // Ścieżki dla trybu DEBUG
        private const string CppPath = @"C:\Users\Dominika\source\repos\JA_proj\Gauss_elim\x64\Debug\Gauss_c++.dll";
        private const string AsmPath = @"C:\Users\Dominika\source\repos\JA_proj\Gauss_elim\x64\Debug\Gauss_asm.dll";
#else
        // Ścieżki dla trybu RELEASE
        private const string CppPath = @"C:\Users\Dominika\source\repos\JA_proj\Gauss_elim\x64\Release\Gauss_c++.dll";
        private const string AsmPath = @"C:\Users\Dominika\source\repos\JA_proj\Gauss_elim\x64\Release\Gauss_asm.dll";
#endif

        // --- IMPORTY Z ASEMBLERA (używamy stałej AsmPath) ---
        [DllImport(AsmPath, CallingConvention = CallingConvention.Cdecl)]
        public static unsafe extern float gauss_elimination(float* rowN, float* rowNext, float pivElim, float abs_pivot);

        [DllImport(AsmPath, CallingConvention = CallingConvention.Cdecl)]
        public static unsafe extern float calculate_dot_product(float* rowPtr, float* xPtr, int count);

        // --- IMPORTY Z C++ (używamy stałej CppPath) ---




        [DllImport(CppPath, CallingConvention = CallingConvention.Cdecl)]
        public static extern void start_gauss(string input_path, string output_path, string outp_slnVec);

        [DllImport(CppPath, CallingConvention = CallingConvention.Cdecl)]
        public static extern void back_substitution(IntPtr matrixPtr);

        [DllImport(CppPath, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr create_matrix(string inputPath);

        [DllImport(CppPath, CallingConvention = CallingConvention.Cdecl)]
        public static extern void destroy_matrix(IntPtr matrixPtr);

        [DllImport(CppPath, CallingConvention = CallingConvention.Cdecl)]
        public static extern void gauss_step(IntPtr matrixPtr, int nRow, int nCol);

        [DllImport(CppPath, CallingConvention = CallingConvention.Cdecl)]
        public static extern void save_matrix(IntPtr matrixPtr, string outputPath);

        [DllImport(CppPath, CallingConvention = CallingConvention.Cdecl)]
        public static extern void save_result(IntPtr matrixPtr, string outputPath);


        [DllImport(CppPath, CallingConvention = CallingConvention.Cdecl)]
        public static extern void apply_pivot(IntPtr matrixPtr, int currentRow);

        [DllImport(CppPath, CallingConvention = CallingConvention.Cdecl)]
        public static extern int get_rows(IntPtr matrixPtr);

        [DllImport(CppPath, CallingConvention = CallingConvention.Cdecl)]
        public static extern int get_cols(IntPtr matrixPtr);

        [DllImport(CppPath, CallingConvention = CallingConvention.Cdecl)]
        public static extern void zero_until_eps(IntPtr matrixPtr, int startRow, int startCol);

        [DllImport(CppPath, CallingConvention = CallingConvention.Cdecl)]
        public static extern float get_eps_abs(IntPtr matrixPtr);

        [DllImport(CppPath, CallingConvention = CallingConvention.Cdecl)]
        public static extern float get_eps_rel(IntPtr matrixPtr);
    }
}

