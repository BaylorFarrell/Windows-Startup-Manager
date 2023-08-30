using Microsoft.Win32;
using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace Windows_Startup_Manager
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            listView1.SmallImageList = imageList1;
            imageList1.ImageSize = new Size(16, 16);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            LoadStartupItems();
            MessageBox.Show("This program may alter the registry", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            listView1.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
        }

        private void LoadStartupItems()
        {
            listView1.Items.Clear();
            imageList1.Images.Clear();

            using (RegistryKey rkApp = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", false))
            {
                if (rkApp != null)
                {
                    foreach (string appName in rkApp.GetValueNames())
                    {
                        string appPath = rkApp.GetValue(appName)?.ToString() ?? "";
                        AddStartupItemToList(appName, appPath);
                    }
                }
            }

            string startupFolder = Environment.GetFolderPath(Environment.SpecialFolder.Startup);
            foreach (string filePath in Directory.GetFiles(startupFolder))
            {
                string fileName = Path.GetFileNameWithoutExtension(filePath);
                AddStartupItemToList(fileName, filePath);
            }
        }

        private void AddStartupItemToList(string appName, string appPath)
        {
            try
            {
                ListViewItem item = new ListViewItem(appName);
                item.SubItems.Add(appPath);

                if (File.Exists(appPath))
                {
                    Icon appIcon = Icon.ExtractAssociatedIcon(appPath);
                    item.ImageIndex = imageList1.Images.Count;
                    imageList1.Images.Add(appIcon);
                }

                listView1.Items.Add(item);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "Executable Files|*.exe";
                openFileDialog.Title = "Select an application";

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    string filePath = openFileDialog.FileName;
                    string fileName = Path.GetFileNameWithoutExtension(filePath);

                    using (RegistryKey rkApp = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true))
                    {
                        if (rkApp != null)
                        {
                            rkApp.SetValue(fileName, filePath);
                        }
                    }

                    LoadStartupItems();
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count > 0)
            {
                try
                {
                    string appName = listView1.SelectedItems[0].Text;
                    string appPath = listView1.SelectedItems[0].SubItems[1].Text;

                    using (RegistryKey rkApp = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true))
                    {
                        if (rkApp != null && rkApp.GetValueNames().Contains(appName))
                        {
                            rkApp.DeleteValue(appName, false);
                        }
                        else
                        {
                            string startupFolder = Environment.GetFolderPath(Environment.SpecialFolder.Startup);
                            string startupFilePath = Path.Combine(startupFolder, $"{appName}.lnk");

                            if (File.Exists(startupFilePath))
                            {
                                File.Delete(startupFilePath);
                            }
                        }
                    }

                    LoadStartupItems();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
            else
            {
                MessageBox.Show("Please select an item to remove.");
            }
        }
    }
}
