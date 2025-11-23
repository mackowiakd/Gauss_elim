using Gauss_elim.threading;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Net.Mime.MediaTypeNames;

namespace Gauss_elim.testing
{
    public class MatrixGenerator{
        public int size { get;  set; }
        public float min { get;  set; }
        public float max { get;  set; }
      
        

        public MatrixGenerator( float min, float max)
        {
            this.min = min; 
            this.max = max;
      

          
        }

       
        public void GenerateMatrix(int size, string file)
        {
            Random rand = new Random();

            // KROK 1: Wylosuj "tajny" wynik X (na liczbach całkowitych)
            // Np. losowe liczby od -10 do 10
            int[] secretX = new int[size];
            for (int i = 0; i < size; i++)
            {
                secretX[i] = rand.Next(-10, 11);
            }
            // 1. Pobierz katalog, w którym ma być plik
            string directory = Path.GetDirectoryName(file);

            // 2. Pobierz samą nazwę pliku BEZ rozszerzenia (np. "matrix250x250")
            string fileNameNoExt = Path.GetFileNameWithoutExtension(file);

            // 3. Zbuduj nową ścieżkę z dopiskiem "_sol.txt"
            // Wynik: C:\...\matrix250x250_sol.txt
            string solutionPath = Path.Combine(directory, fileNameNoExt + "_sol.txt");

            // 4. Zapisz plik
            File.WriteAllText(solutionPath, string.Join(" ", secretX.Select(f => f.ToString(CultureInfo.InvariantCulture))));

           

            // KROK 2: Generuj macierz A i wyliczaj pasujące b
            using (StreamWriter writer = new StreamWriter(file))
            {
                for (int i = 0; i < size; i++) // Dla każdego wiersza
                {
                    string[] rowStrings = new string[size + 1]; // +1 na wyraz wolny b
                    float rowSumB = 0; // Tu będziemy sumować, żeby obliczyć b

                    for (int j = 0; j < size; j++) // Dla każdej kolumny wiersza
                    {
                        // Losujemy współczynnik A[i,j] (float)
                        float valueA = (min + (float)(rand.NextDouble() * (max - min)));

                        // Dodajemy do pliku
                        rowStrings[j] = valueA.ToString("0.00", CultureInfo.InvariantCulture);

                        // *** TU JEST KLUCZ ***
                        // Obliczamy fragment wyrazu wolnego: A * x
                        rowSumB += valueA * secretX[j];
                    }

                    // Na koniec wiersza dopisujemy obliczone b (wyraz wolny)
                    // To jest ta ostatnia kolumna (N+1)
                    rowStrings[size] = rowSumB.ToString("0.00", CultureInfo.InvariantCulture);

                    // Zapisz cały wiersz (A oraz b) do pliku
                    string line = string.Join(" ", rowStrings);
                    writer.WriteLine(line);
                }
            }
        }


    }



    public class tests { 
      
        float min;
        float max;
        ParallelExecutor P_exe = new ParallelExecutor();
        MatrixGenerator generator;
        public tests(float min, float max)
        {
            this.min = min;
            this.max = max;
            generator = new MatrixGenerator(min, max);
          

        }
        public void run_tests(string config) {
            string baseDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"test_data_{config}");
            string resultDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "test_results");
            Directory.CreateDirectory(baseDir); // upewnia się, że katalog istnieje
            Directory.CreateDirectory(resultDir);
            CsvLogger logger = new CsvLogger($"results_{config}.csv");

           
                
