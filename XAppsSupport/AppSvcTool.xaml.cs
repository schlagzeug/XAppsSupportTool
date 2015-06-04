using System;
using System.Windows;
using System.Windows.Shapes;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Input;
using System.Windows.Documents;
using System.Windows.Data;
using System.Windows.Controls;
using System.Text;
using System.Linq;
using System.IO;
using System.Diagnostics;
using System.Data.SqlClient;
using System.Collections;
using System.Collections.Generic;

namespace XAppsSupport
{
    /// <summary>
    /// Interaction logic for AppSvcTool.xaml
    /// </summary>
    public partial class AppSvcTool : Window
    {
        #region Fields //==========================================================================

        private const string sTempAppSvcLogDirectory = @"C:\temp\AppSvc\";
        private const string sXMLFileName = @"\AppSvcFailed.xml";
        private string sLocalOuputDirectory = @"C:\AppSvcReRun\";
        private string[] sTaskServers = { "rcm40vpxapapp01", "rcm40vpxapapp02", "rcm40vpxapapp03", "rcm40vpxapapp04", "rcm40vpxapapp05", "rcm40vpxapapp06", "rcm40vpxapapp07", "rcm40vpxapapp08", "rcm40vpxapapp09", "rcm40vpxapapp10", "rcm40vpxapapp11", "rcm40vpxapapp12", "rcm40vpxapapp13" };

        #endregion Fields //=======================================================================

        #region Constructor //=====================================================================

        public AppSvcTool()
        {
            InitializeComponent();

            // create needed directories
            CreateNeededDirectories();
            SetButtonStatus();
            radioButton_Error.IsChecked = true;
            radioButton_SiteID.IsChecked = true;
            foreach (var server in sTaskServers)
            {
                comboBox_TaskServers.Items.Add(server);
            }
        }

        public AppSvcTool(int siteID)
        {
            InitializeComponent();

            // create needed directories
            CreateNeededDirectories();
            SetButtonStatus();
            radioButton_Error.IsChecked = true;
            radioButton_SiteID.IsChecked = true;
            foreach (var server in sTaskServers)
            {
                comboBox_TaskServers.Items.Add(server);
            }
            textBox_SiteID.Text = siteID.ToString();
            ShowLogs();
        }

        #endregion Constructor //==================================================================

        #region Control Methods //=================================================================

        private void button_CheckAll_Click(object sender, RoutedEventArgs e)
        {
            CheckAll(true);
            SetButtonStatus();
        }

        private void button_ClearAll_Click(object sender, RoutedEventArgs e)
        {
            CheckAll(false);
            SetButtonStatus();
        }

