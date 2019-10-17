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
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.IO;
using System.Runtime.InteropServices;

using System.Resources;

namespace doTimeTable
{

	/// <summary>
	/// Summary description for log.
	/// </summary>
	public class Log : System.Windows.Forms.Form
	{
        private readonly ResourceManager LocRM = new ResourceManager("doTimeTable.WinFormStrings", typeof(Form1).Assembly);

        private bool msgbox = false;
        private string config;
        private bool geometry_changed = false;

        private readonly string logfileName = "doTimeTable.log";
        private string outfile = "";
        private string config_outfile = "";
        private int maxlines = 1000;
        private readonly int maxlinesInLogFile = 5000;
        private bool overwriteOldLog = false;
        private readonly ArrayList cache = new ArrayList();
        private System.Windows.Forms.ListBox log_output;
        private IContainer components;

        // firing event: Window was closed
        public delegate void log_window_changed_delegate(object sender, EventArgs e);
        public static event log_window_changed_delegate Log_window_changed;

        public bool skipActivateEvent = false;
        private ContextMenuStrip contextMenuStrip1;
        private ToolStripMenuItem selectAllToolStripMenuItem;
        private ToolStripMenuItem copyToolStripMenuItem;
        private bool ctrl_c_handled;

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
                        //
                    }
                    break;
            }
            base.WndProc(ref m);
        }

        public Log(string path = "")
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();
            //DisableCloseBtn.CloseButton.Disable(this);

            string outstring;

            if (path.Length > 0)
            {
                //trying logfile writing possible
                StreamWriter log_out_file;
                try
                {
                    if (overwriteOldLog)
                    {
                        File.WriteAllText(config_outfile, "");
                    }
                    log_out_file = File.AppendText(path + Path.DirectorySeparatorChar + outfile);
                    log_out_file.Close();
                    outfile = path + Path.DirectorySeparatorChar + outfile;
                    Form1.app_logFile = outfile;
                }
                catch (Exception)
                {
                    outstring = "can't write to logfile: " + path + Path.DirectorySeparatorChar + outfile;
                    this.Write_to_log(ref outstring);
                    //not possible, getting new path
                    outfile = "." + Path.DirectorySeparatorChar + outfile;
                    Form1.app_logFile = outfile;
                }
                try
                {
                    log_out_file = File.AppendText(outfile);
                    log_out_file.Close();
                }
                catch (Exception)
                {
                    outstring = "can't write to logfile: " + outfile;
                    this.Write_to_log(ref outstring);
                }
                LimitLogFileSize();
            }

            ctrl_c_handled = false;
            config = "";
            //
            // TODO: Add any constructor code after InitializeComponent call
            //
            if (outfile.Length > 0)
            {
                outstring = "current log output:";
                this.Write_to_log(ref outstring);
                outstring = "log file is: " + outfile;
                this.Write_to_log(ref outstring);
            }
            if (Form1.app_restart)
            {
                outstring = "new version was found, application will be restarted";
                this.Write_to_log(ref outstring);
            }            
        }

        public void LimitLogFileSize()
        {
            string outstring;
            try
            {
                string[] oldLogLines = File.ReadAllLines(outfile);
                int lines = oldLogLines.Length;
                if (lines > maxlinesInLogFile)
                {
                    File.WriteAllText(config_outfile, "");
                    StreamWriter log_out_file;
                    log_out_file = File.AppendText(outfile);
                    for ( int index = lines - (maxlinesInLogFile/5); index < lines; index++)
                    {
                        log_out_file.WriteLine(oldLogLines[index]);
                    }
                    log_out_file.Close();
                }
            }
            catch (Exception e)
            {
                outstring = "can't open to logfile: " + outfile;
                this.Write_to_log(ref outstring);
                outstring = e.ToString();
                this.Write_to_log(ref outstring);
            }
        }

        public string GetLogfileWithPath()
        {
            return outfile;
        }
        public string GetLogfileName()
        {
            return logfileName;
        }
        
        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        protected override void Dispose( bool disposing )
		{
            Save_geometry();

            if( disposing )
			{
				if(components != null)
				{
					components.Dispose();
				}
                if (contextMenuStrip1 != null)
                {
                    contextMenuStrip1.Dispose();
                }
                if (copyToolStripMenuItem != null)
                {
                    copyToolStripMenuItem.Dispose();
                }
                if (log_output != null)
                {
                    log_output.Dispose();
                }
                if (selectAllToolStripMenuItem != null)
                {
                    selectAllToolStripMenuItem.Dispose();
                }

            }

            base.Dispose( disposing );
		}

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Log));
            this.log_output = new System.Windows.Forms.ListBox();
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.selectAllToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.copyToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.contextMenuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // log_output
            // 
            this.log_output.CausesValidation = false;
            this.log_output.ContextMenuStrip = this.contextMenuStrip1;
            this.log_output.Dock = System.Windows.Forms.DockStyle.Fill;
            this.log_output.HorizontalScrollbar = true;
            this.log_output.Location = new System.Drawing.Point(0, 0);
            this.log_output.Name = "log_output";
            this.log_output.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
            this.log_output.Size = new System.Drawing.Size(512, 238);
            this.log_output.TabIndex = 0;
            this.log_output.KeyDown += new System.Windows.Forms.KeyEventHandler(this.Log_output_KeyDown);
            this.log_output.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.Log_output_KeyPress);
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.selectAllToolStripMenuItem,
            this.copyToolStripMenuItem});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(123, 48);
            // 
            // selectAllToolStripMenuItem
            // 
            this.selectAllToolStripMenuItem.Name = "selectAllToolStripMenuItem";
            this.selectAllToolStripMenuItem.Size = new System.Drawing.Size(122, 22);
            this.selectAllToolStripMenuItem.Text = "Select All";
            this.selectAllToolStripMenuItem.Click += new System.EventHandler(this.SelectAllToolStripMenuItem_Click);
            // 
            // copyToolStripMenuItem
            // 
            this.copyToolStripMenuItem.Name = "copyToolStripMenuItem";
            this.copyToolStripMenuItem.Size = new System.Drawing.Size(122, 22);
            this.copyToolStripMenuItem.Text = "Copy";
            this.copyToolStripMenuItem.Click += new System.EventHandler(this.CopyToolStripMenuItem_Click);
            // 
            // Log
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
            this.AutoScroll = true;
            this.CausesValidation = false;
            this.ClientSize = new System.Drawing.Size(512, 238);
            this.Controls.Add(this.log_output);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "Log";
            this.Text = "log";
            this.Activated += new System.EventHandler(this.Log_Activated);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Log_FormClosing);
            this.LocationChanged += new System.EventHandler(this.Log_LocationChanged);
            this.SizeChanged += new System.EventHandler(this.Log_SizeChanged);
            this.VisibleChanged += new System.EventHandler(this.Log_VisibleChanged);
            this.Paint += new System.Windows.Forms.PaintEventHandler(this.Log_Paint);
            this.Resize += new System.EventHandler(this.Log_Resize);
            this.contextMenuStrip1.ResumeLayout(false);
            this.ResumeLayout(false);

		}
		#endregion

		private void Log_output_SelectedIndexChanged(object sender, System.EventArgs e)
		{
		
		}
        public void Write_to_log_fromThread(string outstring)
        {
            if (this.InvokeRequired)
            {
                this.BeginInvoke((Action<string>)Write_to_log_fromThread, outstring);
            }
            else
            {
                Write_to_log(ref outstring);
            }
            return;
        }

        public void Write_to_log(ref string outstring)
        {
            //write log to log file
            if (outfile.Length > 0)
            {
                StreamWriter log_out_file;
                try
                {
                    log_out_file = File.AppendText(outfile);
                    if (cache.Count > 0)
                    {
                        foreach (string line in cache)
                        {
                            log_out_file.WriteLine(line);
                        }
                        cache.Clear();
                    }
                    log_out_file.WriteLine(outstring);
                    log_out_file.Flush();
                    log_out_file.Close();
                }
                catch (Exception)
                {
                    if (!msgbox)
                    {
                        string msg = string.Format(LocRM.GetString("String9"), outfile);
                        //string msg = "Datei  " + outfile + " kann nicht geöffnet werden";
                        MessageBox.Show(msg, "Information", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                        msgbox = true;
                    }
                }
            }
            else
            {
                cache.Add(outstring);
            }

            //write log to log window
            log_output.BeginUpdate();
            if (log_output.Items.Count > maxlines)
            {
                log_output.Items.RemoveAt(0);
            }
            log_output.Items.Add(outstring);
            log_output.TopIndex = log_output.Items.Count - 1;
            log_output.EndUpdate();
            log_output.Refresh();
        }
		
        private void Log_Paint(object sender, PaintEventArgs e)
        {
            //DisableCloseBtn.CloseButton.Disable(this);
        }

        private void Log_Resize(object sender, EventArgs e)
        {
            //DisableCloseBtn.CloseButton.Disable(this);
        }

        public void Update_config()
        {
            if( Form1.config.config_xml.DocumentElement.GetElementsByTagName("max_log_lines").Count == 0)
            {
                System.Xml.XmlElement max_log_lines = Form1.config.config_xml.CreateElement("max_log_lines");
                max_log_lines.InnerXml = "1000";
                System.Xml.XmlElement root = Form1.config.config_xml.DocumentElement;
                root.AppendChild(max_log_lines);
                Form1.config.Save_config_file();
            }
            maxlines = Convert.ToInt32(Form1.config.config_xml.DocumentElement["max_log_lines"].InnerXml);

            if (Form1.config.config_xml.DocumentElement.GetElementsByTagName("log_file_name").Count == 0)
            {
                System.Xml.XmlElement log_file_name = Form1.config.config_xml.CreateElement("log_file_name");
                log_file_name.InnerXml = Form1.applicationDir + Path.DirectorySeparatorChar + logfileName;
                System.Xml.XmlElement root = Form1.config.config_xml.DocumentElement;
                root.AppendChild(log_file_name);
                Form1.config.Save_config_file();
            }
            config_outfile = Form1.config.config_xml.DocumentElement["log_file_name"].InnerXml;

            if (Form1.config.config_xml.DocumentElement.GetElementsByTagName("overwrite_old_log_file").Count == 0)
            {
                System.Xml.XmlElement overwrite_old_log_file = Form1.config.config_xml.CreateElement("overwrite_old_log_file");
                overwrite_old_log_file.InnerXml = "false";
                System.Xml.XmlElement root = Form1.config.config_xml.DocumentElement;
                root.AppendChild(overwrite_old_log_file);
                Form1.config.Save_config_file();
            }
            overwriteOldLog = Convert.ToBoolean(Form1.config.config_xml.DocumentElement["overwrite_old_log_file"].InnerXml);

            //trying logfile writing possible
            bool retry = false;
            StreamWriter log_out_file;
            try
            {
                if (overwriteOldLog)
                {
                    File.WriteAllText(config_outfile, "");
                }
                log_out_file = File.AppendText(config_outfile);
                log_out_file.Close();
                outfile = config_outfile;
                Form1.app_logFile = outfile;

                LimitLogFileSize();
                //string outstring = "current log output:";
                //this.write_to_log(ref outstring);

                string outstring = "log file is: " + config_outfile;
                this.Write_to_log(ref outstring);
            }
            catch (Exception )
            {
                retry = true;
                
            }
            if (retry && config_outfile != Form1.applicationDir + Path.DirectorySeparatorChar + logfileName)
            {
                string outstring = "log file " + config_outfile + " can't be created.";
                this.Write_to_log(ref outstring);
                config_outfile = Form1.applicationDir + Path.DirectorySeparatorChar + logfileName;
                try
                {
                    if (overwriteOldLog)
                    {
                        File.WriteAllText(config_outfile, "");
                    }
                    log_out_file = File.AppendText(config_outfile);
                    log_out_file.Close();
                    outfile = config_outfile;
                    Form1.app_logFile = outfile;
                    //outstring = "Current log output:";
                    //this.write_to_log(ref outstring);
                    outstring = "log file is: " + config_outfile;
                    this.Write_to_log(ref outstring);
                }
                catch (Exception e)
                {
                    outstring = e.ToString();
                    this.Write_to_log(ref outstring);
                    if (!msgbox)
                    {
                        string msg = string.Format(LocRM.GetString("String9"), config_outfile);
                        //string msg = "Datei  " + config_outfile + " kann nicht geöffnet werden";
                        MessageBox.Show(msg, "Information", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                        msgbox = true;
                    }
                }
            }





        }

        public void Update_geometry()
        {
            string komponente = "log_view";
            config = Form1.config.Get_config(ref komponente);
        }

        public void Save_geometry()
        {
            if (geometry_changed)
            {
                string komponente = "log_view";

                config = this.Width.ToString() + ";";
                config += this.Height.ToString() + ";";
                config += this.Location.X.ToString() + ";";
                config += this.Location.Y.ToString() + ";";

                if (Form1.app_ready)
                {
                    Form1.config.Save_config(ref komponente, ref config);
                }

                geometry_changed = false;
            }
        }

        public void Log_setVisibleChanged()
        {
            if (config.Length > 0)
            {
                this.Width = Convert.ToInt32(config.Split(';')[0]);
            }
            if (config.Length > 0)
            {
                this.Height = Convert.ToInt32(config.Split(';')[1]);
            }
            if (config.Length > 0 )
            {
                this.DesktopLocation = new Point(Convert.ToInt32(config.Split(';')[2]), Convert.ToInt32(config.Split(';')[3]));
                if (!Form1.myself.IsOnScreen(this.DesktopLocation))
                {
                    this.DesktopLocation = new Point(0, 0);
                }
            }
        }

        private void Log_VisibleChanged(object sender, EventArgs e)
        {
            Log_setVisibleChanged();
        }

        private void Log_FormClosing(object sender, FormClosingEventArgs e)
        {
            Save_geometry();
            this.Hide();
            Log_window_changed(this, e);
            
            e.Cancel = true;
        }

        public bool Is_visible()
        {
            return this.Visible;
        }

        private void Log_output_KeyDown(object sender, KeyEventArgs e)
        {
            ctrl_c_handled = false;
            if (e.KeyData == (Keys.C | Keys.Control))
            {
                CopySelectedToClipboard();
            }
        }

        private void Log_output_KeyPress(object sender, KeyPressEventArgs e)
        {
            e.Handled = ctrl_c_handled;
        }

        private void Log_LocationChanged(object sender, EventArgs e)
        {
            if (this.Visible)
            {
                geometry_changed = true;
            }
        }

        private void Log_SizeChanged(object sender, EventArgs e)
        {
            geometry_changed = true;
        }

        private void Log_Activated(object sender, EventArgs e)
        {
            if (!skipActivateEvent)
            {
                if (true
                    && this != null
                    && this.Visible
                    && this.WindowState == FormWindowState.Minimized)
                {
                    //Form1.skipActivateEvent = true;
                    Form1.myself.WindowState = FormWindowState.Normal;
                    //foreach (Form2 form2 in Form2.myInstantiations)
                    //{
                    //    form2.WindowState = FormWindowState.Normal;
                    //}
                    if (
                        Progress.currentProgress != null &&
                        //Progress.currentProgress.Visible &&
                        //Progress.currentProgress.WindowState == FormWindowState.Minimized &&
                        true
                        )
                    {
                        Progress.currentProgress.WindowState = FormWindowState.Normal;
                    }
                }
                NativeMethods.DoOnProcess(this.Text);
            }
            skipActivateEvent = false;
        }

        private void SelectAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SelectAll();
        }

        private void CopyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CopySelectedToClipboard();
        }

        public void CopySelectedToClipboard()
        {
            string selected = "";
            foreach (int index in log_output.SelectedIndices)
            {
                selected += log_output.Items[index].ToString() + "\n";
            }
            Clipboard.SetDataObject(selected, true);
            ctrl_c_handled = true;
        }

        public void SelectAll()
        {
            for (int i = 0; i < log_output.Items.Count; i++)
            {
                log_output.SetSelected(i, true);
            }
        }
    }

}
