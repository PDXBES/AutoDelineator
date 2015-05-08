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
using ESRI.ArcGIS.ADF.CATIDs;
using ESRI.ArcGIS.ArcMapUI;
using ESRI.ArcGIS.DataSourcesRaster;
using ESRI.ArcGIS.Framework;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.esriSystem;

////using ESRI.ArcGIS.Carto;
////using ESRI.ArcGIS.ArcMapUI;

namespace DHI.Urban.Delineation
{
  [ComVisible(true)]
  [Guid("4F3ADEE2-2CE7-475c-8636-F1155E61ECA4")]
  public class UrbanDelineationExtension : IExtension, IPersistVariant
  {
    private static UrbanDelineationExtension _extension;
    private SetupOp _setupOp = null;
    private IApplication _application = null;
    private IMxDocument _document = null;

    #region "Component Category Registration"
    [ComRegisterFunction()]
    [ComVisible(false)]
    static void Reg(string regKey)
    {
      MxExtension.Register(regKey);
    }

    [ComUnregisterFunction()]
    [ComVisible(false)]
    static void Unreg(string regKey)
    {
      MxExtension.Unregister(regKey);
    }
    #endregion

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
        ITemplates appTemplates = _application.Templates;
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

    #region IExtension Members

    public string Name
    {
      get { return this.GetType().Name; }
    }

    public void Shutdown()
    {
      if (_setupOp != null)
      {
        _setupOp.Dispose();
        _setupOp = null;
      }
    }

