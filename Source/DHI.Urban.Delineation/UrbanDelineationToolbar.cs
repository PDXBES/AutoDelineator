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
using System.Runtime.InteropServices;
using ESRI.ArcGIS.ADF.CATIDs;
using ESRI.ArcGIS.ADF.BaseClasses;

namespace DHI.Urban.Delineation
{
	/// <summary>
	/// Summary description for UrbanDelineationToolbar.
	/// </summary>
	[ComVisible(true)]
	[Guid("b79dd6d6-4f58-4a42-852a-1a00343dde31")]
	public sealed class UrbanDelineationToolbar : BaseToolbar
	{
		#region COM Registration Function(s)
		[ComRegisterFunction()]
		[ComVisible(false)]
		static void RegisterFunction(Type registerType)
		{
			// Required for ArcGIS Component Category Registrar support
			ArcGISCategoryRegistration(registerType);
		}

		[ComUnregisterFunction()]
		[ComVisible(false)]
		static void UnregisterFunction(Type registerType)
		{
			// Required for ArcGIS Component Category Registrar support
			ArcGISCategoryUnregistration(registerType);
		}

		#region ArcGIS Component Category Registrar generated code
		/// <summary>
		/// Required method for ArcGIS Component Category registration -
		/// Do not modify the contents of this method with the code editor.
		/// </summary>
		private static void ArcGISCategoryRegistration(Type registerType)
		{
			string regKey = string.Format("HKEY_CLASSES_ROOT\\CLSID\\{{{0}}}", registerType.GUID);
			MxCommandBars.Register(regKey);
		}
		/// <summary>
		/// Required method for ArcGIS Component Category unregistration -
		/// Do not modify the contents of this method with the code editor.
		/// </summary>
		private static void ArcGISCategoryUnregistration(Type registerType)
		{
			string regKey = string.Format("HKEY_CLASSES_ROOT\\CLSID\\{{{0}}}", registerType.GUID);
			MxCommandBars.Unregister(regKey);
		}

		#endregion
		#endregion

		public UrbanDelineationToolbar()
		{
			AddItem(typeof(SetupCommand));
			AddItem(typeof(UrbanDelineationCommand));
            AddItem(typeof(FlowTracingTool));
            AddItem(typeof(ZingerCommand));

			//AddItem("esriArcMapUI.ZoomInTool");
			//BeginGroup(); //Separator
			//AddItem("{FBF8C3FB-0480-11D2-8D21-080009EE4E51}", 1); //undo command
			//AddItem(new Guid("FBF8C3FB-0480-11D2-8D21-080009EE4E51"), 2); //redo command
		}

		public override string Caption
		{
			get
			{
				return "Urban Catchment Delineation";
			}
		}
		public override string Name
		{
			get
			{
				return "UrbanDelineationToolbar";
			}
		}
	}
}
