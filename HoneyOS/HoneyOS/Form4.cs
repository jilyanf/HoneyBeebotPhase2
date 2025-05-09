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
        private Dictionary<string, string> originalPaths = new Dictionary<string, string>();
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

            listBox1.Items.Clear();
            originalPaths.Clear();

            // Load files
            foreach (string file in Directory.GetFiles(recycleBinPath))
            {
                // Skip metadata files
                if (Path.GetExtension(file) == ".metadata") continue;

                string fileName = Path.GetFileName(file);
                listBox1.Items.Add(fileName);

                // Read metadata
                string metadataPath = Path.Combine(recycleBinPath, fileName + ".metadata");
                if (File.Exists(metadataPath))
                {
                    originalPaths[fileName] = File.ReadAllText(metadataPath);
                }
            }

            // Load directories
            foreach (string dir in Directory.GetDirectories(recycleBinPath))
            {
                string dirName = Path.GetFileName(dir);
                listBox1.Items.Add(dirName);

                // Read metadata
                string metadataPath = Path.Combine(recycleBinPath, dirName + ".metadata");
                if (File.Exists(metadataPath))
                {
                    originalPaths[dirName] = File.ReadAllText(metadataPath);
                }
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

        private void restore_Click(object sender, EventArgs e)
        {
            if (listBox1.SelectedItem == null)
            {
                MessageBox.Show("Please select an item to restore.", "No Selection", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string selectedItem = listBox1.SelectedItem.ToString();
            string recycleBinPath = Path.Combine(Application.StartupPath, "RecycleBin");
            string fullPath = Path.Combine(recycleBinPath, selectedItem);

            if (!originalPaths.ContainsKey(selectedItem))
            {
                MessageBox.Show("Original path information is missing. Cannot restore.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            string originalPath = originalPaths[selectedItem];

            try
            {
                // Check if original location already has something with this name
                if (File.Exists(originalPath) || Directory.Exists(originalPath))
                {
                    MessageBox.Show("Original location already contains an item with this name. Cannot restore.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // Move the item back to its original location
                if (File.Exists(fullPath))
                {
                    File.Move(fullPath, originalPath);
                    File.Delete(Path.Combine(recycleBinPath, selectedItem + ".metadata")); // Delete metadata
                }
                else if (Directory.Exists(fullPath))
                {
                    Directory.Move(fullPath, originalPath);
                    File.Delete(Path.Combine(recycleBinPath, selectedItem + ".metadata")); // Delete metadata
                }

                MessageBox.Show("Item restored to its original location.", "Restored", MessageBoxButtons.OK, MessageBoxIcon.Information);

                // Refresh list
                LoadRecycleBinItems();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error restoring item: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
