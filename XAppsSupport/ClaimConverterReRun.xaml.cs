using System;
using System.Collections.Generic;
using System.Collections;
using System.Diagnostics;
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
using System.IO;
using System.Xml;

using XactiMed;

namespace XAppsSupport
{
    /// <summary>
    /// Interaction logic for ClaimConverterReRun.xaml
    /// </summary>
    public partial class ClaimConverterReRun : Window
    {
        private DateTime startDate = DateTime.MinValue;
        private DateTime endDate = DateTime.MinValue;
        private List<int> exportIDs = new List<int>();
        private List<int> claimIDs = new List<int>();
        private string testFlag = string.Empty;
        private string batchFlag = string.Empty;

        public ClaimConverterReRun()
        {
            InitializeComponent();
            PopulateFields();
        }

        public ClaimConverterReRun(string siteID)
        {
            InitializeComponent();
            textBox_SiteID.Text = siteID;
            PopulateFields();
        }

        public void PopulateFields()
        {
            datePicker_Date.SelectedDate = DateTime.Today;
            datePicker_DateFrom.SelectedDate = DateTime.Today;
            datePicker_DateTo.SelectedDate = DateTime.Today;

            radioButton_Date.IsChecked = true;
            radioButton_Server.IsChecked = true;
        }

        private void button_ShowFiles_Click(object sender, RoutedEventArgs e)
        {
            dataGrid_ConverterFiles.ItemsSource = null;
            if (SiteID != 0)
            {
                string siteLocation = string.Empty;
                if (radioButton_Server.IsChecked == true)
                {
                    siteLocation = Tools.GetSiteLocaion(SiteID);
                }
                else
                {
                    siteLocation = @"C:\CustomerSS\" + SiteID;
                }

                if (Directory.Exists(siteLocation))
                {
                    string cc = siteLocation + @"\XClaim\ClaimConverter.xml";

                    if (File.Exists(cc))
                    {
                        List<ClaimConverterData> data = GetClaimConverterData(cc);
                        dataGrid_ConverterFiles.ItemsSource = data;
                    }
                    else
                    {
                        Tools.ShowError("Site has no ClaimConverter.xml.");
                    }
                }
                else
                {
                    Tools.ShowError("Site doesn't exist.");
                }
            }
            else
            {
                Tools.ShowError("Site ID formatted incorrectly.");
            }
        }

        private List<ClaimConverterData> GetClaimConverterData(string cc)
        {
            List<ClaimConverterData> data = new List<ClaimConverterData>();

            XmlDocument doc = new XmlDocument();
            doc.Load(cc);
            XmlNodeList files = doc.SelectNodes("FILES/FILE");
            foreach (XmlNode file in files)
            {
                ClaimConverterData d = new ClaimConverterData();
                d.AnsiID = file.Attributes.GetNamedItem("ID").Value;
                d.FileName = file.SelectSingleNode("FILEPATHS").Attributes.GetNamedItem("FILENAME").Value;
                d.Type = file.Attributes.GetNamedItem("TYPE").Value;
                foreach (XmlNode filePath in file.SelectNodes("FILEPATHS/FILEPATH"))
                {
                    d.Directories += EzXml.GetStringEz(filePath, string.Empty) + "\r\n";
                }
                if (d.Directories.EndsWith("\r\n"))
                    d.Directories = d.Directories.TrimEnd('\n').TrimEnd('\r');
                data.Add(d);
            }
            return data;
        }

        private void radioButton_Server_Checked(object sender, RoutedEventArgs e)
        {
            dataGrid_ConverterFiles.ItemsSource = null;
        }

        private void radioButton_Local_Checked(object sender, RoutedEventArgs e)
        {
            dataGrid_ConverterFiles.ItemsSource = null;
        }

        private void radioButton_Date_Checked(object sender, RoutedEventArgs e)
        {
            startDate = datePicker_Date.SelectedDate.Value;
            endDate = datePicker_Date.SelectedDate.Value;
        }

        private void radioButton_DateRange_Checked(object sender, RoutedEventArgs e)
        {
            startDate = datePicker_DateFrom.SelectedDate.Value;
            endDate = datePicker_DateTo.SelectedDate.Value;
        }

