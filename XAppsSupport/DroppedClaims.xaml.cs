using System;
using System.Collections.Generic;
using System.Collections;
using System.IO;
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
    /// Interaction logic for DroppedClaims.xaml
    /// </summary>
    public partial class DroppedClaims : Window
    {
        //======================================================================
        #region Fields
        //======================================================================

        public const string hcfaPath = @"\XClaim\HCFA1500\Import\UnarcErr";
        public const string ubPath = @"\XClaim\UB92\Import\UnarcErr";

        //======================================================================
        #endregion Fields
        //======================================================================

        //======================================================================
        #region Constructor
        //======================================================================

        public DroppedClaims()
        {
            InitializeComponent();
        }

        //======================================================================
        #endregion Constructor
        //======================================================================

        //======================================================================
        #region Control Methods
        //======================================================================

        private void button_FindDroppedClaims_Click(object sender, RoutedEventArgs e)
        {
            listBox_Results.Items.Clear();
            ArrayList sites = GetSiteList();
            foreach (string site in sites)
            {
                FindDroppedClaims(site);
            }
        }

        private void button_GenerateLog_Click(object sender, RoutedEventArgs e)
        {
            StreamWriter fileWriter = new StreamWriter(@"\DroppedClaims.txt");
            foreach (string line in listBox_Results.Items)
            {
                if (checkBox_noFiles.IsChecked == true && line.Contains("There are 0 files"))
                    continue;
                if (checkBox_noFolders.IsChecked == true && line.Contains("folder doesn't exist"))
                    continue;
                fileWriter.WriteLine(line);
            }
            fileWriter.Flush();
            fileWriter.Close();
            Tools.ShowMessage("Data exported to log.");
        }

        private void button_OpenLog_Click(object sender, RoutedEventArgs e)
        {
            Tools.OpenFile(@"\DroppedClaims.txt");
        }

        //======================================================================
        #endregion Control Methods
        //======================================================================

        //======================================================================
        #region Other Methods
        //======================================================================

        private void FindDroppedClaims(string siteID)
        {
            string siteUBPath = Tools.GetSiteLocaion(int.Parse(siteID)) + ubPath;
            string siteHCFAPath = Tools.GetSiteLocaion(int.Parse(siteID)) + hcfaPath;

            if (Directory.Exists(siteUBPath))
            {
                listBox_Results.Items.Add(string.Format("There are {0} files in UB folder for site {1}", ShowFiles(siteUBPath), siteID));
            }
            else
            {
                listBox_Results.Items.Add(string.Format("The UB folder doesn't exist for site {0}", siteID));
            }

            if (Directory.Exists(siteHCFAPath))
            {
                listBox_Results.Items.Add(string.Format("There are {0} files in HCFA folder for site {1}", ShowFiles(siteHCFAPath), siteID));
            }
            else
            {
                listBox_Results.Items.Add(string.Format("The HCFA folder doesn't exist for site {0}", siteID));
            }
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

        private int ShowFiles(string path)
        {
            // eventually want to update this to show the file names in the output box
            var di = new DirectoryInfo(path);

            var files = di.GetFiles();

            return files.Length;
        }

        //======================================================================
        #endregion Other Methods
        //======================================================================
    }
}
