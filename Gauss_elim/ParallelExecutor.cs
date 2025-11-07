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
        public long elapsedTime { get; set; }

        /* 
         * fukcja bedzie wywolywac rownolege eliminacje gaussa dla CPP lub ASM
         */
        public void RunParallel(int threads)
        {
           maxThreads = threads;


        }
        public long run_asm(string input1, int thread_count, string outp)
        {
            Stopwatch sw = Stopwatch.StartNew();
            asm_parallel matrixAsm = new asm_parallel(input1, thread_count, outp);
            matrixAsm.Gauss_parallel();
            sw.Stop();
            matrixAsm.Dispose();
            return sw.ElapsedMilliseconds;
            //elapsedTime = sw.ElapsedMilliseconds;
            //Console.WriteLine($"Czas wykonania równoległej eliminacji Gaussa (ASM): {sw.ElapsedMilliseconds} ms");

        }
        public long run_cpp(string input1, int thread_count, string outp) {
            Stopwatch sw = Stopwatch.StartNew();
            Matrix_Cpp_Parallel matrixCpp = new Matrix_Cpp_Parallel(input1, thread_count, outp);
            matrixCpp.Gauss_parallel();
            sw.Stop();
            matrixCpp.Dispose();
            // elapsedTime = sw.ElapsedMilliseconds;
            return sw.ElapsedMilliseconds;
           //Console.WriteLine($"Czas wykonania równoległej eliminacji Gaussa (CPP): {sw.ElapsedMilliseconds} ms");
        }
    }

    public class asm_parallel
    {
        MatrixHandler.MatrixHandler matrix;
        int threadCount;
        string file_outp;
        public asm_parallel(string path, int thread_count, string outp)
        {
          
            matrix = new MatrixHandler.MatrixHandler(path);
            this.threadCount = thread_count;
            this.file_outp = outp;
        }
        public void Gauss_parallel()
        {
            matrix.checkSize();

            for (int y = 0; y < matrix.cols - 1; y++)
            {
               
                float pivot = matrix.data[y * matrix.cols + (y)];

                if (Math.Abs(pivot) > 1.0e-6f) // Sprawdź, czy pivot NIE JEST zerem w wyniku checkSize
                {
                    
                    matrix.ApplyPivot(y);


                    Parallel.For(y, matrix.rows - 1, new ParallelOptions { MaxDegreeOfParallelism = threadCount }, row_elim =>
                    {
                      
                        matrix.gauss_step(row_elim, y);

                    });
                }

            }

            

        }

        public void Dispose()
        {
            matrix.SaveMatrixToFile(file_outp);
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
        float eps_abs;
        float eps_rel;
        string file_outp;
        public Matrix_Cpp_Parallel(string input, int thread_count, string outp) {
            matrixPtr = NativeMethods.GaussCpp.create_matrix(input);
            rows = NativeMethods.GaussCpp.get_rows(matrixPtr);
            cols = NativeMethods.GaussCpp.get_cols(matrixPtr);
            eps_abs = NativeMethods.GaussCpp.get_eps_abs(matrixPtr);
            eps_rel = NativeMethods.GaussCpp.get_eps_rel(matrixPtr);
            file_outp = outp;

            this.threadCount = thread_count;
        }
        public void Gauss_parallel()
        {
          
            for (int y = 0; y < cols - 1; y++) {
                // pivot dla aktualnej kolumny
                NativeMethods.GaussCpp.apply_pivot(matrixPtr, y);

                
                Parallel.For(y , rows - 1, new ParallelOptions { MaxDegreeOfParallelism = threadCount }, row_elim =>
                {
                    NativeMethods.GaussCpp.gauss_step(matrixPtr,row_elim,y);
                  
                });

                // ptr->ZeroUntilEps(y, y);
                NativeMethods.GaussCpp.zero_until_eps(matrixPtr, y, y);
            }
          

        }


        public void Dispose()
        {
            NativeMethods.GaussCpp.save_matrix(matrixPtr, file_outp); //z tym czy bez tego i tak printuje
            if (matrixPtr != IntPtr.Zero)
            {
                NativeMethods.GaussCpp.destroy_matrix(matrixPtr);
                matrixPtr = IntPtr.Zero;
            }
        }
    }


}
