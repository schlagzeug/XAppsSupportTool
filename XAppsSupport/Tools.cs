using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Xml;
using MedAssets.Data;
using Microsoft.Win32;
using XactiMed;
using XactiMed.XApps;
using XactiMed.XApps.XClaim;

namespace XAppsSupport
{
    public enum MenuType { PROD, TEST999, TEST888 };

    public enum UpdateType { UB, HCFA, TABLES, FILESTRUCTURE };

    public static class Tools
    {
        private const string appConfigFile = @"C:\Program Files (x86)\XactiMed\AppStartup\AppStartup.exe.config";

        /// <summary>
        /// Add 5010 header to an 837 (buggy)
        /// </summary>
        /// <param name="origImage">837 file text</param>
        /// <param name="claimType">Claim type</param>
        /// <returns></returns>
        public static string AddHeader(string origImage, string claimType)
        {
            string header = string.Empty;
            string footer = string.Empty;

            if (origImage.Contains("CLM*")) //ANSI
            {
                if (claimType == "UB92")
                {
                    header = "ISA*00*          *00*          *ZZ*232825878      *ZZ*00363          *      *    *{*00501*         *1*T*>~\nGS*HC*945790158*345529167*20120105*0309*302317001*X*005010X223A2~\nST*837*101050001*005010X223A2~\nBHT*0019*00*1*20120105*0309*CH~\nNM1*41*2*MEDASSETS DIAGNOSTIC LAB*****46*232825878~\nPER*IC*CLAIM CREATOR*TE*2157073116*FX*0000000000~\nNM1*40*2*XACTIMED*****46*00363~\n";
                    footer = "SE*50*101050001~\nGE*1*302317001~\nIEA*1*         ~\n";
                }
                else if (claimType == "HCFA1500")
                {
                    header = "ISA*00*          *00*          *ZZ*541820093      *ZZ*               *      *    *{*00501*         *1*P*>~\nGS*HC*TNV147**20120113*0329*142625001*X*005010X222A1~\nST*837*101130001*005010X222A1~\nBHT*0019*00*1*20120113*0329*CH~\nNM1*41*2*MEDASSETS TEST*****46*541820093~\nPER*IC*MEDASSETS TEST*TE*7578892550*FX*7578892550~\nNM1*40*2*XACTIMED******~\n";
                    footer = "SE*72*101130001~\nGE*1*142625001~\nIEA*1*         ~\n";
                }
                else
                {
                    // invalid type
                }

                origImage = origImage.Replace("*~", "~");

                return header + origImage + footer;
            }
            else // printimage
                return origImage;
        }

        /// <summary>
        /// Cleans directory and sub directories of .bak, .tab, .log and .snapshot files
        /// </summary>
        /// <param name="d">Directory to clean</param>
        public static void CleanDirectory(DirectoryInfo d)
        {
            FileInfo[] rgFiles = d.GetFiles("*.*");
            for (int i = 0; i < rgFiles.Length; i++)
            {
                FileInfo file = rgFiles[i];
                string strName = file.FullName.ToUpper();
                if (strName.Contains(".XML") || strName.Contains(".BAT") || strName.Contains(".SCC"))
                    continue;
            }

            rgFiles = d.GetFiles("*.bak");
            DeleteFile(rgFiles);
            rgFiles = d.GetFiles("*.tab");
            DeleteFile(rgFiles);
            rgFiles = d.GetFiles("*.log");
            DeleteFile(rgFiles);
            rgFiles = d.GetFiles("*.snapshot");
            DeleteFile(rgFiles);

            DirectoryInfo[] dirs = d.GetDirectories();
            foreach (DirectoryInfo dir in dirs)
                CleanDirectory(dir);
        }

