using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


   
using System.IO;

using System.Globalization;

    namespace MatrixTests
    {
        public class MatrixDataGenerator
        {
            private readonly Random _rand = new Random();

            /// <summary>
            /// Generuje macierz o znanym wzorcu (Pattern) - idealna do debugowania pamięci/stride.
            /// Wartości rosną sekwencyjnie (np. 10.00, 10.01...), co ułatwia śledzenie błędów.
            /// Oczekiwane rozwiązanie to zawsze wektor samych JEDYNEK [1, 1, ..., 1].
            /// </summary>
            public string GeneratePatternMatrix(int size, string outputDir)
            {
                string baseName = $"matrix{size}x{size}_pattern";
                string inputFile = Path.Combine(outputDir, $"{baseName}.txt");
                string solutionFile = Path.Combine(outputDir, $"{baseName}_sol.txt");

                // Rozwiązanie wzorcowe: same jedynki
                float[] x = Enumerable.Repeat(1.0f, size).ToArray();
                SaveVector(solutionFile, x);

                using (StreamWriter sw = new StreamWriter(inputFile))
                {
                    // Nagłówek: Rozmiar N
                    sw.WriteLine($"{size}");

                    for (int i = 0; i < size; i++)
                    {
                        double rowSumB = 0;
                        string line = "";

                        for (int j = 0; j < size; j++)
                        {
                            // Wzór: 10 + wiersz + (kolumna * 0.01)
                            // Unikamy zer, aby łatwo wykryć błędy zerowania AVX
                            double valA = 10.0 + i + (j * 0.01);

                            line += valA.ToString("F4", CultureInfo.InvariantCulture) + " ";
                            rowSumB += valA * x[j]; // A*x = b
                        }

                        // Dopisz wyraz wolny b na końcu
                        line += rowSumB.ToString("F4", CultureInfo.InvariantCulture);
                        sw.WriteLine(line);
                    }
                }
                return inputFile;
            }

            /// <summary>
            /// Generuje losową macierz do testów obciążeniowych.
            /// Rozwiązanie to losowe liczby całkowite z zakresu [-10, 10].
            /// </summary>
            public string GenerateRandomMatrix(int size, string outputDir)
            {
                string baseName = $"matrix{size}x{size}_random";
                string inputFile = Path.Combine(outputDir, $"{baseName}.txt");
                string solutionFile = Path.Combine(outputDir, $"{baseName}_sol.txt");

                // Losowe rozwiązanie
                float[] x = new float[size];
                for (int i = 0; i < size; i++) x[i] = _rand.Next(-10, 11);
                SaveVector(solutionFile, x);

                using (StreamWriter sw = new StreamWriter(inputFile))
                {
                    sw.WriteLine($"{size}");

                    for (int i = 0; i < size; i++)
                    {
                        double rowSumB = 0;
                        string line = "";
                        for (int j = 0; j < size; j++)
                        {
                            double valA = -100.0 + (_rand.NextDouble() * 200.0);
                            line += valA.ToString("F4", CultureInfo.InvariantCulture) + " ";
                            rowSumB += valA * x[j];
                        }
                        line += rowSumB.ToString("F4", CultureInfo.InvariantCulture);
                        sw.WriteLine(line);
                    }
                }
                return inputFile;
            }

            private void SaveVector(string path, float[] vector)
            {
                using (StreamWriter sw = new StreamWriter(path))
                {
                    // Zapisujemy wektor w jednej linii oddzielony spacjami
                    string content = string.Join(" ", vector.Select(v => v.ToString("F4", CultureInfo.InvariantCulture)));
                    sw.WriteLine(content);
                }
            }
        }
    }

