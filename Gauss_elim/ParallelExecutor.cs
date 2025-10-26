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
        /* 
         * fukcja bedzie wywolywac rownolege eliminacje gaussa dla CPP lub ASM
         */
        public static void RunParallel(string input1, string input2)
        {
            Stopwatch sw = Stopwatch.StartNew();
            asm_parallel matrixAsm = new asm_parallel(input1);
            Console.WriteLine($"Liczba wątków: {Environment.ProcessorCount}");
            matrixAsm.Gauss_parallel();
            sw.Stop();
            Console.WriteLine($"Czas wykonania równoległej eliminacji Gaussa (ASM): {sw.ElapsedMilliseconds} ms");

            sw.Restart();
            Matrix_Cpp_Parallel matrixCpp = new Matrix_Cpp_Parallel(input2);
            matrixCpp.Gauss_parallel();
            matrixCpp.Dispose();
            sw.Stop();
            Console.WriteLine($"Czas wykonania równoległej eliminacji Gaussa (CPP): {sw.ElapsedMilliseconds} ms");

        }
    }

    public class asm_parallel
    {
        MatrixHandler.MatrixHandler matrix;
        int threadCount;
        public asm_parallel(string path)
        {
          
            matrix = new MatrixHandler.MatrixHandler(path);
            threadCount = matrix.rows-1;
        }
        public void Gauss_parallel()
        {
            matrix.checkSize();

            //PARALELL start for n= y
            for (int y = 0; y < matrix.cols - 1; y++)
            {
                // pivot dla aktualnej kolumny
                matrix.ApplyPivot(y);

                //rwnoległe przetwarzanie kolejnych wierszy
                Parallel.For(y, matrix.rows - 1, new ParallelOptions { MaxDegreeOfParallelism = threadCount }, row_elim =>
                {


                    if (matrix.data[y * matrix.cols + (y)] != 0)
                    { //pivot
                        matrix.gauss_step(row_elim, y);
                        matrix.ZeroUntilEps_parallel(row_elim, matrix.data[y * matrix.cols + (y)]);
                    } // zerujemy tylko ten wiersz kotry byl eliminowany (jesli sa wartosc NaN itd)


                    //Console.WriteLine($"Wątek {Task.CurrentId} przetworzył wiersz {row_elim} dla kolumny {y}");
                   
                });
             
                //matrix.PrintMatrix();

            }

            matrix.SaveMatrixToFile("output_asm_parallel.txt");

        }


    }

    public class Matrix_Cpp_Parallel
    {
        //operje na wskazniku do macierzy w cpp
        //wywoluje eliminacje gaussa wielowatkowo
        public IntPtr matrixPtr;
        public int rows;
        public int cols;
        int threadCount;
        public Matrix_Cpp_Parallel(string input) {
            matrixPtr = NativeMethods.GaussCpp.create_matrix(input);
            rows = NativeMethods.GaussCpp.get_rows(matrixPtr);
            cols = NativeMethods.GaussCpp.get_cols(matrixPtr);
            threadCount = rows-1;
        }
        public void Gauss_parallel()
        {
            //for (int y = 0; y < ptr->cols - 1; y++) {

            

            //PARALELL start for n= y
            for (int y = 0; y < cols - 1; y++) {
                // pivot dla aktualnej kolumny
                NativeMethods.GaussCpp.apply_pivot(matrixPtr, y);

                //rwnoległe przetwarzanie kolejnych wierszy
                Parallel.For(y , rows - 1, new ParallelOptions { MaxDegreeOfParallelism = threadCount }, row_elim =>
                {
                    NativeMethods.GaussCpp.gauss_step(matrixPtr,row_elim,y);
                    //Console.WriteLine($"Wątek {Task.CurrentId} przetworzył wiersz {row_elim} dla kolumny {y}");
                });

                // ptr->ZeroUntilEps(y, y);
                NativeMethods.GaussCpp.zero_until_eps(matrixPtr, y, y);
            }
          

        }


        public void Dispose()
        {
            NativeMethods.GaussCpp.save_matrix(matrixPtr, "output_cpp_parallel.txt");
            if (matrixPtr != IntPtr.Zero)
            {
                NativeMethods.GaussCpp.destroy_matrix(matrixPtr);
                matrixPtr = IntPtr.Zero;
            }
        }
    }


}
