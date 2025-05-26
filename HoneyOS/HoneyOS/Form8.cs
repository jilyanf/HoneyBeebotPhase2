using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HoneyOS
{
    // Task Manager Menu Form
    public partial class Form8 : Form
    {
        private Desktop desktopInstance; // Reference to an instance of Desktop form
        public bool button2_Clicked;
        public bool button1_Clicked;
        public algo chosenAlgo;
        public bool FIFO { get; set; }
        public bool PRIO { get; set; }
        public bool RRR { get; set; }
        public bool SJF { get; set; }
        public Form8(Desktop desktopInstance)
        {
            // Initializes the form components
            InitializeComponent();
            this.desktopInstance = desktopInstance; // Assign the reference to the instance of Desktop form
        }
        // Function that opens the Task Manager form
        //private void OpenFileFunction()
        //{
        //    Form6 taskManager = new Form6(desktopInstance);
        //    taskManager.UpdateSchedulingAlgo(chosenAlgo);

        //    // Set the boolean properties
        //    taskManager.FIFO = this.FIFO;
        //    taskManager.SJF = this.SJF;
        //    taskManager.PRIO = this.PRIO;
        //    taskManager.RRR = this.RRR;

        //    taskManager.Show();
        //    this.Close();
        //}
        private void OpenFileFunction()
        {
            // Instead of opening Form6, open Form11 and pass the selected algorithm
            Form11 form11 = new Form11(chosenAlgo);
            form11.Show();
            this.Close();
        }

        //First Come First Served
        /* Click / MouseEnter / MouseLeave Functions */
        private void button1_Click(object sender, EventArgs e)
        {
            button1.BackColor = Color.FromArgb(255, 234, 177);
            chosenAlgo = algo.FIFO;
            FIFO = true;   
            OpenFileFunction();
            
        }
        private void button1_MouseEnter(object sender, EventArgs e)
        {
            button1.BackColor = Color.FromArgb(255, 234, 177);
        }
        private void button1_MouseLeave(object sender, EventArgs e)
        {
            button1.BackColor = Color.FromArgb(250, 240, 230);
        }

        //Shortest Job First
        /* Click / MouseEnter / MouseLeave Functions */
        private void button2_Click(object sender, EventArgs e)
        {
            button2.BackColor = Color.FromArgb(255, 234, 177);
            chosenAlgo = algo.SJF;
            SJF = true;
            OpenFileFunction();
        }
        private void button2_MouseEnter(object sender, EventArgs e)
        {
            button2.BackColor = Color.FromArgb(255, 234, 177);
        }
        private void button2_MouseLeave(object sender, EventArgs e)
        {
            button2.BackColor = Color.FromArgb(250, 240, 230);
        }

        //Priority
        /* Click / MouseEnter / MouseLeave Functions */
        private void button3_MouseEnter(object sender, EventArgs e)
        {
            button3.BackColor = Color.FromArgb(255, 234, 177);
        }
        private void button3_MouseLeave(object sender, EventArgs e)
        {
            button3.BackColor = Color.FromArgb(250, 240, 230);
        }
        private void button3_Click(object sender, EventArgs e)
        {
            button3.BackColor = Color.FromArgb(255, 234, 177);
            chosenAlgo = algo.PRIO;
            PRIO = true;
            OpenFileFunction();
        }

        // Round Robin
        /* Click / MouseEnter / MouseLeave Functions */
        private void button4_MouseEnter(object sender, EventArgs e)
        {
            button4.BackColor = Color.FromArgb(255, 234, 177);
        }
        private void button4_MouseLeave(object sender, EventArgs e)
        {
            button4.BackColor = Color.FromArgb(250, 240, 230);
        }
        private void button4_Click(object sender, EventArgs e)
        {
            button4.BackColor = Color.FromArgb(255, 234, 177);
            chosenAlgo = algo.RRR;
            RRR = true;
            OpenFileFunction();
        }
    }
}
