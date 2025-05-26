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

            // Set up event handlers
            checkBox1.CheckedChanged += AllocationStrategy_CheckedChanged;
            checkBox2.CheckedChanged += AllocationStrategy_CheckedChanged;
            checkBox3.CheckedChanged += AllocationStrategy_CheckedChanged;

            checkBox4.CheckedChanged += DefragPolicy_CheckedChanged;
            checkBox5.CheckedChanged += DefragPolicy_CheckedChanged;
            checkBox6.CheckedChanged += DefragPolicy_CheckedChanged;

            button1.Click += SubmitButton_Click;

            // Initially disable defragmentation strategy checkboxes
            checkBox7.Enabled = false;
            checkBox8.Enabled = false;
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

        private void AllocationStrategy_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox clickedCheckbox = sender as CheckBox;

            // If the clicked checkbox is being checked
            if (clickedCheckbox.Checked)
            {
                // Uncheck other allocation strategy checkboxes
                if (clickedCheckbox != checkBox1) checkBox1.Checked = false;
                if (clickedCheckbox != checkBox2) checkBox2.Checked = false;
                if (clickedCheckbox != checkBox3) checkBox3.Checked = false;
            }
        }

        private void DefragPolicy_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox clickedCheckbox = sender as CheckBox;

            // If the clicked checkbox is being checked
            if (clickedCheckbox.Checked)
            {
                // Uncheck other defragmentation policy checkboxes
                if (clickedCheckbox != checkBox4) checkBox4.Checked = false;
                if (clickedCheckbox != checkBox5) checkBox5.Checked = false;
                if (clickedCheckbox != checkBox6) checkBox6.Checked = false;

                // Enable/disable defragmentation strategy based on "Never" selection
                bool enableStrategies = (clickedCheckbox == checkBox6);
                checkBox7.Enabled = enableStrategies;
                checkBox8.Enabled = enableStrategies;

                // Uncheck strategy checkboxes if not "Never"
                if (!enableStrategies)
                {
                    checkBox7.Checked = false;
                    checkBox8.Checked = false;
                }
            }
        }

        private void SubmitButton_Click(object sender, EventArgs e)
        {
            // Validate allocation strategy selection
            if (!checkBox1.Checked && !checkBox2.Checked && !checkBox3.Checked)
            {
                MessageBox.Show("Please select one allocation strategy.", "Selection Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Validate defragmentation policy selection
            if (!checkBox4.Checked && !checkBox5.Checked && !checkBox6.Checked)
            {
                MessageBox.Show("Please select one defragmentation policy.", "Selection Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Validate defragmentation strategy if "Never" is selected
            if (checkBox6.Checked && !checkBox7.Checked && !checkBox8.Checked)
            {
                MessageBox.Show("Please select one defragmentation strategy.", "Selection Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // All validations passed - proceed with submission
            MessageBox.Show("Configuration submitted successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            // Here you would typically pass the selections to the next part of your application
        }

        // Add the missing click handlers
        private void label1_Click(object sender, EventArgs e) { }
        private void label2_Click(object sender, EventArgs e) { }
        private void label3_Click(object sender, EventArgs e) { }

        private void label3_Click_1(object sender, EventArgs e) { }
        private void label2_Click_1(object sender, EventArgs e) { }
    }
}