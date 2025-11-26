using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Gauss_elim.NativeMethods;

namespace Gauss_elim.MatrixHandler_ASM
{

    public class MatrixHandler
    {
        // To są pola klasy (globalne dla wszystkich metod tej klasy)
        public int rows { get; private set; }
        public int cols { get; private set; }
        public float[] data { get; private set; }
        public float[] slnVector { get; private set; }

        public int oldCols;

        int ymm = 8; // liczba wierszy przetwarzanych jednocześnie przez YMM
      
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
            Console.WriteLine($"Wczytano macierz o rozmiarze {rows}x{cols} z pliku: {path}");

            return (values.ToArray(), rows, cols);
        }
        


        public unsafe void SaveMatrixToFile(string output)
        {
            using (StreamWriter writer = new StreamWriter(output, false, Encoding.UTF8))
            {
                for (int r = 0; r < rows; r++)
                {
                    string line = "";
                    for (int c = 0; c < oldCols; c++)
                    {
                        // dopisujemy element z kropką jako separatorem dziesiętnym
                        line += data[r * cols + c].ToString();
                        if (c < oldCols - 1)
                            line += " "; // separator między kolumnami
                    }
                    writer.WriteLine(line);
                }
            }
            Console.WriteLine("Plik zapisano w: " + Path.GetFullPath(output));

        }
        public void SaveSlnMtrx(string path)
        {
            using (StreamWriter writer = new StreamWriter(path, false, Encoding.UTF8))
            {
               

                string line = "";
                for (int r = 0; r < rows; r++)
                {
                    // dopisujemy element z kropką jako separatorem dziesiętnym
                    line += slnVector[r].ToString();
                    line += " "; // separator między kolumnami

                }
                writer.WriteLine(line);
                
            }
            Console.WriteLine("Plik zapisano w: " + Path.GetFullPath(path));

        }



        public void checkSize()
        {
            oldCols = cols; // zapamiętujemy oryginalną liczbę kolumn
            //rommiar < 8 ??

            if (cols != rows +1)
                throw new InvalidOperationException("Macierz nie jest kwadratowa.");

            if (cols % ymm == 0)
                return; // rozmiar jest już wielokrotnością 16


            else if (cols > ymm && cols % ymm != 0)
            {
                cols = (int)Math.Ceiling((double)cols / ymm) * ymm;
                
            }

            float[] newData = new float[cols * rows];

            // 4. Skopiuj dane do LEWEGO GÓRNEGO rogu (bez offsetów!)
            unsafe
            {
                fixed (float* src = data)
                fixed (float* dst = newData)
                {
                    for (int r = 0; r < rows; r++)
                    {
                        // Kopiujemy wiersz po wierszu
                        // Źródło: stary wiersz r
                        float* srcRow = src + (r * oldCols);
                        // Cel: nowy wiersz r (ale nowa szerokość newCols)
                        float* dstRow = dst + (r * cols);

                        // Kopiujemy tylko tyle bajtów, ile miał stary wiersz
                        Buffer.MemoryCopy(srcRow, dstRow, cols * sizeof(float), oldCols * sizeof(float));
                    }
                }
            }
            data = newData;
            
      
        }


