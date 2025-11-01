using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace Gauss_elim.testing
{
    public class MatrixGenerator{
        public int size { get;  set; }
        public float min { get;  set; }
        public float max { get;  set; }
        public string fileName { get;  set; }
        

        public MatrixGenerator(int size, float min, float max, string file)
        {

            this.size = size;
            this.min = min; 
            this.max = max;
            this.fileName = file;
            GenerateMatrix();

          
        }

        public void GenerateMatrix()
        {
            Random rand = new Random();

            using (StreamWriter writer = new StreamWriter(fileName))
            {
                for (int i = 0; i < size; i++)
                {
                    string[] row = new string[size];
                    for (int j = 0; j < size; j++)
                    {
                        float value = (min + (float)(rand.NextDouble() * (max - min)));// rand.NextDouble() → losuje wartości zmiennoprzecinkowe z przedziału[0, 1)
                        row[j] = value.ToString("0.00", CultureInfo.InvariantCulture); // 4 miejsca po przecinku
                        Console.WriteLine(row[j]);
                    }
                   
                    writer.WriteLine(string.Join(" ", row));
                }
            }
        }

  
    }



    public class tests { 
        string file = "test_matrix.txt";
        float min;
        float max;
        public tests(float min, float max, string file)
        {
            this.min = min;
            this.max = max;
            this.file = file;
        }
        public void run_tests()
        {
            for (int threads = 1; threads <= 64; threads += 2)
            {

                for (int size = 50; size <= 2000; size *= 10)
                {
                    string fileName = $"matrix{size}x{size}.txt";
                    MatrixGenerator generator = new MatrixGenerator(size, min, max, file);
                    CsvLogger logger = new CsvLogger("results.csv");
                   
                    //write time to log
                    //save result to file


                    var sw = System.Diagnostics.Stopwatch.StartNew();
                    //call asm  
                    sw.Stop();
                    logger.LogResult(size, threads: 8, mode: "ASM", elapsedMs: sw.ElapsedMilliseconds);

                  
                    sw.Restart();
                    // call cpp
                    sw.Stop();
                    logger.LogResult(size, threads: 8, mode: "CPP", elapsedMs: sw.ElapsedMilliseconds);
                }
            }
        }

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
                MatrixGenerator generator = new MatrixGenerator(size, min, max, fileName);


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
            this.fileName = Console.ReadLine();*/
