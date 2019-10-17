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
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Data;
using System.Xml;
using System.Windows.Forms;
using System.Threading;

using System.Text.RegularExpressions;


namespace doTimeTable
{
	/// <summary>
	/// Summary description for config.
	/// </summary>
	public class Config
	{
        private readonly string config_file_name = "";
        private readonly string config_file_path = "";

        //public XmlDataDocument config_xml;
        public XmlDocument config_xml;

        protected string table_name;
        protected ArrayList get_columns;
        protected SortedList get_columns_map;
        protected ArrayList add_columns;
        protected ArrayList modify_columns;
        public ArrayList select_columns;

        public ArrayList config_list;

        public Config(string path = ".")
		{
			//
			// TODO: Add constructor logic here
			//
            string log_output;
            try
            {
                bool configFound = false;
                config_file_name = "config.xml";
                config_file_path = "." + Path.DirectorySeparatorChar;
                if (!File.Exists(config_file_path + config_file_name))
                {
                    log_output = "info: config file " + config_file_path + config_file_name + " not found";
                    Form1.logWindow.Write_to_log(ref log_output);
                    config_file_path = Application.StartupPath + Path.DirectorySeparatorChar;
                }
                else
                {
                    configFound = true;
                }
                if (!configFound && !File.Exists(config_file_path + config_file_name))
                {
                    log_output = "info: config file " + config_file_path + config_file_name + " not found";
                    Form1.logWindow.Write_to_log(ref log_output);
#if DEBUG
                    config_file_path = ".." + Path.DirectorySeparatorChar + ".." + Path.DirectorySeparatorChar;
                }
                else
                {
                    configFound = true;
                }
                if (!configFound && !File.Exists(config_file_path + config_file_name))
                {
                    log_output = "info: config file " + config_file_path + config_file_name + " not found";
                    Form1.logWindow.Write_to_log(ref log_output);
#endif
                    config_file_path = path + Path.DirectorySeparatorChar;
                }
                else
                {
                    configFound = true;
                }
                if (!configFound && !File.Exists(config_file_path + config_file_name))
                {
                    log_output = "info: config file " + config_file_path + config_file_name + " not found";
                    Form1.logWindow.Write_to_log(ref log_output);
                }
                else
                {
                    configFound = true;
                }
                if (!configFound)
                {
                    log_output = "info: creating config file " + config_file_path + config_file_name;
                    Form1.logWindow.Write_to_log(ref log_output);
                    File.WriteAllText(config_file_path + config_file_name, "<?xml version=\"1.0\" encoding=\"utf-8\"?>" + Environment.NewLine + "<config xmlns=\"http://heilbit.de/doTimeTable/config.xsd\">" + Environment.NewLine + "</config>");
                }

                if (!configFound && !File.Exists(config_file_path + config_file_name))
                {
                    log_output = "error: config file " + config_file_path + config_file_name + " can not be created";
                    Form1.logWindow.Write_to_log(ref log_output);
                    return;
                }
            }
            catch (Exception)
            {
                return;
            }

            //log_output = "trying config file " + config_file_name + "... found";
            //Form1.logWindow.write_to_log(ref log_output);

			//config_xml = new XmlDataDocument();
            config_xml = new XmlDocument();
			config_xml.Load(config_file_path + config_file_name);

            log_output = "config file found: " + config_file_path + config_file_name;
            Form1.logWindow.Write_to_log(ref log_output);

            table_name = "config";
            get_columns = new ArrayList();
            get_columns_map = new SortedList();
            int index = 0;
            get_columns.Add("id"); get_columns_map.Add("id", index++);
            get_columns.Add("komponente"); get_columns_map.Add("komponente", index++);
            get_columns.Add("config"); get_columns_map.Add("config", index++);
            get_columns.Add("config_user"); get_columns_map.Add("config_user", index++);
            add_columns = new ArrayList
            {
                "komponente",
                "config",
                "config_user"
            };
            modify_columns = new ArrayList
            {
                "id",
                "komponente",
                "config",
                "config_user"
            };
            select_columns = new ArrayList
            {
                "id",
                "komponente",
                "config",
                "config_user",
                //select_columns.Add("deleted");
                //select_columns.Add("version");
                //select_columns.Add("next_version");
                //select_columns.Add("act_version");
                //select_columns.Add("version_connect");
                "version_datetime",
                "version_user"
            };

            //Console.WriteLine("Display the title element...");
            //Console.WriteLine( config_xml.DocumentElement["max_log_lines"].InnerXml );
            //Console.WriteLine( config_xml.DocumentElement["log_file_name"].InnerXml );

        }

