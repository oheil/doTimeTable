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

using System.IO;

namespace doTimeTable
{
    public partial class Form4 : Form
    {
        private readonly string myId = "";
        private readonly int myItemNumber = -1;

        private bool geometry_changed = false;
        private string geometry;
        private string geometry_komponente = "editForm4";
        private string geometry_komponente_base = "editForm4";

        private SortedDictionary<string, List<List<string>>> allClasses;
        private readonly SortedDictionary<string, List<List<string>>> allTeachers;

        public enum CurrentViewType
        {
            CLASSES,
            TEACHERS
        };
        public CurrentViewType currentView = CurrentViewType.CLASSES;

        public Form4(string id, string projectName, int item)
        {
            myId = id;
            myItemNumber = item;
            geometry_komponente = geometry_komponente + "_" + id + "_" + item.ToString();
            InitializeComponent();

            this.Text = Form1.LocRM.GetString("String146") + " - " + projectName + " - #" + item.ToString();

            fileToolStripMenuItem.Text = Form1.LocRM.GetString("String2");
            closeToolStripMenuItem.Text = Form1.LocRM.GetString("String6");
            exportToolStripMenuItem.Text = Form1.LocRM.GetString("String149");
            rosterToolStripMenuItem.Text = Form1.LocRM.GetString("String1");
            exportToDefaultAppToolStripMenuItem.Text = Form1.LocRM.GetString("String150");

            classTeacherToggleToolStripMenuItem.Text = Form1.LocRM.GetString("String147");
            currentView = CurrentViewType.CLASSES;

            allClasses = new SortedDictionary<string, List<List<string>>>();
            allTeachers = new SortedDictionary<string, List<List<string>>>();
            UpdateView();
        }

        public bool UpdateView()
        {
            bool r = true;
            allClasses = Form1.projects.OpenRoster(myId, myItemNumber);
            allTeachers.Clear();
            CreateTeachersView();
            if (allClasses.Count == 0 || allTeachers.Count == 0)
            {
                MyClose();
                Dispose();
                r = false;
            }
            else
            {
                UpdateTabs();
                string tabName;
                if (currentView == CurrentViewType.CLASSES)
                {
                    tabName = allClasses.Keys.ToArray<string>()[tabControl1.SelectedIndex];
                }
                else
                {
                    tabName = allTeachers.Keys.ToArray<string>()[tabControl1.SelectedIndex];
                }
                tabControl1.SelectedTab.Controls.Add(dataGridView1);
                FillDataGrid(tabName);
            }
            return r;
        }

        public void UpdateTabs()
        {
            if (currentView == CurrentViewType.CLASSES)
            {
                tabControl1.TabPages.Clear();
                foreach (string tabHeader in allClasses.Keys)
                {
                    tabControl1.TabPages.Add(tabHeader);
                }
            }
            else
            {
                tabControl1.TabPages.Clear();
                foreach (string tabHeader in allTeachers.Keys)
                {
                    tabControl1.TabPages.Add(tabHeader);
                }
            }
        }

