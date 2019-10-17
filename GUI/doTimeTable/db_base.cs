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
	/// Summary description for db_base.
	/// </summary>
	public abstract class Db_base
	{
		public int connected;
        public ArrayList tablesCreated = new ArrayList();
        protected ArrayList errorCodes = new ArrayList();

		public Db_base()
		{
			//
			// TODO: Add constructor logic here
			//
			connected = 0;
		}

		public abstract int Connect();

        public abstract int Load_data(ref DataSet data);

        public abstract bool Create_database_and_tables(bool do_update_only);

        public abstract State.Database_state Save_data(ref DataSet data, string table);

        public abstract ArrayList Get_by_select(ref string table, ref string column, ref string where);

        public abstract DataRowCollection Get_by_select(ref string table, ref string where);

        public abstract void SetDBAccessRights();
    }
}
