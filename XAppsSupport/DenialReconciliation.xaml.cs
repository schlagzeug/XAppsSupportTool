using System;
using System.Collections;
using System.Diagnostics;
using System.IO;
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
    /// Interaction logic for Denial_Reconciliation.xaml
    /// </summary>
    public partial class DenialReconciliation : Window
    {
        //======================================================================
        #region Fields
        //======================================================================

        private DateTime _dLogFromDate = StartOfDay(DateTime.Now);
        private DateTime _dLogToDate = EndOfDay(DateTime.Now);
        private string _sExceptionText = string.Empty;
        private string _sSiteID = string.Empty;
        private DirectoryInfo localLogDirectory = null;

        private string[] sExceptionKeyWordList = { "Hashtable.Insert",
                                                   "Timeout",
                                                   "SqlClient.SqlException",
                                                   "not enough space on the disk",
                                                   "exception gatekeeper",
                                                   "Default denial status has not been set",
                                                   "Server stack trace",
                                                   "DenialLineContainHcpc",
                                                   "FK_Denials_DenialStatus",
                                                   "The statement has been terminated"
                                                 };

        //======================================================================
        #endregion Fields
        //======================================================================

        //======================================================================
        #region Constructor
        //======================================================================

        public DenialReconciliation()
        {
            InitializeComponent();

            radioButton_Today.IsChecked = true;

            comboBox_Exception.Items.Add("All Exceptions");
            for (int i = 0; i < sExceptionKeyWordList.Length; i++)
            {
                comboBox_Exception.Items.Add(sExceptionKeyWordList[i]);
            }
        }

        //======================================================================
        #endregion Constructor
        //======================================================================

        //======================================================================
        #region Control Methods
        //======================================================================

        private void button_Directory_Click(object sender, RoutedEventArgs e)
        {
            string sDir = "C:\\ResendDenial\\" + textBox_SiteID.Text;

            DirectoryInfo diDir = new DirectoryInfo(sDir);
            if (!diDir.Exists)
            {
                diDir.Create();
            }

            Process.Start(sDir);
        }

        private void button_Generate_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                textBlock_Status.Text = "Working...";
                FileInfo logFile = null;

                if (radioButton_File.IsChecked == true)
                {
                    string filePath = textBox_File.Text;
                    if (filePath.StartsWith("file:"))
                    {
                        filePath = filePath.Substring(5);
                    }
                    logFile = new FileInfo(filePath);
                    if (!logFile.Exists)
                    {
                        MessageBox.Show("Invalid file path.");
                        return;
                    }
                    textBox_SiteID.Text = GetSiteId(logFile);
                }

                // check for site ID
                if (textBox_SiteID.Text == string.Empty)
                {
                    MessageBox.Show("Please enter a site ID.");
                    textBlock_Status.Text = "Waiting...";
                    return;
                }
                _sSiteID = textBox_SiteID.Text.Trim();

                localLogDirectory = new DirectoryInfo("C:\\ResendDenial\\" + _sSiteID);
                string unarchiveRequest = "<Request Enabled=\"Y\" NextCommand=\"0\" ArrivedQueue=\"DOMSVR1\\X3APPQUECL8\" ArrivedTime=\"03/25/2008 09:01:15\" NextRunTime=\"03/25/2008 09:01:15\"><Progress IsComplete=\"N\" Step=\"0\" Total=\"0\" /><Task TaskID=\"861505\" SiteID=\"" + _sSiteID + "\" LogSite=\"0\" LogUser=\"0\" Name=\"Additional ERA processing\" Schedule=\"03/25/2008 09:01:15\" Type=\"RunASAP\"><Commands><Command AbortOnFailure=\"Y\" ProgID=\"XactiMed.XApps.XClaim.ConsoleLauncherTask\" Name=\"Console Launcher\"><ConsoleLauncher FileName=\"%XMBIN%\\DenialReconciler\\DenialReconciler.exe\" Parameters=\"-MsgFile &quot;%M&quot;\" UseShell=\"true\" Timeout=\"0\" /><DenialReconciler PayerReportID=\"$$$$\" Verbose=\"N\" /></Command></Commands></Task></Request>";

                // create directory?
                if (!localLogDirectory.Exists)
                {
                    localLogDirectory.Create();
                }

                // create batch file?
                if (checkBox_GenerateBatch.IsChecked == true)
                {
                    CreateBatchFile(localLogDirectory);
                }

                // generate XML
                if (radioButton_DateRange.IsChecked == true)
                {
                    _dLogFromDate = StartOfDay(datePicker_From.SelectedDate.Value);
                    _dLogToDate = EndOfDay(datePicker_To.SelectedDate.Value);
                    if (_dLogFromDate > _dLogToDate)
                    {
                        MessageBox.Show("Please check your FROM and TO dates.");
                        return;
                    }
                    GenerateXMLByDate(unarchiveRequest);
                }
                else if (radioButton_Today.IsChecked == true)
                {
                    _dLogFromDate = StartOfDay(DateTime.Now);
                    _dLogToDate = EndOfDay(DateTime.Now);
                    GenerateXMLByDate(unarchiveRequest);
                }
                else if (radioButton_List.IsChecked == true)
                {
                    GenerateXMLFromList(unarchiveRequest);
                }
                else if (radioButton_File.IsChecked == true)
                {
                    GenerateXMLFromFile(unarchiveRequest, logFile);
                }
                else
                {
                    // this should never happen
                    MessageBox.Show("You are not a good person. You know that, right? Good people don't get up here.", "GLADOS says...");
                }

                textBlock_Status.Text = "Waiting...";
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void button_Run_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Are you sure?", "Please confirm.", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                if (textBox_SiteID.Text == string.Empty)
                {
                    MessageBox.Show("Please enter a site ID.");
                    return;
                }

                string sDir = "C:\\ResendDenial\\" + textBox_SiteID.Text;

                DirectoryInfo diDir = new DirectoryInfo(sDir);
                if (!diDir.Exists)
                {
                    MessageBox.Show("Invalid Directory.");
                    return;
                }

                string sBatch = sDir + @"\RunDenialRecon.bat";
                FileInfo fiBatch = new FileInfo(sBatch);
                if (!fiBatch.Exists)
                {
                    MessageBox.Show("No batch file.");
                    return;
                }

                string sCurrentDir = Directory.GetCurrentDirectory();
                Directory.SetCurrentDirectory(sDir);
                Process.Start(sBatch);
                Directory.SetCurrentDirectory(sCurrentDir);
            }
        }

        private void radioButton_DateRange_Checked(object sender, RoutedEventArgs e)
        {
            textBox_List.IsEnabled = false;
            datePicker_From.IsEnabled = true;
            textBlock_From.IsEnabled = true;
            datePicker_To.IsEnabled = true;
            textBlock_To.IsEnabled = true;

            comboBox_Exception.IsEnabled = true;
            textBlock_Exception.IsEnabled = true;

            textBox_File.IsEnabled = false;
        }

        private void radioButton_File_Checked(object sender, RoutedEventArgs e)
        {
            textBox_List.IsEnabled = false;
            datePicker_From.IsEnabled = false;
            textBlock_From.IsEnabled = false;
            datePicker_To.IsEnabled = false;
            textBlock_To.IsEnabled = false;

            comboBox_Exception.IsEnabled = false;
            textBlock_Exception.IsEnabled = false;

            textBox_File.IsEnabled = true;

            textBox_File.Focus();
            if (!String.IsNullOrEmpty(textBox_File.Text))
            {
                textBox_File.SelectionStart = 0;
                textBox_File.SelectionLength = textBox_File.Text.Length;
            }
        }

        private void radioButton_List_Checked(object sender, RoutedEventArgs e)
        {
            textBox_List.IsEnabled = true;
            datePicker_From.IsEnabled = false;
            textBlock_From.IsEnabled = false;
            datePicker_To.IsEnabled = false;
            textBlock_To.IsEnabled = false;

            comboBox_Exception.IsEnabled = false;
            textBlock_Exception.IsEnabled = false;

            textBox_File.IsEnabled = false;

            textBox_List.Focus();
            if (!String.IsNullOrEmpty(textBox_List.Text))
            {
                textBox_List.SelectionStart = 0;
                textBox_List.SelectionLength = textBox_List.Text.Length;
            }
        }

        private void radioButton_Today_Checked(object sender, RoutedEventArgs e)
        {
            textBox_List.IsEnabled = false;
            datePicker_From.IsEnabled = false;
            textBlock_From.IsEnabled = false;
            datePicker_To.IsEnabled = false;
            textBlock_To.IsEnabled = false;

            comboBox_Exception.IsEnabled = true;
            textBlock_Exception.IsEnabled = true;

            textBox_File.IsEnabled = false;
        }

        //======================================================================
        #endregion Control Methods
        //======================================================================

        //======================================================================
        #region Other Methods
        //======================================================================

        private static DateTime EndOfDay(DateTime date)
        {
            return new DateTime(date.Year, date.Month, date.Day, 23, 59, 59, 999);
        }

        private static DateTime StartOfDay(DateTime date)
        {
            return new DateTime(date.Year, date.Month, date.Day, 0, 0, 0, 0);
        }

        private void CreateBatchFile(DirectoryInfo localLogDirectory)
        {
            string batchFileText = @"c:\XactiMed\bin\SendUnarchiveMessage\SendUnarchiveMessage   DenialReconciler.xml" + "\nPAUSE";
            StreamWriter localFileWiter = new StreamWriter(localLogDirectory + "\\RunDenialRecon.bat");
            localFileWiter.Write(batchFileText);
            localFileWiter.Flush();
            localFileWiter.Close();
        }

        private void GenerateXMLByDate(string unarchiveRequest)
        {
            if (comboBox_Exception.SelectedIndex < 0)
            {
                MessageBox.Show("Please select an Exception Key.");
                textBlock_Status.Text = "Waiting...";
                return;
            }

            ArrayList payerReportIds = GetPayerReportIds();
            GenerateXMLFromList(unarchiveRequest, payerReportIds);
        }

        private void GenerateXMLFromFile(string unarchiveRequest, FileInfo file)
        {
            string reportId = GetPayerReportId(file);
            ArrayList listOfIds = new ArrayList();
            listOfIds.Add(reportId);
            GenerateXMLFromList(unarchiveRequest, listOfIds);
        }

        private void GenerateXMLFromList(string unarchiveRequest, ArrayList listOfIDs)
        {
            if ((listOfIDs.Count == 1 && (string)listOfIDs[0] == string.Empty) || listOfIDs.Count == 0)
            {
                MessageBox.Show("No Payer Report ID found.", "Error.");
                return;
            }
            string outputString = "The XML file has been created from the following Payer Reports:\n";
            StreamWriter localFileWiter = new StreamWriter(localLogDirectory + "\\DenialReconciler.xml");
            localFileWiter.WriteLine("<Requests>");
            foreach (string id in listOfIDs)
            {
                if (id != string.Empty)
                {
                    outputString += (id + " ");
                    localFileWiter.WriteLine(unarchiveRequest.Replace("$$$$", id));
                    localFileWiter.Flush();
                    textBlock_Status.Text = string.Format("Writing payer report ID: {0}", id);
                }
            }
            localFileWiter.WriteLine("</Requests>");
            localFileWiter.Close();
            textBlock_Status.Text = "Finished!!";

            outputString += string.Format("\nNumber of Payer Report IDs: {0}", listOfIDs.Count);
            MessageBox.Show(outputString);
        }

        private void GenerateXMLFromList(string unarchiveRequest)
        {
            char[] caSeperators = { ',', ';' };
            string sList = textBox_List.Text;
            ArrayList list = new ArrayList(sList.Split(caSeperators, StringSplitOptions.RemoveEmptyEntries));
            GenerateXMLFromList(unarchiveRequest, list);
        }

        private string GetPayerReportId(FileInfo file)
        {
            string sPayerReportID = string.Empty;
            string sCurrentPayerReportID = string.Empty;
            try
            {
                StreamReader reader = new StreamReader(file.FullName);
                string line = reader.ReadLine();
                while (line != null)
                {
                    if (line.Contains("Payer Report:"))
                    {
                        sCurrentPayerReportID = line.Substring(14, line.Length - 14).Trim();
                        break;
                    }
                    line = reader.ReadLine();
                }

                string denialLogText = reader.ReadToEnd();
                reader.Close();

                if (((_sExceptionText == "All Exceptions" || comboBox_Exception.IsEnabled == false) && hasExceptionCode(denialLogText)) || denialLogText.Contains(_sExceptionText))
                {
                    sPayerReportID = sCurrentPayerReportID;
                    file.CopyTo("C:\\ResendDenial\\" + _sSiteID + "\\" + file.Name, true);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }

            return sPayerReportID;
        }

        private ArrayList GetPayerReportIds()
        {
            ArrayList payerReportIds = new ArrayList();

            _sExceptionText = comboBox_Exception.SelectedItem.ToString();
            DirectoryInfo serverLogDirectory = new DirectoryInfo(Tools.GetLogLocation(int.Parse(_sSiteID)) + @"\Logs\DenialReconciler");
            if (!serverLogDirectory.Exists)
            {
                // alternate location
                serverLogDirectory = new DirectoryInfo(Tools.GetSiteLocaion(int.Parse(_sSiteID)) + @"\XDenial\Reports");
            }

            if (!serverLogDirectory.Exists)
            {
                // don't know where the files are
                MessageBox.Show("Server log directory is unavailable.", "Error");
            }
            else
            {
                FileInfo[] logFileList = serverLogDirectory.GetFiles("DenialReconciler.*");
                string sPayerReportID = string.Empty;
                foreach (FileInfo file in logFileList)
                {
                    if (file.CreationTime >= _dLogFromDate && file.CreationTime <= _dLogToDate)
                    {
                        sPayerReportID = GetPayerReportId(file);
                        if (sPayerReportID != string.Empty)
                        {
                            payerReportIds.Add(sPayerReportID);
                        }
                    }
                }
            }

            // remove the duplicates
            for (int i = 0; i < payerReportIds.Count; i++)
            {
                for (int j = payerReportIds.Count - 1; j > i; j--)
                {
                    if (payerReportIds[i].ToString() == payerReportIds[j].ToString())
                    {
                        payerReportIds.RemoveAt(j);
                    }
                }
            }

            return payerReportIds;
        }

        private string GetSiteId(FileInfo file)
        {
            string siteID = string.Empty;
            try
            {
                StreamReader reader = new StreamReader(file.FullName);
                string line = reader.ReadLine();
                while (line != null)
                {
                    if (line.Contains("Site:"))
                    {
                        siteID = line.Substring(14, line.Length - 14).Trim();
                        break;
                    }
                    line = reader.ReadLine();
                }

                reader.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
            return siteID;
        }

        private bool hasExceptionCode(string denialLogText)
        {
            if (denialLogText.Contains("Exception:"))
                return true;

            for (int i = 0; i < sExceptionKeyWordList.Length; i++)
            {
                if (denialLogText.Contains(sExceptionKeyWordList[i]))
                    return true;
            }

            return false;
        }

        //======================================================================
        #endregion Other Methods
        //======================================================================
    }
}
