/*******************************************************************************
Copyright 2019 Oliver Heil, heilbIT

This file is part of doTimeTable.
Official home page https://www.dotimetable.de

doTimeTable is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

doTimeTable is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with doTimeTable.  If not, see <https://www.gnu.org/licenses/>.

*******************************************************************************/
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using System.Runtime.InteropServices;
using System.Resources;
using System.Net;
using System.Net.NetworkInformation;
using System.IO;
using System.IO.Compression;
using System.Threading;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Script.Serialization;
using IWshRuntimeLibrary;

namespace doTimeTable
{
    public partial class Form1 : Form
    {
        static public ResourceManager LocRM = new ResourceManager("doTimeTable.WinFormStrings", typeof(Form1).Assembly);

#if DEBUG
        static public string juliaDir = "JLBIN" + Path.DirectorySeparatorChar + "julia-1.1.0" + Path.DirectorySeparatorChar + "bin";
        static public string juliaScriptDir = "JLSCRIPT" + Path.DirectorySeparatorChar + "julia-script";
#else
        static public string juliaDir = "julia-1.1.0" + Path.DirectorySeparatorChar + "bin";
        static public string juliaScriptDir = "julia-script";
#endif
        public bool julia_ok = false;

        private string geometry_komponente = "editMain";
        private bool startup_log_window = false;
        private bool geometry_changed = false;
        private string geometry;

        static public string version;

        static public string myClientRuntimeID;
        static public string myClientIP;
        static public string myClientHostname;
        static public string applicationDir;
        static public string currentWindowsUser;

        static public Form1 myself;

        static public bool app_ready = false;
        static public bool app_restart = false;
        static public bool app_stop = false;
        static public string app_logFile = "";
        static public Form5 splash = null;
        static public Crypto crypto = null;

        //static public bool registered = false;
        //static public string registrationID = "";

        static public Log logWindow;
        static public State state;
        static public Config config;
        static public Database database;
        static public Projects projects;

        static public bool isProjectListEmpty = true;

        static public List<IntPtr> openWindows = new List<IntPtr>();
        static public bool skipActivateEvent = false;

        private int ignoreCellValueChanged = 0;
        //private DataGridViewRow currentCalculatedRow = null;
        private int currentCalculatedRow = -1;

        private readonly Dictionary<string, Form4> openViews = new Dictionary<string, Form4>();

        static readonly HttpClient httpClient = new HttpClient();
        public List<string> versionsList = new List<string>();
        private readonly Thread autoUpdate = null;
        bool newVersionDownloaded = false;

        private readonly List<Form2> myForm2s = new List<Form2>();

        public enum SubformType
        {
            UNDEF = -1,
            DAYSHOURS,
            SUBJECT,
            CLASSES,
            TEACHER,
            TEACHERTOCLASS,
            CLASSTOSUBJECTPRESET,
            CONFIG,
            CALCULATE,
            VIEW,
        }
        public SubformType doubleClickSubformType = SubformType.DAYSHOURS;

        protected override void WndProc(ref Message m)
        {
            const int WM_SYSCOMMAND = 0x0112;
            const int SC_MINIMIZE = 0xF020;
            switch (m.Msg)
            {
                case WM_SYSCOMMAND:
                    int command = m.WParam.ToInt32() & 0xfff0;
                    if (command == SC_MINIMIZE)
                    {
                        Form1.logWindow.WindowState = FormWindowState.Minimized;
                        foreach (Form2 form2 in Form2.myInstantiations)
                        {
                            form2.WindowState = FormWindowState.Minimized;
                        }
                        if (
                            Progress.currentProgress != null &&
                            //Progress.currentProgress.Visible &&
                            //Progress.currentProgress.WindowState == FormWindowState.Minimized &&
                            true
                            )
                        {
                            Progress.currentProgress.WindowState = FormWindowState.Minimized;
                        }
                    }
                    break;
            }
            base.WndProc(ref m);
        }

