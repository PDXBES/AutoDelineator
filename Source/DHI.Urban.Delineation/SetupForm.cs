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
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using ESRI.ArcGIS.ArcMapUI;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Framework;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.DataSourcesRaster;

namespace DHI.Urban.Delineation
{
  public partial class SetupForm : Form
  {
    private IMxDocument _document;
    private IActiveView _activeView;

    public SetupForm()
    {
      _document = ArcMap.Document;
      _activeView = _document.ActiveView;

      InitializeComponent();
    }

    private void SetupForm_Load(object sender, EventArgs e)
    {
      if (_document != null)
      {
        _SetupDocumentEvents();
      }

      _UpdateComboBoxes();

      // Retrieve saved layers
      List<ILayer> layers = _GetLayers(_document);

      SetupOp setupOp = UrbanDelineationExtension.Extension.Setup;
      if (setupOp.GeometricNetwork != null)
        cbxNetwork.SelectedValue = setupOp.GeometricNetwork;
      ILayer inletLayer = _FindFeatureLayer(setupOp.InletClass, layers);
      if (inletLayer != null)
        cbxInlets.SelectedValue = inletLayer;
      chkSmooth.Checked = setupOp.SmoothBoundaries;
      ILayer demLayer = _FindRasterLayer(setupOp.DEM, layers);
      if (demLayer != null)
        cbxDEM.SelectedValue = demLayer;
      chkIncludeUp.Checked = setupOp.IncludeUpstreamPipeEnds;
      chkExcludeDisabled.Checked = setupOp.ExcludeDisabledNodes;
      chkExcludeDown.Checked = setupOp.ExcludeDownstreamPipeEnds;
      ILayer flowDirLayer = _FindRasterLayer(setupOp.FlowDirection, layers);
      if (flowDirLayer != null)
        cbxFlowDirGrid.SelectedValue = flowDirLayer;
      ILayer catchmentLayer = _FindFeatureLayer(setupOp.Catchments, layers);
      if (catchmentLayer != null)
        cbxCatchments.SelectedValue = catchmentLayer;

      _UpdateButtons();
    }

    protected override void OnClosed(EventArgs e)
    {
      _RemoveDocumentEvents();
      base.OnClosed(e);
    }

    private void _SetupDocumentEvents()
    {
      //Listen to events which would change combobox contents
      IDocumentEvents_Event pNewDocEvents = (IDocumentEvents_Event)_document;
      pNewDocEvents.ActiveViewChanged += new IDocumentEvents_ActiveViewChangedEventHandler(this._OnFocusMapChanged);

      IActiveViewEvents_Event activeViewEvents = (IActiveViewEvents_Event)_activeView;
      activeViewEvents.FocusMapChanged += new IActiveViewEvents_FocusMapChangedEventHandler(this._OnFocusMapChanged);
      activeViewEvents.ItemAdded += new IActiveViewEvents_ItemAddedEventHandler(this._OnLayersChanged);
      activeViewEvents.ItemDeleted += new IActiveViewEvents_ItemDeletedEventHandler(this._OnLayersChanged);
      activeViewEvents.ItemReordered += new IActiveViewEvents_ItemReorderedEventHandler(this._OnLayersReordered);
    }

    private void _RemoveDocumentEvents()
    {
      if (_activeView != null)
      {
        IActiveViewEvents_Event activeViewEvents = (IActiveViewEvents_Event)_activeView;
        activeViewEvents.FocusMapChanged -= new IActiveViewEvents_FocusMapChangedEventHandler(this._OnFocusMapChanged);
        activeViewEvents.ItemAdded -= new IActiveViewEvents_ItemAddedEventHandler(this._OnLayersChanged);
        activeViewEvents.ItemDeleted -= new IActiveViewEvents_ItemDeletedEventHandler(this._OnLayersChanged);
        activeViewEvents.ItemReordered -= new IActiveViewEvents_ItemReorderedEventHandler(this._OnLayersReordered);
        UrbanDelineationExtension.ReleaseComObject(_activeView);
        _activeView = null;
      }

      if (_document != null)
      {
        IDocumentEvents_Event pDocEvents = (IDocumentEvents_Event)_document;
        pDocEvents.ActiveViewChanged -= new IDocumentEvents_ActiveViewChangedEventHandler(this._OnFocusMapChanged);
        UrbanDelineationExtension.ReleaseComObject(_document);
        _document = null;
      }
    }

    private void _OnFocusMapChanged()
    {
      _UpdateComboBoxes();
    }

    private void _OnLayersChanged(object layer)
    {
      _UpdateComboBoxes();
    }