        private void button_GenerateFiles_Click(object sender, RoutedEventArgs e)
        {
            string sTaskSvr;
            var sSiteID = textBox_SiteID.Text;
            var sites = GetSiteList();

            if (radioButton_SiteID.IsChecked == true)
            {
                // make sure the site ID is filled out
                if (sSiteID == string.Empty)
                {
                    Tools.ShowMessage("Please enter one or more site IDs separated by a ',' or a ';'.");
                    return;
                }

                // find the task server
                if (!CheckTaskServers(sites))
                {
                    Tools.ShowError("Sites are not all on the same task server.");
                    return;
                }
                sTaskSvr = Tools.GetTaskServer(int.Parse(sites[0].ToString()));
            }
            else
            {
                if (comboBox_TaskServers.SelectedItem.ToString() == string.Empty)
                {
                    Tools.ShowError("Please pick a task server");
                    return;
                }
                else
                {
                    sTaskSvr = comboBox_TaskServers.SelectedItem.ToString();
                    sSiteID = sTaskSvr;
                    sites = new ArrayList();
                }
            }
            // get the files that are selected
            string sAppSvcLogLocation = @"\\" + sTaskSvr + @"\C$\XactiMed\AppSvc\Logs\";

            ArrayList alLogs = new ArrayList(listBox_Files.Items.Count);
            foreach (var file in listBox_Files.SelectedItems)
            {
                alLogs.Add(file);
            }

            // copy the AppSvc log(s) to local
            if (Directory.Exists(sTempAppSvcLogDirectory))
            {
                Directory.Delete(sTempAppSvcLogDirectory, true);
            }
            Directory.CreateDirectory(sTempAppSvcLogDirectory);

            foreach (var file in listBox_Files.SelectedItems)
            {
                if (File.Exists(sAppSvcLogLocation + file.ToString()))
                {
                    File.Copy(sAppSvcLogLocation + file.ToString(), sTempAppSvcLogDirectory + file.ToString());
                }
                else
                {
                    MessageBox.Show(String.Format("File: {0} doesn't exist", file.ToString()), "Error");
                }
            }

            // process the file
            ArrayList alAllTasksToReRun = new ArrayList();
            if (radioButton_Error.IsChecked == true)
            {
                foreach (var file in listBox_Files.SelectedItems)
                {
                    string sCurrentFile = sTempAppSvcLogDirectory + file.ToString();

                    // get list of failed tasks
                    ArrayList alFailedTasks = GetFailedTasks(sCurrentFile);
                    if (alFailedTasks.Count == 0)
                    {
                        MessageBox.Show("No failed tasks in " + sCurrentFile, "Error");
                        File.Delete(sCurrentFile);
                        continue;
                    }

                    // get list of tasks to re-run
                    ArrayList alTasksToReRun = GetTasksToReRun(sites, sCurrentFile, alFailedTasks);
                    if (alTasksToReRun.Count == 0)
                    {
                        MessageBox.Show("No failed tasks for this site in " + sCurrentFile, "Error");
                        File.Delete(sCurrentFile);
                        continue;
                    }

                    foreach (var task in alTasksToReRun)
                    {
                        alAllTasksToReRun.Add(task);
                    }
                }
            }
            else if (radioButton_KeyWord.IsChecked == true)
            {
                foreach (string file in listBox_Files.SelectedItems)
                {
                    string currentFile = sTempAppSvcLogDirectory + file;
                    ArrayList temp = GetTasksToReRunByKeyWord(sites, currentFile);

                    if (temp.Count == 0)
                    {
                        Tools.ShowMessage(string.Format("No failed tasks matching that keyword in {0}", currentFile));
                        continue;
                    }

                    foreach (var task in temp)
                    {
                        alAllTasksToReRun.Add(task);
                    }
                }
            }

            // create XML and batch file
            string sDir = sLocalOuputDirectory + sSiteID;
            if (!Directory.Exists(sDir))
            {
                Directory.CreateDirectory(sDir);
            }
            CreateXMLFile(sDir, alAllTasksToReRun);
            CreateBatchFile(sDir);

            // delete the temp file(s)
            for (int i = 0; i < 5; i++)
            {
                try
                {
                    Directory.Delete(sTempAppSvcLogDirectory, true);
                    break;
                }
                catch
                {
                    if (i < 4)
                    {
                        System.Threading.Thread.Sleep(5000);
                    }
                    else
                    {
                        Tools.ShowError(string.Format("failed to delete temp file in {0}", sTempAppSvcLogDirectory));
                    }
                }
            }
            MessageBox.Show(string.Format("{0} failed tasks found", alAllTasksToReRun.Count), "Success");
        }

        private ArrayList GetTasksToReRunByKeyWord(ArrayList sites, string currentFile)
        {
            StreamReader reader = new StreamReader(currentFile);
            string line = reader.ReadLine();
            ArrayList matchingLines = new ArrayList();

            while (line != null)
            {
                if (radioButton_SiteID.IsChecked == true)
                {
                    foreach (var site in sites)
                    {
                        if (line.StartsWith("<Request") && line.Contains("SiteID=\"" + site) && line.Contains(textBox_KeyWord.Text))
                            matchingLines.Add(line);
                    }
                }
                else
                {
                    if (line.StartsWith("<Request") && line.Contains(textBox_KeyWord.Text))
                        matchingLines.Add(line);
                }

                line = reader.ReadLine();
            }

            reader.Close();
            return matchingLines;
        }