        /// <summary>
        /// Copy files from one directory to another
        /// </summary>
        /// <param name="sourceDirName">Source directory</param>
        /// <param name="destDirName">Destination directory</param>
        /// <param name="copySubDirs">Copy sub-directories too?</param>
        public static void CopyDirectory(string sourceDirName, string destDirName, bool copySubDirs)
        {
            // Get the subdirectories for the specified directory.
            DirectoryInfo dir = new DirectoryInfo(sourceDirName);
            DirectoryInfo[] dirs = dir.GetDirectories();

            if (!dir.Exists)
            {
                throw new DirectoryNotFoundException(
                    "Source directory does not exist or could not be found: "
                    + sourceDirName);
            }

            // If the destination directory doesn't exist, create it.
            if (!Directory.Exists(destDirName))
            {
                Directory.CreateDirectory(destDirName);
            }

            // Get the files in the directory and copy them to the new location.
            FileInfo[] files = dir.GetFiles();
            foreach (FileInfo file in files)
            {
                string temppath = Path.Combine(destDirName, file.Name);
                if (!File.Exists(temppath))
                {
                    file.CopyTo(temppath, false);
                }
                else
                {
                    FileInfo destFile = new FileInfo(temppath);
                    if (destFile.CreationTime < file.CreationTime)
                    {
                        File.SetAttributes(temppath, FileAttributes.Normal);
                        file.CopyTo(temppath, true);
                    }
                }
            }

            // If copying subdirectories, copy them and their contents to new location.
            if (copySubDirs)
            {
                foreach (DirectoryInfo subdir in dirs)
                {
                    string temppath = Path.Combine(destDirName, subdir.Name);
                    CopyDirectory(subdir.FullName, temppath, copySubDirs);
                }
            }
        }

        /// <summary>
        /// Decompress a .PK file
        /// </summary>
        /// <param name="source">Location of source file</param>
        /// <param name="destination">Location of destination file</param>
        public static void DecompressPkFile(string source, string destination)
        {
            PKStream.Uncompress(source, destination);
        }

        /// <summary>
        /// Get the claim database associated with a certain connection string and site ID
        /// </summary>
        /// <param name="connString">Connection string</param>
        /// <param name="siteID">Site ID</param>
        /// <returns>claim database</returns>
        public static string GetClaimDatabase(string connString, int siteID)
        {
            string claimDB = GetResource(siteID, 1, 0, 1);
            claimDB = claimDB.Replace(";database=Primary;Integrated Security=True", string.Empty);
            claimDB = claimDB.Replace(";database=PRIMARY;Integrated Security=True", string.Empty);
            claimDB = claimDB.Replace("server=", string.Empty);
            return claimDB;
        }

        /// <summary>
        /// Returns a collection of claim IDs in a certain import ID
        /// </summary>
        /// <param name="siteId">Site ID</param>
        /// <param name="importId">Import ID</param>
        /// <param name="resource">Resource (Archive/Primary)</param>
        /// <returns>Claims as SQL Data</returns>
        public static SqlDataReader GetClaimIdsByImportId(int siteId, int importId, XClaimResource resource)
        {
            SqlDataReader reader = null;
            XactiMed.XApps.ResourceDB resourceDB = new XactiMed.XApps.ResourceDB();
            string connString = resourceDB.GetResourceString(siteId, ApplicationID.XClaim, (short)resource);
            SqlConnection conn = new SqlConnection(connString);
            try
            {
                SqlCommand cmd = new SqlCommand("dbo.apiGetClaimIdsByImportId", conn);

                cmd.CommandType = CommandType.StoredProcedure;

                SqlUtil.AddSqlParameter(cmd, "@SiteID", SqlDbType.Int, siteId);
                SqlUtil.AddSqlParameter(cmd, "@ImportID", SqlDbType.Int, importId);
                conn.Open();
                reader = cmd.ExecuteReader();
            }
            catch
            {
                ShowError("Error while getting claims from database.");
            }
            finally
            {
                conn.Close();
            }
            return reader;
        }