    private void _OnLayersReordered(object layer, int position)
    {
      _UpdateComboBoxes();
    }

    private void _UpdateComboBoxes()
    {
      List<ComboBoxItem> geometricNetworks = new List<ComboBoxItem>();
      List<ComboBoxItem> junctionLayers = new List<ComboBoxItem>();
      List<ComboBoxItem> rasterLayers = new List<ComboBoxItem>();
      List<ComboBoxItem> polygonLayers = new List<ComboBoxItem>();

      if (_document != null)
      {
        geometricNetworks.Add(new ComboBoxItem() { Name = Properties.Resources.No_Selection, Value = null });
        junctionLayers.Add(new ComboBoxItem() { Name = Properties.Resources.No_Selection, Value = null });
        rasterLayers.Add(new ComboBoxItem() { Name = Properties.Resources.No_Selection, Value = null });
        polygonLayers.Add(new ComboBoxItem() { Name = Properties.Resources.No_Selection, Value = null });

        IMap focusMap = _document.FocusMap;
        try
        {
          int layerCount = focusMap.LayerCount;
          for (int i = 0; i < layerCount; i++)
          {
            ILayer layer = focusMap.get_Layer(i);
            _CheckLayer(layer, geometricNetworks, junctionLayers, rasterLayers, polygonLayers);
          }
        }
        finally
        {
          UrbanDelineationExtension.ReleaseComObject(focusMap);
        }
      }

      _UpdateCombobox(cbxNetwork, geometricNetworks);
      _UpdateCombobox(cbxInlets, junctionLayers);
      _UpdateCombobox(cbxDEM, rasterLayers);
      _UpdateCombobox(cbxFlowDirGrid, rasterLayers);
      _UpdateCombobox(cbxCatchments, polygonLayers);
    }

    private void _CheckLayer(ILayer layer, List<ComboBoxItem> geometricNetworks, List<ComboBoxItem> junctionLayers, List<ComboBoxItem> rasterLayers, List<ComboBoxItem> polygonLayers)
    {
      bool layerUsed = false;
      if (layer is IGroupLayer && layer is ICompositeLayer)
      {
        ICompositeLayer groupLayer = (ICompositeLayer)layer;
        int layerCount = groupLayer.Count;
        for (int i = 0; i < layerCount; i++)
        {
          ILayer subLayer = groupLayer.get_Layer(i);
          _CheckLayer(subLayer, geometricNetworks, junctionLayers, rasterLayers, polygonLayers);
        }
      }
      else if (layer is IFeatureLayer)
      {
        if (((IFeatureLayer)layer).FeatureClass != null)
        {
          if (((IFeatureLayer)layer).FeatureClass is INetworkClass)
          {
            _AddNetworkForLayer(layer, geometricNetworks);

            INetworkClass networkClass = (INetworkClass)((IFeatureLayer)layer).FeatureClass;
            if (networkClass.FeatureType == esriFeatureType.esriFTSimpleJunction)
            {
              junctionLayers.Add(new ComboBoxItem() { Name = layer.Name, Value = layer });
              layerUsed = true;
            }
          }

          if (((IFeatureLayer)layer).FeatureClass.ShapeType == esriGeometryType.esriGeometryPolygon)
          {
            polygonLayers.Add(new ComboBoxItem() { Name = layer.Name, Value = layer });
            layerUsed = true;
          }
        }
      }
      else if (layer is IRasterLayer && ((IRasterLayer)layer).BandCount == 1)
      {
        if (((IRasterLayer)layer).Raster != null)
        {
          rasterLayers.Add(new ComboBoxItem() { Name = layer.Name, Value = layer });
          layerUsed = true;
        }
      }

      if (!layerUsed)
      {
        UrbanDelineationExtension.ReleaseComObject(layer);
      }
    }

    private void _AddNetworkForLayer(ILayer layer, List<ComboBoxItem> geometricNetworks)
    {
      IFeatureLayer featureLayer = (IFeatureLayer)layer;
      INetworkClass networkClass = (INetworkClass)featureLayer.FeatureClass;
      try
      {
        IGeometricNetwork network = networkClass.GeometricNetwork;
        bool alreadyAdded = false;
        foreach (ComboBoxItem element in geometricNetworks)
        {
          if (element.Value == network)
          {
            alreadyAdded = true;
            break;
          }
        }
        if (!alreadyAdded)
        {
          geometricNetworks.Add(new ComboBoxItem() { Name = ((IDataset)network).Name, Value = network });
        }
      }
      finally
      {
        UrbanDelineationExtension.ReleaseComObject(networkClass);
      }
    }

