using System;
using System.Collections.Generic;
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
using System.Xml;
using XactiMed;

namespace XAppsSupport
{
    /// <summary>
    /// Interaction logic for ServerAppDateCheck.xaml
    /// </summary>
    public partial class ServerAppDateCheck : Window
    {
        public string deployerConfig = string.Empty;
        public ServerAppDateCheck()
        {
            InitializeComponent();
            LoadTypes();
        }

        private void LoadTypes()
        {
            XmlDocument doc = new XmlDocument();
            deployerConfig = GetMostRecentConfig();
            if (deployerConfig == string.Empty)
                Tools.ShowError("Couldn't find Deployer config");
            else
            {
                doc.Load(deployerConfig);
                XmlNodeList entries = doc.SelectNodes("/DEPLOYERCFG/ENTRY");
                foreach (XmlNode entry in entries)
                {
                    string type = EzXml.GetStringEz(entry, "TYPE", string.Empty);
                    if (type != string.Empty && type != "Manual")
                        comboBox_Types.Items.Add(type);
                }
            }
        }

        private string GetMostRecentConfig()
        {
            string appDataFolder = Tools.GetAppDataFolder();
            string startLocation = appDataFolder + @"\Local\Apps\";
            string[] files = Directory.GetFiles(startLocation, "DeployerConfig.xml", SearchOption.AllDirectories);
            if (files.Length > 0)
            {
                FileInfo bestFile = new FileInfo(files[0]);
                foreach (string file in files)
                {
                    FileInfo fi = new FileInfo(file);
                    if (fi.LastWriteTime > bestFile.LastWriteTime)
                        bestFile = fi;
                }
                return bestFile.FullName;
            }
            else
            {
                return string.Empty;
            }
        }

        private void button_Check_Click(object sender, RoutedEventArgs e)
        {
            if (textBox_ProgramName.Text == string.Empty)
            {
                Tools.ShowError("No Program Name entered");
                return;
            }
            string fileToCheck = GetFileToCheck();
            string[] servers = GetServerList(comboBox_Types.SelectedItem.ToString());
            List<ServerFileInfo> resultList = new List<ServerFileInfo>();
            string source = GetSourceDirectory(comboBox_Types.SelectedItem.ToString());
            resultList.Add(new ServerFileInfo("Source", GetTimeStamp(source), source));
            foreach (string server in servers)
            {
                string file = System.IO.Path.Combine(@"\\", server.Trim(), fileToCheck);
                resultList.Add(new ServerFileInfo(server.Trim(), GetTimeStamp(file), file));
            }
            dataGrid_Results.ItemsSource = resultList;
        }

        private string GetFileToCheck()
        {
            if (comboBox_Types.SelectedItem.ToString().Contains("BridgeRoutines") || comboBox_Types.SelectedItem.ToString().Contains("XDenial.Prep"))
            {
                string siteID = textBox_ProgramName.Text.Replace("BridgeRoutines_", string.Empty).Replace(".BridgeRoutines", string.Empty);
                return string.Format(@"Xactimed\bin\{0}\{1}\{2}.dll", comboBox_Types.SelectedItem.ToString(),siteID, textBox_ProgramName.Text);
            }
            else
            {
                return string.Format(@"Xactimed\bin\{0}\{1}\{1}.exe", comboBox_Types.SelectedItem.ToString(), textBox_ProgramName.Text);
            }
        }

        private DateTime GetTimeStamp(string source)
        {
            FileInfo fi = new FileInfo(source);
            if (fi.Exists)
            {
                return fi.LastWriteTime;
            }
            else return DateTime.MinValue;
        }

        private string GetSourceDirectory(string p)
        {
            if (deployerConfig == string.Empty) deployerConfig = GetMostRecentConfig();
            XmlDocument doc = new XmlDocument();
            deployerConfig = GetMostRecentConfig();
            doc.Load(deployerConfig);
            XmlNodeList entries = doc.SelectNodes("/DEPLOYERCFG/ENTRY");
            foreach (XmlNode entry in entries)
            {
                if (EzXml.GetStringEz(entry, "TYPE", string.Empty) == p)
                {
                    if (comboBox_Types.SelectedItem.ToString().Contains("BridgeRoutines") || comboBox_Types.SelectedItem.ToString().Contains("XDenial.Prep"))
                    {
                        return EzXml.GetStringEz(entry, "SOURCE", string.Empty) + string.Format(@"\{0}\bin\x86\Release\{0}.dll", textBox_ProgramName.Text);
                    }
                    else
                    {
                        return EzXml.GetStringEz(entry, "SOURCE", string.Empty) + string.Format(@"\{0}\bin\x86\Release\{0}.exe", textBox_ProgramName.Text);
                    }
                }
            }
            return string.Empty;
        }

        private string[] GetServerList(string p)
        {
            if (deployerConfig == string.Empty) deployerConfig = GetMostRecentConfig();
            XmlDocument doc = new XmlDocument();
            deployerConfig = GetMostRecentConfig();
            doc.Load(deployerConfig);
            XmlNodeList entries = doc.SelectNodes("/DEPLOYERCFG/ENTRY");
            foreach (XmlNode  entry in entries)
            {
                if (EzXml.GetStringEz(entry, "TYPE", string.Empty) == p)
                {
                    return EzXml.GetStringEz(entry, "SERVERS", string.Empty).Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                }
            }
            return new string[] { };
        }
    }

    public class ServerFileInfo
    {
        public ServerFileInfo() { }
        public ServerFileInfo(string l, DateTime d, string p)
        {
            Location = l;
            TimeStamp = d;
            Path = p;
        }
        public string Location { get; set; }
        public DateTime TimeStamp { get; set; }
        public string Path { get; set; }
    }
}
