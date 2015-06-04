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
    /// Interaction logic for DenialReconMulti.xaml
    /// </summary>
    public partial class DenialReconMulti : Window
    {
        //======================================================================
        #region Fields
        //======================================================================

        private const string SLocalLogDirectory = @"C:\ResendDenial\Multi";
        private DateTime _dLogFromDate = StartOfDay(DateTime.Now);
        private DateTime _dLogToDate = EndOfDay(DateTime.Now);
        private string _sStatusMessage = string.Empty;

        //======================================================================
        #endregion Fields
        //======================================================================

        //======================================================================
        #region Constructor
        //======================================================================

        public DenialReconMulti()
        {
            InitializeComponent();

            radioButton_Today.IsChecked = true;
            datePicker_From.IsEnabled = false;
            datePicker_To.IsEnabled = false;

            if (!Directory.Exists(SLocalLogDirectory))
            {
                Directory.CreateDirectory(SLocalLogDirectory);
            }
        }

        //======================================================================
        #endregion Constructor
        //======================================================================

        //======================================================================
        #region Control Methods
        //======================================================================

        private void button_Generate_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(textBox_Sites.Text))
                {
                    throw new Exception("Please enter one or more valid site IDs separated by ';' or ','");
                }

                if (radioButton_DateRange.IsChecked == true)
                {
                    if (datePicker_From.SelectedDate != null)
                    {
                        _dLogFromDate = StartOfDay(datePicker_From.SelectedDate.Value);
                    }
                    if (datePicker_To.SelectedDate != null)
                    {
                        _dLogToDate = EndOfDay(datePicker_To.SelectedDate.Value);
                    }
                    if (_dLogFromDate > _dLogToDate)
                    {
                        MessageBox.Show("Please check your FROM and TO dates.");
                        return;
                    }
                }
                else if (radioButton_Today.IsChecked == true)
                {
                    _dLogFromDate = StartOfDay(DateTime.Now);
                    _dLogToDate = EndOfDay(DateTime.Now);
                }
                else
                {
                    // should not happen
                    MessageBox.Show("CRITICAL ERROR!");
                    return;
                }

                ArrayList sites = GetSiteList();
                var localFileWiter = new StreamWriter(SLocalLogDirectory + "\\DenialReconciler.xml");
                localFileWiter.WriteLine("<Requests>");
                foreach (string site in sites)
                {
                    GenerateXMLFromList(site, GetPayerReportIds(site), localFileWiter);
                }
                localFileWiter.WriteLine("</Requests>");
                localFileWiter.Close();
                MessageBox.Show(_sStatusMessage);
                _sStatusMessage = string.Empty;
                var di = new DirectoryInfo(SLocalLogDirectory);
                CreateBatchFile(di);
            }
            catch (Exception x)
            {
                MessageBox.Show(x.Message);
            }
        }

        private void button_GoToDirectory_Click(object sender, RoutedEventArgs e)
        {
            if (!Directory.Exists(SLocalLogDirectory))
            {
                Directory.CreateDirectory(SLocalLogDirectory);
            }

            Process.Start(SLocalLogDirectory);
        }

        private void button_RunBatchFile_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Are you sure?", "Please confirm.", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                string sDir = SLocalLogDirectory;

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
            datePicker_From.IsEnabled = true;
            datePicker_To.IsEnabled = true;
        }

        private void radioButton_Today_Checked(object sender, RoutedEventArgs e)
        {
            datePicker_From.IsEnabled = false;
            datePicker_To.IsEnabled = false;
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
            const string batchFileText = @"c:\XactiMed\bin\SendUnarchiveMessage\SendUnarchiveMessage   DenialReconciler.xml" + "\nPAUSE";
            var localFileWiter = new StreamWriter(localLogDirectory + "\\RunDenialRecon.bat");
            localFileWiter.Write(batchFileText);
            localFileWiter.Flush();
            localFileWiter.Close();
        }

        private void GenerateXMLFromList(string sSiteID, ArrayList listOfIDs, StreamWriter writer)
        {
            string unarchiveRequest = "<Request Enabled=\"Y\" NextCommand=\"0\" ArrivedQueue=\"DOMSVR1\\X3APPQUECL8\" ArrivedTime=\"03/25/2008 09:01:15\" NextRunTime=\"03/25/2008 09:01:15\"><Progress IsComplete=\"N\" Step=\"0\" Total=\"0\" /><Task TaskID=\"861505\" SiteID=\"" + sSiteID + "\" LogSite=\"0\" LogUser=\"0\" Name=\"Additional ERA processing\" Schedule=\"03/25/2008 09:01:15\" Type=\"RunASAP\"><Commands><Command AbortOnFailure=\"Y\" ProgID=\"XactiMed.XApps.XClaim.ConsoleLauncherTask\" Name=\"Console Launcher\"><ConsoleLauncher FileName=\"%XMBIN%\\DenialReconciler\\DenialReconciler.exe\" Parameters=\"-MsgFile &quot;%M&quot;\" UseShell=\"true\" Timeout=\"0\" /><DenialReconciler PayerReportID=\"$$$$\" Verbose=\"N\" /></Command></Commands></Task></Request>";
            if ((listOfIDs.Count == 1 && (string)listOfIDs[0] == string.Empty) || listOfIDs.Count == 0)
            {
                _sStatusMessage += "No Payer Report IDs found for site " + sSiteID + ".\n";
                return;
            }
            _sStatusMessage += "Added the following for site " + sSiteID + ": ";
            foreach (string id in listOfIDs)
            {
                if (id != string.Empty)
                {
                    _sStatusMessage += id + " ";
                    writer.WriteLine(unarchiveRequest.Replace("$$$$", id));
                    writer.Flush();
                }
            }
            _sStatusMessage += '\n';
        }

        private string GetPayerReportId(FileInfo file)
        {
            var sPayerReportID = string.Empty;
            var sCurrentPayerReportID = string.Empty;
            try
            {
                var reader = new StreamReader(file.FullName);
                var line = reader.ReadLine();
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

                if (denialLogText.Contains("Exception:") || denialLogText.Contains("SqlException"))
                {
                    sPayerReportID = sCurrentPayerReportID;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }

            return sPayerReportID;
        }

        private ArrayList GetPayerReportIds(string sSiteID)
        {
            var payerReportIds = new ArrayList();

            var serverLogDirectory = new DirectoryInfo(Tools.GetLogLocation(int.Parse(sSiteID)) + @"\Logs\DenialReconciler");
            if (!serverLogDirectory.Exists)
            {
                // alternate location
                serverLogDirectory = new DirectoryInfo(Tools.GetSiteLocaion(int.Parse(sSiteID)) + @"\XDenial\Reports");
            }

            if (!serverLogDirectory.Exists)
            {
                // don't know where the files are
                _sStatusMessage += "Server log directory is unavailable for site " + sSiteID + ".\n";
            }
            else
            {
                var logFileList = serverLogDirectory.GetFiles("DenialReconciler.*");
                foreach (FileInfo file in logFileList)
                {
                    if (file.CreationTime >= _dLogFromDate && file.CreationTime <= _dLogToDate)
                    {
                        var sPayerReportID = GetPayerReportId(file);
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

        private ArrayList GetSiteList()
        {
            char[] caSeperators = { ',', ';' };
            var sList = textBox_Sites.Text;
            var sites = new ArrayList(sList.Split(caSeperators, StringSplitOptions.RemoveEmptyEntries));

            // trim siteIDs
            for (int i = 0; i < sites.Count; i++)
            {
                sites[i] = sites[i].ToString().Trim();
            }

            // remove any duplicates
            for (int i = 0; i < sites.Count; i++)
            {
                for (int j = sites.Count - 1; j > i; j--)
                {
                    if (sites[i].ToString() == sites[j].ToString())
                    {
                        sites.RemoveAt(j);
                    }
                }
            }

            return sites;
        }

        //======================================================================
        #endregion Other Methods
        //======================================================================
    }
}
