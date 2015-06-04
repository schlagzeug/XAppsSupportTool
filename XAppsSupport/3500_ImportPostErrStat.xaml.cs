using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace XAppsSupport
{
    /// <summary>
    /// Interaction logic for _3500_ImportPostErrStat.xaml
    /// </summary>
    public partial class _3500_ImportPostErrStat : Window
    {
        string fileLocation = @"C:\CustomerSS\3500\ImportPostErrStat";
        public _3500_ImportPostErrStat()
        {
            InitializeComponent();
            if (!Directory.Exists(fileLocation))
            {
                Directory.CreateDirectory(fileLocation);
            }
            datePicker1.SelectedDate = DateTime.Today;
        }

        private void button_Find_Click(object sender, RoutedEventArgs e)
        {
            string logLocation = System.IO.Path.Combine(Tools.GetLogLocation(3500), @"Logs\ImportPostErrStat");
            DirectoryInfo diLog = new DirectoryInfo(logLocation);
            FileInfo[] files = diLog.GetFiles("ImportPostErrStat.*");
            DateTime endOfDay = Tools.EndOfDay(datePicker1.SelectedDate.Value);
            DateTime beginOfDay = Tools.StartOfDay(datePicker1.SelectedDate.Value);
            List<string> successfulImports = new List<string>();
            foreach (var file in files)
            {
                if (file.CreationTime < endOfDay && file.CreationTime > beginOfDay)
                {
                    successfulImports.Add(GetImportIDFromFile(file));
                }
            }

            // find the import IDs for the selected day
            ArrayList imports = Tools.GetImportsByDate(3500, beginOfDay);
            
            // find which ones need to be re-ran
            string message = string.Empty; // testing
            List<string> importsToReRun = new List<string>();
            foreach (var import in imports)
            {
                if (!successfulImports.Contains(import.ToString()))
                {
                    importsToReRun.Add(import.ToString());
                    message = message + " " + import; // testing
                }
            }

            if (importsToReRun.Count <= 0)
            {
                Tools.ShowMessage("No Reports to re-run.");
                return;
            }

            // generate files
            GenerateFiles(importsToReRun);
            Tools.ShowMessage("Files created.");
            Tools.ShowMessage(message); // testing
        }

        private void GenerateFiles(List<string> importsToReRun)
        {
            // make directory
            if (!Directory.Exists(fileLocation))
                Directory.CreateDirectory(fileLocation);

            // create batch file
            string batFileText = string.Empty;
            foreach (var importID in importsToReRun)
            {
                batFileText += string.Format(@"C:\xactimed\bin\Xclaim.Post\ImportPostErrStat\ImportPostErrStat.exe {0}Report.xml 3500", importID);
                batFileText += "\n";
            }
            batFileText += "PAUSE";

            var localFileWiter = new StreamWriter(fileLocation + @"\Run.bat");
            localFileWiter.Write(batFileText);
            localFileWiter.Flush();
            localFileWiter.Close();

            // Get the import report(s)
            foreach (var importID in importsToReRun)
            {
                Tools.SaveImportReport(3500, importID, fileLocation);
            }
        }

        private string GetImportIDFromFile(FileInfo file)
        {
            string importID = string.Empty;

            try
            {
                StreamReader reader = new StreamReader(file.FullName);
                string line = reader.ReadLine();
                while (line != null)
                {
                    if (line.Contains("Process ImportID:"))
                    {
                        importID = line.Substring(18);
                        for (int i = 1; i < importID.Length; i++)
                        {
                            try
                            {
                                int.Parse(importID.Substring(0, i));
                            }
                            catch
                            {
                                importID = importID.Substring(0, i - 1).Trim();
                                break;
                            }
                        }
                        break;
                    }
                    line = reader.ReadLine();
                }

                string denialLogText = reader.ReadToEnd();
                reader.Close();
            }
            catch (Exception ex)
            {
                Tools.ShowError(ex.ToString());
            }

            return importID;
        }

        private void button_Show_Click(object sender, RoutedEventArgs e)
        {
            Tools.OpenDirectory(fileLocation);
        }
    }
}
