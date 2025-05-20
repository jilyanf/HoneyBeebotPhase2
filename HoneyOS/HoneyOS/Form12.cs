using System;
using System.Drawing;
using System.Windows.Forms;

namespace HoneyOS
{
    public partial class Form12 : Form
    {
        public Form12(algo algorithm, string memoryMode)
        {
            InitializeComponent();
            DisplayConfiguration(algorithm, memoryMode);

            // Adjust existing controls' positions
            MoveControlsDown(80);
        }

        private void DisplayConfiguration(algo algorithm, string memoryMode)
        {
            Panel summaryPanel = new Panel();
            summaryPanel.BackColor = Color.LightBlue;
            summaryPanel.Size = new Size(700, 60);
            summaryPanel.Location = new Point(50, 20);

            Label algoLabel = new Label();
            algoLabel.AutoSize = true;
            algoLabel.Font = new Font("Microsoft Sans Serif", 10, FontStyle.Bold);
            algoLabel.Location = new Point(10, 20);
            algoLabel.Text = $"Scheduling Algorithm: {algorithm}";

            Label memoryLabel = new Label();
            memoryLabel.AutoSize = true;
            memoryLabel.Font = new Font("Microsoft Sans Serif", 10, FontStyle.Bold);
            memoryLabel.Location = new Point(300, 20);
            memoryLabel.Text = $"Memory Mode: {memoryMode}";

            summaryPanel.Controls.Add(algoLabel);
            summaryPanel.Controls.Add(memoryLabel);
            this.Controls.Add(summaryPanel);
            summaryPanel.BringToFront();
        }

        private void MoveControlsDown(int pixels)
        {
            foreach (Control control in this.Controls)
            {
                if (control is Panel) continue; // Skip our new panel
                control.Top += pixels;
            }
        }

        // Add the missing click handlers
        private void label1_Click(object sender, EventArgs e) { }
        private void label2_Click(object sender, EventArgs e) { }
        private void label3_Click(object sender, EventArgs e) { }
    }
}