        private void radioButton_ExportID_Checked(object sender, RoutedEventArgs e)
        {

        }

        public int SiteID 
        { 
            get 
            {
                int siteID = 0;
                int.TryParse(textBox_SiteID.Text, out siteID);
                return siteID;
            } 
        }

        private void button_CreateBatchFile_Click(object sender, RoutedEventArgs e)
        {
            if (checkBox_Testing.IsChecked == true)
                testFlag = " -Test";
            else
                testFlag = string.Empty;

            if (checkBox_Batch.IsChecked == true)
                batchFlag = " -Batch";
            else
                batchFlag = string.Empty;

            string location = string.Format(@"C:\CustomerSS\{0}\XClaim", SiteID);
            if (!Directory.Exists(location))
                Directory.CreateDirectory(location);

            var localFileWriter = new StreamWriter(location + @"\ClaimConverter.bat");
            localFileWriter.Write(GenerateBatchFileText(GetSelectedAnsiIDs()));
            localFileWriter.Flush();
            localFileWriter.Close();

            Tools.ShowMessage("Batch file created.");
            Process.Start(@"notepad.exe", location + @"\ClaimConverter.bat");
        }

        private string GenerateBatchFileText(List<string> ansiIDs)
        {
            string returnVal = string.Empty;
            SetDates();
            if (radioButton_Date.IsChecked == true || (radioButton_DateRange.IsChecked == true && checkBox_OneFileForDates.IsChecked == false))
            {
                List<DateTime> dates = GetDates();
                foreach (var ansiID in ansiIDs)
                {
                    foreach (var date in dates)
                    {
                        string line = string.Format(@"C:\XactiMed\bin\XClaim.Post\ClaimConverterUtility\ClaimConverterUtility -SiteID {0} -AnsiID {1} -Date {2}{3}{4}", SiteID, ansiID, date.ToString(@"MM/dd/yyyy"), batchFlag, testFlag);
                        returnVal += line;
                        returnVal += "\r\n";
                    }
                }
            }
            else if (radioButton_DateRange.IsChecked == true && checkBox_OneFileForDates.IsChecked == true)
            {
                foreach (var ansiID in ansiIDs)
                {
                    string line = string.Format(@"C:\XactiMed\bin\XClaim.Post\ClaimConverterUtility\ClaimConverterUtility -SiteID {0} -AnsiID {1} -FromDate {2} -ThruDate {3}{4}{5}", SiteID, ansiID, startDate.ToString(@"MM/dd/yyyy"), endDate.ToString(@"MM/dd/yyyy"), batchFlag, testFlag);
                    returnVal += line;
                    returnVal += "\r\n";
                }
            }
            else if (radioButton_ExportID.IsChecked == true)
            {
                GetExportIDs();
                foreach (var ansiID in ansiIDs)
                {
                    foreach (var exportID in exportIDs)
                    {
                        string line = string.Format(@"C:\XactiMed\bin\XClaim.Post\ClaimConverterUtility\ClaimConverterUtility -SiteID {0} -AnsiID {1} -ExportID {2}{3}{4}", SiteID, ansiID, exportID, batchFlag, testFlag);
                        returnVal += line;
                        returnVal += "\r\n";
                    }
                }
            }
            else if (radioButton_ClaimID.IsChecked == true)
            {
                GetClaimIDs();
                if (checkBox_OneFileForClaims.IsChecked == true)
                {
                    string clx = GenerateClaimCLX();
                    foreach (var ansiID in ansiIDs)
                    {
                        string line = string.Format("C:\\XactiMed\\bin\\XClaim.Post\\ClaimConverterUtility\\ClaimConverterUtility -SiteID {0} -AnsiID {1} -CLX \"{2}\"{3}{4}", SiteID, ansiID, clx, batchFlag, testFlag);
                        returnVal += line;
                        returnVal += "\r\n";
                    }
                }
                else
                {
                    foreach (var ansiID in ansiIDs)
                    {
                        foreach (var claimID in claimIDs)
                        {
                            string line = string.Format(@"C:\XactiMed\bin\XClaim.Post\ClaimConverterUtility\ClaimConverterUtility -SiteID {0} -AnsiID {1} -ClaimID {2}{3}{4}", SiteID, ansiID, claimID, batchFlag, testFlag);
                            returnVal += line;
                            returnVal += "\r\n";
                        }
                    }
                }
            }
            returnVal += "\r\n" + "PAUSE";
            return returnVal;
        }

