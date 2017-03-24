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
using ESRI.ArcGIS.ADF.Serialization;
using ESRI.ArcGIS.ArcMapUI;
using ESRI.ArcGIS.DataSourcesRaster;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Framework;
using ESRI.ArcGIS.Geodatabase;

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
      SetupOp setupOp = null;
      try
      {
        // NOTE: Do not close or dispose BinaryReader, as this will close the Stream
        BinaryReader reader = new BinaryReader(inStrm);
        int version = reader.ReadInt32();

        // NOTE: With change to Add-In architecture, this extension is NOT backwards compatible with versions 1 and 2

        if (version == 3)
        {
          //// Version 3:
          //// Item 1: FeatureClassName: The orphan junction feature class of the Geometric Network for the underground conveyance system.
          //// Item 2: FeatureClassName: The inlet feature class
          //// Item 3: RasterDatasetName: The DEM dataset
          //// Item 4: Boolean: Whether to smooth boundaries
          //// Item 5: Boolean: Whether to include upstream pipe ends
          //// Item 6: Boolean: Whether to exclude downstream pipe ends
          //// Item 7: RasterDatasetName: The flow direction dataset
          //// Item 8: FeatureClassName: The catchment feature class
          //// Item 9: Boolean: Whether to exclude disabled nodes

          setupOp = new SetupOp();
          setupOp.ResultsDirectory = this.MxDocDirectoryName;
          setupOp.ScratchDirectory = this.TemporaryDirectory;

          FeatureClassName networkJunctionName = null;
          PersistenceHelper.Load<FeatureClassName>(inStrm, ref networkJunctionName);
          INetworkClass junctionClass = _SafeOpen(networkJunctionName) as INetworkClass;
          if (junctionClass != null)
            setupOp.GeometricNetwork = junctionClass.GeometricNetwork;

          FeatureClassName inletClassName = null;
          PersistenceHelper.Load<FeatureClassName>(inStrm, ref inletClassName);
          setupOp.InletClass = _SafeOpen(inletClassName) as IFeatureClass;

          RasterDatasetName demName = null;
          PersistenceHelper.Load<RasterDatasetName>(inStrm, ref demName);
          IRasterDataset demDataset = _SafeOpen(demName) as IRasterDataset;
          if (demDataset != null)
            setupOp.DEM = demDataset.CreateDefaultRaster();

          setupOp.SmoothBoundaries = reader.ReadBoolean();
          setupOp.IncludeUpstreamPipeEnds = reader.ReadBoolean();
          setupOp.ExcludeDownstreamPipeEnds = reader.ReadBoolean();

          RasterDatasetName flowDirName = null;
          PersistenceHelper.Load<RasterDatasetName>(inStrm, ref flowDirName);
          IRasterDataset flowDirDataset = _SafeOpen(flowDirName) as IRasterDataset;
          if (flowDirDataset != null)
            setupOp.FlowDirection = flowDirDataset.CreateDefaultRaster();

          FeatureClassName catchmentClassName = null;
          PersistenceHelper.Load<FeatureClassName>(inStrm, ref catchmentClassName);
          setupOp.Catchments = _SafeOpen(catchmentClassName) as IFeatureClass;

          setupOp.ExcludeDisabledNodes = reader.ReadBoolean();
        }

        _setupOp = setupOp;
      }
      catch (Exception ex)
      {
        System.Diagnostics.Debug.WriteLine(ex.GetType().FullName + ": " + ex.Message);
      }
    }

    protected override void OnSave(Stream outStrm)
    {
      if (_setupOp == null)
        return;

      // NOTE: Do not close or dispose BinaryWriter, as this will close the Stream
      BinaryWriter writer = new BinaryWriter(outStrm);
      int version = 3;
      writer.Write(version);

      // ************** VERSION 3 **************

      //// Item 1: FeatureClassName: The orphan junction feature class of the Geometric Network for the underground conveyance system.
      FeatureClassName networkDataset = null;
      if (_setupOp.GeometricNetwork != null)
      {
        networkDataset = ((IDataset)_setupOp.GeometricNetwork.OrphanJunctionFeatureClass).FullName as FeatureClassName;
      }

      PersistenceHelper.Save<FeatureClassName>(outStrm, networkDataset);

      //// Item 2: FeatureClassName: The inlet feature class
      FeatureClassName inletClass = null;
      if (_setupOp.InletClass != null)
      {
        inletClass = ((IDataset)_setupOp.InletClass).FullName as FeatureClassName;
      }

      PersistenceHelper.Save<FeatureClassName>(outStrm, inletClass);

      //// Item 3: RasterDatasetName: The DEM dataset
      RasterDatasetName demDataset = null;
      if (_setupOp.DEM != null)
      {
        IDataset dem = ((IRasterAnalysisProps)_setupOp.DEM).RasterDataset as IDataset;
        if (dem != null)
        {
          demDataset = dem.FullName as RasterDatasetName;
        }
      }

      PersistenceHelper.Save<RasterDatasetName>(outStrm, demDataset);

      //// Item 4: Boolean: Whether to smooth boundaries
      writer.Write(_setupOp.SmoothBoundaries);

      //// Item 5: Boolean: Whether to include upstream pipe ends
      writer.Write(_setupOp.IncludeUpstreamPipeEnds);

      //// Item 6: Boolean: Whether to exclude downstream pipe ends
      writer.Write(_setupOp.ExcludeDownstreamPipeEnds);

      //// Item 7: RasterDatasetName: The flow direction dataset
      RasterDatasetName flowDirDataset = null;
      if (_setupOp.FlowDirection != null)
      {
        IDataset flowDir = ((IRasterAnalysisProps)_setupOp.FlowDirection).RasterDataset as IDataset;
        if (flowDir != null)
        {
          flowDirDataset = flowDir.FullName as RasterDatasetName;
        }
      }

      PersistenceHelper.Save<RasterDatasetName>(outStrm, flowDirDataset);

      //// Item 8: FeatureClassName: The catchment feature class
      FeatureClassName catchmentClass = null;
      if (_setupOp.Catchments != null)
      {
        catchmentClass = ((IDataset)_setupOp.Catchments).FullName as FeatureClassName;
      }

      PersistenceHelper.Save<FeatureClassName>(outStrm, catchmentClass);

      //// Item 9: Boolean: Whether to exclude disabled nodes
      writer.Write(_setupOp.ExcludeDisabledNodes);
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
  }
}
