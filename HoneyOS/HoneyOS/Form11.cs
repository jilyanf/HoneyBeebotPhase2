using System;
using System.Drawing;
using System.Windows.Forms;

namespace HoneyOS
{
    public partial class Form11 : Form
    {
        private algo chosenAlgo;

        // Public property to expose chosenAlgo
        public algo SelectedPolicy
        {
            get { return chosenAlgo; }
            set { chosenAlgo = value; }
        }

        // Parameterless constructor for designer compatibility
        public Form11()
        {
            InitializeComponent();
        }

        // Constructor with algorithm parameter
        public Form11(algo algorithm) : this() // Calls the parameterless constructor
        {
            this.chosenAlgo = algorithm;
            DisplaySelectedAlgorithm();
        }

        private void DisplaySelectedAlgorithm()
        {
            if (chosenAlgo != null) // Check if algorithm was provided
            {
                // Instead of MessageBox, update a label or other control
                label1.Text = $"Selected Algorithm: {chosenAlgo.ToString()}";

                // Optional: Change label appearance
                label1.ForeColor = Color.Blue;
                label1.Font = new Font(label1.Font.FontFamily, 12, FontStyle.Bold);
                label1.AutoSize = true;
            }
        }

        private void label1_Click(object sender, EventArgs e)
        {
            // Empty implementation
        }
    }
}