        private string GenerateClaimCLX()
        {
            string clx = "<CLX><OR>";
            foreach (var claimID in claimIDs)
            {
                clx += string.Format("<ClaimID>{0}</ClaimID>", claimID.ToString());
            }
            clx += "</OR></CLX>";
            return clx;
        }

        private void GetClaimIDs()
        {
            claimIDs.Clear();
            string[] claimIDsRaw = textBox_ClaimID.Text.Split(new char[] { ',', ';' });
            foreach (var rawClaimID in claimIDsRaw)
            {
                int x = 0;
                if (int.TryParse(rawClaimID, out x))
                {
                    claimIDs.Add(x);
                }
            }
        }

        private void GetExportIDs()
        {
            exportIDs.Clear();
            string[] exportIDsRaw = textBox_ExportID.Text.Split(new char[] { ',', ';' });
            foreach (var rawExportID in exportIDsRaw)
            {
                int x = 0;
                if (int.TryParse(rawExportID, out x))
                {
                    exportIDs.Add(x);
                }
            }
        }

        private void SetDates()
        {
            if (radioButton_Date.IsChecked == true)
            {
                startDate = datePicker_Date.SelectedDate.Value;
                endDate = datePicker_Date.SelectedDate.Value;
            }
            else if (radioButton_DateRange.IsChecked == true)
            {
                startDate = datePicker_DateFrom.SelectedDate.Value;
                endDate = datePicker_DateTo.SelectedDate.Value;
            }
        }

        private List<DateTime> GetDates()
        {
            List<DateTime> dates = new List<DateTime>();
            for (DateTime dt = startDate; dt <= endDate; dt = dt.AddDays(1))
            {
                dates.Add(dt);
            }
            return dates;
        }

        private List<string> GetSelectedAnsiIDs()
        {
            List<string> ansiIDs = new List<string>();
            foreach (ClaimConverterData cell in dataGrid_ConverterFiles.SelectedItems)
            {  
                ansiIDs.Add(cell.AnsiID);
            }
            return ansiIDs;
        }

        private void button_OpenFolder_Click(object sender, RoutedEventArgs e)
        {
            string location = string.Format(@"C:\CustomerSS\{0}\XClaim", SiteID);
            Tools.OpenDirectory(location);
        }

        private void button_RunBatchFile_Click(object sender, RoutedEventArgs e)
        {
            Tools.OpenFile(string.Format(@"C:\CustomerSS\{0}\XClaim\ClaimConverter.bat", SiteID));
        }

        private void button_OpenClaimConverterXML_Click(object sender, RoutedEventArgs e)
        {
            if (SiteID != 0)
            {
                string siteLocation = string.Empty;
                if (radioButton_Server.IsChecked == true)
                {
                    siteLocation = Tools.GetSiteLocaion(SiteID);
                }
                else
                {
                    siteLocation = @"C:\CustomerSS\" + SiteID;
                }

                if (Directory.Exists(siteLocation))
                {
                    string cc = siteLocation + @"\XClaim\ClaimConverter.xml";

                    if (File.Exists(cc))
                    {
                        Tools.OpenFile(cc);
                    }
                    else
                    {
                        Tools.ShowError("Site has no ClaimConverter.xml.");
                    }
                }
                else
                {
                    Tools.ShowError("Site doesn't exist.");
                }
            }
            else
            {
                Tools.ShowError("Site ID formatted incorrectly.");
            }
        }
    }

    public class ClaimConverterData
    {
        public string AnsiID { get; set; }
        public string FileName { get; set; }
        public string Type { get; set; }
        public string Directories { get; set; }
    }
}
