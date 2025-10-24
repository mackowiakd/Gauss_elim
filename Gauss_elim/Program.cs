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
            //matrixHandler.PrintMatrix();
            //matrixHandler.GaussEliminationManaged();
            //matrixHandler.SaveMatrixToFile(outputPath);

            //NativeMethods.GaussCpp.start_gauss(inputPath, "result_2.txt"); //-> metoda z dll cpp 1 watek

            threading.ParallelExecutor.RunParallel(inputPath);
            
        }
    }
}