    private void _UpdateCombobox(ComboBox comboBox, List<ComboBoxItem> dataSource)
    {
      if (dataSource != null)
      {
        // We make a copy so that each ComboBox has it's own datasource
        List<ComboBoxItem> dataSourceCopy = new List<ComboBoxItem>(dataSource);
        object selectedValue = comboBox.SelectedValue;
        comboBox.DataSource = dataSourceCopy;
        comboBox.DisplayMember = "Name";
        comboBox.ValueMember = "Value";
        if (selectedValue != null)
          comboBox.SelectedValue = selectedValue;
      }
      else
      {
        comboBox.DataSource = null;
        comboBox.Items.Clear();
      }
    }

    private ILayer _FindRasterLayer(IRaster raster, List<ILayer> layers)
    {
      if (raster == null)
        return null;

      foreach (ILayer layer in layers)
      {
        IRasterLayer rasterLayer = layer as IRasterLayer;
        if (rasterLayer != null && rasterLayer.Raster != null)
        {
          // Since there can be multiple instances of the raster created from a raster dataset
          // we check for equivalency by looking at the "complete name" of each raster's underlying
          // dataset. Note that the rasters could have different properties.

          string layerPath = ((IRasterAnalysisProps)rasterLayer.Raster).RasterDataset.CompleteName;
          string rasterPath = ((IRasterAnalysisProps)raster).RasterDataset.CompleteName;
          if (string.Compare(layerPath, rasterPath, true) == 0)
            return rasterLayer;
        }
      }
      return null;
    }

    private ILayer _FindFeatureLayer(IFeatureClass featureClass, List<ILayer> layers)
    {
      if (featureClass == null)
        return null;

      foreach (ILayer layer in layers)
      {
        IFeatureLayer featureLayer = layer as IFeatureLayer;
        if (featureLayer != null && featureLayer.FeatureClass != null)
        {
          if (featureLayer.FeatureClass == featureClass)
            return featureLayer;
        }
      }
      return null;
    }

    private List<ILayer> _GetLayers(IMxDocument mxDocument)
    {
      List<ILayer> layers = new List<ILayer>();

      if (mxDocument != null)
      {
        IMap map = null;
        try
        {
          map = mxDocument.FocusMap;
          for (int i = 0; i < map.LayerCount; i++)
          {
            ILayer pLayer = map.get_Layer(i);
            if (pLayer is ICompositeLayer)
              _AddSubLayers((ICompositeLayer)pLayer, layers);
            else
              layers.Add(pLayer);
          }
        }
        finally
        {
          UrbanDelineationExtension.ReleaseComObject(map);
        }
      }

      return layers;
    }

    private void _AddSubLayers(ICompositeLayer compositeLayer, List<ILayer> layers)
    {
      for (int i = 0; i < compositeLayer.Count; i++)
      {
        ILayer pLayer = compositeLayer.get_Layer(i);
        if (pLayer is ICompositeLayer)
          _AddSubLayers((ICompositeLayer)pLayer, layers);
        else
          layers.Add(pLayer);
      }
    }

    private void btnOK_Click(object sender, EventArgs e)
    {
      try
      {
        //Record settings
        SetupOp setupOp = UrbanDelineationExtension.Extension.Setup;
        setupOp.GeometricNetwork = cbxNetwork.SelectedValue as IGeometricNetwork;
        IFeatureLayer inletLayer = cbxInlets.SelectedValue as IFeatureLayer;
        setupOp.InletClass = inletLayer == null ? null : inletLayer.FeatureClass;
        setupOp.SmoothBoundaries = chkSmooth.Checked;
        IRasterLayer demLayer = cbxDEM.SelectedValue as IRasterLayer;
        setupOp.DEM = demLayer == null ? null : demLayer.Raster;
        setupOp.IncludeUpstreamPipeEnds = chkIncludeUp.Checked;
        setupOp.ExcludeDisabledNodes = chkExcludeDisabled.Checked;
        setupOp.ExcludeDownstreamPipeEnds = chkExcludeDown.Checked;
        IRasterLayer flowDirLayer = cbxFlowDirGrid.SelectedValue as IRasterLayer;
        setupOp.FlowDirection = flowDirLayer == null ? null : flowDirLayer.Raster;
        IFeatureLayer catchmentLayer = cbxCatchments.SelectedValue as IFeatureLayer;
        setupOp.Catchments = catchmentLayer == null ? null : catchmentLayer.FeatureClass;

        this.Close();
      }
      catch (Exception ex)
      {
        MessageBox.Show(this, ex.Message, Properties.Resources.Caption_Error);
      }
    }

