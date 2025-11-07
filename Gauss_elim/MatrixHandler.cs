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
        public const float eps = 1.0e-5f;
        public const float EPS_ABS = 1e-6f;
        public const float EPS_REL = 1e-4f;



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
        public void ZeroUntilEps(int startIndex, float pivot) //zerowanie wiersza po kazdej eliminacji i to w czesciach (wdg petli x)
        {
            // przechodzimy po wierszu od startIndex do startIndex + rejestr YMM (8 float)
            for (int i = startIndex; i < startIndex + ymm; i++)
            {
                if (Math.Abs(data[i]) < EPS_ABS + EPS_REL * Math.Abs(pivot))
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



        /* do wykonania rownloeglego*/

        //zerowanie tylko 1 wiersza przez watek ktory byl za nieg odpowiedzialny
        public void ZeroUntilEps_parallel(int elim_startIdx, float pivot)
        {
            int i = elim_startIdx;
            // przechodzimy po wierszu od pivota do konca tego wiersza
            for (int j = 0; j < ymm; j++, i++)
            {
                if (Math.Abs(data[i]) < EPS_ABS + EPS_REL * Math.Abs(pivot))
                    data[i] = 0f;

            }
        }
        /* do wykonania rownloeglego*/
        public void gauss_step(int n, int y)
        {
        
            float pivot = data[y * cols + (y)]; //pivot
            float elim = data[(n + 1) * cols + (y)];  // elim
            float factor = elim / pivot; // 3. Oblicz współczynnik JEDEN RAZ


            //dzielenie wiersza na czesci po 8 float 
            for (int x = 0; x < cols; x += ymm)
            {

                unsafe
                {

                    fixed (float* rowN = &data[y * rows + x]) // const for all col elim
                    fixed (float* rowNext = &data[(n + 1) * cols + x])


                    {
                        //zamiast 3 agr int -> array size 3 (r8 w asm)
                        //if pivot ==0 => all row can be skiped
                        if (data[y * cols + (y)] != 0)
                        {
                            NativeMethods.GaussAsm.gauss_elimination(rowN, rowNext, factor);
                            ZeroUntilEps((n + 1) * cols + x, data[y * cols + (y)]);



                        }

                    }
                }
                
            }
        }
        /*zmienne offset do przemyslenia bo zawsze dokladamy > niz 8 wierzy i kolumn 0
         * wtedy trzeba zaczac od rowOffset ale col =0 i zmienia sie caly algorytm...
         */
        /* wykonywanie eliminacji gaussa tylko dla 1 watku - metoda do tesu algorytmu */
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
                    float factor = elim / pivot; // 3. Oblicz współczynnik JEDEN RAZ

                    //dzielenie wiersza na czesci po 8 float 
                    for (int x = 0; x < cols; x += ymm)
                    {

                        Console.WriteLine($"[{y},{n}] elim={elim}, pivot={pivot}");


                        unsafe
                        {
                           

                            fixed (float* rowN = &data[y * rows + x]) // const for all col elim
                            fixed (float* rowNext = &data[(n + 1) * cols + x])


                            {
                                //zamiast 3 agr int -> array size 3 (r8 w asm)
                                //if pivot ==0 => all row can be skiped
                                if (pivot != 0)
                                {
                                    NativeMethods.GaussAsm.gauss_elimination(rowN, rowNext, factor);
                                    ZeroUntilEps((n + 1) * cols + x, pivot);
                                }

                            }
                        }
                    }

                    //czy lepiej ty raz zrobic zero eps dla mtrx  wiersza po zakonczeniu eliminacji
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
