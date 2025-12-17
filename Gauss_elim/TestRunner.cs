//using System;
//using System.Collections.Generic;
//using System.Diagnostics;
//using System.IO;

//namespace MatrixTests
//{
//    public class TestRunner
//    {
//        private readonly MatrixDataGenerator _generator;
//        private readonly ResultComparator _comparator;
//        private readonly string _baseDir;

//        // Ścieżka do skompilowanego pliku wykonywalnego C++
//        // Upewnij się, że ten plik istnieje!
//        private const string CppExecutablePath = @"C:\Users\Dominika\source\repos\JA_proj\x64\Release\GaussCpp.exe";

//        public TestRunner()
//        {
//            _generator = new MatrixDataGenerator();
//            _comparator = new ResultComparator();
//            _baseDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TestLab");
//            Directory.CreateDirectory(_baseDir);
//        }

//        public void RunSuite(List<int> sizes, bool usePattern = true)
//        {
//            Console.WriteLine($"=== START TESTÓW (Mode: {(usePattern ? "Pattern" : "Random")}) ===");

//            foreach (int size in sizes)
//            {
//                Console.WriteLine($"\n=======================================================");
//                Console.WriteLine($" TEST ROZMIARU: {size}x{size}");
//                Console.WriteLine($"=======================================================");

//                // 1. GENEROWANIE DANYCH
//                string inputFile;
//                if (usePattern)
//                    inputFile = _generator.GeneratePatternMatrix(size, _baseDir);
//                else
//                    inputFile = _generator.GenerateRandomMatrix(size, _baseDir);

//                string baseName = Path.GetFileNameWithoutExtension(inputFile);
//                string asmOutFile = Path.Combine(_baseDir, $"{baseName}_res_ASM.txt");
//                string cppOutFile = Path.Combine(_baseDir, $"{baseName}_res_CPP.txt");
//                string solutionFile = Path.Combine(_baseDir, $"{baseName}_sol.txt");

//                try
//                {
//                    // 2. URUCHOMIENIE ASM (Twoja implementacja)
//                    Console.Write("[ASM] Uruchamianie... ");
//                    // Wywołanie Twojego wrappera. Zakładam, że P_exe przyjmuje (input, asm_dump, result)
//                    Gauss_elim.P_exe.run_asm_sequential(inputFile, Path.ChangeExtension(asmOutFile, ".dump"), asmOutFile);
//                    Console.WriteLine("Gotowe.");

//                    // 3. URUCHOMIENIE C++ (Zewnętrzny proces)
//                    Console.Write("[CPP] Uruchamianie... ");
//                    RunCppProcess(inputFile, cppOutFile);
//                    Console.WriteLine("Gotowe.");

//                    // 4. PORÓWNANIE WYNIKÓW
//                    Console.WriteLine("\n--- ANALIZA WYNIKÓW ---");

//                    // A. Sprawdź ASM vs Wzorzec
//                    bool asmOk = _comparator.CompareResults("ASM vs SOL", asmOutFile, solutionFile);

//                    // B. Sprawdź CPP vs Wzorzec
//                    bool cppOk = _comparator.CompareResults("CPP vs SOL", cppOutFile, solutionFile);

//                    // C. Sprawdź ASM vs CPP (Najważniejsze dla Ciebie!)
//                    _comparator.CompareImplementations(asmOutFile, cppOutFile);

//                }
//                catch (Exception ex)
//                {
//                    Console.WriteLine($"[CRITICAL] Błąd testu: {ex.Message}");
//                }
//            }
//        }

//        private void RunCppProcess(string inputPath, string outputPath)
//        {
//            if (!File.Exists(CppExecutablePath))
//            {
//                Console.WriteLine($"[SKIP] Nie znaleziono pliku C++ EXE: {CppExecutablePath}");
//                // Tworzymy pusty plik, żeby komparator nie wybuchł
//                File.WriteAllText(outputPath, "");
//                return;
//            }

//            // Zakładam, że Twój program C++ przyjmuje argumenty: [input_file] [output_file]
//            ProcessStartInfo startInfo = new ProcessStartInfo
//            {
//                FileName = CppExecutablePath,
//                Arguments = $"\"{inputPath}\" \"{outputPath}\"",
//                UseShellExecute = false,
//                RedirectStandardOutput = true,
//                CreateNoWindow = true
//            };

//            using (Process process = Process.Start(startInfo))
//            {
//                process.WaitForExit();
//                if (process.ExitCode != 0)
//                {
//                    Console.WriteLine($"[CPP ERROR] Kod wyjścia: {process.ExitCode}");
//                }
//            }
//        }
//    }
//}