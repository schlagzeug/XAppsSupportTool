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

namespace XAppsSupport
{
    /// <summary>
    /// Interaction logic for SiteAppServiceLogs.xaml
    /// </summary>
    public partial class SiteAppServiceLogs : Window
    {
        private DateTime fromDate = StartOfDay(DateTime.Today);
        private DateTime thruDate = EndOfDay(DateTime.Today);
        private string[] logTypes = { "All Logs", "ReassignTask", "SchedulerTask", "StatusUpdate", "UnarchiveClaims", "XClaimExport", "XCLSImportAny", "XCLSImporter", "XCLSImport-" };

        public SiteAppServiceLogs()
        {
            InitializeComponent();
            SetupForm(0);
        }

        public SiteAppServiceLogs(int siteID)
        {
            InitializeComponent();
            SetupForm(siteID);
        }

        private void SetupForm(int siteID)
        {
            textBox_SiteID.Text = siteID.ToString();
            foreach (string logType in logTypes)
            {
                comboBox_LogTypes.Items.Add(logType);
            }
            comboBox_LogTypes.SelectedIndex = 0;
            radioButton_ByDate.IsChecked = true;
            datePicker_From.SelectedDate = DateTime.Today;
            datePicker_To.SelectedDate = DateTime.Today;
        }

        public int SiteID
        {
            get
            {
                try
                {
                    return int.Parse(textBox_SiteID.Text);
                }
                catch
                {
                    Tools.ShowError("Listed Site ID isn't valid.");
                    return 0;
                }
            }
        }

        private void radioButton_All_Checked(object sender, RoutedEventArgs e)
        {
            datePicker_To.IsEnabled = false;
            datePicker_From.IsEnabled = false;
        }

        private void radioButton_ByDate_Checked(object sender, RoutedEventArgs e)
        {
            datePicker_To.IsEnabled = true;
            datePicker_From.IsEnabled = true;
        }

        private void button_Refresh_Click(object sender, RoutedEventArgs e)
        {
            ShowLogs();
        }

        private void ShowLogs()
        {
            string logPath = Tools.GetLogLocation(SiteID) + @"\Logs\XactiMed.XApps.XClaim.AppSvc\";
            DirectoryInfo di = new DirectoryInfo(logPath);
            string searchPattern = string.Empty;
            if (comboBox_LogTypes.SelectedIndex == 0)
                searchPattern = "*.*";
            else
                searchPattern = comboBox_LogTypes.SelectedItem.ToString() + "*.*";
            List<FileInfo> fileList = di.GetFiles(searchPattern).OrderBy(f => f.LastWriteTime).ToList();
            
            if (radioButton_ByDate.IsChecked == true)
            {
                for (int i = fileList.Count - 1; i >= 0; i--)
                {
                    if (fileList[i].CreationTime < fromDate || fileList[i].CreationTime > thruDate)
                        fileList.RemoveAt(i);
                }
            }
            dataGrid_Logs.ItemsSource = fileList;
        }

        private void button_OpenSelected_Click(object sender, RoutedEventArgs e)
        {
            string logPath = Tools.GetLogLocation(SiteID) + @"\Logs\XactiMed.XApps.XClaim.AppSvc\";
            foreach (var file in dataGrid_Logs.SelectedCells)
            {
                Tools.OpenFile(logPath + file.Item.ToString());
            }
        }

        private void datePicker_From_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            fromDate = StartOfDay(datePicker_From.SelectedDate.Value);
            //ShowLogs();
        }

        private static DateTime EndOfDay(DateTime date)
        {
            return new DateTime(date.Year, date.Month, date.Day, 23, 59, 59, 999);
        }

        private static DateTime StartOfDay(DateTime date)
        {
            return new DateTime(date.Year, date.Month, date.Day, 0, 0, 0, 0);
        }

        private void datePicker_To_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            thruDate = EndOfDay(datePicker_To.SelectedDate.Value);
            //ShowLogs();
        }

        private void comboBox_LogTypes_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
           //ShowLogs();
        }
    }
}
