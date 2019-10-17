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
using System.Windows.Forms;

using System.Resources;
using System.Timers;
using System.Text.RegularExpressions;
using System.Linq;

namespace doTimeTable
{
    public partial class Form2 : Form
    {
        private string geometry_komponente = "editForm2";
        private bool geometry_changed = false;
        private string geometry;

        private readonly Form1.SubformType mySubformType = Form1.SubformType.UNDEF;
        private readonly string myId = "";
        private bool fillInExampleDataButton2 = false;
        private bool dataNeedsSave = false;
        private bool skipCellEndEdit = false;
        private bool cellCurrentlyInEdit = false;
        private bool readonlyColumnsExist = false;
        private bool columnClickDoSort = true;
        private readonly bool forceSaveButton = false;
        private readonly bool formReadOnly;
        public bool skipActivateEvent = false;

        //private DataGridViewComboBoxColumn combobox_teacher;
        //private DataGridViewComboBoxColumn combobox_classes;
        //private DataGridViewComboBoxColumn combobox_subjects;
        //private DataGridViewComboBoxColumn combobox_days;
        //private DataGridViewComboBoxColumn combobox_hours;

        public static ResourceManager LocRM = new ResourceManager("doTimeTable.WinFormStrings", typeof(Form2).Assembly);
        public static List<Form2> myInstantiations = new List<Form2> { };
        public static Dictionary<string, Form2> myOpenWindows = new Dictionary<string, Form2> { };

        private enum AllowedValues
        {
            INT=0,
            EMPTY=1,
            ZERO=2,
        }
        private readonly List<string> allowedTypesInfo = new List<string> {
            LocRM.GetString("String71"),
            LocRM.GetString("String72"),
            LocRM.GetString("String73"),
        };
        private enum AllowedValuesConstraints
        {
            UNIQUE,
            POSITIVE,
            NOTEMPTY,
        }
        private readonly List<string> constraintTypesInfo = new List<string> {
            LocRM.GetString("String74"),
            LocRM.GetString("String75"),
            LocRM.GetString("String76"),
        };

        private readonly List<List<AllowedValues>> checkAllowedValues = new List<List<AllowedValues>>();
        private readonly List<List<AllowedValuesConstraints>> checkAllowedValuesConstraints = new List<List<AllowedValuesConstraints>>();

        private int undoRedoNextRowID = 0;

        private enum UndoRedoEntryTypes
        {
            EMPTY,
            VALUE,
            LINE_DELETE,
            LINE_INSERT
        };

        private class UndoRedoEntry
        {
            static private int lastid = 0;
            public int id = -1;
            public UndoRedoEntryTypes type = UndoRedoEntryTypes.EMPTY;
            public string rowID = "";
            public int col = -1;
            public string value = "";
            public UndoRedoEntry()
            {
                this.id = lastid;
            }
            public UndoRedoEntry Clone()
            {
                UndoRedoEntry clone = new UndoRedoEntry
                {
                    id = this.id,
                    type = this.type,
                    rowID = this.rowID,
                    col = this.col,
                    value = this.value
                };
                return clone;
            }
            public void IncrementID()
            {
                lastid++;
                id = lastid;
            }
        };
        private readonly Stack<UndoRedoEntry> undo = new Stack<UndoRedoEntry>();
        private readonly Stack<UndoRedoEntry> redo = new Stack<UndoRedoEntry>();
        private UndoRedoEntry undoRedoBeginEdit = new UndoRedoEntry();

