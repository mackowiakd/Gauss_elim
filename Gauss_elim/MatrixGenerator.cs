using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gauss_elim
{
    public class MatrixGenerator{
        public int size { get; private set; }
        public float min { get; private set; }
        public float max { get; private set; }
        public string fileName { get;  set; }
        

        public MatrixGenerator()
        {
            Console.WriteLine("Podaj rozmiar macierzy (np. 10): ");
            this.size = int.Parse(Console.ReadLine());

            Console.WriteLine("Podaj dolną granicę przedziału (np. -5): ");
            this.min = float.Parse(Console.ReadLine(), CultureInfo.InvariantCulture);

            Console.WriteLine("Podaj górną granicę przedziału (np. 5): ");
            this.max = float.Parse(Console.ReadLine(), CultureInfo.InvariantCulture);

            Console.WriteLine("Podaj nazwę pliku wyjściowego (np. matrix10x10.txt): ");
            this.fileName = Console.ReadLine();

            //GenerateMatrix( );
           
            Console.WriteLine($" Wygenerowano plik: {Path.GetFullPath(fileName)}");
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
}
