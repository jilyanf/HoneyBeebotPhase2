using System;
using System.Drawing;
using System.Windows.Forms;

namespace HoneyOS
{
    public partial class Form11 : Form
    {
        private algo chosenAlgo;
        private string selectedMemoryMode;

        // Add parameterless constructor for designer
        public Form11()
        {
            InitializeComponent();
        }

        public Form11(algo algorithm) : this()
        {
            this.chosenAlgo = algorithm;
            DisplaySelectedAlgorithm();
        }

        private void DisplaySelectedAlgorithm()
        {
            if (chosenAlgo != null)
            {
                label1.Text = $"Selected Algorithm: {chosenAlgo.ToString()}";
                label1.ForeColor = Color.Blue;
                label1.Font = new Font(label1.Font.FontFamily, 12, FontStyle.Bold);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            selectedMemoryMode = "Contiguous";
            OpenForm12();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            selectedMemoryMode = "Paged";
            OpenForm12();
        }

        private void OpenForm12()
        {
            MessageBox.Show($"Selected: {chosenAlgo.ToString()}, {selectedMemoryMode}", "Configuration Summary");
            Form12 form12 = new Form12(chosenAlgo, selectedMemoryMode);
            form12.Show();
            this.Close();
        }
    }
}