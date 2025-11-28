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
    public class MatrixGenerator
    {
        public int size { get; set; }
        public float min { get; set; }
        public float max { get; set; }
        public string solutionPath;




        public MatrixGenerator(float min, float max)
        {
            this.min = min;
            this.max = max;
          
          

        }


        public void GenerateMatrix(int size, string file)
        {
            // 1. Pobierz katalog, w którym ma być plik
            string directory = Path.GetDirectoryName(file);

            // 2. Pobierz samą nazwę pliku BEZ rozszerzenia (np. "matrix250x250")
            string fileNameNoExt = Path.GetFileNameWithoutExtension(file);

            // 3. Zbuduj nową ścieżkę z dopiskiem "_sol.txt"
            // Wynik: C:\...\matrix250x250_sol.txt
            this.solutionPath = Path.Combine(directory, fileNameNoExt + "_sol.txt");

            Random rand = new Random();

            // KROK 1: Wylosuj "tajny" wynik X (na liczbach całkowitych)
            // Np. losowe liczby od -10 do 10
            int[] secretX = new int[size];
            for (int i = 0; i < size; i++)
            {
                secretX[i] = rand.Next(-10, 11);
            }


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


        public bool VerifyResults(string outputFilePath, out string message)
        {


            // --- KROK 1: Wczytanie Twojego obliczonego wyniku z pliku ---

            if (!File.Exists(outputFilePath))
            {
                message = "Plik z wynikami nie został utworzony. Coś poszło nie tak z obliczeniami.";
                return false;
            }

            float[] calculatedResult;
            try
            {
                string content = File.ReadAllText(outputFilePath);

                // Parsujemy liczby rozdzielone spacjami (lub nowymi liniami/tabami dla pewności)
                calculatedResult = content.Split(new[] { ' ', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries)
                                          .Select(s => float.Parse(s, CultureInfo.InvariantCulture))
                                          .ToArray();
            }
            catch (Exception ex)
            {
                message = $"Błąd podczas odczytu pliku wyników: {ex.Message}";
                return false;
            }

            // --- KROK 2: Znalezienie i wczytanie pliku wzorcowego (_sol.txt) ---



            if (!File.Exists(solutionPath))
            {
                message = "Nie znaleziono pliku wzorcowego (*_sol.txt). \nWyniki obliczono, ale nie można zweryfikować ich poprawności.";
                // Zwracamy true, bo sam program zadziałał, tylko testu nie ma z czym porównać
                return true;
            }

            float[] expectedResult;
            try
            {
                string content = File.ReadAllText(solutionPath);
                expectedResult = content.Split(new[] { ' ', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries)
                                        .Select(s => float.Parse(s, CultureInfo.InvariantCulture))
                                        .ToArray();
            }
            catch
            {
                message = "Błąd odczytu pliku wzorcowego.";
                return false;
            }

            // --- KROK 3: Porównanie (Logika bez zmian) ---

            if (calculatedResult.Length != expectedResult.Length)
            {
                message = $"BŁĄD WERYFIKACJI: Niezgodność wymiarów!\nObliczono: {calculatedResult.Length} liczb\nOczekiwano: {expectedResult.Length} liczb";
                return false;
            }

            float epsilon = 0.05f; // Tolerancja błędu
            int errorCount = 0;
            float maxError = 0.0f;

            for (int i = 0; i < calculatedResult.Length; i++)
            {
                float diff = Math.Abs(calculatedResult[i] - expectedResult[i]);
                if (diff > maxError) maxError = diff;

                if (diff > epsilon)
                {
                    errorCount++;
                }
            }

            if (errorCount > 0)
            {
                message = $"WERYFIKACJA: NIEPOWODZENIE ❌\nZnaleziono {errorCount} błędnych wyników.\nMaksymalny błąd: {maxError:F6}";
                return false;
            }

            message = $"WERYFIKACJA: SUKCES ✅\nWyniki są poprawne.\nMaksymalne odchylenie: {maxError:F6}";
            return true;

        }

    


};



    public class tests { 
      
        float min;
        float max;
        ParallelExecutor P_exe = new ParallelExecutor();
        MatrixGenerator generator;
        string file;
        public tests(float min, float max, string file)
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
                    string file_outp_asm = Path.Combine(resultDir, $"asm_outp{size}x{size}.txt");
                    string file_outp_cpp = Path.Combine(resultDir, $"cpp_outp{size}x{size}.txt");
                    string file_resAsm = Path.Combine(resultDir, $"res_asm{size}x{size}.txt");
                    string file_resCpp = Path.Combine(resultDir, $"res_cpp{size}x{size}.txt");
                generator.GenerateMatrix(size, file_inpt); //zeby nie mial tego samego pliku bo potem czas ~0ms


                    for (int threads = 1; threads <= 64; threads *= 2) {
                      
                            logger.LogResult(size, threads: threads, mode: "ASM", elapsedMs: P_exe.run_asm(file_inpt, threads, file_outp_asm, file_resAsm) );
                            logger.LogResult(size, threads: threads, mode: "CPP", elapsedMs: P_exe.run_cpp(file_inpt, threads, file_outp_cpp, file_resCpp));
                        }
            }
        }

        public void back_sub_test(string tests_name)
        {
          
            int threads = 4;
            string message;
            bool isCorrect;
            string baseDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "sln_test");
            string resultDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"test_asser{tests_name}");
            Directory.CreateDirectory(baseDir); // upewnia się, że katalog istnieje
            Directory.CreateDirectory(resultDir);

            // Konfiguracja
            int minSize = 70;
            int maxSize = 200; // Nie za duże, żeby dało się czytać logi
            int totalTests = 20; // Max 20 testów

            List<int> sizesToTest = new List<int>();
            Random rand = new Random();

            //// KROK 1: Dodaj "złośliwe" przypadki brzegowe (Gwarancja różnego Modulo 8)
            // Chcemy mieć pewność, że przetestujemy rozmiar, który daje resztę 0, 1, 2... 7
            for (int remainder = 0; remainder < 8; remainder++)
            {
                // Losujemy jakąś bazę (np. 16, 24, 40...) i dodajemy resztę
                int baseNum = rand.Next(1, 10) * 8;
                sizesToTest.Add(baseNum + remainder);
            }

            // KROK 2: Dodaj konkretne małe wartości, gdzie błędy są najczęstsze
            sizesToTest.AddRange(new int[] { 3, 7, 8, 9, 15, 16, 17, 48, 50 });

          
            // KROK 3: Dopełnij losowymi wartościami do limitu (np. 20)
            while (sizesToTest.Count < totalTests)
            {
                sizesToTest.Add(rand.Next(minSize, maxSize));
            }

            // KROK 4: Posortuj i usuń duplikaty
            // (Distinct jest ważny, żeby nie testować 8 dwa razy)
            sizesToTest = sizesToTest.Distinct().OrderBy(x => x).ToList();

            // --- PĘTLA TESTOWA ---
            Console.WriteLine($"Rozpoczynam testy dla {sizesToTest.Count} różnych rozmiarów...");

            foreach (int size in sizesToTest)
            {
                string file_inpt = Path.Combine(baseDir, $"matrix{size}x{size}.txt");

                generator.GenerateMatrix(size, file_inpt);

                string file_outp_asm = Path.Combine(resultDir, $"asm_{size}x{size}.txt");
                string file_resAsm = Path.Combine(resultDir, $"res_asm{size}x{size}.txt");

                string file_outp_cpp = Path.Combine(resultDir, $"cpp_{size}x{size}.txt"); 
                string file_resCpp = Path.Combine(resultDir, $"res_cpp{size}x{size}.txt");

              

                //__ASM_
                P_exe.run_asm(file_inpt, threads, file_outp_asm, file_resAsm);
                isCorrect = generator.VerifyResults(file_resAsm, out message);

                Console.WriteLine(message);

                // 4. (Opcjonalnie) Możesz też zareagować na wynik bool
                if (isCorrect)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("Test zaliczony!");
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Test niezaliczony.");
                }
                Console.ResetColor();

                //////__CPP__
                //P_exe.run_cpp(file_inpt, threads, file_outp_cpp, file_resCpp);
                //isCorrect = generator.VerifyResults(file_resCpp, out message);
                //// 3. Wypisz zwrócony komunikat w konsoli
                //Console.WriteLine(message);

                //// 4. (Opcjonalnie) Możesz też zareagować na wynik bool
                //if (isCorrect)
                //{
                //    Console.ForegroundColor = ConsoleColor.Green;
                //    Console.WriteLine("Test zaliczony!");
                //}
                //else
                //{
                //    Console.ForegroundColor = ConsoleColor.Red;
                //    Console.WriteLine("Test niezaliczony.");
                //}
                //Console.ResetColor();

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
