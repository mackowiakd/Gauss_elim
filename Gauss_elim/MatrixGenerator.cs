using Gauss_elim.threading;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Net.Mime.MediaTypeNames;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Rebar;

namespace Gauss_elim.testing
{
    public class MatrixGenerator
    {
        public int size { get; set; }
        public float min { get; set; }
        public float max { get; set; }
        public string solutionPath;
        public string inputFilePath;




        public MatrixGenerator(float min, float max)
        {
            this.min = min;
            this.max = max;
          
          

        }


        public string GenerateMatrix(int size,string outDir)
        {
            // 1. Pobierz katalog, w którym ma być plik
            string directory = Path.GetDirectoryName(outDir);
           
           
           

            string fileNameNoExt = $"matrix{size}x{size}_random";

            // POPRAWKA 2: Budowanie pełnych ścieżek
            this.solutionPath = Path.Combine(outDir, fileNameNoExt + "_sol.txt");

            // Używamy rozszerzenia .txt dla pliku wejściowego (możesz zmienić na _input.txt jeśli wolisz)
            this.inputFilePath = Path.Combine(outDir, fileNameNoExt + ".txt");
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
            using (StreamWriter writer = new StreamWriter(this.inputFilePath))
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

            return inputFilePath;
        }

        /// <summary>
        /// Generuje macierz o przewidywalnym wzorcu (nie losową).
        /// Służy do wykrywania błędów przesunięcia wskaźników (stride/padding) i błędów AVX.
        /// Konwencja nazwy: matrix{size}x{size}_pattern.txt
        /// </summary>
        public string GeneratePatternMatrix(int size, string outputDir)
        {
            string baseName = $"matrix{size}x{size}_pattern";
            string inputFile = Path.Combine(outputDir, $"{baseName}.txt");
            string solutionFile = Path.Combine(outputDir, $"{baseName}_sol.txt");
            Directory.CreateDirectory(baseName); // upewnia się, że katalog istnieje


            // W tym teście zakładamy, że rozwiązaniem są same JEDYNKI.
            // Ułatwia to weryfikację: b będzie po prostu sumą wiersza.
            float[] secretX = Enumerable.Repeat(1.0f, size).ToArray();

            // Zapisz proste rozwiązanie
            SaveSolution(solutionFile, secretX);

            using (StreamWriter sw = new StreamWriter(inputFile))
            {


                for (int i = 0; i < size; i++)
                {
                    double rowSumB = 0;
                    string line = "";

                    for (int j = 0; j < size; j++)
                    {
                        // WZORZEC: Wartość rośnie sekwencyjnie.
                        // Np. 10.0, 10.1, 10.2... 
                        // Jeśli coś zniknie lub zostanie nadpisane, od razu to zauważysz w podglądzie.
                        float valA = 10.0f + i + (j * 0.01f);

                        line += valA.ToString("F4", CultureInfo.InvariantCulture) + " ";
                        rowSumB += valA * 1.0f; // Bo x=1
                    }

                    line += rowSumB.ToString("F4", CultureInfo.InvariantCulture);
                    sw.WriteLine(line);
                }
            }

            return inputFile;
        }