        public Form1()
        {
            //skipActivateEvent = true;
            myself = this;

            //           major.minor.build.release
            //version = "0.1.000.000";
            // database changes => minor + 1 in Properties -> AssemblyInfo.cs
            version = Application.ProductVersion;

            string ip = GetLocalIPAddress(NetworkInterfaceType.Wireless80211);
            if (ip == "")
            {
                ip = GetLocalIPAddress(NetworkInterfaceType.Ethernet);
            }
            myClientRuntimeID = Guid.NewGuid().ToString();
            myClientIP = ip;
            myClientHostname = System.Net.Dns.GetHostName();

            currentWindowsUser = System.Security.Principal.WindowsIdentity.GetCurrent().Name;

            /*
            //trying to look for new application version
            string test_version_path = "t:\\doTimeTable\\";
            string test_version = test_version_path + "doTimeTable.exe";
            string current_version = Application.StartupPath + "\\" + Application.ProductName + ".exe";
            string old_version = Application.StartupPath + "\\" + Application.ProductName + "_old.exe";
            FileInfo fi_new = new FileInfo(test_version);
            FileInfo fi_current = new FileInfo(current_version);
            FileInfo fi_old = new FileInfo(old_version);
            if (fi_old.Exists)
            {
                File.Delete(old_version);
            }

            if (!fi_new.Exists)
            {
                //string msg = "Master application can not be found.";
                //MessageBox.Show(msg, "Information", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                //app_stop = true;
                //return;
            }

            if (fi_new.LastWriteTime > fi_current.LastWriteTime)
            {
                //newer version exists
                File.Move(current_version, old_version);
                File.Copy(test_version, current_version, true);
                app_restart = true;
            }
            */

            InitializeComponent();

            this.Text = LocRM.GetString("String1");
            this.fileToolStripMenuItem.Text = LocRM.GetString("String2");
            this.editToolStripMenuItem.Text = LocRM.GetString("String129");
            this.newToolStripMenuItem.Text = LocRM.GetString("String4");
            this.newToolStripMenuItem2.Text = LocRM.GetString("String4");
            //this.openToolStripMenuItem.Text = LocRM.GetString("String5");
            this.closeToolStripMenuItem.Text = LocRM.GetString("String6");
            this.viewToolStripMenuItem.Text = LocRM.GetString("String7");
            this.log_window.Text = LocRM.GetString("String8");
            this.deleteToolStripMenuItem.Text = LocRM.GetString("String12");
            this.newToolStripMenuItem1.Text = LocRM.GetString("String13");
            this.deleteToolStripMenuItem1.Text = LocRM.GetString("String12");
            this.deleteToolStripMenuItem1.Enabled = false;
            this.editCreateToolStripMenuItem.Text = LocRM.GetString("String17");
            this.editCreateToolStripMenuItem1.Text = LocRM.GetString("String17");
            this.daysHoursToolStripMenuItem.Text = LocRM.GetString("String60");
            this.daysHoursToolStripMenuItem1.Text = LocRM.GetString("String60");
            this.daysHoursToolStripMenuItem.ToolTipText = LocRM.GetString("String19");
            this.daysHoursToolStripMenuItem1.ToolTipText = LocRM.GetString("String19");
            this.subjectToolStripMenuItem.Text = LocRM.GetString("String18");
            this.subjectsToolStripMenuItem.Text = LocRM.GetString("String18");
            this.subjectToolStripMenuItem.ToolTipText = LocRM.GetString("String19");
            this.subjectsToolStripMenuItem.ToolTipText = LocRM.GetString("String19");
            this.classesToolStripMenuItem.Text = LocRM.GetString("String48");
            this.classesToolStripMenuItem1.Text = LocRM.GetString("String48");
            this.classesToolStripMenuItem.ToolTipText = LocRM.GetString("String19");
            this.classesToolStripMenuItem1.ToolTipText = LocRM.GetString("String19");
            this.teacherToolStripMenuItem.Text = LocRM.GetString("String86");
            this.teacherToolStripMenuItem1.Text = LocRM.GetString("String86");
            this.teacherToolStripMenuItem.ToolTipText = LocRM.GetString("String19");
            this.teacherToolStripMenuItem1.ToolTipText = LocRM.GetString("String19");
            this.teachertoClassToolStripMenuItem.Text = LocRM.GetString("String102");
            this.teachertoClassToolStripMenuItem1.Text = LocRM.GetString("String102");
            this.teachertoClassToolStripMenuItem.ToolTipText = LocRM.GetString("String19");
            this.teachertoClassToolStripMenuItem1.ToolTipText = LocRM.GetString("String19");
            this.classtoSubjectToolStripMenuItem.Text = LocRM.GetString("String111");
            this.classtoSubjectToolStripMenuItem1.Text = LocRM.GetString("String111");
            this.classtoSubjectToolStripMenuItem.ToolTipText = LocRM.GetString("String19");
            this.classtoSubjectToolStripMenuItem1.ToolTipText = LocRM.GetString("String19");
            this.runToolStripMenuItem.Text = LocRM.GetString("String130");
            this.findRosterToolStripMenuItem.Text = LocRM.GetString("String130");
            this.copyToolStripMenuItem.Text = LocRM.GetString("String99");
            this.copyToolStripMenuItem1.Text = LocRM.GetString("String99");
            this.viewRosterToolStripMenuItem.Text = LocRM.GetString("String144");
            this.viewRosterToolStripMenuItem.Font = new Font(this.viewRosterToolStripMenuItem.Font, FontStyle.Regular);
            this.viewRosterToolStripMenuItem1.Text = LocRM.GetString("String144");
            this.viewRosterToolStripMenuItem1.Font = new Font(this.viewRosterToolStripMenuItem.Font, FontStyle.Regular);
            this.configurationToolStripMenuItem.Text = LocRM.GetString("String152");
            this.configurationToolStripMenuItem1.Text = LocRM.GetString("String152");
            this.exportAnonToolStripMenuItem.Text = LocRM.GetString("String161");
            this.exportToolStripMenuItem.Text = LocRM.GetString("String149");
            this.aboutToolStripMenuItem.Text = LocRM.GetString("String169");

            this.installNewVersionToolStripMenuItem.Text = LocRM.GetString("String179");
            this.installNewVersionToolStripMenuItem.Visible = false;
            this.installNewVersionToolStripMenuItem.Enabled = false;

            state = new State();

            applicationDir = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            if (applicationDir.Length == 0)
            {
                applicationDir = System.Environment.GetEnvironmentVariable("LOCALAPPDATA");
            }
            applicationDir = applicationDir + Path.DirectorySeparatorChar + Application.ProductName;


            logWindow = new Log();
            Log.Log_window_changed += new Log.log_window_changed_delegate(Log_window_changed);
            logWindow.Show();
            string log_output;

            log_output = DateTime.Now.ToString();
            logWindow.Write_to_log(ref log_output);
            log_output = "Application initialization, version " + version;
            logWindow.Write_to_log(ref log_output);
            log_output = "Current working path: " + Environment.CurrentDirectory;
            logWindow.Write_to_log(ref log_output);

            crypto = new Crypto();

            string tmpJuliaDir = Environment.CurrentDirectory;
            int backCount = 0;
            while (backCount < 5 && !System.IO.File.Exists(tmpJuliaDir + Path.DirectorySeparatorChar + juliaDir + Path.DirectorySeparatorChar + "libjulia.dll"))
            {
                tmpJuliaDir = tmpJuliaDir + Path.DirectorySeparatorChar + "..";
                backCount++;
            }
            if (!System.IO.File.Exists(tmpJuliaDir + Path.DirectorySeparatorChar + juliaDir + Path.DirectorySeparatorChar + "libjulia.dll"))
            {
                log_output = "julia library not found: " + juliaDir + Path.DirectorySeparatorChar + "libjulia.dll";
                logWindow.Write_to_log(ref log_output);
                julia_ok = false;
            }
            else
            {
                juliaDir = tmpJuliaDir + Path.DirectorySeparatorChar + juliaDir + Path.DirectorySeparatorChar;
                log_output = "julia library found: " + juliaDir + "libjulia.dll";
                logWindow.Write_to_log(ref log_output);
                log_output = juliaDir + "julia.exe";
                logWindow.Write_to_log(ref log_output);
                log_output = juliaDir.Replace("\\", "/") + "julia.exe";
                logWindow.Write_to_log(ref log_output);
                julia_ok = true;
            }

            string tmpJuliaScriptDir = Environment.CurrentDirectory;
            backCount = 0;
            while (backCount < 5 && !System.IO.File.Exists(tmpJuliaScriptDir + Path.DirectorySeparatorChar + juliaScriptDir + Path.DirectorySeparatorChar + "roster.jl"))
            {
                tmpJuliaScriptDir = tmpJuliaScriptDir + Path.DirectorySeparatorChar + "..";
                backCount++;
            }
            if (!System.IO.File.Exists(tmpJuliaScriptDir + Path.DirectorySeparatorChar + juliaScriptDir + Path.DirectorySeparatorChar + "roster.jl"))
            {
                log_output = "julia script not found: " + juliaScriptDir + Path.DirectorySeparatorChar + "roster.jl";
                logWindow.Write_to_log(ref log_output);
                julia_ok = false;
            }
            else
            {
                juliaScriptDir = tmpJuliaScriptDir + Path.DirectorySeparatorChar + juliaScriptDir + Path.DirectorySeparatorChar;
                log_output = "julia script found: " + juliaScriptDir + "roster.jl";
                logWindow.Write_to_log(ref log_output);
                julia_ok = true;
            }

            if (!Directory.Exists(applicationDir))
            {
                log_output = "info: creating directory " + applicationDir;
                logWindow.Write_to_log(ref log_output);
                try
                {
                    Directory.CreateDirectory(applicationDir);
                }
                catch (Exception)
                {
                    log_output = "warning: directory " + applicationDir + " does not exist and can't be created";
                    logWindow.Write_to_log(ref log_output);
                }
            }

            //Reading configurations
            config = new Config(applicationDir);

            bool skipScriptVerification = false;
            if (config.config_xml.DocumentElement.GetElementsByTagName("skip_script_verification").Count == 0)
            {
                System.Xml.XmlElement skip_script_verification = config.config_xml.CreateElement("skip_script_verification");
                skip_script_verification.InnerXml = "false";
                System.Xml.XmlElement root = config.config_xml.DocumentElement;
                root.AppendChild(skip_script_verification);
                Form1.config.Save_config_file();
            }
            if (config.config_xml.DocumentElement["skip_script_verification"].InnerXml == "true")
            {
                skipScriptVerification = true;
            }

            bool juliaOriginal = crypto.VerifyFile(juliaScriptDir + "roster.jl");
            if (!juliaOriginal)
            {
#if DEBUG
                crypto.SignFile(juliaScriptDir + "roster.jl");
#endif
                if (!skipScriptVerification)
                {
                    log_output = "julia script file not verified, aborting application";
                    logWindow.Write_to_log(ref log_output);
                    DialogResult r = MessageBox.Show(LocRM.GetString("String175") +"\n" + LocRM.GetString("String185"), LocRM.GetString("String177"), MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    if( r == DialogResult.Yes)
                    {
                        logWindow.SelectAll();
                        logWindow.CopySelectedToClipboard();
                    }
                    Form1.app_stop = true;
                    return;
                }
                else
                {
                    log_output = "julia script file not verified but ignored";
                    logWindow.Write_to_log(ref log_output);

                    if (MessageBox.Show(LocRM.GetString("String176"), LocRM.GetString("String177"), MessageBoxButtons.OKCancel, MessageBoxIcon.Question) == DialogResult.Cancel)
                    {
                        Form1.app_stop = true;
                        return;
                    }
                }
            }
            else
            {
                log_output = "julia script file verified";
                logWindow.Write_to_log(ref log_output);
            }

            bool juliaDllOriginal = crypto.VerifyFile(juliaDir + "libjulia.dll");
            if (!juliaDllOriginal)
            {
#if DEBUG
                crypto.SignFile(juliaDir + "libjulia.dll");
#endif
                if (!skipScriptVerification)
                {
                    log_output = "julia dll file not verified, aborting application";
                    logWindow.Write_to_log(ref log_output);
                    DialogResult r = MessageBox.Show(LocRM.GetString("String183") + "\n" + LocRM.GetString("String185"), LocRM.GetString("String177"), MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    if (r == DialogResult.Yes)
                    {
                        logWindow.SelectAll();
                        logWindow.CopySelectedToClipboard();
                    }
                    Form1.app_stop = true;
                    return;
                }
                else
                {
                    log_output = "julia dll file not verified but ignored";
                    logWindow.Write_to_log(ref log_output);

                    if (MessageBox.Show(LocRM.GetString("String184"), LocRM.GetString("String177"), MessageBoxButtons.OKCancel, MessageBoxIcon.Question) == DialogResult.Cancel)
                    {
                        Form1.app_stop = true;
                        return;
                    }
                }
            }
            else
            {
                log_output = "julia dll file verified";
                logWindow.Write_to_log(ref log_output);
            }

            crypto.CheckRegistration();
            if (crypto.registered)
            {
                crypto.GetHardwareFingerprint();
                if (crypto.fingerPrint != null)
                {
                    log_output = "Your fingerprint is " + crypto.fingerPrint;
                    logWindow.Write_to_log(ref log_output);
                }
                this.installNewVersionToolStripMenuItem.Visible = true;
            }


#if DEBUG
            //if (!crypto.registered)
            //{
            //crypto.DummyCanBeRemoved2();
            //}
#endif

            //if (config.config_xml == null)
            //{
            //    string msg = "Config file could not be read.";
            //    MessageBox.Show(msg, "Information", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
            //    app_stop = true;
            //    return;
            //}
            logWindow.Update_config();

            database = new Database();
            try
            {
                database.Connect();
            }
            catch (Exception)
            {
                state.current_state = State.State_type.Error;
            }

            if (state.current_state >= State.State_type.Error)
            {
                string msg = "No database connection. Running read-only.";
                MessageBox.Show(msg, "Information", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                //Dispose(true);
            }

            config.Get_values();

            logWindow.Update_geometry();
            logWindow.Log_setVisibleChanged();

            log_output = "Database version: " + database.version;
            logWindow.Write_to_log(ref log_output);

            logWindow.Hide();
            this.log_window.Checked = false;


            //initialSetupDataGridView();

            projects = new Projects();

            UpdateDataGridView();

            bool auto_update_disabled = false;
            if (config.config_xml.DocumentElement.GetElementsByTagName("disable_auto_update").Count == 0)
            {
                System.Xml.XmlElement disable_auto_update = config.config_xml.CreateElement("disable_auto_update");
                disable_auto_update.InnerXml = "false";
                System.Xml.XmlElement root = config.config_xml.DocumentElement;
                root.AppendChild(disable_auto_update);
                config.Save_config_file();
            }
            if (config.config_xml.DocumentElement["disable_auto_update"].InnerXml == "true")
            {
                auto_update_disabled = true;
            }

            if (crypto.registered && !auto_update_disabled)
            {
                autoUpdate = new Thread(new ThreadStart(AutoUpdateThread));
                autoUpdate.Start();
            }

        }

        private async void AutoUpdateThread()
        {
            string log_output;
            string releasesURL = "https://api.github.com/repos/oheil/doTimeTable/releases";
            string responseBody;
            try
            {
                //httpClient.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:69.0) Gecko/20100101 Firefox/69.0");
                httpClient.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "oheil/doTimeTable");

                HttpResponseMessage response = await httpClient.GetAsync(new Uri(releasesURL));
                response.EnsureSuccessStatusCode();

                responseBody = await response.Content.ReadAsStringAsync();
                JavaScriptSerializer js = new JavaScriptSerializer();
                dynamic result = js.DeserializeObject(responseBody);
                foreach (dynamic obj in result)
                {
                    Dictionary<string, dynamic> dict = (Dictionary<string, dynamic>)obj;
                    string version = dict["tag_name"];
                    version = version.Replace("v", "");
                    versionsList.Add(version);
                }
                crypto.GetNewVersion();
                if (crypto.newVersionAvailable && crypto.newVersion != null)
                {
                    EnableInstallMenuItem_fromThread();
                }
            }
            catch (HttpRequestException e)
            {
                log_output = "retreiving releases failed:";
                logWindow.Write_to_log_fromThread(log_output);
                log_output = e.ToString();
                logWindow.Write_to_log_fromThread(log_output);
                log_output = "releases URL: " + releasesURL;
                logWindow.Write_to_log_fromThread(log_output);
            }
#if DEBUG
            EnableInstallMenuItem_fromThread();
#endif
        }

        public void EnableInstallMenuItem()
        {
            if (!this.installNewVersionToolStripMenuItem.Visible) {
                this.installNewVersionToolStripMenuItem.Visible = true;
            }
            if (!this.installNewVersionToolStripMenuItem.Enabled)
            {
                this.installNewVersionToolStripMenuItem.Enabled = true;
                if (CheckDownloadedNewVersion())
                {
                    string log_output = "found verified installer";
                    logWindow.Write_to_log_fromThread(log_output);
                    this.installNewVersionToolStripMenuItem.Text = LocRM.GetString("String180");
                    newVersionDownloaded = true;
                }
            }
        }

        public void EnableInstallMenuItem_fromThread()
        {
            if (this.InvokeRequired)
            {
                this.BeginInvoke((Action)EnableInstallMenuItem_fromThread);
            }
            else
            {
                EnableInstallMenuItem();
            }
            return;
        }

        public void UpdateDataGridView()
        {
            dataGridView1.Sort(dataGridView1.Columns[3], System.ComponentModel.ListSortDirection.Ascending);
            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                SetStatusString(row);
                string id;
                if (row.Cells.Count > 1
                    && row.Cells[1] != null
                    && row.Cells[1].Value != null
                    && row.Cells[1].Value.ToString() != "")
                {
                    id = row.Cells[1].Value.ToString();
                    row.Cells[0].ToolTipText = id;
                }
            }
            dataGridView1.ClearSelection();
        }

        public List<string> GetAllIDsFromDataGridView()
        {
            List<string> allIDs = new List<String>();
            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                if (row.Cells.Count > 1
                    && row.Cells[1] != null
                    && row.Cells[1].Value != null
                    && row.Cells[1].Value.ToString() != "")
                {
                    allIDs.Add(row.Cells[1].Value.ToString());
                }
            }
            return allIDs;
        }

