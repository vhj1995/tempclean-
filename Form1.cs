using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace TempClean__
{
    public partial class Form1 : Form
    {
        public static string tmpPath = String.Empty;
        public static List<string> files = new List<string>();
        public static DirectoryInfo folder;
        public static DirectoryInfo folder2;
        public static DirectoryInfo folder3;
        public Form1()
        {
            InitializeComponent();
            Scan();
        }
        static long folderSize(DirectoryInfo folder)
        {
            long totalSizeOfDir = 0;

            // Get all files into the directory
            FileInfo[] allFiles = folder.GetFiles();

            // Loop through every file and get size of it
            foreach (FileInfo file in allFiles)
            {
                totalSizeOfDir += file.Length;
            }

            // Find all subdirectories
            DirectoryInfo[] subFolders = folder.GetDirectories();

            // Loop through every subdirectory and get size of each
            foreach (DirectoryInfo dir in subFolders)
            {
                totalSizeOfDir += folderSize(dir);
            }

            // Return the total size of folder
            return totalSizeOfDir;
        }
        private void ThreadSafe(MethodInvoker method)
        {
            try
            {
                if (InvokeRequired)
                    Invoke(method);
                else
                    method();
            }
            catch (ObjectDisposedException) { }
        }
        public void Scan()
        {
            tmpPath = Path.GetTempPath();
            files.Clear();
            files.AddRange(Directory.GetFiles(tmpPath, "*.*", SearchOption.AllDirectories));
            files.AddRange(Directory.GetFiles("C:\\Windows\\Temp", "*.*", SearchOption.AllDirectories));
            //files.AddRange(Directory.GetFiles(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)+ "\\Temp","*.*", SearchOption.AllDirectories));

            folder = new DirectoryInfo(tmpPath);
            folder2 = new DirectoryInfo("C:\\Windows\\Temp");
            //folder3 = new DirectoryInfo(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\Temp");

            // Calling a folderSize() method
            long totalFolderSize = folderSize(folder);
            totalFolderSize += folderSize(folder2);
            //totalFolderSize += folderSize(folder3);

            label1.Text = "Total Size of Files to Clean : " + FileSizeFormatter.FormatSize(totalFolderSize);
        }
        private void btnScan_Click(object sender, EventArgs e)
        {
            Scan();
        }
        public static class FileSizeFormatter
        {
            // Load all suffixes in an array  
            static readonly string[] suffixes =
            { " Bytes", " KB", " MB", " GB", " TB", " PB" };
            public static string FormatSize(Int64 bytes)
            {
                int counter = 0;
                decimal number = (decimal)bytes;
                while (Math.Round(number / 1024) >= 1)
                {
                    number = number / 1024;
                    counter++;
                }
                return string.Format("{0:n1}{1}", number, suffixes[counter]);
            }
        }
        private void btnClean_Click(object sender, EventArgs e)
        {
            progressBar1.Show();
            backgroundWorker1.RunWorkerAsync();
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                int calcPercentage = 0;
                for (int i = 0; i < files.Count(); i++)
                {
                    calcPercentage = (100 * i) / files.Count();
                    if (File.Exists(files[i]))
                    {
                        try
                        {
                            File.Delete(files[i]);
                            ThreadSafe(() => label1.Text = "Deleting... " + Path.GetFileName(files[i]));
                        }
                        catch { }

                        backgroundWorker1.ReportProgress(calcPercentage);
                    }
                }

                //Delete Folders
                foreach (DirectoryInfo dir in folder.GetDirectories())
                {
                    try
                    {
                        dir.Delete(true);
                        Directory.Delete(dir.FullName);
                    }
                    catch
                    {

                    }
                }
                foreach (DirectoryInfo dir in folder2.GetDirectories())
                {
                    try
                    {
                        dir.Delete(true);
                        Directory.Delete(dir.FullName);
                    }
                    catch
                    {

                    }
                }
                //foreach (DirectoryInfo dir in folder3.GetDirectories())
                //{
                //    dir.Delete(true);
                //}
            }
            catch
            {
                //backgroundWorker1.ReportProgress(calcPercentage);
            }
        }

        private void backgroundWorker1_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            progressBar1.Value = e.ProgressPercentage;
        }

        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            progressBar1.Value = 0;
            progressBar1.Hide();
            //MessageBox.Show("Done");
            label1.Text = "Done!";
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            progressBar1.Hide();
        }
    }
}
