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


namespace HoneyOS
{
    public partial class Form4 : Form
    {
        public Form4()
        {
            InitializeComponent();
            LoadRecycleBinItems();
        }

        private void LoadRecycleBinItems()
        {
            string recycleBinPath = Path.Combine(Application.StartupPath, "RecycleBin");

            if (!Directory.Exists(recycleBinPath))
            {
                Directory.CreateDirectory(recycleBinPath);
            }

            listBox1.Items.Clear(); // Assuming you added a ListBox named listBox1

            foreach (string file in Directory.GetFiles(recycleBinPath))
            {
                listBox1.Items.Add(Path.GetFileName(file));
            }

            foreach (string dir in Directory.GetDirectories(recycleBinPath))
            {
                listBox1.Items.Add(Path.GetFileName(dir));
            }
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
    }
}
