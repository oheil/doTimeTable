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
using System.Windows.Forms;
using System.IO;

using System.Threading;
using System.Runtime.InteropServices;


namespace doTimeTable
{
    static class Program
    {
        [DllImport("kernel32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool AllocConsole();

        [DllImport("kernel32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool FreeConsole();

        [DllImport("kernel32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool SetConsoleTitle(string lpClassName);

        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("kernel32.dll")]
        static extern IntPtr GetConsoleWindow();

        static public string appLogFile = "";

        static void ShowSplashThread()
        {
            Form5 splash = new Form5(true)
            {
                StartPosition = FormStartPosition.CenterScreen
            };
            splash.ShowDialog();

            splash.Dispose();
        }

        /// <summary>
        /// Der Haupteinstiegspunkt für die Anwendung.
        /// </summary>
        [STAThread]
        static void Main()
        {
            FreeConsole();
            AllocConsole();
            string consoleTitle = "doTimeTable console window";
            SetConsoleTitle(consoleTitle);
            IntPtr hWnd = GetConsoleWindow();
            if (hWnd != IntPtr.Zero)
            {
                //Hide the window
                ShowWindow(hWnd, 0); // 0 = SW_HIDE
            }

            Form1 form1 = null;
            Form3 stackForm = null;

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            try
            {
                form1 = new Form1();
                appLogFile = Form1.app_logFile;

//#if DEBUG
//#else
                if (!Form1.app_stop && !Form1.app_restart && !Form1.crypto.registered)
                {
                    Thread splashThread = new Thread(new ThreadStart(ShowSplashThread));
                    splashThread.SetApartmentState(System.Threading.ApartmentState.STA);
                    splashThread.Start();
                }
//#endif

                if (Form1.app_restart == true)
                {
                    Application.Exit();
                    Application.Restart();
                }
                else if (Form1.app_stop == true)
                {
                    Application.Exit();
                }
                else
                {
                    Form1.app_ready = true;
                    Application.Run(form1);
                }
                
            }
            catch (Exception e)
            {
                julia.NativeMethods.StopThreads();

                e.ToString();
                if (appLogFile.Length > 0)
                {
                    StreamWriter log_out_file;
                    log_out_file = File.AppendText(appLogFile);
                    log_out_file.WriteLine(e.ToString());
                    log_out_file.WriteLine(e.StackTrace.ToString());
                    log_out_file.Flush();
                    log_out_file.Close();
                }
                string stack = e.ToString();
                stack += e.StackTrace.ToString();
                stackForm = new Form3(stack);
                stackForm.ShowDialog();
            }
            finally
            {
                if (form1 != null)
                {
                    form1.Dispose();
                }
                if(stackForm!= null)
                {
                    stackForm.Dispose();
                }
            }

            return;
        }
    }
}
