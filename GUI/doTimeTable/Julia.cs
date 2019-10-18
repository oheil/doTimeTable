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

using System.Runtime.InteropServices;
using System.IO;
using System.IO.Pipes;
using System.Diagnostics;
using System.Threading;

namespace julia
{
    //class Julia : NativeMethods
    class NativeMethods
    {
        [DllImport("Kernel32.dll", SetLastError = true)]
        public static extern int SetStdHandle(int device, IntPtr handle);

        //[DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
        [DllImport("kernel32.dll")]
        public static extern bool SetDllDirectory(string pathName);

        //[DllImport("libjulia.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        //[DllImport("libjulia.dll", CallingConvention = CallingConvention.Cdecl)]
        //private static extern void jl_init(string julia_home_dir);

        //[DllImport("libjulia.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        [DllImport("libjulia.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern void jl_init__threading();

        //[DllImport("libjulia.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        [DllImport("libjulia.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr jl_eval_string(string str);

        [DllImport("libjulia.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr jl_exception_occurred();

        [DllImport("libjulia.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr jl_typeof_str(IntPtr value);

        //[DllImport("libjulia.dll", CallingConvention = CallingConvention.Cdecl)]
        //public static extern IntPtr jl_box_float64(float value);

        //[DllImport("libjulia.dll", CallingConvention = CallingConvention.Cdecl)]
        //public static extern double jl_unbox_float64(IntPtr value);

        //[DllImport("libjulia.dll", CallingConvention = CallingConvention.Cdecl)]
        [DllImport("libjulia.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void jl_atexit_hook(int a);

        //[DllImport("libjulia.dll", CallingConvention = CallingConvention.Cdecl)]
        //public static extern IntPtr jl_array_data(IntPtr value);

        private static void Eval_string(string command)
        {
            IntPtr exception;
            try
            {
                jl_eval_string(command);
                exception = jl_exception_occurred();
                if (exception != (IntPtr)0x0)
                {
                    string exceptionString = System.Runtime.InteropServices.Marshal.PtrToStringAnsi(jl_typeof_str(jl_exception_occurred()));
                    string log_output = "Julia command <" + command + "> caused exception:";
                    doTimeTable.Form1.logWindow.Write_to_log_fromThread(log_output);
                    doTimeTable.Form1.logWindow.Write_to_log_fromThread(exceptionString);
                    //Console.WriteLine(exceptionString);
                }
            }
            catch (Exception e)
            {
                doTimeTable.Form1.logWindow.Write_to_log_fromThread(e.ToString());
            }
        }

        private static readonly int id = Process.GetCurrentProcess().Id;
        private static readonly NamedPipeServerStream serverPipe = new NamedPipeServerStream("consoleRedirect" + id, PipeDirection.In, 1, PipeTransmissionMode.Byte, PipeOptions.WriteThrough);
        private static readonly NamedPipeClientStream clientPipe = new NamedPipeClientStream(".", "consoleRedirect" + id, PipeDirection.Out, PipeOptions.WriteThrough);

        public static bool juliaRunning = false;

        ~NativeMethods()
        {
            clientPipe.Dispose();
            serverPipe.Dispose();
        }

        private static bool thread_serverPipe = false;
        private static bool thread_juliaWorker = false;
        public static Thread serverPipeThread = null;
        public static Thread workerThread = null;
        private static string staticJuliaDir;
        private static string staticJuliaScriptDir;
        private static string staticProjectPath;
        public static bool juliaThreadsRunning = false;
        public static bool restartWorkerThread = false;
        public static bool stopWorkerThread = false;
        public static bool stopServerPipeThread = false;
        public static bool workerThreadStopped = false;
        public static bool serverPipeThreadStopped = false;

        public static void StopThreads()
        {
            if (juliaThreadsRunning)
            {
                stopWorkerThread = true;

                int check_thread = 0;
                while (check_thread < 100)
                {
                    Thread.Sleep(10);
                    check_thread++;
                    if (workerThreadStopped)
                    {
                        check_thread = 100;
                    }
                }

                check_thread = 0;
                while (check_thread < 100)
                {
                    Thread.Sleep(10);
                    check_thread++;
                    if (serverPipeThreadStopped)
                    {
                        check_thread = 100;
                    }
                }

                if (!workerThreadStopped)
                {
                }
                if (!serverPipeThreadStopped)
                {
                }
            }
        }

        public static void ThreadServerPipe()
        {
            thread_serverPipe = true;
            if (!serverPipe.IsConnected)
            {
                serverPipe.WaitForConnection();
            }
            using (StreamReader stm = new StreamReader(serverPipe))
            {
                string outString;
                while (serverPipe.IsConnected && !stopServerPipeThread)
                {
                    try
                    {
                        string streamline = stm.ReadLine();
                        if (!string.IsNullOrEmpty(streamline))
                        {
                            if (!streamline.Contains("###PROGRESS###"))
                            {
                                if (streamline.Contains("###NOHEADER###"))
                                {
                                    outString = streamline.Replace("###NOHEADER###", "");
                                    doTimeTable.Form1.logWindow.Write_to_log_fromThread(outString);
                                }
                                else if (streamline.Contains("###USERMSG###"))
                                {
                                    outString = streamline.Replace("###USERMSG###", "");
                                    Object[] values = { outString };
                                    doTimeTable.Progress.currentProgress.ShowWarningText_fromThread(values);
                                }
                                else
                                {
                                    outString = "Console out: " + streamline;
                                    doTimeTable.Form1.logWindow.Write_to_log_fromThread(outString);
                                }
                            }
                            else
                            {
                                // Update progress meter here
                                int value = Convert.ToInt32(streamline.Replace("###PROGRESS###", ""));
                                Object[] values = { value };
                                doTimeTable.Progress.currentProgress.SetProgress2_fromThread(values);
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        doTimeTable.Form1.logWindow.Write_to_log_fromThread(e.ToString());
                        break;
                    }
                }
                doTimeTable.Form1.logWindow.Write_to_log_fromThread("ThreadServerPipe: Julia ServerPipe thread finished.");
            }
            thread_serverPipe = false;
            serverPipeThreadStopped = true;
        }

        public static void ThreadWorker()
        {
            string log_output;

            thread_juliaWorker = true;
            string script;
            while (!stopWorkerThread)
            {
                try
                {
                    if (!clientPipe.IsConnected)
                    {
                        clientPipe.Connect(10000);
                        HandleRef pipeHandle = new HandleRef(clientPipe, clientPipe.SafePipeHandle.DangerousGetHandle());
                        SetStdHandle(-11, pipeHandle.Handle);
                        SetStdHandle(-12, pipeHandle.Handle);
                    }

                    SetDllDirectory(staticJuliaDir);
                    jl_init__threading();
                    clientPipe.WaitForPipeDrain();

                    script = @"println(""Depot path: "",string(DEPOT_PATH))";
                    Eval_string(script);
                    clientPipe.WaitForPipeDrain();
                    script = @"println(""###NOHEADER###empty!(DEPOT_PATH)"")";
                    Eval_string(script);
                    clientPipe.WaitForPipeDrain();
                    script = @"println(""###NOHEADER###push!(DEPOT_PATH,raw\"""",string(DEPOT_PATH[1]),""\"")"")";
                    Eval_string(script);
                    clientPipe.WaitForPipeDrain();

                    script = @"println(""Julia version: "",string(VERSION))";
                    Eval_string(script);
                    clientPipe.WaitForPipeDrain();

                    //Thread.Sleep(2000);

                    script = @"cd(raw""" + staticProjectPath + @""");";
                    Eval_string(script);
                    clientPipe.WaitForPipeDrain();

                    script = @"println(""Pwd: "",pwd())";
                    Eval_string(script);
                    clientPipe.WaitForPipeDrain();
                    script = @"println(""###NOHEADER###cd(raw\"""",pwd(),""\"")"")";
                    Eval_string(script);
                    clientPipe.WaitForPipeDrain();

                    log_output = @"include(""" + staticJuliaScriptDir + "roster.jl" + @""")";
                    doTimeTable.Form1.logWindow.Write_to_log_fromThread(log_output);
                    log_output = @"include(""" + staticJuliaScriptDir.Replace("\\", "/") + "roster.jl" + @""")";
                    doTimeTable.Form1.logWindow.Write_to_log_fromThread(log_output);

                    script = @"Base.include(Base,raw""" + staticJuliaScriptDir + @"roster.jl"")";
                    Eval_string(script);
                    clientPipe.WaitForPipeDrain();

                    string exceptionString = "ThreadWorker: calculation finished.";
                    doTimeTable.Form1.logWindow.Write_to_log_fromThread(exceptionString);

                    //jl_atexit_hook(0);
                }
                catch (Exception e)
                {
                    string exceptionString = "ThreadWorker: " + e.ToString();
                    doTimeTable.Form1.logWindow.Write_to_log_fromThread(exceptionString);
                }

                doTimeTable.Progress.currentProgress.Julia_ready_fromThread();
                juliaRunning = false;

                restartWorkerThread = false;
                while (!restartWorkerThread && !stopWorkerThread)
                {
                    thread_juliaWorker = false;
                    Thread.Sleep(500);
                }
            }

            stopServerPipeThread = true;

            //needed STDOUT output for ServerPipeThread return from ReadLine
            script = @"println(""End."")";
            Eval_string(script);
            clientPipe.WaitForPipeDrain();

            jl_atexit_hook(0);

            log_output = "ThreadWorker: Julia worker thread finished.";
            doTimeTable.Form1.logWindow.Write_to_log_fromThread(log_output);

            workerThreadStopped = true;
        }

        public static void TouchJuliaAbortFile()
        {
            FileStream abort = File.Create(staticProjectPath + Path.DirectorySeparatorChar + "abort");
            abort.Close();
        }

        public static void Julia(string juliaDir, string juliaScriptDir, string projectPath)
        {
            juliaThreadsRunning = true;

            staticJuliaDir = juliaDir;
            staticJuliaScriptDir = juliaScriptDir;
            staticProjectPath = projectPath;

            if (File.Exists(projectPath + Path.DirectorySeparatorChar + "abort"))
            {
                File.Delete(projectPath + Path.DirectorySeparatorChar + "abort");
            }

            if (doTimeTable.Form1.myself.julia_ok)
            {
                Environment.SetEnvironmentVariable("JULIA_DEPOT_PATH", juliaDir + @"..\julia_depot");

                if (!thread_serverPipe)
                {
                    serverPipeThread = new Thread(new ThreadStart(ThreadServerPipe));
                    serverPipeThread.Start();
                }
                int check_thread = 0;
                while (check_thread < 100)
                {
                    Thread.Sleep(10);
                    check_thread++;
                    if (thread_serverPipe)
                    {
                        check_thread = 100;
                    }
                }
                if (thread_serverPipe)
                {
                    if (!thread_juliaWorker)
                    {
                        if (workerThread == null)
                        {
                            workerThread = new Thread(new ThreadStart(ThreadWorker));
                            workerThread.Start();

                            check_thread = 0;
                            while (check_thread < 100)
                            {
                                Thread.Sleep(10);
                                check_thread++;
                                if (thread_juliaWorker)
                                {
                                    check_thread = 100;
                                }
                            }
                            if (!thread_juliaWorker)
                            {
                                string log_output = "Failed to start thread for juliaWorker";
                                doTimeTable.Form1.logWindow.Write_to_log(ref log_output);
                            }
                        }
                        else
                        {
                            if (workerThread.IsAlive)
                            {
                                restartWorkerThread = true;
                            }
                            else
                            {
                                string log_output = "juliaWorker is unexpectedly dead";
                                doTimeTable.Form1.logWindow.Write_to_log(ref log_output);
                            }
                        }
                    }
                }
                else
                {
                    string log_output = "Failed to start thread for serverPipe";
                    doTimeTable.Form1.logWindow.Write_to_log(ref log_output);
                }
            }
        }
    }
}
