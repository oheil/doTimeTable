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
using System.Text;

using System.IO;
using System.Xml;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace doTimeTable
{
    public class Projects
    {
        private readonly Dictionary<Form1.SubformType, string> fileNames = new Dictionary<Form1.SubformType, string>();
        private readonly List<string> projectFolders = new List<string>();
        private readonly string projectBaseFolder = "";
        private string log_output = "";

        private struct UniqueForms
        {
            public string id;
            public Form1.SubformType subformType;
            public UniqueForms(string id, Form1.SubformType subformType)
            {
                this.id = id;
                this.subformType = subformType;
            }
        }
        private readonly Dictionary<UniqueForms, bool> openFormsForEdit = new Dictionary<UniqueForms, bool>();

        public string GetBaseFolder()
        {
            return projectBaseFolder;
        }

        // returns false if form should be editable, true if form should be read only
        public bool RegisterForm(string id, Form1.SubformType subformType)
        {
            bool r = false;
            foreach(UniqueForms form in openFormsForEdit.Keys)
            {
                if (form.id == id && form.subformType != subformType)
                {
                    if (!r)
                    {
                        r = openFormsForEdit[form];
                    }
                }
            }
            UniqueForms newForm = new UniqueForms(id, subformType);
            openFormsForEdit[newForm] = !r;
            return r;
        }

        public void DeregisterForm(string id, Form1.SubformType subformType)
        {
            UniqueForms deleteForm = new UniqueForms();
            bool delete = false;
            foreach (UniqueForms form in openFormsForEdit.Keys)
            {
                if (form.id == id && form.subformType == subformType)
                {
                    deleteForm = form;
                    delete = true;
                }
            }
            if (delete)
            {
                openFormsForEdit.Remove(deleteForm);
            }
        }

        public Projects()
        {
            fileNames.Add(Form1.SubformType.DAYSHOURS, "tageStunden.csv");
            fileNames.Add(Form1.SubformType.SUBJECT, "faecher.csv");
            fileNames.Add(Form1.SubformType.CLASSES, "klassen.csv");
            fileNames.Add(Form1.SubformType.TEACHER, "lehrer.csv");
            fileNames.Add(Form1.SubformType.TEACHERTOCLASS, "klasseFachLehrer.csv");
            fileNames.Add(Form1.SubformType.CLASSTOSUBJECTPRESET, "klassenStundenFest.csv");
            fileNames.Add(Form1.SubformType.CONFIG, "config.csv");

            if (Form1.config.config_xml.DocumentElement.GetElementsByTagName("projects_base_folder").Count == 0)
            {
                System.Xml.XmlElement projects_base_folder = Form1.config.config_xml.CreateElement("projects_base_folder");
                projects_base_folder.InnerXml = Form1.applicationDir + Path.DirectorySeparatorChar + "projects";
                System.Xml.XmlElement root = Form1.config.config_xml.DocumentElement;
                root.AppendChild(projects_base_folder);
                Form1.config.Save_config_file();
            }
            projectBaseFolder = Form1.config.config_xml.DocumentElement["projects_base_folder"].InnerXml;

            log_output = "info: project base folder is " + projectBaseFolder;
            Form1.logWindow.Write_to_log(ref log_output);

            FillProjectView();
        }

        public void FillProjectView()
        {
            projectFolders.Clear();
            ScanBaseFolder();
            if (projectFolders.Count > 0)
            {
                Form1.myself.FinalSetupDataGridView();
                string metaXML;
                foreach (string projectFolder in projectFolders)
                {
                    metaXML = projectBaseFolder + Path.DirectorySeparatorChar + projectFolder + Path.DirectorySeparatorChar + "meta.xml";
                    if (File.Exists(metaXML))
                    {
                        XmlDocument meta_xml = new XmlDocument();
                        meta_xml.Load(metaXML);

                        string name = "?";
                        if (meta_xml.DocumentElement.GetElementsByTagName("name").Count > 0)
                        {
                            name = meta_xml.DocumentElement["name"].InnerXml.ToString();
                        }
                        string description = "";
                        if (meta_xml.DocumentElement.GetElementsByTagName("description").Count > 0)
                        {
                            description = meta_xml.DocumentElement["description"].InnerXml.ToString();
                        }
                        string date = "";
                        if (meta_xml.DocumentElement.GetElementsByTagName("creationDate").Count > 0)
                        {
                            date = meta_xml.DocumentElement["creationDate"].InnerXml.ToString();
                        }
                        string state = "";
                        if (meta_xml.DocumentElement.GetElementsByTagName("state").Count > 0)
                        {
                            state = meta_xml.DocumentElement["state"].InnerXml.ToString();
                        }
                        string[] row = { name, projectFolder, description, date, "", state };
                        Form1.myself.AddRowToDataGridView(row);
                    }
                    else
                    {
                        log_output = "warning: file " + metaXML + " does not exist";
                        Form1.logWindow.Write_to_log(ref log_output);

                        string[] row = { "?", projectFolder };
                        Form1.myself.AddRowToDataGridView(row);
                    }
                }
            }
            else
            {
                Form1.myself.InitialSetupDataGridView();
            }
        }

        public void DeleteProject(string id)
        {
            string projectName = "unknown";
            string metaXML = projectBaseFolder + Path.DirectorySeparatorChar + id + Path.DirectorySeparatorChar + "meta.xml";
            if (File.Exists(metaXML))
            {
                XmlDocument meta_xml = new XmlDocument();
                meta_xml.Load(metaXML);
                if (meta_xml.DocumentElement.GetElementsByTagName("name").Count > 0)
                {
                    XmlNode name = meta_xml.DocumentElement.GetElementsByTagName("name")[0];
                    projectName = name.InnerXml;
                }
            }

            string projectPath = projectBaseFolder + Path.DirectorySeparatorChar + id;
            string deletePath = projectBaseFolder + Path.DirectorySeparatorChar + id + ".deleted";
            try
            {
                //Directory.Delete(projectPath, true);
                Directory.Move(projectPath, deletePath);
            }
            catch (Exception e)
            {
                log_output = "warning: can not delete project <" + projectName + "> in directory " + projectPath + ":";
                Form1.logWindow.Write_to_log(ref log_output);
                log_output = e.ToString();
                Form1.logWindow.Write_to_log(ref log_output);

                return;
            }

            log_output = "info: project <" + projectName + "> with id <" + id + "> deleted";
            Form1.logWindow.Write_to_log(ref log_output);
        }

        //public void EditProject(string projectName, string id, int rowIndex)
        public void EditProject(DataGridViewRow row)
        {
            string id = "";
            string projectName = Form1.LocRM.GetString("String15");
            string description = "";
            string state = "";
            if (row.Cells[0].Value != null && row.Cells[0].Value.ToString().Length > 0)
            {
                projectName = row.Cells[0].Value.ToString();
            }
            if (row.Cells.Count >= 4)
            {
                if (row.Cells[1].Value != null && row.Cells[1].Value.ToString().Length > 0)
                {
                    id = row.Cells[1].Value.ToString();
                }
                if (row.Cells[2].Value != null && row.Cells[2].Value.ToString().Length > 0)
                {
                    description = row.Cells[2].Value.ToString();
                }
                if (row.Cells[4].Value != null && row.Cells[4].Value.ToString().Length > 0)
                {
                    state = row.Cells[5].Value.ToString();
                }
            }
            string metaXML = projectBaseFolder + Path.DirectorySeparatorChar + id + Path.DirectorySeparatorChar + "meta.xml";
            if (File.Exists(metaXML))
            {
                XmlDocument meta_xml = new XmlDocument();
                meta_xml.Load(metaXML);
                if (meta_xml.DocumentElement.GetElementsByTagName("name").Count == 0)
                {
                    System.Xml.XmlElement name = meta_xml.CreateElement("name");
                    name.InnerXml = projectName;
                    System.Xml.XmlElement root = meta_xml.DocumentElement;
                    root.AppendChild(name);
                }
                else
                {
                    XmlNode name = meta_xml.DocumentElement.GetElementsByTagName("name")[0];
                    name.InnerXml = projectName;
                }

                if (meta_xml.DocumentElement.GetElementsByTagName("description").Count == 0)
                {
                    System.Xml.XmlElement name = meta_xml.CreateElement("description");
                    name.InnerXml = description;
                    System.Xml.XmlElement root = meta_xml.DocumentElement;
                    root.AppendChild(name);
                }
                else
                {
                    XmlNode name = meta_xml.DocumentElement.GetElementsByTagName("description")[0];
                    name.InnerXml = description;
                }

                if (meta_xml.DocumentElement.GetElementsByTagName("state").Count == 0)
                {
                    System.Xml.XmlElement xmlState = meta_xml.CreateElement("state");
                    xmlState.InnerXml = state;
                    System.Xml.XmlElement root = meta_xml.DocumentElement;
                    root.AppendChild(xmlState);
                }
                else
                {
                    XmlNode xmlState = meta_xml.DocumentElement.GetElementsByTagName("state")[0];
                    xmlState.InnerXml = state;
                }

                meta_xml.Save(metaXML);

                //log_output = "info: project " + metaXML + " changed";
                //Form1.logWindow.Write_to_log(ref log_output);
            }
            else
            {
                log_output = "warning: file " + metaXML + " does not exist";
                Form1.logWindow.Write_to_log(ref log_output);
            }
        }

        public bool CheckProject(string guid)
        {
            bool ret = true;
            string metaXML = projectBaseFolder + Path.DirectorySeparatorChar + guid + Path.DirectorySeparatorChar + "meta.xml";
            if (! File.Exists(metaXML))
            {
                ret = false;
            }
            XmlDocument meta_xml = new XmlDocument();
            meta_xml.Load(metaXML);
            System.Xml.XmlElement root = meta_xml.DocumentElement;
            if( root.HasAttribute("xmlns") )
            {
                if (root.GetAttribute("xmlns") != "http://heilbit.de/doTimeTable/meta.xsd")
                {
                    ret = false;
                }
            }
            else
            {
                ret = false;
            }
            //more checks?

            return ret;
        }

        public void UpdateMetaProjectID(string newGuid)
        {
            string metaXML = projectBaseFolder + Path.DirectorySeparatorChar + newGuid + Path.DirectorySeparatorChar + "meta.xml";
            if (File.Exists(metaXML))
            {
                XmlDocument meta_xml = new XmlDocument();
                meta_xml.Load(metaXML);
                if (meta_xml.DocumentElement.GetElementsByTagName("id").Count == 0)
                {
                    System.Xml.XmlElement id = meta_xml.CreateElement("id");
                    id.InnerXml = newGuid;
                    System.Xml.XmlElement root = meta_xml.DocumentElement;
                    root.AppendChild(id);
                }
                else
                {
                    XmlNode id = meta_xml.DocumentElement.GetElementsByTagName("id")[0];
                    id.InnerXml = newGuid;
                }
                meta_xml.Save(metaXML);
                log_output = "info: project " + metaXML + " id changed";
                Form1.logWindow.Write_to_log(ref log_output);
            }
        }

        public void UpdateMetaProjectName(string newGuid, string newProjectName)
        {
            string metaXML = projectBaseFolder + Path.DirectorySeparatorChar + newGuid + Path.DirectorySeparatorChar + "meta.xml";
            if (File.Exists(metaXML))
            {
                XmlDocument meta_xml = new XmlDocument();
                meta_xml.Load(metaXML);
                if (meta_xml.DocumentElement.GetElementsByTagName("name").Count == 0)
                {
                    System.Xml.XmlElement name = meta_xml.CreateElement("name");
                    name.InnerXml = newProjectName;
                    System.Xml.XmlElement root = meta_xml.DocumentElement;
                    root.AppendChild(name);
                }
                else
                {
                    XmlNode name = meta_xml.DocumentElement.GetElementsByTagName("name")[0];
                    name.InnerXml = newProjectName;
                }
                meta_xml.Save(metaXML);
                log_output = "info: project " + metaXML + " name changed";
                Form1.logWindow.Write_to_log(ref log_output);
            }
        }

        public string CreateNewFromOld(string projectName, string description, string date, string oldId, bool tmp = false)
        {
            string oldProjectPath;
            string newProjectPath;
            Guid newGuid = Guid.NewGuid();
            string newGuidString;
            Form1.SubformType state;
            if (projectBaseFolder.Length > 0)
            {
                state = GetState(oldId);
                oldProjectPath = projectBaseFolder + Path.DirectorySeparatorChar + oldId;
                while (Directory.Exists(projectBaseFolder + Path.DirectorySeparatorChar + newGuid.ToString()))
                {
                    newGuid = Guid.NewGuid();
                }
                newGuidString = newGuid.ToString();
                if (tmp)
                {
                    newGuidString += ".tmp";
                }
                newProjectPath = projectBaseFolder + Path.DirectorySeparatorChar + newGuidString;
                try
                {
                    Directory.CreateDirectory(newProjectPath);
                }
                catch (Exception e)
                {
                    log_output = "warning: can not create directory " + newProjectPath + ":";
                    Form1.logWindow.Write_to_log(ref log_output);
                    log_output = e.ToString();
                    Form1.logWindow.Write_to_log(ref log_output);
                    return null;
                }
                string metaXML = newProjectPath + Path.DirectorySeparatorChar + "meta.xml";
                try
                {
                    File.WriteAllText(metaXML, "<?xml version=\"1.0\" encoding=\"utf-8\"?>" + Environment.NewLine + "<meta xmlns=\"http://heilbit.de/doTimeTable/meta.xsd\">" + Environment.NewLine + "</meta>");
                }
                catch (Exception e)
                {
                    log_output = "warning: can not write " + metaXML + ":";
                    Form1.logWindow.Write_to_log(ref log_output);
                    log_output = e.ToString();
                    Form1.logWindow.Write_to_log(ref log_output);
                    return null;
                }
                XmlDocument meta_xml = new XmlDocument();
                meta_xml.Load(metaXML);
                XmlElement root = meta_xml.DocumentElement;
                XmlElement id = meta_xml.CreateElement("id");
                id.InnerXml = newGuidString;
                root.AppendChild(id);
                XmlElement name = meta_xml.CreateElement("name");
                name.InnerXml = projectName;
                root.AppendChild(name);
                XmlElement xmlDescription = meta_xml.CreateElement("description");
                xmlDescription.InnerXml = description;
                root.AppendChild(xmlDescription);
                XmlElement creationDate = meta_xml.CreateElement("creationDate");
                creationDate.InnerXml = date;
                root.AppendChild(creationDate);
                XmlElement stateXml = meta_xml.CreateElement("state");
                stateXml.InnerXml = state.ToString();
                root.AppendChild(stateXml);

                meta_xml.Save(metaXML);

                if (Directory.Exists(oldProjectPath))
                {
                    foreach (string file in fileNames.Values)
                    {
                        if (File.Exists(oldProjectPath + Path.DirectorySeparatorChar + file))
                        {
                            File.Copy(oldProjectPath + Path.DirectorySeparatorChar + file, newProjectPath + Path.DirectorySeparatorChar + file);
                        }
                    }
                }
            }
            else
            {
                log_output = "warning: project base folder is not defined";
                Form1.logWindow.Write_to_log(ref log_output);
                return null;
            }

            return newGuidString;
        }

        public void Copy(DataGridViewRow row)
        {
            string oldId;
            if (row != null && row.Cells.Count >= 3 && row.Cells[1].Value != null && row.Cells[1].Value.ToString().Length > 0)
            {
                oldId = row.Cells[1].Value.ToString();
            }
            else
            {
                return;
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
                projectName = row.Cells[0].Value.ToString() + " (" + Form1.LocRM.GetString("String145") + ")";
            }

            string newGuid;
            newGuid = CreateNewFromOld(projectName, description, date, oldId);
            if (newGuid != null)
            {
                Form1.SubformType state;
                state = GetState(newGuid);

                string statusString = Form1.myself.GetStatusString(newGuid.ToString(), state);
                string[] newRow = { projectName, newGuid.ToString(), description, date, statusString, state.ToString() };
                Form1.myself.AddRowToDataGridView(newRow);

                Form1.myself.UpdateDataGridView();

                log_output = "info: copy of project created, id=" + newGuid.ToString();
                Form1.logWindow.Write_to_log(ref log_output);
            }
        }

        public void CreateNewEmptyProject(ref string projectPath, ref string newGuid)
        {
            newGuid = Guid.NewGuid().ToString();
            projectPath = null;
            if (projectBaseFolder.Length > 0)
            {
                while (Directory.Exists(projectBaseFolder + Path.DirectorySeparatorChar + newGuid))
                {
                    newGuid = Guid.NewGuid().ToString();
                }
                projectPath = projectBaseFolder + Path.DirectorySeparatorChar + newGuid;
                try
                {
                    Directory.CreateDirectory(projectPath);
                }
                catch (Exception e)
                {
                    log_output = "warning: can not create directory " + projectPath + ":";
                    Form1.logWindow.Write_to_log(ref log_output);
                    log_output = e.ToString();
                    Form1.logWindow.Write_to_log(ref log_output);

                    projectPath = null;
                    newGuid = null;
                }
            }
        }

        public void CreateNewProject(string projectName = "", int rowIndex = -1)
        {
            string newGuidString = null;
            string projectPath = null; 
            string description = Form1.LocRM.GetString("String141");
            string date = DateTime.Now.ToString();

            if (projectName.Length == 0)
            {
                projectName = Form1.LocRM.GetString("String14");
            }

            if (projectBaseFolder.Length > 0)
            {
                CreateNewEmptyProject(ref projectPath, ref newGuidString);
                if (newGuidString != null && projectPath != null)
                {
                    string metaXML = projectPath + Path.DirectorySeparatorChar + "meta.xml";
                    try
                    {
                        File.WriteAllText(metaXML, "<?xml version=\"1.0\" encoding=\"utf-8\"?>" + Environment.NewLine + "<meta xmlns=\"http://heilbit.de/doTimeTable/meta.xsd\">" + Environment.NewLine + "</meta>");
                    }
                    catch (Exception e)
                    {
                        log_output = "warning: can not write " + metaXML + ":";
                        Form1.logWindow.Write_to_log(ref log_output);
                        log_output = e.ToString();
                        Form1.logWindow.Write_to_log(ref log_output);

                        ResetProjectList(rowIndex);

                        return;
                    }

                    XmlDocument meta_xml = new XmlDocument();
                    meta_xml.Load(metaXML);
                    XmlElement root = meta_xml.DocumentElement;
                    XmlElement id = meta_xml.CreateElement("id");
                    id.InnerXml = newGuidString;
                    root.AppendChild(id);
                    XmlElement name = meta_xml.CreateElement("name");
                    name.InnerXml = projectName;
                    root.AppendChild(name);
                    XmlElement xmlDescription = meta_xml.CreateElement("description");
                    xmlDescription.InnerXml = description;
                    root.AppendChild(xmlDescription);
                    XmlElement creationDate = meta_xml.CreateElement("creationDate");
                    creationDate.InnerXml = date;
                    root.AppendChild(creationDate);
                    XmlElement state = meta_xml.CreateElement("state");
                    state.InnerXml = Form1.SubformType.UNDEF.ToString();
                    root.AppendChild(state);
                    meta_xml.Save(metaXML);
                }
                else
                {
                    ResetProjectList(rowIndex);
                    return;
                }
            }
            else
            {
                log_output = "warning: project base folder is not defined";
                Form1.logWindow.Write_to_log(ref log_output);

                ResetProjectList(rowIndex);

                return;
            }

            if (Form1.isProjectListEmpty)
            {
                Form1.myself.FinalSetupDataGridView();
            }

            string status = Form1.myself.GetStatusString(newGuidString, Form1.SubformType.UNDEF);
            string[] newRow = { projectName, newGuidString, description, date, status, Form1.SubformType.UNDEF.ToString() };
            Form1.myself.AddRowToDataGridView(newRow, rowIndex);

            Form1.myself.UpdateDataGridView();

            log_output = "info: new project created, id=" + newGuidString;
            Form1.logWindow.Write_to_log(ref log_output);
        }

        private void ResetProjectList(int rowIndex)
        {
            if (Form1.isProjectListEmpty)
            {
                Form1.myself.InitialSetupDataGridView();
            }
            else
            {
                string[] newEmptyRow = { "" };
                Form1.myself.AddRowToDataGridView(newEmptyRow, rowIndex);
            }
        }

        private void ScanBaseFolder()
        {
            if (projectBaseFolder.Length > 0)
            {
                if (!Directory.Exists(projectBaseFolder))
                {
                    try
                    {
                        Directory.CreateDirectory(projectBaseFolder);
                    }
                    catch (Exception e)
                    {
                        log_output = "warning: directory " + projectBaseFolder + " does not exist and can't be created:";
                        Form1.logWindow.Write_to_log(ref log_output);
                        log_output = e.ToString();
                        Form1.logWindow.Write_to_log(ref log_output);
                        return;
                    }
                }
                DirectoryInfo baseDir;
                try
                {
                    baseDir = new DirectoryInfo(projectBaseFolder);
                }
                catch (Exception e)
                {
                    log_output = "warning: can't get directory info for " + projectBaseFolder + ":";
                    Form1.logWindow.Write_to_log(ref log_output);
                    log_output = e.ToString();
                    Form1.logWindow.Write_to_log(ref log_output);
                    return;
                }
                DirectoryInfo[] di;
                try
                {
                    di = baseDir.GetDirectories();
                }
                catch (Exception e)
                {
                    log_output = "warning: can't get subdirectories for " + projectBaseFolder + ":";
                    Form1.logWindow.Write_to_log(ref log_output);
                    log_output = e.ToString();
                    Form1.logWindow.Write_to_log(ref log_output);
                    return;
                }
                Regex rgxDel = new Regex(@".deleted$", RegexOptions.IgnoreCase);
                Regex rgxTmp = new Regex(@".tmp$", RegexOptions.IgnoreCase);
                Match matchesDel;
                Match matchesTmp;
                foreach (DirectoryInfo projectDir in di)
                {
                    matchesDel = rgxDel.Match(projectDir.Name);
                    matchesTmp = rgxTmp.Match(projectDir.Name);
                    if (!matchesDel.Success && !matchesTmp.Success)
                    {
                        projectFolders.Add(projectDir.Name);
                    }
                }
            }
        }

        public Form1.SubformType GetState(string id)
        {
            Form1.SubformType r = Form1.SubformType.UNDEF;
            try
            {
                string metaXML = projectBaseFolder + Path.DirectorySeparatorChar + id + Path.DirectorySeparatorChar + "meta.xml";
                if (File.Exists(metaXML))
                {
                    XmlDocument meta_xml = new XmlDocument();
                    meta_xml.Load(metaXML);
                    if (meta_xml.DocumentElement.GetElementsByTagName("state").Count == 1)
                    {
                        XmlNode stateXml = meta_xml.DocumentElement.GetElementsByTagName("state")[0];
                        string state = stateXml.InnerXml;
                        Enum.TryParse(state, out Form1.SubformType subform);
                        r = subform;
                    }
                }
            }
            catch (IOException e)
            {
                log_output = e.ToString();
                Form1.logWindow.Write_to_log(ref log_output);
            }

            return r;
        }

        public string GetProjectName(string id)
        {
            string r = null;
            try
            {
                string metaXML = projectBaseFolder + Path.DirectorySeparatorChar + id + Path.DirectorySeparatorChar + "meta.xml";
                if (File.Exists(metaXML))
                {
                    XmlDocument meta_xml = new XmlDocument();
                    meta_xml.Load(metaXML);
                    if (meta_xml.DocumentElement.GetElementsByTagName("name").Count == 1)
                    {
                        XmlNode stateXml = meta_xml.DocumentElement.GetElementsByTagName("name")[0];
                        r = stateXml.InnerXml;
                    }
                }
            }
            catch (IOException e)
            {
                log_output = e.ToString();
                Form1.logWindow.Write_to_log(ref log_output);
            }

            return r;
        }

        public void SaveData(string id, Form1.SubformType mySubformType, DataGridView dataGrid)
        {

            List<List<string>> data = new List<List<string>>();
            int lastColIndex = dataGrid.Columns.Count;
            List<string> newRow = new List<string>();
            foreach (DataGridViewColumn col in dataGrid.Columns)
            {
                if (col.Index < lastColIndex - 1 && !col.ReadOnly)
                {
                    if (col != null)
                    {
                        newRow.Add(col.HeaderText);
                    }
                }
            }
            data.Add(newRow);
            foreach (DataGridViewRow row in dataGrid.Rows)
            {
                newRow = new List<string>();
                if (row != null)
                {
                    foreach (DataGridViewCell cell in row.Cells)
                    {
                        if (cell.ColumnIndex < lastColIndex - 1 && !cell.ReadOnly)
                        {
                            if (cell != null && cell.Value != null && cell.Value.ToString() != "")
                            {
                                newRow.Add(cell.Value.ToString().Trim());
                            }
                            else
                            {
                                newRow.Add("");
                            }
                        }
                    }
                    data.Add(newRow);
                }
            }
            SaveData(id, mySubformType, data);
        }

        public void SaveData(string id, Form1.SubformType mySubformType, List<string[]> data)
        {
            List<List<string>> listData = new List<List<string>>();
            List<string> row;
            foreach( string[] array in data)
            {
                row = new List<string>(array);
                listData.Add(row);
            }
            SaveData(id, mySubformType, listData);
        }

        public void SaveData(string id, Form1.SubformType mySubformType, List<List<string>> data)
        {
            try
            {
                string fileName = fileNames[mySubformType];

                string metaXML = projectBaseFolder + Path.DirectorySeparatorChar + id + Path.DirectorySeparatorChar + "meta.xml";
                if (File.Exists(metaXML))
                {
                    if (data.Count > 1)
                    {
                        XmlDocument meta_xml = new XmlDocument();
                        meta_xml.Load(metaXML);
                        if (meta_xml.DocumentElement.GetElementsByTagName("state").Count == 0)
                        {
                            System.Xml.XmlElement state = meta_xml.CreateElement("state");
                            state.InnerXml = mySubformType.ToString();
                            System.Xml.XmlElement root = meta_xml.DocumentElement;
                            root.AppendChild(state);
                        }
                        else
                        {
                            XmlNode state = meta_xml.DocumentElement.GetElementsByTagName("state")[0];
                            state.InnerXml = mySubformType.ToString();
                        }
                        meta_xml.Save(metaXML);

                        Form1.myself.DeactivateActivateMenuItems(mySubformType);
                    }

                    string writeFile = projectBaseFolder + Path.DirectorySeparatorChar + id + Path.DirectorySeparatorChar + fileName;
                    Encoding utf8WithoutBom = new UTF8Encoding(false);
                    StreamWriter sw = new StreamWriter(writeFile, false, utf8WithoutBom);
                    int lastColIndex = data[0].Count;
                    foreach (List<string> row in data)
                    {
                        if (row != null)
                        {
                            foreach (string cell in row)
                            {
                                string sout = cell.Trim();
                                sout = sout.Replace(";", "###SEMICOLON###");
                                sw.Write(sout);
                                sw.Write(";");
                            }
                            sw.WriteLine();
                        }
                    }
                    sw.Close();
                }
            }
            catch (IOException e)
            {
                log_output = e.ToString();
                Form1.logWindow.Write_to_log(ref log_output);

                string msg = Form1.LocRM.GetString("String133");
                MessageBox.Show(msg, "Information", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
            }
        }

        public List<string[]> LoadData(string id, Form1.SubformType mySubformType, bool skipHeader = true)
        {
            string fileName = fileNames[mySubformType];

            List<string[]> returnData = new List<string[]>();
            List<int> skipEmptyCols = new List<int>();
            string loadFile = projectBaseFolder + Path.DirectorySeparatorChar + id + Path.DirectorySeparatorChar + fileName;
            if (File.Exists(loadFile))
            {
                try
                {
                    StreamReader sr = new StreamReader(loadFile);
                    string line = "";
                    string[] inValues;
                    string[] outValues;
                    int count = 0;
                    while (true && line != null)
                    {
                        line = sr.ReadLine();
                        if (line != null)
                        {
                            inValues = line.Split(';');
                            outValues = inValues;
                            bool all_zero = true;
                            int index = 0;
                            foreach (string value in inValues)
                            {
                                if( count== 0 && value == "" )
                                {
                                    skipEmptyCols.Add(index);
                                }
                                if (value != "" && value != "0")
                                {
                                    all_zero = false;
                                }
                                if (!skipEmptyCols.Contains(index))
                                {
                                    outValues[index] = value.Replace("###SEMICOLON###", ";");
                                }
                                index++;
                            }
                            if (!all_zero && (count > 0 || !skipHeader))
                            {
                                returnData.Add(outValues);
                            }
                        }
                        count++;
                    }
                    sr.Close();
                }
                catch (IOException e)
                {
                    log_output = e.ToString();
                    Form1.logWindow.Write_to_log(ref log_output);
                }
            }
            List<string[]> finalReturnData = new List<string[]>();
            foreach ( string[] row in returnData)
            {
                if (skipEmptyCols.Count > 0)
                {
                    int endIndex = row.Length - skipEmptyCols.Count + 1;
                    string[] newRow = new string[endIndex];
                    Array.Copy(row, newRow, endIndex);
                    finalReturnData.Add(newRow);
                }
                else
                {
                    finalReturnData.Add(row);
                }
            }
            return finalReturnData;
        }

        public List<FileInfo> CheckForCalculatedRosters(string id)
        {
            List<FileInfo> rosterFound = new List<FileInfo>();
            
            string projectPath = projectBaseFolder + Path.DirectorySeparatorChar + id;
            DirectoryInfo baseDir;
            try
            {
                baseDir = new DirectoryInfo(projectPath);
            }
            catch (Exception e)
            {
                log_output = "warning: can't get directory info for " + projectPath + ":";
                Form1.logWindow.Write_to_log(ref log_output);
                log_output = e.ToString();
                Form1.logWindow.Write_to_log(ref log_output);
                return rosterFound;
            }
            FileInfo[] fi;
            try
            {
                fi = baseDir.GetFiles();
            }
            catch (Exception e)
            {
                log_output = "warning: can't get files in " + projectPath + ":";
                Form1.logWindow.Write_to_log(ref log_output);
                log_output = e.ToString();
                Form1.logWindow.Write_to_log(ref log_output);
                return rosterFound;
            }
            Regex rgx = new Regex(@"^timetable.*.csv$");
            Match matches;
            foreach (FileInfo file in fi)
            {
                matches = rgx.Match(file.Name);
                if (matches.Success)
                {
                    rosterFound.Add(file);
                }
            }

            return rosterFound;
        }

        public void CalculateRoster(string id, Form1.CalcRosterReadyCallbackDelegate callback)
        {
            string projectPath = projectBaseFolder + Path.DirectorySeparatorChar + id;
            string name = GetProjectName(id);
            if (name != null)
            {
                if (!julia.NativeMethods.juliaRunning && Form1.myself.julia_ok)
                {
                    List<FileInfo> rosterFound = CheckForCalculatedRosters(id);
                    foreach (FileInfo file in rosterFound)
                    {
                        try
                        {
                            file.Delete();
                        }
                        catch (Exception e)
                        {
                            log_output = "warning: can't delete " + file.Name + "file in " + projectPath + ":";
                            Form1.logWindow.Write_to_log(ref log_output);
                            log_output = e.ToString();
                            Form1.logWindow.Write_to_log(ref log_output);
                            return;
                        }
                    }

                    log_output = "calculating project <" + name + ">";
                    Form1.logWindow.Write_to_log(ref log_output);

                    julia.NativeMethods.juliaRunning = true;
                    julia.NativeMethods.Julia(Form1.juliaDir, Form1.juliaScriptDir, projectPath);

                    Progress.currentProgress.Show();
                    Progress.currentProgress.ActivateMe(callback);

                    Form1.myself.Enabled = false;
                    foreach (Form2 form2 in Form2.myInstantiations)
                    {
                        form2.Enabled = false;
                    }
                }
                else
                {
                    log_output = "A new Julia thread can not be opened.";
                    Form1.logWindow.Write_to_log(ref log_output);
                }
            }

            return;
        }

        public SortedDictionary<string,List<List<string>>> OpenRoster(string id, int item)
        {
            SortedDictionary<string, List<List<string>>> allClasses = new SortedDictionary<string, List<List<string>>>();

            string projectPath = projectBaseFolder + Path.DirectorySeparatorChar + id;
            DirectoryInfo baseDir;
            try
            {
                baseDir = new DirectoryInfo(projectPath);
            }
            catch (Exception e)
            {
                log_output = "warning: can't get directory info for " + projectPath + ":";
                Form1.logWindow.Write_to_log(ref log_output);
                log_output = e.ToString();
                Form1.logWindow.Write_to_log(ref log_output);
                return allClasses;
            }
            FileInfo[] fi;
            try
            {
                fi = baseDir.GetFiles();
            }
            catch (Exception e)
            {
                log_output = "warning: can't get files in " + projectPath + ":";
                Form1.logWindow.Write_to_log(ref log_output);
                log_output = e.ToString();
                Form1.logWindow.Write_to_log(ref log_output);
                return allClasses;
            }
            Regex rgx = new Regex(@"^timetable.*.csv$");
            Match matches;
            string file2open = null;
            int count = 1;
            foreach (FileInfo file in fi)
            {
                matches = rgx.Match(file.Name);
                if (matches.Success)
                {
                    if (count == item)
                    {
                        file2open = projectPath = projectBaseFolder + Path.DirectorySeparatorChar + id + Path.DirectorySeparatorChar + file.Name;
                        break;
                    }
                    count++;
                }
            }

            if (file2open != null)
            {
                try
                {
                    StreamReader sr = new StreamReader(file2open);
                    string line = "";
                    string[] inValues;
                    bool newClass = false;
                    string currentClass = null;
                    while (true && line != null)
                    {
                        List<string> newLine;
                        line = sr.ReadLine();
                        if (line != null && (count > 0))
                        {
                            inValues = line.Split(';');
                            string value = inValues[0];
                            if (value == "###KLASSE###")
                            {
                                newClass = true;
                            }
                            else
                            {
                                if (newClass)
                                {
                                    newClass = false;
                                    currentClass = value;
                                    if (!allClasses.ContainsKey(currentClass))
                                    {
                                        allClasses[currentClass] = new List<List<string>>();
                                    }
                                }
                                else
                                {
                                    newLine = new List<string>(inValues);
                                    if (currentClass != null)
                                    {
                                        allClasses[currentClass].Add(newLine);
                                    }
                                }
                            }
                        }
                    }
                    sr.Close();
                }
                catch (IOException e)
                {
                    log_output = e.ToString();
                    Form1.logWindow.Write_to_log(ref log_output);
                }
            }

            return allClasses;
        }

    }
}
