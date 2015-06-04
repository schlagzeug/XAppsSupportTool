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
    /// Interaction logic for JobTrack2.xaml
    /// </summary>
    public partial class JobTrack2 : Window
    {
        List<JobTrackScheduledJob> jobs = new List<JobTrackScheduledJob>();
        public JobTrack2()
        {
            InitializeComponent();
        }

        public JobTrack2(string siteID)
        {
            InitializeComponent();
            textBox_SearchTerms.Text = siteID;
            FindJobTrackJobs(siteID);
            dataGrid_Results.ItemsSource = jobs;
        }

        private void button_Search_Click(object sender, RoutedEventArgs e)
        {
            dataGrid_Results.ItemsSource = null;
            jobs.Clear();
            FindJobTrackJobs(textBox_SearchTerms.Text);
            dataGrid_Results.ItemsSource = jobs;
        }

        private void FindJobTrackJobs(string p)
        {
            string query = string.Format("SELECT * FROM [JobTrack].[dbo].[JTScheduledJobs] WHERE [Name] like '%{0}%'", p);
            using (SqlConnection conn = new SqlConnection(@"server=RCM40CPPLADB01\Plat;database=JobTrack;Integrated Security=True"))
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
                            JobTrackScheduledJob jt = new JobTrackScheduledJob();
                            jt.ScheduledJobID = row["ScheduledJobID"].ToString();
                            jt.Name = row["Name"].ToString();
                            jt.Description = row["Description"].ToString();
                            jt.ClientID = row["ClientID"].ToString();
                            jt.Owner = row["Owner"].ToString();
                            jt.ParametersXml = row["ParametersXml"].ToString();
                            jt.Priority = row["Priority"].ToString();
                            jt.NextRunDate = row["NextRunDate"].ToString();
                            jt.ScheduleXml = row["ScheduleXml"].ToString();
                            jt.ScheduleSummary = row["ScheduleSummary"].ToString();
                            jt.InsertDate = row["InsertDate"].ToString();
                            jt.UpdateDate = row["UpdateDate"].ToString();
                            jt.UpdateBy = row["UpdateBy"].ToString();
                            jt.LastNoteID = row["LastNoteID"].ToString();
                            jt.CircuitID = row["CircuitID"].ToString();
                            jt.FacilityID = row["FacilityID"].ToString();
                            jobs.Add(jt);
                        }
                    }
                }
            }
        }
    }

    public class JobTrackScheduledJob
    {
        public string ScheduledJobID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string ClientID { get; set; }
        public string Owner { get; set; }
        public string ParametersXml { get; set; }
        public string Priority { get; set; }
        public string NextRunDate { get; set; }
        public string ScheduleXml { get; set; }
        public string ScheduleSummary { get; set; }
        public string InsertDate { get; set; }
        public string UpdateDate { get; set; }
        public string UpdateBy { get; set; }
        public string LastNoteID { get; set; }
        public string CircuitID { get; set; }
        public string FacilityID { get; set; }
    }
}
