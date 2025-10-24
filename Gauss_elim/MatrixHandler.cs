using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gauss_elim.NativeMethods;

namespace Gauss_elim.MatrixHandler
{

    public class MatrixHandler
    {
        // To są pola klasy (globalne dla wszystkich metod tej klasy)
        public int rows { get; private set; }
        public int cols { get; private set; }
        public float[] data { get; private set; }

        int ymm = 8; // liczba wierszy przetwarzanych jednocześnie przez YMM

        private int rowOffset = 0; // zmienna offset powinna wskazywac ile ymm mozna sie przesunac aby dostac aktulane dane (w przypadku resizingu)
        private int colOffset = 0;
        private const float eps = 1.0e-5f;

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
        public void ZeroUntilEps(int startIndex)
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

        public void ZeroUntilEps_parallel(int elim_row)
        {
           int i = elim_row * cols + elim_row;
            // przechodzimy po wierszu od pivota do konca -> ewentualnie zerowanie tylko 1 wiersza przez watek ktory byl za nieg odpowiedzialny
            for ( ; i < cols; i++)
            {
                // jeśli wartość bezwzględna < eps → zerujemy
                if (Math.Abs(data[i]) < eps)
                    data[i] = 0f;
                
            }
        }


        public void SaveMatrixToFile(string path)
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



        public void checkSize()
        {
            int newCols = ymm;
            int newRows = ymm;
            //rommiar < 8 ??

            if (cols != rows)
                throw new InvalidOperationException("Macierz nie jest kwadratowa.");

            if (cols % ymm == 0)
                return; // rozmiar jest już wielokrotnością 16


            else if (cols > ymm && cols % ymm != 0)
            {
                newCols = (int)Math.Ceiling((double)cols / ymm) * ymm;
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


        public void ApplyPivot(int currentRow)
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

            // zamień wiersze
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

     


        public void gauss_step(int n, int y)
        {

            //dzielenie wiersza na czesci po 8 float 
            for (int x = 0; x < cols; x += ymm)
            {

                unsafe
                {
                    float* value1 = stackalloc float[2];
                    value1[0] = data[y * cols + (y)]; //pivot
                    value1[1] = data[(n + 1) * cols + (y)];  // elim
                    

                    fixed (float* rowN = &data[y * rows + x]) // const for all col elim
                    fixed (float* rowNext = &data[(n + 1) * cols + x])


                    {
                        //zamiast 3 agr int -> array size 3 (r8 w asm)
                        //if pivot ==0 => all row can be skiped
                        if (data[y * cols + (y)] != 0)
                        {
                            NativeMethods.GaussAsm.gauss_elimination(rowN, rowNext, value1);

                        }

                    }
                }
            }
        }
        /*zmienne offset do przemyslenia bo zawsze dokladamy > niz 8 wierzy i kolumn 0
         * wtedy trzeba zaczac od rowOffset ale col =0 i zmienia sie caly algorytm...
         */
        public void GaussEliminationManaged()
        {
            for (int y = 0; y < cols - 1; y++)
            {
                //zawsze pivoting
                ApplyPivot(y);
                float pivot = data[y * cols + (y)];

                for (int n = y; n < rows - 1; n++) // kazde n wiersz dla innega watku
                {
                    float elim = data[(n + 1) * cols + (y)]; // element do eliminacji z rowNext

                    //dzielenie wiersza na czesci po 8 float 
                    for (int x = 0; x < cols; x += ymm)
                    {

                        Console.WriteLine($"[{y},{n}] elim={elim}, pivot={pivot}");


                        unsafe
                        {
                            float* value1 = stackalloc float[2];
                            value1[0] = pivot;
                            value1[1] = elim;  // to jest const dla danego n ALE moze ulec zmianie przy x=0
                           

                            fixed (float* rowN = &data[y * rows + x]) // const for all col elim
                            fixed (float* rowNext = &data[(n + 1) * cols + x])


                            {
                                //zamiast 3 agr int -> array size 3 (r8 w asm)
                                //if pivot ==0 => all row can be skiped
                                if (pivot != 0)
                                {
                                    NativeMethods.GaussAsm.gauss_elimination(rowN, rowNext, value1);
                                    ZeroUntilEps((n + 1) * cols + x);
                                }

                            }
                        }
                    }


                }
            }

        }

        public void PrintMatrix()
        {
            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    Console.Write(data[r * cols + c].ToString("F2") + " ");
                }
                Console.WriteLine();
            }
            Console.WriteLine();
        }
    }
}