        public void InitialSetupDataGridView()
        {
            ignoreCellValueChanged++;

            isProjectListEmpty = true;
            dataGridView1.Rows.Clear();
            dataGridView1.ColumnCount = 1;
            dataGridView1.Columns[0].Name = LocRM.GetString("String10");
            FillDataGridViewWithEmptyLines();
            //dataGridView1.ClearSelection();
            dataGridView1.Refresh();

            string[] row = { Form1.LocRM.GetString("String11") };
            Form1.myself.AddRowToDataGridView(row);

            ignoreCellValueChanged--;
        }

        public void FinalSetupDataGridView()
        {
            ignoreCellValueChanged++;

            isProjectListEmpty = false;
            dataGridView1.Rows.Clear();
            dataGridView1.ColumnCount = 6;
            dataGridView1.Columns[0].Name = LocRM.GetString("String10");
            dataGridView1.Columns[1].Name = "";
            dataGridView1.Columns[2].Name = LocRM.GetString("String140");
            dataGridView1.Columns[3].Name = LocRM.GetString("String142");
            dataGridView1.Columns[4].Name = LocRM.GetString("String143");
            dataGridView1.Columns[5].Name = "";
            FillDataGridViewWithEmptyLines();
            //dataGridView1.ClearSelection();
            dataGridView1.Refresh();

            dataGridView1.Columns[1].Visible = false;
            dataGridView1.Columns[2].DefaultCellStyle.WrapMode = DataGridViewTriState.True;
            dataGridView1.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None;
            dataGridView1.Columns[3].ReadOnly = true;
            dataGridView1.Columns[4].ReadOnly = true;
            dataGridView1.Columns[5].ReadOnly = true;
            dataGridView1.Columns[5].Visible = false;

            ignoreCellValueChanged--;
        }

        private void FillDataGridViewWithEmptyLines()
        {
            ignoreCellValueChanged++;

            int rowIndex = 0;
            string[] row = { "", "", "", "", "" };
            if (dataGridView1.Rows.Count == 0)
            {
                dataGridView1.Rows.Add(row);
                dataGridView1.FirstDisplayedScrollingRowIndex = rowIndex;
            }
            rowIndex = dataGridView1.Rows.Count - 1;
            int visibleRowsCount = dataGridView1.DisplayedRowCount(true);
            while (rowIndex < visibleRowsCount)
            {
                rowIndex = dataGridView1.Rows.Add(row);
                visibleRowsCount = dataGridView1.DisplayedRowCount(true);
            }
            if (dataGridView1.Rows[rowIndex].Cells[0].Value == null || dataGridView1.Rows[rowIndex].Cells[0].Value.ToString() == "")
            {
                dataGridView1.Rows.RemoveAt(rowIndex);
            }
            SetupDataGridViewScrollBars();

            ignoreCellValueChanged--;
        }

        private void SetupDataGridViewScrollBars()
        {
            int visibleRowsCount = dataGridView1.DisplayedRowCount(true);
            if (dataGridView1.Rows.Count > visibleRowsCount)
            {
                dataGridView1.ScrollBars = ScrollBars.Both;
            }
            else
            {
                dataGridView1.ScrollBars = ScrollBars.Horizontal;
            }
            return;
        }

        private int GetNextDataGridViewIndex()
        {
            int rowIndex = -1;
            if (dataGridView1.Rows.Count > 0)
            {
                rowIndex = 0;
                while (rowIndex <= dataGridView1.Rows.Count - 1 && dataGridView1.Rows[rowIndex].Cells[0].Value != null && !dataGridView1.Rows[rowIndex].Cells[0].Value.ToString().Equals(""))
                {
                    rowIndex++;
                }
            }
            return rowIndex;
        }

        public void AddRowToDataGridView(string[] row, int rowIndex = -1)
        {
            ignoreCellValueChanged++;

            List<string> localRow = new List<string>();
            for (int index = 0; index < dataGridView1.ColumnCount; index++)
            {
                if (index < row.Length)
                {
                    localRow.Add(row[index]);
                }
                else
                {
                    localRow.Add("");
                }
            }
            if (localRow.Count != dataGridView1.ColumnCount)
            {
                string log_output = "can't add row with column count " + row.Length;
                logWindow.Write_to_log(ref log_output);
            }
            else
            {
                if (rowIndex < 0)
                {
                    rowIndex = GetNextDataGridViewIndex();
                }
                if (rowIndex < 0)
                {
                    dataGridView1.Rows.Add(localRow);
                    dataGridView1.FirstDisplayedScrollingRowIndex = dataGridView1.Rows.Count;
                }
                else
                {
                    foreach (DataGridViewCell cell in dataGridView1.Rows[rowIndex].Cells)
                    {
                        cell.Value = localRow[cell.ColumnIndex];
                    }
                }
            }
            SetupDataGridViewScrollBars();

            ignoreCellValueChanged--;
        }

        private void DataGridView1_SelectionChanged(object sender, EventArgs e)
        {
            bool selectedNonEmpty = false;
            foreach (DataGridViewRow row in dataGridView1.SelectedRows)
            {
                if (row.Cells.Count > 1 && row.Cells[0].Value != null && !row.Cells[0].Value.ToString().Equals(""))
                {
                    selectedNonEmpty = true;
                }
            }
            if (selectedNonEmpty)
            {
                this.deleteToolStripMenuItem1.Enabled = true;
                deleteToolStripMenuItem.Enabled = true;
            }
            else
            {
                this.deleteToolStripMenuItem1.Enabled = false;
                deleteToolStripMenuItem.Enabled = false;
            }
            if (selectedNonEmpty && dataGridView1.SelectedRows.Count == 1)
            {
                this.editCreateToolStripMenuItem.Enabled = true;
                this.editCreateToolStripMenuItem1.Enabled = true;

                this.daysHoursToolStripMenuItem.ToolTipText = "";
                this.daysHoursToolStripMenuItem1.ToolTipText = "";

                this.subjectToolStripMenuItem.ToolTipText = "";
                this.subjectsToolStripMenuItem.ToolTipText = "";

                this.classesToolStripMenuItem.ToolTipText = "";
                this.classesToolStripMenuItem1.ToolTipText = "";

                this.teacherToolStripMenuItem.ToolTipText = "";
                this.teacherToolStripMenuItem1.ToolTipText = "";

                this.teachertoClassToolStripMenuItem.Enabled = true;
                this.teachertoClassToolStripMenuItem1.Enabled = true;
                this.teachertoClassToolStripMenuItem.ToolTipText = "";
                this.teachertoClassToolStripMenuItem1.ToolTipText = "";

                this.classtoSubjectToolStripMenuItem.Enabled = true;
                this.classtoSubjectToolStripMenuItem1.Enabled = true;
                this.classtoSubjectToolStripMenuItem.ToolTipText = "";
                this.classtoSubjectToolStripMenuItem1.ToolTipText = "";

                this.runToolStripMenuItem.ToolTipText = "";
                this.findRosterToolStripMenuItem.ToolTipText = "";

                this.copyToolStripMenuItem.Enabled = true;
                this.copyToolStripMenuItem1.Enabled = true;

                this.viewRosterToolStripMenuItem.Enabled = true;
                this.viewRosterToolStripMenuItem1.Enabled = true;

                exportAnonToolStripMenuItem.Enabled = true;
                exportAnonToolStripMenuItem.ToolTipText = "";
            }
            else
            {
                this.editCreateToolStripMenuItem.Enabled = true;
                this.editCreateToolStripMenuItem1.Enabled = true;

                this.daysHoursToolStripMenuItem.ToolTipText = LocRM.GetString("String19");
                this.daysHoursToolStripMenuItem1.ToolTipText = LocRM.GetString("String19");

                this.subjectToolStripMenuItem.ToolTipText = LocRM.GetString("String19");
                this.subjectsToolStripMenuItem.ToolTipText = LocRM.GetString("String19");

                this.classesToolStripMenuItem.ToolTipText = LocRM.GetString("String19");
                this.classesToolStripMenuItem1.ToolTipText = LocRM.GetString("String19");

                this.teacherToolStripMenuItem.ToolTipText = LocRM.GetString("String19");
                this.teacherToolStripMenuItem1.ToolTipText = LocRM.GetString("String19");

                this.teachertoClassToolStripMenuItem.Enabled = false;
                this.teachertoClassToolStripMenuItem1.Enabled = false;
                this.teachertoClassToolStripMenuItem.ToolTipText = LocRM.GetString("String19");
                this.teachertoClassToolStripMenuItem1.ToolTipText = LocRM.GetString("String19");

                this.classtoSubjectToolStripMenuItem.Enabled = false;
                this.classtoSubjectToolStripMenuItem1.Enabled = false;
                this.classtoSubjectToolStripMenuItem.ToolTipText = LocRM.GetString("String19");
                this.classtoSubjectToolStripMenuItem1.ToolTipText = LocRM.GetString("String19");

                this.runToolStripMenuItem.ToolTipText = LocRM.GetString("String19");
                this.findRosterToolStripMenuItem.ToolTipText = LocRM.GetString("String19");

                this.copyToolStripMenuItem.Enabled = false;
                this.copyToolStripMenuItem1.Enabled = false;

                this.viewRosterToolStripMenuItem.Enabled = false;
                this.viewRosterToolStripMenuItem1.Enabled = false;

                exportAnonToolStripMenuItem.Enabled = false;
                exportAnonToolStripMenuItem.ToolTipText = LocRM.GetString("String19");
            }
            DeactivateActivateMenuItems();
        }

        private void DataGridView1_SortCompare(object sender, DataGridViewSortCompareEventArgs e)
        {
            int col = e.Column.Index;
            DataGridViewRow row1 = dataGridView1.Rows[e.RowIndex1];
            DataGridViewRow row2 = dataGridView1.Rows[e.RowIndex2];
            string sv1 = "";
            if (row1.Cells[col].Value != null)
            {
                sv1 = row1.Cells[col].Value.ToString();
            }
            string sv2 = "";
            if (row2.Cells[col].Value != null)
            {
                sv2 = row2.Cells[col].Value.ToString();
            }
            if (col == 3)
            {
                DateTime dt1 = new DateTime();
                DateTime dt2 = new DateTime();
                if (sv1.Length > 0)
                {
                    dt1 = DateTime.Parse(sv1);
                }
                if (sv2.Length > 0)
                {
                    dt2 = DateTime.Parse(sv2);
                }
                e.SortResult = DateTime.Compare(dt1, dt2);
            }
            else
            {
                e.SortResult = System.String.Compare(sv1, sv2);
            }
            int so = -1;
            if (dataGridView1.SortOrder == SortOrder.Ascending)
            {
                so = 1;
            }
            if (sv1.Length == 0)
            {
                e.SortResult = so * 1;
            }
            if (sv2.Length == 0)
            {
                e.SortResult = so * -1;
            }
            e.Handled = true;
        }

        private void CloseToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void NewToolStripMenuItem_Click(object sender, EventArgs e)
        {
            projects.CreateNewProject();
        }

