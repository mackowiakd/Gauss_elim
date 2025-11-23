using Gauss_elim;
using Gauss_elim.MatrixHandler_ASM;
using Gauss_elim.testing;
using Gauss_elim.threading;
using GUI;
using Microsoft.SqlServer.Server;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Emit;
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
       
            //string mode = "release"; //"debug"; //
            float min = -12.25f;
            float max =128.00f;
            int size = 6;
            // tests t = new tests(min ,max);
            // t.run_tests(mode);
            MatrixGenerator generator = new MatrixGenerator(min, max);
            string baseDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"macierz_rozszerz");
            Directory.CreateDirectory(baseDir); // upewnia się, że katalog istnieje
            string file_inpt = Path.Combine(baseDir, $"matrix{size}x{size}.txt");
            generator.GenerateMatrix(size, file_inpt);

            ////testy 1 watkowo 
            MatrixHandler asm_1thread_mtrx = new MatrixHandler(file_inpt);
            asm_1thread_mtrx.checkSize();
            asm_1thread_mtrx.GaussEliminationManaged();
            asm_1thread_mtrx.SaveMatrixToFile(Path.Combine(baseDir, $"result_asm_1T_{size}x{size}_1thread.txt"));
            asm_1thread_mtrx.SaveSlnMtrx(Path.Combine(baseDir, $"solution_asm1T{size}x{size}_1thread.txt"));

            //testy 1 watkowa cpp z bckw substitution
            NativeMethods.import_func.start_gauss(file_inpt, Path.Combine(baseDir, $"CPP_{size}x{size}_SingT.txt"), Path.Combine(baseDir, $"sln_cpp{size}x{size}_SingT.txt"));

            //wielotkowa wersja asm
            ParallelExecutor P_exe = new ParallelExecutor();
            P_exe.run_asm(file_inpt, 2, Path.Combine(baseDir, $"result_asm_{size}x{size}_4threads.txt"), Path.Combine(baseDir, $"sln_asm_{size}x{size}_4threads.txt"));





        }
    }
}

//pod GUI
/*
 * ParallelExecutor P_exe = new ParallelExecutor();
Form1 form = new Form1();
string inputPath = form.GetInputFilePath();
Application.Run(form); // ← WinForms GUI


if (form.IsUsingAsm())
        P_exe.run_asm(inputPath, form.ThreadCount);
else { 
        P_exe.run_asm(inputPath, form.ThreadCount);
}
//zeby to zosatlo wypritowane Forms musi byc jako punkt startowy
form.SetExecutionTime(P_exe.elapsedTime); 
 * 
*/

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