        /// <summary>
        /// Returns an ArrayList of claim IDs in an import (both Archive and Primary)
        /// </summary>
        /// <param name="siteID">Site ID</param>
        /// <param name="importID">Import ID</param>
        /// <returns>ArrayList of claim IDs</returns>
        public static ArrayList GetClaimIdsByImportId(int siteID, int importID)
        {
            ArrayList claimIDs = new ArrayList();

            SqlDataReader archiveReader = GetClaimIdsByImportId(siteID, importID, XClaimResource.ArcDatabase);
            while (archiveReader.Read())
            {
                claimIDs.Add(archiveReader.GetInt32(0));
            }
            archiveReader.Close();

            SqlDataReader primaryReader = GetClaimIdsByImportId(siteID, importID, XClaimResource.PriDatabase);
            while (primaryReader.Read())
            {
                claimIDs.Add(primaryReader.GetInt32(0));
            }
            primaryReader.Close();

            return claimIDs;
        }

        /// <summary>
        /// Gets the original images of multiple claims
        /// </summary>
        /// <param name="siteID">Site ID</param>
        /// <param name="claimIDs">ArrayList of claim IDs</param>
        /// <returns>Original images</returns>
        public static string GetClaimOriginalImage(int siteID, ArrayList claimIDs)
        {
            string origImages = string.Empty;
            foreach (var claimID in claimIDs)
            {
                int iClaimID = int.Parse(claimID.ToString());
                string origImage = GetClaimOriginalImage(siteID, iClaimID);
                origImages = string.Concat(origImages, origImage);
            }
            return origImages;
        }

        /// <summary>
        /// Get the current connection string from the registry
        /// </summary>
        /// <returns>connection string</returns>
        public static string GetConnectionString()
        {
            string sConnectionString = string.Empty;

            RegistryKey key = Registry.LocalMachine.CreateSubKey("SOFTWARE\\Wow6432Node\\XactiMed");
            sConnectionString = key.GetValue("ResourceDB").ToString();

            return sConnectionString;
        }

        /// <summary>
        /// Gets the resource from the Resources table
        /// </summary>
        /// <param name="siteID">Site ID</param>
        /// <param name="appID">App ID</param>
        /// <param name="resID">Resource ID</param>
        /// <param name="type">Type</param>
        /// <returns>resource</returns>
        public static string GetResource(int siteID, int appID, int resID, int type)
        {
            var resource = string.Empty;
            var connString = GetConnectionString();

            using (SqlConnection conn = new SqlConnection(connString))
            {
                string sQuery = string.Format("SELECT Resource FROM Resources WHERE SiteID = {0} and AppID = {1} and ResID = {2} and Type = {3}", siteID, appID, resID, type);
                using (SqlCommand cmd = new SqlCommand(sQuery, conn))
                {
                    conn.Open();
                    object returnval = cmd.ExecuteScalar();
                    if (returnval != null)
                    {
                        resource = returnval.ToString();
                    }
                    conn.Close();
                }
            }

            return resource;
        }

        /// <summary>
        /// Get the site directory location
        /// </summary>
        /// <param name="connectionString">Connection string</param>
        /// <param name="siteID">Site ID</param>
        /// <returns></returns>
        public static string GetSiteLocaion(int siteID)
        {
            var site = GetResource(siteID, 0, 5, 4);
            if (site == string.Empty)
            {
                site = GetResource(siteID, 0, 5, 8);
            }
            return site;
        }

        /// <summary>
        /// Gets the task server for the specified site ID
        /// </summary>
        /// <param name="siteID">Site ID</param>
        /// <returns>task server</returns>
        public static string GetTaskServer(int siteID)
        {
            var taskServer = GetResource(siteID, 1, 3, 2);
            taskServer = taskServer.Substring(taskServer.Length - 2);

            var connectionString = GetConnectionString();

            if (connectionString.Contains(@"rcm40cpXapDB50\domsvr"))
            {
                taskServer = "rcm40vpxapapp" + taskServer.Replace('e', '0');
            }
            else if (connectionString.Contains(@"rcm40csXapDB50\domsvr1"))
            {
                taskServer = "RCM40vsXapApp01";
            }
            else if (connectionString.Contains(@"rcm40csXapDB51\domsvr2"))
            {
                taskServer = "RCM40vsXapApp10";
            }

            return taskServer;
        }

