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

namespace doTimeTable
{
	/// <summary>
	/// Summary description for state.
	/// </summary>
	public class State
	{
		public enum State_type
		{
			Fine = 1,
			Warning = 2,
            Error = 3,
			Fatal = 4
		}
        public enum Database_state
        {
            Success = 0,
            Try_again = 1,
            Failed = 2
        }
		
		public State_type current_state = State_type.Fine;
		public ArrayList last_messages = new ArrayList();

		public State()
		{
			//
			// TODO: Add constructor logic here
			//
		}
	}
}
