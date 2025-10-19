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
        /* Uruchamia równoległe przetwarzanie eliminacji Gaussa -> musi isc w petli po ilsoci col => modyf funkcje (obciac petle y)
         * + pivoting tez przed watkami
         * input - ścieżka do pliku wejściowego
         * output - ścieżka do pliku wyjściowego
         * threadCount - liczba wątków do uruchomienia
         * mode - "cpp" lub "asm" określający, której implementacji użyć
         */
        public static void RunParallel(string input, string output, int threadCount, string mode)
        {
            Stopwatch sw = Stopwatch.StartNew();

            Parallel.For(0, threadCount, i =>
            {
                if (mode == "cpp")
                    NativeMethods.NativeMethods.start_gauss(input, output + $"_cpp_{i}.txt");
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
}
