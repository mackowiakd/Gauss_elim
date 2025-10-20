using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gauss_elim.NativeMethods;

namespace Gauss_elim.threading
{
    public class ParallelExecutor
    {
        int maxThreads = Environment.ProcessorCount;
        /* Uruchamia równoległe przetwarzanie eliminacji Gaussa -> musi isc w petli po ilsoci col => modyf funkcje (obciac petle y)
         * + pivoting tez przed watkami
         * input - ścieżka do pliku wejściowego
         * output - ścieżka do pliku wyjściowego
         * threadCount - liczba wątków do uruchomienia
         * mode - "cpp" lub "asm" określający, której implementacji użyć
         * 
         * musi dostac:
         *      rozmiar macierzy 
         *      odpowiedn, metode do pivotingu (asm ma inna niz cpp)
         * 
         */
        public static void RunParallel(string input, string output, int threadCount, string mode)
        {
            Stopwatch sw = Stopwatch.StartNew();

            Parallel.For(0, threadCount, i =>
            {
                if (mode == "cpp")
                    NativeMethods.GaussCpp.start_gauss(input, output + $"_cpp_{i}.txt");
                else if (mode == "asm")
                {
                    // przykładowe wywołanie asm
                    // NativeMethods.gauss_elimination(...);
                }
            });

            sw.Stop();
            Console.WriteLine($"Zakończono ({mode}) w {sw.ElapsedMilliseconds} ms na {threadCount} wątkach.");
        }
    }


    public class Matrix_Cpp_warpper
    {
        //operje na wskazniku do macierzy w cpp
        //wywoluje eliminacje gaussa wielowatkowo
        public IntPtr matrixPtr;
        public int rows;
        public int cols;
        public Matrix_Cpp_warpper(string input) {
            matrixPtr = NativeMethods.GaussCpp.create_matrix(input);
            rows = NativeMethods.GaussCpp.get_rows(matrixPtr);
            cols = NativeMethods.GaussCpp.get_cols(matrixPtr);
        }
        public void Gauss_parallel(int n)
        {
            //for (int y = 0; y < ptr->cols - 1; y++) {

            //ptr->ApplyPivot(y);
            NativeMethods.GaussCpp.apply_pivot(matrixPtr, n);
           
            //PARALELL start for n= y+1 to rows- 
            NativeMethods.GaussCpp.gauss_step(matrixPtr, n,cols); // in Loop from n= y+1 to rows-1 => in threads scheduler
           
            // ptr->ZeroUntilEps(y, y);
            NativeMethods.GaussCpp.zero_until_eps(matrixPtr, n, n);
        }
    }

}
