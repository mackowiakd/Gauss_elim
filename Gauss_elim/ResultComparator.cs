

//using System;
//using System.IO;
//using System.Linq;
//using System.Collections.Generic;
//using System.Globalization;

//namespace MatrixTests
//{
//    public class ResultComparator
//    {
//        /// <summary>
//        /// Porównuje plik z wynikiem algorytmu (calculated) z plikiem wzorcowym (expected).
//        /// Zwraca true, jeśli błąd jest w granicach tolerancji i struktura pliku jest OK.
//        /// </summary>
//        // Tolerancja błędu (float precision)
//        private const float Tolerance = 1e-4f;
//        public bool CompareResults(string resultPath, string solutionPath, float tolerance = 0.05f)
//        {
//            if (!File.Exists(resultPath))
//            {
//                Console.ForegroundColor = ConsoleColor.Red;
//                Console.WriteLine($"[ERR] Brak pliku wyniku: {Path.GetFileName(resultPath)}");
//                Console.ResetColor();
//                return false;
//            }

//            // 1. Wczytanie danych
//            float[] actual = LoadValues(resultPath);
//            float[] expected = LoadValues(solutionPath);

//            // 2. Walidacja Strukturalna (Czy ASM nie uciął pliku?)
//            if (actual.Length != expected.Length)
//            {
//                Console.ForegroundColor = ConsoleColor.Red;
//                Console.WriteLine($"[FAIL] Niezgodność rozmiarów! Oczekiwano: {expected.Length}, Otrzymano: {actual.Length}");
//                Console.ResetColor();
//                return false;
//            }

//            // 3. Detekcja "Zero Blocks" (Błąd AVX/Stride)
//            // Szukamy sekwencji 4 lub więcej zer, co w macierzach Pattern jest niemożliwe
//            int zeroRun = 0;
//            for (int i = 0; i < actual.Length; i++)
//            {
//                if (Math.Abs(actual[i]) < 1e-9) zeroRun++;
//                else zeroRun = 0;

//                if (zeroRun >= 4)
//                {
//                    Console.ForegroundColor = ConsoleColor.Magenta;
//                    Console.WriteLine($"[WARN] Wykryto blok zer (indeks {i}). Możliwy błąd AVX lub Stride!");
//                    Console.ResetColor();
//                    // Nie przerywamy, sprawdzamy dalej błąd matematyczny
//                    break;
//                }
//            }

//            // 4. Porównanie Matematyczne
//            float maxDiff = 0;
//            int errorCount = 0;

//            for (int i = 0; i < actual.Length; i++)
//            {
//                float diff = Math.Abs(actual[i] - expected[i]);
//                if (diff > maxDiff) maxDiff = diff;
//                if (diff > tolerance) errorCount++;
//            }

//            Console.WriteLine($"   Max Diff: {maxDiff:F6}");

//            if (errorCount == 0)
//            {
//                Console.ForegroundColor = ConsoleColor.Green;
//                Console.WriteLine($"[PASS] Wyniki poprawne (Max Diff < {tolerance})");
//                Console.ResetColor();
//                return true;
//            }
//            else
//            {
//                Console.ForegroundColor = ConsoleColor.Red;
//                Console.WriteLine($"[FAIL] Błędy w {errorCount} elementach. Wyniki rozbieżne.");
//                Console.ResetColor();
//                // Podgląd pierwszego błędu
//                for (int i = 0; i < actual.Length; i++)
//                {
//                    if (Math.Abs(actual[i] - expected[i]) > tolerance)
//                    {
//                        Console.WriteLine($"   Pierwszy błąd idx[{i}]: Oczekiwano {expected[i]}, jest {actual[i]}");
//                        break;
//                    }
//                }
//                return false;
//            }
//        }

//        private float[] LoadValues(string path)
//        {
//            var content = File.ReadAllText(path);
//            return content.Split(new[] { ' ', '\n', '\r', '\t' }, StringSplitOptions.RemoveEmptyEntries)
//                          .Select(s => float.Parse(s, CultureInfo.InvariantCulture))
//                          .ToArray();
//        }

//        public void CompareImplementations(string asmPath, string cppPath)
//        {
//            Console.WriteLine("--- Cross-Check (ASM vs CPP) ---");
//            if (!File.Exists(asmPath) || !File.Exists(cppPath)) return;

//            float[] asmVals = LoadValues(asmPath);
//            float[] cppVals = LoadValues(cppPath);

//            if (asmVals.Length != cppVals.Length)
//            {
//                Console.WriteLine($"[DIFF] Różne długości plików! ASM={asmVals.Length}, CPP={cppVals.Length}");
//                return;
//            }

//            float maxDiff = 0;
//            for (int i = 0; i < asmVals.Length; i++)
//            {
//                float diff = Math.Abs(asmVals[i] - cppVals[i]);
//                if (diff > maxDiff) maxDiff = diff;
//            }

//            if (maxDiff < Tolerance)
//            {
//                Console.ForegroundColor = ConsoleColor.Cyan;
//                Console.WriteLine($"[MATCH] Implementacje zgodne! (Max Diff: {maxDiff:E2})");
//            }
//            else
//            {
//                Console.ForegroundColor = ConsoleColor.Yellow;
//                Console.WriteLine($"[MISMATCH] Implementacje dają różne wyniki! (Max Diff: {maxDiff})");
//                Console.WriteLine("To oznacza, że jeden z algorytmów działa inaczej (prawdopodobnie ASM ma błąd Stride).");
//            }
//            Console.ResetColor();
//        }

//        private bool ContainsZeroBlocks(float[] data)
//        {
//            int run = 0;
//            foreach (var v in data)
//            {
//                if (Math.Abs(v) < 1e-9) run++;
//                else run = 0;
//                if (run >= 4) return true;
//            }
//            return false;
//        }

       
//        private void PrintColor(string msg, ConsoleColor color)
//        {
//            Console.ForegroundColor = color;
//            Console.WriteLine(msg);
//            Console.ResetColor();
//        }
//    }
//}
//}