        private System.Timers.Timer aTimer = null;
        //private readonly System.Timers.Timer aTimer = new System.Timers.Timer();
        private readonly List<DataGridViewCellStyle> effectCells = new List<DataGridViewCellStyle>();
        private readonly List<Color> defaultBackground = new List<Color>();
        private readonly List<int> effectStep = new List<int>();
        private readonly List<DataGridViewCell> isComboBox = new List<DataGridViewCell>();

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
                        /*
                        bool skip = false;
                        skip = skip || (
                                Form1.logWindow != null &&
                                Form1.logWindow.Visible &&
                                Form1.logWindow.WindowState != FormWindowState.Minimized
                                );
                        skip = skip || (
                            Form1.myself != null &&
                            Form1.myself.Visible &&
                            Form1.myself.WindowState == FormWindowState.Minimized
                            );
                        foreach (Form2 form2 in Form2.myInstantiations)
                        {
                            if (form2 != this)
                            {
                                skip = skip || (
                                    form2 != null &&
                                    form2.Visible &&
                                    form2.WindowState != FormWindowState.Minimized
                                    );
                            }
                        }
                        skip = skip || (
                            Progress.currentProgress != null &&
                            Progress.currentProgress.Visible &&
                            Progress.currentProgress.WindowState == FormWindowState.Minimized
                            );
                        if (skip)
                        {
                            Form1.skipActivateEvent = true;
                        }
                        */
                    }
                    break;
            }
            base.WndProc(ref m);
        }

        public Form2(Form1.SubformType subformType, string id, string projectName)
        {
            mySubformType = subformType;
            myId = id;

            formReadOnly = Form1.projects.RegisterForm(myId, mySubformType);

            geometry_komponente = "editForm2_" + mySubformType;

            Form1.SubformType lastEdit = Form1.projects.GetState(myId);
            if (lastEdit < subformType && subformType != Form1.SubformType.CONFIG)
            {
                forceSaveButton = true;
            }

            InitializeComponent();

            splitContainer3.Panel2MinSize = 50;
            splitContainer3.Panel2Collapsed = true;

            textBox1.ForeColor = Color.Red;
            textBox1.BackColor = Color.White;

            switch (mySubformType)
            {
                case Form1.SubformType.DAYSHOURS:
                    this.SetWindowTitle(LocRM.GetString("String61") + " - " + projectName);
                    break;
                case Form1.SubformType.SUBJECT:
                    this.SetWindowTitle(LocRM.GetString("String23") + " - " + projectName);
                    break;
                case Form1.SubformType.CLASSES:
                    this.SetWindowTitle(LocRM.GetString("String47") + " - " + projectName);
                    break;
                case Form1.SubformType.TEACHER:
                    this.SetWindowTitle(LocRM.GetString("String87") + " - " + projectName);
                    break;
                case Form1.SubformType.TEACHERTOCLASS:
                    this.SetWindowTitle(LocRM.GetString("String103") + " - " + projectName);
                    break;
                case Form1.SubformType.CLASSTOSUBJECTPRESET:
                    this.SetWindowTitle(LocRM.GetString("String112") + " - " + projectName);
                    break;
                case Form1.SubformType.CONFIG:
                    this.SetWindowTitle(LocRM.GetString("String152") + " - " + projectName);
                    break;
                default:
                    break;
            }

            this.button1.Text = LocRM.GetString("String6");
            button1.BackColor = Color.Red;
            //button1.ForeColor = Color.Black;
            this.button2.Text = LocRM.GetString("String21");
            this.button3.Text = LocRM.GetString("String20");

            myInstantiations.Add(this);
            myOpenWindows[mySubformType + myId] = this;

            randomOrderToolStripMenuItem.Text = LocRM.GetString("String160");

            InitialSetupHelpPanel();
            InitialSetupDataGridView();
        }

        public void InitialSetupHelpPanel()
        {
            string body = "";

            body += LocRM.GetString("String32");

            switch (mySubformType)
            {
                case Form1.SubformType.DAYSHOURS:
                    body += LocRM.GetString("String77");
                    body += LocRM.GetString("String78");
                    body += LocRM.GetString("String31");
                    body += LocRM.GetString("String79");
                    body += LocRM.GetString("String80");
                    body += LocRM.GetString("String81");
                    body += LocRM.GetString("String82");
                    body += LocRM.GetString("String83");
                    body += LocRM.GetString("String84");
                    webBrowser1.DocumentText = body;
                    break;
                case Form1.SubformType.SUBJECT:
                    body += LocRM.GetString("String29");
                    body += LocRM.GetString("String30");
                    body += LocRM.GetString("String31");
                    body += LocRM.GetString("String33");
                    body += LocRM.GetString("String38");
                    body += LocRM.GetString("String35");
                    body += LocRM.GetString("String40");
                    body += LocRM.GetString("String34");
                    body += LocRM.GetString("String39");
                    body += LocRM.GetString("String83");
                    webBrowser1.DocumentText = body;
                    break;
                case Form1.SubformType.CLASSES:
                    body += LocRM.GetString("String49");
                    body += LocRM.GetString("String50");
                    body += LocRM.GetString("String31");
                    body += LocRM.GetString("String51");
                    body += LocRM.GetString("String52");
                    body += LocRM.GetString("String53");
                    body += LocRM.GetString("String54");
                    body += LocRM.GetString("String56");
                    body += LocRM.GetString("String83");
                    webBrowser1.DocumentText = body;
                    break;
                case Form1.SubformType.TEACHER:
                    body += LocRM.GetString("String90");
                    body += LocRM.GetString("String91");
                    body += LocRM.GetString("String31");
                    body += LocRM.GetString("String92");
                    body += LocRM.GetString("String93");
                    body += LocRM.GetString("String94");
                    body += LocRM.GetString("String95");
                    body += LocRM.GetString("String53");
                    body += LocRM.GetString("String96");
                    body += LocRM.GetString("String83");
                    webBrowser1.DocumentText = body;
                    break;
                case Form1.SubformType.TEACHERTOCLASS:
                    body += LocRM.GetString("String104");
                    body += LocRM.GetString("String105");
                    body += LocRM.GetString("String31");
                    body += LocRM.GetString("String51");
                    body += LocRM.GetString("String52");
                    body += LocRM.GetString("String53");
                    body += LocRM.GetString("String106");
                    body += LocRM.GetString("String107");
                    body += LocRM.GetString("String83");
                    webBrowser1.DocumentText = body;
                    break;
                case Form1.SubformType.CLASSTOSUBJECTPRESET:
                    body += LocRM.GetString("String113");
                    body += LocRM.GetString("String114");
                    body += LocRM.GetString("String31");
                    body += LocRM.GetString("String51");
                    body += LocRM.GetString("String52");
                    body += LocRM.GetString("String115");
                    body += LocRM.GetString("String116");
                    body += LocRM.GetString("String33").Replace("<ul>","");
                    body += LocRM.GetString("String117");
                    body += LocRM.GetString("String118");
                    body += LocRM.GetString("String119");
                    body += LocRM.GetString("String120");
                    body += LocRM.GetString("String121");
                    body += LocRM.GetString("String83");

                    body += LocRM.GetString("String123");
                    body += "<table style='width:100%'>";
                    body += "<tr>";
                    body += "<th style='text-align:left;'>";
                    body += LocRM.GetString("String55");
                    body += "</th>";
                    body += "<th style='text-align:left;'>";
                    body += LocRM.GetString("String115").Replace("<li><b>", "").Replace("</b></li>", "");
                    body += "</th>";
                    body += "<th style='text-align:left;'>";
                    body += LocRM.GetString("String24");
                    body += "</th>";
                    body += "<th style='text-align:left;'>";
                    body += LocRM.GetString("String118").Replace("<li><b>", "").Replace("</b></li>", "");
                    body += "</th>";
                    body += "<th style='text-align:left;'>";
                    body += LocRM.GetString("String120").Replace("<li><b>", "").Replace("</b></li>", "");
                    body += "</th>";
                    body += "</tr>";
                    body += "<tr>";
                    body += "<td style='text-align:left;'>";
                    body += "1a";
                    body += "</td>";
                    body += "<td style='text-align:left;'>";
                    body += "1";
                    body += "</td>";
                    body += "<td style='text-align:left;'>";
                    body += LocRM.GetString("String27");
                    body += "</td>";
                    body += "<td style='text-align:left;'>";
                    body += "</td>";
                    body += "<td style='text-align:left;'>";
                    body += "</td>";
                    body += "</tr>";
                    body += "<tr>";
                    body += "<td style='text-align:left;'>";
                    body += "1b";
                    body += "</td>";
                    body += "<td style='text-align:left;'>";
                    body += "1";
                    body += "</td>";
                    body += "<td style='text-align:left;'>";
                    body += LocRM.GetString("String27");
                    body += "</td>";
                    body += "<td style='text-align:left;'>";
                    body += "</td>";
                    body += "<td style='text-align:left;'>";
                    body += "</td>";
                    body += "</tr>";
                    body += "<tr>";
                    body += "<td style='text-align:left;'>";
                    body += "2a";
                    body += "</td>";
                    body += "<td style='text-align:left;'>";
                    body += "2";
                    body += "</td>";
                    body += "<td style='text-align:left;'>";
                    body += LocRM.GetString("String27");
                    body += "</td>";
                    body += "<td style='text-align:left;'>";
                    body += LocRM.GetString("String64");
                    body += "</td>";
                    body += "<td style='text-align:left;'>";
                    body += "3";
                    body += "</td>";
                    body += "</tr>";
                    body += "<tr>";
                    body += "<td style='text-align:left;'>";
                    body += "2b";
                    body += "</td>";
                    body += "<td style='text-align:left;'>";
                    body += "2";
                    body += "</td>";
                    body += "<td style='text-align:left;'>";
                    body += LocRM.GetString("String27");
                    body += "</td>";
                    body += "<td style='text-align:left;'>";
                    body += LocRM.GetString("String64");
                    body += "</td>";
                    body += "<td style='text-align:left;'>";
                    body += "3";
                    body += "</td>";
                    body += "</tr>";
                    body += "<tr>";
                    body += "<td style='text-align:left;'>";
                    body += "2a";
                    body += "</td>";
                    body += "<td style='text-align:left;'>";
                    body += "3";
                    body += "</td>";
                    body += "<td style='text-align:left;'>";
                    body += LocRM.GetString("String27");
                    body += "</td>";
                    body += "<td style='text-align:left;'>";
                    body += LocRM.GetString("String64");
                    body += "</td>";
                    body += "<td style='text-align:left;'>";
                    body += "4";
                    body += "</td>";
                    body += "</tr>";
                    body += "<tr>";
                    body += "<td style='text-align:left;'>";
                    body += "2b";
                    body += "</td>";
                    body += "<td style='text-align:left;'>";
                    body += "3";
                    body += "</td>";
                    body += "<td style='text-align:left;'>";
                    body += LocRM.GetString("String27");
                    body += "</td>";
                    body += "<td style='text-align:left;'>";
                    body += LocRM.GetString("String64");
                    body += "</td>";
                    body += "<td style='text-align:left;'>";
                    body += "4";
                    body += "</td>";
                    body += "</tr>";
                    body += "</table>";
                    body += LocRM.GetString("String124") + "<br />";
                    body += LocRM.GetString("String125");
                    webBrowser1.DocumentText = body;
                    break;
                case Form1.SubformType.CONFIG:
                    body += LocRM.GetString("String155");
                    body += LocRM.GetString("String156");
                    body += "<ul>";
                    body += "<li><b>relaxTeacherAvailability</b>";
                    body += LocRM.GetString("String157");
                    body += "</li>";
                    body += "<li><b>maxRosterPrefillCount</b>";
                    body += LocRM.GetString("String158");
                    body += "</li>";
                    body += "<li><b>backtraceWarningLimit</b>";
                    body += LocRM.GetString("String165");
                    body += "</li>";
                    body += "<li><b>classesInRandomOrder</b>";
                    body += LocRM.GetString("String167");
                    body += "</li>";
                    body += "<li><b>coursesInOrderOfTeacherCoverage</b>";
                    body += LocRM.GetString("String168");
                    body += "</li>";

                    body += "</ul>";

                    webBrowser1.DocumentText = body;
                    break;
                default:
                    break;
            }


        }

        public void InitialSetupDataGridView()
        {
            List<string> header = new List<string>();
            List<string> row = new List<string>();
            List<string[]> daysHours;
            List<string[]> subjects;
            List<string[]> classes;
            List<string[]> teacher;

            //dataGridView1 column header
            switch (mySubformType)
            {
                case Form1.SubformType.DAYSHOURS:
                    header.Add(LocRM.GetString("String62"));
                    header.Add(LocRM.GetString("String63"));
                    header.Add("");
                    row.Add("");
                    row.Add("");
                    row.Add("");
                    break;
                case Form1.SubformType.SUBJECT:
                    dataGridView1.ContextMenuStrip = contextMenuStrip1;
                    header.Add(LocRM.GetString("String24"));
                    header.Add(LocRM.GetString("String26"));
                    header.Add(LocRM.GetString("String25"));
                    header.Add("");
                    row.Add("");
                    row.Add("");
                    row.Add("");
                    row.Add("");
                    break;
                case Form1.SubformType.CLASSES:
                    //dataGridView1.ContextMenuStrip = contextMenuStrip1;
                    header.Add(LocRM.GetString("String55"));
                    row.Add("");
                    subjects = Form1.projects.LoadData(myId, Form1.SubformType.SUBJECT);
                    foreach (string[] subjectRow in subjects)
                    {
                        if (subjectRow[0].Length>0)
                        {
                            header.Add(subjectRow[0]);
                            row.Add("");
                        }
                    }
                    header.Add("");
                    row.Add("");
                    break;
                case Form1.SubformType.TEACHER:
                    dataGridView1.ContextMenuStrip = contextMenuStrip1;
                    header.Add(LocRM.GetString("String88"));
                    header.Add(LocRM.GetString("String89"));
                    row.Add("");
                    row.Add("");
                    subjects = Form1.projects.LoadData(myId, Form1.SubformType.SUBJECT);
                    foreach (string[] subjectRow in subjects)
                    {
                        if (subjectRow[0].Length > 0)
                        {
                            header.Add(subjectRow[0]);
                            row.Add("");
                        }
                    }
                    header.Add("");
                    row.Add("");
                    break;
                case Form1.SubformType.TEACHERTOCLASS:
                    header.Add(LocRM.GetString("String55"));
                    row.Add("");
                    subjects = Form1.projects.LoadData(myId, Form1.SubformType.SUBJECT);
                    foreach (string[] subjectRow in subjects)
                    {
                        if (subjectRow[0].Length > 0)
                        {
                            header.Add(subjectRow[0]);
                            row.Add("");
                        }
                    }
                    header.Add("");
                    row.Add("");
                    break;
                case Form1.SubformType.CLASSTOSUBJECTPRESET:
                    header.Add(LocRM.GetString("String55"));
                    row.Add("");
                    header.Add(LocRM.GetString("String115").Replace("<li><b>", "").Replace("</b></li>", ""));
                    row.Add("");
                    header.Add(LocRM.GetString("String24"));
                    row.Add("");
                    header.Add(LocRM.GetString("String118").Replace("<li><b>", "").Replace("</b></li>", ""));
                    row.Add("");
                    header.Add(LocRM.GetString("String120").Replace("<li><b>", "").Replace("</b></li>", ""));
                    row.Add("");
                    header.Add("");
                    row.Add("");
                    break;
                case Form1.SubformType.CONFIG:
                    header.Add(LocRM.GetString("String153"));
                    row.Add("");
                    header.Add(LocRM.GetString("String154"));
                    row.Add("");
                    header.Add("");
                    row.Add("");
                    break;
                default:
                    break;
            }

            //dataGridView1 column types
            switch (mySubformType)
            {
                case Form1.SubformType.DAYSHOURS:
                    checkAllowedValues.Add(new List<AllowedValues> { });
                    checkAllowedValuesConstraints.Add(new List<AllowedValuesConstraints> { AllowedValuesConstraints.UNIQUE });
                    checkAllowedValues.Add(new List<AllowedValues> { });
                    checkAllowedValuesConstraints.Add(new List<AllowedValuesConstraints> { AllowedValuesConstraints.UNIQUE });
                    checkAllowedValues.Add(new List<AllowedValues> { });
                    checkAllowedValuesConstraints.Add(new List<AllowedValuesConstraints> { });
                    break;
                case Form1.SubformType.SUBJECT:
                    checkAllowedValues.Add(new List<AllowedValues> { });
                    checkAllowedValuesConstraints.Add(new List<AllowedValuesConstraints> { AllowedValuesConstraints.NOTEMPTY, AllowedValuesConstraints.UNIQUE });
                    checkAllowedValues.Add(new List<AllowedValues> { AllowedValues.INT, AllowedValues.ZERO, AllowedValues.EMPTY });
                    checkAllowedValuesConstraints.Add(new List<AllowedValuesConstraints> { AllowedValuesConstraints.POSITIVE });
                    checkAllowedValues.Add(new List<AllowedValues> { AllowedValues.INT, AllowedValues.ZERO, AllowedValues.EMPTY });
                    checkAllowedValuesConstraints.Add(new List<AllowedValuesConstraints> { AllowedValuesConstraints.POSITIVE });
                    checkAllowedValues.Add(new List<AllowedValues> { });
                    checkAllowedValuesConstraints.Add(new List<AllowedValuesConstraints> { });
                    break;
                case Form1.SubformType.CLASSES:
                    checkAllowedValues.Add(new List<AllowedValues> { });
                    checkAllowedValuesConstraints.Add(new List<AllowedValuesConstraints> { AllowedValuesConstraints.NOTEMPTY, AllowedValuesConstraints.UNIQUE });
                    for (int i = 1; i < header.Count-1; i++)
                    {
                        checkAllowedValues.Add(new List<AllowedValues> { AllowedValues.INT, AllowedValues.ZERO });
                        checkAllowedValuesConstraints.Add(new List<AllowedValuesConstraints> { AllowedValuesConstraints.NOTEMPTY, AllowedValuesConstraints.POSITIVE });
                    }
                    checkAllowedValues.Add(new List<AllowedValues> { });
                    checkAllowedValuesConstraints.Add(new List<AllowedValuesConstraints> { });
                    break;
                case Form1.SubformType.TEACHER:
                    checkAllowedValues.Add(new List<AllowedValues> { });
                    checkAllowedValuesConstraints.Add(new List<AllowedValuesConstraints> { AllowedValuesConstraints.NOTEMPTY, AllowedValuesConstraints.UNIQUE });
                    for (int i = 1; i < header.Count - 1; i++)
                    {
                        checkAllowedValues.Add(new List<AllowedValues> { AllowedValues.INT, AllowedValues.ZERO, AllowedValues.EMPTY });
                        checkAllowedValuesConstraints.Add(new List<AllowedValuesConstraints> {AllowedValuesConstraints.POSITIVE });
                    }
                    checkAllowedValues.Add(new List<AllowedValues> { });
                    checkAllowedValuesConstraints.Add(new List<AllowedValuesConstraints> { });
                    break;
                case Form1.SubformType.TEACHERTOCLASS:
                    for (int i = 0; i < header.Count; i++)
                    {
                        checkAllowedValues.Add(new List<AllowedValues> { });
                        checkAllowedValuesConstraints.Add(new List<AllowedValuesConstraints> { });
                    }
                    break;
                case Form1.SubformType.CLASSTOSUBJECTPRESET:
                    checkAllowedValues.Add(new List<AllowedValues> { });
                    checkAllowedValuesConstraints.Add(new List<AllowedValuesConstraints> { AllowedValuesConstraints.NOTEMPTY });
                    checkAllowedValues.Add(new List<AllowedValues> { });
                    checkAllowedValuesConstraints.Add(new List<AllowedValuesConstraints> { AllowedValuesConstraints.NOTEMPTY });
                    checkAllowedValues.Add(new List<AllowedValues> { });
                    checkAllowedValuesConstraints.Add(new List<AllowedValuesConstraints> { AllowedValuesConstraints.NOTEMPTY });
                    checkAllowedValues.Add(new List<AllowedValues> { });
                    checkAllowedValuesConstraints.Add(new List<AllowedValuesConstraints> { });
                    checkAllowedValues.Add(new List<AllowedValues> { });
                    checkAllowedValuesConstraints.Add(new List<AllowedValuesConstraints> { });
                    checkAllowedValues.Add(new List<AllowedValues> { });
                    checkAllowedValuesConstraints.Add(new List<AllowedValuesConstraints> { });
                    break;
                case Form1.SubformType.CONFIG:
                    //checkAllowedValues.Add(new List<AllowedValues> { });
                    //checkAllowedValuesConstraints.Add(new List<AllowedValuesConstraints> { AllowedValuesConstraints.UNIQUE });
                    checkAllowedValues.Add(new List<AllowedValues> { AllowedValues.INT, AllowedValues.ZERO });
                    checkAllowedValuesConstraints.Add(new List<AllowedValuesConstraints> { AllowedValuesConstraints.NOTEMPTY });
                    checkAllowedValues.Add(new List<AllowedValues> { });
                    checkAllowedValuesConstraints.Add(new List<AllowedValuesConstraints> { });
                    break;
                default:
                    break;
            }

            dataGridView1.Rows.Clear();
            dataGridView1.ColumnCount = header.Count;
            for (int i = 0; i < header.Count; i++)
            {
                dataGridView1.Columns[i].HeaderText = header[i];
                if( header[i].Length == 0)
                {
                    dataGridView1.Columns[i].Visible = false;
                }
            }
            //dataGridView1.Columns[header.Length - 1].Visible = false;

            List<string[]> gridData;
            string[] newRow;
            switch (mySubformType)
            {
                case Form1.SubformType.DAYSHOURS:
                case Form1.SubformType.SUBJECT:
                    gridData = Form1.projects.LoadData(myId, mySubformType);
                    if (gridData.Count > 0)
                    {
                        newRow = new string[dataGridView1.ColumnCount];
                        foreach (string[] rowData in gridData)
                        {
                            if (rowData.Length <= dataGridView1.ColumnCount)
                            {
                                for (int index = 0; index < newRow.Length; index++)
                                {
                                    if (index < rowData.Length && rowData[index] != null)
                                    {
                                        newRow[index] = rowData[index];
                                    }
                                    else
                                    {
                                        newRow[index] = "";
                                    }
                                }
                                newRow[dataGridView1.ColumnCount - 1] = undoRedoNextRowID.ToString();
                                undoRedoNextRowID++;
                                dataGridView1.Rows.Add(newRow);
                            }
                        }
                    }
                    break;
                case Form1.SubformType.CLASSES:
                case Form1.SubformType.TEACHER:
                    gridData = Form1.projects.LoadData(myId, mySubformType, false);
                    if (gridData.Count > 0)
                    {
                        int colCount;
                        if (header.Count > gridData[0].Length + 1)
                        {
                            colCount = header.Count + 1;
                        }
                        else
                        {
                            colCount = gridData[0].Length + 1;
                        }
                        newRow = new string[colCount];
                        Dictionary<int, int> columnMap = new Dictionary<int, int>();
                        Dictionary<int, int> columnsNotFound = new Dictionary<int, int>();
                        for (int rowIndex = 0; rowIndex < gridData.Count; rowIndex++)
                        {
                            if (rowIndex == 0)
                            {
                                columnMap[0] = 0;
                                for (int colIndex = 1; colIndex < dataGridView1.ColumnCount - 1; colIndex++)
                                {
                                    string colHeader = dataGridView1.Columns[colIndex].HeaderText;
                                    for (int j = 1; j < gridData[rowIndex].Length - 1; j++)
                                    {
                                        if (!columnsNotFound.ContainsKey(j))
                                        {
                                            columnsNotFound[j] = 0;
                                        }
                                        if (colHeader == gridData[rowIndex][j])
                                        {
                                            columnMap[colIndex] = j;
                                            columnsNotFound[j] = 1;
                                        }
                                    }
                                }
                                foreach (int j in columnsNotFound.Keys)
                                {
                                    if (columnsNotFound[j] == 0)
                                    {
                                        DataGridViewCellStyle style = new DataGridViewCellStyle
                                        {
                                            BackColor = Color.LightGray
                                        };
                                        int newColIndex = dataGridView1.ColumnCount - 1;
                                        DataGridViewColumn col = new DataGridViewColumn
                                        {
                                            CellTemplate = dataGridView1.Columns[0].CellTemplate,
                                            HeaderText = gridData[rowIndex][j]
                                        };
                                        dataGridView1.Columns.Insert(newColIndex, col);
                                        dataGridView1.Columns[newColIndex].ReadOnly = true;
                                        dataGridView1.Columns[newColIndex].DefaultCellStyle = style;
                                        columnMap[newColIndex] = j;
                                        readonlyColumnsExist = true;
                                    }
                                }
                                newRow = new string[dataGridView1.Columns.Count];
                                ToggleSaveButton();
                            }
                            else
                            {
                                string[] rowData = gridData[rowIndex];
                                for (int colIndex = 0; colIndex < dataGridView1.ColumnCount - 1; colIndex++)
                                {
                                    if (columnMap.ContainsKey(colIndex))
                                    {
                                        newRow[colIndex] = rowData[columnMap[colIndex]];
                                    }
                                    else
                                    {
                                        newRow[colIndex] = "";
                                    }
                                }
                                newRow[dataGridView1.ColumnCount - 1] = undoRedoNextRowID.ToString();
                                undoRedoNextRowID++;
                                dataGridView1.Rows.Add(newRow);
                            }
                        }
                    }
                    break;
                case Form1.SubformType.TEACHERTOCLASS:
                    gridData = Form1.projects.LoadData(myId, mySubformType, false);
                    classes = Form1.projects.LoadData(myId, Form1.SubformType.CLASSES, true);
                    teacher = Form1.projects.LoadData(myId, Form1.SubformType.TEACHER, false);
                    subjects = Form1.projects.LoadData(myId, Form1.SubformType.SUBJECT, true);
                    if (classes.Count > 0 && teacher.Count > 0 && subjects.Count > 0)
                    {
                        newRow = new string[dataGridView1.ColumnCount];
                        string[] rowData;
                        for (int rowindex = 0; rowindex < classes.Count; rowindex++)
                        {
                            rowData = classes[rowindex];
                            newRow[0] = rowData[0];
                            for (int index = 1; index < newRow.Length; index++)
                            {
                                newRow[index] = "";
                            }
                            newRow[dataGridView1.ColumnCount - 1] = undoRedoNextRowID.ToString();
                            undoRedoNextRowID++;
                            dataGridView1.Rows.Add(newRow);
                        }
                        dataGridView1.Columns[0].ReadOnly = true;
                        for (int colIndex = 1; colIndex < dataGridView1.Columns.Count - 1; colIndex++)
                        {
                            string subject = dataGridView1.Columns[colIndex].HeaderText;
                            DataGridViewComboBoxColumn combobox_teacher = new DataGridViewComboBoxColumn
                            {
                                Name = subject,
                                AutoComplete = true
                            };
                            dataGridView1.Columns.Insert(colIndex, combobox_teacher);
                            dataGridView1.Columns[colIndex].SortMode = DataGridViewColumnSortMode.Automatic;
                            dataGridView1.Columns.RemoveAt(colIndex + 1);
                            combobox_teacher.Items.Add("");
                            for (int teacherColIndex = 0; teacherColIndex < teacher[0].Length; teacherColIndex++)
                            {
                                if (teacher[0][teacherColIndex] == subject)
                                {
                                    for (int teacherRowIndex = 1; teacherRowIndex < teacher.Count; teacherRowIndex++) {
                                        string[] teacherRow = teacher[teacherRowIndex];
                                        string value = teacherRow[teacherColIndex].Trim();
                                        string pattern = @"^\d+$";
                                        Regex rgx = new Regex(pattern);
                                        if (rgx.IsMatch(value))
                                        {
                                            if (Int32.Parse(value) > 0)
                                            {
                                                combobox_teacher.Items.Add(teacherRow[0]);
                                            }
                                        }
                                    }
                                    break;
                                }
                            }
                            if (gridData.Count > 0)
                            {
                                foreach (DataGridViewRow dgRow in dataGridView1.Rows)
                                {
                                    if (!dgRow.IsNewRow)
                                    {
                                        string className = dgRow.Cells[0].Value.ToString();
                                        int gridDataCol = -1;
                                        for (int gridDataColIndex = 1; gridDataColIndex < gridData[0].Length; gridDataColIndex++)
                                        {
                                            if (gridData[0][gridDataColIndex] == subject)
                                            {
                                                gridDataCol = gridDataColIndex;
                                            }
                                        }
                                        if (gridDataCol >= 0)
                                        {
                                            foreach (string[] gridDataRow in gridData)
                                            {
                                                if (gridDataRow[0] == className)
                                                {
                                                    if (combobox_teacher.Items.Contains(gridDataRow[gridDataCol]))
                                                    {
                                                        dataGridView1.Rows[dgRow.Index].Cells[colIndex].Value = gridDataRow[gridDataCol];
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        splitContainer3.Panel2Collapsed = false;
                        textBox1.AppendText(LocRM.GetString("String108"));
                        dataGridView1.ReadOnly = true;
                        button2.Enabled = false;
                    }
                    dataGridView1.AllowUserToAddRows = false;
                    break;
                case Form1.SubformType.CLASSTOSUBJECTPRESET:
                    gridData = Form1.projects.LoadData(myId, mySubformType, true);
                    classes = Form1.projects.LoadData(myId, Form1.SubformType.CLASSES, true);
                    subjects = Form1.projects.LoadData(myId, Form1.SubformType.SUBJECT, true);
                    daysHours = Form1.projects.LoadData(myId, Form1.SubformType.DAYSHOURS, true);
                    if (classes.Count > 0 && subjects.Count > 0)
                    {
                        int colIndex = 0;
                        string colheader = dataGridView1.Columns[colIndex].HeaderText;
                        DataGridViewComboBoxColumn combobox_classes = new DataGridViewComboBoxColumn
                        {
                            Name = colheader,
                            AutoComplete = true
                        };
                        dataGridView1.Columns.Insert(colIndex, combobox_classes);
                        dataGridView1.Columns[colIndex].SortMode = DataGridViewColumnSortMode.Automatic;
                        dataGridView1.Columns.RemoveAt(colIndex + 1);
                        int classColIndex = 0;
                        for (int classRowIndex = 0; classRowIndex < classes.Count; classRowIndex++)
                        {
                            string[] classRow = classes[classRowIndex];
                            combobox_classes.Items.Add(classRow[classColIndex]);
                        }
                        colIndex = 2;
                        colheader = dataGridView1.Columns[colIndex].HeaderText;
                        DataGridViewComboBoxColumn combobox_subjects = new DataGridViewComboBoxColumn
                        {
                            Name = colheader,
                            AutoComplete = true
                        };
                        dataGridView1.Columns.Insert(colIndex, combobox_subjects);
                        dataGridView1.Columns[colIndex].SortMode = DataGridViewColumnSortMode.Automatic;
                        dataGridView1.Columns.RemoveAt(colIndex + 1);
                        int subjectColIndex = 0;
                        for (int subjectRowIndex = 0; subjectRowIndex < subjects.Count; subjectRowIndex++)
                        {
                            string[] subjectRow = subjects[subjectRowIndex];
                            combobox_subjects.Items.Add(subjectRow[subjectColIndex]);
                        }
                        colIndex = 3;
                        colheader = dataGridView1.Columns[colIndex].HeaderText;
                        DataGridViewComboBoxColumn combobox_days = new DataGridViewComboBoxColumn
                        {
                            Name = colheader,
                            AutoComplete = true
                        };
                        combobox_days.Items.Add("");
                        dataGridView1.Columns.Insert(colIndex, combobox_days);
                        dataGridView1.Columns[colIndex].SortMode = DataGridViewColumnSortMode.Automatic;
                        dataGridView1.Columns.RemoveAt(colIndex + 1);
                        int dayColIndex = 0;
                        for (int dayRowIndex = 0; dayRowIndex < daysHours.Count; dayRowIndex++)
                        {
                            string[] dayRow = daysHours[dayRowIndex];
                            if (dayRow[dayColIndex] != null && dayRow[dayColIndex] != "")
                            {
                                combobox_days.Items.Add(dayRow[dayColIndex]);
                            }
                        }
                        colIndex = 4;
                        colheader = dataGridView1.Columns[colIndex].HeaderText;
                        DataGridViewComboBoxColumn combobox_hours = new DataGridViewComboBoxColumn
                        {
                            Name = colheader,
                            AutoComplete = true
                        };
                        combobox_hours.Items.Add("");
                        dataGridView1.Columns.Insert(colIndex, combobox_hours);
                        dataGridView1.Columns[colIndex].SortMode = DataGridViewColumnSortMode.Automatic;
                        dataGridView1.Columns.RemoveAt(colIndex + 1);
                        int hourColIndex = 1;
                        for (int hourRowIndex = 0; hourRowIndex < daysHours.Count; hourRowIndex++)
                        {
                            string[] dayRow = daysHours[hourRowIndex];
                            if (dayRow[hourColIndex] != null && dayRow[hourColIndex] != "")
                            {
                                combobox_hours.Items.Add(dayRow[hourColIndex]);
                            }
                        }
                        if (gridData.Count > 0)
                        {
                            foreach (string[] dataRow in gridData)
                            {
                                bool putRow = true;
                                for (int dataGridCol = 0; dataGridCol < dataGridView1.ColumnCount - 1; dataGridCol++)
                                {
                                    if (dataGridCol == 0)
                                    {
                                        DataGridViewComboBoxColumn combo = (DataGridViewComboBoxColumn)dataGridView1.Columns[dataGridCol];
                                        if (!combo.Items.Contains(dataRow[dataGridCol]))
                                        {
                                            putRow = false;
                                        }
                                    }
                                    if (dataGridCol == 2)
                                    {
                                        DataGridViewComboBoxColumn combo = (DataGridViewComboBoxColumn)dataGridView1.Columns[dataGridCol];
                                        if (!combo.Items.Contains(dataRow[dataGridCol]))
                                        {
                                            dataRow[dataGridCol] = "";
                                            readonlyColumnsExist = true;
                                            ToggleSaveButton();
                                        }
                                    }
                                    if (dataGridCol == 3 || dataGridCol == 4)
                                    {
                                        DataGridViewComboBoxColumn combo = (DataGridViewComboBoxColumn)dataGridView1.Columns[dataGridCol];
                                        if (!combo.Items.Contains(dataRow[dataGridCol]))
                                        {
                                            putRow = false;
                                        }
                                    }
                                }
                                if (putRow)
                                {
                                    dataGridView1.Rows.Add(dataRow);
                                }
                            }
                        }
                    }
                    else
                    {
                        splitContainer3.Panel2Collapsed = false;
                        textBox1.AppendText(LocRM.GetString("String122"));
                        dataGridView1.ReadOnly = true;
                        button2.Enabled = false;
                    }
                    break;
                case Form1.SubformType.CONFIG:
                    gridData = Form1.projects.LoadData(myId, mySubformType);
                    List<string> allowedParameters = new List<string> {
                        "relaxTeacherAvailability",
                        "maxRosterPrefillCount",
                        "backtraceWarningLimit",
                        "classesInRandomOrder",
                        "coursesInOrderOfTeacherCoverage",
                    };
                    List<string> allowedParametersDefaultValues = new List<string> {
                        "0",
                        "1",
                        "10000",
                        "0",
                        "0",
                    };
                    List<string> allowedParametersTooltips = new List<string> {
                        LocRM.GetString("String157").Replace("<br />",""),
                        LocRM.GetString("String158").Replace("<br />",""),
                        LocRM.GetString("String165").Replace("<br />",""),
                        LocRM.GetString("String167").Replace("<br />",""),
                        LocRM.GetString("String168").Replace("<br />",""),
                    };
                    List<string> availableParams = new List<string>();
                    if (gridData.Count > 0)
                    {
                        newRow = new string[dataGridView1.ColumnCount];
                        foreach (string[] rowData in gridData)
                        {
                            if (rowData.Length <= dataGridView1.ColumnCount)
                            {
                                if (allowedParameters.Contains(rowData[0]))
                                {
                                    availableParams.Add(rowData[0]);
                                    for (int index = 0; index < newRow.Length; index++)
                                    {
                                        if (index < rowData.Length && rowData[index] != null)
                                        {
                                            newRow[index] = rowData[index];
                                        }
                                        else
                                        {
                                            newRow[index] = "";
                                        }
                                    }
                                    newRow[dataGridView1.ColumnCount - 1] = undoRedoNextRowID.ToString();
                                    undoRedoNextRowID++;
                                    int rowIndex = dataGridView1.Rows.Add(newRow);
                                    int toolTipIndex = allowedParameters.FindIndex( x => x == rowData[0]);
                                    if (rowIndex >= 0 && rowIndex < dataGridView1.Rows.Count && toolTipIndex >= 0 && toolTipIndex < allowedParametersTooltips.Count)
                                    {
                                        dataGridView1.Rows[rowIndex].Cells[0].ToolTipText = allowedParametersTooltips[toolTipIndex];
                                    }
                                }
                            }
                        }
                    }
                    bool missFound = false;
                    for (int index = 0; index < allowedParameters.Count; index++)
                    {
                        string param = allowedParameters[index];
                        if (!availableParams.Contains(param))
                        {
                            newRow = new string[] { param, allowedParametersDefaultValues[index], undoRedoNextRowID.ToString() };
                            int rowIndex = dataGridView1.Rows.Add(newRow);
                            int toolTipIndex = allowedParameters.FindIndex(x => x == param);
                            if (rowIndex >= 0 && rowIndex < dataGridView1.Rows.Count && toolTipIndex >= 0 && toolTipIndex < allowedParametersTooltips.Count)
                            {
                                dataGridView1.Rows[rowIndex].Cells[0].ToolTipText = allowedParametersTooltips[toolTipIndex];
                            }
                            missFound = true;
                        }
                    }
                    if (missFound)
                    {
                        ToggleSaveButton(true);
                    }
                    dataGridView1.Columns[0].ReadOnly = true;
                    dataGridView1.AllowUserToAddRows = false;
                    break;
                default:
                    break;
            }


            switch (mySubformType)
            {
                case Form1.SubformType.CLASSES:
                case Form1.SubformType.DAYSHOURS:
                case Form1.SubformType.CLASSTOSUBJECTPRESET:
                case Form1.SubformType.TEACHERTOCLASS:
                case Form1.SubformType.CONFIG:
                    foreach (DataGridViewColumn column in dataGridView1.Columns)
                    {
                        column.SortMode = DataGridViewColumnSortMode.NotSortable;
                    }
                    columnClickDoSort = false;
                    break;
                default:
                    foreach (DataGridViewColumn column in dataGridView1.Columns)
                    {
                        column.SortMode = DataGridViewColumnSortMode.Automatic;
                    }
                    columnClickDoSort = true;
                    break;
            }

            if ( formReadOnly )
            {
                dataGridView1.ReadOnly = true;
                splitContainer3.Panel2Collapsed = false;
                textBox1.AppendText(
                    LocRM.GetString("String58") + Environment.NewLine);

                button2.Enabled = false;
            }
            else
            {
                if( Checkdata() )
                {
                    ToggleSaveButton(true);
                }
            }

            dataGridView1.ClearSelection();
            dataGridView1.Refresh();
        }

        public void SetWindowTitle(string title)
        {
            this.Text = title;
        }

        private void Save_geometry()
        {
            if (geometry_changed)
            {
                if (Form1.app_ready && geometry != null)
                {
                    geometry = this.Width.ToString() + ";";
                    geometry += this.Height.ToString() + ";";
                    geometry += this.Location.X.ToString() + ";";
                    geometry += this.Location.Y.ToString() + ";";
                    geometry += this.splitContainer1.SplitterDistance.ToString() + ";";
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
                    index = 0; if (values.Length > index) { this.Width = Convert.ToInt32(values[index]); }
                    index = 1; if (values.Length > index) { this.Height = Convert.ToInt32(values[index]); }
                    index = 3; if (values.Length > index) { this.DesktopLocation = new Point(Convert.ToInt32(geometry.Split(';')[index - 1]), Convert.ToInt32(geometry.Split(';')[index])); }
                    index = 4; if (values.Length > index) { this.splitContainer1.SplitterDistance = Convert.ToInt32(values[index]); }
                }
                catch
                {
                    string log_output = "Setting form2 geometry failed: <" + geometry + ">";
                    Form1.logWindow.Write_to_log(ref log_output);
                }
                if (!Form1.myself.IsOnScreen(this.DesktopLocation))
                {
                    this.DesktopLocation = new Point(0, 0);
                }

                geometry_changed = false;
            }

        }

        private void Form2_FormClosing(object sender, FormClosingEventArgs e)
        {
            if( CheckClosing() )
            {
                e.Cancel = true;
            }
        }

        public bool CheckClosing()
        {
            this.Focus();
            if (dataNeedsSave)
            {
                if (!ShowDataNeedsSaveDialog())
                {
                    //don't close
                    return true;
                }
            }
            return false;
        }

        private void Form2_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (aTimer != null)
            {
                aTimer.Stop();
                aTimer.Dispose();
            }

            myInstantiations.Remove(this);
            myOpenWindows.Remove(mySubformType + myId);

            Form1.projects.DeregisterForm(myId, mySubformType);

            Save_geometry();
        }

        private void Form2_Load(object sender, EventArgs e)
        {
            Set_geometry();
        }

        private void Form2_LocationChanged(object sender, EventArgs e)
        {
            geometry_changed = true;
        }

        private void Form2_SizeChanged(object sender, EventArgs e)
        {
            geometry_changed = true;
            CheckSplitter();
        }

        private void SplitContainer1_SplitterMoved(object sender, SplitterEventArgs e)
        {
            geometry_changed = true;
            CheckSplitter();
        }

        private void CheckSplitter()
        {
            if (this.WindowState != FormWindowState.Minimized)
            {
                if (this.Height - this.splitContainer1.SplitterDistance < 150)
                {
                    this.splitContainer1.SplitterDistance = (int)(this.Height - 150);
                }
            }
        }

        private void Form2_Activated(object sender, EventArgs e)
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
                    if (Progress.currentProgress.WindowOpen())
                    {
                        Form1.myself.WindowState = FormWindowState.Normal;
                        Form1.logWindow.WindowState = FormWindowState.Normal;
                    }
                    if (
                        Progress.currentProgress != null &&
                        Progress.currentProgress.WindowOpen() &&
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

        private void DataGridView1_RowsAdded(object sender, DataGridViewRowsAddedEventArgs e)
        {
            SetExampleDataButton();
        }

        private void DataGridView1_RowsRemoved(object sender, DataGridViewRowsRemovedEventArgs e)
        {
            SetExampleDataButton();
        }

        private void UserDeletingRow(int rowIndex)
        {
            undoRedoBeginEdit.IncrementID();
            DataGridViewRow row = dataGridView1.Rows[rowIndex];
            if (row != null && !row.IsNewRow)
            {
                string deletedRowID = row.Cells[dataGridView1.ColumnCount - 1].Value.ToString();
                foreach (DataGridViewCell cell in row.Cells)
                {
                    undoRedoBeginEdit.type = UndoRedoEntryTypes.VALUE;
                    undoRedoBeginEdit.rowID = deletedRowID;
                    undoRedoBeginEdit.col = cell.ColumnIndex;
                    if (cell.Value != null)
                    {
                        undoRedoBeginEdit.value = cell.Value.ToString();
                    }
                    else
                    {
                        undoRedoBeginEdit.value = "";
                    }
                    undo.Push(undoRedoBeginEdit.Clone());
                }
                undoRedoBeginEdit.type = UndoRedoEntryTypes.LINE_DELETE;
                undoRedoBeginEdit.rowID = deletedRowID;
                undoRedoBeginEdit.value = "";
                undo.Push(undoRedoBeginEdit.Clone());

                undoRedoBeginEdit.IncrementID();
                redo.Clear();
            }
            ToggleSaveButton();
        }

        private void DataGridView1_UserDeletingRow(object sender, DataGridViewRowCancelEventArgs e)
        {
            int rowIndex = e.Row.Index;
            UserDeletingRow(rowIndex);
        }

        private void UserAddedRow(int rowIndex)
        {
            if (
                dataGridView1.Rows[rowIndex].Cells[dataGridView1.ColumnCount - 1].Value == null ||
                dataGridView1.Rows[rowIndex].Cells[dataGridView1.ColumnCount - 1].Value.ToString() == "" ||
                false
                )
            {
                dataGridView1.Rows[rowIndex].Cells[dataGridView1.ColumnCount - 1].Value = undoRedoNextRowID.ToString();
                undoRedoNextRowID++;
            }

            UndoRedoEntry tmp = undoRedoBeginEdit.Clone();
            undoRedoBeginEdit.IncrementID();
            undoRedoBeginEdit = new UndoRedoEntry
            {
                col = tmp.col,
                rowID = tmp.rowID,
                type = tmp.type,
                value = tmp.value
            };

            tmp.type = UndoRedoEntryTypes.LINE_INSERT;
            tmp.rowID = dataGridView1.Rows[rowIndex].Cells[dataGridView1.ColumnCount - 1].Value.ToString();
            undo.Push(tmp);

            redo.Clear();
            ToggleSaveButton();
        }

        private void DataGridView1_UserAddedRow(object sender, DataGridViewRowEventArgs e)
        {
            int rowIndex = e.Row.Index - 1;
            UserAddedRow(rowIndex);
        }

        private void SetExampleDataButton()
        {
            if (dataGridView1.Rows.Count <= 1 
                && mySubformType != Form1.SubformType.CLASSTOSUBJECTPRESET
                && mySubformType != Form1.SubformType.CONFIG
                )
            {
                button2.Text = LocRM.GetString("String36");
                fillInExampleDataButton2 = true;
            }
            else
            {
                button2.Text = LocRM.GetString("String21");
                fillInExampleDataButton2 = false;
            }
        }

        private void Button2_Click(object sender, EventArgs e)
        {
            if (fillInExampleDataButton2)
            {
                FillInExampleData();
            }
            else
            {
                Checkdata();
            }
        }

        private void FillInExampleData()
        {
            string[] data;
            List<string[]> rows = new List<string[]>();
            List<string> dataList;

            switch (mySubformType)
            {
                case Form1.SubformType.DAYSHOURS:
                    data = new string[] { LocRM.GetString("String64"), "1", undoRedoNextRowID.ToString() };
                    undoRedoNextRowID++;
                    rows.Add(data);
                    data = new string[] { LocRM.GetString("String65"), "2", undoRedoNextRowID.ToString() };
                    undoRedoNextRowID++;
                    rows.Add(data);
                    data = new string[] { LocRM.GetString("String66"), "3", undoRedoNextRowID.ToString() };
                    undoRedoNextRowID++;
                    rows.Add(data);
                    data = new string[] { LocRM.GetString("String67"), "4", undoRedoNextRowID.ToString() };
                    undoRedoNextRowID++;
                    rows.Add(data);
                    data = new string[] { LocRM.GetString("String68"), "5", undoRedoNextRowID.ToString() };
                    undoRedoNextRowID++;
                    rows.Add(data);
                    data = new string[] { " ", "6", undoRedoNextRowID.ToString() };
                    undoRedoNextRowID++;
                    rows.Add(data);
                    data = new string[] { " ", "8", undoRedoNextRowID.ToString() };
                    undoRedoNextRowID++;
                    rows.Add(data);
                    data = new string[] { " ", "9", undoRedoNextRowID.ToString() };
                    undoRedoNextRowID++;
                    rows.Add(data);
                    data = new string[] { " ", "10", undoRedoNextRowID.ToString() };
                    undoRedoNextRowID++;
                    rows.Add(data);
                    break;
                case Form1.SubformType.SUBJECT:
                    data = new string[] { LocRM.GetString("String37"), "2", "", undoRedoNextRowID.ToString() };
                    undoRedoNextRowID++;
                    rows.Add(data);
                    data = new string[] { LocRM.GetString("String27"), "2", "2", undoRedoNextRowID.ToString() };
                    undoRedoNextRowID++;
                    rows.Add(data);
                    data = new string[] { LocRM.GetString("String28"), "2", "", undoRedoNextRowID.ToString() };
                    undoRedoNextRowID++;
                    rows.Add(data);
                    break;
                case Form1.SubformType.CLASSES:
                    List<string> classes = new List<string> { "1a", "1b", "2a", "2b" };
                    dataList = new List<string>();
                    foreach (string example in classes)
                    {
                        dataList.Clear();
                        dataList.Add(example);
                        for (int i = 1; i < dataGridView1.ColumnCount-1; i++)
                        {
                            dataList.Add("2");
                        }
                        dataList.Add(undoRedoNextRowID.ToString());
                        undoRedoNextRowID++;
                        rows.Add(dataList.ToArray());
                    }
                    break;
                case Form1.SubformType.TEACHER:
                    List<string> teacher = new List<string> { "Mr. Minit", "Mary Poppins" };
                    dataList = new List<string>();
                    foreach (string example in teacher)
                    {
                        dataList.Clear();
                        dataList.Add(example);
                        for (int i = 1; i < dataGridView1.ColumnCount - 1; i++)
                        {
                            dataList.Add("26");
                        }
                        dataList.Add(undoRedoNextRowID.ToString());
                        undoRedoNextRowID++;
                        rows.Add(dataList.ToArray());
                    }
                    break;
                default:
                    break;
            }

            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                UserDeletingRow(row.Index);
            }
            dataGridView1.Rows.Clear();
            int rowIndex ;
            foreach (string[] row in rows)
            {
                rowIndex = dataGridView1.Rows.Add();
                dataGridView1.Rows[rowIndex].Cells[dataGridView1.ColumnCount - 1].Value = row[dataGridView1.ColumnCount - 1];
                UserAddedRow(rowIndex);
                foreach (DataGridViewCell cell in dataGridView1.Rows[rowIndex].Cells)
                {
                    CellBeginEdit(rowIndex, cell.ColumnIndex);
                    cell.Value = row[cell.ColumnIndex];
                    CellEndEdit(rowIndex, cell.ColumnIndex);
                }
            }
        }

        private bool Checkdata()
        {
            bool errorFound = false;
            textBox1.Clear();

            DataGridViewCell cell;
            int rowIndex;
            int colIndex;
            int colTypesIndex = 0;
            for (colIndex = 0; colIndex < dataGridView1.Columns.Count; colIndex++)
            {
                List<AllowedValues> allowed = null;
                List<AllowedValuesConstraints> constraints = null;
                bool isAllowed;
                bool isNotAllowed;
                bool allowedError = false;
                bool constraintError = false;
                if (!dataGridView1.Columns[colIndex].ReadOnly)
                {
                    allowed = checkAllowedValues[colTypesIndex];
                    constraints = checkAllowedValuesConstraints[colTypesIndex];
                    Dictionary<string, int> allValues = new Dictionary<string, int>();
                    for (rowIndex = 0; rowIndex < dataGridView1.Rows.Count; rowIndex++)
                    {
                        if (! dataGridView1.Rows[rowIndex].IsNewRow && dataGridView1.Rows[rowIndex] != null)
                        {
                            string value;
                            cell = dataGridView1.Rows[rowIndex].Cells[colIndex];
                            isAllowed = false;
                            isNotAllowed = false;
                            if (allowed.Count == 0)
                            {
                                isAllowed = true;
                            }
                            foreach (AllowedValues a in allowed)
                            {
                                switch (a)
                                {
                                    case AllowedValues.EMPTY:
                                        if (cell == null || cell.Value == null || cell.Value.ToString().Trim() == "")
                                        {
                                            isAllowed = true;
                                        }
                                        break;
                                    case AllowedValues.INT:
                                        if (cell != null && cell.Value != null)
                                        {
                                            value = cell.Value.ToString().Trim();
                                            string pattern = @"^\d+$";
                                            Regex rgx = new Regex(pattern);
                                            if (rgx.IsMatch(value))
                                            {
                                                isAllowed = true;
                                            }
                                        }
                                        break;
                                    case AllowedValues.ZERO:
                                        if (cell != null && cell.Value != null)
                                        {
                                            value = cell.Value.ToString().Trim();
                                            if (value == "0")
                                            {
                                                isAllowed = true;
                                            }
                                        }
                                        break;
                                    default:
                                        break;
                                }
                            }
                            foreach(AllowedValuesConstraints c in constraints){
                                switch (c)
                                {
                                    case AllowedValuesConstraints.NOTEMPTY:
                                        if (cell == null || cell.Value == null || cell.Value.ToString().Trim() == "")
                                        {
                                            isNotAllowed = true;
                                        }
                                        break;
                                    case AllowedValuesConstraints.POSITIVE:
                                        if (cell != null && cell.Value != null)
                                        {
                                            value = cell.Value.ToString().Trim();
                                            string pattern = @"^$|^\d+$";
                                            Regex rgx = new Regex(pattern);
                                            if (!rgx.IsMatch(value))
                                            {
                                                isNotAllowed = true;
                                            }
                                        }
                                        break;
                                    case AllowedValuesConstraints.UNIQUE:
                                        if (cell != null && cell.Value != null && cell.Value.ToString().Trim() != "")
                                        {
                                            value = cell.Value.ToString().Trim();
                                            if (allValues.ContainsKey(value))
                                            {
                                                StartCellBackgroundEffect(cell);
                                                DataGridViewCell otherCell = dataGridView1.Rows[allValues[value]].Cells[colIndex];
                                                StartCellBackgroundEffect(otherCell);
                                                constraintError = true;
                                                errorFound = true;
                                            }
                                            else
                                            {
                                                allValues[value] = rowIndex;
                                            }
                                        }
                                        break;
                                    default:
                                        break;
                                }
                            }
                            if (!isAllowed)
                            {
                                allowedError = true;
                                StartCellBackgroundEffect(cell);
                                errorFound = true;
                            }
                            if (isNotAllowed)
                            {
                                constraintError = true;
                                StartCellBackgroundEffect(cell);
                                errorFound = true;
                            }
                        }
                    }
                    colTypesIndex++;
                }
                if (allowedError|| constraintError)
                {
                    splitContainer3.Panel2Collapsed = false;
                    textBox1.AppendText(
                        LocRM.GetString("String45") + " " + (colIndex + 1) + " '" +
                        dataGridView1.Columns[colIndex].HeaderText + "' ");
                }
                if (allowedError)
                {
                    textBox1.AppendText(LocRM.GetString("String43") + ": ");
                    List<string> types = new List<string>();
                    foreach (AllowedValues a in allowed)
                    {
                        types.Add(allowedTypesInfo[(int)a]);
                    }
                    textBox1.AppendText(String.Join(", ",types.ToArray()));
                    textBox1.AppendText(". ");
                }
                if (constraintError)
                {
                    textBox1.AppendText(LocRM.GetString("String44") + ": ");
                    List<string> types = new List<string>();
                    foreach (AllowedValues c in constraints)
                    {
                        types.Add(constraintTypesInfo[(int)c]);
                    }
                    textBox1.AppendText(String.Join(", ", types.ToArray()));
                    textBox1.AppendText(". ");
                }
                if (allowedError || constraintError)
                {
                    textBox1.AppendText(Environment.NewLine);
                }
            }
            if (!errorFound)
            {
                //special checks here
                switch (mySubformType)
                {
                    case Form1.SubformType.DAYSHOURS:
                        break;
                    case Form1.SubformType.SUBJECT:
                        break;
                    case Form1.SubformType.CLASSES:
                        errorFound = errorFound || CheckClassHoursPerWeekPossible();
                        break;
                    case Form1.SubformType.TEACHER:
                        errorFound = errorFound || CheckTeacherMaxHoursToSubjectHours();
                        errorFound = errorFound || CheckTeacherHoursPerWeekEnough();
                        errorFound = errorFound || CheckTeacherHoursPerWeekAndSubjectEnough();
                        break;
                    case Form1.SubformType.TEACHERTOCLASS:
                        errorFound = errorFound || CheckTeacherToClassMaxSubjectHours();
                        break;
                    case Form1.SubformType.CLASSTOSUBJECTPRESET:
                        errorFound = errorFound || CheckClass2SubjectIDs();
                        break;
                    default:
                        break;
                }
            }
            if (!errorFound)
            {
                splitContainer3.Panel2Collapsed = true;
                //button1.Enabled = true;
                button1.BackColor = Color.Green;
                button1.ForeColor = Color.White;
            }
            return errorFound;
        }

        private bool CheckClass2SubjectIDs()
        {
            //check, that for each unique ID all classes are different and all subjects are equal and times are equal or empty
            bool re = false;
            string id;
            string className;
            string subject;
            string day;
            string hour;
            Dictionary<string, int> idsCount = new Dictionary<string, int>();
            Dictionary<string, List<string>> id2class = new Dictionary<string, List<string>>();
            Dictionary<string, List<int>> idsLines = new Dictionary<string, List<int>>();
            Dictionary<string, List<string>> id2subject = new Dictionary<string, List<string>>();
            Dictionary<string, List<string>> id2day = new Dictionary<string, List<string>>();
            Dictionary<string, List<string>> id2hour = new Dictionary<string, List<string>>();
            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                if (!row.IsNewRow)
                {
                    id = row.Cells[1].Value.ToString();
                    className = row.Cells[0].Value.ToString();
                    subject = row.Cells[2].Value.ToString();
                    if (row.Cells[3].Value != null)
                    {
                        day = row.Cells[3].Value.ToString();
                    }
                    else
                    {
                        day = "";
                    }
                    if (row.Cells[4].Value != null)
                    {
                        hour = row.Cells[4].Value.ToString();
                    }
                    else
                    {
                        hour = "";
                    }
                    if (idsCount.ContainsKey(id))
                    {
                        idsCount[id] = idsCount[id] + 1;
                        List<string> classes = id2class[id];
                        if (!classes.Contains(className))
                        {
                            classes.Add(className);
                            id2class[id] = classes;
                        }
                        List<int> lines = idsLines[id];
                        lines.Add(row.Index);
                        idsLines[id] = lines;
                        List<string> subjects = id2subject[id];
                        if (!subjects.Contains(subject))
                        {
                            subjects.Add(subject);
                            id2subject[id] = subjects;
                        }
                        List<string> days = id2day[id];
                        if (!days.Contains(day))
                        {
                            days.Add(day);
                            id2day[id] = days;
                        }
                        List<string> hours = id2hour[id];
                        if (!hours.Contains(hour))
                        {
                            hours.Add(hour);
                            id2hour[id] = hours;
                        }
                    }
                    else
                    {
                        idsCount[id] = 1;
                        List<string> classes = new List<string>
                        {
                            className
                        };
                        id2class[id] = classes;
                        List<int> lines = new List<int>
                        {
                            row.Index
                        };
                        idsLines[id] = lines;
                        List<string> subjects = new List<string>
                        {
                            subject
                        };
                        id2subject[id] = subjects;
                        List<string> days = new List<string> {
                            day
                        };
                        id2day[id] = days;
                        List<string> hours = new List<string>() {
                            hour
                        };
                        id2hour[id] = hours;
                    }
                }
            }
            foreach (string currentID in idsCount.Keys)
            {
                List<string> classes = id2class[currentID];
                if (classes.Count != idsCount[currentID])
                {
                    splitContainer3.Panel2Collapsed = false;
                    textBox1.AppendText(LocRM.GetString("String126"));
                    textBox1.AppendText(Environment.NewLine);
                    foreach (DataGridViewRow row in dataGridView1.Rows)
                    {
                        if (!row.IsNewRow)
                        {
                            foreach (DataGridViewCell cell in row.Cells)
                            {
                                id = row.Cells[1].Value.ToString();
                                if (idsLines[currentID].Contains(row.Index) && cell.ColumnIndex <= 1)
                                {
                                    if (cell.ColumnIndex == 0)
                                    {
                                        ((DataGridViewComboBoxCell)cell).DisplayStyle = DataGridViewComboBoxDisplayStyle.Nothing;
                                        StartCellBackgroundEffect(cell, true);
                                    }
                                    else
                                    {
                                        StartCellBackgroundEffect(cell);
                                    }
                                }
                            }
                        }
                    }
                    re = true;
                }
                if (re) break;
            }
            if (!re)
            {
                foreach (string currentID in idsCount.Keys)
                {
                    List<string> subjects = id2subject[currentID];
                    if (subjects.Count != 1)
                    {
                        splitContainer3.Panel2Collapsed = false;
                        textBox1.AppendText(LocRM.GetString("String127"));
                        textBox1.AppendText(Environment.NewLine);
                        foreach (DataGridViewRow row in dataGridView1.Rows)
                        {
                            if (!row.IsNewRow)
                            {
                                foreach (DataGridViewCell cell in row.Cells)
                                {
                                    id = row.Cells[1].Value.ToString();
                                    if (idsLines[currentID].Contains(row.Index) && cell.ColumnIndex >= 1 && cell.ColumnIndex <= 2)
                                    {
                                        if (cell.ColumnIndex == 2)
                                        {
                                            ((DataGridViewComboBoxCell)cell).DisplayStyle = DataGridViewComboBoxDisplayStyle.Nothing;
                                            StartCellBackgroundEffect(cell, true);
                                        }
                                        else
                                        {
                                            StartCellBackgroundEffect(cell);
                                        }
                                    }
                                }
                            }
                        }
                        re = true;
                    }
                    if (re) break;
                }
            }
            if (!re)
            {
                foreach (string currentID in idsCount.Keys)
                {
                    List<string> days = id2day[currentID];
                    List<string> hours = id2hour[currentID];
                    if (
                        days.Count > 1 ||
                        hours.Count > 1 ||
                        (days[0] == "" && hours[0] != "") ||
                        (days[0] != "" && hours[0] == "") ||
                        false
                        )
                    {
                        splitContainer3.Panel2Collapsed = false;
                        textBox1.AppendText(LocRM.GetString("String128"));
                        textBox1.AppendText(Environment.NewLine);
                        foreach (DataGridViewRow row in dataGridView1.Rows)
                        {
                            if (!row.IsNewRow)
                            {
                                foreach (DataGridViewCell cell in row.Cells)
                                {
                                    id = row.Cells[1].Value.ToString();
                                    if (idsLines[currentID].Contains(row.Index) &&
                                        ((cell.ColumnIndex >= 3 && cell.ColumnIndex <= 4) || cell.ColumnIndex == 1)
                                        )
                                    {
                                        if (cell.ColumnIndex == 3 || cell.ColumnIndex == 4)
                                        {
                                            ((DataGridViewComboBoxCell)cell).DisplayStyle = DataGridViewComboBoxDisplayStyle.Nothing;
                                            StartCellBackgroundEffect(cell, true);
                                        }
                                        else
                                        {
                                            StartCellBackgroundEffect(cell);
                                        }
                                    }
                                }
                            }
                        }
                        re = true;
                    }
                    if (re) break;
                }
            }
            return re;
        }

        private bool CheckTeacherToClassMaxSubjectHours()
        {
            bool re = false;
            List<string[]> allClasses = Form1.projects.LoadData(myId, Form1.SubformType.CLASSES, false);
            List<string[]> allTeacher = Form1.projects.LoadData(myId, Form1.SubformType.TEACHER, false);        
            Dictionary<string, Dictionary<string, int>> teacherToSubjectHoursSum = new Dictionary<string, Dictionary<string, int>>();
            Dictionary<string, int> subjectHoursSum;
            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                if (!row.IsNewRow)
                {
                    string curClass = row.Cells[0].Value.ToString();
                    for (int colIndex = 1; colIndex < row.Cells.Count - 1; colIndex++)
                    {
                        string curSubject = dataGridView1.Columns[colIndex].HeaderText.ToString();
                        int classSubjectHours = 0;
                        for (int classColIndex = 0; classColIndex < allClasses[0].Length - 1; classColIndex++)
                        {
                            if (allClasses[0][classColIndex].ToString() == curSubject)
                            {
                                for (int classRowIndex = 1; classRowIndex < allClasses.Count; classRowIndex++)
                                {
                                    if (allClasses[classRowIndex][0] == curClass)
                                    {
                                        string value = allClasses[classRowIndex][classColIndex].Trim();
                                        string pattern = @"^\d+$";
                                        Regex rgx = new Regex(pattern);
                                        if (rgx.IsMatch(value))
                                        {
                                            classSubjectHours = Int32.Parse(value);
                                        }
                                    }
                                }
                            }
                        }
                        if( row.Cells[colIndex].Value != null && row.Cells[colIndex].Value.ToString() != "")
                        {
                            string teacher = row.Cells[colIndex].Value.ToString();
                            if (teacherToSubjectHoursSum.ContainsKey(teacher))
                            {
                                subjectHoursSum = teacherToSubjectHoursSum[teacher];
                                if (subjectHoursSum.ContainsKey(curSubject))
                                {
                                    subjectHoursSum[curSubject] += classSubjectHours;
                                }
                                else
                                {
                                    subjectHoursSum = new Dictionary<string, int>
                                    {
                                        [curSubject] = classSubjectHours
                                    };
                                }
                            }
                            else
                            {
                                subjectHoursSum = new Dictionary<string, int>
                                {
                                    [curSubject] = classSubjectHours
                                };
                                teacherToSubjectHoursSum[teacher] = subjectHoursSum;
                            }
                        }
                    }
                }
            }
            int teacherSubjectHours;
            int maxTeacherSubjectHours = 0;
            foreach (string teacher in teacherToSubjectHoursSum.Keys)
            {
                subjectHoursSum = teacherToSubjectHoursSum[teacher];
                foreach (string subject in subjectHoursSum.Keys)
                {
                    teacherSubjectHours = subjectHoursSum[subject];
                    for (int teacherColIndex = 2; teacherColIndex < allTeacher[0].Length - 1 && !re; teacherColIndex++)
                    {
                        if (allTeacher[0][teacherColIndex] == subject)
                        {
                            for (int teacherRowIndex = 1; teacherRowIndex < allTeacher.Count && ! re; teacherRowIndex++)
                            {
                                if (allTeacher[teacherRowIndex][0] == teacher)
                                {
                                    string value = allTeacher[teacherRowIndex][teacherColIndex].Trim();
                                    string pattern = @"^\d+$";
                                    Regex rgx = new Regex(pattern);
                                    if (rgx.IsMatch(value))
                                    {
                                        maxTeacherSubjectHours = Int32.Parse(value);
                                    }
                                    if (teacherSubjectHours > maxTeacherSubjectHours)
                                    {
                                        splitContainer3.Panel2Collapsed = false;
                                        textBox1.AppendText(string.Format(LocRM.GetString("String110"),
                                            teacherSubjectHours.ToString(), subject, teacher, maxTeacherSubjectHours.ToString()));
                                        textBox1.AppendText(Environment.NewLine);
                                        foreach (DataGridViewRow row in dataGridView1.Rows)
                                        {
                                            if (!row.IsNewRow)
                                            {
                                                foreach (DataGridViewCell cell in row.Cells)
                                                {
                                                    if (dataGridView1.Columns[cell.ColumnIndex].HeaderText == subject &&
                                                        cell.Value != null &&
                                                        cell.Value.ToString() == teacher &&
                                                        true
                                                        )
                                                    {
                                                        ((DataGridViewComboBoxCell)cell).DisplayStyle = DataGridViewComboBoxDisplayStyle.Nothing;
                                                        StartCellBackgroundEffect(cell, true);
                                                        //DataGridViewCellStyle red = dataGridView1.DefaultCellStyle.Clone();
                                                        //red.BackColor = Color.Red;
                                                        //cell.Style = red;

                                                    }
                                                }
                                            }
                                        }
                                        re = true;
                                    }
                                }
                            }
                        }
                    }
                    if( re ) { break; }
                }
                if (re) { break; }
            }
            return re;
        }

        private bool CheckTeacherMaxHoursToSubjectHours()
        {
            bool re = false;
            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                if (!row.IsNewRow)
                {
                    int maxHours = 0;
                    string value;
                    if (row != null && row.Cells[1] != null && row.Cells[1].Value != null)
                    {
                        value = row.Cells[1].Value.ToString().Trim();
                        string pattern = @"^\d+$";
                        Regex rgx = new Regex(pattern);
                        if (rgx.IsMatch(value))
                        {
                            maxHours = Int32.Parse(value);
                        }
                    }
                    for (int colIndex = 2; colIndex < dataGridView1.ColumnCount - 1; colIndex++)
                    {
                        if (!dataGridView1.Columns[colIndex].ReadOnly)
                        {
                            string subject = dataGridView1.Columns[colIndex].HeaderText.ToString();
                            int maxClassHours = 0;
                            if (row != null && row.Cells[colIndex] != null && row.Cells[colIndex].Value != null)
                            {
                                if (row.Cells[colIndex].Value.ToString().Trim().Length > 0)
                                {
                                    value = row.Cells[colIndex].Value.ToString().Trim();
                                    string pattern = @"^\d+$";
                                    Regex rgx = new Regex(pattern);
                                    if (rgx.IsMatch(value))
                                    {
                                        maxClassHours = Int32.Parse(value);
                                    }
                                }
                            }
                            if (maxClassHours > maxHours)
                            {
                                StartCellBackgroundEffect(row.Cells[0]);
                                StartCellBackgroundEffect(row.Cells[1]);
                                StartCellBackgroundEffect(row.Cells[colIndex]);
                                splitContainer3.Panel2Collapsed = false;
                                string teacherName = row.Cells[0].Value.ToString().Trim();
                                textBox1.AppendText(LocRM.GetString("String86") + " " + teacherName + ": ");
                                textBox1.AppendText(string.Format(LocRM.GetString("String101"), subject, maxClassHours.ToString(), maxHours.ToString()));
                                textBox1.AppendText(Environment.NewLine);
                                re = true;
                            }
                        }
                    }
                }
            }

            return re;
        }

        private bool CheckTeacherHoursPerWeekAndSubjectEnough()
        {
            bool re = false;
            List<string[]> classes = Form1.projects.LoadData(myId, Form1.SubformType.CLASSES, false);
            if (classes.Count > 1)
            {
                string[] classesSubjects = classes[0];
                for (int classesSubjectsIndex = 1; classesSubjectsIndex < classesSubjects.Length - 1; classesSubjectsIndex++)
                {
                    string subject = classesSubjects[classesSubjectsIndex];
                    int classesSubjectHours = 0;
                    for (int classesIndex = 1; classesIndex < classes.Count; classesIndex++)
                    {
                        string value = classes[classesIndex][classesSubjectsIndex].ToString().Trim();
                        string pattern = @"^\d+$";
                        Regex rgx = new Regex(pattern);
                        if (rgx.IsMatch(value))
                        {
                            classesSubjectHours += Int32.Parse(value);
                        }
                    }
                    int teacherSubjectHours = 0;
                    foreach (DataGridViewRow row in dataGridView1.Rows)
                    {
                        if (!row.IsNewRow)
                        {
                            string value = "";
                            if (row.Cells[classesSubjectsIndex + 1].Value != null)
                            {
                                value = row.Cells[classesSubjectsIndex + 1].Value.ToString().Trim();
                            }
                            string pattern = @"^\d+$";
                            Regex rgx = new Regex(pattern);
                            if (rgx.IsMatch(value))
                            {
                                teacherSubjectHours += Int32.Parse(value);
                            }
                        }
                    }
                    if (teacherSubjectHours < classesSubjectHours)
                    {
                        foreach (DataGridViewRow row in dataGridView1.Rows)
                        {
                            if (!row.IsNewRow)
                            {
                                StartCellBackgroundEffect(row.Cells[classesSubjectsIndex + 1]);
                            }
                        }
                        splitContainer3.Panel2Collapsed = false;
                        textBox1.AppendText(string.Format(LocRM.GetString("String109"), teacherSubjectHours.ToString(), subject.ToString(), classesSubjectHours.ToString()));
                        textBox1.AppendText(Environment.NewLine);
                        re = true;
                    }
                }
            }
            return re;
        }

        private bool CheckTeacherHoursPerWeekEnough()
        {
            bool re = false;
            List<string[]> classesHours;
            classesHours = Form1.projects.LoadData(myId, Form1.SubformType.CLASSES);
            int classHoursSum = 0;
            string value;
            foreach (string[] row in classesHours)
            {
                for (int colIndex = 1; colIndex < row.Length - 1; colIndex++)
                {
                    if (row[colIndex].Trim().Length > 0)
                    {
                        value = row[colIndex].Trim();
                        string pattern = @"^\d+$";
                        Regex rgx = new Regex(pattern);
                        if (rgx.IsMatch(value))
                        {
                            classHoursSum += Int32.Parse(value);
                        }
                    }
                }
            }
            int teacherHoursSum = 0;
            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                if (!row.IsNewRow)
                {
                    if (row != null && row.Cells[1] != null && row.Cells[1].Value != null)
                    {
                        value = row.Cells[1].Value.ToString().Trim();
                        string pattern = @"^\d+$";
                        Regex rgx = new Regex(pattern);
                        if (rgx.IsMatch(value))
                        {
                            teacherHoursSum += Int32.Parse(value);
                        }
                    }
                }
            }
            if (classHoursSum > teacherHoursSum)
            {
                foreach (DataGridViewRow row in dataGridView1.Rows)
                {
                    if (!row.IsNewRow)
                    {
                        StartCellBackgroundEffect(row.Cells[1]);
                    }
                }
                splitContainer3.Panel2Collapsed = false;
                textBox1.AppendText(string.Format(LocRM.GetString("String100"), teacherHoursSum.ToString(), classHoursSum.ToString()));
                textBox1.AppendText(Environment.NewLine);
                re = true;
            }
            return re;
        }

        private bool CheckClassHoursPerWeekPossible()
        {
            bool re = false;

            List<string[]> daysHours;
            daysHours = Form1.projects.LoadData(myId, Form1.SubformType.DAYSHOURS);
            int dayCount = 0;
            int hourCount = 0;
            foreach ( string[] lines in daysHours)
            {
                if( lines[0].Trim().Length > 0  )
                {
                    dayCount++;
                }
                if (lines[1].Trim().Length > 0)
                {
                    hourCount++;
                }
            }
            int hoursPerWeek = dayCount * hourCount;
            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                if (!row.IsNewRow)
                {
                    int classHoursSum = 0;
                    for (int colIndex = 1; colIndex < dataGridView1.ColumnCount - 1; colIndex++)
                    {
                        string value;
                        if ( ! row.Cells[colIndex].ReadOnly && row != null && row.Cells[colIndex] != null && row.Cells[colIndex].Value != null)
                        {
                            value = row.Cells[colIndex].Value.ToString().Trim();
                            string pattern = @"^\d+$";
                            Regex rgx = new Regex(pattern);
                            if (rgx.IsMatch(value))
                            {
                                classHoursSum += Int32.Parse(value);
                            }
                        }
                    }
                    if (classHoursSum > hoursPerWeek)
                    {
                        foreach (DataGridViewCell cell in row.Cells)
                        {
                            StartCellBackgroundEffect(cell);
                        }
                        splitContainer3.Panel2Collapsed = false;
                        string className = row.Cells[0].Value.ToString().Trim();
                        textBox1.AppendText(LocRM.GetString("String97")+" "+className+" :");
                        textBox1.AppendText(string.Format(LocRM.GetString("String98"), classHoursSum.ToString(), hoursPerWeek.ToString()));
                        textBox1.AppendText(Environment.NewLine);
                        re = true;
                    }
                }
            }
            return re;
        }

        private void Button3_Click(object sender, EventArgs e)
        {
            if (dataNeedsSave)
            {
                if (ShowDataNeedsSaveDialog())
                {
                    dataNeedsSave = false;
                    this.Close();
                }
            }
            else
            {
                dataNeedsSave = false;
                this.Close();
            }
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            if (dataNeedsSave || readonlyColumnsExist)
            {
                DialogResult result = System.Windows.Forms.DialogResult.Yes;
                if (button1.BackColor == Color.Red)
                {
                    result = MessageBox.Show(LocRM.GetString("String85"), LocRM.GetString("String42"), MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                }
                if (result == System.Windows.Forms.DialogResult.Yes && readonlyColumnsExist)
                {
                    result = MessageBox.Show(LocRM.GetString("String57"), LocRM.GetString("String42"), MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                }
                if (result == System.Windows.Forms.DialogResult.Yes)
                {
                    if (
                        mySubformType == Form1.SubformType.TEACHERTOCLASS ||
                        mySubformType == Form1.SubformType.CONFIG ||
                        false
                        )
                    {
                        dataGridView1.Columns[0].ReadOnly = false;
                    }
                    Form1.projects.SaveData(myId, mySubformType, dataGridView1);
                    dataNeedsSave = false;
                    this.Close();
                }
            }
            else
            {
                this.Close();
            }
        }

        private bool ShowDataNeedsSaveDialog()
        {
            bool rt = false;

            DialogResult result;
            result = MessageBox.Show(LocRM.GetString("String41"), LocRM.GetString("String42"), MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == System.Windows.Forms.DialogResult.Yes)
            {
                rt = true;
            }
            return rt;
        }

        private void CellBeginEdit(int rowIndex, int colIndex)
        {
            undoRedoBeginEdit.IncrementID();
            undoRedoBeginEdit.type = UndoRedoEntryTypes.VALUE;
            if (dataGridView1.Rows[rowIndex].Cells[dataGridView1.ColumnCount - 1].Value == null)
            {
                dataGridView1.Rows[rowIndex].Cells[dataGridView1.ColumnCount - 1].Value = undoRedoNextRowID.ToString();
                undoRedoNextRowID++;
            }
            undoRedoBeginEdit.rowID = dataGridView1.Rows[rowIndex].Cells[dataGridView1.ColumnCount - 1].Value.ToString();
            undoRedoBeginEdit.col = colIndex;
            if (
                dataGridView1.Rows[rowIndex].Cells[undoRedoBeginEdit.col] != null &&
                dataGridView1.Rows[rowIndex].Cells[undoRedoBeginEdit.col].Value != null
                )
            {
                undoRedoBeginEdit.value = dataGridView1.Rows[rowIndex].Cells[undoRedoBeginEdit.col].Value.ToString();
            }
            else
            {
                undoRedoBeginEdit.value = "";
            }
        }

        private void DataGridView1_CellBeginEdit(object sender, DataGridViewCellCancelEventArgs e)
        {
            cellCurrentlyInEdit = true;
            int rowIndex = e.RowIndex;
            int colIndex = e.ColumnIndex;
            CellBeginEdit(rowIndex, colIndex);
        }

        private void CellEndEdit(int rowIndex, int colIndex)
        {
            skipCellEndEdit = true;
            string rowID = dataGridView1.Rows[rowIndex].Cells[dataGridView1.ColumnCount - 1].Value.ToString();
            bool emptyRow = true;
            foreach (DataGridViewCell cell in dataGridView1.Rows[rowIndex].Cells)
            {
                if (
                    cell != null &&
                    cell.ColumnIndex < dataGridView1.Columns.Count - 1 &&
                    cell.Value != null &&
                    cell.Value.ToString() != "" &&
                    true)
                {
                    emptyRow = false;
                }
            }

            bool addUndoRedoLineDelete = false;
            if (emptyRow)
            {
                DataGridViewRow tmprow = dataGridView1.Rows[rowIndex];
                if (tmprow != null && !tmprow.IsNewRow)
                {
                    if (undo.Count > 0)
                    {
                        UndoRedoEntry lineInsert = undo.Peek().Clone();
                        if (
                            lineInsert.type == UndoRedoEntryTypes.LINE_INSERT &&
                            lineInsert.rowID == rowID &&
                            true
                            )
                        {
                            undo.Pop();
                        }
                        else
                        {
                            addUndoRedoLineDelete = true;
                        }
                    }
                    else
                    {
                        addUndoRedoLineDelete = true;
                    }
                    DataGridViewElementStates state = dataGridView1.Rows.GetRowState(rowIndex);
                    if (state != DataGridViewElementStates.None && !dataGridView1.Rows[rowIndex].IsNewRow)
                    {
                        this.BeginInvoke(new MethodInvoker(() => { dataGridView1.Rows.RemoveAt(rowIndex); }));
                    }
                }
            }

            if (
                !emptyRow &&
                undoRedoBeginEdit.type == UndoRedoEntryTypes.VALUE &&
                undoRedoBeginEdit.rowID == rowID &&
                undoRedoBeginEdit.col == colIndex &&
                true
                )
            {
                if (
                    (
                        dataGridView1.Rows[rowIndex].Cells[undoRedoBeginEdit.col].Value == null &&
                        undoRedoBeginEdit.value != "" &&
                        true
                    ) ||
                    (
                        dataGridView1.Rows[rowIndex].Cells[undoRedoBeginEdit.col].Value != null &&
                        undoRedoBeginEdit.value != dataGridView1.Rows[rowIndex].Cells[undoRedoBeginEdit.col].Value.ToString() &&
                        true
                    ) ||
                    false
                    )
                {
                    undo.Push(undoRedoBeginEdit.Clone());
                    undoRedoBeginEdit.IncrementID();
                    redo.Clear();
                }
            }

            if (addUndoRedoLineDelete)
            {
                undoRedoBeginEdit.type = UndoRedoEntryTypes.LINE_DELETE;
                undoRedoBeginEdit.rowID = rowID;
                undo.Push(undoRedoBeginEdit.Clone());
                undoRedoBeginEdit.IncrementID();
            }
            ToggleSaveButton();
            skipCellEndEdit = false;

            cellCurrentlyInEdit = false;
        }

        private void DataGridView1_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            if (!skipCellEndEdit)
            {
                int rowIndex = e.RowIndex;
                int colIndex = e.ColumnIndex;
                CellEndEdit(rowIndex, colIndex);
            }
        }

        private int GetRowIndexFromRowID(string rowID)
        {
            int rowIndex = dataGridView1.RowCount;
            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                if (
                    row != null &&
                    row.Cells[dataGridView1.ColumnCount - 1] != null &&
                    row.Cells[dataGridView1.ColumnCount - 1].Value != null &&
                    true
                    )
                {
                    if (row.Cells[dataGridView1.ColumnCount - 1].Value.ToString() == rowID)
                    {
                        rowIndex = row.Index;
                    }
                }
            }
            return rowIndex;
        }

        private void KeyboardKeyDown(string key, bool control)
        {
            int rowIndex;
            if (true
                && (key == "Delete" || key == "Back") && control == false
                )
            {
                if (!cellCurrentlyInEdit)
                {
                    if (dataGridView1.SelectedRows.Count == 0)
                    {
                        DataGridViewCell currentCell = dataGridView1.CurrentCell;
                        if (currentCell != null && currentCell.Value != null && !currentCell.ReadOnly)
                        {
                            rowIndex = currentCell.RowIndex;
                            int colIndex = currentCell.ColumnIndex;
                            CellBeginEdit(rowIndex, colIndex);

                            currentCell.Value = "";

                            CellEndEdit(rowIndex, colIndex);
                        }
                    }
                    else
                    {
                        foreach (DataGridViewRow row in dataGridView1.SelectedRows)
                        {
                            if (!row.IsNewRow)
                            {
                                UserDeletingRow(row.Index);
                                dataGridView1.Rows.RemoveAt(row.Index);
                            }
                        }
                    }
                }
            }
            if (true
                && key == "Z" && control == true
                )
            {
                if (undo.Count > 0)
                {
                    undoRedoBeginEdit = undo.Peek().Clone();
                    rowIndex = GetRowIndexFromRowID(undoRedoBeginEdit.rowID);
                    if (undoRedoBeginEdit.type == UndoRedoEntryTypes.VALUE)
                    {
                        if (
                            dataGridView1.Rows[rowIndex] != null &&
                            dataGridView1.Rows[rowIndex].Cells[undoRedoBeginEdit.col] != null &&
                            true
                            )
                        {
                            if (dataGridView1.Rows[rowIndex].Cells[undoRedoBeginEdit.col].Value != null)
                            {
                                undoRedoBeginEdit.value = dataGridView1.Rows[rowIndex].Cells[undoRedoBeginEdit.col].Value.ToString();
                            }
                            else
                            {
                                undoRedoBeginEdit.value = "";
                            }
                            redo.Push(undoRedoBeginEdit.Clone());
                            undoRedoBeginEdit = undo.Pop().Clone();
                            dataGridView1.Rows[rowIndex].Cells[undoRedoBeginEdit.col].Value = undoRedoBeginEdit.value;

                            StartCellBackgroundEffect(dataGridView1.Rows[rowIndex].Cells[undoRedoBeginEdit.col]);
                        }
                    }
                    else if (undoRedoBeginEdit.type == UndoRedoEntryTypes.LINE_DELETE)
                    {
                        UndoRedoEntry tmp = undoRedoBeginEdit.Clone();
                        int id = undoRedoBeginEdit.id;
                        string[] values = new string[dataGridView1.ColumnCount];
                        if (tmp.col >= 0)
                        {
                            values[tmp.col] = tmp.value;
                            values[dataGridView1.ColumnCount - 1] = tmp.rowID;
                        }
                        undo.Pop();
                        if (undo.Count > 0)
                        {
                            undoRedoBeginEdit = undo.Peek().Clone();
                            while (id == undoRedoBeginEdit.id)
                            {
                                if (undoRedoBeginEdit.type == UndoRedoEntryTypes.VALUE)
                                {
                                    values[undoRedoBeginEdit.col] = undoRedoBeginEdit.value;
                                    redo.Push(undoRedoBeginEdit.Clone());
                                    undo.Pop();
                                    if (undo.Count > 0)
                                    {
                                        undoRedoBeginEdit = undo.Peek().Clone();
                                    }
                                    else
                                    {
                                        id = -1;
                                    }
                                }
                                else
                                {
                                    id = -1;
                                }
                            }
                        }
                        redo.Push(tmp);
                        if (rowIndex >= dataGridView1.RowCount)
                        {
                            rowIndex = dataGridView1.Rows.Add(values);
                            foreach (DataGridViewCell cell in dataGridView1.Rows[rowIndex].Cells)
                            {
                                StartCellBackgroundEffect(cell);
                            }
                        }
                        else
                        {
                            dataGridView1.Rows.Insert(rowIndex, values);
                            foreach (DataGridViewCell cell in dataGridView1.Rows[rowIndex].Cells)
                            {
                                StartCellBackgroundEffect(cell);
                            }
                        }
                    }
                    else if (undoRedoBeginEdit.type == UndoRedoEntryTypes.LINE_INSERT)
                    {
                        DataGridViewRow row = dataGridView1.Rows[rowIndex];
                        if (row != null && !row.IsNewRow)
                        {
                            skipCellEndEdit = true;
                            dataGridView1.Rows.Remove(row);
                            skipCellEndEdit = false;
                            redo.Push(undoRedoBeginEdit.Clone());
                        }
                        undo.Pop();
                    }
                    else
                    {
                        undo.Pop();
                    }
                    if (undo.Count > 0)
                    {
                        undoRedoBeginEdit = undo.Peek().Clone();
                        if (undoRedoBeginEdit.type == UndoRedoEntryTypes.LINE_INSERT)
                        {
                            rowIndex = GetRowIndexFromRowID(undoRedoBeginEdit.rowID);
                            redo.Push(undoRedoBeginEdit.Clone());
                            undo.Pop();
                            dataGridView1.Rows.RemoveAt(rowIndex);
                        }
                    }
                }
                ToggleSaveButton();
            }
            if (true
                && key == "Y" && control == true
                )
            {
                if (redo.Count > 0)
                {
                    undoRedoBeginEdit = redo.Peek().Clone();
                    rowIndex = GetRowIndexFromRowID(undoRedoBeginEdit.rowID);
                    if (undoRedoBeginEdit.type == UndoRedoEntryTypes.VALUE)
                    {
                        if (
                            dataGridView1.Rows[rowIndex] != null &&
                            dataGridView1.Rows[rowIndex].Cells[undoRedoBeginEdit.col] != null &&
                            true
                            )
                        {
                            if (dataGridView1.Rows[rowIndex].Cells[undoRedoBeginEdit.col].Value != null)
                            {
                                undoRedoBeginEdit.value = dataGridView1.Rows[rowIndex].Cells[undoRedoBeginEdit.col].Value.ToString();
                            }
                            else
                            {
                                undoRedoBeginEdit.value = "";
                            }
                            undo.Push(undoRedoBeginEdit.Clone());
                            undoRedoBeginEdit = redo.Pop().Clone();
                            dataGridView1.Rows[rowIndex].Cells[undoRedoBeginEdit.col].Value = undoRedoBeginEdit.value;

                            StartCellBackgroundEffect(dataGridView1.Rows[rowIndex].Cells[undoRedoBeginEdit.col]);
                        }
                    }
                    else if (undoRedoBeginEdit.type == UndoRedoEntryTypes.LINE_DELETE)
                    {
                        UndoRedoEntry tmp = undoRedoBeginEdit.Clone();
                        int id = undoRedoBeginEdit.id;
                        redo.Pop();
                        if (redo.Count > 0)
                        {
                            undoRedoBeginEdit = redo.Peek().Clone();
                            while (id == undoRedoBeginEdit.id)
                            {
                                undo.Push(undoRedoBeginEdit.Clone());
                                redo.Pop();
                                if (redo.Count > 0)
                                {
                                    undoRedoBeginEdit = redo.Peek().Clone();
                                }
                                else
                                {
                                    id = -1;
                                }
                            }
                        }
                        if (rowIndex >= 0 && rowIndex < dataGridView1.Rows.Count && !dataGridView1.Rows[rowIndex].IsNewRow)
                        {
                            dataGridView1.Rows.RemoveAt(rowIndex);
                        }
                        undo.Push(tmp);
                    }
                    else if (undoRedoBeginEdit.type == UndoRedoEntryTypes.LINE_INSERT)
                    {
                        if (rowIndex >= dataGridView1.RowCount)
                        {
                            rowIndex = dataGridView1.Rows.Add();
                        }
                        else
                        {
                            dataGridView1.Rows.Insert(rowIndex);
                        }
                        dataGridView1.Rows[rowIndex].Cells[dataGridView1.ColumnCount - 1].Value = undoRedoBeginEdit.rowID;
                        undo.Push(undoRedoBeginEdit.Clone());
                        redo.Pop();
                        if (redo.Count > 0)
                        {
                            undoRedoBeginEdit = redo.Peek().Clone();
                            if (undoRedoBeginEdit.type == UndoRedoEntryTypes.VALUE)
                            {
                                if (
                                    dataGridView1.Rows[rowIndex] != null &&
                                    dataGridView1.Rows[rowIndex].Cells[undoRedoBeginEdit.col] != null &&
                                    true
                                    )
                                {
                                    undoRedoBeginEdit.value = "";
                                    undo.Push(undoRedoBeginEdit.Clone());
                                    undoRedoBeginEdit = redo.Pop().Clone();
                                    rowIndex = GetRowIndexFromRowID(undoRedoBeginEdit.rowID);
                                    dataGridView1.Rows[rowIndex].Cells[undoRedoBeginEdit.col].Value = undoRedoBeginEdit.value;

                                    StartCellBackgroundEffect(dataGridView1.Rows[rowIndex].Cells[undoRedoBeginEdit.col]);
                                }
                            }
                        }
                    }
                    else
                    {
                        redo.Pop();
                    }
                }
                ToggleSaveButton();
            }
        }

        private void DataGridView1_KeyDown(object sender, KeyEventArgs e)
        {
            string key = e.KeyCode.ToString();
            if (!e.Handled && key == "Z" && e.Control == true)
            {
                KeyboardKeyDown(key, e.Control);
                e.Handled = true;
            }
            if (!e.Handled && key == "Y" && e.Control == true)
            {
                KeyboardKeyDown(key, e.Control);
                e.Handled = true;
            }
            if (!e.Handled && ( key == "Delete" || key == "Back" ) && e.Control == false)
            {
                KeyboardKeyDown(key, e.Control);
                e.Handled = true;
            }
        }

        private void Form2_KeyDown(object sender, KeyEventArgs e)
        {
            string key = e.KeyCode.ToString();
            if (!e.Handled && key == "Z" && e.Control == true)
            {
                KeyboardKeyDown(key, e.Control);
                e.Handled = true;
            }
            if (!e.Handled && key == "Y" && e.Control == true)
            {
                KeyboardKeyDown(key, e.Control);
                e.Handled = true;
            }
            if (!e.Handled && (key == "Delete" || key == "Back") && e.Control == false)
            {
                KeyboardKeyDown(key, e.Control);
                e.Handled = true;
                if (key == "Delete" && cellCurrentlyInEdit)
                {
                    e.Handled = false;
                }
            }
        }

        private void WebBrowser1_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            string key = e.KeyCode.ToString();
            if (key == "Z" && e.Control == true)
            {
                KeyboardKeyDown(key, e.Control);
            }
            if (key == "Y" && e.Control == true)
            {
                KeyboardKeyDown(key, e.Control);
            }
            if ( (key == "Delete" || key == "Back") && e.Control == false)
            {
                KeyboardKeyDown(key, e.Control);
            }
        }

        private void DataGridView1_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            ToggleSaveButton();
        }

        private void DataGridView1_UserDeletedRow(object sender, DataGridViewRowEventArgs e)
        {
            ToggleSaveButton();
        }

        private void ToggleSaveButton( bool forceSave = false )
        {
            if (undo.Count == 0)
            {
                dataNeedsSave = false;
            }
            if (! forceSave && undo.Count == 0 && !readonlyColumnsExist && !forceSaveButton)
            {
                button1.Text = LocRM.GetString("String6");
                //button1.Enabled = true;
                button1.BackColor = Color.Green;
                button1.ForeColor = Color.White;
                button3.Enabled = false;
            }
            else
            {
                button1.Text = LocRM.GetString("String22");
                //button1.Enabled = false;
                button1.BackColor = Color.Red;
                //button1.ForeColor = Color.Black;
                button3.Enabled = true;
                dataNeedsSave = true;
            }

            toolStripStatusLabel1.Text = "Undo: " + undo.Count.ToString();
            toolStripStatusLabel2.Text = "Redo: " + redo.Count.ToString();
        }

        private void StartCellBackgroundEffect(DataGridViewCell cell, bool cellIsComboBox = false)
        {
            double timerIntervall = 50.0;
            double effectDurationInMilliSeconds = 500.0;
            int steps = (int)(effectDurationInMilliSeconds / timerIntervall);
            Color orange = Color.FromName("orange");
            Color defaultBackColor = dataGridView1.DefaultCellStyle.BackColor;
            DataGridViewCellStyle style = cell.Style;
            if (dataGridView1.Columns[cell.ColumnIndex].HasDefaultCellStyle)
            {                
                defaultBackColor = dataGridView1.Columns[cell.ColumnIndex].DefaultCellStyle.BackColor;
                style.BackColor = defaultBackColor;
            }
            if (!effectCells.Contains(style))
            {
                effectCells.Add(style);
                defaultBackground.Add(defaultBackColor);
                effectStep.Add(steps);
                if (cellIsComboBox)
                {
                    isComboBox.Add(cell);
                }
                else
                {
                    isComboBox.Add(null);
                }
                style.BackColor = orange;
            }
            if (aTimer == null)
            //if( aTimer.Enabled == false)
            {
                aTimer = new System.Timers.Timer(timerIntervall);
                //aTimer.Interval = timerIntervall;
                aTimer.Elapsed += OnTimedEvent;
                aTimer.AutoReset = true;
                //aTimer.Enabled = true;
                aTimer.Start();
            }
        }

        private void OnTimedEvent(Object source, ElapsedEventArgs e)
        {
            Color orange = Color.FromName("orange");
            List<int> toDelete = new List<int>();
            for (int index = 0; index < effectCells.Count; index++)
            {
                DataGridViewCellStyle style = effectCells[index];
                Color defaultBackColor = defaultBackground[index];
                int step = effectStep[index];
                if (step == 0)
                {
                    toDelete.Add(index);
                }
                else
                {
                    if (isComboBox[index] != null)
                    {
                        ((DataGridViewComboBoxCell)isComboBox[index]).DisplayStyle = DataGridViewComboBoxDisplayStyle.Nothing;
                    }
                    int newR = orange.R + (int)Math.Round(((double)defaultBackColor.R - (double)orange.R) / (double)step);
                    int newG = orange.G + (int)Math.Round(((double)defaultBackColor.G - (double)orange.G) / (double)step);
                    int newB = orange.B + (int)Math.Round(((double)defaultBackColor.B - (double)orange.B) / (double)step);
                    style.BackColor = Color.FromArgb(newR, newG, newB);
                    step--;
                    effectStep[index] = step;
                }
            }
            toDelete.Reverse();
            foreach (int index in toDelete)
            {
                if (isComboBox[index] != null)
                {
                    ((DataGridViewComboBoxCell)isComboBox[index]).DisplayStyle = DataGridViewComboBoxDisplayStyle.DropDownButton;
                }
                effectCells.RemoveAt(index);
                defaultBackground.RemoveAt(index);
                effectStep.RemoveAt(index);
                isComboBox.RemoveAt(index);
            }
            if (effectCells.Count == 0 && aTimer != null)
            //if (effectCells.Count == 0 && aTimer.Enabled == true)
            {
                aTimer.Stop();
                aTimer.Dispose();
                aTimer = null;
                //aTimer.Enabled = false;
            }
        }

        private void TextBox1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            ShowHidePanel2();
        }

        private void ShowHidePanel2()
        {
            if (splitContainer3.Panel1Collapsed)
            {
                splitContainer3.Panel2Collapsed = false;
            }
            else
            {
                splitContainer3.Panel2Collapsed = true;
            }
        }

        private void DataGridView1_ColumnHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (columnClickDoSort)
            {
                ToggleSaveButton(true);
            }
        }

        private void RandomOrderToolStripMenuItem_Click(object sender, EventArgs e)
        {
            dataGridView1.ClearSelection();

            Random rn = new Random((int)DateTime.Now.Ticks);

            List<int> oldIndices = Enumerable.Range(0, dataGridView1.Rows.Count - 1).ToList();
            List<int> newIndices = new List<int>();
            int next;
            int miss = dataGridView1.Rows.Count - 1;
            while (miss > 0)
            {
                next = rn.Next(miss);
                newIndices.Add(oldIndices[next]);
                oldIndices.RemoveAt(next);
                miss--;
            }

            List<List<string>> newRows = new List<List<string>>();
            List<string> newRow;
            foreach (int index in newIndices)
            {
                DataGridViewRow row = dataGridView1.Rows[index];
                if (!row.IsNewRow)
                {
                    newRow = new List<string>();
                    foreach (DataGridViewCell cell in row.Cells)
                    {
                        if (cell != null && cell.Value != null)
                        {
                            newRow.Add(cell.Value.ToString());
                        }
                        else
                        {
                            newRow.Add("");
                        }
                    }
                    newRows.Add(newRow);
                }
            }

            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                if (!row.IsNewRow)
                {
                    foreach (DataGridViewCell cell in row.Cells)
                    {
                        cell.Value = newRows[row.Index][cell.ColumnIndex];
                    }
                }
            }
            ToggleSaveButton(true);
        }
    }
}
