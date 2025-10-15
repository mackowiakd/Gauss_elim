using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Gauss_elim;
using System.Net;

namespace Gauss_elim
{
    public class NativeMethods
    {
        [DllImport(@"C:\Users\Dominika\source\repos\Gauss_elim\x64\Debug\Gauss_asm.dll", CallingConvention = CallingConvention.Cdecl)]
        public static unsafe extern float gauss_elimination(float* rowN, float* rowNext, float* pivElim, int* offsIdxSize);
    }

    internal class Program
    {

        static void Main(string[] args)
        {
            string inputPath = "matrix.txt";
            string outputPath = "result_1.txt";
            Matrix_operations.MatrixHandler matrixHandler = new Matrix_operations.MatrixHandler(inputPath);
            //matrixHandler.checkSize();
            matrixHandler.GaussEliminationManaged();
            matrixHandler.SaveMatrixToFile(outputPath, matrixHandler.data, matrixHandler.rows, matrixHandler.cols);
        }
    }
}



namespace Matrix_operations
{
    public class MatrixHandler
    {
        // To są pola klasy (globalne dla wszystkich metod tej klasy)
        public int rows { get; private set; }
        public int cols { get; private set; }
        public float[] data { get; private set; }

        int ymm = 8; // liczba wierszy przetwarzanych jednocześnie przez YMM
        
        private int rowOffset=0;
        private int colOffset=0;
        private const float EPS = 1.0e-5f;

        public MatrixHandler(string path)
        {
            (data, rows, cols) = LoadMatrixFromFile(path);
            
        }

        public (float[], int, int) LoadMatrixFromFile(string path)
        {
            string[] lines = File.ReadAllLines(path);


            List<float> values = new List<float>();

            foreach (string line in lines)
            {
                string[] numbers = line.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                if (numbers.Length == 0) continue; // pomiń puste linie

                if (rows == 0)
                    cols = numbers.Length;  // ustalamy kolumny na podstawie 1. wiersza

                foreach (string num in numbers)
                    values.Add(float.Parse(num, System.Globalization.CultureInfo.InvariantCulture));

                rows++;
            }

            return (values.ToArray(), rows, cols);
        }
        public void ZeroUntilEps(float[] data, int startIndex,  float eps = 1.0e-5f)
        {
            // przechodzimy po wierszu od startIndex do startIndex + rejestr YMM (8 float)
            for (int i = startIndex; i < startIndex + ymm; i++)
            {
                // jeśli wartość bezwzględna < eps → zerujemy
                if (Math.Abs(data[i]) < eps)
                    data[i] = 0f;
                else
                    break; // po pierwszej wartości > eps kończymy
            }
        }


        public void SaveMatrixToFile(string path, float[] data, int rows, int cols)
        {
            using (StreamWriter writer = new StreamWriter(path, false, Encoding.UTF8))
            {
                for (int r = 0; r < rows; r++)
                {
                    string line = "";
                    for (int c = 0; c < cols; c++)
                    {
                        // dopisujemy element z kropką jako separatorem dziesiętnym
                        line += data[r * cols + c].ToString(System.Globalization.CultureInfo.InvariantCulture);
                        if (c < cols - 1)
                            line += " "; // separator między kolumnami
                    }
                    writer.WriteLine(line);
                }
            }
            Console.WriteLine("Plik zapisano w: " + Path.GetFullPath(path));

        }


        /* Metoda spr rozmiar -> ewnetulane uzupelnienie 0 do size mod16 
            -> alokacja nowej tablicy o rom wiekszej 
         */

        public void checkSize()
        {
            int newCols = ymm;
            int newRows = ymm;

            if( cols!= rows)
                throw new InvalidOperationException("Macierz nie jest kwadratowa.");

            if (cols % ymm == 0)
                return; // rozmiar jest już wielokrotnością 16


            else if (cols>ymm && cols % ymm != 0)
            {
                newCols = (cols / ymm) + 1;
                newRows = newCols;
            } 

            float[] newData = new float[newCols * newCols];

            rowOffset = newRows - rows;
            colOffset = rowOffset;

            unsafe
            {
                fixed (float* src = data)
                fixed (float* dst = newData)
                {
                    for (int r = 0; r < rows; r++)
                    {
                        float* srcRow = src + r * cols;
                        float* dstRow = dst + (r + rowOffset) * newCols + colOffset;
                        Buffer.MemoryCopy(srcRow, dstRow, cols * sizeof(float), cols * sizeof(float));
                    }
                }
            }

            data = newData;
            cols = newCols;
            rows = newCols;

        }
        //input zbyt bliskie wartosci 0 -> AV albo NaN

        public  void ApplyPivot(float[] data, int rows, int cols, int currentRow)
        {
            int pivotRow = currentRow;
            float maxAbs = Math.Abs(data[currentRow * cols + currentRow]);

            // Znajdź wiersz o największej wartości bezwzględnej w danej kolumnie
            for (int i = currentRow + 1; i < rows; i++)
            {
                float val = Math.Abs(data[i * cols + currentRow]);
                if (val > maxAbs)
                {
                    maxAbs = val;
                    pivotRow = i;
                }
            }

            // Jeśli trzeba — zamień wiersze
            if (pivotRow != currentRow)
            {
                for (int j = 0; j < cols; j++)
                {
                    float tmp = data[currentRow * cols + j];
                    data[currentRow * cols + j] = data[pivotRow * cols + j];
                    data[pivotRow * cols + j] = tmp;
                }
            }
        }

        public void GaussEliminationManaged()
        {
            for (int y = 0; y < cols-1; y++)
            {
                //zawsze pivoting
                ApplyPivot(data, rows, cols, y);
               float pivot = data[y * cols + (y + colOffset)];

                for (int n = y; n < rows-1 ; n++)
                {
                    float elim = data[(n + 1) * cols + (y + colOffset)];

                    //dzielenie wiersza na czesci po 8 float 
                    for (int x = 0; x < cols; x += ymm)
                        {
                        /* na ty poziomie pivot moze zostac zmieniony <=> x==0
                         * -> potem juz tylko odjemowanie i tak nic by sie nie wyzerowalo
                         * zapis zminay pivotingu -> elim_gaussa musi zwrocic elimNew aby spr czy elim!=elimNew
                         */
                        Console.WriteLine($"[{y},{n}] elim={elim}, pivot={pivot}");
                        

                        unsafe
                        {
                            float* value1 = stackalloc float[2];
                            value1[0] = pivot;
                            value1[1] = elim;  // to jest const dla danego n ALE moze ulec zmianie przy x=0
                            int* value2 = stackalloc int[3];
                            value2[0] = rows / 8 - 1;
                            value2[1] = y * rows + x;
                            value2[2] = rows * cols;
                           
                            fixed (float* rowN = &data[y * rows + x]) // const for all col elim
                            fixed (float* rowNext = &data[(n + 1) * cols + x])
                            

                            {
                                //zamiast 3 agr int -> array size 3 (r8 w asm)
                                NativeMethods.gauss_elimination(rowN, rowNext, value1, value2);
                                ZeroUntilEps(data, (n + 1) * cols + x, EPS);
                                }
                                
                            }
                        }
                    

                }
            }




        }
    }
}