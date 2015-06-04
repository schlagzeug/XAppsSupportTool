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
    /// Interaction logic for SQL_Jobs.xaml
    /// </summary>
    public partial class SQL_Jobs : Window
    {
        List<SQLJobInfo> jobData = new List<SQLJobInfo>();
        public SQL_Jobs()
        {
            InitializeComponent();
        }

        public SQL_Jobs(string keyWord)
        {
            InitializeComponent();
            textBox_SearchString.Text = keyWord;
            PopulateGrid();
        }

        private void button_Find_Click(object sender, RoutedEventArgs e)
        {
            PopulateGrid();
        }

        private void PopulateGrid()
        {
            jobData.Clear();
            dataGrid_results.ItemsSource = null;
            string query = string.Format("SELECT j.name, js.step_id,js.step_name, js.command, j.enabled FROM dbo.sysjobs j WITH (NOLOCK) JOIN dbo.sysjobsteps js WITH (NOLOCK) ON js.job_id = j.job_id WHERE j.name LIKE N'%{0}%' order by j.name", textBox_SearchString.Text);
            using (SqlConnection conn = new SqlConnection("server=RCM40VPXAPJBS01;database=msdb;Integrated Security=True"))
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
                            SQLJobInfo ji = new SQLJobInfo();
                            int enabled = int.Parse(row["enabled"].ToString());
                            if (enabled == 1)
                                ji.Enabled = true;
                            else
                                ji.Enabled = false;
                            ji.JobName = row["name"].ToString();
                            ji.StepNumber = int.Parse(row["step_id"].ToString());
                            ji.StepName = row["step_name"].ToString();
                            ji.Command = row["command"].ToString();
                            ji.JT2_AppPath = GetJTAppPath(ji.Command);
                            ji.JT2_Arguments = GetJTArguments(ji.Command);
                            jobData.Add(ji);
                        }
                    }
                }
            }

            dataGrid_results.ItemsSource = jobData;
        }

        private string GetJTArguments(string command)
        {            
            if (command.Contains("ClaimConverter"))
            {
                string arguments = command.Substring(command.IndexOf(' ') + 1);
                string[] args = arguments.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                arguments = string.Empty;
                for (int i = 0; i < args.Length; i++)
                {
                    args[i] = args[i].Trim();
                    if (args[i].ToUpper() == "-SITEID")
                    {
                        args[i + 1] = "%(Job.ClientID)% ";
                    }

                    arguments += args[i] + " ";
                }
                arguments += "-JOBTRACK -Date %(Job.ScheduledStartDate)%";
                return arguments;// arguments;
            }
            else
            {
                return "Currently Only for ClaimConverter Jobs.";
            }
        }

        private string GetJTAppPath(string command)
        {
            if (command.ToUpper().Contains("CLAIMCONVERTER"))
            {
                string appPath = @"C:\XactiMed\Bin\XClaim.Post\ClaimConverterUtility\ClaimConverterUtility.exe";

                return appPath;
            }
            else
            {
                return "Currently Only for ClaimConverter Jobs.";
            }
        }

        private void button_PopOut_Click(object sender, RoutedEventArgs e)
        {
            var x = new PopOutGrid(jobData, "SQL Jobs");
            x.Show();
        }
    }

    public class SQLJobInfo
    {
        public bool Enabled { get; set; }
        public string JobName { get; set; }
        public int StepNumber { get; set; }
        public string StepName { get; set; }
        public string Command { get; set; }

        public string JT2_AppPath { private get; set; }
        public string JT2_Arguments { private get; set; }
    }
}
