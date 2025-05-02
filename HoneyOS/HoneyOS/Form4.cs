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

        private void delete_Click(object sender, EventArgs e)
        {
            if (listBox1.SelectedItem == null)
            {
                MessageBox.Show("Please select an item to delete permanently.", "No Selection", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string selectedItem = listBox1.SelectedItem.ToString();
            string recycleBinPath = Path.Combine(Application.StartupPath, "RecycleBin");
            string fullPath = Path.Combine(recycleBinPath, selectedItem);

            try
            {
                // Check if it's a file or a directory and delete accordingly
                if (File.Exists(fullPath))
                {
                    File.Delete(fullPath);
                }
                else if (Directory.Exists(fullPath))
                {
                    Directory.Delete(fullPath, true); // true to delete contents
                }

                MessageBox.Show("Item permanently deleted.", "Deleted", MessageBoxButtons.OK, MessageBoxIcon.Information);

                // Refresh list
                LoadRecycleBinItems();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error deleting item: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

    }
}
