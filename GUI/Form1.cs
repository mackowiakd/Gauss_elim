using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;




namespace GUI
{
    public partial class Form1 : Form
    {
        private string inputPath = "";
        private bool useAsm = false;
        private bool useCpp = false;
        public Form1()
        {
            InitializeComponent();
        }

        public string GetInputFilePath()
        {
            return inputPath;
        }
        public bool IsUsingAsm()
        {
            return useAsm;
        }
        public bool IsUsingCpp()
        {
            return useCpp;
        }
        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            //bierze sciezke do pliku dla progrmu glownego main w cs
            // pobiera ścieżkę do pliku wejściowego
            inputPath = textBox1.Text.Trim();

            if (!File.Exists(inputPath))
            {
                textBox1.BackColor = System.Drawing.Color.LightCoral;
            }
            else
            {
                textBox1.BackColor = System.Drawing.Color.White;
            }
        }

        private void ASM_button_CheckedChanged(object sender, EventArgs e)
        {
            //powinna byc typu bool aby sprawdzac czy jest zaznaczona
            useAsm = ASM_button.Checked;
        }


        private void CPP_button_CheckedChanged(object sender, EventArgs e)
        {
            //powinna byc typu bool aby sprawdzac czy jest zaznaczona
            useCpp = CPP_button.Checked;
        }

        private void Run_button_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(inputPath) || !File.Exists(inputPath))
            {
                MessageBox.Show("Podaj poprawną ścieżkę do pliku wejściowego.", "Błąd", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!useAsm && !useCpp)
            {
                MessageBox.Show("Wybierz bibliotekę (ASM lub C++).", "Brak wyboru", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            //try
            //{
            //    var sw = System.Diagnostics.Stopwatch.StartNew();

            //    if (useAsm)
            //    {
            //        // wywołanie Twojej klasy ASM
                  
            //        ParallelExecutor P_exe = new ParallelExecutor();
            //        P_exe.run_asm(inputPath);
                 
            //    }
            //    else if (useCpp)
            //    {
            //        // wywołanie Twojej klasy C++
                  
            //        ParallelExecutor P_exe = new ParallelExecutor();
            //        P_exe.run_cpp(inputPath);
            //    }

            //    sw.Stop();

            //    MessageBox.Show($"Zakończono.\nCzas wykonania: {sw.ElapsedMilliseconds} ms", "Wynik", MessageBoxButtons.OK, MessageBoxIcon.Information);
            //}
            //catch (Exception ex)
            //{
            //    MessageBox.Show($"Błąd wykonania: {ex.Message}", "Błąd", MessageBoxButtons.OK, MessageBoxIcon.Error);
            //}
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            //if > 64 while loop with message?
        }
    }
}