    public void Startup(ref object initializationData)
    {
      _application = initializationData as IApplication;
      if (_application == null)
        return;

      _document = _application.Document as IMxDocument;
      if (_document != null)
      {
        IDocumentEvents_Event documentEvents = _document as IDocumentEvents_Event;
        documentEvents.NewDocument += new IDocumentEvents_NewDocumentEventHandler(_DocumentEvents_NewDocument);
        documentEvents.OpenDocument += new IDocumentEvents_OpenDocumentEventHandler(_DocumentEvents_OpenDocument);
        documentEvents.CloseDocument += new IDocumentEvents_CloseDocumentEventHandler(_DocumentEvents_CloseDocument);
      }

      _extension = this;
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

    #endregion

    #region IPersistVariant Members

    public UID ID
    {
      get
      {
        UID uid = new UID();
        uid.Value = this.GetType().GUID.ToString("B");
        return uid;
      }
    }

    public void Load(IVariantStream stream)
    {
      if (System.IO.Path.GetExtension(this.MxDocFileName).ToUpper() == ".MXT")
        return;

      SetupOp setupOp = null;
      try
      {
        object firstValue = stream.Read();
        if (firstValue != null)
        {
          int version = 0;
          if (firstValue is string && (string)firstValue == "Version")
            version = (int)stream.Read();

          if (version >= 1)
          {
            setupOp = new SetupOp();
            setupOp.ResultsDirectory = this.MxDocDirectoryName;
            setupOp.ScratchDirectory = this.TemporaryDirectory;

            // Version 1:
            // Item 1: geometric network (saved as orphan junction feature class)
            // Item 2: inlet featureclass
            // Item 3: dem dataset
            // Item 4: boolean--whether to smooth boundaries
            // Item 5: boolean--whether to include upstream pipe ends
            // Item 6: boolean--whether to exclude downstream pipe ends
            // Item 7: flow direction dataset
            // Item 8: catchment feature class

            IName junctionClassName = stream.Read() as IName;
            INetworkClass junctionClass = _SafeOpen(junctionClassName) as INetworkClass;
            if (junctionClass != null)
              setupOp.GeometricNetwork = junctionClass.GeometricNetwork;

            IName inletClassName = stream.Read() as IName;
            IFeatureClass inletClass = _SafeOpen(inletClassName) as IFeatureClass;
            if (inletClass != null)
              setupOp.InletClass = inletClass;

            IName demName = stream.Read() as IName;
            IRasterDataset demDataset = _SafeOpen(demName) as IRasterDataset;
            if (demDataset != null)
              setupOp.DEM = demDataset.CreateDefaultRaster();

            setupOp.SmoothBoundaries = (bool)stream.Read();
            setupOp.IncludeUpstreamPipeEnds = (bool)stream.Read();
            setupOp.ExcludeDownstreamPipeEnds = (bool)stream.Read();

            IName flowDirName = stream.Read() as IName;
            IRasterDataset flowDirDataset = _SafeOpen(flowDirName) as IRasterDataset;
            if (flowDirDataset != null)
              setupOp.FlowDirection = flowDirDataset.CreateDefaultRaster();

            IName catchmentClassName = stream.Read() as IName;
            IFeatureClass catchmentClass = _SafeOpen(catchmentClassName) as IFeatureClass;
            if (catchmentClass != null)
              setupOp.Catchments = catchmentClass;

            if (version >= 2)
            {
              // Version 2:
              // Item 9: boolean--whether to exclude disabled nodes

              setupOp.ExcludeDisabledNodes = (bool)stream.Read();
            }
          }
        }

        _setupOp = setupOp;
      }
      catch (Exception ex)
      {
        System.Diagnostics.Debug.WriteLine(ex.GetType().FullName + ": " + ex.Message);
      }
    }

    private object _SafeOpen(IName datasetName)
    {
      // Returns null if opening dataset fails (e.g. dataset no longer exists)
      object result = null;
      if (datasetName != null)
      {
        try
        {
          result = datasetName.Open();
        }
        catch (COMException comex)
        {
          // Do nothing
          System.Diagnostics.Debug.WriteLine(string.Format("{0}: {1} ({2})", comex.GetType().FullName, comex.Message, comex.ErrorCode));
        }
      }
      return result;
    }

    public void Save(IVariantStream stream)
    {
      if (System.IO.Path.GetExtension(this.MxDocFileName).ToUpper() == ".MXT")
        return;

      if (_setupOp != null)
      {
        stream.Write("Version");
        stream.Write(2);

        // Item 1: geometric network (saved as orphan junction feature class)
        if (_setupOp.GeometricNetwork != null)
        {
          stream.Write(((IDataset)_setupOp.GeometricNetwork.OrphanJunctionFeatureClass).FullName);
        }
        else
        {
          stream.Write(null);
        }

        // Item 2: inlet featureclass
        if (_setupOp.InletClass != null)
        {
          stream.Write(((IDataset)_setupOp.InletClass).FullName);
        }
        else
        {
          stream.Write(null);
        }

        // Item 3: dem dataset
        if (_setupOp.DEM != null)
        {
          IDataset demDataset = ((IRasterAnalysisProps)_setupOp.DEM).RasterDataset as IDataset;
          if (demDataset != null)
          {
            stream.Write(demDataset.FullName);
          }
          else
          {
            stream.Write(null);
          }
        }
        else
        {
          stream.Write(null);
        }

        // Item 4: boolean--whether to smooth boundaries
        stream.Write(_setupOp.SmoothBoundaries);

        // Item 5: boolean--whether to include upstream pipe ends
        stream.Write(_setupOp.IncludeUpstreamPipeEnds);

        // Item 6: boolean--whether to exclude downstream pipe ends
        stream.Write(_setupOp.ExcludeDownstreamPipeEnds);

        // Item 7: flow direction dataset
        if (_setupOp.FlowDirection != null)
        {
          IDataset flowDirDataset = ((IRasterAnalysisProps)_setupOp.FlowDirection).RasterDataset as IDataset;
          if (flowDirDataset != null)
          {
            stream.Write(flowDirDataset.FullName);
          }
          else
          {
            stream.Write(null);
          }
        }
        else
        {
          stream.Write(null);
        }

        // Item 8: catchment feature class
        if (_setupOp.Catchments != null)
        {
          stream.Write(((IDataset)_setupOp.Catchments).FullName);
        }
        else
        {
          stream.Write(null);
        }

        // Item 9: boolean--whether to exclude disabled nodes
        stream.Write(_setupOp.ExcludeDisabledNodes);
      }
    }

    #endregion
  }
}
