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
using System.Runtime.InteropServices;
using ESRI.ArcGIS.Framework;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.ArcMapUI;

namespace DHI.Urban.Delineation
{
  public class UrbanDelineationExtension : ESRI.ArcGIS.Desktop.AddIns.Extension
  {
    private static UrbanDelineationExtension _extension;
    private SetupOp _setupOp = null;
    private IMxDocument _document = null;

    /// <summary>
    /// A reference to the instantiated Urban Catchment Delienation Extension.
    /// </summary>
    public static UrbanDelineationExtension Extension
    {
      get { return _extension; }
    }

    public static int ReleaseComObject(object comObject)
    {
      if (comObject != null && Marshal.IsComObject(comObject))
      {
        return Marshal.ReleaseComObject(comObject);
      }
      else
      {
        return 0;
      }
    }

    public static string GetDatasetPath(IDataset dataset)
    {
      if (dataset != null)
      {
        string workspacePath = dataset.Workspace.PathName;

        // Check for feature dataset
        IFeatureDataset featureDataset = null;
        if (dataset is IFeatureClass)
        {
          featureDataset = ((IFeatureClass)dataset).FeatureDataset;
        }
        else if (dataset is IGraph)
        {
          featureDataset = ((IGraph)dataset).FeatureDataset;
        }

        if (featureDataset != null)
        {
          workspacePath = System.IO.Path.Combine(workspacePath, featureDataset.BrowseName);
        }

        return System.IO.Path.Combine(workspacePath, dataset.Name);
      }
      else
      {
        return null;
      }
    }

    /// <summary>
    /// Delineation setup information and functionality.
    /// </summary>
    public SetupOp Setup
    {
      get
      {
        if (_setupOp == null)
        {
          _setupOp = new SetupOp();
          _setupOp.ResultsDirectory = this.MxDocDirectoryName;
          _setupOp.ScratchDirectory = this.TemporaryDirectory;
        }
        return _setupOp;
      }
    }

    /// <summary>
    /// Gets the full path name of the current map document
    /// </summary>
    public string MxDocFileName
    {
      get
      {
        ITemplates appTemplates = ArcMap.Application.Templates;
        return appTemplates.get_Item(appTemplates.Count - 1);
      }
    }

    public string MxDocDirectoryName
    {
      get
      {
        return System.IO.Path.GetDirectoryName(this.MxDocFileName);
      }
    }

    public string TemporaryDirectory
    {
      get
      {
        string documents = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        string tempPath = System.IO.Path.Combine(documents, "temp");
        return System.IO.Path.Combine(tempPath, "UrbanDelineation");
      }
    }

    protected override void OnStartup()
    {
      _document = ArcMap.Document;
      if (_document != null)
      {
        IDocumentEvents_Event documentEvents = _document as IDocumentEvents_Event;
        documentEvents.NewDocument += new IDocumentEvents_NewDocumentEventHandler(_DocumentEvents_NewDocument);
        documentEvents.OpenDocument += new IDocumentEvents_OpenDocumentEventHandler(_DocumentEvents_OpenDocument);
        documentEvents.CloseDocument += new IDocumentEvents_CloseDocumentEventHandler(_DocumentEvents_CloseDocument);
      }

      _extension = this;
    }

    protected override void OnShutdown()
    {
      if (_setupOp != null)
      {
        _setupOp.Dispose();
        _setupOp = null;
      }
    }

    private void _DocumentEvents_CloseDocument()
    {
      _ClearProject();
    }

    private void _DocumentEvents_OpenDocument()
    {
      // Do not clear when opening a document, since it may have a saved setup
    }

    private void _DocumentEvents_NewDocument()
    {
      _ClearProject();
    }

    private void _ClearProject()
    {
      // A null setup object indicates this document has not been used for catchment delineation
      _setupOp = null;
    }

    //TODO: Implement load and save

    protected override void OnLoad(Stream inStrm)
    {
      base.OnLoad(inStrm);

#warning Not Implemented
    }

    protected override void OnSave(Stream outStrm)
    {
      base.OnSave(outStrm);

#warning Not Implemented
    }
  }
}