        public void ApplyPivot(int currentCol)
        {
            // UWAGA: Ponieważ mamy macierz rozszerzoną, pivot na przekątnej to [y, y]
            // currentCol to nasze 'y'.

            int pivotRow = currentCol;

            // Odczytujemy wartość w aktualnym wierszu
            // (Bez żadnych rowOffsetów!)
            float maxAbs = Math.Abs(data[currentCol * cols + currentCol]);

            // Szukamy wiersza z największym elementem
            // Iterujemy tylko do realRows (nie wchodzimy na puste zera na dole, jeśli są)
            for (int i = currentCol + 1; i < rows; i++)
            {
                float val = Math.Abs(data[i * cols + currentCol]);
                if (val > maxAbs)
                {
                    maxAbs = val;
                    pivotRow = i;
                }
            }

            // Sprawdzenie czy pivot nie jest zerem (osobliwość)
            if (maxAbs < 1e-9f) return;

            // Zamiana wierszy
            if (pivotRow != currentCol)
            {
                // Zamieniamy CAŁY wiersz (aż do newCols, łącznie z paddingiem z prawej)
                // To jest bezpieczne i szybkie (AVX lubi pełne wiersze)
                for (int j = 0; j < oldCols; j++)
                {
                    float tmp = data[currentCol * cols + j];
                    data[currentCol * cols + j] = data[pivotRow * cols + j];
                    data[pivotRow * cols + j] = tmp;
                }
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

                    fixed (float* rowN = &data[y * cols + x]) // const for all col elim
                    fixed (float* rowNext = &data[(n + 1) * cols + x])


                    {
                        
                        //if (data[y * cols + (y)] > EPS_ABS{ spr w petli glownej
                            NativeMethods.import_func.gauss_elimination(rowN, rowNext, factor, Math.Abs(pivot));
                            

                        //}

                    }
                }
                
            }
        }
        
        /* wykonywanie eliminacji gaussa tylko dla 1 watku - metoda do tesu algorytmu */
        public void GaussEliminationManaged()
        {
            for (int y = 0; y < rows-1; y++) //interesuje mnie tylko przekątna główna, która jest wyznaczona przez min(rows, cols).
            {
                //zawsze pivoting
               
                ApplyPivot(y);
               
                float pivot = data[y * cols + (y)];
   
             

                for (int n = y; n < rows-1 ; n++) // kazde n wiersz dla innega watku
                {
                    float elim = data[(n + 1) * cols + (y)]; // element do eliminacji z rowNext
                    float factor = elim / pivot; // 3. Oblicz współczynnik JEDEN RAZ

                    //dzielenie wiersza na czesci po 8 float 
                    for (int x = 0; x < cols; x += ymm)
                    {

                        //Console.WriteLine($"[{y},{n}] elim={elim}, pivot={pivot}");

                       
                        unsafe
                        {
                           

                            fixed (float* rowN = &data[y * cols + x]) // const for all col elim
                            fixed (float* rowNext = &data[(n + 1) * cols + x])


                            {
                              
                                //if pivot ==0 => all row can be skiped
                                if (Math.Abs(pivot)>EPS_ABS)
                                {
                                    NativeMethods.import_func.gauss_elimination(rowN, rowNext, factor, Math.Abs(pivot));
                                   
                                }

                            }
                        }
                    }

                    
                }
              

            }
            BackSubstitution();

        }
        public void BackSubstitution()
        {
            // slnVector to tablica na wyniki 'x' (rozmiar N)
            slnVector = new float[rows];

            unsafe
            {
                fixed (float* mtrxPtr = data)
                fixed (float* slnPtr = slnVector)
                {
                    // Pętla od ostatniego wiersza w górę
                    for (int i = rows -1; i >= 0; i--)
                    {
                        // 1. Przygotuj dane dla ASM
                        // Chcemy sumować elementy od kolumny (i + 1) do końca
                        int startCol = i + 1;
                        int count = rows - (startCol ); // Ile liczb przemnożyć

                        float sum = 0.0f;

                        if (count > 0)
                        {
                            // Wskaźnik na start danych w wierszu macierzy
                            float* rowSegment = mtrxPtr + (i * cols) + startCol;

                            // Wskaźnik na start danych w wektorze wyników
                            float* slnSegment = slnPtr + startCol;

                            // WOŁAMY ASM: "Policz iloczyn skalarny tych fragmentów"
                            sum = NativeMethods.import_func.calculate_dot_product(rowSegment, slnSegment, count);

                            //float sumCs = 0;
                            //for (int k = 0; k < count; k++)
                            //{
                            //    sumCs += rowSegment[k] * slnSegment[k];
                            //}
                            //sum = sumCs;
                        }
                      


                        // 2. Dokończ obliczenia w C# (to jest szybkie, bo tylko raz na wiersz)
                        float b_i = mtrxPtr[i * cols + rows]; // Wyraz wolny (ostatnia kolumna)
                        float pivot = mtrxPtr[i * cols + i];        // Pivot

                        

                        if (Math.Abs(pivot) > 1e-9f)
                            slnVector[i] = (b_i - sum) / pivot;
                        else
                            slnVector[i] = 0; // Zabezpieczenie
                      
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


/*   //zerowanie tylko 1 wiersza przez watek ktory byl za nieg odpowiedzialny
        public void ZeroUntilEps_parallel(int elim_startIdx, float pivot)
        {
            int i = elim_startIdx;
            // przechodzimy po wierszu od pivota do konca tego wiersza
            for (; i < cols;  i++)
            {
                if (Math.Abs(data[i]) < EPS_ABS + EPS_REL * Math.Abs(pivot))
                    data[i] = 0f;

            }
        }
        public void ZeroUntilEps(int startIndex, float pivot) //zerowanie wiersza po kazdej eliminacji i to w czesciach (wdg petli x)
        {
            // przechodzimy po wierszu od startIndex do startIndex + rejestr YMM (8 float)
            for (int i = startIndex; i <cols; i++)
            {
                if (Math.Abs(data[i]) < EPS_ABS + EPS_REL * Math.Abs(pivot))
                    data[i] = 0f;

            }
        }

*/