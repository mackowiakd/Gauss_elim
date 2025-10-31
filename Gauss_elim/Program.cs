using Gauss_elim;
using Gauss_elim.MatrixHandler;
using Gauss_elim.threading;
using GUI;
using Microsoft.SqlServer.Server;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms; // ← To jest ważne!

namespace Gauss_elim
{
  

    internal class Program
    {
        [STAThread]
        static void Main()
        {
           Form1 form = new Form1();
            Application.Run(form); // ← WinForms GUI

            if(form.IsUsingAsm())
            {
                string inputPath = form.GetInputFilePath();
               
                ParallelExecutor P_exe = new ParallelExecutor();
                P_exe.run_asm(inputPath);
            }

            if (form.IsUsingCpp())
            {
                string inputPath = form.GetInputFilePath();
                
                ParallelExecutor P_exe = new ParallelExecutor();
                P_exe.run_cpp(inputPath);
            }
        }

    }
}

//static void Main(string[] args)
//{
//    string inputPath1 = "mmm.txt";
//    //string inputPath2 = "matrix2.txt";
//    MatrixGenerator gen1 = new MatrixGenerator();

//    gen1.fileName = inputPath1;


//    // threading.ParallelExecutor.RunParallel(inputPath1, inputPath1);

//}

//string outputPath = "result_1.txt";
//MatrixHandler.MatrixHandler matrixHandler = new MatrixHandler.MatrixHandler(inputPath1);
//matrixHandler.checkSize();
//matrixHandler.PrintMatrix();
//matrixHandler.GaussEliminationManaged();
//matrixHandler.SaveMatrixToFile("res_asm_oneT.txt");
//NativeMethods.GaussCpp.start_gauss(inputPath, "result_2.txt"); //-> metoda z dll cpp 1 watek

//inputPath1 =gen1.filePath;