        /// <summary>
        /// Opens specified directory in Windows explorer
        /// </summary>
        /// <param name="path">Directory to open</param>
        public static void OpenDirectory(string path)
        {
            if (path == string.Empty)
            {
                ShowError("Directory doesn't exist.");
                return;
            }
            DirectoryInfo diDir = new DirectoryInfo(path);
            if (diDir.Exists)
            {
                Process.Start(path);
            }
            else
            {
                ShowError("Directory doesn't exist.");
            }
        }

        /// <summary>
        /// Opens selected file
        /// </summary>
        /// <param name="path">Path to desired file</param>
        public static void OpenFile(string path)
        {
            FileInfo fi = new FileInfo(path);
            if (fi.Exists)
            {
                Process.Start(path);
            }
            else
            {
                ShowError("File not found.");
            }
        }

        /// <summary>
        /// Displays the error message in a MessageBox
        /// </summary>
        /// <param name="error">Error to display</param>
        public static void ShowError(string error)
        {
            System.Windows.MessageBox.Show(error, "Error.");
        }

        /// <summary>
        /// Shows message in a MessageBox
        /// </summary>
        /// <param name="message">Message to show</param>
        public static void ShowMessage(string message)
        {
            System.Windows.MessageBox.Show(message);
        }

        /// <summary>
        /// Updates the CustomerSS folder for the specified claim type
        /// </summary>
        /// <param name="SiteID">Site ID</param>
        /// <param name="claimType">What to update</param>
        public static void UpdateCustomerSS(int SiteID, UpdateType type)
        {
            BackgroundWorker bw = new BackgroundWorker();
            bw.DoWork += new DoWorkEventHandler(
                delegate(object o, DoWorkEventArgs args)
                {
                    string baseServerDir = GetSiteLocaion(SiteID);
                    string baseLocalDir = string.Format(@"C:\CustomerSS\{0}", SiteID);
                    string tablesPath = @"XClaim\Tables\";
                    string ubPath = @"XClaim\UB92\Import\";
                    string hcfaPath = @"XClaim\HCFA1500\Import\";

                    if (type == UpdateType.TABLES)
                    {
                        string serverTables = Path.Combine(baseServerDir, tablesPath);
                        string localTables = Path.Combine(baseLocalDir, tablesPath);

                        CopyDirectory(serverTables, localTables, true);

                        ShowMessage("Tables have been updated.");
                    }
                    else if (type == UpdateType.UB)
                    {
                        string serverPath = Path.Combine(baseServerDir, ubPath);
                        string localPath = Path.Combine(baseLocalDir, ubPath);

                        CopyDirectory(serverPath, localPath, false);

                        CreateTestBatFilesAndStdIn(localPath, SiteID);

                        ShowMessage("UB files updated.");
                    }
                    else if (type == UpdateType.HCFA)
                    {
                        string serverPath = Path.Combine(baseServerDir, hcfaPath);
                        string localPath = Path.Combine(baseLocalDir, hcfaPath);

                        CopyDirectory(serverPath, localPath, false);

                        CreateTestBatFilesAndStdIn(localPath, SiteID);

                        ShowMessage("HCFA files updated.");
                    }
                    else if (type == UpdateType.FILESTRUCTURE)
                    {
                        CreateCustomerSSFileStructure(SiteID);
                        string reportDir = Path.Combine(baseLocalDir, @"\XClaim\Reports");
                        if (!Directory.Exists(reportDir))
                        {
                            Directory.CreateDirectory(reportDir);
                        }
                    }
                });
            bw.RunWorkerAsync();
        }

        /// <summary>
        /// Latest time in the day
        /// </summary>
        /// <param name="date">Date</param>
        /// <returns>Latest DateTime of provided date</returns>
        internal static DateTime EndOfDay(DateTime date)
        {
            return new DateTime(date.Year, date.Month, date.Day, 23, 59, 59, 999);
        }