    private void btnCancel_Click(object sender, EventArgs e)
    {
      try
      {
        this.Close();
      }
      catch (Exception ex)
      {
        MessageBox.Show(this, ex.Message, Properties.Resources.Caption_Error);
      }
    }

    private void btnPreprocess_Click(object sender, EventArgs e)
    {
      try
      {
        System.Windows.Forms.Application.DoEvents();
        System.Windows.Forms.Cursor.Current = Cursors.WaitCursor;

        DialogResult choice = MessageBox.Show(this, "Depending on the size of the network and DEM, this operation may take a long time.",
          Properties.Resources.Caption_Notice, MessageBoxButtons.OKCancel, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button1);
        if (choice == DialogResult.OK)
        {
          SetupOp setupOp = UrbanDelineationExtension.Extension.Setup;

          setupOp.GeometricNetwork = cbxNetwork.SelectedValue as IGeometricNetwork;
          IFeatureLayer inletLayer = cbxInlets.SelectedValue as IFeatureLayer;
          setupOp.InletClass = inletLayer.FeatureClass;
          IRasterLayer demLayer = cbxDEM.SelectedValue as IRasterLayer;
          setupOp.DEM = demLayer.Raster;
          setupOp.IncludeUpstreamPipeEnds = chkIncludeUp.Checked;
          setupOp.ExcludeDisabledNodes = chkExcludeDisabled.Checked;
          setupOp.ExcludeDownstreamPipeEnds = chkExcludeDown.Checked;
          setupOp.SmoothBoundaries = chkSmooth.Checked;
          setupOp.Preprocess();

          //Add results to map
          EnumLayer layers = new EnumLayer();

          IFeatureLayer drainLayer = new FeatureLayerClass();
          drainLayer.FeatureClass = setupOp.DrainagePoints;
          drainLayer.Name = "Drainage Points";
          drainLayer.Visible = false;
          layers.Add(drainLayer);

          IRasterLayer punchedLayer = new RasterLayerClass();
          punchedLayer.CreateFromRaster(setupOp.PunchedDEM);
          punchedLayer.Name = "Punched DEM";
          punchedLayer.Visible = false;
          layers.Add(punchedLayer);

          IRasterLayer fillLayer = new RasterLayerClass();
          fillLayer.CreateFromRaster(setupOp.FilledDEM);
          fillLayer.Name = "Filled DEM";
          fillLayer.Visible = false;
          layers.Add(fillLayer);

          IRasterLayer flowDirLayer = new RasterLayerClass();
          flowDirLayer.CreateFromRaster(setupOp.FlowDirection);
          flowDirLayer.Name = "Flow Direction";
          flowDirLayer.Visible = false;
          layers.Add(flowDirLayer);

          IFeatureLayer catchmentLayer = new FeatureLayerClass();
          catchmentLayer.FeatureClass = setupOp.Catchments;
          catchmentLayer.Name = "Inlet Catchments";
          catchmentLayer.Visible = false;
          ((ILayerEffects)catchmentLayer).Transparency = 50;
          layers.Add(catchmentLayer);

          IMap map = null;
          try
          {
            map = ArcMap.Document.FocusMap;
            map.AddLayers(layers, true);

            cbxFlowDirGrid.SelectedValue = flowDirLayer;
            cbxCatchments.SelectedValue = catchmentLayer;
          }
          finally
          {
            UrbanDelineationExtension.ReleaseComObject(map);
          }
        }
      }
      catch (Exception ex)
      {
        MessageBox.Show(this, ex.Message, Properties.Resources.Caption_Error);
      }
      finally
      {
        System.Windows.Forms.Cursor.Current = Cursors.Default;
      }
    }

    private void _UpdateButtons()
    {
      if (cbxNetwork.SelectedValue != null && cbxInlets.SelectedValue != null && cbxDEM.SelectedValue != null)
        btnPreprocess.Enabled = true;
      else
        btnPreprocess.Enabled = false;
    }

    private void ComboBox_SelectedIndexChanged(object sender, EventArgs e)
    {
      try
      {
        _UpdateButtons();
      }
      catch (Exception ex)
      {
        MessageBox.Show(this, ex.Message, Properties.Resources.Caption_Error);
      }
    }

    private void SetupForm_HelpButtonClicked(object sender, CancelEventArgs e)
    {
      AboutForm aboutForm = new AboutForm();
      aboutForm.ShowDialog(this);
    }

  }
}