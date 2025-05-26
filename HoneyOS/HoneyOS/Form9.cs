using System;
using System.Windows.Forms;

namespace HoneyOS
{
    public partial class Form9 : Form
    {
        private string selectedAlgorithm; // Field to store the passed algorithm

        // Modified constructor to accept the algorithm
        public Form9(string chosenAlgo)
        {
            InitializeComponent();
            this.selectedAlgorithm = chosenAlgo;
            DisplaySelectedAlgorithm(); // Call method to display the algorithm
        }

        private void DisplaySelectedAlgorithm()
        {
            // You can display it in different ways:
            
            // Option 1: Set it as the form's title
            this.Text = $"Scheduling Policy - {selectedAlgorithm}";
            
            // Option 2: Display in the existing label
            label1.Text = $"Selected Policy: {selectedAlgorithm}";
            
            // Option 3: Show a message (though you said you don't want MessageBox)
            // MessageBox.Show($"Selected algorithm: {selectedAlgorithm}");
        }

        private void button1_Click(object sender, EventArgs e)
        {
            // Your existing button click code
        }

        private void label1_Click(object sender, EventArgs e)
        {
            // Your existing label click code
        }
    }
}