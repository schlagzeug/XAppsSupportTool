using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace XAppsSupport
{
    /// <summary>
    /// Interaction logic for StVincentFacilities.xaml
    /// </summary>
    public partial class StVincentFacilities : Window
    {
        public bool codeTriggered = false;
        public StVincentFacilities()
        {
            InitializeComponent();
            radioButton_All.IsChecked = true;
            PopulateRoutineComboBox();
            PopulateDataGrid();
        }

        private void PopulateRoutineComboBox()
        {
            try
            {
                ClearOutComboBox();
                string query = "select * from XAppsGlobal.dbo.BridgeRoutines where ClientID = 230000 and Active = 1";
                string connString = Tools.GetConnectionString();
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
                            comboBox_Routines.Items.Add(row["ClientRoutineID"].ToString() + " - " + row["Title"].ToString());
                        }
                    }
                }
                comboBox_Routines.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                Tools.ShowError(string.Format("Error getting Routine data: {0}", ex.ToString()));
            }
        }

        private void ClearOutComboBox()
        {
            codeTriggered = true;
            comboBox_Routines.Items.Clear();
            codeTriggered = false;
        }

        private void PopulateDataGrid()
        {
            dataGrid.ItemsSource = null;
            if (radioButton_All.IsChecked.Value)
            {
                List<FacilityData> allFacData = new List<FacilityData>();

                try
                {
                    string query = "select * from X3Domain1.dbo.Facilities where ClientID = 230000";
                    string connString = Tools.GetConnectionString();
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
                                FacilityData fd = new FacilityData();
                                fd.FacilityID = row["FacilityID"].ToString();
                                fd.FacilityKey = row["FacilityKey"].ToString();
                                fd.FacilityName = row["Name"].ToString();
                                allFacData.Add(fd);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Tools.ShowError(string.Format("Error getting Facility data: {0}", ex.ToString()));
                }

                dataGrid.ItemsSource = allFacData;
            }
            else
            {
                /* select f.FacilityID, f.FacilityKey, f.Name
                 * from X3Domain1.dbo.Facilities f
                 * --join X3Domain1.dbo.FacilityGroups fg on f.ClientID = fg.ClientID
                 * join X3Domain1.dbo.FacilityGroupFacilities fgf on f.ClientID = fgf.ClientID
                 * join XAppsGlobal.dbo.FacilityGroupBridgeRoutines fgbr on f.ClientID = fgbr.ClientID
                 * where f.ClientID = 230000
                 * --and fg.FacilityGroupID = fgbr.FacilityGroupID
                 * and fgf.FacilityGroupID = fgbr.FacilityGroupID
                 * and fgf.FacilityID = f.FacilityID
                 * and fgbr.ClientRoutineID = 316
                 */

                List<FacilityData> routineFacData = new List<FacilityData>();

                try
                {
                    string routineID = comboBox_Routines.SelectedItem.ToString();
                    routineID = routineID.Substring(0, routineID.IndexOf(" "));
                    string query = string.Format("select f.FacilityID, f.FacilityKey, f.Name from X3Domain1.dbo.Facilities f join X3Domain1.dbo.FacilityGroupFacilities fgf on f.ClientID = fgf.ClientID join XAppsGlobal.dbo.FacilityGroupBridgeRoutines fgbr on f.ClientID = fgbr.ClientID where f.ClientID = 230000 and fgf.FacilityGroupID = fgbr.FacilityGroupID and fgf.FacilityID = f.FacilityID and fgbr.ClientRoutineID = {0}", routineID);
                    string connString = Tools.GetConnectionString();
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
                                FacilityData fd = new FacilityData();
                                fd.FacilityID = row["FacilityID"].ToString();
                                fd.FacilityKey = row["FacilityKey"].ToString();
                                fd.FacilityName = row["Name"].ToString();
                                routineFacData.Add(fd);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Tools.ShowError(string.Format("Error getting facility data for routine: {0}", ex.ToString()));
                }
                if (routineFacData.Count == 0)
                    routineFacData.Add(new FacilityData()); // makes it look better
                dataGrid.ItemsSource = routineFacData;
            }
        }

        private void radioButton_All_Checked(object sender, RoutedEventArgs e)
        {
            comboBox_Routines.IsEnabled = false;
            PopulateDataGrid();
        }

        private void radioButton_ByRoutine_Checked(object sender, RoutedEventArgs e)
        {
            comboBox_Routines.IsEnabled = true;
            PopulateDataGrid();
        }

        private void comboBox_Routines_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!codeTriggered) PopulateDataGrid();
        }

        private void button_Refresh_Click(object sender, RoutedEventArgs e)
        {
            string selectedComboValue = comboBox_Routines.SelectedItem.ToString();
            PopulateRoutineComboBox();
            if (comboBox_Routines.Items.Contains(selectedComboValue))
                comboBox_Routines.SelectedValue = selectedComboValue;
            else
                comboBox_Routines.SelectedIndex = 0;
            PopulateDataGrid();
        }
    }

    public class FacilityData
    {
        public string FacilityID { get; set; }
        public string FacilityKey { get; set; }
        public string FacilityName { get; set; }
    }
}
