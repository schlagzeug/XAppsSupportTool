using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Xml;
using System.Xml.Linq;

namespace XAppsSupport
{
    /// <summary>
    /// Interaction logic for SecondaryConfig.xaml
    /// </summary>
    public partial class SecondaryConfig : Window
    {
        public SecondaryConfig()
        {
            InitializeComponent();
            Setup(string.Empty);
        }

        public SecondaryConfig(int siteID)
        {
            InitializeComponent();
            Setup(siteID.ToString());
        }

        public void Setup(string site)
        {
            PopulateSiteIDs();
            comboBox_SiteIDs.Focus();
            if (site != string.Empty)
            {
                comboBox_SiteIDs.SelectedValue = site.ToString();
                ProcessFile();
            }
        }

        private void PopulateSiteIDs()
        {
            comboBox_SiteIDs.Items.Clear();
            List<string> sites = Tools.GetListOfSites();
            foreach (var site in sites)
            {
                comboBox_SiteIDs.Items.Add(site);
            }
            comboBox_SiteIDs.SelectedIndex = 0;
        }

        private void button_Show_Click(object sender, RoutedEventArgs e)
        {
            ProcessFile();
        }

        private void ProcessFile()
        {
            string configPath = string.Format(@"C:\CustomerSS\{0}\XClaim\SecondaryConfig.xml", comboBox_SiteIDs.SelectedItem.ToString());
            if (!File.Exists(configPath))
            {
                Tools.ShowError(string.Format("File: {0} not found.", configPath));
                return;
            }
            BuildTree(treeView_Results, XDocument.Load(configPath));



            //TreeViewItem t = new TreeViewItem();
            //t.Header = "TopTest";
            //TreeViewItem subt = new TreeViewItem();
            //subt.Header = "SubTest";
            //subt.Items.Add(new TreeViewItem() { Header = "subsub" });
            //t.Items.Add(subt);
            //treeView_Results.Items.Add(t);
        }
        private void BuildTree(TreeView treeView, XDocument doc)
        {
            TreeViewItem treeNode = new TreeViewItem();
            treeNode.Header = doc.Root.Name.LocalName;
            treeNode.IsExpanded = true;
            treeView.Items.Add(treeNode);
            BuildNodes(treeNode, doc.Root);
        }



        private void BuildNodes(TreeViewItem treeNode, XElement element)
        {
            foreach (XNode child in element.Nodes())
            {
                switch (child.NodeType)
                {
                    case XmlNodeType.Element:
                        XElement childElement = child as XElement;
                        TreeViewItem childTreeNode = new TreeViewItem();
                        childTreeNode.Header = childElement.Name.LocalName;
                        childTreeNode.IsExpanded = true;
                        treeNode.Items.Add(childTreeNode);
                        BuildNodes(childTreeNode, childElement);
                        break;
                    case XmlNodeType.Text:
                        XText childText = child as XText;
                        treeNode.Items.Add(new TreeViewItem() { Header = childText.Value });
                        break;
                }
            }
        }
        //private void BuildTree(TreeView treeView, XDocument doc)
        //{
        //    TreeViewItem treeNode = new TreeViewItem
        //    {
        //        //Should be Root
        //        Header = doc.Root.Name.LocalName,
        //        IsExpanded = true
        //    };
        //    treeView.Items.Add(treeNode);
        //    BuildNodes(treeNode, doc.Root);
        //}

        //private void BuildNodes(TreeViewItem treeNode, XElement element)
        //{
        //    foreach (XNode child in element.Nodes())
        //    {
        //        switch (child.NodeType)
        //        {
        //            case XmlNodeType.Element:
        //                XElement childElement = child as XElement;
        //                string test = childElement.Attributes().First(s => s.Name == "value").Value;
        //                TreeViewItem childTreeNode = new TreeViewItem
        //                {
        //                    //Get First attribute where it is equal to value
        //                    Header = childElement.Attributes().First(s => s.Name == "value").Value,
        //                    //Automatically expand elements
        //                    IsExpanded = true
        //                };
        //                treeNode.Items.Add(childTreeNode);
        //                BuildNodes(childTreeNode, childElement);
        //                break;
        //            case XmlNodeType.Text:
        //                XText childText = child as XText;
        //                treeNode.Items.Add(new TreeViewItem { Header = childText.Value, });
        //                break;
        //        }
        //    }
        //}

        private void button_ViewFile_Click(object sender, RoutedEventArgs e)
        {
            string configPath = string.Format(@"C:\CustomerSS\{0}\XClaim\SecondaryConfig.xml", comboBox_SiteIDs.SelectedItem.ToString());
            if (!File.Exists(configPath))
            {
                Tools.ShowError(string.Format("File: {0} not found.", configPath));
                return;
            }
            Tools.OpenFile(configPath);
        }
    }
}