        private void SaveSolution<T>(string filepath, IEnumerable<T> data)
        {
            using (StreamWriter sw = new StreamWriter(filepath))
            {
                // Zapisujemy w jednej linii oddzielone spacjami (standardowy format wektora)
                sw.WriteLine(string.Join(" ", data.Select(x => string.Format(CultureInfo.InvariantCulture, "{0}", x))));
            }
        }

    };



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
        public void run_tests(string config, int iter) {
            string baseDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"test_data_{config}");
            string resultDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "test_results");
            Directory.CreateDirectory(baseDir); // upewnia się, że katalog istnieje
            Directory.CreateDirectory(resultDir);
            CsvLogger logger = new CsvLogger($"results_{config}_{iter}.csv");

           
                
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


        

    }

    public class memory_test
    {
        ParallelExecutor P_exe = new ParallelExecutor();
      
        MatrixGenerator G= new MatrixGenerator(-149.0f,189.0f);

      

        public void test_sequnetial_asm(string tests_name)
        {
            string baseDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Random_data_tests");
            Directory.CreateDirectory(baseDir);
        
           
            // Lista rozmiarów do przetestowania
            // 160 = wyrównane do AVX (łatwe)
            // 161 = niewyrównane (testuje layout pamięci)
            int[] sizesToTest = {  211, 14, 3, 9, 7 };

            foreach (int size in sizesToTest)
            {
                Console.WriteLine($"\n--- TESTING SIZE: {size} ---");

               
                string inputFile = G.GenerateMatrix(size, baseDir);
                //string inputFile =GeneratePatternMatrix(size, baseDir); // Użyj tego jak coś nie działa

                // Ustal nazwy wyjściowe zgodnie z konwencją
                string fileNameOnly = Path.GetFileNameWithoutExtension(inputFile);
                string outputFileAsm = Path.Combine(baseDir, $"{fileNameOnly}_asm_out.txt");
                string outputFileResAsm = Path.Combine(baseDir, $"{fileNameOnly}_asm_result.txt");
                string outputFileCpp = Path.Combine(baseDir, $"{fileNameOnly}_cpp_out.txt");
                string outputFileResCpp = Path.Combine(baseDir, $"{fileNameOnly}_cpp_result.txt");

                // 2. Uruchom Asembler
                P_exe.run_asm_sequential(inputFile, outputFileAsm, outputFileResAsm);
                P_exe.run_cpp(inputFile,1, outputFileCpp, outputFileResCpp);
               MatrixComparator.CompareRowEchelonFiles(outputFileResCpp, outputFileResAsm, "ASM","CPP");
               MatrixComparator.CompareRowEchelonFiles(outputFileCpp, outputFileAsm,  "ASM_out", "CPP_out");
                //asm vs generated solution
                MatrixComparator.CompareRowEchelonFiles(G.solutionPath, outputFileResAsm,  "Generated_sol", "ASM");
            }
        }
    }
  
    
        public static class MatrixComparator
        {
        /// <summary>
        /// Porównuje dwa pliki tekstowe zawierające liczby zmiennoprzecinkowe.
        /// </summary>
        /// <param name="pathCpp">Ścieżka do wyniku z C++ (Wzorzec)</param>
        /// <param name="pathAsm">Ścieżka do wyniku z ASM (Testowany)</param>
        /// <param name="tolerance">Dopuszczalna różnica (domyślnie 0.01)</param>
        /// <param name="config1">Nazwa konfiguracji 1 (np. "CPP")</param>
        /// <param name="config2">Nazwa konfiguracji 2 (np. "ASM")</param>
        public static void CompareRowEchelonFiles(string pathCpp, string pathAsm, string config1, string config2, float tolerance = 0.09f)
            {
                Console.WriteLine($"\n--- PORÓWNANIE: {config1} vs {config2} ---");
                Console.WriteLine($"{config1}: {Path.GetFileName(pathCpp)}");
                Console.WriteLine($"{config2}: {Path.GetFileName(pathAsm)}");

                if (!File.Exists(pathCpp) || !File.Exists(pathAsm))
                {
                    PrintColor("[BŁĄD] Brakuje jednego z plików!", ConsoleColor.Red);
                    return;
                }

                try
                {
                    // 1. Wczytaj wszystkie liczby do płaskich tablic
                    float[] valsCpp = LoadFloats(pathCpp);
                    float[] valsAsm = LoadFloats(pathAsm);

                    // 2. Sprawdź liczbę elementów
                    if (valsCpp.Length != valsAsm.Length)
                    {
                        PrintColor($"[FAIL] Różna liczba elementów!", ConsoleColor.Red);
                        Console.WriteLine($"   {config1}: {valsCpp.Length}");
                        Console.WriteLine($"   {config2}: {valsAsm.Length}");
                        Console.WriteLine("   To sugeruje, że jeden algorytm zapisał więcej/mniej danych (np. padding).");
                        return;
                    }

                    // 3. Porównanie element po elemencie
                    int errorCount = 0;
                    float maxDiff = 0.0f;
                    int firstErrorIndex = -1;

                    for (int i = 0; i < valsCpp.Length; i++)
                    {
                        float v1 = valsCpp[i];
                        float v2 = valsAsm[i];
                        float diff = Math.Abs(v1 - v2);

                        if (diff > maxDiff) maxDiff = diff;

                        if (diff > tolerance)
                        {
                            if (errorCount == 0) firstErrorIndex = i;
                            errorCount++;
                        }
                    }

                    // 4. Raport
                    Console.WriteLine($"Liczba elementów: {valsCpp.Length}");
                    Console.WriteLine($"Maksymalna różnica: {maxDiff:F6}");

                    if (errorCount == 0)
                    {
                        PrintColor($"[SUKCES] Pliki są identyczne (w granicach tolerancji {tolerance}).", ConsoleColor.Green);
                    }
                    else
                    {
                        PrintColor($"[PORAŻKA] Znaleziono {errorCount} różnic powyżej tolerancji!", ConsoleColor.Red);

                        if (firstErrorIndex != -1)
                        {
                            Console.WriteLine($"   Pierwszy błąd pod indeksem {firstErrorIndex}:");
                            Console.WriteLine($"   {config1}: {valsCpp[firstErrorIndex]:F6}");
                            Console.WriteLine($"   {config2}: {valsAsm[firstErrorIndex]:F6}");
                            Console.WriteLine($"   Diff: {Math.Abs(valsCpp[firstErrorIndex] - valsAsm[firstErrorIndex]):F6}");

                            // Diagnostyka "Zer ASM"
                            if (Math.Abs(valsAsm[firstErrorIndex]) < 1e-9 && Math.Abs(valsCpp[firstErrorIndex]) > 1.0)
                            {
                                Console.WriteLine($"   [!] {config2} ma ZERO tam, gdzie {config1} ma dane. To sugeruje błąd Stride lub YMM Zeroing.");
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    PrintColor($"[CRITICAL] Błąd podczas parsowania plików: {ex.Message}", ConsoleColor.DarkRed);
                }
            }

            // Pomocnicza: Czyta plik ignorując formatowanie (spacje, entery, tabulatory)
            private static float[] LoadFloats(string path)
            {
                var text = File.ReadAllText(path);
                // Dzielimy po wszystkich białych znakach
                var tokens = text.Split(new[] { ' ', '\n', '\r', '\t' }, StringSplitOptions.RemoveEmptyEntries);

                var list = new List<float>();
                foreach (var token in tokens)
                {
                    // Ignorujemy nagłówki typu "Size: 160" jeśli parser natrafi na coś, co nie jest liczbą
                    if (float.TryParse(token, NumberStyles.Float, CultureInfo.InvariantCulture, out float val))
                    {
                        list.Add(val);
                    }
                }
                return list.ToArray();
            }

            private static void PrintColor(string msg, ConsoleColor color)
            {
                Console.ForegroundColor = color;
                Console.WriteLine(msg);
                Console.ResetColor();
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


/*  public void back_sub_test(string tests_name)
        {
          
            int threads = 4;
            string message;
            bool isCorrect;
            string baseDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "sln_test");
            string resultDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"test_asser{tests_name}");
            Directory.CreateDirectory(baseDir); // upewnia się, że katalog istnieje
            Directory.CreateDirectory(resultDir);

            

            List<int> sizesToTest = new List<int>();
          

            // Dodaj konkretne małe wartości, gdzie błędy są najczęstsze
            sizesToTest.AddRange(new int[] {  13, 48, 217 });

          
          
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
                isCorrect = generator.VerifyResults(file_resAsm, out message, "asm");

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
                //isCorrect = generator.VerifyResults(file_resCpp, out message, "cpp");
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




        public bool VerifyResults(string outputFilePath, out string message,string mode)
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
                message = $"{mode}\nWERYFIKACJA: NIEPOWODZENIE ❌\nZnaleziono {errorCount} błędnych wyników.\nMaksymalny błąd: {maxError:F6}";
                return false;
            }

            message = $"{mode}\nWERYFIKACJA: SUKCES ✅\nWyniki są poprawne.\nMaksymalne odchylenie: {maxError:F6}";
            return true;

        }

*/