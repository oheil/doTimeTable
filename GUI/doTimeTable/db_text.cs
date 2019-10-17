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
	/// Summary description for db_text.
	/// </summary>
	public class Db_text : Db_base
	{
		public Db_text()
		{
			//
			// TODO: Add constructor logic here
			//
		}

		public override int Connect() 
		{
			Console.WriteLine( "connected" );

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
			return 0;
		}
        public override bool Create_database_and_tables(bool do_update_only)
        {
            return true;
        }
        public override State.Database_state Save_data(ref DataSet data, string table)
        {
            return 0;
        }
        public override void SetDBAccessRights()
        {
        }

	}
}
