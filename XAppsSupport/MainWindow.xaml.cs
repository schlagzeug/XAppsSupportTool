using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Collections;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Xml;
using Microsoft.Win32;
using XactiMed;
using XactiMed.XApps;
using XactiMed.XApps.Common;
using XactiMed.XApps.Server;
using XactiMed.XApps.XClaim;
using XCLBCLAIMEDITORHCFALib;
using XCLBCLAIMEDITORUB92Lib;

namespace XAppsSupport
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Fields //==========================================================================

        private const string appConfigFile = @"C:\Program Files (x86)\XactiMed\AppStartup\AppStartup.exe.config";
        private const string locDSiteUserXml = @"\\crp40ppfs05\XApps_Public\BusinessLogic\Tools\Sites_Owner\Sites_Owner.xml";
        private const string LDCProdConnString = @"server=rcm40cpXapDB50\domsvr;database=X3Resource;Integrated Security=True";
        private const string LDCTestConnString = @"server=rcm40csXapDB50\domsvr1;database=X3Resource;Integrated Security=True";
        private const string LDC5010ConnString = @"server=rcm40csXapDB51\domsvr2;database=X3Resource;Integrated Security=True";
        private const string LDCProdServer = @"https://xWebApps.MedAssets.com/XApps.NET/niaGetZipPackage.aspx";
        private const string LDCTestServer = @"https://xTest.MedAssets.com/XApps.NET/niaGetZipPackage.aspx";
        private const string LDC5010Server = @"https://X5010.MedAssets.com/XApps.NET/niaGetZipPackage.aspx";
        
        ArrayList m_ClaimXMB_List = new ArrayList();
        int m_Index = 0;

        public SiteUserData siteUserData;
        public SiteGroupData siteGroupData;
        List<GroupData> GroupDataGridResults = null;
        List<ClaimData> ClaimDataGridResults = null;
        List<ImportData> ImportDataList = null;
        List<ExportData> ExportDataList = null;

        #endregion Fields //=======================================================================

        #region Properties //======================================================================

        public int SiteID
        {
            get
            {
                if (comboBox_SiteIDs.SelectedItem != null)
                    return int.Parse(comboBox_SiteIDs.SelectedItem.ToString());
                else
                    return -1;
            }
        }
        public string ConnectionString
        {
            get
            {
                return Tools.GetConnectionString();
            }
        }
        public string SelectedClaimType
        {
            get
            {
                if (radioButton_UB.IsChecked == true)
                {
                    return "UB92";
                }
                else
                {
                    return "HCFA1500";
                }
            }
        }
        public string ProdConnectionString
        {
            get
            {
                return LDCProdConnString;
            }
        }
        public string TestConnectionString
        {
            get
            {
                return LDCTestConnString;
            }
        }
        public string Test888ConnectionString
        {
            get
            {
                return LDC5010ConnString;
            }
        }
        public string ProdServer
        {
            get
            {
                return LDCProdServer;
            }
        }
        public string TestServer
        {
            get
            {
                return LDCTestServer;
            }
        }
        public string Test5010Server
        {
            get
            {
                return LDC5010Server;
            }
        }

        #endregion Properties //===================================================================

        #region Constructor //=====================================================================

        public MainWindow()
        {
            while (true)
            {
                try
                {
                    InitializeComponent();
                    radioButton_ClaimIDs.IsChecked = true;
                    SetInitialDrSeuss();
                    comboBox_SiteIDs.Focus();
                    radioButton_ClaimLookup_ClaimIDs.IsChecked = true;
                    textBox_ClaimLookup_MaxCount.Text = "1000";
                    radioButton_ImportExport_ByDate.IsChecked = true;
                    radioButton_ImportExport_Import.IsChecked = true;
                    datePicker_ImportExport_From.SelectedDate = DateTime.Today;
                    datePicker_ImportExport_To.SelectedDate = DateTime.Today;
                    break;
                }
                catch
                {
                    Tools.ShowError("ERROR DURING SETUP.");
                    if (System.Windows.MessageBox.Show("Try to reload?", "Please confirm.", MessageBoxButton.YesNo) == MessageBoxResult.Yes) continue;
                    break;
                }
            }
        }

        #endregion Constructor //==================================================================

        #region Import Methods From Other DLLs //==================================================

        // Get a handle to an application window.
        [DllImport("USER32.DLL", CharSet = CharSet.Unicode)]
        public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        // Activate an application window.
        [DllImport("USER32.DLL")]
        public static extern bool SetForegroundWindow(IntPtr hWnd);

        #endregion Methods from other DLLs //======================================================

        #region Control Methods //=================================================================

        #region Main Window //=====================================================================

        private void button_LaunchXClaim_Click(object sender, RoutedEventArgs e)
        {
            string sDir = @"C:\Program Files (x86)\XactiMed\AppStartup\AppStartup.exe";
            FileInfo fiApp = new FileInfo(sDir);
            if (fiApp.Exists)
            {
                Process.Start(sDir, "-allowmultiple");
                if (menu_EnableOneFish.IsChecked == false)
                    return;
                for (int i = 0; i < 5; i++)
                {
                    System.Threading.Thread.Sleep(4000);
                    IntPtr LoginHandle = FindWindow(null, "Support Login");
                    if (LoginHandle == IntPtr.Zero)
                    {
                        continue;
                    }

                    if (checkBox_AutoLogin.IsChecked == true)
                    {
                        SetForegroundWindow(LoginHandle);
                        System.Threading.Thread.Sleep(1000);
                        SendKeys.SendWait("{TAB}");
                        System.Threading.Thread.Sleep(50);
                        SendKeys.SendWait(SiteID.ToString());
                        System.Threading.Thread.Sleep(50);
                        SendKeys.SendWait("{TAB}");
                        System.Threading.Thread.Sleep(50);
                        SendKeys.SendWait("xactitest");
                        System.Threading.Thread.Sleep(50);
                        SendKeys.SendWait("{TAB}");
                        System.Threading.Thread.Sleep(50);
                        SendKeys.SendWait("oad2hcwimh!");
                        System.Threading.Thread.Sleep(50);
                        SendKeys.SendWait("{TAB}");
                        System.Threading.Thread.Sleep(50);
                        SendKeys.SendWait(" ");
                        System.Threading.Thread.Sleep(50);
                        SendKeys.SendWait("{TAB}");
                        System.Threading.Thread.Sleep(50);
                        SendKeys.SendWait("{TAB}");
                        System.Threading.Thread.Sleep(50);
                        SendKeys.SendWait("{ENTER}");
                        break;
                    }
                    else
                    {
                        SetForegroundWindow(LoginHandle);
                        System.Threading.Thread.Sleep(1000);
                        SendKeys.SendWait("{TAB}");
                        System.Threading.Thread.Sleep(50);
                        SendKeys.SendWait(SiteID.ToString());
                        System.Threading.Thread.Sleep(50);
                        SendKeys.SendWait("{TAB}");
                        System.Threading.Thread.Sleep(50);
                        SendKeys.SendWait("{TAB}");
                        System.Threading.Thread.Sleep(50);
                        SendKeys.SendWait("oad2hcwimh!");
                        System.Threading.Thread.Sleep(50);
                        SendKeys.SendWait("+{TAB}");
                        break;
                    }
                }
            }
            else
            {
                Tools.ShowError("The AppStartup executable could not be found!\nExpected location: " + sDir);
            }
        }
        private void comboBox_SiteIDs_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            textBox_Resources.Clear();
            dataGrid_ClaimLookup_Results.ItemsSource = null;
            label_ClaimLookup_ReturnedRows.Content = "Click 'Find' to search for Claims";
            dataGrid_ImportExport_Results.ItemsSource = null;
            label_ImportExport_RowsReturned.Content = "Click 'Find' to search for Imports and Exports";
            dataGrid_Groups_Results.ItemsSource = null;
            label_Group.Content = "Click 'Find' to search a site's groups";
            GroupDataGridResults = null;
            ClaimDataGridResults = null;
            ImportDataList = null;
            ExportDataList = null;
            dataGrid_Apps.ItemsSource = null;
        }
        
        #endregion Main Window //==================================================================

        #region Resources Tab //===================================================================
        
        private void button_Resources_Click(object sender, RoutedEventArgs e)
        {
            SetResourceTextBox();
        }
        private void SetResourceTextBox()
        {
            if (SiteID == 0)
            {
                textBox_Resources.Clear();
                textBox_Resources.Text = "Choose a valid site ID.";
                return;
            }
            try
            {
                textBox_Resources.Text = string.Empty;

                string connString = ConnectionString;
                string claimDB = Tools.GetClaimDatabase(connString, SiteID);// GetClaimDatabase(connString);

                // get the site name
                textBox_Resources.Text = Tools.GetSiteName(SiteID) + "\n*******************************************\n";

                // get the claim data base
                textBox_Resources.Text += "Claim Server:\t" + claimDB;

                // get the task server
                textBox_Resources.Text += "\nTask Server:\t" + Tools.GetTaskServer(SiteID);

                // get recycle bin information
                connString = "server=" + claimDB + ";database=Archive;Integrated Security=True";
                string maxDelete = string.Empty;
                using (SqlConnection conn = new SqlConnection(connString))
                {
                    string sQuery = "SELECT MaxDelete FROM Options WHERE SiteID = " + SiteID;
                    using (SqlCommand cmd = new SqlCommand(sQuery, conn))
                    {
                        conn.Open();
                        object returnval = cmd.ExecuteScalar();
                        maxDelete = returnval.ToString();
                        conn.Close();
                    }
                }

                connString = "server=" + claimDB + ";database=Primary;Integrated Security=True";
                using (SqlConnection conn = new SqlConnection(connString))
                {
                    string sQuery = "SELECT count(*) FROM Claims WHERE SiteID = " + SiteID + "and FolderID = 3";
                    using (SqlCommand cmd = new SqlCommand(sQuery, conn))
                    {
                        conn.Open();
                        object returnval = cmd.ExecuteScalar();
                        string deletedClaims = returnval.ToString();
                        conn.Close();

                        textBox_Resources.Text += "\nRecycle Bin:\t" + deletedClaims + "/" + maxDelete;

                        if (int.Parse(deletedClaims) >= int.Parse(maxDelete))
                        {
                            textBox_Resources.Text += "  - full";
                        }
                        else
                        {
                            textBox_Resources.Text += "  - OK";
                        }
                    }
                }

                // check for xlink user
                bool bHasXlinkUser = false;
                string sUserID = string.Empty;
                string sPwdDate = string.Empty;
                string sPwdCount = string.Empty;
                string sPwdLockout = string.Empty;
                string sPwdExpires = string.Empty;

                connString = ConnectionString;
                string domainString = Tools.GetDomainConnection(SiteID);
                using (SqlConnection conn = new SqlConnection(domainString))
                {
                    string sQuery = "SELECT UserID, PwdDate, PwdCount FROM Users WHERE SiteID = " + SiteID + "and Username like '%xlink%'";
                    using (SqlCommand cmd = new SqlCommand(sQuery, conn))
                    {
                        using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                        {
                            DataSet ds = new DataSet();

                            conn.Open();
                            da.Fill(ds);
                            conn.Close();

                            if (ds.Tables[0].Rows.Count <= 0)
                            {
                                textBox_Resources.Text += "\nNo XLink user detected";
                            }
                            else
                            {
                                DataRow row = ds.Tables[0].Rows[0];
                                sUserID = row["UserID"].ToString();
                                sPwdDate = row["PwdDate"].ToString();
                                sPwdCount = row["PwdCount"].ToString();

                                bHasXlinkUser = true;
                            }
                        }
                    }
                }

                if (bHasXlinkUser)
                {
                    using (SqlConnection conn = new SqlConnection(domainString))
                    {
                        string sQuery = "SELECT PwdLockout, PwdExpires FROM Sites WHERE SiteID = " + SiteID;
                        using (SqlCommand cmd = new SqlCommand(sQuery, conn))
                        {
                            using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                            {
                                DataSet ds = new DataSet();

                                conn.Open();
                                da.Fill(ds);
                                conn.Close();

                                if (ds.Tables[0].Rows.Count <= 0)
                                {
                                    textBox_Resources.Text += "\nNo XLink user detected";
                                }
                                else
                                {
                                    DataRow row = ds.Tables[0].Rows[0];
                                    sPwdLockout = row["PwdLockout"].ToString();
                                    sPwdExpires = row["PwdExpires"].ToString();

                                    // construct data output
                                    textBox_Resources.Text += "\nXLink User ID: " + sUserID;
                                    textBox_Resources.Text += "\n  Password lockout: " + sPwdCount + "/" + sPwdLockout;
                                    textBox_Resources.Text += "\n  Password expires " + sPwdExpires + " days after " + sPwdDate;
                                }
                            }
                        }
                    }
                }

                // BR framework Site?
                string xAppsGlobalConnString = Tools.GetResource(0, 0, 11, 1);
                List<string> clientIDs = new List<string>();
                using (SqlConnection conn = new SqlConnection(xAppsGlobalConnString))
                {
                    string sQuery = "select distinct clientID from BridgeRoutines";
                    using (SqlCommand cmd = new SqlCommand(sQuery, conn))
                    {
                        using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                        {
                            DataSet ds = new DataSet();

                            conn.Open();
                            da.Fill(ds);
                            conn.Close();

                            foreach (DataRow row in ds.Tables[0].Rows)
                            {
                                clientIDs.Add(row["ClientID"].ToString());
                            }
                        }
                    }
                }
                if (clientIDs.Contains(SiteID.ToString()))
                {
                    textBox_Resources.Text += "\nClient is a Bridge Routine Framework site.";
                    string currentMax = string.Empty;
                    using (SqlConnection conn = new SqlConnection(xAppsGlobalConnString))
                    {
                        string sQuery = string.Format("select MAX(clientRoutineID) from BridgeRoutines where ClientID = {0}", SiteID);
                        using (SqlCommand cmd = new SqlCommand(sQuery, conn))
                        {
                            using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                            {
                                conn.Open();
                                object returnval = cmd.ExecuteScalar();
                                currentMax = returnval.ToString();
                                conn.Close();
                            }
                        }
                    }
                    if (currentMax != string.Empty)
                    {
                        int max = int.Parse(currentMax) + 1;
                        textBox_Resources.Text += "\n  Next available routine ID: " + max.ToString();
                    }
                }

                // Who owns the site
                XmlDocument ownersDoc = new XmlDocument();
                if (File.Exists(locDSiteUserXml))
                {
                    ownersDoc.Load(locDSiteUserXml);
                    XmlNodeList listowners = ownersDoc.SelectNodes("/SiteOwners/SiteOwner");
                    string owner = string.Empty;
                    foreach (XmlNode node in listowners)
                    {
                        if (owner != string.Empty) break;
                        string sites = EzXml.GetStringEz(node, string.Empty);
                        if (sites != string.Empty)
                        {
                            string[] sitesArray = sites.Split(new char[] { ',' });
                            foreach (string site in sitesArray)
                            {
                                int nSite = 0;
                                if (int.TryParse(site, out nSite) && nSite == SiteID)
                                {
                                    owner = EzXml.GetStringAttributeEz(node, "OwnerName", string.Empty);
                                }
                            }
                        }
                    }
                    if (owner != string.Empty)
                    {
                        textBox_Resources.Text += string.Format("\nSite Owner is {0}", owner);
                    }
                }
            }
            catch (Exception ex)
            {
                Tools.ShowError(ex.ToString());
                return;
            }
        }
        private void button_ClaimServer_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string connString = Tools.GetConnectionString();
                string claimDB = Tools.GetClaimDatabase(connString, SiteID);
                Process.Start("ssms", "-noSplash -S " + claimDB);
            }
            catch
            {
                Tools.ShowError("Select a valid Site ID.");
            }
        }
        private void button_TaskServer_Click(object sender, RoutedEventArgs e)
        {
            string taskServer = @"\\" + Tools.GetTaskServer(SiteID) + @"\C$";
            Process.Start(taskServer);
        }
        private void button_DomainServer_Click(object sender, RoutedEventArgs e)
        {
            string sDomain = Tools.GetCurrentServerSimple();
            Process.Start("ssms", "-noSplash -S " + sDomain);
        }
        private void button_Resources_SQLJobs_Click(object sender, RoutedEventArgs e)
        {
            var x = new SQL_Jobs(SiteID.ToString());
            x.Show();
        }
        private void button_ViewJobTrackJobs_Click(object sender, RoutedEventArgs e)
        {
            if (SiteID != 0)
            {
                var x = new JobTrack2(SiteID.ToString());
                x.Show();
            }
            else
            {
                var x = new JobTrack2();
                x.Show();
            }
        }
        
        #endregion Resources Tab //================================================================

        #region Applications Tab //================================================================

        private void button_Apps_OpenSelected_Click(object sender, RoutedEventArgs e)
        {
            if (dataGrid_Apps.SelectedItems.Count > 3)
            {
                if (System.Windows.MessageBox.Show("You have selected more than 3 apps to open.  This could slow the system down, are you sure?", "Please confirm.", MessageBoxButton.YesNo) == MessageBoxResult.No)
                {
                    return;
                }
            }

            foreach (AppInfo app in dataGrid_Apps.SelectedItems)
            {
                Tools.OpenFile(app.GetLocalLocation());
            }
        }
        private void button_Apps_UpdateSelected_Click(object sender, RoutedEventArgs e)
        {
            if (dataGrid_Apps.SelectedItems.Count <= 0) Tools.ShowMessage("No apps selected.");

            foreach (AppInfo app in dataGrid_Apps.SelectedItems)
            {
                try
                {
                    string localdir = System.IO.Path.GetDirectoryName(app.GetServerLocation());
                    string serverdir = localdir.Replace(@"C:\", string.Empty);

                    if (localdir == string.Empty || serverdir == string.Empty)
                    {
                        Tools.ShowError(string.Format("Directory error: Local={0}; Server={1}", localdir, serverdir));
                        continue;
                    }
                    serverdir = string.Format(@"\\{0}\C$\{1}", Tools.GetTaskServer(SiteID), serverdir);

                    Tools.CopyDirectory(serverdir, localdir, false);
                }
                catch (Exception ex)
                {
                    Tools.ShowError(ex.ToString());
                }
            }
            Tools.ShowMessage(string.Format("{0} directories updated.", dataGrid_Apps.SelectedItems.Count));
        }
        private void button_Apps_Show_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                dataGrid_Apps.ItemsSource = null;

                List<AppInfo> allAppList = new List<AppInfo>();

                if (checkBox_Apps_SpecFiles.IsChecked == true)
                    allAppList.AddRange(GetAppsFromSpecFiles());
                if (checkBox_Apps_ImportCfg.IsChecked == true)
                    allAppList.AddRange(GetAppsFromImportConfigs());
                if (checkBox_Apps_UnarchiveCfg.IsChecked == true)
                    allAppList.AddRange(GetAppsFromUnarchiveConfigs());
                if (checkBox_Apps_ExportCfg.IsChecked == true)
                    allAppList.AddRange(GetAppsFromExportConfig());

                RemoveDupesInList(allAppList);

                dataGrid_Apps.ItemsSource = allAppList;
            }
            catch (Exception ex)
            {
                Tools.ShowError(ex.ToString());
            }
        }
        private void RemoveDupesInList(List<AppInfo> list)
        {
            List<AppInfo> cleanedList = new List<AppInfo>();
            for (int i = 0; i < list.Count; i++)
            {
                for (int j = i + 1; j < list.Count; j++)
                {
                    if (list[i].Equals(list[j]))
                    {
                        list.RemoveAt(j);
                        j--;
                    }
                }
            }
        }
        private List<AppInfo> GetAppsFromExportConfig()
        {
            List<AppInfo> appList = new List<AppInfo>();
            if (checkBox_Apps_Inst.IsChecked == true || checkBox_Apps_Prof.IsChecked == true)
            {
                string siteLocation = Tools.GetSiteLocaion(SiteID);
                string exportConfigLocation = siteLocation + @"\XClaim\ExportConfig.xml";

                if (File.Exists(exportConfigLocation))
                {
                    XmlDocument doc = new XmlDocument();
                    doc.Load(exportConfigLocation);

                    XmlNode exportConfig = doc.FirstChild;
                    XmlNodeList xmlPosts = exportConfig.SelectNodes("/EXPORTCFG/XMLPOST/DO");

                    foreach (XmlNode postApp in xmlPosts)
                    {
                        if (!postApp.InnerText.Contains(".exe")) continue;
                        string appPath = postApp.Attributes.GetNamedItem("EXEC").Value;
                        string appName = System.IO.Path.GetFileName(appPath);
                        string solutionPath = GetSolutionPath(appPath);
                        AppInfo ai = new AppInfo("ExportConfig.xml", "BOTH", "XMLPOST", appName, appPath, solutionPath);
                        appList.Add(ai);
                    }
                }
            }
            return appList;
        }
        private List<AppInfo> GetAppsFromUnarchiveConfigs()
        {
            List<AppInfo> appList = new List<AppInfo>();
            string siteLocation = Tools.GetSiteLocaion(SiteID);
            string ubUnarchiveConfig = siteLocation + @"\XClaim\UB92\Import\UnarchiveCfg.xml";
            string hcfaUnarchiveConfig = siteLocation + @"\XClaim\HCFA1500\Import\UnarchiveCfg.xml";

            if (File.Exists(ubUnarchiveConfig) && checkBox_Apps_Inst.IsChecked == true)
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(ubUnarchiveConfig);

                XmlNode exportConfig = doc.FirstChild;
                XmlNodeList xmlPreps = exportConfig.SelectNodes("/UNARCHIVECFG/XMLPREP/CMD");
                XmlNodeList xmlPosts = exportConfig.SelectNodes("/UNARCHIVECFG/XMLPOST/CMD");

                foreach (XmlNode app in xmlPreps)
                {
                    if (!app.InnerText.Contains(".exe")) continue;
                    string appPath = app.InnerText.Substring(0, app.InnerText.IndexOf(".exe") + 4);
                    string appName = System.IO.Path.GetFileName(appPath);
                    string solutionPath = GetSolutionPath(appPath);
                    AppInfo ai = new AppInfo("UnarchiveCfg.xml", "INST", "XMLPREP", appName, appPath, solutionPath);
                    appList.Add(ai);
                }

                foreach (XmlNode app in xmlPosts)
                {
                    if (!app.InnerText.Contains(".exe")) continue;
                    string appPath = app.InnerText.Substring(0, app.InnerText.IndexOf(".exe") + 4);
                    string appName = System.IO.Path.GetFileName(appPath);
                    string solutionPath = GetSolutionPath(appPath);
                    AppInfo ai = new AppInfo("UnarchiveCfg.xml", "INST", "XMLPOST", appName, appPath, solutionPath);
                    appList.Add(ai);
                }
            }

            if (File.Exists(hcfaUnarchiveConfig) && checkBox_Apps_Prof.IsChecked == true)
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(hcfaUnarchiveConfig);

                XmlNode exportConfig = doc.FirstChild;
                XmlNodeList xmlPreps = exportConfig.SelectNodes("/UNARCHIVECFG/XMLPREP/CMD");
                XmlNodeList xmlPosts = exportConfig.SelectNodes("/UNARCHIVECFG/XMLPOST/CMD");

                foreach (XmlNode app in xmlPreps)
                {
                    if (!app.InnerText.Contains(".exe")) continue;
                    string appPath = app.InnerText.Substring(0, app.InnerText.IndexOf(".exe") + 4);
                    string appName = System.IO.Path.GetFileName(appPath);
                    string solutionPath = GetSolutionPath(appPath);
                    AppInfo ai = new AppInfo("UnarchiveCfg.xml", "PROF", "XMLPREP", appName, appPath, solutionPath);
                    appList.Add(ai);
                }

                foreach (XmlNode app in xmlPosts)
                {
                    if (!app.InnerText.Contains(".exe")) continue;
                    string appPath = app.InnerText.Substring(0, app.InnerText.IndexOf(".exe") + 4);
                    string appName = System.IO.Path.GetFileName(appPath);
                    string solutionPath = GetSolutionPath(appPath);
                    AppInfo ai = new AppInfo("UnarchiveCfg.xml", "PROF", "XMLPOST", appName, appPath, solutionPath);
                    appList.Add(ai);
                }
            }

            return appList;
        }
        private List<AppInfo> GetAppsFromImportConfigs()
        {
            List<AppInfo> appList = new List<AppInfo>();
            string siteLocation = Tools.GetSiteLocaion(SiteID);
            string ubImportConfig = siteLocation + @"\XClaim\UB92\Import\ImportCfg.xml";
            string hcfaImportConfig = siteLocation + @"\XClaim\HCFA1500\Import\ImportCfg.xml";

            if (File.Exists(ubImportConfig) && checkBox_Apps_Inst.IsChecked == true)
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(ubImportConfig);

                XmlNode exportConfig = doc.FirstChild;
                XmlNodeList xmlPosts = exportConfig.SelectNodes("/IMPORTCFG/XMLPOST/CMD");

                foreach (XmlNode app in xmlPosts)
                {
                    if (!app.InnerText.Contains(".exe")) continue;
                    string appPath = app.InnerText.Substring(0, app.InnerText.IndexOf(".exe") + 4);
                    string appName = System.IO.Path.GetFileName(appPath);
                    string solutionPath = GetSolutionPath(appPath);
                    AppInfo ai = new AppInfo("ImportCfg.xml", "INST", "XMLPOST", appName, appPath, solutionPath);
                    appList.Add(ai);
                }
            }

            if (File.Exists(hcfaImportConfig) && checkBox_Apps_Prof.IsChecked == true)
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(hcfaImportConfig);

                XmlNode exportConfig = doc.FirstChild;
                XmlNodeList xmlPosts = exportConfig.SelectNodes("/IMPORTCFG/XMLPOST/CMD");

                foreach (XmlNode app in xmlPosts)
                {
                    if (!app.InnerText.Contains(".exe")) continue;
                    string appPath = app.InnerText.Substring(0, app.InnerText.IndexOf(".exe") + 4);
                    string appName = System.IO.Path.GetFileName(appPath);
                    string solutionPath = GetSolutionPath(appPath);
                    AppInfo ai = new AppInfo("ImportCfg.xml", "PROF", "XMLPOST", appName, appPath, solutionPath);
                    appList.Add(ai);
                }
            }

            return appList;
        }
        private List<AppInfo> GetAppsFromSpecFiles()
        {
            List<AppInfo> appList = new List<AppInfo>();
            string siteLocation = Tools.GetSiteLocaion(SiteID);
            string ubImportDirectory = siteLocation + @"\XClaim\UB92\Import";
            string hcfaImportDirectory = siteLocation + @"\XClaim\HCFA1500\Import";
            string[] ubSpecFiles = Directory.GetFiles(ubImportDirectory, "*spec*.xml");
            string[] hcfaSpecFiles = Directory.GetFiles(hcfaImportDirectory, "*spec*.xml");

            if (checkBox_Apps_Inst.IsChecked == true)
            {
                foreach (string file in ubSpecFiles)
                {
                    XmlDocument doc = new XmlDocument();
                    using (StreamReader oReader = new StreamReader(file, Encoding.GetEncoding("ISO-8859-1")))
                        doc.Load(oReader);

                    XmlNode exportConfig = doc.FirstChild;
                    XmlNodeList imgPreps = exportConfig.SelectNodes("/IMAGESPEC/IMGPREP/CMD");
                    XmlNodeList xmlPreps = exportConfig.SelectNodes("/IMAGESPEC/XMLPREP/CMD");

                    foreach (XmlNode app in imgPreps)
                    {
                        if (!app.InnerText.Contains(".exe")) continue;
                        string appPath = app.InnerText.Substring(0, app.InnerText.IndexOf(' ') + 1); // incase of scripts
                        string appName = System.IO.Path.GetFileName(appPath);
                        string solutionPath = GetSolutionPath(appPath);
                        AppInfo ai = new AppInfo(System.IO.Path.GetFileName(file), "INST", "IMGPREP", appName, appPath, solutionPath);
                        appList.Add(ai);
                    }

                    foreach (XmlNode app in xmlPreps)
                    {
                        if (!app.InnerText.Contains(".exe")) continue;
                        string appPath = app.InnerText.Substring(0, app.InnerText.IndexOf(' ') + 1); // incase of scripts
                        string appName = System.IO.Path.GetFileName(appPath);
                        string solutionPath = GetSolutionPath(appPath);
                        AppInfo ai = new AppInfo(System.IO.Path.GetFileName(file), "INST", "XMLPREP", appName, appPath, solutionPath);
                        appList.Add(ai);
                    }
                }
            }

            if (checkBox_Apps_Prof.IsChecked == true)
            {
                foreach (string file in hcfaSpecFiles)
                {
                    XmlDocument doc = new XmlDocument();
                    doc.Load(file);

                    XmlNode exportConfig = doc.FirstChild;
                    XmlNodeList imgPreps = exportConfig.SelectNodes("/IMAGESPEC/IMGPREP/CMD");
                    XmlNodeList xmlPreps = exportConfig.SelectNodes("/IMAGESPEC/XMLPREP/CMD");

                    foreach (XmlNode app in imgPreps)
                    {
                        if (!app.InnerText.Contains(".exe")) continue;
                        string appPath = app.InnerText.Substring(0, app.InnerText.IndexOf(' ') + 1); // incase of scripts
                        string appName = System.IO.Path.GetFileName(appPath);
                        string solutionPath = GetSolutionPath(appPath);
                        AppInfo ai = new AppInfo(System.IO.Path.GetFileName(file), "PROF", "IMGPREP", appName, appPath, solutionPath);
                        appList.Add(ai);
                    }

                    foreach (XmlNode app in xmlPreps)
                    {
                        if (!app.InnerText.Contains(".exe")) continue;
                        string appPath = app.InnerText.Substring(0, app.InnerText.IndexOf(' ') + 1); // incase of scripts
                        string appName = System.IO.Path.GetFileName(appPath);
                        string solutionPath = GetSolutionPath(appPath);
                        AppInfo ai = new AppInfo(System.IO.Path.GetFileName(file), "PROF", "XMLPREP", appName, appPath, solutionPath);
                        appList.Add(ai);
                    }
                }
            }

            return appList;
        }
        private string GetSolutionPath(string appPath)
        {
            string returnPath = string.Empty;
            string customDevLocation = @"C:\XactiMed.Net\XApps\customdev";
            string trunkLocation = @"C:\XactiMed.Net\XApps\trunk";
            string appName = System.IO.Path.GetFileNameWithoutExtension(appPath);

            if (appPath.Contains("XClaim.ImgPreps"))
            {
                if (appName == "Ansi837ToXmb")
                {
                    returnPath = @"C:\XactiMed.Net\XApps\trunk\XactiMed.XApps.XClaim\Ansi\Ansi837ToXmb\Ansi837ToXmb.sln";
                }
                else
                {
                    returnPath = string.Format(@"{0}\XactiMed.XApps.XClaim\ImgPreps\{1}\{1}.sln", customDevLocation, appName);
                }
            }
            else if (appPath.Contains("XClaim.Prep"))
            {
                returnPath = string.Format(@"{0}\XactiMed.XApps.XClaim\XactiMed.XApps.XClaim.Prep\{1}\{1}.sln", customDevLocation, appName);
            }
            else if (appPath.Contains("XClaim.BridgeRoutines"))
            {
                appName = string.Format("BridgeRoutines_{0}.BridgeRoutines", SiteID.ToString());
                returnPath = string.Format(@"{0}\XactiMed.XApps.XClaim\XactiMed.Xapps.XClaim.BridgeRoutinePrep\{1}\{1}.sln", customDevLocation, appName);
            }
            else if (appPath.Contains("XClaim.Post"))
            {
                returnPath = string.Format(@"{0}\XactiMed.XApps.XClaim\XactiMed.Xapps.XClaim.Post\{1}\{1}.sln", customDevLocation, appName);
            }
            else if (appPath.Contains("XClaim.UnarchivePrep"))
            {
                returnPath = string.Format(@"{0}\XactiMed.XApps.XClaim\XactiMed.XApps.XClaim.UnarchivePrep\{1}\{1}.sln", customDevLocation, appName);
            }
            else if (new string[] { "XmlPrepUB92", "XmlPrepHcfa1500" }.Contains(appName))
            {
                returnPath = string.Format(@"{0}\Native\XClaim.Prep\{1}\{1}.sln", trunkLocation, appName);
            }
            else if (appPath.Contains("GenericFileUnzipper"))
            {
                returnPath = string.Format(@"{0}\XactiMed.XApps.XClaim\_TOOLS\{1}\{1}.sln", customDevLocation, appName);
            }
            else
            {
                //Perl.exe ????


                //string[] results = Directory.GetFiles(customDevLocation, string.Format("{0}.sln", appName), SearchOption.AllDirectories);
                //if (results.Length == 0)
                //{
                //    results = Directory.GetFiles(trunkLocation, appName, SearchOption.AllDirectories);
                //}
            }

            //Tools.OpenFile(returnPath);
            return returnPath;
        }
        private void button_Apps_OpenSolutionLocation_Click(object sender, RoutedEventArgs e)
        {
            foreach (AppInfo app in dataGrid_Apps.SelectedItems)
            {
                string dir = System.IO.Path.GetDirectoryName(app.GetLocalLocation());
                Tools.OpenDirectory(dir);
            }
        }

        #endregion //==============================================================================

        #region File Locations Tab //==============================================================

        private void button_LocalData_Click(object sender, RoutedEventArgs e)
        {
            string appDataFolder = Tools.GetAppDataFolder();

            string sDir = string.Empty;
            if (SiteID == 0)
            {
                sDir = System.IO.Path.Combine(appDataFolder + @"\Local\XactiMed\XApps\Data\");
            }
            else
            {
                sDir = System.IO.Path.Combine(appDataFolder + @"\Local\XactiMed\XApps\Data\" + SiteID.ToString());
            }

            Tools.OpenDirectory(sDir);
        }
        private void button_CustomerSS_Click(object sender, RoutedEventArgs e)
        {
            string sDir = string.Empty;
            string siteID = SiteID.ToString();
            if (siteID.StartsWith("999") || siteID.StartsWith("888"))
            {
                siteID = siteID.Substring(3);
            }
            if (SiteID == 0)
            {
                sDir = @"C:\CustomerSS\";
            }
            else
            {
                sDir = @"C:\CustomerSS\" + siteID;
            }

            Tools.OpenDirectory(sDir);
        }
        private void button_UpdateCustomerSS_Click(object sender, RoutedEventArgs e)
        {
            if (SiteID == 0)
            {
                Tools.ShowError("Please pick a site ID.");
                return;
            }

            if (SiteID.ToString().StartsWith("999") || SiteID.ToString().StartsWith("888"))
            {
                Tools.ShowError("Please switch to producton to update.");
                return;
            }

            if (checkBox_UB.IsChecked == true)
            {
                Tools.UpdateCustomerSS(SiteID, UpdateType.UB);
            }

            if (checkBox_HCFA.IsChecked == true)
            {
                Tools.UpdateCustomerSS(SiteID, UpdateType.HCFA);
            }

            if (checkBox_Tables.IsChecked == true)
            {
                Tools.UpdateCustomerSS(SiteID, UpdateType.TABLES);
            }

            if (checkBox_FileStructure.IsChecked == true)
            {
                Tools.AddReportFolder(SiteID);
                Tools.CreateCustomerSSFileStructure(SiteID);
            }
        }
        private void button_ClearData_Click(object sender, RoutedEventArgs e)
        {
            if (System.Windows.MessageBox.Show("This will delete all of your local XApps data.  Are you sure?", "Please confirm.", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                string appDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                if (appDataFolder.EndsWith(@"\Roaming"))
                {
                    appDataFolder = appDataFolder.Replace(@"\Roaming", string.Empty);
                }
                string sDir = System.IO.Path.Combine(appDataFolder + @"\Local\XactiMed\XApps\Data\");
                DirectoryInfo di = new DirectoryInfo(sDir);
                foreach (FileInfo file in di.GetFiles())
                {
                    file.Delete();
                }
                foreach (DirectoryInfo dir in di.GetDirectories())
                {
                    dir.Delete(true);
                }
                Tools.ShowMessage("Files deleted.");
            }
        }
        private void button_CleanCustomerSS_Click(object sender, RoutedEventArgs e)
        {
            if (System.Windows.MessageBox.Show("This will remove all .tab, .bak, .log, and .snapshot files from the CustomerSS folder.  Are you sure?", "Please confirm.", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                DirectoryInfo d = new DirectoryInfo(@"C:\CustomerSS");
                Tools.CleanDirectory(d);
                Tools.ShowMessage("CustomerSS has been cleaned.");
            }
        }
        private void button_Prod_Click(object sender, RoutedEventArgs e)
        {
            string sDir = Tools.GetSiteLocaion(SiteID);
            Tools.OpenDirectory(sDir);
        }
        private void button_DownloadMisc_Click(object sender, RoutedEventArgs e)
        {
            string sDir = Tools.GetSiteLocaion(SiteID) + @"\XClaim\Download\Misc";
            Tools.OpenDirectory(sDir);
        }
        private void button_ProdLog_Click(object sender, RoutedEventArgs e)
        {
            string sDir = Tools.GetLogLocation(SiteID);
            Tools.OpenDirectory(sDir);
        }
        private void button_WorkingFiles_Click(object sender, RoutedEventArgs e)
        {
            var dir = Tools.GetWorkingFilesDirectory(SiteID);
            Tools.OpenDirectory(dir);
        }
        private void button_FTP_Click(object sender, RoutedEventArgs e)
        {
            var dir = Tools.GetFTPDirectory(SiteID);
            Tools.OpenDirectory(dir);
        }
        private void button_ClaimsPrinting_Click(object sender, RoutedEventArgs e)
        {
            string siteLocation = Tools.GetSiteLocaion(SiteID);
            string cpxml = siteLocation + @"\XClaim\ClaimsPrinting.xml";
            Tools.OpenFile(cpxml);
        }
        private void button_XLinkSiteCfg_Click(object sender, RoutedEventArgs e)
        {
            string siteLocation = Tools.GetSiteLocaion(SiteID);
            string xlink = siteLocation + @"\XLinkSiteCfg.xml";
            Tools.OpenFile(xlink);
        }
        private void button_ExportConfig_Click(object sender, RoutedEventArgs e)
        {
            string siteLocation = Tools.GetSiteLocaion(SiteID);
            string export = siteLocation + @"\XClaim\ExportConfig.xml";
            Tools.OpenFile(export);
        }
        private void button_XDMConfig_Click(object sender, RoutedEventArgs e)
        {
            string siteLocation = Tools.GetSiteLocaion(SiteID);
            string xdm = siteLocation + @"\XDenial\XDM.config";
            Tools.OpenFile(xdm);
        }
        private void button_ClaimConverter_Click(object sender, RoutedEventArgs e)
        {
            string siteLocation = Tools.GetSiteLocaion(SiteID);
            string cc = siteLocation + @"\XClaim\ClaimConverter.xml";
            Tools.OpenFile(cc);
        }
        private void button_SecondaryConfig_Click(object sender, RoutedEventArgs e)
        {
            string siteLocation = Tools.GetSiteLocaion(SiteID);
            string sc = siteLocation + @"\XClaim\SecondaryConfig.xml";
            Tools.OpenFile(sc);
        }
        
        #endregion File Locations Tab //===========================================================
        
        #region Claims Tab //======================================================================
        
        private void button_OpenClaim_Click(object sender, RoutedEventArgs e)
        {
            while (true)
            {
                string sClaimXmlTemp = string.Empty;
                string sClaimXml = string.Empty;
                string siteID = SiteID.ToString();
                if (siteID.StartsWith("999") || siteID.StartsWith("888"))
                    siteID = siteID.Substring(3);
                System.Windows.Forms.OpenFileDialog openDiag = new System.Windows.Forms.OpenFileDialog();
                openDiag.InitialDirectory = string.Format(@"C:\CustomerSS\{0}\XClaim\{1}\IMPORT\", siteID, SelectedClaimType);
                openDiag.Multiselect = false;
                openDiag.Filter = "XMB File | *.xmb";
                openDiag.DefaultExt = "xmb";
                openDiag.FileName = "*.xmb";

                System.Windows.Forms.DialogResult result1 = openDiag.ShowDialog();
                if (result1 == System.Windows.Forms.DialogResult.OK)
                {
                    XmlBatchFileCombo mCombo = OpenCombo(openDiag.FileName);
                    XmlNode claimXml = mCombo.GetNextDocument();
                    if (claimXml == null)
                    {
                        Tools.ShowError("Empty XMB.");
                        continue;
                    }
                    m_ClaimXMB_List = new ArrayList();
                    m_ClaimXMB_List.Add(claimXml.OuterXml);
                    mCombo.AddDocument(claimXml);
                    while (claimXml != null)
                    {
                        claimXml = mCombo.GetNextDocument();
                        if (claimXml != null)
                        {
                            m_ClaimXMB_List.Add(claimXml.OuterXml);
                            mCombo.AddDocument(claimXml);
                        }
                    }
                    mCombo.Close();

                    m_Index = 0;
                    string ClaimXmlTemp = m_ClaimXMB_List[m_Index].ToString();

                    if (SelectedClaimType == "UB92")
                    {
                        ClaimEditor ce = new ClaimEditor(SiteID, 0, m_ClaimXMB_List);
                    }
                    else
                    {
                        ClaimEditor ce = new ClaimEditor(SiteID, 1, m_ClaimXMB_List);
                    }
                }
                return;
            }
        }
        private void button_SaveXMB_Click(object sender, RoutedEventArgs e)
        {
            if (SiteID == 0)
            {
                Tools.ShowMessage("Please pick a valid site ID.");
                return;
            }
            System.Windows.Forms.SaveFileDialog saveDiag = new System.Windows.Forms.SaveFileDialog();
            saveDiag.Filter = "XMB Files | *.xmb";
            saveDiag.DefaultExt = "xmb";
            saveDiag.InitialDirectory = string.Format(@"C:\CustomerSS\{0}\XClaim\{1}\Import\", SiteID, SelectedClaimType);

            if (radioButton_ImportID.IsChecked == true)
            {
                try
                {
                    int importID = int.Parse(textBox_ClaimSearch.Text);
                    ArrayList claimIDs = Tools.GetClaimIdsByImportId(SiteID, importID);

                    saveDiag.FileName = string.Format("Import_{0}", importID);
                    DialogResult result = saveDiag.ShowDialog();
                    if (result == System.Windows.Forms.DialogResult.OK)
                    {
                        string savePath = saveDiag.FileName;
                        Tools.WriteXMB(SiteID, claimIDs, savePath);
                    }
                }
                catch
                {
                    Tools.ShowError("Invalid import ID.");
                }
            }
            else if (radioButton_ClaimIDs.IsChecked == true)
            {
                try
                {
                    ArrayList claimIDs = GetClaimIDs();

                    saveDiag.FileName = "*.xmb";
                    DialogResult result = saveDiag.ShowDialog();
                    if (result == System.Windows.Forms.DialogResult.OK)
                    {
                        string savePath = saveDiag.FileName;
                        Tools.WriteXMB(SiteID, claimIDs, savePath);
                    }
                }
                catch
                {
                    Tools.ShowError("Invalid claim ID.");
                }
            }
        }
        private void button_SaveOriginal_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.SaveFileDialog saveDiag = new System.Windows.Forms.SaveFileDialog();
            saveDiag.Filter = "XMB Files | *.xmb";
            saveDiag.DefaultExt = "xmb";
            saveDiag.InitialDirectory = string.Format(@"C:\CustomerSS\{0}\XClaim\{1}\Import\", SiteID, SelectedClaimType);
            string origWithHeader = string.Empty;

            try
            {
                if (radioButton_ImportID.IsChecked == true)
                {
                    int importID = 0;
                    try
                    {
                        importID = int.Parse(textBox_ClaimSearch.Text);
                    }
                    catch
                    {
                        Tools.ShowError("Invalid import ID.");
                    }

                    ArrayList claimIDs = Tools.GetClaimIdsByImportId(SiteID, importID);
                    string origImage = Tools.GetClaimOriginalImage(SiteID, claimIDs);
                    origWithHeader = Tools.AddHeader(origImage, SelectedClaimType);

                    //string savePath = string.Format(@"C:\CustomerSS\{0}\XClaim\{1}\Import\import_{2}.837", SiteID, SelectedClaimType, importID);

                    //Tools.WriteOriginalImageFile(origWithHeader, savePath);
                }
                else if (radioButton_ClaimIDs.IsChecked == true)
                {
                    ArrayList claimIDs = GetClaimIDs();
                    string origImage = Tools.GetClaimOriginalImage(SiteID, claimIDs);
                    origWithHeader = Tools.AddHeader(origImage, SelectedClaimType);

                    //string savePath = string.Format(@"C:\CustomerSS\{0}\XClaim\{1}\Import\claims.837", SiteID, SelectedClaimType);

                    //Tools.WriteOriginalImageFile(origWithHeader, savePath);
                }

                if (origWithHeader.Contains("ISA*"))
                {
                    saveDiag.Filter = "837 Files | *.837";
                    saveDiag.DefaultExt = "837";
                    saveDiag.FileName = "*.837";
                }
                else
                {
                    saveDiag.Filter = "Image Files | *.image";
                    saveDiag.DefaultExt = "image";
                    saveDiag.FileName = "*.image";
                }

                DialogResult result = saveDiag.ShowDialog();
                if (result == System.Windows.Forms.DialogResult.OK)
                {
                    string savePath = saveDiag.FileName;
                    Tools.WriteOriginalImageFile(origWithHeader, savePath);
                }
            }
            catch (Exception ex)
            {
                Tools.ShowError(ex.ToString());
            }
        }
        private void button_LocalImportFolder_Click(object sender, RoutedEventArgs e)
        {
            string folderPath = string.Format(@"C:\CustomerSS\{0}\XClaim\{1}\Import\", SiteID, SelectedClaimType);
            Tools.OpenDirectory(folderPath);
        }
        private void radioButton_ClaimIDs_Checked(object sender, RoutedEventArgs e)
        {
            groupBox_ClaimType.IsEnabled = true;
            textBox_ClaimSearch.Clear();
        }
        private void radioButton_ImportID_Checked(object sender, RoutedEventArgs e)
        {
            groupBox_ClaimType.IsEnabled = false;
            textBox_ClaimSearch.Clear();
        }
        
        #endregion Claims Tab //===================================================================

        #region Misc Tab //========================================================================

        private void button_FPImmediate_Click(object sender, RoutedEventArgs e)
        {
            Tools.OpenDirectory(@"\\rcm40vpxapapp70\c$\XactiMed\FileProcessingServices\FileProcessingServiceImmediate\Logs");
        }
        private void button_FPImportOnly_Click(object sender, RoutedEventArgs e)
        {
            Tools.OpenDirectory(@"\\rcm40vpxapapp70\c$\XactiMed\FileProcessingServices\FileProcessingServiceImportOnly\Logs");
        }
        private void button_FPLongRunning_Click(object sender, RoutedEventArgs e)
        {
            Tools.OpenDirectory(@"\\rcm40vpxapapp70\c$\XactiMed\FileProcessingServices\FileProcessingServiceLongRunning\Logs");
        }

        private void button_TaskServers_Click(object sender, RoutedEventArgs e)
        {
            var taskServerFileLocation = @"C:\taskservers.txt";
            ArrayList sites = GetSiteList();
            StreamWriter outfile = new StreamWriter(taskServerFileLocation);
            foreach (string site in sites)
            {
                var taskServer = Tools.GetTaskServer(int.Parse(site));
                outfile.WriteLine(string.Format("{0} -- {1}", taskServer, site));
            }
            outfile.Close();
            Tools.OpenFile(taskServerFileLocation);
        }
        private void button_ClaimServers_Click(object sender, RoutedEventArgs e)
        {
            var claimServerFileLocation = @"C:\claimservers.txt";
            ArrayList sites = GetSiteList();
            StreamWriter outfile = new StreamWriter(claimServerFileLocation);
            foreach (string site in sites)
            {
                var taskServer = Tools.GetClaimDatabase(ConnectionString, int.Parse(site));
                outfile.WriteLine(string.Format("{0} -- {1}", taskServer, site));
            }
            outfile.Close();
            Tools.OpenFile(claimServerFileLocation);
        }
        private ArrayList GetSiteList()
        {
            char[] caSeperators = { ',', ';' };
            var sList = textBox_Sites.Text;
            var sites = new ArrayList(sList.Split(caSeperators, System.StringSplitOptions.RemoveEmptyEntries));

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

        private void button_Misc_SiteLookup_Find_Click(object sender, RoutedEventArgs e)
        {
            textBox_Misc_SiteLookup_Answer.Text = "SEARCHING......";

            using (SqlConnection conn = new SqlConnection(LDCProdConnString))
            {
                string sQuery = string.Format("SELECT DISTINCT SiteID FROM Resources WHERE Resource like '%{0}%'", textBox_Misc_SiteLookup_SearchTerm.Text);
                using (SqlCommand cmd = new SqlCommand(sQuery, conn))
                {
                    using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                    {
                        DataSet ds = new DataSet();

                        conn.Open();
                        da.Fill(ds);
                        conn.Close();

                        textBox_Misc_SiteLookup_Answer.Text = string.Empty;

                        foreach (DataRow row in ds.Tables[0].Rows)
                        {
                            textBox_Misc_SiteLookup_Answer.Text += row["SiteID"].ToString() + "\r\n";
                        }

                        if (textBox_Misc_SiteLookup_Answer.Text == string.Empty)
                            textBox_Misc_SiteLookup_Answer.Text = "NOTHING FOUND...";
                    }
                }
            }
        }

        #endregion Misc Tab //=====================================================================

        #region Claim Lookup Tab //================================================================
        
        private void button_ClaimLookup_Find_Click(object sender, RoutedEventArgs e)
        {
            if (ClaimDataGridResults == null) ClaimDataGridResults = new List<ClaimData>();
            ClaimDataGridResults.Clear();

            int maxrows = 1000;
            if (!int.TryParse(textBox_ClaimLookup_MaxCount.Text, out maxrows))
            {
                textBox_ClaimLookup_MaxCount.Text = "1000";
                maxrows = 1000;
            }
            string input = SanitizeClaimLookupInput();
            string query = string.Format("SELECT TOP {0} * FROM Claims WITH (NOLOCK) WHERE SiteID = {1} ",maxrows, SiteID);
            if (radioButton_ClaimLookup_ClaimIDs.IsChecked == true)
            {
                query += string.Format("AND ClaimID in ({0}) ", input);
            }
            else if (radioButton_ClaimLookup_PCNs.IsChecked == true)
            {
                query += string.Format("AND PCN in ({0}) ", input);
            }
            else if (radioButton_ClaimLookup_ImportIDs.IsChecked == true)
            {
                query += string.Format("AND ImportID in ({0})", input);
            }
            else if (radioButton_ClaimLookup_ExportIDs.IsChecked == true)
            {
                query += string.Format("AND ExportID in ({0})", input);
            }
            else
            {
                Tools.ShowError("Input type must be selected. This shouldn't happen!");
                return;
            }

            LoadClaimDataList(ClaimDataGridResults, query, false);
            LoadClaimDataList(ClaimDataGridResults, query, true);
            dataGrid_ClaimLookup_Results.ItemsSource = null;
            dataGrid_ClaimLookup_Results.ItemsSource = ClaimDataGridResults;
            label_ClaimLookup_ReturnedRows.Content = string.Format("{0} Rows Returned", ClaimDataGridResults.Count);
        }
        private string SanitizeClaimLookupInput()
        {
            string returnValue = string.Empty;
            if (textBox_ClaimLookup_Input.Text.ToUpper().Contains("DROP"))
                return string.Empty;
            string[] splitValues = textBox_ClaimLookup_Input.Text.Split(new char[] { ',', ';' });
            foreach (string value in splitValues)
            {
                returnValue += string.Format("'{0}',", value);
            }
            if (returnValue.EndsWith(","))
                returnValue = returnValue.Substring(0, returnValue.Length - 1);
            return returnValue;
        }
        private void LoadClaimDataList(List<ClaimData> claimData, string query, bool isArchive)
        {
            try
            {
                string connString = isArchive ? Tools.GetArchiveClaimDBConnectionString(SiteID) : Tools.GetPrimaryClaimDBConnectionString(SiteID);
                string server = isArchive ? "ARCHIVE" : "PRIMARY";
                using (SqlConnection conn = new SqlConnection(connString))
                {
                    using (SqlDataAdapter da = new SqlDataAdapter(query, conn))
                    {
                        conn.Open();
                        DataSet ds = new DataSet();
                        da.Fill(ds);
                        conn.Close();
                        if (siteUserData == null)
                            siteUserData = new SiteUserData(SiteID);
                        if (siteUserData.SiteID != SiteID)
                            siteUserData.LoadUserData(SiteID);
                        if (siteGroupData == null)
                            siteGroupData = new SiteGroupData(SiteID);
                        if (siteGroupData.SiteID != SiteID)
                            siteGroupData.LoadGroupData(SiteID);
                        foreach (DataRow row in ds.Tables[0].Rows)
                        {
                            ClaimData cd = new ClaimData(siteUserData, siteGroupData);
                            cd.Server = server;
                            cd.FolderID = row["FolderID"].ToString();
                            cd.ClaimID = row["ClaimID"].ToString();
                            cd.Type = row["Type"].ToString();
                            cd.PCN = row["PCN"].ToString();
                            cd.ImportID = row["ImportID"].ToString();
                            cd.ImportDate = row["ImportDate"].ToString();
                            cd.ExportID = row["ExportID"].ToString();
                            cd.ExportDate = row["ExportDate"].ToString();
                            cd.User = row["UserID"].ToString();
                            cd.Group = row["GroupID"].ToString();
                            cd.ExtraChr2 = row["ExtraChr2"].ToString();
                            claimData.Add(cd);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Tools.ShowError(string.Format("Error getting claims: {0}", ex.ToString()));
            }
        }
        private void button_ClaimLookup_Clear_Click(object sender, RoutedEventArgs e)
        {
            dataGrid_ClaimLookup_Results.ItemsSource = null;
            ClaimDataGridResults = null;
            label_ClaimLookup_ReturnedRows.Content = "Click 'Find' to search for Claims";
        }
        private void button_ClaimLookup_PopOut_Click(object sender, RoutedEventArgs e)
        {
            if (ClaimDataGridResults == null || ClaimDataGridResults.Count <= 0)
            {
                Tools.ShowError("No results to pop out.");
            }
            else
            {
                var x = new PopOutGrid(ClaimDataGridResults, "Claims");
                x.Show();
            }
        }
        private void radioButton_ClaimLookup_ClaimIDs_Click(object sender, RoutedEventArgs e)
        {
            textBox_ClaimLookup_Input.Focus();
        }
        private void radioButton_ClaimLookup_PCNs_Click(object sender, RoutedEventArgs e)
        {
            textBox_ClaimLookup_Input.Focus();
        }
        private void radioButton_ClaimLookup_ImportIDs_Click(object sender, RoutedEventArgs e)
        {
            textBox_ClaimLookup_Input.Focus();
        }
        private void radioButton_ClaimLookup_ExportIDs_Click(object sender, RoutedEventArgs e)
        {
            textBox_ClaimLookup_Input.Focus();
        }
        private void button_ClaimLookup_ViewHistory_Click(object sender, RoutedEventArgs e)
        {
            foreach (var selectedClaim in dataGrid_ClaimLookup_Results.SelectedCells)
            {
                if (selectedClaim.Column.Header.ToString() == "ClaimID")
                {
                    try
                    {
                        ClaimData cd = (ClaimData)selectedClaim.Item;
                        ClaimDB claimdb = new ClaimDB();

                        string diffString = string.Empty;
                        if (cd.Server == "PRIMARY")
                            diffString = claimdb.GetClaimDiffListXmlEx(XClaimResource.PriDatabase, SiteID, int.Parse(cd.ClaimID));
                        else
                            diffString = claimdb.GetClaimDiffListXmlEx(XClaimResource.ArcDatabase, SiteID, int.Parse(cd.ClaimID));
                        
                        XmlDocument doc = new XmlDocument();
                        doc.LoadXml(diffString);

                        XmlNodeList diffsNodes = doc.SelectNodes("row/Diffs");
                        List<Diffs> diffs = new List<Diffs>();

                        foreach (XmlNode diff in diffsNodes)
                        {
                            Diffs d = new Diffs(diff);
                            diffs.Add(d);
                        }

                        string result = string.Empty;
                        for (int i = 0; i < diffs.Count; i++)
                        {
                            if (i > 0)
                            {
                                result += "\r\n======================================================\r\n";
                            }
                                
                            result += diffs[i].ToString();
                        }

                        PopOutTextBox tb = new PopOutTextBox(result, string.Format("Change History for claim {0}", cd.ClaimID));
                        tb.Show();
                    }
                    catch
                    {
                        Tools.ShowError("Error getting History");
                    }
                }
            }
        }
        private void button_ClaimLookup_ViewNotes_Click(object sender, RoutedEventArgs e)
        {
            foreach (var selectedClaim in dataGrid_ClaimLookup_Results.SelectedCells)
            {
                if (selectedClaim.Column.Header.ToString() == "ClaimID")
                {
                    try
                    {
                        ClaimData cd = (ClaimData)selectedClaim.Item;
                        ClaimDB claimdb = new ClaimDB();
                        Stream stream = new MemoryStream();
                        if (cd.Server == "PRIMARY")
                            claimdb.GetClaimNoteListXmlAllEx(XClaimResource.PriDatabase, SiteID, int.Parse(cd.ClaimID), stream);
                        else
                            claimdb.GetClaimNoteListXmlAllEx(XClaimResource.ArcDatabase, SiteID, int.Parse(cd.ClaimID), stream);
                        stream.Position = 0;
                        XmlDocument doc = new XmlDocument();
                        doc.Load(stream);

                        XmlNodeList noteNodes = doc.SelectNodes("row/Note");
                        List<ClaimNote> notes = new List<ClaimNote>();
                        foreach (XmlNode note in noteNodes)
                        {
                            ClaimNote cn = new ClaimNote();
                            bool isSystemUser = false;
                            for (int i = 0; i < note.Attributes.Count; i++)
                            {
                                if (note.Attributes[i].Name == "Text")
                                    cn.Type = note.Attributes[i].Value;
                                else if (note.Attributes[i].Name == "UserName")
                                    cn.UserName = note.Attributes[i].Value;
                                else if (note.Attributes[i].Name == "Date")
                                    cn.Date = note.Attributes[i].Value;
                                else if (note.Attributes[i].Name == "User" && note.Attributes[i].Value == "0")
                                    isSystemUser = true;
                            }
                            if (string.IsNullOrEmpty(cn.UserName) && isSystemUser)
                                cn.UserName = "SYSTEM";
                            cn.Note = note.InnerText;
                            notes.Add(cn);
                        }

                        PopOutGrid grid = new PopOutGrid(notes, string.Format("Notes for Claim ID {0}", cd.ClaimID));
                        grid.Show();
                    }
                    catch
                    {
                        Tools.ShowError("Error getting notes");
                    }
                }
            }
        }
        private void button_ClaimLookup_SaveXMB_Click(object sender, RoutedEventArgs e)
        {
            if (SiteID == 0)
            {
                Tools.ShowMessage("Please pick a valid site ID.");
                return;
            }

            ArrayList instClaimIDs = new ArrayList();
            ArrayList profClaimIDs = new ArrayList();
            foreach (var selectedClaim in dataGrid_ClaimLookup_Results.SelectedCells)
            {
                if (selectedClaim.Column.Header.ToString() == "ClaimID")
                {
                    ClaimData cd = (ClaimData)selectedClaim.Item;
                    if (cd.Type == "PROF")
                        profClaimIDs.Add(cd.ClaimID);
                    else if (cd.Type == "INST")
                        instClaimIDs.Add(cd.ClaimID);
                }
            }

            if (profClaimIDs.Count > 0)
            {
                System.Windows.Forms.SaveFileDialog profSaveDiag = new System.Windows.Forms.SaveFileDialog();
                profSaveDiag.Filter = "XMB Files | *.xmb";
                profSaveDiag.DefaultExt = "xmb";
                profSaveDiag.InitialDirectory = string.Format(@"C:\CustomerSS\{0}\XClaim\HCFA1500\Import\", SiteID);

                try
                {
                    profSaveDiag.FileName = "*.xmb";
                    DialogResult result = profSaveDiag.ShowDialog();
                    if (result == System.Windows.Forms.DialogResult.OK)
                    {
                        string savePath = profSaveDiag.FileName;
                        Tools.WriteXMB(SiteID, profClaimIDs, savePath);
                    }
                }
                catch
                {
                    Tools.ShowError("Invalid claim ID.");
                }
            }

            if (instClaimIDs.Count > 0)
            {
                System.Windows.Forms.SaveFileDialog instSaveDiag = new System.Windows.Forms.SaveFileDialog();
                instSaveDiag.Filter = "XMB Files | *.xmb";
                instSaveDiag.DefaultExt = "xmb";
                instSaveDiag.InitialDirectory = string.Format(@"C:\CustomerSS\{0}\XClaim\UB92\Import\", SiteID);

                try
                {
                    instSaveDiag.FileName = "*.xmb";
                    DialogResult result = instSaveDiag.ShowDialog();
                    if (result == System.Windows.Forms.DialogResult.OK)
                    {
                        string savePath = instSaveDiag.FileName;
                        Tools.WriteXMB(SiteID, instClaimIDs, savePath);
                    }
                }
                catch
                {
                    Tools.ShowError("Invalid claim ID.");
                }
            }
        }
        private void button_ClaimLookup_SaveOriginal_Click(object sender, RoutedEventArgs e)
        {
            ArrayList instClaimIDs = new ArrayList();
            ArrayList profClaimIDs = new ArrayList();
            foreach (var selectedClaim in dataGrid_ClaimLookup_Results.SelectedCells)
            {
                if (selectedClaim.Column.Header.ToString() == "ClaimID")
                {
                    ClaimData cd = (ClaimData)selectedClaim.Item;
                    if (cd.Type == "PROF")
                        profClaimIDs.Add(cd.ClaimID);
                    else if (cd.Type == "INST")
                        instClaimIDs.Add(cd.ClaimID);
                }
            }

            if (profClaimIDs.Count > 0)
            {
                System.Windows.Forms.SaveFileDialog profSaveDiag = new System.Windows.Forms.SaveFileDialog();
                profSaveDiag.Filter = "XMB Files | *.xmb";
                profSaveDiag.DefaultExt = "xmb";
                profSaveDiag.InitialDirectory = string.Format(@"C:\CustomerSS\{0}\XClaim\HCFA1500\Import\", SiteID);
                string origWithHeader = string.Empty;

                try
                {
                    string origImage = Tools.GetClaimOriginalImage(SiteID, profClaimIDs);
                    origWithHeader = Tools.AddHeader(origImage, "HCFA1500");

                    if (origWithHeader.Contains("ISA*"))
                    {
                        profSaveDiag.Filter = "837 Files | *.837";
                        profSaveDiag.DefaultExt = "837";
                        profSaveDiag.FileName = "*.837";
                    }
                    else
                    {
                        profSaveDiag.Filter = "Image Files | *.image";
                        profSaveDiag.DefaultExt = "image";
                        profSaveDiag.FileName = "*.image";
                    }

                    DialogResult result = profSaveDiag.ShowDialog();
                    if (result == System.Windows.Forms.DialogResult.OK)
                    {
                        string savePath = profSaveDiag.FileName;
                        Tools.WriteOriginalImageFile(origWithHeader, savePath);
                    }
                }
                catch (Exception ex)
                {
                    Tools.ShowError(ex.ToString());
                }
            }

            if (instClaimIDs.Count > 0)
            {
                System.Windows.Forms.SaveFileDialog instSaveDiag = new System.Windows.Forms.SaveFileDialog();
                instSaveDiag.Filter = "XMB Files | *.xmb";
                instSaveDiag.DefaultExt = "xmb";
                instSaveDiag.InitialDirectory = string.Format(@"C:\CustomerSS\{0}\XClaim\UB92\Import\", SiteID);
                string origWithHeader = string.Empty;

                try
                {
                    string origImage = Tools.GetClaimOriginalImage(SiteID, instClaimIDs);
                    origWithHeader = Tools.AddHeader(origImage, "UB92");

                    if (origWithHeader.Contains("ISA*"))
                    {
                        instSaveDiag.Filter = "837 Files | *.837";
                        instSaveDiag.DefaultExt = "837";
                        instSaveDiag.FileName = "*.837";
                    }
                    else
                    {
                        instSaveDiag.Filter = "Image Files | *.image";
                        instSaveDiag.DefaultExt = "image";
                        instSaveDiag.FileName = "*.image";
                    }

                    DialogResult result = instSaveDiag.ShowDialog();
                    if (result == System.Windows.Forms.DialogResult.OK)
                    {
                        string savePath = instSaveDiag.FileName;
                        Tools.WriteOriginalImageFile(origWithHeader, savePath);
                    }
                }
                catch (Exception ex)
                {
                    Tools.ShowError(ex.ToString());
                }
            }
        }

        #endregion Claim Lookup Tab //=============================================================

        #region Import/Export Tab //===============================================================

        private void button_ImportExport_Find_Click(object sender, RoutedEventArgs e)
        {
            if (radioButton_ImportExport_Import.IsChecked == true)
            {
                if (ImportDataList == null) ImportDataList = new List<ImportData>();
                ImportDataList.Clear();

                string query = string.Format("SELECT TOP 1000 * FROM Imports WITH (NOLOCK) WHERE SiteID = {0} ", SiteID);
                if (radioButton_ImportExport_ByID.IsChecked == true)
                {
                    string importIDs = GetImportExportIDsSanitized();
                    query += string.Format("AND ImportID in ({0}) ", importIDs);
                }
                else if (radioButton_ImportExport_ByDate.IsChecked == true)
                {
                    query += string.Format("AND [Date] > '{0}' AND [Date] < '{1}'",
                        Tools.StartOfDay(datePicker_ImportExport_From.SelectedDate.Value).ToString(),
                        Tools.EndOfDay(datePicker_ImportExport_To.SelectedDate.Value).ToString());
                }
                else
                {
                    Tools.ShowError("This shouldn't happen");
                    return;
                }

                LoadImportDataList(ImportDataList, query);
                dataGrid_ImportExport_Results.ItemsSource = null;
                dataGrid_ImportExport_Results.ItemsSource = ImportDataList;
                label_ImportExport_RowsReturned.Content = string.Format("{0} Rows Returned", ImportDataList.Count);
            }
            else if (radioButton_ImportExport_Export.IsChecked == true)
            {
                if (ExportDataList == null) ExportDataList = new List<ExportData>();
                ExportDataList.Clear();
                string query = string.Format("SELECT TOP 1000 * FROM Exports WITH (NOLOCK) WHERE SiteID = {0} ", SiteID);
                
                if (radioButton_ImportExport_ByID.IsChecked == true)
                {
                    string exportIDs = GetImportExportIDsSanitized();
                    query += string.Format("AND ExportID in ({0}) ", exportIDs);
                }
                else if (radioButton_ImportExport_ByDate.IsChecked == true)
                {
                    query += string.Format("AND [Date] > '{0}' AND [Date] < '{1}'",
                        Tools.StartOfDay(datePicker_ImportExport_From.SelectedDate.Value).ToString(),
                        Tools.EndOfDay(datePicker_ImportExport_To.SelectedDate.Value).ToString());
                }
                else
                {
                    Tools.ShowError("This shouldn't happen");
                    return;
                }

                LoadExportDataList(ExportDataList, query);
                dataGrid_ImportExport_Results.ItemsSource = null;
                dataGrid_ImportExport_Results.ItemsSource = ExportDataList;
                label_ImportExport_RowsReturned.Content = string.Format("{0} Rows Returned", ExportDataList.Count);
            }
            else
            {
                Tools.ShowError("This shouldn't happen");
                return;
            }
        }
        private void button_ImportExport_Clear_Click(object sender, RoutedEventArgs e)
        {
            dataGrid_ImportExport_Results.ItemsSource = null;
            label_ImportExport_RowsReturned.Content = "Click 'Find' to search for Imports and Exports";
        }
        private void radioButton_ImportExport_Import_Checked(object sender, RoutedEventArgs e)
        {
            dataGrid_ImportExport_Results.ItemsSource = null;
            button_ImportExport_ViewReport.IsEnabled = true;
        }
        private void radioButton_ImportExport_Export_Checked(object sender, RoutedEventArgs e)
        {
            dataGrid_ImportExport_Results.ItemsSource = null;
            button_ImportExport_ViewReport.IsEnabled = false;
        }
        private void radioButton_ImportExport_ByID_Checked(object sender, RoutedEventArgs e)
        {
            datePicker_ImportExport_To.IsEnabled = false;
            datePicker_ImportExport_From.IsEnabled = false;
            textBox_ImportExport_IDs.IsEnabled = true;
            textBox_ImportExport_IDs.Focus();
        }
        private void radioButton_ImportExport_ByDate_Checked(object sender, RoutedEventArgs e)
        {
            datePicker_ImportExport_To.IsEnabled = true;
            datePicker_ImportExport_From.IsEnabled = true;
            textBox_ImportExport_IDs.IsEnabled = false;
        }
        private void LoadExportDataList(List<ExportData> exportDataList, string query)
        {
            try
            {
                string connString = Tools.GetPrimaryClaimDBConnectionString(SiteID);
                using (SqlConnection conn = new SqlConnection(connString))
                {
                    using (SqlDataAdapter da = new SqlDataAdapter(query, conn))
                    {
                        conn.Open();
                        DataSet ds = new DataSet();
                        da.Fill(ds);
                        conn.Close();
                        if (siteUserData == null)
                            siteUserData = new SiteUserData(SiteID);
                        if (siteUserData.SiteID != SiteID)
                            siteUserData.LoadUserData(SiteID);
                        foreach (DataRow row in ds.Tables[0].Rows)
                        {
                            ExportData ed = new ExportData(siteUserData);
                            ed.ExportID = row["ExportID"].ToString();
                            ed.Date = row["Date"].ToString();
                            ed.ExportType = row["ExportType"].ToString();
                            ed.ClaimType = row["ClaimType"].ToString();
                            ed.User = row["LogUser"].ToString();
                            ed.Count = row["Count"].ToString();
                            exportDataList.Add(ed);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Tools.ShowError(string.Format("Error getting Export Data: {0}", ex.ToString()));
            }
        }
        private void LoadImportDataList(List<ImportData> importDataList, string query)
        {
            try
            {
                string connString = Tools.GetPrimaryClaimDBConnectionString(SiteID);
                using (SqlConnection conn = new SqlConnection(connString))
                {
                    using (SqlDataAdapter da = new SqlDataAdapter(query, conn))
                    {
                        conn.Open();
                        DataSet ds = new DataSet();
                        da.Fill(ds);
                        conn.Close();
                        if (siteUserData == null)
                            siteUserData = new SiteUserData(SiteID);
                        if (siteUserData.SiteID != SiteID)
                            siteUserData.LoadUserData(SiteID);
                        foreach (DataRow row in ds.Tables[0].Rows)
                        {
                            ImportData cd = new ImportData(siteUserData);
                            cd.ImportID = row["ImportID"].ToString();
                            cd.Date = row["Date"].ToString();
                            cd.ImportType = row["ImportType"].ToString();
                            cd.ClaimType = row["ClaimType"].ToString();
                            cd.User = row["LogUser"].ToString();
                            cd.Count = row["Count"].ToString();
                            importDataList.Add(cd);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Tools.ShowError(string.Format("Error getting Import Data: {0}", ex.ToString()));
            }
        }
        private string GetImportExportIDsSanitized()
        {
            string returnValue = string.Empty;
            if (textBox_ImportExport_IDs.Text.ToUpper().Contains("DROP"))
                return string.Empty;
            string[] splitValues = textBox_ImportExport_IDs.Text.Split(new char[] { ',', ';' });
            foreach (string value in splitValues)
            {
                returnValue += string.Format("'{0}',", value);
            }
            if (returnValue.EndsWith(","))
                returnValue = returnValue.Substring(0, returnValue.Length - 1);
            return returnValue;
        }
        private void button_ImportExport_ViewReport_Click(object sender, RoutedEventArgs e)
        {
            foreach (var import in dataGrid_ImportExport_Results.SelectedCells)
            {
                try
                {
                    if (import.Column.Header.ToString() == "ImportID")
                    {
                        var reportFileName = string.Empty;
                        ImportData id = (ImportData)import.Item;
                        var importID = id.ImportID;
                        string filePath = string.Format(@"C:\CustomerSS\{0}\XClaim\Reports\Import", SiteID);
                        if (!Directory.Exists(filePath)) Directory.CreateDirectory(filePath);
                        string sOutFile = string.Format(@"C:\CustomerSS\{0}\XClaim\Reports\Import\{1}_importReport.xml", SiteID, importID);

                        using (SqlConnection conn = new SqlConnection(Tools.GetPrimaryClaimDBConnectionString(SiteID)))
                        {
                            string sQuery = string.Format("SELECT RptFile FROM Imports WHERE SiteID = {0} AND ImportID = {1}", SiteID, importID);
                            using (SqlCommand cmd = new SqlCommand(sQuery, conn))
                            {
                                conn.Open();
                                object returnval = cmd.ExecuteScalar();
                                reportFileName = returnval.ToString();
                                conn.Close();
                            }
                        }

                        string siteDirectory = Tools.GetSiteLocaion(SiteID);
                        string inFilePath = siteDirectory + @"\XClaim\Reports\Import\" + reportFileName;

                        if (File.Exists(inFilePath))
                        {
                            Tools.DecompressPkFile(inFilePath, sOutFile);

                            Tools.OpenFile(sOutFile);
                        }
                        else
                        {
                            Tools.ShowError(string.Format("Couldn't find import report for import ID {0}", importID));
                        }
                    }
                }
                catch (Exception ex)
                {
                    Tools.ShowError(ex.ToString());
                }
            }
        }
        private void button_ImportExport_PopOut_Click(object sender, RoutedEventArgs e)
        {
            if (radioButton_ImportExport_Import.IsChecked == true)
            {
                if (ImportDataList == null || ImportDataList.Count <= 0)
                {
                    Tools.ShowError("No imports to show.");
                }
                else
                {
                    var x = new PopOutGrid(ImportDataList, "Import Data");
                    x.Show();
                }
            }
            else if (radioButton_ImportExport_Export.IsChecked == true)
            {
                if (ExportDataList == null || ExportDataList.Count <= 0)
                {
                    Tools.ShowError("No exports to show.");
                }
                else
                {
                    var x = new PopOutGrid(ExportDataList, "Export Data");
                    x.Show();
                }
            }
        }

        #endregion Import/Export Tab //============================================================

        #region Groups Tab //======================================================================

        private void button_Groups_Find_Click(object sender, RoutedEventArgs e)
        {
            if (siteGroupData == null) siteGroupData = new SiteGroupData(SiteID);
            if (siteGroupData.SiteID != SiteID) siteGroupData.LoadGroupData(SiteID);
            if (GroupDataGridResults != null) GroupDataGridResults = null;

            GroupDataGridResults = new List<GroupData>(siteGroupData.GroupDataList);
            for (int i = 0; i < GroupDataGridResults.Count; i++)
            {
                if (!ValidateGroup(GroupDataGridResults[i]))
                {
                    GroupDataGridResults.RemoveAt(i);
                    i--;
                }
            }

            dataGrid_Groups_Results.ItemsSource = GroupDataGridResults;
            label_Group.Content = string.Format("{0} rows returned.", GroupDataGridResults.Count);
        }
        private bool ValidateGroup(GroupData groupData)
        {
            bool returnValue_Status, returnValue_Scheduled, returnValue_App, returnValue_Name, returnValue_CLX;
            if ((checkBox_Groups_Active.IsChecked == false && checkBox_Groups_Deleted.IsChecked == false && checkBox_Groups_Disabled.IsChecked == false) ||
                (checkBox_Groups_Active.IsChecked == true && groupData.ActiveStatus == "Active") ||
                (checkBox_Groups_Deleted.IsChecked == true && groupData.ActiveStatus == "Deleted") ||
                (checkBox_Groups_Disabled.IsChecked == true && groupData.ActiveStatus == "Disabled"))
                returnValue_Status = true;
            else
                returnValue_Status = false;

            if ((checkBox_Groups_isNotScheduled.IsChecked == false && checkBox_Groups_isScheduled.IsChecked == false) || 
                (checkBox_Groups_isScheduled.IsChecked == true && groupData.Scheduled) ||
                (checkBox_Groups_isNotScheduled.IsChecked == true && !groupData.Scheduled))
                returnValue_Scheduled = true;
            else
                returnValue_Scheduled = false;

            if ((checkBox_Groups_AppXClaim.IsChecked == false && checkBox_Groups_AppXDM.IsChecked == false) ||
                (checkBox_Groups_AppXClaim.IsChecked == true && groupData.App == 1) ||
                (checkBox_Groups_AppXDM.IsChecked == true && groupData.App == 2))
                returnValue_App = true;
            else
                returnValue_App = false;

            if (textBox_Groups_NameFilter.Text == string.Empty || groupData.GroupName.ToUpper().Contains(textBox_Groups_NameFilter.Text.ToUpper()))
                returnValue_Name = true;
            else
                returnValue_Name = false;

            if (textBox_Groups_CLXFilter.Text == string.Empty || groupData.ClxData.ToUpper().Contains(textBox_Groups_CLXFilter.Text.ToUpper()))
                returnValue_CLX = true;
            else
                returnValue_CLX = false;

            return returnValue_Status && returnValue_Scheduled && returnValue_App && returnValue_Name && returnValue_CLX;
        }
        private void button_Groups_PopOut_Click(object sender, RoutedEventArgs e)
        {
            if (GroupDataGridResults == null || GroupDataGridResults.Count <= 0)
            {
                Tools.ShowError("No results to pop out.");
            }
            else
            {
                var x = new PopOutGrid(GroupDataGridResults, "Group List");
                x.Show();
            }
        }
        private void button_Groups_Clear_Click(object sender, RoutedEventArgs e)
        {
            siteGroupData = null;
            GroupDataGridResults = null;
            dataGrid_Groups_Results.ItemsSource = null;
        }

        #endregion Groups Tab //===================================================================

        #region Batching Tab //====================================================================

        private void button_Batching_Local_BConfigs_Click(object sender, RoutedEventArgs e)
        {
            Tools.OpenDirectory(@"C:\XCONNECTSS\Outgoing\Config");
        }
        private void button_Batching_Local_RConfigs_Click(object sender, RoutedEventArgs e)
        {
            Tools.OpenDirectory(@"C:\ProductionSS\Outgoing\Config\ProcessConfig ");
        }
        private void button_Batching_Local_BSchedules_Click(object sender, RoutedEventArgs e)
        {
            Tools.OpenDirectory(@"C:\XCONNECTSS\Outgoing\Batchfiles");
        }
        private void button_Batching_Server_BConfigs_Click(object sender, RoutedEventArgs e)
        {
            Tools.OpenDirectory(@"\\RCM40VPXAPAPP52\c$\XConnect\Outgoing\Config");
        }
        private void button_Batching_Server_RConfigs_Click(object sender, RoutedEventArgs e)
        {
            Tools.OpenDirectory(@"\\RCM40VPXAPAPP52\c$\XConnect\Config");
        }
        private void button_Batching_Server_BSchedules_Click(object sender, RoutedEventArgs e)
        {
            Tools.OpenDirectory(@"\\RCM40VPXAPJBS01\c$\XactiMed\bin");
        }

        #endregion //==============================================================================

        #region Menu //============================================================================

        #region Launch Tool //=====================================================================

        private void menu_Tool_ClaimConverterReRun_Click(object sender, RoutedEventArgs e)
        {
            if (SiteID != 0)
            {
                var x = new ClaimConverterReRun(SiteID.ToString());
                x.Show();
            }
            else
            {
                var x = new ClaimConverterReRun();
                x.Show();
            }
        }
        private void menu_Tool_3500ImportPostErrStat_Click(object sender, RoutedEventArgs e)
        {
            var x = new _3500_ImportPostErrStat();
            x.Show();
        }
        private void menu_Tool_3500ImportPostErrorNotes_Click(object sender, RoutedEventArgs e)
        {
            var x = new _3500_ImportPostErrorNotes();
            x.Show();
        }
        private void menu_Tool_3502PINFile_Click(object sender, RoutedEventArgs e)
        {
            Tools.OpenDirectory(@"C:\CustomerSS\3502\Provider Pin file");
            Tools.OpenDirectory(@"\\MedAssets.com\CRP\SFTP\s05980_3502\XClaim\Uploads\PromasterUpdates");
            Tools.OpenDirectory(@"\\rcm40vpxapapp70\c$\XactiMed\FileProcessingServices\FileProcessingServiceImmediate\Logs");
            //var x = new PinFile3502();
            //x.Show();
        }
        private void menu_Tool_DenialRecon_Click(object sender, RoutedEventArgs e)
        {
            var x = new DenialReconciliation();
            x.Show();
        }
        private void menu_Tool_DenialReconMulti_Click(object sender, RoutedEventArgs e)
        {
            var x = new DenialReconMulti();
            x.Show();
        }
        private void menu_Tool_Appservice_Click(object sender, RoutedEventArgs e)
        {
            if (SiteID != 0)
            {
                var x = new AppSvcTool(SiteID);
                x.Show();
            }
            else
            {
                var x = new AppSvcTool();
                x.Show();
            }
        }
        private void menu_Tool_DroppedClaims_Click(object sender, RoutedEventArgs e)
        {
            var x = new DroppedClaims();
            x.Show();
        }
        private void menu_Tool_WebLinks_Click(object sender, RoutedEventArgs e)
        {
            var x = new WebLinks();
            x.Show();
        }
        private void menu_Tool_SiteAppservice_Click(object sender, RoutedEventArgs e)
        {
            var x = new SiteAppServiceLogs(SiteID);
            x.Show();
        }
        private void menu_Tool_ListFLFs_Click(object sender, RoutedEventArgs e)
        {
            var x = new ListFLF();
            x.Show();
        }
        private void menu_Tool_ServerAppDateCheck_Click(object sender, RoutedEventArgs e)
        {
            var x = new ServerAppDateCheck();
            x.Show();
        }
        private void menu_Tool_StVincentFacilities_Click(object sender, RoutedEventArgs e)
        {
            var x = new StVincentFacilities();
            x.Show();
        }
        private void menu_Tool_SecondaryConfig_Click(object sender, RoutedEventArgs e)
        {
            if (SiteID != 0)
            {
                var x = new SecondaryConfig(SiteID);
                x.Show();
            }
            else
            {
                var x = new SecondaryConfig();
                x.Show();
            }
        }
        private void menu_Tool_SiteActivityLogs_Click(object sender, RoutedEventArgs e)
        {
            var x = new SiteActivityLogs();
            x.Show();
        }
        
        #endregion Launch Tool //==================================================================

        #region Web Link //========================================================================

        private void menu_Web_RFBFProd_Click(object sender, RoutedEventArgs e)
        {
            string targetURL = @"https://xwebapps.medassets.com/xapps.net/redfishbluefish.aspx";
            Process.Start(targetURL);
        }
        private void menu_Web_RFBF999_Click(object sender, RoutedEventArgs e)
        {
            string targetURL = @"https://xTest.MedAssets.com/xapps.net/redfishbluefish.aspx";
            Process.Start(targetURL);
        }
        private void menu_Web_RFBF888_Click(object sender, RoutedEventArgs e)
        {
            string targetURL = @"https://X5010.MedAssets.com/xapps.net/redfishbluefish.aspx";
            Process.Start(targetURL);
        }
        private void menu_Web_TeamHome_Click(object sender, RoutedEventArgs e)
        {
            string targetURL = @"http://mac.medassets.com/sites/revenuemanagement/TSS/TS/SolutionsEngineering/SEXApps/default.aspx";
            Process.Start(targetURL);
        }
        private void menu_Web_TeamViews_Click(object sender, RoutedEventArgs e)
        {
            string pathUser = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            string pathDownload = System.IO.Path.Combine(pathUser, "Downloads");
            string fileLocation = System.IO.Path.Combine(pathDownload, "TeamViews.xlsx");
                int count = 0;
            if (!File.Exists(fileLocation))
            {
                string targetURL = @"http://mac.medassets.com/sites/revenuemanagement/TSS/TS/SolutionsEngineering/SEXApps/Shared%20Documents/TeamViews.xlsx";
                Process.Start(targetURL);
                while (count < 5)
                {
                    if (File.Exists(fileLocation)) break;
                    System.Threading.Thread.Sleep(1000);
                    count++;
                }
            }
            Tools.OpenFile(fileLocation);
        }
        private void menu_Web_LocsToolsSiteOwners_Click(object sender, RoutedEventArgs e)
        {
            Tools.OpenFile(locDSiteUserXml);
        }
        private void menu_Web_XappsWiki_Click(object sender, RoutedEventArgs e)
        {
            string targetURL = @"http://rtwiki/doku.php?id=customdev:devs:kb";
            Process.Start(targetURL);
        }
        private void menu_Web_TimeCard_Click(object sender, RoutedEventArgs e)
        {
            string targetURL = @"http://timex.medassets.com/BP/Project/Project%20Center%20Pages/Time.aspx";
            Process.Start(targetURL);
        }
        private void menu_Web_SalesForce_Click(object sender, RoutedEventArgs e)
        {
            string targetURL = @"http://login.salesforce.com";
            Process.Start(targetURL);
        }
        private void menu_Web_TalentManager_Click(object sender, RoutedEventArgs e)
        {
            string targetUrl = @"http://mac.medassets.com/sites/hr/TalentManager/default.aspx";
            Process.Start(targetUrl);
        }

        #endregion Web Link //=====================================================================

        #region Launch Apps //=====================================================================

        private void menu_App_MasterTool_Click(object sender, RoutedEventArgs e)
        {
            string appDataFolder = Tools.GetAppDataFolder();
            string location = System.IO.Path.Combine(appDataFolder, @"Roaming\Microsoft\Windows\Start Menu\Programs\MedAssets\XAppsMasterTool.appref-ms");
            Process.Start(location);
        }
        private void menu_App_ProgramDeployer_Click(object sender, RoutedEventArgs e)
        {
            string appDataFolder = Tools.GetAppDataFolder();
            string location = System.IO.Path.Combine(appDataFolder, @"Roaming\Microsoft\Windows\Start Menu\Programs\MedAssets\Program Deployer.appref-ms");
            Process.Start(location);
        }
        private void menu_App_ImportFinder_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(@"C:\Tools\Import Finder\Release\ImportFinder.exe");
        }
        private void menu_App_ClaimEditor_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(@"C:\Tools\ClAIM EDITOR\ClaimCreatorEditorTester.exe");
        }
        private void menu_App_SourceSafe_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(@"C:\Program Files (x86)\Microsoft Visual Studio\VSS\win32\SSEXP.EXE");
        }

        #endregion Launch Apps //==================================================================

        #region View List //=======================================================================

        private void menu_Lists_FolderIDs_Click(object sender, RoutedEventArgs e)
        {
            List<FolderData> fd_list = new List<FolderData>();
            try
            {
                string query = "select * from Archive.dbo.FolderStatus";
                string connString = Tools.GetPrimaryClaimDBConnectionString(SiteID);
                using (SqlConnection conn = new SqlConnection(connString))
                {
                    using (SqlDataAdapter da = new SqlDataAdapter(query, conn))
                    {
                        conn.Open();
                        DataSet ds = new DataSet();
                        da.Fill(ds);
                        conn.Close();

                        foreach (DataRow row in ds.Tables[0].Rows)
                        {
                            FolderData fd = new FolderData();
                            fd.FolderID = row["FolderID"].ToString();
                            fd.Name = row["Name"].ToString();
                            fd_list.Add(fd);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Tools.ShowError(string.Format("Error getting Folders: {0}", ex.ToString()));
                return;
            }

            if (fd_list.Count > 0)
            {
                PopOutGrid grid = new PopOutGrid(fd_list, "Folders");
                grid.Show();
            }
            else
            {
                Tools.ShowMessage("Nothing found");
            }
        }
        private void menu_Lists_ImportTypes_Click(object sender, RoutedEventArgs e)
        {
            List<ImportTypeData> it_list = new List<ImportTypeData>();
            try
            {
                string query = "select * from [Primary].dbo.ImportTypes";
                string connString = Tools.GetPrimaryClaimDBConnectionString(SiteID);
                using (SqlConnection conn = new SqlConnection(connString))
                {
                    using (SqlDataAdapter da = new SqlDataAdapter(query, conn))
                    {
                        conn.Open();
                        DataSet ds = new DataSet();
                        da.Fill(ds);
                        conn.Close();

                        foreach (DataRow row in ds.Tables[0].Rows)
                        {
                            ImportTypeData itd = new ImportTypeData();
                            itd.ImportType = row["ImportType"].ToString();
                            itd.Name = row["Name"].ToString();
                            it_list.Add(itd);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Tools.ShowError(string.Format("Error getting Import Types: {0}", ex.ToString()));
                return;
            }

            if (it_list.Count > 0)
            {
                PopOutGrid grid = new PopOutGrid(it_list, "Import Types");
                grid.Show();
            }
            else
            {
                Tools.ShowMessage("Nothing found");
            }
        }
        private void menu_Lists_DenialStatus_Click(object sender, RoutedEventArgs e)
        {
            List<DenialStatus> ds_list = new List<DenialStatus>();
            try
            {
                string query = string.Format("select * from Archive.dbo.DenialStatus where siteid = {0}", SiteID.ToString());
                string connString = Tools.GetPrimaryClaimDBConnectionString(SiteID);
                using (SqlConnection conn = new SqlConnection(connString))
                {
                    using (SqlDataAdapter da = new SqlDataAdapter(query, conn))
                    {
                        conn.Open();
                        DataSet ds = new DataSet();
                        da.Fill(ds);
                        conn.Close();

                        foreach (DataRow row in ds.Tables[0].Rows)
                        {
                            DenialStatus ds_info = new DenialStatus();
                            //ds.SiteID = row["SiteID"].ToString();
                            ds_info.DenialStatusID = row["DenialStatusID"].ToString();
                            ds_info.Name = row["Name"].ToString();
                            ds_info.Description = row["Description"].ToString();
                            ds_info.DenialFolderID = row["DenialFolderID"].ToString();
                            ds_info.Options = row["Options"].ToString();
                            ds_list.Add(ds_info);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Tools.ShowError(string.Format("Error getting Denial Status Ids: {0}", ex.ToString()));
                return;
            }

            if (ds_list.Count > 0)
            {
                PopOutGrid grid = new PopOutGrid(ds_list, "Denial Status Types");
                grid.Show();
            }
            else
            {
                Tools.ShowMessage("Nothing found");
            }
        }
        private void menu_Lists_DenialActivity_Click(object sender, RoutedEventArgs e)
        {
            //todo - get this working correctly
            List<DenialStatus> ds_list = new List<DenialStatus>();
            try
            {
                string query = string.Format("select * from Archive.dbo.DenialStatus where siteid = {0}", SiteID.ToString());
                string connString = Tools.GetPrimaryClaimDBConnectionString(SiteID);
                using (SqlConnection conn = new SqlConnection(connString))
                {
                    using (SqlDataAdapter da = new SqlDataAdapter(query, conn))
                    {
                        conn.Open();
                        DataSet ds = new DataSet();
                        da.Fill(ds);
                        conn.Close();

                        foreach (DataRow row in ds.Tables[0].Rows)
                        {
                            DenialStatus ds_info = new DenialStatus();
                            //ds.SiteID = row["SiteID"].ToString();
                            ds_info.DenialStatusID = row["DenialStatusID"].ToString();
                            ds_info.Name = row["Name"].ToString();
                            ds_info.Description = row["Description"].ToString();
                            ds_info.DenialFolderID = row["DenialFolderID"].ToString();
                            ds_info.Options = row["Options"].ToString();
                            ds_list.Add(ds_info);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Tools.ShowError(string.Format("Error getting Denial Status Ids: {0}", ex.ToString()));
                return;
            }

            if (ds_list.Count > 0)
            {
                PopOutGrid grid = new PopOutGrid(ds_list, "Denial Status Types");
                grid.Show();
            }
            else
            {
                Tools.ShowMessage("Nothing found");
            }
        }

        #endregion //==============================================================================

        #region Dr Seuss //========================================================================

        private void menu_Server_Production_Click(object sender, RoutedEventArgs e)
        {
            UpdateMenuServer(MenuType.PROD);
        }
        private void menu_Server_999_Click(object sender, RoutedEventArgs e)
        {
            UpdateMenuServer(MenuType.TEST999);
        }
        private void menu_Server_888_Click(object sender, RoutedEventArgs e)
        {
            UpdateMenuServer(MenuType.TEST888);
        }
        private void menu_32_Production_Click(object sender, RoutedEventArgs e)
        {
            UpdateMenu32(MenuType.PROD);
        }
        private void menu_32_999_Click(object sender, RoutedEventArgs e)
        {
            UpdateMenu32(MenuType.TEST999);
        }
        private void menu_32_888_Click(object sender, RoutedEventArgs e)
        {
            UpdateMenu32(MenuType.TEST888);
        }
        private void menu_64_Production_Click(object sender, RoutedEventArgs e)
        {
            UpdateMenu64(MenuType.PROD);
        }
        private void menu_64_999_Click(object sender, RoutedEventArgs e)
        {
            UpdateMenu64(MenuType.TEST999);
        }
        private void menu_64_888_Click(object sender, RoutedEventArgs e)
        {
            UpdateMenu64(MenuType.TEST888);
        }
        private void menu_SyncResources_Click(object sender, RoutedEventArgs e)
        {
            SyncResources();
        }
        private void menu_EnableOneFish_Click(object sender, RoutedEventArgs e)
        {
            if (menu_EnableOneFish.IsChecked)
            {
                SetOneFish(true);
            }
            else
            {
                SetOneFish(false);
            }
        }
        private void menu_LDC_Click(object sender, RoutedEventArgs e)
        {
            if (menu_SyncResources.IsChecked)
            {
                if (menu_Server_Production.IsChecked)
                {
                    UpdateMenuServer(MenuType.PROD);
                    UpdateMenu32(MenuType.PROD);
                    UpdateMenu64(MenuType.PROD);
                }
                else if (menu_Server_999.IsChecked)
                {
                    UpdateMenuServer(MenuType.TEST999);
                    UpdateMenu32(MenuType.TEST999);
                    UpdateMenu64(MenuType.TEST999);
                }
                else if (menu_Server_888.IsChecked)
                {
                    UpdateMenuServer(MenuType.TEST888);
                    UpdateMenu32(MenuType.TEST888);
                    UpdateMenu64(MenuType.TEST888);
                }
            }
            else
            {
                if (menu_Server_Production.IsChecked)
                    UpdateMenuServer(MenuType.PROD);
                else if (menu_Server_999.IsChecked)
                    UpdateMenuServer(MenuType.TEST999);
                else if (menu_Server_888.IsChecked)
                    UpdateMenuServer(MenuType.TEST888);

                if (menu_32_Production.IsChecked)
                    UpdateMenu32(MenuType.PROD);
                else if (menu_32_999.IsChecked)
                    UpdateMenu32(MenuType.TEST999);
                else if (menu_32_888.IsChecked)
                    UpdateMenu32(MenuType.TEST888);

                if (menu_64_Production.IsChecked)
                    UpdateMenu64(MenuType.PROD);
                else if (menu_64_999.IsChecked)
                    UpdateMenu64(MenuType.TEST999);
                else if (menu_64_888.IsChecked)
                    UpdateMenu64(MenuType.TEST888);
            }
        }

        #endregion Dr Seuss //=====================================================================

        #endregion Menu //=========================================================================

        #endregion Control Methods //==============================================================

        #region Other Methods //===================================================================
        
        private void UpdateMenuServer(MenuType menuType)
        {
            if (menuType == MenuType.PROD)
            {
                UpdateServer(ProdServer, ProdConnectionString);

                menu_Server_Production.IsEnabled = false;
                menu_Server_999.IsEnabled = true;
                menu_Server_888.IsEnabled = true;

                menu_Server_Production.IsChecked = true;
                menu_Server_999.IsChecked = false;
                menu_Server_888.IsChecked = false;
            }
            else if (menuType == MenuType.TEST999)
            {
                UpdateServer(TestServer, TestConnectionString);

                menu_Server_Production.IsEnabled = true;
                menu_Server_999.IsEnabled = false;
                menu_Server_888.IsEnabled = true;

                menu_Server_Production.IsChecked = false;
                menu_Server_999.IsChecked = true;
                menu_Server_888.IsChecked = false;
            }
            else if (menuType == MenuType.TEST888)
            {
                UpdateServer(Test5010Server, Test888ConnectionString);

                menu_Server_Production.IsEnabled = true;
                menu_Server_999.IsEnabled = true;
                menu_Server_888.IsEnabled = false;

                menu_Server_Production.IsChecked = false;
                menu_Server_999.IsChecked = false;
                menu_Server_888.IsChecked = true;
            }
            UpdateSeussStatus();
        }
        private void UpdateMenu32(MenuType menuType)
        {
            if (menuType == MenuType.PROD)
            {
                Tools.SetResourceRegistry(ProdConnectionString, false);

                menu_32_Production.IsEnabled = false;
                menu_32_999.IsEnabled = true;
                menu_32_888.IsEnabled = true;

                menu_32_Production.IsChecked = true;
                menu_32_999.IsChecked = false;
                menu_32_888.IsChecked = false;
            }
            else if (menuType == MenuType.TEST999)
            {
                Tools.SetResourceRegistry(TestConnectionString, false);

                menu_32_Production.IsEnabled = true;
                menu_32_999.IsEnabled = false;
                menu_32_888.IsEnabled = true;

                menu_32_Production.IsChecked = false;
                menu_32_999.IsChecked = true;
                menu_32_888.IsChecked = false;
            }
            else if (menuType == MenuType.TEST888)
            {
                Tools.SetResourceRegistry(Test888ConnectionString, false);

                menu_32_Production.IsEnabled = true;
                menu_32_999.IsEnabled = true;
                menu_32_888.IsEnabled = false;

                menu_32_Production.IsChecked = false;
                menu_32_999.IsChecked = false;
                menu_32_888.IsChecked = true;
            }
            UpdateSeussStatus();
        }
        private void UpdateMenu64(MenuType menuType)
        {
            if (menuType == MenuType.PROD)
            {
                Tools.SetResourceRegistry(ProdConnectionString, true);

                menu_64_Production.IsEnabled = false;
                menu_64_999.IsEnabled = true;
                menu_64_888.IsEnabled = true;

                menu_64_Production.IsChecked = true;
                menu_64_999.IsChecked = false;
                menu_64_888.IsChecked = false;
            }
            else if (menuType == MenuType.TEST999)
            {
                Tools.SetResourceRegistry(TestConnectionString, true);

                menu_64_Production.IsEnabled = true;
                menu_64_999.IsEnabled = false;
                menu_64_888.IsEnabled = true;
                     
                menu_64_Production.IsChecked = false;
                menu_64_999.IsChecked = true;
                menu_64_888.IsChecked = false;
            }
            else if (menuType == MenuType.TEST888)
            {
                Tools.SetResourceRegistry(Test888ConnectionString, true);

                menu_64_Production.IsEnabled = true;
                menu_64_999.IsEnabled = true;
                menu_64_888.IsEnabled = false;
                     
                menu_64_Production.IsChecked = false;
                menu_64_999.IsChecked = false;
                menu_64_888.IsChecked = true;
            }
            UpdateSeussStatus();
        }
        private void SetInitialDrSeuss()
        {
            MenuType typeServer = MenuType.PROD;
            string server = Tools.GetCurrentServer();
            if (server == ProdServer)
                typeServer = MenuType.PROD;
            else if (server == TestServer)
                typeServer = MenuType.TEST999;
            else if (server == Test5010Server)
                typeServer = MenuType.TEST888;
            else //LDC
            {
                menu_LDC.IsChecked = true;
                if (server == ProdServer)
                    typeServer = MenuType.PROD;
                else if (server == TestServer)
                    typeServer = MenuType.TEST999;
                else if (server == Test5010Server)
                    typeServer = MenuType.TEST888;
            }

            MenuType type32 = MenuType.TEST999;
            string res32 = Tools.GetConnectionString(false);
            if (res32 == ProdConnectionString)
                type32 = MenuType.PROD;
            else if (res32 == TestConnectionString)
                type32 = MenuType.TEST999;
            else if (res32 == Test888ConnectionString)
                type32 = MenuType.TEST888;

            MenuType type64 = MenuType.TEST888;
            string res64 = Tools.GetConnectionString(true);
            if (res64 == ProdConnectionString)
                type64 = MenuType.PROD;
            else if (res64 == TestConnectionString)
                type64 = MenuType.TEST999;
            else if (res64 == Test888ConnectionString)
                type64 = MenuType.TEST888;

            if (Tools.IsOneFishTwoFishEnabled())
            {
                menu_EnableOneFish.IsChecked = true;
                checkBox_AutoLogin.IsEnabled = true;
            }
            else
            {
                menu_EnableOneFish.IsChecked = false;
                checkBox_AutoLogin.IsEnabled = false;
            }

            UpdateMenuServer(typeServer);
            UpdateMenu32(type32);
            UpdateMenu64(type64);

            if (typeServer == type32 && typeServer == type64)
            {
                menu_SyncResources.IsChecked = true;
                SyncResources();
            }
        }
        private void UpdateSeussStatus()
        {
            string status = string.Empty;
            string server = Tools.GetCurrentServer();
            server = server.Replace(@"/XApps.NET/niaGetZipPackage.aspx", string.Empty);
            server = server.Replace(@"https://", string.Empty);
            server = server.Replace(@"http://", string.Empty);

            string res32 = Tools.GetConnectionString(false);
            res32 = res32.Replace(@";database=X3Resource;Integrated Security=True", string.Empty);
            res32 = res32.Replace(@"server=", string.Empty);

            string res64 = Tools.GetConnectionString(true);
            res64 = res64.Replace(@";database=X3Resource;Integrated Security=True", string.Empty);
            res64 = res64.Replace(@"server=", string.Empty);

            status = "Server: " + server + " | " + "32 bit Resource: " + res32 + " | " + "64 bit Resource: " + res64;
            Status.Text = status;
        }
        private void SetOneFish(bool isOn)
        {
            RegistryKey key = Registry.CurrentUser.CreateSubKey(@"Software\XactiMed\XApps");
            if (isOn)
            {
                key.SetValue("AltLogin", 1);
                checkBox_AutoLogin.IsEnabled = true;
            }
            else
            {
                key.SetValue("AltLogin", 0);
                checkBox_AutoLogin.IsEnabled = false;
            }
        }
        private void SyncResources()
        {
            if (menu_SyncResources.IsChecked)
            {
                menu_32.IsEnabled = false;
                menu_64.IsEnabled = false;
            }
            else
            {
                menu_32.IsEnabled = true;
                menu_64.IsEnabled = true;
            }

            if (menu_Server_Production.IsChecked)
            {
                UpdateMenu32(MenuType.PROD);
                UpdateMenu64(MenuType.PROD);
            }
            else if (menu_Server_999.IsChecked)
            {
                UpdateMenu32(MenuType.TEST999);
                UpdateMenu64(MenuType.TEST999);
            }
            else if (menu_Server_888.IsChecked)
            {
                UpdateMenu32(MenuType.TEST888);
                UpdateMenu64(MenuType.TEST888);
            }
        }
        private void PopulateSiteIDs(string connString)
        {
            comboBox_SiteIDs.Items.Clear();
            List<string> sites = Tools.GetListOfSites();
            foreach (var site in sites)
            {
                comboBox_SiteIDs.Items.Add(site);
            }
            comboBox_SiteIDs.SelectedIndex = 0;
        }
        private ArrayList GetClaimIDs()
        {
            char[] caSeperators = { ',', ';' };
            var claimSearchText = textBox_ClaimSearch.Text;
            var claimIDs = new ArrayList(claimSearchText.Split(caSeperators, StringSplitOptions.RemoveEmptyEntries));

            // trim claim IDs
            for (int i = 0; i < claimIDs.Count; i++)
            {
                claimIDs[i] = claimIDs[i].ToString().Trim();
            }

            // remove any duplicates
            for (int i = 0; i < claimIDs.Count; i++)
            {
                for (int j = claimIDs.Count - 1; j > i; j--)
                {
                    if (claimIDs[i].ToString() == claimIDs[j].ToString())
                    {
                        claimIDs.RemoveAt(j);
                    }
                }
            }

            return claimIDs;
        }
        private void UpdateServer(string server, string connString)
        {
            string newSiteID = SiteID.ToString();
            if (SiteID.ToString().StartsWith("999") || SiteID.ToString().StartsWith("888"))
            {
                newSiteID = SiteID.ToString().Substring(3);
            }
            textBox_Resources.Text = string.Empty;

            if (server == TestServer)
                newSiteID = "999" + newSiteID;
            else if (server == Test5010Server)
                newSiteID = "888" + newSiteID;


            Tools.UpdateServer(server);
            if (menu_SyncResources.IsChecked)
            {
                Tools.SetResourceRegistry(connString);
            }

            PopulateSiteIDs(connString);
            if (comboBox_SiteIDs.Items.Contains(newSiteID))
            {
                comboBox_SiteIDs.SelectedValue = newSiteID;
            }
            else
            {
                comboBox_SiteIDs.SelectedIndex = 0;
            }

            SetResourceTextBox();
        }
        public XmlBatchFileCombo OpenCombo(string sXmbFile)
        {
            int nRetryCount = 0;
            while (nRetryCount < 5)
            {
                try
                {
                    XmlBatchFileCombo m_combo = new XmlBatchFileCombo(sXmbFile);

                    return m_combo;
                }
                catch (Exception)
                {
                    if (nRetryCount < 5)
                    {
                        nRetryCount++;
                        System.Threading.Thread.Sleep(60000);
                    }
                    else
                        throw;
                }
            }
            return null;
        }

        #endregion Other Methods //================================================================

    }

    public class ClaimData
    {
        public ClaimData() { }

        public ClaimData(SiteUserData userData, SiteGroupData groupData)
        {
            siteUserData = userData;
            siteGroupData = groupData;
        }
        
        public SiteUserData siteUserData;
        public SiteGroupData siteGroupData;
        public string Server { get; set; }
        private string folderID;
        public string FolderID
        {
            get { return folderID; } 
            set
            {
                switch (value)
                {
                    case "-2":
                        folderID = "Permanent Archive";
                        break;
                    case "-1":
                        folderID = "Permanent Deleted";
                        break;
                    case "0":
                        folderID = "Incomplete";
                        break;
                    case "1":	
                        folderID = "Complete";
                        break;
                    case "2":	
                        folderID = "Outgoing";
                        break;
                    case "3":	
                        folderID = "Deleted";
                        break;
                    case "4":	
                        folderID = "Find";
                        break;
                    case "5":	
                        folderID = "Archive";
                        break;
                    case "6":	
                        folderID = "Groups";
                        break;
                    case "7":	
                        folderID = "Reassign";
                        break;
                    case "8":	
                        folderID = "Hold";
                        break;
                    case "9":	
                        folderID = "Rejected";
                        break;
                    case "10":	
                        folderID = "EOB";
                        break;
                    case "11":	
                        folderID = "Denied";
                        break;
                    case "12":	
                        folderID = "Mock";
                        break;
                    case "13":	
                        folderID = "XDirect Staging";
                        break;
                    case "14":	
                        folderID = "XDirect Processing";
                        break;
                    case "15":	
                        folderID = "XDirect Submitted";
                        break;
                    case "18":	
                        folderID = "Eligibility Pending";
                        break;
                    case "19":	
                        folderID = "Eligibility Processing";
                        break;
                    default:
                        folderID = string.Format("{0} - UNKNOWN", value);
                        break;
                }
            }
        }
        public string ClaimID { get; set; }
        private string type;
        public string Type
        {
            get { return type; }
            set
            {
                if (value == "0")
                    type = "INST";
                else if (value == "1")
                    type = "PROF";
                else
                    type = "UNKNOWN";
            }
        }
        public string PCN { get; set; }
        public string ImportID { get; set; }
        public string ImportDate { get; set; }
        public string ExportID { get; set; }
        public string ExportDate { get; set; }
        private string user;
        public string User
        {
            get { return user; }
            set
            {
                if (siteUserData != null)
                {
                    try
                    {
                        if (value == "0")
                            user = "SYSTEM";
                        else
                            user = siteUserData.FindUserData(int.Parse(value)).UserName;
                    }
                    catch
                    {
                        user = value;
                    }
                }
                else
                {
                    user = value;
                }
            }
        }
        private string group;
        public string Group
        {
            get { return group; }
            set
            {
                if (siteGroupData != null)
                {
                    try
                    {
                        group = siteGroupData.FindGroupData(int.Parse(value)).GroupName;
                    }
                    catch
                    {
                        group = value;
                    }
                }
                else
                {
                    group = value;
                }
            }
        }
        public string ExtraChr2 { get; set; }
    }

    public class ImportData
    {
        public ImportData()
        {
            siteUserData = null;
        }
        public ImportData(SiteUserData data)
        {
            siteUserData = data;
        }
        public SiteUserData siteUserData;
        public string ImportID { get; set; }
        private string user;
        public string User
        {
            get { return user; }
            set
            {
                if (siteUserData != null)
                {
                    try
                    {
                        if (value == "0")
                            user = "SYSTEM";
                        else
                            user = siteUserData.FindUserData(int.Parse(value)).UserName;
                    }
                    catch
                    {
                        user = value;
                    }
                }
                else
                {
                    user = value;
                }
            }
        }
        public string Date { get; set; }
        private string importType;
        public string ImportType
        {
            get { return importType; }
            set
            {
                switch (value)
                {
                    case "0":  importType = "Normal"; break;
                    case "1":  importType = "Unknown"; break;
                    case "2":  importType = "Keyboard"; break;
                    case "3":  importType = "Primary Copy"; break;
                    case "4":  importType = "Archive Copy"; break;
                    case "5":  importType = "ASB"; break;
                    case "6":  importType = "Cash Receipt"; break;
                    case "7":  importType = "Payer Report"; break;
                    case "8":  importType = "ERA 2ndary"; break;
                    case "9":  importType = "ERA Denied"; break;
                    case "10": importType = "Rebill Request"; break;
                    case "11": importType = "XDirect Unarchive"; break;
                    case "12": importType = "CCA Unarchive"; break;
                    default:
                        break;
                }
            }
        }
        private string claimType;
        public string ClaimType
        {
            get { return claimType; }
            set
            {
                if (value == "0")
                    claimType = "INST";
                else if (value == "1")
                    claimType = "PROF";
                else
                    claimType = "UNKNOWN";
            }
        }
        public string Count { get; set; }
    }

    public class ExportData
    {
        public ExportData()
        {
            siteUserData = null;
        }
        public ExportData(SiteUserData data)
        {
            siteUserData = data;
        }
        public SiteUserData siteUserData;
        public string ExportID { get; set; }
        private string user;
        public string User
        {
            get { return user; }
            set
            {
                if (siteUserData != null)
                {
                    try
                    {
                        if (value == "0")
                            user = "SYSTEM";
                        else
                            user = siteUserData.FindUserData(int.Parse(value)).UserName;
                    }
                    catch
                    {
                        user = value;
                    }
                }
                else
                {
                    user = value;
                }
            }
        }
        public string Date { get; set; }
        private string exportType;
        public string ExportType
        {
            get { return exportType; }
            set
            {
                if (value == "0")
                    exportType = "Normal";
                else if (value == "1")
                    exportType = "XDirect";
                else
                    exportType = value;
            }
        }
        private string claimType;
        public string ClaimType
        {
            get { return claimType; }
            set
            {
                if (value == "0")
                    claimType = "INST";
                else if (value == "1")
                    claimType = "PROF";
                else
                    claimType = "UNKNOWN";
            }
        }
        public string Count { get; set; }
    }

    public class SiteUserData
    {
        public List<UserData> UserDataList;

        public int SiteID { get; private set; }

        public SiteUserData()
        {
            SiteID = 0;
            UserDataList = null;
        }
        public SiteUserData(int siteID)
        {
            SiteID = siteID;
            UserDataList = LoadUserData();
        }

        private List<UserData> LoadUserData()
        {
            List<UserData> userDataList = new List<UserData>();
            string query = string.Format("SELECT * FROM Users WITH (NOLOCK) WHERE SiteID = {0} ", SiteID);
            using (SqlConnection conn = new SqlConnection(Tools.GetDomainConnection(SiteID)))
            {
                using (SqlDataAdapter da = new SqlDataAdapter(query, conn))
                {
                    conn.Open();
                    DataSet ds = new DataSet();
                    da.Fill(ds);
                    conn.Close();
                    foreach (DataRow row in ds.Tables[0].Rows)
                    {
                        UserData ud = new UserData();
                        ud.SiteID = SiteID;
                        ud.UserID = int.Parse(row["UserID"].ToString());
                        ud.UserName = row["Username"].ToString();
                        string status = row["Status"].ToString();
                        if (status == "0")
                            ud.isActive = true;
                        else
                            ud.isActive = false;
                        userDataList.Add(ud);
                    }
                }
            }
            return userDataList;
        }
        public UserData FindUserData(int userID)
        {
            foreach (UserData data in UserDataList)
            {
                if (data.UserID == userID)
                    return data;
            }
            return new UserData();
        }
        public void LoadUserData(int siteID)
        {
            SiteID = siteID;
            UserDataList = LoadUserData();
        }
    }

    public class UserData
    {
        public int SiteID { get; set; }
        public int UserID { get; set; }
        public string UserName { get; set; }
        public bool isActive { get; set; }
    }

    public class SiteGroupData
    {
        public List<GroupData> GroupDataList;
        private SiteUserData siteUserData;
        public int SiteID { get; private set; }

        public SiteGroupData()
        {
            SiteID = 0;
            GroupDataList = null;
        }
        public SiteGroupData(int siteID)
        {
            SiteID = siteID;
            siteUserData = new SiteUserData(siteID);
            GroupDataList = LoadGroupData();
        }
        public SiteGroupData(int siteID, SiteUserData data)
        {
            siteUserData = data;
            SiteID = siteID;
            GroupDataList = LoadGroupData();
        }

        private List<GroupData> LoadGroupData()
        {
            List<GroupData> groupDataList = new List<GroupData>();
            string query = string.Format("SELECT * FROM Groups WITH (NOLOCK) WHERE SiteID = {0} ", SiteID);
            using (SqlConnection conn = new SqlConnection(Tools.GetDomainConnection(SiteID)))
            {
                using (SqlDataAdapter da = new SqlDataAdapter(query, conn))
                {
                    conn.Open();
                    DataSet ds = new DataSet();
                    da.Fill(ds);
                    conn.Close();
                    foreach (DataRow row in ds.Tables[0].Rows)
                    {
                        GroupData gd = new GroupData();
                        gd.SiteID = SiteID;
                        gd.GroupID = int.Parse(row["GroupID"].ToString());
                        gd.App = int.Parse(row["AppID"].ToString());
                        gd.GroupName = row["GroupName"].ToString();
                        string status = row["Status"].ToString();
                        if (status == "0")
                            gd.ActiveStatus = "Active";
                        else if (status == "-1")
                            gd.ActiveStatus = "Disabled";
                        else if (status == "-100")
                            gd.ActiveStatus = "Deleted";
                        else
                            gd.ActiveStatus = status;

                        if (row["Schedule"].ToString() == "False")
                            gd.Scheduled = false;
                        else
                            gd.Scheduled = true;

                        string supervisor = row["Supervisor"].ToString();
                        int nSupervisor = 0;
                        if (int.TryParse(supervisor, out nSupervisor))
                        {
                            gd.PrimarySupervisor = siteUserData.FindUserData(nSupervisor).UserName;
                        }
                        else
                        {
                            gd.PrimarySupervisor = supervisor;
                        }
                        gd.ClxData = row["XmlData"].ToString();
                        groupDataList.Add(gd);
                    }
                }
            }
            return groupDataList;
        }
        public GroupData FindGroupData(int groupID)
        {
            foreach (GroupData data in GroupDataList)
            {
                if (data.GroupID == groupID)
                    return data;
            }
            return new GroupData();
        }
        public void LoadGroupData(int siteID)
        {
            SiteID = siteID;
            GroupDataList = LoadGroupData();
        }
    }

    public class GroupData
    {
        public int SiteID { get; set; }
        public int GroupID { get; set; }
        public int App { get; set; }
        public string GroupName { get; set; }
        public string ActiveStatus { get; set; }
        public bool Scheduled { get; set; }
        public string PrimarySupervisor { get; set; }
        public string ClxData { get; set; }
    }

    public class FolderData
    {
        public string FolderID { get; set; }
        public string Name { get; set; }
    }

    public class ImportTypeData
    {
        public string ImportType { get; set; }
        public string Name { get; set; }
    }

    public class DenialStatus
    {
        //public string SiteID { get; set; }
        public string DenialStatusID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string DenialFolderID { get; set; }
        public string Options { get; set; }
    }

    public class AppInfo
    {
        public AppInfo() { }
        public AppInfo(string fileName, string claimType, string runLocation, string appName, string serverLocation, string localCodeLocation)
        {
            SetInfo(fileName, claimType, runLocation, appName, serverLocation, localCodeLocation);
        }

        public string AppName { get; set; }
        public string FileName { get; set; }
        public string RunLocation { get; set; }
        public string ClaimType { get; set; }
        public string ServerLocation { private get; set; }
        public string LocalCodeLocation { private get; set; }

        public void SetInfo(string fileName, string claimType, string runLocation, string appName, string serverLocation, string localCodeLocation)
        {
            this.FileName = fileName;
            this.ClaimType = claimType;
            this.RunLocation = runLocation;
            this.AppName = appName;
            this.ServerLocation = serverLocation;
            this.LocalCodeLocation = localCodeLocation;
        }
        public string GetServerLocation()
        {
            return ServerLocation;
        }
        public string GetLocalLocation()
        {
            return LocalCodeLocation;
        }

        public override bool Equals(object obj)
        {
            if (obj == null) return false;

            AppInfo ai = obj as AppInfo;
            if ((object)ai == null) return false;

            return (FileName == ai.FileName && AppName == ai.AppName && ClaimType == ai.ClaimType);
        }
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }

    public class ClaimNote
    {
        public ClaimNote() { }
        public ClaimNote(string type, string userName, string date, string note)
        {
            Type = type;
            UserName = userName;
            Date = date;
            Note = note;
        }

        public string Type { get; set; }
        public string UserName { get; set; }
        public string Date { get; set; }
        public string Note { get; set; }
    }

    public class Diffs
    {
        public Diffs() { }

        public Diffs(XmlNode diffnode)
        {
            UserName = EzXml.GetAttributeValue(diffnode, "UserName");
            Date = EzXml.GetAttributeValue(diffnode, "Date");
            XmlNodeList subDiffs = diffnode.SelectNodes("Diff");
            foreach (XmlNode diff in subDiffs)
            {
                Diff d = new Diff(diff);
                changes.Add(d);
            }
        }

        string UserName { get; set; }
        string Date { get; set; }
        List<Diff> changes = new List<Diff>();

        public override string ToString()
        {
            string retval = string.Format("User {0} made the following changes on {1}:", UserName, Date);
            TabIndex.index++;
            foreach (var change in changes)
            {
                retval += "\r\n";//\t" + change.ToString();
                for (int i = 0; i < TabIndex.index; i++)
                {
                    retval += "\t";
                }
                retval += change.ToString();
            }
            TabIndex.index--;
            return retval;
        }
    }

    public class Diff
    {
        public Diff() { }

        public Diff(XmlNode diffnode)
        {
            string operation = EzXml.GetAttributeValue(diffnode, "Op");
            switch (operation)
            {
                case "Same":
                    Operation = DiffOps.Same;
                    break;
                case "Change":
                    Operation = DiffOps.Change;
                    break;
                case "Remove":
                    Operation = DiffOps.Remove;
                    break;
                case "Insert":
                    Operation = DiffOps.Insert;
                    break;
                default:
                    break;
            }
            Name = EzXml.GetAttributeValue(diffnode, "Name");
            OldValue = EzXml.GetAttributeValue(diffnode, "Old");
            NewValue = EzXml.GetAttributeValue(diffnode, "New");

            XmlNodeList subDiffs = diffnode.SelectNodes("Diff");
            foreach (XmlNode diff in subDiffs)
            {
                Diff d = new Diff(diff);
                changes.Add(d);
            }
        }

        DiffOps Operation { get; set; }
        string Name { get; set; }
        string OldValue { get; set; }
        string NewValue { get; set; }
        List<Diff> changes = new List<Diff>();

        public override string ToString()
        {
            string retvalue = string.Empty;
            switch (Operation)
            {
                case DiffOps.Same:
                    if (string.IsNullOrEmpty(OldValue) && string.IsNullOrEmpty(NewValue))
                    {
                        retvalue += string.Format("In section: {0}", Name);
                    }
                    else
                    {
                        retvalue += string.Format("{0} = \"{1}\"", Name, OldValue);
                    }
                    break;
                case DiffOps.Change:
                    retvalue += string.Format("{0} changed from \"{1}\" to \"{2}\"", Name, OldValue, NewValue);
                    break;
                case DiffOps.Remove:
                    retvalue += string.Format("Removed {0}, value \"{1}\"", Name, OldValue);
                    break;
                case DiffOps.Insert:
                    retvalue += string.Format("Added {0}, value \"{1}\"", Name, NewValue);
                    break;
                default:
                    break;
            }

            TabIndex.index++;
            foreach (var change in changes)
            {
                retvalue += "\r\n";//\t" + change.ToString();
                for (int i = 0; i < TabIndex.index; i++)
                {
                    retvalue += "\t";
                }
                retvalue += change.ToString();
            }
            TabIndex.index--;

            return retvalue;
        }
    }

    public enum DiffOps {Same, Change, Remove, Insert}

    public static class TabIndex
    {
        public static int index = 0;
    }
}
