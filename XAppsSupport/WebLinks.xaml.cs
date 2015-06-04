using System;
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
using System.Diagnostics;
using System.Data.SqlClient;
using System.Collections;
using System.IO;

namespace XAppsSupport
{
    /// <summary>
    /// Interaction logic for WebLinks.xaml
    /// </summary>
    public partial class WebLinks : Window
    {
        #region Constructor //=====================================================================

        public WebLinks()
        {
            InitializeComponent();
        }

        #endregion Constructor //==================================================================

        #region Control Methods //=================================================================

        private void button_SalesForce_Click(object sender, RoutedEventArgs e)
        {
            string targetURL = @"http://login.salesforce.com";
            Process.Start(targetURL);
        }

        private void button_Home_Click(object sender, RoutedEventArgs e)
        {
            string targetURL = @"http://mac.medassets.com/sites/revenuemanagement/TSS/TS/PS/Xapps/default.aspx";
            Process.Start(targetURL);
        }

        private void button_Prod_Click(object sender, RoutedEventArgs e)
        {
            string targetURL = @"https://webapps.xactimed.com/xapps.net/redfishbluefish.aspx";
            Process.Start(targetURL);
        }

        private void button_Test_Click(object sender, RoutedEventArgs e)
        {
            string targetURL = @"https://test.xactimed.com/xapps.net/redfishbluefish.aspx";
            Process.Start(targetURL);
        }

        private void button_5010_Click(object sender, RoutedEventArgs e)
        {
            string targetURL = @"https://5010.xactimed.com/xapps.net/redfishbluefish.aspx";
            Process.Start(targetURL);
        }

        private void button_TimeCard_Click(object sender, RoutedEventArgs e)
        {
            string targetURL = @"http://timex.medassets.com/BP/Project/Project%20Center%20Pages/Time.aspx";
            Process.Start(targetURL);
        }

        private void button_XappsWiki_Click(object sender, RoutedEventArgs e)
        {
            string targetURL = @"http://rtwiki/doku.php?id=customdev:devs:kb";
            Process.Start(targetURL);
        }

        private void button_TIDB_Click(object sender, RoutedEventArgs e)
        {
            string targetURL = @"http://tidb.medassets.com/CustomerDB/(S(en0g4ayhr3fzadrfh1cphnis))/Default.aspx";
            Process.Start(targetURL);
        }

        private void button_Webex_Click(object sender, RoutedEventArgs e)
        {
            string targetURL = @"https://medassets.webex.com/mw0306ld/mywebex/default.do?siteurl=medassets";
            Process.Start(targetURL);
        }

        private void button_MasterBuildSvc_Click(object sender, RoutedEventArgs e)
        {
            string sDir = @"\\xmd23pptask01\xactimed\bin\MasterBuildService\Logs";
            Tools.OpenDirectory(sDir);
        }

        private void button_ProMasterBuildSvc_Click(object sender, RoutedEventArgs e)
        {
            string sDir = @"\\xmd23pptask01\xactimed\bin\ProMasterBuildSvc\Logs";
            Tools.OpenDirectory(sDir);
        }

        private void button_ProPayerBuildSvc_Click(object sender, RoutedEventArgs e)
        {
            string sDir = @"\\xmd23pptask01\xactimed\bin\ProPayerBuildSvc\Logs";
            Tools.OpenDirectory(sDir);
        }

        private void button_Open_Click(object sender, RoutedEventArgs e)
        {
            if (checkBox_File.IsChecked == true)
            {
                // have the file to convert
                // SiteID == InFile (required)
                // ImportID == OutFile (optional)

                string sInFile = textBox_SiteID.Text;
                string sOutFile = textBox_ImportID.Text;

                if (sInFile == string.Empty)
                {
                    Tools.ShowError("You must enter an input file location.");
                    return;
                }

                if (sOutFile == string.Empty)
                {
                    sOutFile = @"C:\output.xml";
                }

                Tools.DecompressPkFile(sInFile, sOutFile);

                Tools.OpenFile(sOutFile);
            }
            else
            {
                // need to find the file based on site and import ID
                var reportFileName = string.Empty;
                var siteID = int.Parse(textBox_SiteID.Text);
                var importID = textBox_ImportID.Text;
                string sOutFile = @"C:\output.xml";

                string sConnectionString = Tools.GetResource(siteID, 1, 0, 1);

                using (SqlConnection conn = new SqlConnection(sConnectionString))
                {
                    string sQuery = string.Format("SELECT RptFile FROM Imports WHERE SiteID = {0} AND ImportID = {1}", siteID, importID);
                    using (SqlCommand cmd = new SqlCommand(sQuery, conn))
                    {
                        conn.Open();
                        object returnval = cmd.ExecuteScalar();
                        reportFileName = returnval.ToString();
                        conn.Close();
                    }
                }

                string siteDirectory = Tools.GetResource(siteID, 0, 5, 4);
                string inFilePath = siteDirectory + @"\XClaim\Reports\Import\" + reportFileName;

                Tools.DecompressPkFile(inFilePath, sOutFile);

                Tools.OpenFile(sOutFile);
            }
        }

        private void checkBox_File_Checked(object sender, RoutedEventArgs e)
        {
            textBox_ImportID.Text = string.Empty;
            textBox_SiteID.Text = string.Empty;
            label1.Content = "InFile";
            label2.Content = "OutFile";
            textBox_ImportID.Text = @"C:\output.xml";
        }

        private void checkBox_File_Unchecked(object sender, RoutedEventArgs e)
        {
            textBox_ImportID.Text = string.Empty;
            textBox_SiteID.Text = string.Empty;
            label1.Content = "Site ID";
            label2.Content = "Import ID";
        }

        #endregion Control Methods //==============================================================

        private void button_FindTaskServers_Click(object sender, RoutedEventArgs e)
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

        private ArrayList GetSiteList()
        {
            char[] caSeperators = { ',', ';' };
            var sList = textBox_TaskServerSites.Text;
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
    }
}