        public string GetLocalIPAddress(NetworkInterfaceType _type)
        {
            string output = "";
            foreach (NetworkInterface item in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (item.NetworkInterfaceType == _type && item.OperationalStatus == System.Net.NetworkInformation.OperationalStatus.Up)
                {
                    IPInterfaceProperties adapterProperties = item.GetIPProperties();

                    if (adapterProperties.GatewayAddresses.Count > 0)
                    {
                        foreach (UnicastIPAddressInformation ip in adapterProperties.UnicastAddresses)
                        {
                            if (ip.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                            {
                                output = ip.Address.ToString();
                            }
                        }
                    }
                }
            }
            return output;
        }

        private void Log_window_changed(object sender, EventArgs e)
        {
            if (logWindow.Is_visible())
            {
                this.log_window.Checked = true;
            }
            else
            {
                this.log_window.Checked = false;
            }
        }

        public bool IsOnScreen(Point p)
        {
            bool result = false;
            p.X += 20;
            p.Y += 20;
            Screen[] screens = Screen.AllScreens;
            foreach (Screen screen in screens)
            {
                if (screen.WorkingArea.Contains(p))
                {
                    result = true;
                }
            }

            return result;
        }
        private void Log_window_Click(object sender, EventArgs e)
        {
            Log_window_Click();
        }

        private void Log_window_Click()
        {
            if (logWindow == null)
            {
                logWindow = new Log();
                logWindow.Show();
                this.log_window.Checked = true;
            }
            else
            {
                geometry_changed = true;
                if (this.log_window.Checked == false)
                {
                    logWindow.Show();
                    logWindow.WindowState = FormWindowState.Normal;
                    //logWindow.Activate();
                    this.log_window.Checked = true;
                }
                else
                {
                    logWindow.Save_geometry();
                    logWindow.Hide();
                    this.log_window.Checked = false;
                }
            }
        }

        private void Save_geometry()
        {
            if (geometry_changed)
            {
                if (app_ready && geometry != null)
                {
                    geometry = this.Width.ToString() + ";";
                    geometry += this.Height.ToString() + ";";
                    geometry += this.Location.X.ToString() + ";";
                    geometry += this.Location.Y.ToString() + ";";
                    geometry += this.log_window.Checked + ";";
                    Form1.config.Save_config(ref geometry_komponente, ref geometry);
                }

                geometry_changed = false;
            }
        }

        public void Set_geometry()
        {
            geometry = Form1.config.Get_config(ref geometry_komponente);

            this.StartPosition = FormStartPosition.Manual;

            if (geometry != null && geometry.Length > 0)
            {
                string[] values = geometry.Split(';');

                int index;
                try
                {
                    index = 0; if (values.Length > index) { this.Width = Convert.ToInt32(geometry.Split(';')[index]); }
                    index = 1; if (values.Length > index) { this.Height = Convert.ToInt32(geometry.Split(';')[index]); }
                    index = 3; if (values.Length > index) { this.DesktopLocation = new Point(Convert.ToInt32(geometry.Split(';')[index - 1]), Convert.ToInt32(geometry.Split(';')[index])); }
                }
                catch
                {
                    string log_output = "Setting form1 geometry failed: <" + geometry + ">";
                    Form1.logWindow.Write_to_log(ref log_output);
                }
                if (!Form1.myself.IsOnScreen(this.DesktopLocation))
                {
                    this.DesktopLocation = new Point(0, 0);
                }
                if (geometry.Split(';')[4] == "True")
                {
                    startup_log_window = true;
                    //log_window_Click();
                }
            }

        }

        private void Form1_LocationChanged(object sender, EventArgs e)
        {
            if (this.Visible)
            {
                geometry_changed = true;
            }
        }

        private void Form1_SizeChanged(object sender, EventArgs e)
        {
            geometry_changed = true;
            FillDataGridViewWithEmptyLines();
            if (Progress.currentProgress.WindowOpen())
            {
                Progress.currentProgress.ActivateMe();
            }
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            Save_geometry();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            bool abortClose = false;
            foreach (Form2 form2 in Form2.myInstantiations)
            {
                abortClose = form2.CheckClosing();
            }
            if (abortClose)
            {
                e.Cancel = true;
                return;
            }

            string log_output = "Waiting for julia serverPipe thread to close";
            logWindow.Write_to_log(ref log_output);

            julia.NativeMethods.StopThreads();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            Set_geometry();
        }

        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            if (startup_log_window)
            {
                logWindow.Update_geometry();
                startup_log_window = false;
                Log_window_Click();
                this.Focus();
            }
        }

        private void Form1_Activated(object sender, EventArgs e)
        {
            if (!skipActivateEvent)
            {
                if (
                    this != null &&
                    this.Visible &&
                    this.WindowState == FormWindowState.Minimized &&
                    true
                    )
                {
                    Form1.logWindow.WindowState = FormWindowState.Normal;
                    if (
                        Progress.currentProgress != null &&
                        //Progress.currentProgress.Visible &&
                        //Progress.currentProgress.WindowState == FormWindowState.Minimized &&
                        true
                        )
                    {
                        Progress.currentProgress.WindowState = FormWindowState.Normal;
                    }
                    foreach (Form2 form2 in Form2.myInstantiations)
                    {
                        form2.WindowState = FormWindowState.Normal;
                    }
                }
                NativeMethods.DoOnProcess(this.Text);
            }
            skipActivateEvent = false;
        }

        private void DeleteSelectedProjects()
        {
            if (dataGridView1.SelectedRows.Count <= 0)
            {
                return;
            }

            if (MessageBox.Show(LocRM.GetString("String16"), LocRM.GetString("String12"), MessageBoxButtons.OKCancel, MessageBoxIcon.Question) == DialogResult.Cancel)
            {
                return;
            }

            ignoreCellValueChanged++;

            List<string> deleteIDs = new List<string>();
            foreach (DataGridViewRow row in dataGridView1.SelectedRows)
            {
                if (true
                    && row.Cells.Count >= 2
                    && row.Cells[1].Value != null
                    && row.Cells[1].Value.ToString().Length > 0
                    )
                {
                    deleteIDs.Add(row.Cells[1].Value.ToString());
                }
            }
            dataGridView1.ClearSelection();
            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                if (true
                    && row.Cells.Count >= 2
                    && row.Cells[1].Value != null
                    && row.Cells[1].Value.ToString().Length > 0
                    )
                {
                    string currentID = row.Cells[1].Value.ToString();
                    if (deleteIDs.Contains(currentID))
                    {
                        string[] emptyRow = { "", "", "", "", "" };
                        AddRowToDataGridView(emptyRow, row.Index);
                        projects.DeleteProject(currentID);
                    }
                }
            }

            for (int rowIndex = 0; rowIndex < dataGridView1.RowCount; rowIndex++)
            {
                DataGridViewRow row = dataGridView1.Rows[rowIndex];
                if (true
                    && row.Cells.Count >= 2
                    && (row.Cells[0].Value == null || row.Cells[0].Value.ToString().Length == 0)
                    && (row.Cells[1].Value == null || row.Cells[1].Value.ToString().Length == 0)
                    )
                {
                    for (int nextRowIndex = rowIndex + 1; nextRowIndex < dataGridView1.RowCount; nextRowIndex++)
                    {
                        DataGridViewRow row2 = dataGridView1.Rows[nextRowIndex];
                        if (true
                            && row2.Cells.Count >= 2
                            && row2.Cells[1].Value != null
                            && row2.Cells[0].Value.ToString().Length > 0
                            )
                        {
                            SwapRows(rowIndex, nextRowIndex);
                        }
                    }
                }
            }

            UpdateDataGridView();

            ignoreCellValueChanged--;

            CheckNoProjects();
        }