        private void button_OpenDirectory_Click(object sender, RoutedEventArgs e)
        {
            string dir = string.Empty;
            if (radioButton_SiteID.IsChecked == true)
            {
                dir = textBox_SiteID.Text;
            }
            else
            {
                dir = comboBox_TaskServers.SelectedItem.ToString();
            }
            Tools.OpenDirectory(sLocalOuputDirectory + dir);
        }

        private void button_OpenFile_Click(object sender, RoutedEventArgs e)
        {
            if (radioButton_SiteID.IsChecked == true)
            {
                Tools.OpenFile(sLocalOuputDirectory + textBox_SiteID.Text + sXMLFileName);
            }
            else
            {
                Tools.OpenFile(sLocalOuputDirectory + comboBox_TaskServers.SelectedItem.ToString() + sXMLFileName);
            }
        }

        private void button_OpenLogDirectory_Click(object sender, RoutedEventArgs e)
        {
            string sTaskServer = GetTaskServer(); 
            if (sTaskServer != string.Empty)
            {
                string sDir = @"\\" + sTaskServer + @"\C$\XactiMed\AppSvc\Logs";
                Tools.OpenDirectory(sDir);
            }
        }

        private string GetTaskServer()
        {
            if (radioButton_SiteID.IsChecked == true)
                return Tools.GetTaskServer(int.Parse(textBox_SiteID.Text));
            else
                return comboBox_TaskServers.SelectedItem.ToString();
        }