        /// <summary>
        /// Returns the app data folder for the current user
        /// </summary>
        /// <returns>Location of the app data folder</returns>
        internal static string GetAppDataFolder()
        {
            string appDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            if (appDataFolder.EndsWith(@"\Roaming"))
            {
                appDataFolder = appDataFolder.Replace(@"\Roaming", string.Empty);
            }

            return appDataFolder;
        }

        /// <summary>
        /// Gets an original image of a single claim
        /// </summary>
        /// <param name="siteID">Site ID</param>
        /// <param name="claimID">Claim ID</param>
        /// <returns>Original image</returns>
        internal static string GetClaimOriginalImage(int siteID, int claimID)
        {
            XactiMed.XApps.ResourceDB resDB = new XactiMed.XApps.ResourceDB();
            resDB.ConnectionString = GetConnectionString();
            ClaimDB claimdb = new ClaimDB(resDB);

            System.IO.MemoryStream imageStream = new System.IO.MemoryStream();
            claimdb.GetClaimImageListXml(XClaimResource.PriDatabase, siteID, claimID, imageStream);
            XmlDocument docImageList = claimdb.XmlDocumentFromStream(imageStream);
            XmlNodeList nodeRows = docImageList.SelectNodes("/list/row");
            string origImageEnc = EzXml.GetAttributeValue(nodeRows[nodeRows.Count - 1], "Image");
            string origImage = claimdb.ExtractClaimImage(CompressionType.XPKLib, origImageEnc);

            if (origImage == null)
            {
                imageStream = new System.IO.MemoryStream();
                claimdb.GetClaimImageListXml(XClaimResource.ArcDatabase, siteID, claimID, imageStream);
                docImageList = claimdb.XmlDocumentFromStream(imageStream);
                nodeRows = docImageList.SelectNodes("/list/row");
                origImageEnc = EzXml.GetAttributeValue(nodeRows[nodeRows.Count - 1], "Image");
                origImage = claimdb.ExtractClaimImage(CompressionType.XPKLib, origImageEnc);
            }

            return origImage;
        }

        /// <summary>
        /// Returns current connection string from registry
        /// </summary>
        /// <param name="is64bit">true=64 bit, false=32 bit</param>
        /// <returns>Connection string</returns>
        internal static string GetConnectionString(bool is64bit)
        {
            string keyPath = @"Software\XactiMed\";
            var x86View = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32);
            var x64View = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);
            RegistryKey key;

            if (is64bit)
            {
                key = x64View.OpenSubKey(keyPath, true);
            }
            else
            {
                key = x86View.OpenSubKey(keyPath, true);
            }

