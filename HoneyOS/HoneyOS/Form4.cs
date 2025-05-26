using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO; // Make sure this is present

namespace HoneyOS
{
    public partial class Form4 : Form
    {
        private Dictionary<string, string> originalPaths = new Dictionary<string, string>();
        private string recycleBinPath; // Declare recycleBinPath here to be accessible

        public Form4()
        {
            InitializeComponent();
            // Initialize recycleBinPath here or in Form4_Load
            recycleBinPath = Path.Combine(Application.StartupPath, "RecycleBin");
        }

        // --- NEW: Form Load Event Handler ---
        private void Form4_Load(object sender, EventArgs e)
        {
            // Ensure the recycle bin directory exists when the form loads
            if (!Directory.Exists(recycleBinPath))
            {
                Directory.CreateDirectory(recycleBinPath);
            }

            // Load items when the form loads
            LoadRecycleBinItems();
            UpdateRecycleBinCountLabel(); // Call this to update the label initially
        }


        private void LoadRecycleBinItems()
        {
            // Ensure the directory exists before trying to access it
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
                if (Path.GetExtension(file).Equals(".metadata", StringComparison.OrdinalIgnoreCase))
                {
                    continue; // Skip the metadata files themselves
                }

                string fileName = Path.GetFileName(file);
                listBox1.Items.Add(fileName);

                // Read metadata
                string metadataPath = Path.Combine(recycleBinPath, fileName + ".metadata");
                if (File.Exists(metadataPath))
                {
                    originalPaths[fileName] = File.ReadAllText(metadataPath);
                }
                else
                {
                    // Handle cases where metadata might be missing (e.g., if files were moved manually)
                    originalPaths[fileName] = "Unknown Location"; // Assign a default or handle as needed
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
                else
                {
                    originalPaths[dirName] = "Unknown Location"; // Assign a default
                }
            }
            UpdateRecycleBinCountLabel(); // Update count after loading
        }

        private void UpdateRecycleBinCountLabel()
        {
            lblRecycleBinItems.Text = $"Recycle Bin ({listBox1.Items.Count} items)";
        }


        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            // You can add logic here to enable/disable buttons based on selection
            // For example:
            bool itemSelected = listBox1.SelectedItem != null;
            btnDelete.Enabled = itemSelected;
            btnRestore.Enabled = itemSelected;
        }

        private void delete_Click(object sender, EventArgs e)
        {
            if (listBox1.SelectedItem == null)
            {
                MessageBox.Show("Please select an item to delete permanently.", "No Selection", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string selectedItem = listBox1.SelectedItem.ToString();
            string fullPath = Path.Combine(recycleBinPath, selectedItem);
            string metadataPath = Path.Combine(recycleBinPath, selectedItem + ".metadata");

            if (MessageBox.Show($"Are you sure you want to permanently delete '{selectedItem}'?", "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
            {
                return; // User cancelled
            }

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

                // Delete the associated metadata file
                if (File.Exists(metadataPath))
                {
                    File.Delete(metadataPath);
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
            string fullPathInRecycleBin = Path.Combine(recycleBinPath, selectedItem);

            if (!originalPaths.ContainsKey(selectedItem))
            {
                MessageBox.Show("Original path information is missing. Cannot restore.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            string originalPath = originalPaths[selectedItem];

            try
            {
                // Create the target directory if it doesn't exist
                string destinationDirectory = Path.GetDirectoryName(originalPath);
                if (!Directory.Exists(destinationDirectory))
                {
                    Directory.CreateDirectory(destinationDirectory);
                }

                // Check if original location already has something with this name
                if (File.Exists(originalPath) || Directory.Exists(originalPath))
                {
                    // Offer to overwrite or rename? For now, we'll prevent restore to avoid data loss.
                    MessageBox.Show($"The original location '{originalPath}' already contains an item with this name. Please move or rename the existing item before restoring.", "Conflict", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // Move the item back to its original location
                if (File.Exists(fullPathInRecycleBin))
                {
                    File.Move(fullPathInRecycleBin, originalPath);
                    File.Delete(Path.Combine(recycleBinPath, selectedItem + ".metadata")); // Delete metadata
                }
                else if (Directory.Exists(fullPathInRecycleBin))
                {
                    Directory.Move(fullPathInRecycleBin, originalPath);
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

        // --- NEW: Empty Recycle Bin Event Handler ---
        private void btnClearAll_Click(object sender, EventArgs e)
        {
            if (listBox1.Items.Count == 0)
            {
                MessageBox.Show("Recycle Bin is already empty.", "Empty", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (MessageBox.Show("Are you sure you want to permanently delete ALL items in the Recycle Bin?", "Empty Recycle Bin", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.No)
            {
                return; // User cancelled
            }

            try
            {
                // Delete all files and directories within the recycle bin folder
                foreach (string file in Directory.GetFiles(recycleBinPath))
                {
                    File.Delete(file);
                }
                foreach (string dir in Directory.GetDirectories(recycleBinPath))
                {
                    Directory.Delete(dir, true); // true to delete contents
                }

                MessageBox.Show("Recycle Bin emptied successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                LoadRecycleBinItems(); // Refresh the list after emptying
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error emptying Recycle Bin: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}