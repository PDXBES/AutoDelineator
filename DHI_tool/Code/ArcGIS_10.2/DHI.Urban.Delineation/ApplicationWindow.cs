// DHI Urban Catchment Delineation
// Copyright (c) 2007, 2010, 2012-2014 DHI Water & Environment, Inc.
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
using System.Windows.Forms;

namespace DHI.Urban.Delineation
{
    public class ApplicationWindow : IWin32Window
    {
        private IntPtr _windowHandle;

        public ApplicationWindow(int handle)
        {
            _windowHandle = new IntPtr(handle);
        }

        public IntPtr Handle
        {
            get { return _windowHandle; }
        }
    }
}
