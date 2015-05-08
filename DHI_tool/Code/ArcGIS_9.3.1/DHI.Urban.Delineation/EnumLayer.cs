// DHI Urban Catchment Delineation
// Copyright (c) 2007, 2010, 2012 DHI Water & Environment, Inc.
// Author: Arnold Engelmann, ahe@dhigroup.com
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program. If not, see <http://www.gnu.org/licenses/>.
//
// All other rights reserved.
using System;
using System.Collections.Generic;
using System.Text;
using ESRI.ArcGIS.Carto;

namespace DHI.Urban.Delineation
{
    public class EnumLayer : List<ILayer>, IEnumLayer
    {
        int _currentIndex = -1;


        #region IEnumLayer Members

        public ILayer Next()
        {
            if (_currentIndex < this.Count - 1)
            {
                return this[++_currentIndex];
            }
            else
            {
                // ArcObjects expects error, not null at end of enumeration.
                throw new System.Runtime.InteropServices.COMException("Index out of range.");
            }
        }

        public void Reset()
        {
            _currentIndex = -1;
        }

        #endregion
    }
}