                for (int size = 50 ; size <= 2000; size *= 5) {
                    string file_inpt = Path.Combine(baseDir, $"matrix{size}x{size}.txt");
                    string file_outp_asm = Path.Combine(resultDir, $"asm_{size}x{size}.txt");
                    string file_outp_cpp = Path.Combine(resultDir, $"cpp_{size}x{size}.txt");
                    string file_resAsm = Path.Combine(resultDir, $"res_asm{size}x{size}.txt");
                    string file_resCpp = Path.Combine(resultDir, $"res_cpp{size}x{size}.txt");
                generator.GenerateMatrix(size, file_inpt); //zeby nie mial tego samego pliku bo potem czas ~0ms


                    for (int threads = 1; threads <= 64; threads *= 2) {
                      
                            logger.LogResult(size, threads: threads, mode: "ASM", elapsedMs: P_exe.run_asm(file_inpt, threads, file_outp_asm, file_resAsm) );
                            logger.LogResult(size, threads: threads, mode: "CPP", elapsedMs: P_exe.run_cpp(file_inpt, threads, file_outp_cpp, file_resCpp));
                        }
            }
        }


        private bool VerifyResults(string matrixPath, float[] actualResult)
        {
            string solutionPath = matrixPath + ".sol";

            // 1. Sprawdź czy plik z rozwiązaniem istnieje
            if (!File.Exists(solutionPath))
            {
                MessageBox.Show("Nie znaleziono pliku z poprawnym wynikiem (.sol). Nie można zweryfikować.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return false;
            }

            try
            {
                // 2. Wczytaj oczekiwane wyniki
                string content = File.ReadAllText(solutionPath);
                float[] expectedResult = content.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)
                                                .Select(s => float.Parse(s, System.Globalization.CultureInfo.InvariantCulture))
                                                .ToArray();

                // 3. Sprawdź długość
                if (actualResult.Length != expectedResult.Length)
                {
                    MessageBox.Show($"Niezgodność rozmiarów! Otrzymano {actualResult.Length}, oczekiwano {expectedResult.Length}.", "Błąd Weryfikacji", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }

                // 4. Porównaj każdy element z tolerancją błędu
                float epsilon = 0.05f; // Dość luźna tolerancja dla dużych macierzy float
                int errorCount = 0;

                for (int i = 0; i < actualResult.Length; i++)
                {
                    float diff = Math.Abs(actualResult[i] - expectedResult[i]);

                    // Jeśli różnica jest zbyt duża
                    if (diff > epsilon)
                    {
                        // Dla celów debugowania możesz wypisać pierwszy błąd
                        if (errorCount == 0)
                            Console.WriteLine($"Błąd w indeksie {i}: Oczekiwano {expectedResult[i]}, jest {actualResult[i]}");

                        errorCount++;
                    }
                }

                if (errorCount > 0)
                {
                    MessageBox.Show($"Weryfikacja NIEPOWODZENIE. Znaleziono {errorCount} błędnych wyników.", "Wynik testu", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }

                return true; // Wszystko OK
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Błąd podczas weryfikacji: {ex.Message}");
                return false;
            }
        }

    }



    public class CsvLogger
    {
        private readonly string filePath;

        public CsvLogger(string filePath)
        {
            this.filePath = filePath;

            // jeśli plik nie istnieje — utwórz nagłówek
            if (!File.Exists(filePath))
            {
                File.WriteAllText(filePath, "MatrixSize,Threads,Mode,ElapsedMs\n");
            }
        }

        public void LogResult(int size, int threads, string mode, long elapsedMs) //mode: "ASM" lub "CPP"
        {
            string line = $"{size},{threads},{mode},{elapsedMs.ToString(CultureInfo.InvariantCulture)}";
            File.AppendAllText(filePath, line + Environment.NewLine);
        }
    }
}

/*   Console.WriteLine("Podaj rozmiar macierzy (np. 10): ");
            this.size = int.Parse(Console.ReadLine());

            Console.WriteLine("Podaj dolną granicę przedziału (np. -5): ");
            this.min = float.Parse(Console.ReadLine(), CultureInfo.InvariantCulture);

            Console.WriteLine("Podaj górną granicę przedziału (np. 5): ");
            this.max = float.Parse(Console.ReadLine(), CultureInfo.InvariantCulture);

            Console.WriteLine("Podaj nazwę pliku wyjściowego (np. matrix10x10.txt): ");
            this.fileName = Console.ReadLine();

 
 
 
   public void numeric_stability_test()
        {
            //generuj macierz z bardzo małymi i bardzo dużymi liczbami
            //porównaj wyniki asm i cpp
            for (int size = 50; size <= 2000; size *= 10)
            {
                Random rnd = new Random();
                float min = (float)(rnd.NextDouble() * -100);
                float max = (float)(rnd.NextDouble() * 100);
                string fileName = $"matrix{size}x{size}.txt";
               


            }

        }
 
 
 






 public void GenerateMatrix(int size, string file)
        {
            Random rand = new Random();

            using (StreamWriter writer = new StreamWriter(file))
            {
                for (int i = 0; i < size; i++)
                {
                    string[] row = new string[size];
                    for (int j = 0; j < size; j++)
                    {
                        float value = (min + (float)(rand.NextDouble() * (max - min)));// rand.NextDouble() → losuje wartości zmiennoprzecinkowe z przedziału[0, 1)
                        row[j] = value.ToString("0.00", CultureInfo.InvariantCulture); // 4 miejsca po przecinku
                        //Console.WriteLine(row[j]);
                    }
                    string line = string.Join(" ", row);

                    // Zapisuje cały wiersz do pliku
                    writer.WriteLine(line);

                }
            
            }
        }
 
 
 */
