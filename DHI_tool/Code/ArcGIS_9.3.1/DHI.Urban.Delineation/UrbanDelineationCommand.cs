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
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using ESRI.ArcGIS.Framework;
using ESRI.ArcGIS.ADF.BaseClasses;
using ESRI.ArcGIS.ADF.CATIDs;

namespace DHI.Urban.Delineation
{
	[ComVisible(true)]
	[Guid("029EF678-9B99-4905-A270-684DA5F05D74")]
	public class UrbanDelineationCommand : BaseCommand
	{
		#region "Component Category Registration"
		[ComRegisterFunction()]
		static void Reg(string regKey)
		{
			MxCommands.Register(regKey);
		}

		[ComUnregisterFunction()]
		static void Unreg(string regKey)
		{
			MxCommands.Unregister(regKey);
		}
		#endregion

		private IApplication m_pApp = null;
        private ApplicationWindow _appWindow = null;

		public UrbanDelineationCommand()
		{
			m_caption = Properties.Resources.Caption_UrbanDelineationCommand;
			m_toolTip = m_caption;
            m_category = Properties.Resources.Category_UrbanDelineation;
			m_name = this.GetType().FullName;
			m_message = Properties.Resources.Message_UrbanDelineationCommand;
			m_bitmap = Properties.Resources.UrbanDelineationIcon;
		}

		public override void OnCreate(object hook)
		{
			m_pApp = hook as IApplication;
            _appWindow = new ApplicationWindow(m_pApp.hWnd);
		}

		public override void OnClick()
		{
			try
			{
				base.OnClick();

				UrbanDelineationForm pForm = new UrbanDelineationForm(m_pApp);
				pForm.Show(_appWindow);
			}
			catch (Exception ex)
			{
                MessageBox.Show(_appWindow, ex.Message, Properties.Resources.Caption_Error, MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}
	}
}
