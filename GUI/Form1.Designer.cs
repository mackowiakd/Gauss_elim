namespace GUI
{
    partial class Form1
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.ASM_button = new System.Windows.Forms.CheckBox();
            this.CPP_button = new System.Windows.Forms.CheckBox();
            this.Run_button = new System.Windows.Forms.Button();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.numericUpDown1 = new System.Windows.Forms.NumericUpDown();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown1)).BeginInit();
            this.SuspendLayout();
            // 
            // ASM_button
            // 
            this.ASM_button.AutoSize = true;
            this.ASM_button.Location = new System.Drawing.Point(143, 198);
            this.ASM_button.Name = "ASM_button";
            this.ASM_button.Size = new System.Drawing.Size(58, 20);
            this.ASM_button.TabIndex = 0;
            this.ASM_button.Text = "ASM";
            this.ASM_button.UseVisualStyleBackColor = true;
            this.ASM_button.CheckedChanged += new System.EventHandler(this.ASM_button_CheckedChanged);
            // 
            // CPP_button
            // 
            this.CPP_button.AutoSize = true;
            this.CPP_button.Location = new System.Drawing.Point(275, 198);
            this.CPP_button.Name = "CPP_button";
            this.CPP_button.Size = new System.Drawing.Size(56, 20);
            this.CPP_button.TabIndex = 1;
            this.CPP_button.Text = "CPP";
            this.CPP_button.UseVisualStyleBackColor = true;
            this.CPP_button.CheckedChanged += new System.EventHandler(this.CPP_button_CheckedChanged);
            // 
            // Run_button
            // 
            this.Run_button.Location = new System.Drawing.Point(284, 315);
            this.Run_button.Name = "Run_button";
            this.Run_button.Size = new System.Drawing.Size(75, 23);
            this.Run_button.TabIndex = 2;
            this.Run_button.Text = "Run";
            this.Run_button.UseVisualStyleBackColor = true;
            this.Run_button.Click += new System.EventHandler(this.Run_button_Click);
            // 
            // textBox1
            // 
            this.textBox1.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.textBox1.Location = new System.Drawing.Point(143, 125);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(216, 28);
            this.textBox1.TabIndex = 3;
            this.textBox1.TextChanged += new System.EventHandler(this.textBox1_TextChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.label1.Location = new System.Drawing.Point(139, 102);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(124, 20);
            this.label1.TabIndex = 4;
            this.label1.Text = "Matrix file path ";
            // 
            // numericUpDown1
            // 
            this.numericUpDown1.Location = new System.Drawing.Point(211, 250);
            this.numericUpDown1.Name = "numericUpDown1";
            this.numericUpDown1.Size = new System.Drawing.Size(120, 22);
            this.numericUpDown1.TabIndex = 5;
            this.numericUpDown1.ValueChanged += new System.EventHandler(this.numericUpDown1_ValueChanged);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(534, 450);
            this.Controls.Add(this.numericUpDown1);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.Run_button);
            this.Controls.Add(this.CPP_button);
            this.Controls.Add(this.ASM_button);
            this.Name = "Form1";
            this.Text = "Form1";
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.CheckBox ASM_button;
        private System.Windows.Forms.CheckBox CPP_button;
        private System.Windows.Forms.Button Run_button;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.NumericUpDown numericUpDown1;
    }
}

