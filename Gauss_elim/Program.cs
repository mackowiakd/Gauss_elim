using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Gauss_elim;
using System.Net;
using Gauss_elim.MatrixHandler;

namespace Gauss_elim
{
  

    internal class Program
    {

        static void Main(string[] args)
        {
            string inputPath = "matrix.txt";
            //string outputPath = "result_1.txt";
            //MatrixHandler.MatrixHandler matrixHandler = new MatrixHandler.MatrixHandler(inputPath);
            //matrixHandler.checkSize();
            //matrixHandler.PrintMatrix(matrixHandler.data, matrixHandler.rows, matrixHandler.cols);
            //matrixHandler.GaussEliminationManaged();
            //matrixHandler.SaveMatrixToFile(outputPath, matrixHandler.data, matrixHandler.rows, matrixHandler.cols);

           // NativeMethods.GaussCpp.start_gauss(inputPath, "result_2.txt");

            //threading.ParallelExecutor.RunParallel(inputPath);
            threading.asm_parallel asm_Parallel = new threading.asm_parallel(inputPath, Environment.ProcessorCount);
        }
    }
}


