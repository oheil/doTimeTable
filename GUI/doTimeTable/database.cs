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
using System.Data;
using System.Collections;

namespace doTimeTable
{
	/// <summary>
	/// Summary description for database.
	/// </summary>
	public class Database : IDisposable
    {
		private readonly string db_type;
		private readonly Db_base db;

        private bool lastDBconnected = false;
        private bool lastDBcreated = false;

		public DataSet data;

        public string version = "NA";

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                // dispose managed resources
            }
            // free native resources
            data.Dispose();
        }

        public void Dispose()
        {
            Dispose(true);
        }

        /// <summary>
        /// 
        /// </summary>
        public Database()
		{
            //which database
            //db_type = Form1.config.config_xml.DocumentElement["database_type"].InnerXml;

            if ( ! Form1.config.Get_xml_config("database_type", ref db_type))
            {
                System.Xml.XmlElement database_type = Form1.config.config_xml.CreateElement("database_type");
                database_type.InnerXml = "xml";
                System.Xml.XmlElement root = Form1.config.config_xml.DocumentElement;
                root.AppendChild(database_type);
                Form1.config.Save_config_file();
            }
            Form1.config.Get_xml_config("database_type", ref db_type);
            if (db_type == "flat")
            {
                db = new Db_text();
            }
            else if (db_type == "xml")
            {
                db = new Db_xml();
            }
            else
            {
                string log_output;
                log_output = "error: database type " + db_type + " unknown";
                Form1.logWindow.Write_to_log(ref log_output);
                return;
            }

            data = new DataSet();
        }

        public void Connect()
        {
            //string db_name = Form1.config.config_xml.DocumentElement["database_name"].InnerXml;
            string db_name = "";
            if( !Form1.config.Get_xml_config("database_name", ref db_name))
            {
                System.Xml.XmlElement database_name = Form1.config.config_xml.CreateElement("database_name");
                database_name.InnerXml = "doTimeTable";
                System.Xml.XmlElement root = Form1.config.config_xml.DocumentElement;
                root.AppendChild(database_name);
                Form1.config.Save_config_file();
            }
            Form1.config.Get_xml_config("database_name", ref db_name);

            if (!lastDBconnected )
            {
                Form1.state.current_state = State.State_type.Fine;

                lastDBconnected = true;
                lastDBcreated = false;

                //first try to connect database
                if (db.Connect() != 0)
                {
                    Form1.state.current_state = State.State_type.Fatal;
                    Form1.state.last_messages.Add("fatal:database connection failed");
                }

                //if database connection is ok load data
                data = new DataSet();
                if (db.Load_data(ref data) != 0)
                {
                    if (!lastDBcreated )
                    {
                        lastDBcreated = true;

                        db.Create_database_and_tables(false);
                    }
                    //try again
                    data = new DataSet();
                    if (db.Load_data(ref data) != 0)
                    {
                        Form1.state.current_state = State.State_type.Fatal;
                        Form1.state.last_messages.Add("fatal:database data load failed");
                        string log_output = "Loading data from database failed";
                        Form1.logWindow.Write_to_log(ref log_output);
                    }
                }
            }

            //setting user rights on database tables
            if (Form1.state.current_state < State.State_type.Error)
            {
                db.SetDBAccessRights();
            }

            //save schema as file
            string database_write_schema_xml = "0";
            Form1.config.Get_xml_config("database_write_schema_xml", ref database_write_schema_xml);
            if (
                Form1.state.current_state < State.State_type.Error &&
                1 == Convert.ToInt32(database_write_schema_xml)
                )
            {
                string xml_schema;
                string database_write_xml_target = "";
                if (Form1.config.Get_xml_config("database_write_xml_target", ref database_write_xml_target))
                {
                    if (database_write_xml_target.Split(';').Length >= 0)
                    {
                        xml_schema = database_write_xml_target.Split(';')[0];
                        data.WriteXmlSchema(xml_schema);
                        data.DataSetName = db_name;
                    }
                }
            }
            string database_write_data_xml = "0";
            Form1.config.Get_xml_config("database_write_data_xml", ref database_write_data_xml);
            if (
                Form1.state.current_state < State.State_type.Error &&
                1 == Convert.ToInt32(database_write_data_xml)
                )
            {
                string xml_data;
                string database_write_xml_target = "";
                if (Form1.config.Get_xml_config("database_write_xml_target", ref database_write_xml_target))
                {
                    if (database_write_xml_target.Split(';').Length >= 1)
                    {
                        xml_data = database_write_xml_target.Split(';')[1];
                        data.DataSetName = db_name;
                        data.WriteXml(xml_data);
                    }
                }
            }

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="table"></param>
        public State.Database_state Sava_data(string table)
        {
            return db.Save_data( ref data, table );
        }

        public ArrayList Get_versioned_data_list(
            ref string table,
            ref ArrayList columns)
        {
            return Get_versioned_data_list(ref table, ref columns, false);
        }

        public ArrayList Get_by_select(ref string table, ref string column, ref string where)
        {
            return db.Get_by_select( ref table, ref column, ref where );
        }
            
        /// <summary>
        /// 
        /// </summary>
        /// <param name="table"></param>
        /// <param name="columns"></param>
        /// <returns>an ArrayList(rows) of ArrayLists(cols)</returns>
        public ArrayList Get_versioned_data_list(
            ref string table, 
            ref ArrayList columns,
            bool with_deleted)
        {
            ArrayList data_list = new ArrayList();

            //string entry;

            if (data.Tables.Contains(table))
            {
                DataRowCollection existing_rows = data.Tables[table].Rows;
                foreach (DataRow existing_row in existing_rows)
                {
                    //only those rows where actual version equals 1
                    // and which are not deleted
                    if (existing_row["act_version"].Equals(1) &&
                        (with_deleted || existing_row["deleted"].Equals(0)))
                    {
                        /*
                        ArrayList row = new ArrayList();
                        foreach (string column in columns)
                        {
                            entry = existing_row[column].ToString();
                            row.Add(entry);
                        }
                        */
                        ArrayList row = new ArrayList(existing_row.ItemArray);
                        data_list.Add(row);
                    }
                }
            }
            return data_list;
        }

        public ArrayList Get_notversioned_data_list(
            ref string table,
            ref ArrayList columns)
        {
            ArrayList data_list = new ArrayList();

            //string entry;

            if (data.Tables.Contains(table))
            {
                DataRowCollection existing_rows = data.Tables[table].Rows;
                foreach (DataRow existing_row in existing_rows)
                {
                    /*
                    ArrayList row = new ArrayList();
                    foreach (string column in columns)
                    {
                        entry = existing_row[column].ToString();
                        row.Add(entry);
                    }
                    */
                    ArrayList row = new ArrayList(existing_row.ItemArray);
                    data_list.Add(row);
                }
            }
            return data_list;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="table"></param>
        /// <param name="columns"></param>
        /// <param name="values"></param>
        public string Add_versioned_data(
            ref string table, 
            ref ArrayList columns, 
            ref ArrayList values)
        {
            string r_id = "";

            if (data.Tables.Contains(table))
            {
                DataRow new_row = data.Tables[table].NewRow();
                string column;
                string entry;
                for (int i = 0; i < columns.Count; i++)
                {
                    column = columns[i].ToString();
                    bool isDateTime = true;
                    DataColumn col = data.Tables[table].Columns[column];
                    if (values[i] != null && col.DataType == typeof(DateTime))
                    {
                        isDateTime = DateTime.TryParse(values[i].ToString(),out _);
                    }
                    if (values[i] != null && isDateTime)
                    {
                        entry = values[i].ToString();
                        new_row[column] = entry;
                    }
                    else
                    {
                        new_row[column] = DBNull.Value;
                    }
                }
                new_row["deleted"] = 0;
                new_row["version"] = 0;
                new_row["next_version"] = 1;
                new_row["act_version"] = 1;
                new_row["version_datetime"] = DateTime.Now;
                new_row["version_user"] = "";

                int count = 0;
                while (count >= 0 && count < 5)
                {
                    try
                    {
                        r_id = Guid.NewGuid().ToString();
                        new_row["id"] = r_id;
                        new_row["version_connect"] = new_row["id"];
                        data.Tables[table].Rows.Add(new_row);
                        count = -1;
                    }
                    catch (Exception)
                    {
                        count++;
                    }
                }
                if (count > 0)
                {
                    return "";
                }
                State.Database_state save_rc;
                save_rc = Sava_data(table);
                count = 0;
                while (save_rc == State.Database_state.Try_again && count < 5)
                {
                    data.Tables[table].Rows.Remove(new_row);
                    r_id = Guid.NewGuid().ToString();
                    new_row["id"] = r_id;
                    new_row["version_connect"] = new_row["id"];
                    data.Tables[table].Rows.Add(new_row);
                    save_rc = Sava_data(table);
                    count++;
                }
                if (save_rc == State.Database_state.Success)
                {
                    return r_id;
                }
                data.Tables[table].Rows.RemoveAt(data.Tables[table].Rows.Count - 1);
            }
            
            return "";
        }

        public string Add_notversioned_data(
            ref string table,
            ref ArrayList columns,
            ref ArrayList values)
        {
            string r_id = "";

            if (data.Tables.Contains(table))
            {
                DataRow new_row = data.Tables[table].NewRow();
                string column;
                string entry;
                for (int i = 0; i < columns.Count; i++)
                {
                    column = columns[i].ToString();
                    bool isDateTime = true;
                    DataColumn col = data.Tables[table].Columns[column];
                    if (values[i] != null && col.DataType == typeof(DateTime))
                    {
                        isDateTime = DateTime.TryParse(values[i].ToString(), out _);
                    }
                    if (values[i] != null && isDateTime)
                    {
                        entry = values[i].ToString();
                        new_row[column] = entry;
                    }
                    else
                    {
                        new_row[column] = DBNull.Value;
                    }
                }
                new_row["version_datetime"] = DateTime.Now;
                new_row["version_user"] = "";

                int count = 0;
                while (count >= 0 && count < 5)
                {
                    try
                    {
                        r_id = Guid.NewGuid().ToString();
                        new_row["id"] = r_id;
                        data.Tables[table].Rows.Add(new_row);
                        count = -1;
                    }
                    catch (Exception)
                    {
                        count++;
                    }
                }
                if (count > 0)
                {
                    return "";
                }
                State.Database_state save_rc;
                save_rc = Sava_data(table);
                count = 0;
                while (save_rc == State.Database_state.Try_again && count < 5)
                {
                    data.Tables[table].Rows.Remove(new_row);
                    r_id = Guid.NewGuid().ToString();
                    new_row["id"] = r_id;
                    data.Tables[table].Rows.Add(new_row);
                    save_rc = Sava_data(table);
                    count++;
                }

                if (save_rc == State.Database_state.Success)
                {
                    return r_id;
                }
            }

            return "";
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="table"></param>
        /// <param name="columns">first entry must be id(database unique identifier)</param>
        /// <param name="values"></param>
        public string Modify_versioned_data(
            ref string table,
            ref ArrayList columns, 
            ref ArrayList values)
        {
            string r_id = "";

            int next_version = -1;
            string id = values[0].ToString();
            string version_connect = "";
            if (data.Tables.Contains(table))
            {
                foreach (DataRow existing_row in data.Tables[table].Rows)
                {
                    //find row with given id
                    if (existing_row["id"].ToString() == id)
                    {
                        existing_row["act_version"] = 0;
                        next_version = (int)existing_row["next_version"];
                        version_connect = existing_row["version_connect"].ToString();
                        break;
                    }
                }
                DataRow new_row = data.Tables[table].NewRow();

                new_row["deleted"] = 0;
                new_row["version"] = next_version;
                new_row["next_version"] = next_version + 1;
                new_row["act_version"] = 1;
                new_row["version_connect"] = version_connect;
                new_row["version_datetime"] = DateTime.Now;
                new_row["version_user"] = "";

                string column;
                string entry;
                for (int i = 1; i < columns.Count; i++)
                {
                    column = columns[i].ToString();
                    bool isDateTime = true;
                    DataColumn col = data.Tables[table].Columns[column];
                    if (values[i] != null && col.DataType == typeof(DateTime))
                    {
                        isDateTime = DateTime.TryParse(values[i].ToString(), out _);
                    }
                    if (values[i] != null && isDateTime)
                    {
                        entry = values[i].ToString();
                        new_row[column] = entry;
                    }
                    else
                    {
                        new_row[column] = DBNull.Value;
                    }
                }

                int count = 0;
                while (count >= 0 && count < 5)
                {
                    try
                    {
                        r_id = Guid.NewGuid().ToString();
                        new_row["id"] = r_id;
                        data.Tables[table].Rows.Add(new_row);
                        count = -1;
                    }
                    catch (Exception)
                    {
                        count++;
                    }
                }
                if (count > 0)
                {
                    return "";
                }
                State.Database_state save_rc;
                save_rc = Sava_data(table);
                count = 0;
                while (save_rc == State.Database_state.Try_again && count < 5)
                {
                    data.Tables[table].Rows.Remove(new_row);
                    r_id = Guid.NewGuid().ToString();
                    new_row["id"] = r_id;
                    data.Tables[table].Rows.Add(new_row);
                    save_rc = Sava_data(table);
                    count++;
                }

                if (save_rc == State.Database_state.Success)
                {
                    return r_id;
                }
            }

            return "";
        }

        public string Modify_notversioned_data(
            ref string table,
            ref ArrayList columns,
            ref ArrayList values)
        {
            string r_id = values[0].ToString();
            if (data.Tables.Contains(table))
            {
                foreach (DataRow existing_row in data.Tables[table].Rows)
                {
                    //find row with given id
                    if (existing_row["id"].ToString() == r_id)
                    {
                        string column;
                        string entry;
                        for (int i = 1; i < columns.Count; i++)
                        {
                            column = columns[i].ToString();
                            entry = "";
                            bool isDateTime = true;
                            DataColumn col = data.Tables[table].Columns[column];
                            if (values[i] != null && col.DataType == typeof(DateTime))
                            {
                                DateTime dt = new DateTime();
                                isDateTime = DateTime.TryParse(values[i].ToString(), out dt);
                            }
                            if (values[i] != null && isDateTime)
                            {
                                entry = values[i].ToString();
                                existing_row[column] = entry;
                            }
                            else
                            {
                                existing_row[column] = DBNull.Value;
                            }
                        }
                        existing_row["version_datetime"] = DateTime.Now;
                        existing_row["version_user"] = "";

                        if (Sava_data(table) == State.Database_state.Success)
                        {
                        }
                        else
                        {
                            r_id = "";
                        }
                        break;
                    }
                }
            }

            return r_id;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="table"></param>
        /// <param name="ids"></param>
        public void Delete_versioned_data(
            ref string table,
            ref ArrayList ids)
        {
            if (data.Tables.Contains(table))
            {
                string version_connect;
                foreach (string id in ids)
                {
                    version_connect = "";
                    foreach (DataRow existing_row in data.Tables[table].Rows)
                    {
                        //find row with given id
                        if (existing_row["id"].ToString() == id)
                        {
                            version_connect = existing_row["version_connect"].ToString();
                            break;
                        }
                    }
                    foreach (DataRow existing_row in data.Tables[table].Rows)
                    {
                        //all rows with given version_connect
                        if (existing_row["version_connect"].ToString() == version_connect)
                        {
                            existing_row["deleted"] = 1;
                        }
                    }
                }

                if (Sava_data(table) == State.Database_state.Success)
                {
                }
            }

            return;
        }

        internal ArrayList Get_versions_with_id(ref string table, ref ArrayList columns, ref string id)
        {
            ArrayList data_list = new ArrayList();

            string entry;
            string version_connect = "";

            if (data.Tables.Contains(table))
            {
                foreach (DataRow existing_row in data.Tables[table].Rows)
                {
                    //find row with given id
                    if (existing_row["id"].ToString() == id || existing_row["version_connect"].ToString() == id)
                    {
                        version_connect = existing_row["version_connect"].ToString();
                        //ArrayList row = new ArrayList(existing_row.ItemArray);
                        //data_list.Add(row);
                        break;
                    }
                }                
                foreach (DataRow existing_row in data.Tables[table].Rows)
                {
                    //all rows with given version_connect
                    if (existing_row["version_connect"].ToString() == version_connect)
                    {
                        ArrayList row = new ArrayList();
                        foreach (string column in columns)
                        {
                            entry = existing_row[column].ToString();
                            row.Add(entry);
                        }
                        data_list.Add(row);
                    }
                }
            }
            return data_list;
        }

        internal ArrayList Get_values_of_id(ref string table, ref ArrayList columns, ref string id)
        {
            //ArrayList data_list = new ArrayList();
            ArrayList row = new ArrayList();
            if (data.Tables.Contains(table))
            {
                foreach (DataRow existing_row in data.Tables[table].Rows)
                {
                    //find row with given id
                    if (existing_row["id"].ToString() == id)
                    {
                        row = new ArrayList(existing_row.ItemArray);
                        break;
                    }
                }
            }
            return row;
        }

        internal void Expunge_versioned_data(ref string table, ref ArrayList ids)
        {
            if (data.Tables.Contains(table))
            {
                string version_connect;
                foreach (string id in ids)
                {
                    foreach (DataRow existing_row in data.Tables[table].Rows)
                    {
                        //find row with given id
                        //if (existing_row.RowState == DataRowState.Unchanged)
                        //{
                        if (existing_row.RowState != DataRowState.Deleted && existing_row["id"].ToString() == id)
                        {
                            version_connect = existing_row["version_connect"].ToString();
                            existing_row.Delete();
                            break;
                        }
                        //}
                    }
                }

                if (Sava_data(table) == State.Database_state.Success)
                {
                }
            }

            return;
        }

        internal void Expunge_notversioned_data(ref string table, ref ArrayList ids)
        {
            if (data.Tables.Contains(table))
            {
                foreach (string id in ids)
                {
                    foreach (DataRow existing_row in data.Tables[table].Rows)
                    {
                        //find row with given id
                        //if (existing_row.RowState == DataRowState.Unchanged)
                        //{
                        if (existing_row.RowState != DataRowState.Deleted && existing_row["id"].ToString() == id)
                        {
                            //version_connect = existing_row["version_connect"].ToString();
                            existing_row.Delete();
                            break;
                        }
                        //}
                    }
                }

                if (Sava_data(table) == State.Database_state.Success)
                {
                }
            }

            return;
        }

        internal void Set_undelete_versioned_data(ref string table, ref string id)
        {
            string version_connect = "";
            if (data.Tables.Contains(table))
            {
                foreach (DataRow existing_row in data.Tables[table].Rows)
                {
                    //find row with given id
                    if (existing_row["id"].ToString() == id)
                    {
                        version_connect = existing_row["version_connect"].ToString();
                        break;
                    }
                }
                int max_version = -1;
                int version;
                foreach (DataRow existing_row in data.Tables[table].Rows)
                {
                    //find row with given id
                    if (existing_row["version_connect"].ToString() == version_connect)
                    {
                        version = Convert.ToInt32(existing_row["version"].ToString());
                        if (max_version < version)
                        {
                            max_version = version;
                        }
                    }
                }
                foreach (DataRow existing_row in data.Tables[table].Rows)
                {
                    //find row with given id
                    if (existing_row["version_connect"].ToString() == version_connect)
                    {
                        if (existing_row["id"].ToString() == id)
                        {
                            existing_row["deleted"] = 0;
                            existing_row["act_version"] = 1;
                            existing_row["next_version"] = max_version + 1;
                        }
                        else
                        {
                            existing_row["deleted"] = 0;
                            existing_row["act_version"] = 0;
                        }
                    }
                }

                if (Sava_data(table) == State.Database_state.Success)
                {
                }
            }
            return;
        }
    }
}
