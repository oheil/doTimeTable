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
using System.Xml;
using System.Windows.Forms;
using System.Collections;

using System.IO;

namespace doTimeTable
{
	/// <summary>
	/// Summary description for db_xml.
	/// </summary>
	public class Db_xml : Db_base, IDisposable
    {
        //private XmlDataDocument database_xml;
        //private XmlDocument database_xml;
        private DataSet dataSet;
        int connectMsg = 0;

        private string xml_schema = "";
        private string xml_data = "";

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                // dispose managed resources
            }
            // free native resources
            dataSet.Dispose();
        }

        public void Dispose()
        {
            Dispose(true);
        }

        public Db_xml()
		{
			//
			// TODO: Add constructor logic here
			//
		}

		public override int Connect() 
		{
            string newSchema = @"<?xml version=""1.0"" standalone=""yes""?>
<xs:schema id=""doTimeTable"" xmlns="""" xmlns:xs=""http://www.w3.org/2001/XMLSchema"" xmlns:msdata=""urn:schemas-microsoft-com:xml-msdata"">
  <xs:element name=""doTimeTable"" msdata:IsDataSet=""true"" msdata:UseCurrentLocale=""true"">
    <xs:complexType>
      <xs:choice minOccurs=""0"" maxOccurs=""unbounded"">
        <xs:element name=""version"">
          <xs:complexType>
            <xs:sequence>
              <xs:element name=""id"" type=""xs:string"" />
              <xs:element name=""version"" type=""xs:string"" minOccurs=""0"" />
            </xs:sequence>
          </xs:complexType>
        </xs:element>
        <xs:element name=""config"">
          <xs:complexType>
            <xs:sequence>
              <xs:element name=""id"" type=""xs:string"" />
              <xs:element name=""komponente"" type=""xs:string"" minOccurs=""0"" />
              <xs:element name=""config"" type=""xs:string"" minOccurs=""0"" />
              <xs:element name=""config_user"" type=""xs:string"" minOccurs=""0"" />
              <xs:element name=""version_datetime"" type=""xs:dateTime"" minOccurs=""0"" />
              <xs:element name=""version_user"" type=""xs:string"" minOccurs=""0"" />
            </xs:sequence>
          </xs:complexType>
        </xs:element>
      </xs:choice>
    </xs:complexType>
    <xs:unique name=""Constraint1"" msdata:PrimaryKey=""true"">
      <xs:selector xpath="".//version"" />
      <xs:field xpath=""id"" />
    </xs:unique>
    <xs:unique name=""config_Constraint1"" msdata:ConstraintName=""Constraint1"" msdata:PrimaryKey=""true"">
      <xs:selector xpath="".//config"" />
      <xs:field xpath=""id"" />
    </xs:unique>
  </xs:element>
</xs:schema>";
            string newData = @"<?xml version=""1.0"" standalone=""yes""?>
<doTimeTable>
  <version>
    <id>00000000-0000-0000-0000-000000000000</id>
    <version>0.1.0.0</version>
  </version>
</doTimeTable>";

            string log_output;

            xml_schema = "";
            if (!Form1.config.Get_xml_config("database_schema", ref xml_schema))
            {
                System.Xml.XmlElement database_schema = Form1.config.config_xml.CreateElement("database_schema");
                database_schema.InnerXml = Form1.applicationDir + Path.DirectorySeparatorChar + "db_schema.xml";
                System.Xml.XmlElement root = Form1.config.config_xml.DocumentElement;
                root.AppendChild(database_schema);
                Form1.config.Save_config_file();
            }
            Form1.config.Get_xml_config("database_schema", ref xml_schema);

            xml_data = "";
            if (!Form1.config.Get_xml_config("database_data", ref xml_data))
            {
                System.Xml.XmlElement database_data = Form1.config.config_xml.CreateElement("database_data");
                database_data.InnerXml = Form1.applicationDir + Path.DirectorySeparatorChar + "db_data.xml";
                System.Xml.XmlElement root = Form1.config.config_xml.DocumentElement;
                root.AppendChild(database_data);
                Form1.config.Save_config_file();
            }
            Form1.config.Get_xml_config("database_data", ref xml_data);

            //string xml_schema = Form1.config.config_xml.DocumentElement["database_connection"].InnerXml.Split(';')[0];
            //string xml_data = Form1.config.config_xml.DocumentElement["database_connection"].InnerXml.Split(';')[1];

            //database_xml = new XmlDataDocument();
            //database_xml = new XmlDocument();
            dataSet = new DataSet();
            try
            {
                dataSet.ReadXmlSchema(xml_schema);
            }
            catch (Exception)
            {
                log_output = "loading xml database schema " + xml_schema + " failed, creating it.";
                Form1.logWindow.Write_to_log(ref log_output);
                try
                {
                    File.WriteAllText(xml_schema, newSchema);
                    dataSet.ReadXmlSchema(xml_schema);
                }
                catch (Exception e1)
                {
                    log_output = "creating " + xml_schema + " failed:";
                    Form1.logWindow.Write_to_log(ref log_output);
                    log_output = e1.ToString();
                    Form1.logWindow.Write_to_log(ref log_output);
                    xml_schema = Form1.applicationDir + Path.DirectorySeparatorChar + "db_schema.xml";
                    log_output = "setting xml database schema to " + xml_schema;
                    Form1.logWindow.Write_to_log(ref log_output);
                    try
                    {
                        dataSet.ReadXmlSchema(xml_schema);
                    }
                    catch (Exception)
                    {
                        log_output = "loading xml database schema " + xml_schema + " failed, creating it.";
                        Form1.logWindow.Write_to_log(ref log_output);
                        try
                        {
                            File.WriteAllText(xml_schema, newSchema);
                            dataSet.ReadXmlSchema(xml_schema);
                        }
                        catch (Exception e2)
                        {
                            log_output = "creating " + xml_schema + " failed:";
                            Form1.logWindow.Write_to_log(ref log_output);
                            log_output = e2.ToString();
                            Form1.logWindow.Write_to_log(ref log_output);
                            return 1;
                        }
                    }
                }
            }

            try
            {
                dataSet.ReadXml(xml_data);
            }
            catch (Exception)
            {
                log_output = "loading xml database  " + xml_data + " failed, creating it.";
                Form1.logWindow.Write_to_log(ref log_output);
                try
                {
                    File.WriteAllText(xml_data, newData);
                    dataSet.ReadXml(xml_data);
                }
                catch (Exception e1)
                {
                    log_output = "creating " + xml_data + " failed:";
                    Form1.logWindow.Write_to_log(ref log_output);
                    log_output = e1.ToString();
                    Form1.logWindow.Write_to_log(ref log_output);
                    xml_data = Form1.applicationDir + Path.DirectorySeparatorChar + "db_data.xml";
                    log_output = "setting xml database to " + xml_data;
                    Form1.logWindow.Write_to_log(ref log_output);
                    try
                    {
                        dataSet.ReadXml(xml_data);
                    }
                    catch (Exception)
                    {
                        log_output = "loading xml database " + xml_data + " failed, creating it.";
                        Form1.logWindow.Write_to_log(ref log_output);
                        try
                        {
                            File.WriteAllText(xml_data, newData);
                            dataSet.ReadXml(xml_data);
                        }
                        catch (Exception e2)
                        {
                            log_output = "creating " + xml_data + " failed:";
                            Form1.logWindow.Write_to_log(ref log_output);
                            log_output = e2.ToString();
                            Form1.logWindow.Write_to_log(ref log_output);
                            return 1;
                        }
                    }
                }
            }

            connectMsg++;
            if (connectMsg > 1)
            {
                log_output = "XML database connection successful";
                Form1.logWindow.Write_to_log(ref log_output);
            }

            connected = 1;
            
            return 0;
		}

        public override ArrayList Get_by_select(ref string table, ref string column, ref string where)
        {
            ArrayList data_list = new ArrayList();
            return data_list;
        }

        public override DataRowCollection Get_by_select(ref string table, ref string where)
        {
            return null;
        }

        public override int Load_data(ref DataSet data) 
		{
            if (connected == 1)
            {
                data.Dispose();
                //data = database_xml.DataSet;
                data = dataSet;
            }

            //set database version
            int dbversion = 0;
            string[] v;
            if (data.Tables["version"].Rows.Count > 0)
            {
                DataRow[] data_list = data.Tables["version"].Select();
                foreach(DataRow row in data_list)
                {                    
                    string dbversion_tmp = row.ItemArray[1].ToString();
                    v = dbversion_tmp.Split('.');
                    if (v.Length >= 1)
                    {
                        if (Convert.ToInt32(v[0] + v[1]) > dbversion)
                        {
                            dbversion = Convert.ToInt32(v[0] + v[1]);
                            Form1.database.version = v[0] + "." + v[1];
                        }
                    }
                }
            }

            return 0;
		}
        public override bool Create_database_and_tables(bool do_update_only)
        {
            return true;
        }
        public override State.Database_state Save_data(ref DataSet data, string table)
        {
            if (connected == 1)
            {
                //string xml_schema = Form1.config.config_xml.DocumentElement["database_schema"].InnerXml;
                //string xml_data = Form1.config.config_xml.DocumentElement["database_data"].InnerXml;

                Application.UseWaitCursor = true;
                Cursor.Current = Cursors.WaitCursor;

                dataSet.WriteXmlSchema(xml_schema);
                dataSet.WriteXml(xml_data);

                Cursor.Current = Cursors.Default;
                Application.UseWaitCursor = false;
            }

            return 0;
        }
        public override void SetDBAccessRights()
        {
        }

	}
}