        public void CreateTeachersView()
        {
            allTeachers.Clear();
            List<List<string>> currentDefault = new List<List<string>>();
            foreach (string className in allClasses.Keys)
            {
                currentDefault = new List<List<string>>();
                for (int rowIndex = 0; rowIndex < allClasses[className].Count; rowIndex++) {
                    List<string> row = new List<string>(allClasses[className][rowIndex]);
                    if (rowIndex == 0)
                    {
                        currentDefault.Add(row);
                    }
                    else
                    {
                        List<string> currentDefaultRow = new List<string>();
                        currentDefault.Add(currentDefaultRow);
                        for ( int entryIndex = 0; entryIndex<row.Count; entryIndex++) {
                            string entry = row[entryIndex];
                            if (entryIndex == 0)
                            {
                                currentDefaultRow.Add(entry);
                            }
                            else
                            {
                                currentDefaultRow.Add("");
                                if (entry.Length > 0)
                                {
                                    string subjectName;
                                    string teacherName;
                                    string[] entrySplitted = entry.Split(':');
                                    if(entrySplitted.Length == 2)
                                    {
                                        subjectName = entrySplitted[0];
                                        teacherName = entrySplitted[1];
                                    }
                                    else
                                    {
                                        subjectName = entry;
                                        teacherName = "";
                                    }

                                    subjectName = subjectName.Replace("###SEMICOLON###", ";");
                                    subjectName = subjectName.Replace("###COLON###", ":");
                                    teacherName = teacherName.Replace("###SEMICOLON###", ";");
                                    teacherName = teacherName.Replace("###COLON###", ":");

                                    if (allTeachers.ContainsKey(teacherName))
                                    {
                                        while (allTeachers[teacherName].Count < rowIndex)
                                        {
                                            List<string> newRow = new List<string>(currentDefault[allTeachers[teacherName].Count]);
                                            allTeachers[teacherName].Add(newRow);
                                        }
                                        if (allTeachers[teacherName].Count == rowIndex)
                                        {
                                            List<string> newRow = new List<string>(currentDefaultRow);
                                            allTeachers[teacherName].Add(newRow);
                                        }
                                        if (allTeachers[teacherName].Count > rowIndex)
                                        {
                                            List<string> newRow = allTeachers[teacherName][rowIndex];
                                            while (newRow.Count <= entryIndex)
                                            {
                                                newRow.Add("");
                                            }
                                            if (newRow[entryIndex].Length > 0)
                                            {
                                                newRow[entryIndex] = newRow[entryIndex] + "|" + className + ":" + subjectName;
                                            }
                                            else
                                            {
                                                newRow[entryIndex] = className + ":" + subjectName;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        allTeachers[teacherName] = new List<List<string>>();
                                        foreach( List<string> row2 in currentDefault)
                                        {
                                            List<string> newRow = new List<string>(row2);
                                            allTeachers[teacherName].Add(newRow);
                                        }
                                        allTeachers[teacherName][rowIndex][entryIndex] = className + ":" + subjectName;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            foreach (string teacher in allTeachers.Keys)
            {
                for (int rowIndex = 0; rowIndex < currentDefault.Count; rowIndex++)
                {
                    List<String> row = currentDefault[rowIndex];
                    if (allTeachers[teacher].Count < rowIndex + 1)
                    {
                        List<string> newRow = new List<string>(row);
                        allTeachers[teacher].Add(row);
                    }
                    for (int entryIndex = 0; entryIndex < row.Count; entryIndex++)
                    {
                        List<string> newRow = allTeachers[teacher][rowIndex];
                        if(newRow.Count < entryIndex + 1)
                        {
                            newRow.Add("");
                        }
                    }
                }
            }
        }

        private void Form4_FormClosed(object sender, FormClosedEventArgs e)
        {
            MyClose();
        }

        private void MyClose()
        {
            Form1.myself.RemoveIDfromOpenView(myId + "_" + myItemNumber.ToString());
            Save_geometry();
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
                    Form1.config.Prune_config(ref geometry_komponente_base);
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
                }
                catch
                {
                    string log_output = "Setting form4 geometry failed: <" + geometry + ">";
                    Form1.logWindow.Write_to_log(ref log_output);
                }
                if (!Form1.myself.IsOnScreen(this.DesktopLocation))
                {
                    this.DesktopLocation = new Point(0, 0);
                }

                geometry_changed = false;
            }

        }

        private void Form4_Load(object sender, EventArgs e)
        {
            Set_geometry();
        }

        private void Form4_LocationChanged(object sender, EventArgs e)
        {
            geometry_changed = true;
        }

        private void Form4_SizeChanged(object sender, EventArgs e)
        {
            geometry_changed = true;
        }

        private void TabControl1_Selected(object sender, TabControlEventArgs e)
        {
            TabSelectionChanged();
        }

        public void TabSelectionChanged()
        {
            if (tabControl1.SelectedIndex >= 0)
            {
                string tabName;
                if (currentView == CurrentViewType.CLASSES)
                {
                    tabName = allClasses.Keys.ToArray<string>()[tabControl1.SelectedIndex];
                }
                else
                {
                    tabName = allTeachers.Keys.ToArray<string>()[tabControl1.SelectedIndex];
                }
                FillDataGrid(tabName);
                tabControl1.SelectedTab.Controls.Add(dataGridView1);
            }
        }

        private void FillDataGrid(string tabHeader)
        {
            dataGridView1.Rows.Clear();

            List<List<string>> rows;
            if (currentView == CurrentViewType.CLASSES)
            {
                rows = allClasses[tabHeader];
            }
            else
            {
                rows = allTeachers[tabHeader];
            }

            int colNumber = 0;
            foreach (List<string> row in rows)
            {
                if (row.Count > colNumber)
                {
                    colNumber = row.Count - 1;
                }
            }
            dataGridView1.ColumnCount = colNumber;
            foreach (DataGridViewColumn column in dataGridView1.Columns)
            {
                column.SortMode = DataGridViewColumnSortMode.NotSortable;
            }
            foreach (List<string> row in rows)
            {
                string[] rowArray = row.ToArray();
                for (int index = 0; index < rowArray.Length; index++)
                {
                    rowArray[index] = rowArray[index].Replace("###SEMICOLON###", ";");
                    rowArray[index] = rowArray[index].Replace("###COLON###", ":");
                }
                dataGridView1.Rows.Add(rowArray);
            }
        }

        private void ClassTeacherToggleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (currentView == CurrentViewType.CLASSES)
            {
                classTeacherToggleToolStripMenuItem.Text = Form1.LocRM.GetString("String148");
                currentView = CurrentViewType.TEACHERS;
            }
            else
            {
                classTeacherToggleToolStripMenuItem.Text = Form1.LocRM.GetString("String147");
                currentView = CurrentViewType.CLASSES;
            }
            UpdateTabs();
            TabSelectionChanged();
        }

        private void CloseToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MyClose();
            this.Dispose();
        }

        private void ExportToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ExportToFile();
        }

        private void ExportToFile(string fileName = null)
        {
            if (fileName == null)
            {
                SaveFileDialog saveFileDialog1 = new SaveFileDialog
                {
                    Filter = "txt files (*.txt)|*.txt|csv files (*.csv)|*.csv",
                    FilterIndex = 2,
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
                    return;
                }
            }
            //Encoding utf8WithoutBom = new UTF8Encoding(false);
            Encoding utf8WithBom = new UTF8Encoding(true);
            try
            {
                StreamWriter sw = new StreamWriter(fileName, false, utf8WithBom);

                SortedDictionary<string, List<List<string>>> exportedData;
                if (currentView == CurrentViewType.CLASSES)
                {
                    exportedData = allClasses;
                }
                else
                {
                    exportedData = allTeachers;
                }
                foreach (string key in exportedData.Keys)
                {
                    List<List<string>> tab = exportedData[key];
                    int rowIndex = 0;
                    foreach (List<string> row in tab)
                    {
                        List<string> exportRow = new List<string>(row);
                        if (rowIndex == 0)
                        {
                            exportRow[0] = key;
                        }
                        foreach (string entry in exportRow)
                        {
                            string sout = entry;
                            sout = sout.Replace("###SEMICOLON###", ";");
                            sout = sout.Replace("###COLON###", ":");
                            sw.Write(sout);
                            sw.Write(";");
                        }
                        sw.WriteLine();
                        rowIndex++;
                    }
                    sw.WriteLine();
                }
                sw.Close();
            }
            catch (IOException ex)
            {
                string log_output = ex.ToString();
                Form1.logWindow.Write_to_log(ref log_output);

                string msg = Form1.LocRM.GetString("String133");
                MessageBox.Show(msg, "Information", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
            }
        }

        private void ExportToDefaultAppToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string fileName = Path.GetTempFileName();
            fileName = fileName.Replace(".tmp", ".csv");
            ExportToFile(fileName);
            System.Diagnostics.Process.Start(fileName);
        }
    }
}