            string sConnectionString = string.Empty;
            sConnectionString = key.GetValue("ResourceDB").ToString();
            key.Close();
            return sConnectionString;
        }

        /// <summary>
        /// Returns the current server that is set in the app config file
        /// </summary>
        /// <returns>Server</returns>
        internal static string GetCurrentServer()
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(appConfigFile);
            XmlNode serverNode = doc.SelectSingleNode(@"/configuration/appSettings/add[@key='ZipPackageSync.PageUri']");
            return serverNode.Attributes[1].Value;
        }

        /// <summary>
        /// Get simple version of current server off of the connection string
        /// </summary>
        /// <returns>Simple server string</returns>
        internal static string GetCurrentServerSimple()
        {
            string server = GetConnectionString();
            server = server.Replace(@";database=X3Resource;Integrated Security=True", string.Empty);
            server = server.Replace(@"server=", string.Empty);
            return server;
        }

        /// <summary>
        /// Gets the domain connection for a site ID
        /// </summary>
        /// <param name="siteID">Site ID</param>
        /// <returns>Domain connection</returns>
        internal static string GetDomainConnection(int siteID)
        {
            return GetResource(siteID, 0, 0, 1);
        }

        /// <summary>
        /// Get the directory for the site logs
        /// </summary>
        /// <param name="SiteID">Site ID</param>
        /// <returns>Directory of the site logs</returns>
        internal static string GetLogLocation(int SiteID)
        {
            return Tools.GetResource(SiteID, 0, 30, 4);
        }

        /// <summary>
        /// Gets the site's name
        /// </summary>
        /// <param name="siteID">Site ID</param>
        /// <returns>Name</returns>
        internal static string GetSiteName(int siteID)
        {
            return GetResource(siteID, 0, 6, 8);
        }

        /// <summary>
        /// Whether or not OneFishTwoFish is enabled in the registry
        /// </summary>
        /// <returns>true if AltLogin is set, false otherwise</returns>
        internal static bool IsOneFishTwoFishEnabled()
        {
            RegistryKey key = Registry.CurrentUser.CreateSubKey(@"Software\XactiMed\XApps");
            string state = key.GetValue("AltLogin").ToString();
            key.Close();
            if (state == "1")
                return true;
            else
                return false;
        }

        /// <summary>
        /// Set connection string registry
        /// </summary>
        /// <param name="newConnectionString">Connection string to set</param>
        /// <param name="is64bit">True=64bit, false=32bit</param>
        internal static void SetResourceRegistry(object newConnectionString, bool is64bit)
        {
            string keyPath = @"Software\XactiMed\";
            var x86View = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32);
            var x64View = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);
            RegistryKey key;

            if (is64bit)
            {
                key = x64View.OpenSubKey(keyPath, true);
            }
            else
            {
                key = x86View.OpenSubKey(keyPath, true);
            }

            key.SetValue("ResourceDB", newConnectionString);
            key.Close();
        }

        /// <summary>
        /// Set connection string registry for both 32 and 64 bit
        /// </summary>
        /// <param name="connString">Connection string to set</param>
        internal static void SetResourceRegistry(string connString)
        {
            SetResourceRegistry(connString, true);
            SetResourceRegistry(connString, false);
        }

        /// <summary>
        /// Earliest time in they day
        /// </summary>
        /// <param name="date">Date</param>
        /// <returns>Earliest DateTime of provided date</returns>
        internal static DateTime StartOfDay(DateTime date)
        {
            return new DateTime(date.Year, date.Month, date.Day, 0, 0, 0, 0);
        }

        /// <summary>
        /// Updates the server in the app config file
        /// </summary>
        /// <param name="server">Server to update</param>
        internal static void UpdateServer(string server)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(appConfigFile);
            XmlNode addNode = doc.SelectSingleNode(@"/configuration/appSettings/add[@key='ZipPackageSync.PageUri']");
            addNode.Attributes[1].Value = server;
            doc.Save(appConfigFile);
        }

        /// <summary>
        /// Writes data to the specified file path
        /// </summary>
        /// <param name="origWithHeader">Full text</param>
        /// <param name="savePath">Save path</param>
        internal static void WriteOriginalImageFile(string origWithHeader, string savePath)
        {
            var localFileWriter = new StreamWriter(savePath);
            localFileWriter.Write(origWithHeader);
            localFileWriter.Flush();
            localFileWriter.Close();

            Tools.ShowMessage(string.Format("Original imagae saved as: {0}", savePath));
        }

        /// <summary>
        /// Creates xmb file of list of claim IDs
        /// </summary>
        /// <param name="siteID">Site ID</param>
        /// <param name="claimIDs">List of claim IDs</param>
        /// <param name="savePath">Path to save xmb</param>
        internal static void WriteXMB(int siteID, ArrayList claimIDs, string savePath)
        {
            XactiMed.XApps.ResourceDB resDB = new XactiMed.XApps.ResourceDB();

            resDB.ConnectionString = Tools.GetConnectionString();
            ClaimDB claimdb = new ClaimDB(resDB);
            XmlBatchFileWriter xmbWriter = new XmlBatchFileWriter(savePath);
            foreach (string claimID in claimIDs)
            {
                int claim = int.Parse(claimID);
                string claimXML = claimdb.GetClaimXml(XClaimResource.PriDatabase, siteID, claim);
                if (claimXML == string.Empty)
                    claimXML = claimdb.GetClaimXml(XClaimResource.ArcDatabase, siteID, claim);
                xmbWriter.AddDocumentStr(claimXML);
            }

            xmbWriter.Close();

            Tools.ShowMessage(string.Format("XMB saved as: {0}", savePath));
        }

        /// <summary>
        /// Creates atest.bat file in the specified path
        /// </summary>
        /// <param name="localPath">Path where file will be created</param>
        private static void CreateATestBat(string localPath)
        {
            const string batchFileText = "del test.*\ndel *.log\n\ncopy original.837 test.txt\n\n" + @"C:\XactiMed.NET\XApps\trunk\Native\Base\_TESTING\AnyConvTest\Debug\AnyConvTest.exe -s Spec.xml -i test.txt -n stdin.xml";
            var localFileWiter = new StreamWriter(localPath + @"\atest.bat");
            localFileWiter.Write(batchFileText);
            localFileWiter.Flush();
            localFileWiter.Close();
        }

        /// <summary>
        /// Creates ptest.bat file in the specified path
        /// </summary>
        /// <param name="localPath">Path where file will be created</param>
        private static void CreatePTestBat(string localPath)
        {
            const string batchFileText = "del test.*\ndel *.log\n\ncopy original.image test.txt\n\n" + @"C:\XactiMed.NET\XApps\trunk\Native\Base\_TESTING\PIConvTest\Debug\PIConvTest.exe -s Spec.xml -m Map.xml -i test.txt -n stdin.xml";
            var localFileWiter = new StreamWriter(localPath + @"\ptest.bat");
            localFileWiter.Write(batchFileText);
            localFileWiter.Flush();
            localFileWiter.Close();
        }

        /// <summary>
        /// Creates stdin.xml file in the specified path
        /// </summary>
        /// <param name="localPath">Path where file will be created</param>
        /// <param name="siteID">Site ID</param>
        private static void CreateStdInxml(string localPath, int siteID)
        {
            string filetext = string.Format(@"<MSG><SID>{0}</SID><DATA><CL><ROOTDIR>\Customerss</ROOTDIR></CL></DATA></MSG>", siteID);
            var localFileWiter = new StreamWriter(localPath + @"\StdIn.xml");
            localFileWiter.Write(filetext);
            localFileWiter.Flush();
            localFileWiter.Close();
        }

        /// <summary>
        /// Creates atest.bat, ptest.bat, tests.bat and stdin.xml
        /// </summary>
        /// <param name="localPath">Path where files will be created</param>
        /// <param name="siteID">Site ID</param>
        private static void CreateTestBatFilesAndStdIn(string localPath, int siteID)
        {
            CreateATestBat(localPath);
            CreatePTestBat(localPath);
            CreateTestS(localPath, siteID);
            CreateStdInxml(localPath, siteID);
        }

        /// <summary>
        /// Creates tests.bat file in the specified path
        /// </summary>
        /// <param name="localPath">Path where file will be created</param>
        /// <param name="siteID">Site ID</param>
        private static void CreateTestS(string localPath, int siteID)
        {
            string batchFileText = "copy original.xmb test.xmb\r\n\r\n" + string.Format(@"C:\xactimed\Bin\XClaim.Prep\SecondaryClaimPrep\SecondaryClaimPrep.exe -siteid {0} -importtype 8 -xmb test.xmb -Testing", siteID);
            var localFileWiter = new StreamWriter(localPath + @"\tests.bat");
            localFileWiter.Write(batchFileText);
            localFileWiter.Flush();
            localFileWiter.Close();
        }

        /// <summary>
        /// Deletes an arry of files
        /// </summary>
        /// <param name="rgFiles">Files to delete</param>
        private static void DeleteFile(FileInfo[] rgFiles)
        {
            for (int i = 0; i < rgFiles.Length; i++)
            {
                FileInfo file = rgFiles[i];
                string strName = file.FullName.ToUpper();
                File.SetAttributes(strName, FileAttributes.Normal);
                file.Delete();
            }
        }

        /// <summary>
        /// Creates reports folder in the sites CustomerSS folder
        /// </summary>
        /// <param name="siteID">Site ID</param>
        internal static void AddReportFolder(int siteID)
        {
            Directory.CreateDirectory(string.Format(@"C:\CustomerSS\{0}\XClaim\Reports", siteID));
        }

        /// <summary>
        /// Returns a list of import IDs from the begin date to current
        /// </summary>
        /// <param name="siteID">Site ID</param>
        /// <param name="beginOfDay">Day you want to begin at</param>
        /// <returns>ArrayList of integers</returns>
        internal static ArrayList GetImportsByDate(int siteID, DateTime beginOfDay)
        {
            ArrayList imports = new ArrayList();
            var resource = string.Empty;
            var connString = GetResource(siteID, 1, 0, 1);

            using (SqlConnection conn = new SqlConnection(connString))
            {
                string sQuery = string.Format("SELECT ImportID FROM Imports WHERE SiteID = {0} and Date > '{1}' and ImportType = 0 and Amount > 0", siteID, beginOfDay.ToShortDateString());
                using (SqlCommand cmd = new SqlCommand(sQuery, conn))
                {
                    conn.Open();
                    var reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        imports.Add(reader[0]);
                    }
                    conn.Close();
                }
            }

            return imports;
        }

        internal static void SaveImportReport(int siteID, string importID, string fileLocation)
        {
            string importReportLocation = GetImportReportPath(siteID, importID);
            string destination = fileLocation + string.Format(@"\{0}Report.xml", importID);
            DecompressPkFile(importReportLocation, destination);
        }

        private static string GetImportReportPath(int siteID, string importID)
        {
            return (GetSiteLocaion(siteID) + @"\XClaim\Reports\Import\" + GetImportReportName(siteID, importID));
        }

        private static string GetImportReportName(int siteID, string importID)
        {
            var resource = string.Empty;
            var connString = GetResource(siteID, 1, 0, 1);

            using (SqlConnection conn = new SqlConnection(connString))
            {
                string sQuery = string.Format("select RptFile from Imports where SiteID = {0} and ImportID = {1}", siteID, importID);
                using (SqlCommand cmd = new SqlCommand(sQuery, conn))
                {
                    conn.Open();
                    object returnval = cmd.ExecuteScalar();
                    if (returnval != null)
                    {
                        resource = returnval.ToString();
                    }
                    conn.Close();
                }
            }

            return resource;
        }

        internal static string GetWorkingFilesDirectory(int siteID)
        {
            return GetResource(siteID, 0, 36, 4);
        }

        internal static string GetFTPDirectory(int siteID)
        {
            return GetResource(siteID, 0, 8, 4);
        }

        internal static void CreateCustomerSSFileStructure(int SiteID)
        {
            throw new NotImplementedException();
        }

        internal static string GetPrimaryClaimDBConnectionString(int SiteID)
        {
            return Tools.GetResource(SiteID, 1, 0, 1);
        }

        internal static string GetArchiveClaimDBConnectionString(int SiteID)
        {
            return Tools.GetResource(SiteID, 1, 1, 1);
        }

        internal static List<string> GetListOfSites()
        {
            List<string> sites = new List<string>();
            using (SqlConnection conn = new SqlConnection(GetConnectionString()))
            {
                string sQuery = "SELECT DISTINCT SiteID FROM Resources";
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
                            sites.Add(row["SiteID"].ToString());
                        }
                    }
                }
            }
            return sites;
        }

        internal static string GetActivityLogDBConnectionString(int siteID)
        {
            return GetResource(siteID, 0, 2, 1);
        }

        internal static void ShowMessage(string title, string message)
        {
            System.Windows.MessageBox.Show(message, title);
        }
    }
}