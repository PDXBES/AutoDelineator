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
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using ESRI.ArcGIS.Framework;
using ESRI.ArcGIS.ADF.BaseClasses;
using ESRI.ArcGIS.ADF.CATIDs;

namespace DHI.Urban.Delineation
{
    [ComVisible(true)]
	[Guid("C68DA727-BA8E-4baf-A539-B647D796C093")]
	public sealed class SetupCommand : BaseCommand
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

		private IApplication _application = null;
        private ApplicationWindow _appWindow = null;

		public SetupCommand()
		{
			m_caption = Properties.Resources.Caption_SetupCommand;
			m_toolTip = m_caption;
            m_category = Properties.Resources.Category_UrbanDelineation;
			m_name = this.GetType().FullName;
			m_message = Properties.Resources.Message_SetupCommand;
			m_bitmap = Properties.Resources.SetupIcon;
		}

		public override void OnCreate(object hook)
		{
			_application = hook as IApplication;
            _appWindow = new ApplicationWindow(_application.hWnd);
		}

		public override void OnClick()
		{
			try
			{
				base.OnClick();

				SetupForm pForm = new SetupForm(_application);
				pForm.Show(_appWindow);
			}
			catch (Exception ex)
			{
                MessageBox.Show(_appWindow, ex.Message, Properties.Resources.Caption_Error, MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}
	}
}
