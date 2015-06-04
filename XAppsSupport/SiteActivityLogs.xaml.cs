using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
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
    /// Interaction logic for SiteActivityLogs.xaml
    /// </summary>
    public partial class SiteActivityLogs : Window
    {
        Dictionary<int, string> LogDescriptions = null;
        SiteUserData UserData = null;
        string defaultMaxRows = "100";

        public int SiteID
        {
            get
            {
                return int.Parse(comboBox_SiteIDs.SelectedItem.ToString());
            }
        }
        public int MaxRows
        {
            get
            {
                int result = -1;
                if (int.TryParse(textBox_MaxRows.Text, out result))
                {
                    return result;
                }
                else
                {
                    Tools.ShowError(string.Format("Max rows must be an integer. Max rows reset to {0}", defaultMaxRows));
                    textBox_MaxRows.Text = defaultMaxRows;
                    return int.Parse(defaultMaxRows);
                }
            }
        }

        public SiteActivityLogs()
        {
            InitializeComponent();
            textBox_Database.IsEnabled = false;
            List<string> siteIds = Tools.GetListOfSites();

            foreach (var site in siteIds)
            {
                comboBox_SiteIDs.Items.Add(site);
            }

            datePicker_From.SelectedDate = DateTime.Today;
            datePicker_To.SelectedDate = DateTime.Today;
            radioButton_DateALL.IsChecked = true;
            button_RunQuery.IsEnabled = false;
            textBox_MaxRows.Text = defaultMaxRows;
            //comboBox_SiteIDs.Focus();
        }

        private void comboBox_SiteIDs_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ClearLists();
            string connectionString = Tools.GetActivityLogDBConnectionString(SiteID);
            textBox_Database.Text = GetLogDatabase(connectionString);
            PopulateUserList();
            PopulateLogDescList();
            PopulateSupportUserList();
            button_RunQuery.IsEnabled = true;
            tabControl_Main.SelectedIndex = 0; //query tab
            dataGrid_Results.ItemsSource = null;
            label_Status.Content = "Select a SiteID to begin";
            label_SiteName.Content = Tools.GetSiteName(SiteID);
        }

        private void PopulateSupportUserList()
        {
            string query = string.Format("SELECT distinct SupportName FROM Logs WHERE SiteID = {0}", SiteID.ToString());
            using (SqlConnection conn = new SqlConnection(Tools.GetActivityLogDBConnectionString(SiteID)))
            {
                using (SqlDataAdapter da = new SqlDataAdapter(query, conn))
                {
                    conn.Open();
                    DataSet ds = new DataSet();
                    da.Fill(ds);
                    conn.Close();
                    foreach (DataRow row in ds.Tables[0].Rows)
                    {
                        string su = row["SupportName"].ToString();
                        if (su.Trim() != string.Empty)
                            listBox_SupportUserAvailable.Items.Add(su);
                    }
                }
            }
            SortList(listBox_SupportUserAvailable.Items);
        }

        private void ClearLists()
        {
            listBox_UsersAvailable.Items.Clear();
            listBox_UsersSelected.Items.Clear();
            listBox_LogAvailable.Items.Clear();
            listBox_LogSelected.Items.Clear();
            listBox_SupportUserAvailable.Items.Clear();
            listBox_SupportUserSelected.Items.Clear();
        }

        private void PopulateLogDescList()
        {
            LogDescriptions = new Dictionary<int, string>();
            string query = "SELECT * FROM LogIDs";
            using (SqlConnection conn = new SqlConnection(Tools.GetActivityLogDBConnectionString(SiteID)))
            {
                using (SqlDataAdapter da = new SqlDataAdapter(query, conn))
                {
                    conn.Open();
                    DataSet ds = new DataSet();
                    da.Fill(ds);
                    conn.Close();
                    foreach (DataRow row in ds.Tables[0].Rows)
                    {
                        int logId = int.Parse(row["LogID"].ToString());
                        string desc = row["Desc"].ToString();
                        LogDescriptions.Add(logId, desc);
                    }
                }
            }

            foreach (var log in LogDescriptions)
            {
                listBox_LogAvailable.Items.Add(log.Value);
            }
            SortList(listBox_LogAvailable.Items);
        }

        private void PopulateUserList()
        {
            UserData = new SiteUserData(SiteID);
            foreach (var user in UserData.UserDataList)
            {
                listBox_UsersAvailable.Items.Add(string.Format("{0} ({1})", user.UserName.ToString(), user.UserID.ToString()));
            }
            SortList(listBox_UsersAvailable.Items);
        }

        private string GetLogDatabase(string connectionString)
        {
            string db = connectionString.Substring(connectionString.IndexOf("database="));
            db = db.Replace("database=", string.Empty);
            db = db.Substring(0, db.IndexOf(';'));
            return db;
        }

        private void SortList(ItemCollection itemCollection)
        {
            itemCollection.SortDescriptions.Add(new System.ComponentModel.SortDescription("", System.ComponentModel.ListSortDirection.Ascending));
        }

        #region User ==============================================================================
        private void button_User_Select_Click(object sender, RoutedEventArgs e)
        {
            foreach (string user in listBox_UsersAvailable.SelectedItems)
            {
                if (!listBox_UsersSelected.Items.Contains(user))
                {
                    listBox_UsersSelected.Items.Add(user);
                }
            }
            SortList(listBox_UsersSelected.Items);
        }

        private void listBox_UsersAvailable_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            foreach (string user in listBox_UsersAvailable.SelectedItems)
            {
                if (!listBox_UsersSelected.Items.Contains(user))
                {
                    listBox_UsersSelected.Items.Add(user);
                }
            }
            SortList(listBox_UsersSelected.Items);
        }

        private void button_User_SelectALL_Click(object sender, RoutedEventArgs e)
        {
            foreach (string user in listBox_UsersAvailable.Items)
            {
                if (!listBox_UsersSelected.Items.Contains(user))
                {
                    listBox_UsersSelected.Items.Add(user);
                }
            }
            SortList(listBox_UsersSelected.Items);
        }

        private void button_User_Deselect_Click(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < listBox_UsersSelected.Items.Count; i++)
            {
                if (listBox_UsersSelected.SelectedItems.Contains(listBox_UsersSelected.Items[i]))
                {
                    listBox_UsersSelected.Items.RemoveAt(i);
                    i--;
                }
            }
        }

        private void listBox_UsersSelected_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            for (int i = 0; i < listBox_UsersSelected.Items.Count; i++)
            {
                if (listBox_UsersSelected.SelectedItems.Contains(listBox_UsersSelected.Items[i]))
                {
                    listBox_UsersSelected.Items.RemoveAt(i);
                    i--;
                }
            }
        }

        private void button_User_DeselectALL_Click(object sender, RoutedEventArgs e)
        {
            listBox_UsersSelected.Items.Clear();
        }
        #endregion User ===========================================================================

        #region LogDesc ===========================================================================
        private void button_Log_Select_Click(object sender, RoutedEventArgs e)
        {
            foreach (string log in listBox_LogAvailable.SelectedItems)
            {
                if (!listBox_LogSelected.Items.Contains(log))
                {
                    listBox_LogSelected.Items.Add(log);
                }
            }
            SortList(listBox_LogSelected.Items);
        }

        private void listBox_LogAvailable_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            foreach (string log in listBox_LogAvailable.SelectedItems)
            {
                if (!listBox_LogSelected.Items.Contains(log))
                {
                    listBox_LogSelected.Items.Add(log);
                }
            }
            SortList(listBox_LogSelected.Items);
        }

        private void button_Log_SelectALL_Click(object sender, RoutedEventArgs e)
        {
            foreach (string log in listBox_LogAvailable.Items)
            {
                if (!listBox_LogSelected.Items.Contains(log))
                {
                    listBox_LogSelected.Items.Add(log);
                }
            }
            SortList(listBox_LogSelected.Items);
        }

        private void button_Log_Deselect_Click(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < listBox_LogSelected.Items.Count; i++)
            {
                if (listBox_LogSelected.SelectedItems.Contains(listBox_LogSelected.Items[i]))
                {
                    listBox_LogSelected.Items.RemoveAt(i);
                    i--;
                }
            }
        }

        private void listBox_LogSelected_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            for (int i = 0; i < listBox_LogSelected.Items.Count; i++)
            {
                if (listBox_LogSelected.SelectedItems.Contains(listBox_LogSelected.Items[i]))
                {
                    listBox_LogSelected.Items.RemoveAt(i);
                    i--;
                }
            }
        }

        private void button_Log_DeselectALL_Click(object sender, RoutedEventArgs e)
        {
            listBox_LogSelected.Items.Clear();
        }
        #endregion LogDesc ========================================================================

        #region RunQuery Button ===================================================================
        private void button_RunQuery_Click(object sender, RoutedEventArgs e)
        {
            string query = string.Format("SELECT TOP {0} * FROM Logs WITH (NOLOCK) WHERE SiteID = {1}", MaxRows.ToString(), SiteID);
            try
            {
                query += GetAuxIDAnd();
                query += GetUserIDAnd();
                query += GetDateAnd();
                query += GetLogIDAnd();
                query += GetSupportUserAnd();

                RunQuery(query);
            }
            catch
            {
                Tools.ShowError("There was an issue with your query, double check that your AuxID is an integer value.");
                Tools.ShowMessage("Here is your query", query);
            }
        }

        private string GetAuxIDAnd()
        {
            string[] auxIds = textBox_AuxID.Text.Split(new char[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
            if (auxIds.Length < 1) return string.Empty;

            string andClause = " AND AuxID in (";
            for (int i = 0; i < auxIds.Length; i++)
            {
                if (i == 0)
                    andClause += ("'" + auxIds[i] + "'");
                else
                    andClause += (", '" + auxIds[i] + "'");
            }
            andClause += ")";

            return andClause;
        }

        private string GetUserIDAnd()
        {
            if (listBox_UsersSelected.Items.Count < 1) return string.Empty;

            string andClause = " AND LogUser in (";
            for (int i = 0; i < listBox_UsersSelected.Items.Count; i++)
            {
                if (i == 0)
                    andClause += ("'" + GetUserID(listBox_UsersSelected.Items[i]) + "'");
                else
                    andClause += (", '" + GetUserID(listBox_UsersSelected.Items[i]) + "'");
            }
            andClause += ")";
            
            return andClause;
        }

        private string GetUserID(object p)
        {
            string userEntry = (string)p;

            return userEntry.Substring(userEntry.IndexOf('(')).Replace("(", string.Empty).Replace(")", string.Empty).Trim();
        }

        private string GetDateAnd()
        {
            if (radioButton_DateALL.IsChecked == true) return string.Empty;
            if (datePicker_To.SelectedDate < datePicker_From.SelectedDate)
            {
                Tools.ShowError("'To' date is earlier than 'From' date. Choice reverted to 'All");
                radioButton_DateALL.IsChecked = true;
                return string.Empty;
            }
            return string.Format(" AND [Date] BETWEEN '{0}' AND '{1}'", datePicker_From.SelectedDate.ToString(), datePicker_To.SelectedDate.ToString());
        }

        private string GetLogIDAnd()
        {
            if (listBox_LogSelected.Items.Count < 1) return string.Empty;

            string andClause = " AND LogID in (";
            for (int i = 0; i < listBox_LogSelected.Items.Count; i++)
            {
                if (i == 0)
                    andClause += ("'" + GetLogID(listBox_LogSelected.Items[i]) + "'");
                else
                    andClause += (", '" + GetLogID(listBox_LogSelected.Items[i]) + "'");
            }
            andClause += ")";

            return andClause;
        }

        private string GetLogID(object p)
        {
            string logDesc = (string)p;

            foreach (int key in LogDescriptions.Keys)
            {
                if (logDesc == LogDescriptions[key])
                    return key.ToString();
            }
            return string.Empty;
        }

        private string GetSupportUserAnd()
        {
            if (listBox_SupportUserSelected.Items.Count < 1) return string.Empty;

            string andClause = " AND SupportName in (";
            for (int i = 0; i < listBox_SupportUserSelected.Items.Count; i++)
            {
                if (i == 0)
                    andClause += string.Format("'{0}'", listBox_SupportUserSelected.Items[i]);
                else
                    andClause += string.Format(", '{0}'", listBox_SupportUserSelected.Items[i]);
            }
            andClause += ")";

            return andClause;
        }

        private void RunQuery(string query)
        {
            List<LogEntry> LogEntries = new List<LogEntry>();
            using (SqlConnection conn = new SqlConnection(Tools.GetActivityLogDBConnectionString(SiteID)))
            {
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                    {
                        DataSet ds = new DataSet();

                        conn.Open();
                        da.Fill(ds);
                        conn.Close();

                        foreach (DataRow row in ds.Tables[0].Rows)
                        {
                            LogEntry le = new LogEntry();
                            //le.Pos = row["Pos"].ToString();
                            le.SiteID = row["SiteID"].ToString();
                            le.Date = row["Date"].ToString();
                            //le.Severity = row["Severity"].ToString();
                            string appID = row["AppID"].ToString();
                            switch(appID)
                            {
                                case "0":
                                    le.App = "SYSTEM";
                                    break;
                                case "1":
                                    le.App = "XClaim";
                                    break;
                                case "2":
                                    le.App = "XDenial";
                                    break;
                                case "3":
                                    le.App = "XCollect";
                                    break;
                                default:
                                    le.App = appID;
                                    break;
                            }
                            //le.LogSite = row["LogSite"].ToString();
                            le.User = UserData.FindUserData(int.Parse(row["LogUser"].ToString())).UserName;
                            le.AuxID = row["AuxID"].ToString();
                            le.Count = row["Count"].ToString();
                            le.LogID = LogDescriptions[int.Parse(row["LogID"].ToString())];
                            le.Data = row["Data"].ToString();
                            //le.IssueCreated = row["IssueCreated"].ToString();
                            le.SupportName = row["SupportName"].ToString();

                            LogEntries.Add(le);
                        }
                    }
                }
            }

            label_Status.Content = string.Format("{0} rows returned.", LogEntries.Count.ToString());
            tabControl_Main.SelectedIndex = 1;
            dataGrid_Results.ItemsSource = LogEntries;
        }
        #endregion RunQuery Button ================================================================

        private void radioButton_DateALL_Checked(object sender, RoutedEventArgs e)
        {
            datePicker_From.IsEnabled = false;
            datePicker_To.IsEnabled = false;
            label_From.IsEnabled = false;
            label_To.IsEnabled = false;
        }

        private void radioButton_DateRange_Checked(object sender, RoutedEventArgs e)
        {
            datePicker_To.IsEnabled = true;
            datePicker_From.IsEnabled = true;
            label_From.IsEnabled = true;
            label_To.IsEnabled = true;
        }

        private void listBox_SupportUserAvailable_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            foreach (string user in listBox_SupportUserAvailable.SelectedItems)
            {
                if (!listBox_SupportUserSelected.Items.Contains(user))
                {
                    listBox_SupportUserSelected.Items.Add(user);
                }
            }
            SortList(listBox_SupportUserSelected.Items);
        }

        private void listBox_SupportUserSelected_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            for (int i = 0; i < listBox_SupportUserSelected.Items.Count; i++)
            {
                if (listBox_SupportUserSelected.SelectedItems.Contains(listBox_SupportUserSelected.Items[i]))
                {
                    listBox_SupportUserSelected.Items.RemoveAt(i);
                    i--;
                }
            }
        }

        private void button_SupportUser_Select_Click(object sender, RoutedEventArgs e)
        {
            foreach (string user in listBox_SupportUserAvailable.SelectedItems)
            {
                if (!listBox_SupportUserSelected.Items.Contains(user))
                {
                    listBox_SupportUserSelected.Items.Add(user);
                }
            }
            SortList(listBox_SupportUserSelected.Items);
        }

        private void button_SupportUser_SelectALL_Click(object sender, RoutedEventArgs e)
        {
            foreach (string user in listBox_SupportUserAvailable.Items)
            {
                if (!listBox_SupportUserSelected.Items.Contains(user))
                {
                    listBox_SupportUserSelected.Items.Add(user);
                }
            }
            SortList(listBox_SupportUserSelected.Items);
        }

        private void button_SupportUser_Deselect_Click(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < listBox_SupportUserSelected.Items.Count; i++)
            {
                if (listBox_SupportUserSelected.SelectedItems.Contains(listBox_SupportUserSelected.Items[i]))
                {
                    listBox_SupportUserSelected.Items.RemoveAt(i);
                    i--;
                }
            }
        }

        private void button_SupportUser_DeselectALL_Click(object sender, RoutedEventArgs e)
        {
            listBox_SupportUserSelected.Items.Clear();
        }
    }

    public class LogEntry
    {
        public string SiteID { get; set; }
        public string User { get; set; } //LogUser
        public string Date { get; set; }
        public string LogID { get; set; }
        public string AuxID { get; set; }
        public string App { get; set; } //AppID
        public string Count { get; set; }
        public string Data { get; set; }
        public string SupportName { get; set; }
        //public string Pos { get; set; }
        //public string Severity { get; set; }
        //public string LogSite { get; set; } 
        //public string IssueCreated { get; set; }
    }
}
