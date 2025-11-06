using Gauss_elim.threading;
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
      
        

        public MatrixGenerator( float min, float max)
        {
            this.min = min; 
            this.max = max;
      

          
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

  
    }



    public class tests { 
        string file_asm ;
        string file_cpp ;
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
                    generator.GenerateMatrix(size, file_inpt); //zeby nie mial tego samego pliku bo potem czas ~0ms


                for (int threads = 1; threads <= 64; threads *= 2) {
                      
                        logger.LogResult(size, threads: threads, mode: "ASM", elapsedMs: P_exe.run_asm(file_inpt, threads, file_outp_asm) );
                        logger.LogResult(size, threads: threads, mode: "CPP", elapsedMs: P_exe.run_cpp(file_inpt, threads, file_outp_cpp));
                    }
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
 
 
 
 
 
 */
