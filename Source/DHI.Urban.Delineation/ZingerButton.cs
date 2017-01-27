// DHI Urban Catchment Delineation
// Copyright (c) 2007, 2010, 2012-2017 DHI Water & Environment, Inc.
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
using System.IO;
using System.Windows.Forms;

namespace DHI.Urban.Delineation
{
  public class ZingerButton : ESRI.ArcGIS.Desktop.AddIns.Button
  {
    private ApplicationWindow _appWindow;

    public ZingerButton()
    {
      _appWindow = new ApplicationWindow(ArcMap.Application.hWnd);
    }

    protected override void OnClick()
    {
      try
      {
        base.OnClick();

        ZingerForm form = new ZingerForm();
        form.Show(_appWindow);
      }
      catch (Exception ex)
      {
        MessageBox.Show(_appWindow, ex.Message, Properties.Resources.Caption_Error, MessageBoxButtons.OK, MessageBoxIcon.Error);
      }
    }

    protected override void OnUpdate()
    {
      Enabled = ArcMap.Application != null;
    }
  }
}
