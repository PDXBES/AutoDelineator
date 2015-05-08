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
using System.Drawing;
using System.Runtime.InteropServices;
using ESRI.ArcGIS.ADF.BaseClasses;
using ESRI.ArcGIS.ADF.CATIDs;
using ESRI.ArcGIS.Framework;
using ESRI.ArcGIS.ArcMapUI;
using ESRI.ArcGIS.GeoAnalyst;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.DataSourcesRaster;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.SpatialAnalyst;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Carto;
using System.Windows.Forms;
using System.IO;

namespace DHI.Urban.Delineation
{
  /// <summary>
  /// An ArcMap tool for tracing the flow across a flow direction grid.
  /// </summary>
  [ComVisible(true)]
  [Guid("4fe3087a-73ef-42a5-9231-fe57392cfcc6")]
  public sealed class FlowTracingTool : BaseTool
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
      MxCommands.Register(regKey);

    }
    /// <summary>
    /// Required method for ArcGIS Component Category unregistration -
    /// Do not modify the contents of this method with the code editor.
    /// </summary>
    private static void ArcGISCategoryUnregistration(Type registerType)
    {
      string regKey = string.Format("HKEY_CLASSES_ROOT\\CLSID\\{{{0}}}", registerType.GUID);
      MxCommands.Unregister(regKey);

    }

    #endregion
    #endregion

    private const string ELEMENT_NAME = "DHI.Urban.Delineation.FlowTracingTool";
    private IApplication _application;
    private ApplicationWindow _appWindow = null;

    public FlowTracingTool()
    {
      base.m_category = Properties.Resources.Category_UrbanDelineation;
      base.m_caption = Properties.Resources.Caption_FlowTracingTool;
      base.m_message = Properties.Resources.Message_FlowTracingTool;
      base.m_toolTip = m_caption;
      base.m_name = this.GetType().FullName;
      base.m_bitmap = Properties.Resources.FlowTracingIcon;
      base.m_cursor = new System.Windows.Forms.Cursor(new MemoryStream(Properties.Resources.FlowTracingCursor));
    }

    #region Overriden Class Methods

    /// <summary>
    /// Occurs when this tool is created
    /// </summary>
    /// <param name="hook">Instance of the application</param>
    public override void OnCreate(object hook)
    {
      _application = hook as IApplication;
      _appWindow = new ApplicationWindow(_application.hWnd);

      //Disable if it is not ArcMap
      if (hook is IMxApplication)
        base.m_enabled = true;
      else
        base.m_enabled = false;
    }

    /// <summary>
    /// Occurs when this tool is clicked
    /// </summary>
    public override void OnClick()
    {
    }

    public override void OnMouseDown(int Button, int Shift, int X, int Y)
    {
    }

    public override void OnMouseMove(int Button, int Shift, int X, int Y)
    {
    }

    public override void OnMouseUp(int Button, int Shift, int X, int Y)
    {
      IMxDocument document = null;
      try
      {
        // Get user point
        document = (IMxDocument)_application.Document;
        IMxApplication mxApplication = _application as IMxApplication;
        IPoint mapPoint = mxApplication.Display.DisplayTransformation.ToMapPoint(X, Y);

        // Get flow direction
        SetupOp setupOp = UrbanDelineationExtension.Extension.Setup;
        IRaster flowDir = setupOp.FlowDirection;
        if (flowDir == null)
        {
          MessageBox.Show(_appWindow, Properties.Resources.Error_NoFlowDirection, Properties.Resources.Category_UrbanDelineation);
          return;
        }
        IGeoDataset flowDirDataset = (IGeoDataset)flowDir;

        // Process point
        IPointCollection pointCollection = new MultipointClass();
        ((IGeometry)pointCollection).SpatialReference = flowDirDataset.SpatialReference;
        mapPoint.Project(flowDirDataset.SpatialReference);
        IPoint[] points = new IPoint[] { mapPoint };
        pointCollection.AddPoints(1, ref points[0]);

        // Get flow path geometry
        IPolyline flowPath = null;
        string outputDir = null;
        IDistanceOp distanceOp = new RasterDistanceOpClass();
        try
        {
          IEnvelope outputExtent = _GetSurfaceOutputExtent(mapPoint);
          if (outputExtent == null)
          {
            setupOp.SetAnalysisEnvironment((IRasterAnalysisEnvironment)distanceOp);
          }
          else
          {
            // Expand to ensure full boundary of watershed is included.
            double cellWidth = ((IRasterAnalysisProps)setupOp.FlowDirection).PixelWidth;
            double cellHeight = ((IRasterAnalysisProps)setupOp.FlowDirection).PixelHeight;
            outputExtent.Expand(cellWidth * 3.0, cellHeight * 3.0, false);

            setupOp.SetAnalysisEnvironment((IRasterAnalysisEnvironment)distanceOp, outputExtent);
          }

          // The RasterDistanceOpClass does not properly clean up after itself, so we have to do it ourselves
          // To do this, we need to track the output directory and delete it, since we don't have direct access to the
          // dataset that is left behind. To delete the directory, it must contain only the temp files generated by
          // the RasterDistanceOpClass and no others, so we create one here. The directory is then deleted in the
          // finally clause.
          ((IRasterAnalysisEnvironment)distanceOp).OutWorkspace = _GetUniqueOutputWorkspace();
          outputDir = ((IRasterAnalysisEnvironment)distanceOp).OutWorkspace.PathName;

          IGeometryCollection flowPaths = distanceOp.CostPathAsPolyline(pointCollection, flowDirDataset, flowDirDataset);
          if (flowPaths.GeometryCount > 0)
            flowPath = flowPaths.get_Geometry(0) as IPolyline;
        }
        finally
        {
          UrbanDelineationExtension.ReleaseComObject(distanceOp);
          try
          {
            if (Directory.Exists(outputDir))
              Directory.Delete(outputDir, true);
          }
          catch (Exception ex)
          {
            System.Diagnostics.Debug.WriteLine("FlowTracingTool.OnMouseUp: " + ex.GetType().FullName + ": " + ex.Message);
          }
        }

        // Add flow path to map
        if (flowPath != null && !flowPath.IsEmpty)
        {
          // Create graphic element
          flowPath.SpatialReference = flowDirDataset.SpatialReference;
          // Weed removes points that are in-line since flow path is generated with vertices for every cell, even if they are exactly in-line
          ((IPolycurve)flowPath).Weed(0.001);
          IElement flowElement = new LineElementClass();
          ((IElementProperties3)flowElement).Name = ELEMENT_NAME;
          flowElement.Geometry = flowPath;

          // Set color
          ILineSymbol lineSymbol = ((ILineElement)flowElement).Symbol;
          IRgbColor color = lineSymbol.Color as IRgbColor;
          Color systemColor = Color.Red;
          color.Blue = systemColor.B;
          color.Red = systemColor.R;
          color.Green = systemColor.G;
          lineSymbol.Color = color;
          ((ILineElement)flowElement).Symbol = lineSymbol;

          ((IGraphicsContainer)document.FocusMap.BasicGraphicsLayer).AddElement(flowElement, 0);
          ((IDocumentDirty)document).SetDirty();

          document.ActiveView.Refresh();
        }
      }
      catch (Exception ex)
      {
        MessageBox.Show(_appWindow, ex.Message, Properties.Resources.Caption_Error, MessageBoxButtons.OK, MessageBoxIcon.Error);
      }
      finally
      {
        UrbanDelineationExtension.ReleaseComObject(document);
      }
    }

    private IWorkspace _GetUniqueOutputWorkspace()
    {
      IWorkspaceFactory pWSFactory = new RasterWorkspaceFactoryClass();

      string documents = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
      string uniqueDirectory = System.IO.Path.Combine(documents, "temp");
      uniqueDirectory = System.IO.Path.Combine(uniqueDirectory, Guid.NewGuid().ToString("D"));
      Directory.CreateDirectory(uniqueDirectory);

      return pWSFactory.OpenFromFile(uniqueDirectory, 0);
    }

    private IEnvelope _GetSurfaceOutputExtent(IPoint mapPoint)
    {
      IEnvelope extent = null;

      SetupOp setupOp = UrbanDelineationExtension.Extension.Setup;
      if (setupOp.Catchments != null)
      {
        // Find all inlet catchments that intersect any source feature and union envelopes
        IFeatureCursor catchCursor = setupOp.Catchments.Search(null, false);
        try
        {
          IFeature catchment = catchCursor.NextFeature();
          while (catchment != null)
          {
            try
            {
              IRelationalOperator relationOp = (IRelationalOperator)catchment.Shape;
              if (!relationOp.Disjoint(mapPoint))
              {
                extent = new EnvelopeClass();
                catchment.Extent.QueryEnvelope(extent);
                break;
              }
            }
            finally
            {
              UrbanDelineationExtension.ReleaseComObject(catchment);
            }
            catchment = catchCursor.NextFeature();
          }
        }
        finally
        {
          UrbanDelineationExtension.ReleaseComObject(catchCursor);
        }
      }

      return extent;
    }
    #endregion
  }
}