        public bool Get_xml_config(string id, ref string result)
        {
            //string result = null;
            bool found = false;
            if( this.config_xml.DocumentElement[id] != null)
            {
                result = this.config_xml.DocumentElement[id].InnerXml;
                found = true;
            }
            return found;
        }

        public int Get_index_of_database_col(string col_name)
        {
            int rc = get_columns_map.IndexOfKey(col_name);
            if (rc >= 0)
            {
                rc = (int)get_columns_map.GetByIndex(rc);
            }
            return rc;
        }

        public string Add_new_value(ref ArrayList values)
        {
            string id = Form1.database.Add_notversioned_data(
                ref table_name,
                ref add_columns,
                ref values);

            return id;
        }
        public string Modify_data(ref string id, ref ArrayList values, bool save_test = false)
        {
            values.Insert(0, id);
            string r_id = "";
            if (Form1.app_ready || save_test)
            {
                r_id = Form1.database.Modify_notversioned_data(
                    ref table_name,
                    ref modify_columns,
                    ref values);
            }
            return r_id;
        }

        public void Delete_data(ref ArrayList ids)
        {
            Form1.database.Expunge_notversioned_data(
                ref table_name,
                ref ids
                );
        }

        public ArrayList Get_values()
        {
            //ArrayList data_list = Get_values(false);
            //return data_list;
            config_list = Form1.database.Get_notversioned_data_list(ref table_name, ref get_columns);
            return config_list;
        }
        /*
        public ArrayList Get_values(bool withDeleted)
        {
            //loading data
            config_list = Form1.database.Get_notversioned_data_list(ref table_name, ref get_columns);
        
            return config_list;
        }
        */

        public void Prune_config(ref string komponente)
        {
            switch (komponente) {
                case "editForm4":
                    ArrayList removeIDs = new ArrayList();
                    string removeID;
                    string config_komp;
                    foreach (ArrayList row in config_list)
                    {
                        //remove unused config entries of form editForm4_id
                        List<string> allCurrentIDs = Form1.myself.GetAllIDsFromDataGridView();
                        removeID = row[Get_index_of_database_col("id")].ToString();
                        config_komp = row[Get_index_of_database_col("komponente")].ToString();
                        if (config_komp.Contains(komponente))
                        {
                            config_komp = config_komp.Replace(komponente + "_", "");
                            string pattern = @"_\d+$";
                            Regex rgx = new Regex(pattern);
                            config_komp = rgx.Replace(config_komp, "");
                            if (!allCurrentIDs.Contains(config_komp))
                            {
                                removeIDs.Add(removeID);
                            }
                        }
                    }
                    if (removeIDs.Count > 0)
                    {
                        Delete_data(ref removeIDs);
                    }
                    break;
                default:
                    break;
            }
        }

        public void Save_config(ref string komponente, ref string config)
        {
            if (config_list == null) return;

            bool found = false;
            ArrayList found_row = new ArrayList();
            string id = "";
            foreach (ArrayList row in config_list)
            {
                if (row[Get_index_of_database_col("komponente")].ToString() == komponente)
                {
                    id = row[Get_index_of_database_col("id")].ToString();
                    found_row = row;
                    found = true;
                    break;
                }
            }
            if (found)
            {
                found_row[Get_index_of_database_col("config")] = config;
                found_row.RemoveAt(0);
                Modify_data(ref id, ref found_row);
                Get_values();
            }
            else
            {
                found_row.Add(komponente);
                found_row.Add(config);
                found_row.Add(Form1.currentWindowsUser);

                Add_new_value(ref found_row);
                Get_values();
            }
        }

        public string Get_config(ref string komponente)
        {
            string tmp_config = "";
            foreach (ArrayList row in config_list)
            {
                if (row[Get_index_of_database_col("komponente")].ToString() == komponente )
                {
                    tmp_config = row[Get_index_of_database_col("config")].ToString();
                    break;
                }
            }

            return tmp_config;
        }

        public void Save_config_file()
        {
            config_xml.Save(config_file_path + config_file_name);
        }

    }
}
