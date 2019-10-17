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
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using System.Threading;
using System.Resources;
using System.Windows.Automation;
using System.IO;
using System.Text.RegularExpressions;

namespace doTimeTable
{
    public partial class Progress : Form
    {
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        static extern bool HideCaret(IntPtr hWnd);

        public static ResourceManager LocRM = new ResourceManager("doTimeTable.WinFormStrings", typeof(Form2).Assembly);

        public static Progress currentProgress = new Progress();
        public bool skipActivateEvent = false;

        private bool windowOpen = false;

        private bool dialogOpen = false;
        private int width = -1;
        private int height = -1;
        private Point pos = new Point(-1,-1);

        private readonly Color textBox3BgColor;
        private bool warningTextOut = false;
        private bool calcFinished = false;

        Form1.CalcRosterReadyCallbackDelegate currentCallback = null;

        public bool WindowOpen()
        {
            return windowOpen;
        }

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
                        width = this.Width;
                        height = this.Height;
                        pos = this.Location;

                        Form1.myself.WindowState = FormWindowState.Minimized;
                        Form1.logWindow.WindowState = FormWindowState.Minimized;
                        foreach (Form2 form2 in Form2.myInstantiations)
                        {
                            form2.WindowState = FormWindowState.Minimized;
                        }
                    }
                    break;
            }
            base.WndProc(ref m);
        }

        public Progress()
        {
            InitializeComponent();

            progressBar1.Style = ProgressBarStyle.Marquee;
            progressBar1.UseWaitCursor = true;
            progressBar1.MarqueeAnimationSpeed = 10;

            progressBar2.Style = ProgressBarStyle.Continuous;
            progressBar2.UseWaitCursor = true;
            progressBar2.Minimum = 0;
            progressBar2.Maximum = 100;
            progressBar2.Value = 0;
            progressBar2.Step = 1;

            textBox3BgColor = textBox3.BackColor;
            HideCaret(textBox3.Handle);
        }

        private void Progress_Activated(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Minimized)
            {
                ActivateMe();
            }
        }

        public delegate void InvokeDelegate_setProgress2(int value);
        public void SetProgress2_fromThread(Object[] values)
        {
            if (this.InvokeRequired)
            {
                this.BeginInvoke(new InvokeDelegate_setProgress2(SetProgress2), values);
            }
            else
            {
                int value = (int)values[0];
                SetProgress2(value);
            }
        }

        public void SetProgress2(int value)
        {
            textBox2.Text = LocRM.GetString("String135") + " " + value.ToString();
            if( value > 100 )
            {
                value = 100;
            }
            progressBar2.Value = value;
        }

        public delegate void InvokeDelegate_showText(string line);
        public void ShowText_fromThread(Object[] values)
        {
            if (this.InvokeRequired)
            {
                this.BeginInvoke(new InvokeDelegate_showText(ShowText), values);
            }
            else
            {
                string line = (string)values[0];
                ShowText(line);
            }
        }
        public void ShowText(string line)
        {
            string newLine;
            int[] localTextIDs = new int[] { 137, 138, 151, 166 };
            List<int> idsFound = new List<int>();
            foreach( int id in localTextIDs)
            {
                string idString = "String" + id.ToString();
                string localText = LocRM.GetString(idString);
                newLine = line.Replace(idString, localText);
                if( newLine != line)
                {
                    idsFound.Add(id);
                }
                line = newLine;
            }
            textBox3.ForeColor = Color.Red;
            textBox3.BackColor = textBox3BgColor;
            if (!warningTextOut)
            {
                textBox3.Clear();
                if (!idsFound.Contains(151))
                {
                    textBox3.AppendText(LocRM.GetString("String139") + Environment.NewLine);
                }
            }
            if (!idsFound.Contains(151))
            {
                line = "\t" + line;
            }
            textBox3.AppendText(line + Environment.NewLine);
            warningTextOut = true;
        }

        public void ActivateMe(Form1.CalcRosterReadyCallbackDelegate callback = null)
        {
            calcFinished = false;
            warningTextOut = false;

            if (callback!= null)
            {
                currentCallback = callback;
            }

            this.Text = LocRM.GetString("String136");

            splitContainer1.Enabled = false;
            splitContainer2.Enabled = false;
            splitContainer3.Enabled = false;

            textBox1.Text = LocRM.GetString("String134");
            textBox1.DeselectAll();
            textBox1.HideSelection = false;
            textBox1.Enabled = false;

            textBox2.Text = LocRM.GetString("String135") + " " + "0";
            textBox2.DeselectAll();
            textBox2.HideSelection = false;
            textBox2.Enabled = false;

            textBox3.Clear();
            textBox3.DeselectAll();
            textBox3.HideSelection = false;
            textBox3.ReadOnly = true;
            //textBox3.Enabled = false;
            textBox3.ForeColor = Color.Red;
            textBox3.BackColor = textBox3BgColor;
            textBox3.ScrollBars = ScrollBars.Vertical;

            textBox3.AppendText(LocRM.GetString("String159") + Environment.NewLine);

            HideCaret(textBox3.Handle);

            progressBar1.Style = ProgressBarStyle.Marquee;
            progressBar2.Value = 0;

            if (!skipActivateEvent)
            {
                if (width > 0 && height > 0 && pos.X > 0 && pos.Y > 0)
                {
                    this.Width = width;
                    this.Height = height;
                    this.Location = pos;
                    windowOpen = true;
                }

                if (
                    this != null &&
                    // this.Visible &&
                    this.WindowState == FormWindowState.Minimized &&
                    true
                    )
                {
                    this.WindowState = FormWindowState.Normal;
                    Form1.logWindow.WindowState = FormWindowState.Normal;
                    Form1.myself.WindowState = FormWindowState.Normal;
                    //foreach (Form2 form2 in Form2.myInstantiations)
                    //{
                    //    form2.WindowState = FormWindowState.Normal;
                    //}
                }
                NativeMethods.DoOnProcess(this.Text);

            }
            skipActivateEvent = false;

            int x = Form1.myself.Location.X + Form1.myself.Width / 2 - this.Width / 2;
            int y = Form1.myself.Location.Y + Form1.myself.Height / 2 - this.Height / 2;
            this.Location = new Point(x, y);
        }


        public void Julia_ready_fromThread()
        {
            if (this.InvokeRequired)
            {
                this.BeginInvoke((Action)Julia_ready_fromThread);
            }
            else
            {
                MyClose();
            }
        }

        private void Progress_FormClosing(object sender, FormClosingEventArgs e)
        {
            DialogResult dialogResult = System.Windows.Forms.DialogResult.Yes;
            if (!calcFinished)
            {
                dialogOpen = true;
                dialogResult = MessageBox.Show(LocRM.GetString("String131"), LocRM.GetString("String132"), MessageBoxButtons.YesNoCancel, MessageBoxIcon.Exclamation);
                dialogOpen = false;
            }
            if (dialogResult == System.Windows.Forms.DialogResult.Yes)
            {
                //dialogResult = DialogResult.Ignore;
                string log_output = "Waiting for julia to stop";
                doTimeTable.Form1.logWindow.Write_to_log(ref log_output);
                julia.NativeMethods.TouchJuliaAbortFile();
                while (julia.NativeMethods.juliaRunning)
                {
                    Thread.Sleep(500);
                }
                e.Cancel = true;
                MyClose(true);
            }
            //dialogResult = DialogResult.Ignore;
            e.Cancel = true;
        }

        private void MyClose(bool force = false)
        {
            progressBar1.Style = ProgressBarStyle.Blocks;
            if (!warningTextOut || force)
            {
                if (dialogOpen)
                {
                    CloseModalWindows();
                }
                this.Hide();
                windowOpen = false;

                Form1.myself.Enabled = true;
                foreach (Form2 form2 in Form2.myInstantiations)
                {
                    form2.Enabled = true;
                }

                currentCallback?.Invoke();
                currentCallback = null;

                textBox3.Clear();
                warningTextOut = false;
            }
            calcFinished = true;
        }

        public static void CloseModalWindows()
        {
            AutomationElement root = AutomationElement.FromHandle(System.Diagnostics.Process.GetCurrentProcess().MainWindowHandle);
            if (root == null)
                return;
            if (!root.TryGetCurrentPattern(WindowPattern.Pattern, out object pattern))
                return;

            WindowPattern window = (WindowPattern)pattern;
            if (window.Current.WindowInteractionState != WindowInteractionState.ReadyForUserInteraction)
            {
                foreach (AutomationElement element in root.FindAll(TreeScope.Children, new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.Window)))
                {
                    if (element.TryGetCurrentPattern(WindowPattern.Pattern, out pattern))
                    {
                        WindowPattern childWindow = (WindowPattern)pattern;
                        if (childWindow.Current.WindowInteractionState == WindowInteractionState.ReadyForUserInteraction)
                        {
                            childWindow.Close();
                        }
                    }
                }
            }
        }

        private void Progress_Shown(object sender, EventArgs e)
        {
            windowOpen = true;
        }

        private void TextBox3_TextChanged(object sender, EventArgs e)
        {
            HideCaret(textBox3.Handle);
        }
    }
}