        private void DeleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DeleteSelectedProjects();
        }

        private void SwapRows(int row1, int row2)
        {
            if (true
                && row1 != row2
                && row1 < dataGridView1.Rows.Count
                && row1 >= 0
                && row2 < dataGridView1.Rows.Count
                && row2 >= 0
                )
            {
                foreach (DataGridViewCell cell1 in dataGridView1.Rows[row1].Cells)
                {
                    string value1 = "";
                    if (true
                        && cell1 != null
                        && cell1.Value.ToString().Length > 0
                        )
                    {
                        value1 = cell1.Value.ToString();
                    }
                    DataGridViewCell cell2 = dataGridView1.Rows[row2].Cells[cell1.ColumnIndex];
                    string value2 = "";
                    if (true
                        && cell2 != null
                        && cell2.Value.ToString().Length > 0
                        )
                    {
                        value2 = cell2.Value.ToString();
                    }
                    cell1.Value = value2;
                    cell2.Value = value1;
                }
            }
        }

        private void CheckNoProjects()
        {
            bool empty = true;
            for (int rowIndex = 0; rowIndex < dataGridView1.RowCount; rowIndex++)
            {
                DataGridViewRow row = dataGridView1.Rows[rowIndex];
                if (true
                    && row.Cells.Count >= 2
                    && row.Cells[0].Value != null
                    && row.Cells[0].Value.ToString().Length > 0
                    && row.Cells[1].Value != null
                    && row.Cells[1].Value.ToString().Length > 0
                    )
                {
                    empty = false;
                }
            }
            if (empty)
            {
                InitialSetupDataGridView();
            }
        }

        private void NewToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            projects.CreateNewProject();
        }

        private void DataGridView1_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            //if (e.ColumnIndex == 0 && ignoreCellValueChanged == 0)
            if (ignoreCellValueChanged == 0)
            {
                if (projects != null)
                {
                    string projectName = LocRM.GetString("String15");
                    if (dataGridView1.Rows[e.RowIndex].Cells[0].Value != null && dataGridView1.Rows[e.RowIndex].Cells[0].Value.ToString().Length > 0)
                    {
                        projectName = dataGridView1.Rows[e.RowIndex].Cells[0].Value.ToString();
                    }
                    dataGridView1.Rows[e.RowIndex].Cells[0].Value = projectName;
                    if (true
                        && dataGridView1.Rows[e.RowIndex].Cells.Count >= 2
                        && dataGridView1.Rows[e.RowIndex].Cells[1].Value != null
                        && dataGridView1.Rows[e.RowIndex].Cells[1].Value.ToString().Length > 0
                        )
                    {
                        //string id = dataGridView1.Rows[e.RowIndex].Cells[1].Value.ToString();
                        //projects.EditProject(projectName, id, e.RowIndex);
                        projects.EditProject(dataGridView1.Rows[e.RowIndex]);
                    }
                    else
                    {
                        projects.CreateNewProject(projectName, e.RowIndex);
                    }
                }
            }
        }

        private void DeleteToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            DeleteSelectedProjects();
        }

        private void DaysHoursToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ShowEditCreateSubform(SubformType.DAYSHOURS);
        }

        private void DaysHoursToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            ShowEditCreateSubform(SubformType.DAYSHOURS);
        }

        private void SubjectsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ShowEditCreateSubform(SubformType.SUBJECT);
        }

        private void SubjectToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ShowEditCreateSubform(SubformType.SUBJECT);
        }

        private void ClassesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ShowEditCreateSubform(SubformType.CLASSES);
        }

        private void ClassesToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            ShowEditCreateSubform(SubformType.CLASSES);
        }

        private void TeacherToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ShowEditCreateSubform(SubformType.TEACHER);
        }

        private void TeacherToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            ShowEditCreateSubform(SubformType.TEACHER);
        }

        private void TeachertoClassToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ShowEditCreateSubform(SubformType.TEACHERTOCLASS);
        }

        private void TeachertoClassToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            ShowEditCreateSubform(SubformType.TEACHERTOCLASS);
        }

        private void ClasstoSubjectToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ShowEditCreateSubform(SubformType.CLASSTOSUBJECTPRESET);
        }

        private void ClasstoSubjectToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            ShowEditCreateSubform(SubformType.CLASSTOSUBJECTPRESET);
        }

        private void ShowEditCreateSubform(SubformType subformType)
        {
            if (dataGridView1.SelectedRows.Count != 1)
            {
                return;
            }
            DataGridViewRow row = dataGridView1.SelectedRows[0];
            if (false
                || row.Cells.Count < 2
                || row.Cells[0].Value == null
                || row.Cells[0].Value.ToString().Length == 0
                || row.Cells[1].Value == null
                || row.Cells[1].Value.ToString().Length == 0
                )
            {
                return;
            }
            string projectName = row.Cells[0].Value.ToString();
            string id = row.Cells[1].Value.ToString();

            ShowEditCreateSubform(projectName, id, subformType);
        }

        private void ShowEditCreateSubform(string projectName, string id, SubformType subformType)
        {
            if (Form2.myOpenWindows.ContainsKey(subformType + id))
            {
                Form2 form = Form2.myOpenWindows[subformType + id];
                form.WindowState = FormWindowState.Normal;
                form.Focus();
            }
            else
            {
                Form2 form = new Form2(subformType, id, projectName);
                myForm2s.Add(form);
                //form.ShowDialog();
                form.Show();
            }
        }

        public void SetStatus(DataGridViewRow row, SubformType type)
        {
            if (row.Cells.Count >= 6)
            {
                row.Cells[5].Value = type.ToString();
                SetStatusString(row, type);
            }
        }

        public void SetStatusString(DataGridViewRow row, SubformType type)
        {
            if (row.Cells.Count >= 6)
            {
                row.Cells[4].Value = GetStatusString(row, type);
            }
        }

        public void SetStatusString(DataGridViewRow row)
        {
            if (row.Cells.Count >= 6)
            {
                if (row.Cells[5] != null &&
                    row.Cells[5].Value != null &&
                    row.Cells[5].Value.ToString() != "" &&
                    row.Cells[1] != null &&
                    row.Cells[1].Value != null &&
                    row.Cells[1].Value.ToString() != "")
                {
                    string status = row.Cells[5].Value.ToString();
                    Enum.TryParse(status, out Form1.SubformType subform);
                    string id = row.Cells[1].Value.ToString();
                    string statusString = GetStatusString(id, subform);
                    row.Cells[4].Value = statusString;
                }
            }
        }

        public string GetStatusString(string id, SubformType type)
        {
            string r;
            switch (type)
            {
                case SubformType.UNDEF:
                    r = LocRM.GetString("String61");
                    break;
                case SubformType.DAYSHOURS:
                    r = LocRM.GetString("String23");
                    break;
                case SubformType.SUBJECT:
                    r = LocRM.GetString("String47");
                    break;
                case SubformType.CLASSES:
                    r = LocRM.GetString("String87");
                    break;
                case SubformType.TEACHER:
                    r = LocRM.GetString("String130");
                    break;
                case SubformType.CALCULATE:
                    List<FileInfo> rosterFound = projects.CheckForCalculatedRosters(id);
                    if (rosterFound.Count > 0)
                    {
                        r = LocRM.GetString("String144");
                    }
                    else
                    {
                        r = LocRM.GetString("String130");
                    }
                    break;
                default:
                    r = LocRM.GetString("String130");
                    break;
            }
            return r;
        }

        public string GetStatusString(DataGridViewRow row, SubformType type)
        {
            string selectedID = row.Cells[1].Value.ToString();
            return GetStatusString(selectedID, type);
        }

        public void DeactivateActivateMenuItems(SubformType lastChanged = SubformType.UNDEF)
        {
            List<ToolStripMenuItem> menuItems = new List<ToolStripMenuItem>();
            foreach (ToolStripMenuItem item in contextMenuStrip1.Items)
            {
                if (item.GetType() == typeof(ToolStripMenuItem))
                {
                    menuItems.Add(item);
                }
            }
            ToolStripMenuItem[] switchedItems = new ToolStripMenuItem[] {
                daysHoursToolStripMenuItem,
                daysHoursToolStripMenuItem1,
                subjectsToolStripMenuItem,
                subjectToolStripMenuItem,
                classesToolStripMenuItem,
                classesToolStripMenuItem1,
                teacherToolStripMenuItem,
                teacherToolStripMenuItem1,
                runToolStripMenuItem,
                findRosterToolStripMenuItem,
                configurationToolStripMenuItem,
                configurationToolStripMenuItem1
            };
            int[] items2switch = null;

            if (dataGridView1.SelectedRows.Count == 1)
            {
                string selectedID = "";
                DataGridViewRow row = dataGridView1.SelectedRows[0];
                if (true
                    && row.Cells.Count >= 2
                    && row.Cells[1].Value != null
                    && row.Cells[1].Value.ToString().Length > 0
                    )
                {
                    selectedID = row.Cells[1].Value.ToString();
                    if (lastChanged == SubformType.UNDEF)
                    {
                        lastChanged = projects.GetState(selectedID);
                    }
                    SetStatus(row, lastChanged);
                    switch (lastChanged)
                    {
                        case SubformType.UNDEF:
                            items2switch = new int[] { 2, 2, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, };
                            doubleClickSubformType = SubformType.DAYSHOURS;
                            break;
                        case SubformType.DAYSHOURS:
                            items2switch = new int[] { 1, 1, 2, 2, 0, 0, 0, 0, 0, 0, 1, 1, };
                            doubleClickSubformType = SubformType.SUBJECT;
                            break;
                        case SubformType.SUBJECT:
                            doubleClickSubformType = SubformType.CLASSES;
                            items2switch = new int[] { 1, 1, 1, 1, 2, 2, 0, 0, 0, 0, 1, 1, };
                            break;
                        case SubformType.CLASSES:
                            items2switch = new int[] { 1, 1, 1, 1, 1, 1, 2, 2, 0, 0, 1, 1, };
                            doubleClickSubformType = SubformType.TEACHER;
                            break;
                        case SubformType.TEACHER:
                        case SubformType.CALCULATE:
                        case SubformType.VIEW:
                        default:
                            items2switch = new int[] { 1, 1, 1, 1, 1, 1, 1, 1, 2, 2, 1, 1, };
                            doubleClickSubformType = SubformType.UNDEF;
                            break;
                    }
                }
                if (selectedID == "")
                {
                    doubleClickSubformType = SubformType.UNDEF;
                    items2switch = new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, };
                }
                else
                {
                    List<FileInfo> rosterFound = projects.CheckForCalculatedRosters(selectedID);
                    if (rosterFound.Count > 0)
                    {
                        this.viewRosterToolStripMenuItem.Enabled = true;
                        this.viewRosterToolStripMenuItem.Font = new Font(this.viewRosterToolStripMenuItem.Font, FontStyle.Bold);
                        this.viewRosterToolStripMenuItem.DropDownItems.Clear();
                        for (int i = 1; i <= rosterFound.Count; i++)
                        {
                            this.viewRosterToolStripMenuItem.DropDownItems.Add(i.ToString(), null, ViewRosterToolStripMenuItem1_Click);
                        }
                        this.viewRosterToolStripMenuItem1.Enabled = true;
                        this.viewRosterToolStripMenuItem1.Font = new Font(this.viewRosterToolStripMenuItem1.Font, FontStyle.Bold);
                        this.viewRosterToolStripMenuItem1.DropDownItems.Clear();
                        for (int i = 1; i <= rosterFound.Count; i++)
                        {
                            this.viewRosterToolStripMenuItem1.DropDownItems.Add(i.ToString(), null, ViewRosterToolStripMenuItem_Click);
                        }
                    }
                    else
                    {
                        this.viewRosterToolStripMenuItem.Enabled = false;
                        this.viewRosterToolStripMenuItem.Font = new Font(this.viewRosterToolStripMenuItem.Font, FontStyle.Regular);
                        this.viewRosterToolStripMenuItem.DropDownItems.Clear();
                        this.viewRosterToolStripMenuItem1.Enabled = false;
                        this.viewRosterToolStripMenuItem1.Font = new Font(this.viewRosterToolStripMenuItem1.Font, FontStyle.Regular);
                        this.viewRosterToolStripMenuItem1.DropDownItems.Clear();
                    }
                }
            }
            else
            {
                doubleClickSubformType = SubformType.UNDEF;
                items2switch = new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, };
            }

            if (items2switch != null)
            {
                for (int index = 0; index < items2switch.Length; index++)
                {
                    if (items2switch[index] >= 1)
                    {
                        switchedItems[index].Enabled = true;
                        if (items2switch[index] >= 2)
                        {
                            switchedItems[index].Font = new Font(switchedItems[index].Font, FontStyle.Bold);
                        }
                        else
                        {
                            switchedItems[index].Font = new Font(switchedItems[index].Font, FontStyle.Regular);
                        }
                    }
                    else
                    {
                        switchedItems[index].Enabled = false;
                        switchedItems[index].Font = new Font(switchedItems[index].Font, FontStyle.Regular);
                    }
                }
            }

            return;
        }


        private void NewToolStripMenuItem2_Click(object sender, EventArgs e)
        {
            projects.CreateNewProject();
        }


        public delegate void CalcRosterReadyCallbackDelegate();
        private void CalcRosterSelectedProject()
        {
            if (dataGridView1.SelectedRows.Count != 1)
            {
                return;
            }

            string selectedID;
            DataGridViewRow row = dataGridView1.SelectedRows[0];
            if (true
                && row.Cells.Count >= 2
                && row.Cells[1].Value != null
                && row.Cells[1].Value.ToString().Length > 0
                )
            {
                selectedID = row.Cells[1].Value.ToString();

                CalcRosterReadyCallbackDelegate callback = CalcRosterReadyCallback;

                currentCalculatedRow = row.Index;
                projects.CalculateRoster(selectedID, callback);
            }

        }

        public void CalcRosterReadyCallback()
        {
            if (true
                && currentCalculatedRow >= 0
                && dataGridView1.Rows[currentCalculatedRow].Cells.Count >= 2
                && dataGridView1.Rows[currentCalculatedRow].Cells[1].Value != null
                && dataGridView1.Rows[currentCalculatedRow].Cells[1].Value.ToString().Length > 0
                )
            {
                SetStatus(dataGridView1.Rows[currentCalculatedRow], SubformType.CALCULATE);
                DeactivateActivateMenuItems();

                string id = dataGridView1.Rows[currentCalculatedRow].Cells[1].Value.ToString();
                //string projectname = dataGridView1.Rows[currentCalculatedRow].Cells[0].Value.ToString();

                List<string> keys = new List<string>(openViews.Keys);
                foreach (string key in keys)
                {
                    if (key.Contains(id))
                    {
                        Form4 view = openViews[key];
                        view.WindowState = FormWindowState.Normal;
                        if (view.UpdateView())
                        {
                            view.BringToFront();
                        }
                    }
                }
            }
            currentCalculatedRow = -1;
        }


        private void FindRosterToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CalcRosterSelectedProject();
        }

        private void RunToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CalcRosterSelectedProject();

        }

        private void DataGridView1_DoubleClick(object sender, EventArgs e)
        {
            if (doubleClickSubformType != SubformType.UNDEF)
            {
                ShowEditCreateSubform(doubleClickSubformType);
            }
        }

        private void CopyToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            CopyProject();
        }

        private void CopyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CopyProject();
        }
        private void CopyProject()
        {
            if (dataGridView1.SelectedRows.Count != 1)
            {
                return;
            }
            DataGridViewRow row = dataGridView1.SelectedRows[0];
            if (true
                && row.Cells.Count >= 2
                && row.Cells[1].Value != null
                && row.Cells[1].Value.ToString().Length > 0
                )
            {
                projects.Copy(row);
            }
        }

        private void ViewRosterToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            ToolStripItem toolStripItem = (ToolStripItem)sender;
            int item;
            try
            {
                item = Convert.ToInt32(toolStripItem.Text);
            }
            catch (Exception)
            {
                item = -1;
            }
            ViewRosterToolStripMenuItem_Click(item);
        }

        private void ViewRosterToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ToolStripItem toolStripItem = (ToolStripItem)sender;
            int item;
            try
            {
                item = Convert.ToInt32(toolStripItem.Text);
            }
            catch (Exception)
            {
                item = -1;
            }
            ViewRosterToolStripMenuItem_Click(item);
        }

        private void ViewRosterToolStripMenuItem_Click(int item = -1)
        {
            if (dataGridView1.SelectedRows.Count != 1 || item <= 0)
            {
                return;
            }
            DataGridViewRow row = dataGridView1.SelectedRows[0];
            if (true
                && row.Cells.Count >= 2
                && row.Cells[1].Value != null
                && row.Cells[1].Value.ToString().Length > 0
                )
            {
                string id = row.Cells[1].Value.ToString();
                string projectname = row.Cells[0].Value.ToString();
                if (!openViews.ContainsKey(id + "_" + item.ToString()))
                {
                    Form4 view = new Form4(id, projectname, item);
                    openViews.Add(id + "_" + item.ToString(), view);
                    view.Show();
                }
                else
                {
                    Form4 view = openViews[id + "_" + item.ToString()];
                    view.WindowState = FormWindowState.Normal;
                    view.UpdateView();
                    view.BringToFront();
                }
            }
        }

        public void RemoveIDfromOpenView(string id)
        {
            if (openViews.ContainsKey(id))
            {
                openViews.Remove(id);
            }
        }

        private void ConfigurationToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            ShowEditCreateSubform(SubformType.CONFIG);
        }

        private void ConfigurationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ShowEditCreateSubform(SubformType.CONFIG);
        }

        private void ExportToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!Export())
            {
                MessageBox.Show(LocRM.GetString("String162"), "Information", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
            }
        }

        private bool Export(string fileName = null)
        {
            bool r = false;
            string log_output;
            DataGridViewRow row = dataGridView1.SelectedRows[0];
            if (row != null)
            {
                if (fileName == null)
                {
                    if (
                        row.Cells[0] != null
                        && row.Cells[0].Value != null
                        && row.Cells[0].Value.ToString() != ""
                        )
                    {
                        fileName = row.Cells[0].Value.ToString();
                    }
                    SaveFileDialog saveFileDialog1 = new SaveFileDialog
                    {
                        Filter = "zip files (*.zip)|*.zip",
                        FileName = fileName + ".zip",
                        RestoreDirectory = true
                    };
                    if (saveFileDialog1.ShowDialog() == DialogResult.OK)
                    {
                        fileName = saveFileDialog1.FileName;
                        saveFileDialog1.Dispose();
                    }
                    else
                    {
                        saveFileDialog1.Dispose();
                        return true;
                    }
                }
                if (fileName != null && fileName.Length > 0)
                {
                    if (
                        row.Cells[1] != null
                        && row.Cells[1].Value != null
                        && row.Cells[1].Value.ToString() != ""
                        )
                    {
                        string oldId;
                        if (row != null && row.Cells.Count >= 3 && row.Cells[1].Value != null && row.Cells[1].Value.ToString().Length > 0)
                        {
                            oldId = row.Cells[1].Value.ToString();
                        }
                        else
                        {
                            return r;
                        }
                        string date = DateTime.Now.ToString();
                        string description = Form1.LocRM.GetString("String141");

                        if (row.Cells[2].Value != null)
                        {
                            description = row.Cells[2].Value.ToString();
                        }
                        string projectName = Form1.LocRM.GetString("String14");
                        if (row.Cells[0].Value != null)
                        {
                            projectName = row.Cells[0].Value.ToString();
                        }

                        string newGuid;
                        newGuid = projects.CreateNewFromOld(projectName, description, date, oldId, true);
                        if (newGuid != null)
                        {
                            string projectPath = projects.GetBaseFolder() + Path.DirectorySeparatorChar + newGuid;

                            string origLog = Form1.logWindow.GetLogfileWithPath();
                            string copyLog = projectPath + Path.DirectorySeparatorChar + Form1.logWindow.GetLogfileName();
                            try
                            {
                                System.IO.File.Copy(origLog, copyLog, true);
                            }
                            catch (Exception e)
                            {
                                log_output = "warning: can not copy logfile";
                                Form1.logWindow.Write_to_log(ref log_output);
                                log_output = e.ToString();
                                Form1.logWindow.Write_to_log(ref log_output);
                            }

                            FileInfo file = new FileInfo(fileName);
                            if (file.Exists)
                            {
                                try
                                {
                                    file.Delete();
                                }
                                catch (Exception e)
                                {
                                    log_output = "warning: can not overwrite " + fileName;
                                    Form1.logWindow.Write_to_log(ref log_output);
                                    log_output = e.ToString();
                                    Form1.logWindow.Write_to_log(ref log_output);
                                }
                            }
                            try
                            {
                                ZipFile.CreateFromDirectory(projectPath, fileName);
                                r = true;
                            }
                            catch (Exception e)
                            {
                                log_output = "warning: can not create zip file " + fileName;
                                Form1.logWindow.Write_to_log(ref log_output);
                                log_output = e.ToString();
                                Form1.logWindow.Write_to_log(ref log_output);
                            }
                            try
                            {
                                Directory.Delete(projectPath, true);
                            }
                            catch (Exception e)
                            {
                                log_output = "warning: can not delete temporary project directory " + projectPath;
                                Form1.logWindow.Write_to_log(ref log_output);
                                log_output = e.ToString();
                                Form1.logWindow.Write_to_log(ref log_output);
                            }
                        }
                    }
                }
            }
            return r;
        }

        private void ExportAnonToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!ExportAnon())
            {
                MessageBox.Show(LocRM.GetString("String162"), "Information", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
            }
        }

        private bool ExportAnon(string fileName = null)
        {
            bool r = false;
            string log_output;
            DataGridViewRow row = dataGridView1.SelectedRows[0];
            if (row != null)
            {
                if (fileName == null)
                {
                    if (
                        row.Cells[0] != null
                        && row.Cells[0].Value != null
                        && row.Cells[0].Value.ToString() != ""
                        )
                    {
                        fileName = row.Cells[0].Value.ToString();
                    }
                    SaveFileDialog saveFileDialog1 = new SaveFileDialog
                    {
                        Filter = "zip files (*.zip)|*.zip",
                        FileName = fileName + ".zip",
                        RestoreDirectory = true
                    };
                    if (saveFileDialog1.ShowDialog() == DialogResult.OK)
                    {
                        fileName = saveFileDialog1.FileName;
                        saveFileDialog1.Dispose();
                    }
                    else
                    {
                        saveFileDialog1.Dispose();
                        return true;
                    }
                }
                if (fileName != null && fileName.Length > 0)
                {
                    if (
                        row.Cells[1] != null
                        && row.Cells[1].Value != null
                        && row.Cells[1].Value.ToString() != ""
                        )
                    {
                        string oldId;
                        if (row != null && row.Cells.Count >= 3 && row.Cells[1].Value != null && row.Cells[1].Value.ToString().Length > 0)
                        {
                            oldId = row.Cells[1].Value.ToString();
                        }
                        else
                        {
                            return r;
                        }
                        string date = DateTime.Now.ToString();
                        string description = Form1.LocRM.GetString("String141");

                        if (row.Cells[2].Value != null)
                        {
                            description = row.Cells[2].Value.ToString();
                        }
                        string projectName = Form1.LocRM.GetString("String14");
                        if (row.Cells[0].Value != null)
                        {
                            projectName = row.Cells[0].Value.ToString();
                        }

                        string newGuid;
                        newGuid = projects.CreateNewFromOld(projectName, description, date, oldId, true);
                        if (newGuid != null)
                        {
                            string projectPath = projects.GetBaseFolder() + Path.DirectorySeparatorChar + newGuid;

                            string origLog = Form1.logWindow.GetLogfileWithPath();
                            string copyLog = projectPath + Path.DirectorySeparatorChar + Form1.logWindow.GetLogfileName();
                            try
                            {
                                System.IO.File.Copy(origLog, copyLog, true);
                            }
                            catch (Exception e)
                            {
                                log_output = "warning: can not copy logfile";
                                Form1.logWindow.Write_to_log(ref log_output);
                                log_output = e.ToString();
                                Form1.logWindow.Write_to_log(ref log_output);
                            }

                            //anonymize start here
                            List<string[]> teacher = Form1.projects.LoadData(newGuid, Form1.SubformType.TEACHER, false);
                            Dictionary<string, string> teacher2token = new Dictionary<string, string>();
                            if (teacher.Count > 0)
                            {
                                string t;
                                int index = 1;
                                string token;
                                foreach (string[] line in teacher)
                                {
                                    t = line[0];
                                    if (index > 1)
                                    {
                                        token = (index - 1).ToString();
                                        while (token.Length < 3)
                                        {
                                            token = "0" + token;
                                        }
                                        token = "t" + token;
                                        teacher2token[line[0]] = token;
                                        line[0] = token;
                                    }
                                    index++;
                                }
                                Form1.projects.SaveData(newGuid, Form1.SubformType.TEACHER, teacher);
                            }

                            List<string[]> teacher2class = Form1.projects.LoadData(newGuid, Form1.SubformType.TEACHERTOCLASS, false);
                            if (teacher2class.Count > 0)
                            {
                                int skipHeader = 1;
                                foreach (string[] line in teacher2class)
                                {
                                    if (skipHeader > 1)
                                    {
                                        for (int entryIndex = 1; entryIndex < line.Length; entryIndex++)
                                        {
                                            if (teacher2token.ContainsKey(line[entryIndex]))
                                            {
                                                line[entryIndex] = teacher2token[line[entryIndex]];
                                            }
                                        }
                                    }
                                    skipHeader++;
                                }
                                Form1.projects.SaveData(newGuid, Form1.SubformType.TEACHERTOCLASS, teacher2class);
                            }

                            string currentWindowsUserWoDomain = currentWindowsUser;
                            string[] tmp = currentWindowsUser.Split(new char[] { '\\' });
                            if (tmp.Length > 1)
                            {
                                currentWindowsUserWoDomain = tmp[1];
                            }
                            if (System.IO.File.Exists(copyLog))
                            {
                                try
                                {
                                    string log = System.IO.File.ReadAllText(copyLog);
                                    log = log.Replace(currentWindowsUserWoDomain, "###USERNAME###");
                                    foreach (KeyValuePair<string, string> entry in teacher2token)
                                    {
                                        log = log.Replace(entry.Key, "###" + entry.Value + "###");
                                    }
                                    System.IO.File.WriteAllText(copyLog, log);
                                }
                                catch (Exception e)
                                {
                                    log_output = "warning: can write to " + copyLog;
                                    Form1.logWindow.Write_to_log(ref log_output);
                                    log_output = e.ToString();
                                    Form1.logWindow.Write_to_log(ref log_output);
                                }
                            }
                            //anonymize end

                            FileInfo file = new FileInfo(fileName);
                            if (file.Exists)
                            {
                                try
                                {
                                    file.Delete();
                                }
                                catch (Exception e)
                                {
                                    log_output = "warning: can not overwrite " + fileName;
                                    Form1.logWindow.Write_to_log(ref log_output);
                                    log_output = e.ToString();
                                    Form1.logWindow.Write_to_log(ref log_output);
                                }
                            }
                            try
                            {
                                ZipFile.CreateFromDirectory(projectPath, fileName);
                                r = true;
                            }
                            catch (Exception e)
                            {
                                log_output = "warning: can not create zip file " + fileName;
                                Form1.logWindow.Write_to_log(ref log_output);
                                log_output = e.ToString();
                                Form1.logWindow.Write_to_log(ref log_output);
                            }
                            try
                            {
                                Directory.Delete(projectPath, true);
                            }
                            catch (Exception e)
                            {
                                log_output = "warning: can not delete temporary project directory " + projectPath;
                                Form1.logWindow.Write_to_log(ref log_output);
                                log_output = e.ToString();
                                Form1.logWindow.Write_to_log(ref log_output);
                            }
                        }
                    }
                }
            }
            return r;
        }

        private void ImportToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!Import())
            {
                MessageBox.Show(LocRM.GetString("String163"), "Information", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
            }
        }

        private bool Import()
        {
            bool ret = false;
            string log_output;

            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "zip files (*.zip)|*.zip",
                RestoreDirectory = true
            };
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                string filePath = openFileDialog.FileName;
                string newGuid = null;
                string newProjectPath = null;
                projects.CreateNewEmptyProject(ref newProjectPath, ref newGuid);
                if (newProjectPath != null && newGuid != null)
                {
                    try
                    {
                        ZipFile.ExtractToDirectory(filePath, newProjectPath);
                        if (projects.CheckProject(newGuid))
                        {
                            projects.UpdateMetaProjectID(newGuid);
                            string projectname = projects.GetProjectName(newGuid);
                            if (projectname != null)
                            {
                                projectname += " (" + LocRM.GetString("String164") + ")";
                            }
                            else
                            {
                                projectname = "(" + LocRM.GetString("String164") + ")";
                            }
                            projects.UpdateMetaProjectName(newGuid, projectname);
                            projects.FillProjectView();
                            UpdateDataGridView();
                            ret = true;
                        }
                        else
                        {
                            Directory.Delete(newProjectPath, true);
                            log_output = "warning: zip file " + filePath + " not a valid project";
                            Form1.logWindow.Write_to_log(ref log_output);
                        }
                    }
                    catch (Exception e)
                    {
                        log_output = "warning: can not open zip file " + filePath;
                        Form1.logWindow.Write_to_log(ref log_output);
                        log_output = e.ToString();
                        Form1.logWindow.Write_to_log(ref log_output);
                    }
                }
            }
            openFileDialog.Dispose();

            return ret;
        }

        private void DoTimeTableToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ShowSplash();
        }

        static public void ShowSplash()
        {
            if (splash == null)
            {
                splash = new Form5();
            }
            splash.StartPosition = FormStartPosition.CenterScreen;
            splash.ShowDialog();
        }

        [DllImport("Shell32.dll")]
        private static extern int SHGetKnownFolderPath(
        [MarshalAs(UnmanagedType.LPStruct)]Guid rfid, uint dwFlags, IntPtr hToken, out IntPtr ppszPath);

        private void InstallNewVersionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!newVersionDownloaded)
            {
                DownloadNewVersion();
            }
            else
            {
                InstallNewVersion();
            }
        }

        private string GetDownLoadPath()
        {
            string downloadPath = null;
            bool defaultUser = false;
            uint flag = 0x00000400;
            int r = SHGetKnownFolderPath(new Guid("{bfb9d5e0-c6a9-404c-b2b2-ae6db6af4968}"), flag, new IntPtr(defaultUser ? -1 : 0), out IntPtr outPathLink);
            if (r >= 0)
            {
                string linkPath = Marshal.PtrToStringUni(outPathLink);
                Marshal.FreeCoTaskMem(outPathLink);
                downloadPath = linkPath + Path.DirectorySeparatorChar + "Downloads.lnk";

                WshShell shell = new WshShell();
                WshShortcut shortcut = (WshShortcut)shell.CreateShortcut(downloadPath);
                downloadPath = shortcut.TargetPath;
                if (!Directory.Exists(downloadPath))
                {
                    downloadPath = null;
                }
            }
            if (downloadPath == null)
            {
                r = SHGetKnownFolderPath(new Guid("{374DE290-123F-4565-9164-39C4925E467B}"), flag, new IntPtr(defaultUser ? -1 : 0), out IntPtr outPath);
                if (r >= 0)
                {
                    downloadPath = Marshal.PtrToStringUni(outPath);
                    Marshal.FreeCoTaskMem(outPath);
                    if (!Directory.Exists(downloadPath))
                    {
                        downloadPath = null;
                    }
                }
            }
            return downloadPath;
        }

        private bool CheckDownloadedNewVersion()
        {
            string log_output;
            bool r = false;
            string downloadPath = GetDownLoadPath();
            if (downloadPath != null)
            {
                string localNewVersion = "";
#if DEBUG
                Version lv = new Version(version);
                //lv.versionArray[1] += 1;
                localNewVersion = lv.ToString();
#endif
                if (localNewVersion.Length > 0 || crypto.newVersion != null && crypto.newVersion.Length > 0)
                {
                    string newVersion;
                    if (crypto.newVersion == null || crypto.newVersion.Length == 0)
                    {
                        newVersion = localNewVersion;
                    }
                    else
                    {
                        newVersion = crypto.newVersion;
                    }
                    string newInstallerName = "doTimeTableSetup-" + newVersion + ".msi";
#if DEBUG
                    if (
                        System.IO.File.Exists(downloadPath + Path.DirectorySeparatorChar + newInstallerName) &&
                            (
                                !System.IO.File.Exists(downloadPath + Path.DirectorySeparatorChar + newInstallerName + ".sha256") ||
                                !System.IO.File.Exists(downloadPath + Path.DirectorySeparatorChar + newInstallerName + ".sha256_checksum")
                            )
                        )
                    {
                        log_output = "Private key available?";
                        Form1.logWindow.Write_to_log(ref log_output);
                        crypto.SignFile(downloadPath + Path.DirectorySeparatorChar + newInstallerName);
                        crypto.Sha256ChecksumFile(downloadPath + Path.DirectorySeparatorChar + newInstallerName);
                    }
#endif
                    if (
                        System.IO.File.Exists(downloadPath + Path.DirectorySeparatorChar + newInstallerName) &&
                        System.IO.File.Exists(downloadPath + Path.DirectorySeparatorChar + newInstallerName + ".sha256")
                        )
                    {
                        if (!crypto.VerifyFile(downloadPath + Path.DirectorySeparatorChar + newInstallerName))
                        {
                            log_output = "downloaded installer could not be verified";
                            Form1.logWindow.Write_to_log(ref log_output);
                            try
                            {
                                if (System.IO.File.Exists(downloadPath + Path.DirectorySeparatorChar + newInstallerName + ".NOT_VERIFIED"))
                                {
                                    System.IO.File.Delete(downloadPath + Path.DirectorySeparatorChar + newInstallerName + ".NOT_VERIFIED");
                                }
                                System.IO.File.Move(downloadPath + Path.DirectorySeparatorChar + newInstallerName, downloadPath + Path.DirectorySeparatorChar + newInstallerName + ".NOT_VERIFIED");
                            }
                            catch (Exception ex)
                            {
                                log_output = "File " + downloadPath + Path.DirectorySeparatorChar + newInstallerName + " exists and can't be deleted";
                                Form1.logWindow.Write_to_log(ref log_output);
                                log_output = ex.Message;
                                Form1.logWindow.Write_to_log(ref log_output);
                            }
                            try
                            {
                                //File.Delete(downloadPath + Path.DirectorySeparatorChar + newInstallerName + ".sha256");
                            }
                            catch (Exception ex)
                            {
                                log_output = "File " + downloadPath + Path.DirectorySeparatorChar + newInstallerName + ".sha256" + " exists and can't be deleted";
                                Form1.logWindow.Write_to_log(ref log_output);
                                log_output = ex.Message;
                                Form1.logWindow.Write_to_log(ref log_output);
                            }
                        }
                        else
                        {
                            r = true;
                        }
                    }
                }
            }
            return r;
        }

        private void SetDownloadProgress( int value )
        {
            Object[] values = { value };
            Progress.currentProgress.SetProgress2_fromThread(values);
        }

        private void SetDownloadText(string text)
        {
            Object[] values = { text };
            doTimeTable.Progress.currentProgress.ShowText_fromThread(values);
        }

        private async void DownloadNewVersion()
        {
            string log_output;
            string downloadPath = GetDownLoadPath();
            if (downloadPath != null)
            {
                if (Directory.Exists(downloadPath))
                {
                    if (httpClient != null)
                    {
                        Progress.currentProgress.Show();
                        Progress.currentProgress.ActivateMe(Progress.Type.DOWNLOAD);

                        string localNewVersion = "";
#if DEBUG
                        Version lv = new Version(version);
                        //lv.versionArray[1] += 1;
                        localNewVersion = lv.ToString();
#endif
                        if (localNewVersion.Length > 0 || crypto.newVersion != null && crypto.newVersion.Length > 0)
                        {
                            string newVersion;
                            if (crypto.newVersion == null || crypto.newVersion.Length == 0)
                            {
                                newVersion = localNewVersion;
                            }
                            else
                            {
                                newVersion = crypto.newVersion;
                            }

                            //string installerName = "doTimeTableSetup.msi";
                            string newInstallerName = "doTimeTableSetup-" + newVersion + ".msi";
                            string downloadURL = "https://github.com/oheil/doTimeTable/releases/download/v" + newVersion + "/" + newInstallerName;
                            if (System.IO.File.Exists(downloadPath + Path.DirectorySeparatorChar + newInstallerName))
                            {
                                try
                                {
                                    System.IO.File.Delete(downloadPath + Path.DirectorySeparatorChar + newInstallerName);
                                }
                                catch (Exception ex)
                                {
                                    log_output = "File " + downloadPath + Path.DirectorySeparatorChar + newInstallerName + " exists and can't be deleted";
                                    Form1.logWindow.Write_to_log(ref log_output);
                                    log_output = ex.Message;
                                    Form1.logWindow.Write_to_log(ref log_output);
                                }
                            }
                            if (System.IO.File.Exists(downloadPath + Path.DirectorySeparatorChar + newInstallerName + ".sha256"))
                            {
                                try
                                {
                                    System.IO.File.Delete(downloadPath + Path.DirectorySeparatorChar + newInstallerName + ".sha256");
                                }
                                catch (Exception ex)
                                {
                                    log_output = "File " + downloadPath + Path.DirectorySeparatorChar + newInstallerName + ".sha256" + " exists and can't be deleted";
                                    Form1.logWindow.Write_to_log(ref log_output);
                                    log_output = ex.Message;
                                    Form1.logWindow.Write_to_log(ref log_output);
                                }
                            }
#if DEBUG
                            string installerSource = ".." + Path.DirectorySeparatorChar + ".." + Path.DirectorySeparatorChar + ".." + Path.DirectorySeparatorChar +
                                ".." + Path.DirectorySeparatorChar + "doTimeTableSetup" + Path.DirectorySeparatorChar + "Release" + Path.DirectorySeparatorChar +
                                "doTimeTableSetup.msi";
                            if (System.IO.File.Exists(installerSource))
                            {
                                System.IO.File.Copy(installerSource, downloadPath + Path.DirectorySeparatorChar + newInstallerName);
                                CheckDownloadedNewVersion();
                            }
#endif
                            SetDownloadProgress(10);
                            SetDownloadText(LocRM.GetString("String186") + " " + newInstallerName);
                            log_output = "Downloading " + newInstallerName;
                            Form1.logWindow.Write_to_log(ref log_output);
                            if (!System.IO.File.Exists(downloadPath + Path.DirectorySeparatorChar + newInstallerName))
                            {
                                try
                                {
                                    HttpResponseMessage response = await httpClient.GetAsync(new Uri(downloadURL));
                                    if (response.IsSuccessStatusCode)
                                    {
                                        using (var fs = new FileStream(downloadPath + Path.DirectorySeparatorChar + newInstallerName, FileMode.Create))
                                        {
                                            await response.Content.CopyToAsync(fs);
                                        }
                                    }
                                    else
                                    {
                                        log_output = "Download " + newInstallerName + " failed";
                                        Form1.logWindow.Write_to_log(ref log_output);
                                    }
                                }
                                catch (Exception ex)
                                {
                                    log_output = ex.Message;
                                    Form1.logWindow.Write_to_log(ref log_output);
                                }
                            }
                            SetDownloadProgress(70);
                            SetDownloadText(LocRM.GetString("String186") + " " + newInstallerName + ".sha256");
                            log_output = "Downloading " + newInstallerName + ".sha256";
                            Form1.logWindow.Write_to_log(ref log_output);
                            if (!System.IO.File.Exists(downloadPath + Path.DirectorySeparatorChar + newInstallerName + ".sha256"))
                            {
                                try
                                {
                                    HttpResponseMessage response = await httpClient.GetAsync(new Uri(downloadURL + ".sha256"));
                                    if (response.IsSuccessStatusCode)
                                    {
                                        using (var fs = new FileStream(downloadPath + Path.DirectorySeparatorChar + newInstallerName + ".sha256", FileMode.Create))
                                        {
                                            await response.Content.CopyToAsync(fs);
                                        }
                                    }
                                    else
                                    {
                                        log_output = "Download " + newInstallerName + ".sha256" + " failed";
                                        Form1.logWindow.Write_to_log(ref log_output);
                                    }
                                }
                                catch (Exception ex)
                                {
                                    log_output = ex.Message;
                                    Form1.logWindow.Write_to_log(ref log_output);
                                }
                            }
                            SetDownloadProgress(80);
                            SetDownloadText(LocRM.GetString("String189"));
                            log_output = "Verifying " + newInstallerName;
                            Form1.logWindow.Write_to_log(ref log_output);
                            if (CheckDownloadedNewVersion() && System.IO.File.Exists(downloadPath + Path.DirectorySeparatorChar + newInstallerName))
                            {
                                log_output = "downloaded and verified installer: " + downloadPath + Path.DirectorySeparatorChar + newInstallerName;
                                Form1.logWindow.Write_to_log(ref log_output);
                                this.installNewVersionToolStripMenuItem.Text = LocRM.GetString("String180");
                                newVersionDownloaded = true;
                            }
                            else
                            {
                                log_output = "error: installer could not be downloaded or verified";
                                Form1.logWindow.Write_to_log(ref log_output);
                            }
                            SetDownloadProgress(100);
                        }
                        else
                        {
                            log_output = "error: new version unknown";
                            Form1.logWindow.Write_to_log(ref log_output);
                        }

                        Progress.currentProgress.Julia_ready_fromThread();
                    }
                }
                else
                {
                    log_output = "error: default download folder " + downloadPath + " does not exist";
                    Form1.logWindow.Write_to_log(ref log_output);
                }
            }
            else
            {
                log_output = "error: can not determine default download folder";
                Form1.logWindow.Write_to_log(ref log_output);
            }
        }

        private void InstallNewVersion()
        {
            string log_output;
            string downloadPath = GetDownLoadPath();
            if (downloadPath != null)
            {
                string localNewVersion = "";
#if DEBUG
                Version lv = new Version(version);
                lv.versionArray[1] += 1;
                localNewVersion = lv.ToString();
#endif
                string newVersion;
                if (crypto.newVersion == null || crypto.newVersion.Length == 0)
                {
                    newVersion = localNewVersion;
                }
                else
                {
                    newVersion = crypto.newVersion;
                }

                string newInstallerName = "doTimeTableSetup-" + newVersion + ".msi";
                if (CheckDownloadedNewVersion() && System.IO.File.Exists(downloadPath + Path.DirectorySeparatorChar + newInstallerName))
                {
                    System.Diagnostics.Process.Start(downloadPath + Path.DirectorySeparatorChar + newInstallerName);
                }
                else
                {
                    log_output = "error: installer not found";
                    Form1.logWindow.Write_to_log(ref log_output);
                    this.installNewVersionToolStripMenuItem.Text = LocRM.GetString("String179");
                    newVersionDownloaded = false;
                }
            }
        }
    }

    public class NativeMethods
    {
        [DllImportAttribute("User32.dll", CharSet = CharSet.Unicode)]
        private static extern int FindWindow(String ClassName, String WindowName);

        const int SWP_NOMOVE = 0x0002;
        const int SWP_NOSIZE = 0x0001;
        const int SWP_SHOWWINDOW = 0x0040;
        const int SWP_NOACTIVATE = 0x0010;
        [DllImport("user32.dll", EntryPoint = "SetWindowPos")]
        private static extern IntPtr SetWindowPos(IntPtr hWnd, int hWndInsertAfter, int x, int Y, int cx, int cy, int wFlags);
        [DllImport("user32.dll")]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);
        [DllImport("user32.Dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool EnumChildWindows(IntPtr parentHandle, Win32Callback callback, IntPtr lParam);
        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr GetWindow(IntPtr hWnd, uint uCmd);
        [DllImport("user32.dll")]
        static extern IntPtr GetTopWindow(IntPtr hWnd);
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool IsWindowVisible(IntPtr hWnd);
        [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);
        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        static extern int GetWindowTextLength(IntPtr hWnd);

        public delegate bool Win32Callback(IntPtr hwnd, IntPtr lParam);

        static public void DoOnProcess(string windowTitle)
        {
            System.Diagnostics.Process myProcess = System.Diagnostics.Process.GetCurrentProcess();
            int pid = myProcess.Id;

            List<IntPtr> all = GetRootWindowsOfProcess(pid);
            int length;
            StringBuilder sb;
            IntPtr root;
            IntPtr myHandle = IntPtr.Zero;
            foreach (IntPtr hWnd in all)
            {
                root = GetTopWindow(hWnd);
                if (root != IntPtr.Zero)
                {
                    if (IsWindowVisible(hWnd))
                    {
                        length = GetWindowTextLength(hWnd);
                        sb = new StringBuilder(length + 1);
                        GetWindowText(hWnd, sb, sb.Capacity);
                        if (sb.ToString().Equals(windowTitle))
                        {
                            myHandle = hWnd;
                        }
                        else if (!Form1.openWindows.Contains(hWnd))
                        {
                            Form1.openWindows.Add(hWnd);
                        }
                    }
                    else
                    {
                        while (Form1.openWindows.Contains(hWnd))
                        {
                            Form1.openWindows.Remove(hWnd);
                        }
                    }
                }
            }
            if (myHandle != IntPtr.Zero)
            {
                while (Form1.openWindows.Contains(myHandle))
                {
                    Form1.openWindows.Remove(myHandle);
                }
                Form1.openWindows.Add(myHandle);
            }
            int index = 0;
            IntPtr hWnd2;
            while ( index < Form1.openWindows.Count )
            {
                hWnd2 = Form1.openWindows[index];
                // Change behavior by settings the wFlags params. See http://msdn.microsoft.com/en-us/library/ms633545(VS.85).aspx
                SetWindowPos(hWnd2, 0, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE | SWP_SHOWWINDOW | SWP_NOACTIVATE);
                index++;
            }

            if (Progress.currentProgress.WindowOpen())
            {
                Progress.currentProgress.BringToFront();
            }

            //foreach (IntPtr hWnd in Form1.openWindows)
            //{
                // Change behavior by settings the wFlags params. See http://msdn.microsoft.com/en-us/library/ms633545(VS.85).aspx
                //SetWindowPos(hWnd, 0, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE | SWP_SHOWWINDOW | SWP_NOACTIVATE);
            //}
        }
        static List<IntPtr> GetRootWindowsOfProcess(int pid)
        {
            List<IntPtr> rootWindows = GetChildWindows(IntPtr.Zero);
            List<IntPtr> dsProcRootWindows = new List<IntPtr>();
            foreach (IntPtr hWnd in rootWindows)
            {
                GetWindowThreadProcessId(hWnd, out uint lpdwProcessId);
                if (lpdwProcessId == pid)
                    dsProcRootWindows.Add(hWnd);
            }
            return dsProcRootWindows;
        }
        static public List<IntPtr> GetChildWindows(IntPtr parent)
        {
            List<IntPtr> result = new List<IntPtr>();
            GCHandle listHandle = GCHandle.Alloc(result);
            try
            {
                Win32Callback childProc = new Win32Callback(EnumWindowCallback);
                EnumChildWindows(parent, childProc, GCHandle.ToIntPtr(listHandle));
            }
            finally
            {
                if (listHandle.IsAllocated)
                    listHandle.Free();
            }
            return result;
        }
        private static bool EnumWindowCallback(IntPtr handle, IntPtr pointer)
        {
            GCHandle gch = GCHandle.FromIntPtr(pointer);
            //List<IntPtr> list = gch.Target as List<IntPtr>;
            //if (list == null)
            if (!(gch.Target is List<IntPtr> list))
                {
                    throw new InvalidCastException("GCHandle Target could not be cast as List<IntPtr>");
            }
            list.Add(handle);
            //  You can modify this to check to see if you want to cancel the operation, then return a null here
            return true;
        }
    }

}