        private void button_RunBatch_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Are you sure?", "Please confirm.", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                if (radioButton_SiteID.IsChecked == true)
                {
                    // MUST have a site ID
                    string sSiteID = textBox_SiteID.Text;
                    if (sSiteID == string.Empty)
                    {
                        Tools.ShowError("Please enter a site ID.");
                        return;
                    }

                    // directory MUST exist
                    string sDir = sLocalOuputDirectory + sSiteID;
                    if (!Directory.Exists(sDir))
                    {
                        Tools.ShowError("Invalid Directory.");
                        return;
                    }

                    // batch file MUST exist
                    string sBatch = sDir + @"\Run.bat";
                    if (!File.Exists(sBatch))
                    {
                        Tools.ShowError("No batch file.");
                        return;
                    }

                    // all checks have passed, run the batch
                    string sCurrentDir = Directory.GetCurrentDirectory();
                    Directory.SetCurrentDirectory(sDir);
                    Process.Start(sBatch);
                    Directory.SetCurrentDirectory(sCurrentDir);
                }
                else
                {
                    // MUST have seleted a task server
                    if (comboBox_TaskServers.SelectedItem.ToString() == string.Empty)
                    {
                        Tools.ShowError("Please choose a task server.");
                        return;
                    }

                    // directory MUST exist
                    string sDir = sLocalOuputDirectory + comboBox_TaskServers.SelectedItem.ToString();
                    if (!Directory.Exists(sDir))
                    {
                        Tools.ShowError("Invalid Directory.");
                        return;
                    }

                    // batch file MUST exist
                    string sBatch = sDir + @"\Run.bat";
                    if (!File.Exists(sBatch))
                    {
                        Tools.ShowError("No batch file.");
                        return;
                    }

                    // all checks have passed, run the batch
                    string sCurrentDir = Directory.GetCurrentDirectory();
                    Directory.SetCurrentDirectory(sDir);
                    Process.Start(sBatch);
                    Directory.SetCurrentDirectory(sCurrentDir);
                }
            }
        }

        private void button_ShowLogs_Click(object sender, RoutedEventArgs e)
        {
            ShowLogs();
        }

        private void ShowLogs()
        {
            string sTaskSvr = string.Empty;
            if (radioButton_SiteID.IsChecked == true)
            {
                // get the site ID
                string sSiteID = textBox_SiteID.Text;
                if (sSiteID == string.Empty)
                {
                    MessageBox.Show("Please pick a site ID.", "Error");
                    return;
                }

                // find the task server
                ArrayList sites = GetSiteList();
                if (!CheckTaskServers(sites))
                {
                    Tools.ShowError("Sites are not all on the same task server.");
                    return;
                }

                // find the task server
                sSiteID = sites[0].ToString();
                sTaskSvr = Tools.GetTaskServer(int.Parse(sSiteID));
                if (sTaskSvr == string.Empty)
                {
                    MessageBox.Show("Could not determine task server.", "Error");
                    return;
                }
            }
            else
            {
                sTaskSvr = comboBox_TaskServers.SelectedItem.ToString();
                if (sTaskSvr == string.Empty)
                {
                    Tools.ShowError("Please pick a task server");
                    return;
                }
            }
            // populate the text box with a list of the filenames
            listBox_Files.Items.Clear();
            string sAppSvcLogLocation = @"\\" + sTaskSvr + @"\C$\XactiMed\AppSvc\Logs\";
            DirectoryInfo di = new DirectoryInfo(sAppSvcLogLocation);
            FileInfo[] fileList = di.GetFiles();
            foreach (FileInfo file in fileList)
            {
                listBox_Files.Items.Add(file.Name);
            }
        }

        private void button_Test_Click(object sender, RoutedEventArgs e)
        {
            // for testing log parsing

            // open the file
            StreamReader reader = new StreamReader(@"C:\AppSvc_Test.log");
            string line = reader.ReadLine();
            ArrayList failedTasks = new ArrayList();
            ArrayList listOfRequests = new ArrayList();

            // get all the failed task IDs
            while (line != null)
            {
                if (line.Contains("BEGIN exception: TaskThread "))
                {
                    failedTasks.Add(line.Substring(28));
                }
                // get next line
                line = reader.ReadLine();
            }
            reader.Close();

            reader = new StreamReader(@"C:\AppSvc_Test.log");
            line = reader.ReadLine();

            // get the failed requests
            while (line != null)
            {
                foreach (string sTaskID in failedTasks)
                {
                    if (line.StartsWith("<Request") && line.Contains("SiteID=\"300050") && line.Contains("TaskID=\"" + sTaskID))
                    {
                        listOfRequests.Add(line);
                    }
                }
                // get next line
                line = reader.ReadLine();
            }
            reader.Close();

            // write xml and batch file
            string sDir = sLocalOuputDirectory + "300050";
            if (!Directory.Exists(sDir))
            {
                Directory.CreateDirectory(sDir);
            }
            CreateXMLFile(sDir, listOfRequests);
            CreateBatchFile(sDir);
        }

        private void checkBox_HCFAImports_Checked(object sender, RoutedEventArgs e)
        {
            SetButtonStatus();
        }

        private void checkBox_HCFAImports_Unchecked(object sender, RoutedEventArgs e)
        {
            SetButtonStatus();
        }

        private void checkBox_StatusUpdates_Checked(object sender, RoutedEventArgs e)
        {
            SetButtonStatus();
        }

        private void checkBox_StatusUpdates_Unchecked(object sender, RoutedEventArgs e)
        {
            SetButtonStatus();
        }

        private void checkBox_UBImports_Checked(object sender, RoutedEventArgs e)
        {
            SetButtonStatus();
        }

        private void checkBox_UBImports_Unchecked(object sender, RoutedEventArgs e)
        {
            SetButtonStatus();
        }

        private void checkBox_Unarchive_Checked(object sender, RoutedEventArgs e)
        {
            SetButtonStatus();
        }

        private void checkBox_Unarchive_Unchecked(object sender, RoutedEventArgs e)
        {
            SetButtonStatus();
        }

        #endregion Control Methods //==============================================================

        #region Other Methods //===================================================================

        private void CheckAll(bool value)
        {
            checkBox_Unarchive.IsChecked = value;
            checkBox_StatusUpdates.IsChecked = value;
            checkBox_UBImports.IsChecked = value;
            checkBox_HCFAImports.IsChecked = value;
        }

        private bool CheckTaskServers(ArrayList sites)
        {
            if (sites.Count == 0)
            {
                Tools.ShowError("No sites.");
                return false;
            }
            else if (sites.Count == 1)
            {
                return true;
            }
            else
            {
                string sBaseTaskServer = Tools.GetTaskServer(int.Parse(sites[0].ToString()));
                for (int i = 1; i < sites.Count; i++)
                {
                    string sTaskServer = Tools.GetTaskServer(int.Parse(sites[i].ToString()));
                    if (sTaskServer != sBaseTaskServer)
                    {
                        return false;
                    }
                }

                return true;
            }
        }

        private void CreateBatchFile(string sDir)
        {
            string batchFileText = @"c:\XactiMed\bin\SendUnarchiveMessage\SendUnarchiveMessage   AppSvcFailed.xml" + "\nPAUSE";
            StreamWriter fileWriter = new StreamWriter(sDir + @"\Run.bat");
            fileWriter.Write(batchFileText);
            fileWriter.Flush();
            fileWriter.Close();
        }

        private void CreateNeededDirectories()
        {
            if (!Directory.Exists(sTempAppSvcLogDirectory))
            {
                Directory.CreateDirectory(sTempAppSvcLogDirectory);
            }
            if (!Directory.Exists(sLocalOuputDirectory))
            {
                Directory.CreateDirectory(sLocalOuputDirectory);
            }
        }

        private void CreateXMLFile(string sDir, ArrayList alTasksToReRun)
        {
            // write xml and batch file
            StreamWriter fileWriter = new StreamWriter(sDir + sXMLFileName);
            fileWriter.WriteLine("<Requests>");
            foreach (string request in alTasksToReRun)
            {
                fileWriter.WriteLine(request);
            }
            fileWriter.WriteLine("</Requests>");
            fileWriter.Flush();
            fileWriter.Close();
        }

        private ArrayList GetFailedTasks(string sLogLocation)
        {
            // open the file
            StreamReader reader = new StreamReader(sLogLocation);
            string line = reader.ReadLine();
            ArrayList failedTasks = new ArrayList();

            // get all the failed task IDs
            while (line != null)
            {
                if (line.Contains("BEGIN exception: TaskThread "))
                {
                    failedTasks.Add(line.Substring(28));
                }
                // get next line
                line = reader.ReadLine();
            }
            reader.Close();

            return failedTasks;
        }

        private ArrayList GetSiteList()
        {
            char[] caSeperators = { ',', ';' };
            var sList = textBox_SiteID.Text;
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

        private ArrayList GetTasksToReRun(string sSiteID, string sLogLocation, ArrayList alFailedTasks)
        {
            StreamReader reader = new StreamReader(sLogLocation);
            string line = reader.ReadLine();
            ArrayList alListOfRequests = new ArrayList();

            // get the failed requests
            while (line != null)
            {
                foreach (string sTaskID in alFailedTasks)
                {
                    if (line.StartsWith("<Request") && line.Contains("SiteID=\"" + sSiteID) && line.Contains("TaskID=\"" + sTaskID))
                    {
                        bool bUnarchive = false;
                        bool bHcfa = false;
                        bool bUB = false;
                        bool bStatus = false;

                        if (checkBox_Unarchive.IsChecked == true && line.Contains("Name=\"UnarchiveClaimsTask\""))
                        {
                            bUnarchive = true;
                        }

                        if (checkBox_UBImports.IsChecked == true && line.Contains("Name=\"UB92 Import + Schedule\""))
                        {
                            bUB = true;
                        }

                        if (checkBox_HCFAImports.IsChecked == true && line.Contains("Name=\"HCFA1500 Import + Schedule\""))
                        {
                            bHcfa = true;
                        }

                        if (checkBox_StatusUpdates.IsChecked == true && line.Contains("Name=\"Payer Status Update\""))
                        {
                            bStatus = true;
                        }

                        if (bUnarchive || bHcfa || bUB || bStatus || isAllChecked(true))
                        {
                            alListOfRequests.Add(line);
                        }
                    }
                }
                // get next line
                line = reader.ReadLine();
            }
            reader.Close();

            return alListOfRequests;
        }

        private ArrayList GetTasksToReRun(ArrayList sites, string sLogLocation, ArrayList alFailedTasks)
        {
            StreamReader reader = new StreamReader(sLogLocation);
            string line = reader.ReadLine();
            ArrayList alListOfRequests = new ArrayList();

            // get the failed requests
            while (line != null)
            {
                foreach (string sTaskID in alFailedTasks)
                {
                    foreach (string site in sites)
                    {
                        if (line.StartsWith("<Request") && line.Contains("SiteID=\"" + site) && line.Contains("TaskID=\"" + sTaskID))
                        {
                            bool bUnarchive = false;
                            bool bHcfa = false;
                            bool bUB = false;
                            bool bStatus = false;

                            if (checkBox_Unarchive.IsChecked == true && line.Contains("Name=\"UnarchiveClaimsTask\""))
                            {
                                bUnarchive = true;
                            }

                            if (checkBox_UBImports.IsChecked == true && line.Contains("Name=\"UB92 Import + Schedule\""))
                            {
                                bUB = true;
                            }

                            if (checkBox_HCFAImports.IsChecked == true && line.Contains("Name=\"HCFA1500 Import + Schedule\""))
                            {
                                bHcfa = true;
                            }

                            if (checkBox_StatusUpdates.IsChecked == true && line.Contains("Name=\"Payer Status Update\""))
                            {
                                bStatus = true;
                            }

                            if (bUnarchive || bHcfa || bUB || bStatus || isAllChecked(true))
                            {
                                alListOfRequests.Add(line);
                            }
                        }
                    }
                }
                // get next line
                line = reader.ReadLine();
            }
            reader.Close();

            return alListOfRequests;
        }

        private bool isAllChecked(bool value)
        {
            if (checkBox_Unarchive.IsChecked == value &&
                checkBox_StatusUpdates.IsChecked == value &&
                checkBox_UBImports.IsChecked == value &&
                checkBox_HCFAImports.IsChecked == value)
                return true;
            return false;
        }

        private void SetButtonStatus()
        {
            if (isAllChecked(true))
            {
                button_CheckAll.IsEnabled = false;
                button_ClearAll.IsEnabled = true;
            }
            else if (isAllChecked(false))
            {
                button_CheckAll.IsEnabled = true;
                button_ClearAll.IsEnabled = false;
            }
            else
            {
                button_CheckAll.IsEnabled = true;
                button_ClearAll.IsEnabled = true;
            }
        }

        #endregion Other Methods //================================================================

        private void radioButton_SiteID_Checked(object sender, RoutedEventArgs e)
        {
            textBox_SiteID.IsEnabled = true;
            comboBox_TaskServers.IsEnabled = false;
            listBox_Files.Items.Clear();
        }

        private void radioButton_TaskServer_Checked(object sender, RoutedEventArgs e)
        {
            textBox_SiteID.IsEnabled = false;
            comboBox_TaskServers.IsEnabled = true;
            listBox_Files.Items.Clear();
        }

        private void comboBox_TaskServers_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            listBox_Files.Items.Clear();
            ShowLogs();
        }

        private void radioButton_Error_Checked(object sender, RoutedEventArgs e)
        {
            groupBox1.IsEnabled = true;
            groupBox3.IsEnabled = false;
        }

        private void radioButton_KeyWord_Checked(object sender, RoutedEventArgs e)
        {
            groupBox1.IsEnabled = false;
            groupBox3.IsEnabled = true;
        }
    }
}
