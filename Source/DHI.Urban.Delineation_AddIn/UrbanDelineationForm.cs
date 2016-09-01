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
using System.Collections.Specialized;
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

namespace DHI.Urban.Delineation
{
  public partial class UrbanDelineationForm : Form
  {
    private static bool _lastExtendOverland = true;
    private static bool _lastStopAtDisabled = true;
    private static string _lastOutletField = string.Empty;
    private static IFeatureLayer _lastOutletSource = null;
    private static bool _lastSnapToPourPoint = true;
    private static double _lastSnapDistance = 12.0;

    private IMxDocument _document;
    private IActiveView _activeView;
    private bool _updatingLayers = false;

    public UrbanDelineationForm()
    {
      _document = ArcMap.Document;
      _activeView = _document.ActiveView;

      InitializeComponent();
    }

    public static bool LastExtendOverlandState
    {
      get { return _lastExtendOverland; }
      set { _lastExtendOverland = value; }
    }

    public static bool LastStopAtDisabledState
    {
      get { return _lastStopAtDisabled; }
      set { _lastStopAtDisabled = value; }
    }

    public static string LastOutletFieldText
    {
      get { return _lastOutletField; }
      set { _lastOutletField = value; }
    }

    public static IFeatureLayer LastOutletSource
    {
      get { return _lastOutletSource; }
      set { _lastOutletSource = value; }
    }

    public static bool LastSnapToPourPointState
    {
      get { return _lastSnapToPourPoint; }
      set { _lastSnapToPourPoint = value; }
    }

    public static double LastSnapDistance
    {
      get { return _lastSnapDistance; }
      set { _lastSnapDistance = value; }
    }

    protected override void OnLoad(EventArgs e)
    {
      if (_document != null)
      {
        _SetupDocumentEvents();
      }

      _UpdateComboBoxes();

      chkExtendOverland.Checked = UrbanDelineationForm.LastExtendOverlandState;
      chkStopAtDisabled.Checked = UrbanDelineationForm.LastStopAtDisabledState;
      chkSnapToPourPoint.Checked = UrbanDelineationForm.LastSnapToPourPointState;
      tbxSnapDistance.Text = UrbanDelineationForm.LastSnapDistance.ToString();

      _UpdateControls();
      _UpdateUnitLabel();

      base.OnLoad(e);
    }

    private void _UpdateOutletFields()
    {
      cbxOutletField.DataSource = null;
      cbxOutletField.Items.Clear();

      List<ComboBoxItem> fieldNames = new List<ComboBoxItem>();
      fieldNames.Add(new ComboBoxItem() { Name = Properties.Resources.Optional_Field, Value = null });

      IFeatureLayer sourceLayer = cbxOutletSource.SelectedValue as IFeatureLayer;
      if (sourceLayer == null)
      {
        // If source layer is null, we use all junction layer fields
        if (UrbanDelineationExtension.Extension != null)
        {
          if (UrbanDelineationExtension.Extension.Setup != null)
          {
            IGeometricNetwork geometricNetwork = UrbanDelineationExtension.Extension.Setup.GeometricNetwork;
            if (geometricNetwork != null)
            {
              IEnumFeatureClass junctionClasses = geometricNetwork.get_ClassesByType(esriFeatureType.esriFTSimpleJunction);
              IFeatureClass junctionClass = junctionClasses.Next();
              while (junctionClass != null)
              {
                try
                {
                  for (int i = 0; i < junctionClass.Fields.FieldCount; i++)
                  {
                    string aliasName = junctionClass.Fields.get_Field(i).AliasName;
                    string fieldName = junctionClass.Fields.get_Field(i).Name;

                    if (fieldName != junctionClass.ShapeFieldName)
                    {
                      string displayFieldName = string.Format("{0}.{1}", junctionClass.AliasName, aliasName);
                      fieldNames.Add(new ComboBoxItem() { Name = displayFieldName, Value = fieldName });
                    }
                  }
                }
                finally
                {
                  UrbanDelineationExtension.ReleaseComObject(junctionClass);
                }
                junctionClass = junctionClasses.Next();
              }
            }
          }
        }
      }
      else
      {
        IFeatureClass sourceClass = sourceLayer.FeatureClass;
        if (sourceClass != null)
        {
          try
          {
            for (int i = 0; i < sourceClass.Fields.FieldCount; i++)
            {
              string aliasName = sourceClass.Fields.get_Field(i).AliasName;
              string fieldName = sourceClass.Fields.get_Field(i).Name;

              if (fieldName != sourceClass.ShapeFieldName)
              {
                fieldNames.Add(new ComboBoxItem() { Name = aliasName, Value = fieldName });
              }
            }
          }
          finally
          {
            UrbanDelineationExtension.ReleaseComObject(sourceClass);
          }
        }
      }

      _UpdateCombobox(cbxOutletField, fieldNames);
      if (!string.IsNullOrEmpty(UrbanDelineationForm.LastOutletFieldText))
      {
        cbxOutletField.SelectedValue = UrbanDelineationForm.LastOutletFieldText;
      }

      string selectedField = cbxOutletField.SelectedValue as string;
      if (selectedField != UrbanDelineationForm.LastOutletFieldText)
      {
        cbxOutletField.SelectedIndex = 0;
        UrbanDelineationForm.LastOutletFieldText = cbxOutletField.SelectedValue as string;
      }
    }

    private void btnDelineate_Click(object sender, EventArgs e)
    {
      try
      {
        if (_CheckInput())
        {
          System.Windows.Forms.Cursor.Current = Cursors.WaitCursor;

          Delineator delineator = new Delineator();
          delineator.Setup = UrbanDelineationExtension.Extension.Setup;
          delineator.Application = ArcMap.Application;
          delineator.OutletSource = cbxOutletSource.SelectedValue as IFeatureLayer;
          delineator.OutletIdField = cbxOutletField.SelectedValue as string;
          delineator.ExtendOverland = chkExtendOverland.Checked;
          delineator.StopAtDisabledFeatures = chkStopAtDisabled.Checked;

          if (chkSnapToPourPoint.Enabled && chkSnapToPourPoint.Checked)
          {
            delineator.SnapToPourPoint = true;
            double snapDistance;
            if (double.TryParse(tbxSnapDistance.Text, out snapDistance))
            {
              delineator.SnapDistance = snapDistance;
            }
          }
          else
          {
            delineator.SnapToPourPoint = false;
          }

          delineator.DelineateCatchments();

          EnumLayer layers = new EnumLayer();

          //Add results to map
          IFeatureLayer watershedLayer = new FeatureLayerClass();
          watershedLayer.FeatureClass = delineator.OutletWatersheds;
          watershedLayer.Name = delineator.OutletWatersheds.AliasName;
          ((ILayerEffects)watershedLayer).Transparency = 50;
          if (!string.IsNullOrEmpty(delineator.OutletIdField))
            _AddLabel(watershedLayer, delineator.OutletIdField);
          layers.Add(watershedLayer);

          IMap map = null;
          try
          {
            map = ArcMap.Document.FocusMap;
            map.AddLayers(layers, true);
          }
          finally
          {
            UrbanDelineationExtension.ReleaseComObject(map);
          }
        }
      }
      catch (Exception ex)
      {
        MessageBox.Show(this, ex.Message, Properties.Resources.Caption_Error, MessageBoxButtons.OK, MessageBoxIcon.Error);
      }
      finally
      {
        System.Windows.Forms.Cursor.Current = Cursors.Default;
      }
    }

    private void _AddLabel(IFeatureLayer featureLayer, string fieldName)
    {
      IGeoFeatureLayer geoFeatureLayer = featureLayer as IGeoFeatureLayer;
      IAnnotateLayerPropertiesCollection annoPropsCollection = geoFeatureLayer.AnnotationProperties;
      IAnnotateLayerProperties annoProps;
      IElementCollection placedElements, unplacedElements;
      annoPropsCollection.QueryItem(0, out annoProps, out placedElements, out unplacedElements);
      ((ILabelEngineLayerProperties)annoProps).Expression = string.Format("[{0}]", fieldName);
      geoFeatureLayer.DisplayAnnotation = true;
    }

    private bool _CheckInput()
    {
      SetupOp pSetupOp = UrbanDelineationExtension.Extension.Setup;

      if (pSetupOp.GeometricNetwork == null)
      {
        MessageBox.Show(this, "The drainaige network has not been specified. Please check the setup and try again.", Properties.Resources.Caption_InvalidEntry, MessageBoxButtons.OK, MessageBoxIcon.Warning);
        return false;
      }

      if (pSetupOp.InletClass == null)
      {
        MessageBox.Show(this, "The inlet node layer has not been specified. Please check the setup and try again.", Properties.Resources.Caption_InvalidEntry, MessageBoxButtons.OK, MessageBoxIcon.Warning);
        return false;
      }

      if (cbxOutletSource.SelectedValue == null || chkExtendOverland.Checked)
      {
        if (pSetupOp.Catchments == null)
        {
          MessageBox.Show(this, "The preprocessed inlet catchments have not been calculated. Please check the setup and try again.", Properties.Resources.Caption_InvalidEntry, MessageBoxButtons.OK, MessageBoxIcon.Warning);
          return false;
        }
      }

      if (chkSnapToPourPoint.Enabled && chkSnapToPourPoint.Checked)
      {
        double snapDistance;
        if (!double.TryParse(tbxSnapDistance.Text, out snapDistance))
        {
          MessageBox.Show(this, "The snap distance has not been specified, or is not a valid number. Please check the snap distance.", Properties.Resources.Caption_InvalidEntry, MessageBoxButtons.OK, MessageBoxIcon.Warning);
          return false;
        }
        else
        {
          if (snapDistance <= 0)
          {
            MessageBox.Show(this, "The snap distance is less than or equal to zero. Please enter a value greater than zero.", Properties.Resources.Caption_InvalidEntry, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return false;
          }
        }
      }

      IFeatureLayer sourceLayer = cbxOutletSource.SelectedValue as IFeatureLayer;
      if (sourceLayer != null)
      {
        if (pSetupOp.FlowDirection == null)
        {
          MessageBox.Show(this, "The flow direction layer has not been specified. Please check the setup and try again.", Properties.Resources.Caption_InvalidEntry, MessageBoxButtons.OK, MessageBoxIcon.Warning);
          return false;
        }

        if (((IFeatureSelection)sourceLayer).SelectionSet.Count == 0)
        {
          DialogResult userResponse = MessageBox.Show(this, "There are no features selected in the chosen source feature layer. Are you sure you want to delineate from all features in the feature layer?", "No Selection", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2);
          if (userResponse != DialogResult.Yes)
          {
            return false;
          }
        }
      }

      return true;
    }

    private void chkExtendOverland_CheckedChanged(object sender, EventArgs e)
    {
      UrbanDelineationForm.LastExtendOverlandState = chkExtendOverland.Checked;
      _UpdateControls();
    }

    private void chkStopAtDisabled_CheckedChanged(object sender, EventArgs e)
    {
      UrbanDelineationForm.LastStopAtDisabledState = chkStopAtDisabled.Checked;
    }

    private void UrbanDelineationForm_HelpButtonClicked(object sender, CancelEventArgs e)
    {
      AboutForm aboutForm = new AboutForm();
      aboutForm.ShowDialog(this);
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
      _updatingLayers = true;
      try
      {
        List<ComboBoxItem> featureLayers = new List<ComboBoxItem>();

        if (_document != null)
        {
          featureLayers.Add(new ComboBoxItem() { Name = Properties.Resources.Use_Selected_Nodes, Value = null });

          IMap focusMap = _document.FocusMap;
          try
          {
            int layerCount = focusMap.LayerCount;
            for (int i = 0; i < layerCount; i++)
            {
              ILayer layer = focusMap.get_Layer(i);
              _CheckLayer(layer, featureLayers);
            }
          }
          finally
          {
            UrbanDelineationExtension.ReleaseComObject(focusMap);
          }
        }

        _UpdateCombobox(cbxOutletSource, featureLayers);

        if (UrbanDelineationForm.LastOutletSource != null)
        {
          cbxOutletSource.SelectedValue = UrbanDelineationForm.LastOutletSource;
        }

        if (cbxOutletSource.SelectedValue != UrbanDelineationForm.LastOutletSource)
        {
          cbxOutletSource.SelectedIndex = 0;
          UrbanDelineationForm.LastOutletSource = cbxOutletSource.SelectedValue as IFeatureLayer;
        }

        _UpdateOutletFields();
      }
      finally
      {
        _updatingLayers = false;
      }
    }

    private void _CheckLayer(ILayer layer, List<ComboBoxItem> featureLayers)
    {
      bool layerUsed = false;
      if (layer is IGroupLayer && layer is ICompositeLayer)
      {
        ICompositeLayer groupLayer = (ICompositeLayer)layer;
        int layerCount = groupLayer.Count;
        for (int i = 0; i < layerCount; i++)
        {
          ILayer subLayer = groupLayer.get_Layer(i);
          _CheckLayer(subLayer, featureLayers);
        }
      }
      else if (layer is IFeatureLayer)
      {
        if (((IFeatureLayer)layer).FeatureClass != null)
        {
          featureLayers.Add(new ComboBoxItem() { Name = layer.Name, Value = layer });
          layerUsed = true;
        }
      }

      if (!layerUsed)
      {
        UrbanDelineationExtension.ReleaseComObject(layer);
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

    private void _UpdateControls()
    {
      bool enableSnap = false;
      IFeatureLayer layer = cbxOutletSource.SelectedValue as IFeatureLayer;
      if (layer != null && layer.FeatureClass != null)
      {
        if (layer.FeatureClass.ShapeType == esriGeometryType.esriGeometryPoint)
        {
          enableSnap = true;
        }
      }
      chkSnapToPourPoint.Enabled = enableSnap;
      tbxSnapDistance.Enabled = enableSnap;
      lblSnapUnit.Enabled = enableSnap;
    }

    private void _UpdateUnitLabel()
    {
      IProjectedCoordinateSystem coordSys = null;
      if (UrbanDelineationExtension.Extension.Setup != null)
      {
        if (UrbanDelineationExtension.Extension.Setup.FlowDirection != null)
        {
          IRaster flowdir = UrbanDelineationExtension.Extension.Setup.FlowDirection;
          coordSys = ((IGeoDataset)flowdir).SpatialReference as IProjectedCoordinateSystem;
        }
        else if (UrbanDelineationExtension.Extension.Setup.Catchments != null)
        {
          IFeatureClass catchClass = UrbanDelineationExtension.Extension.Setup.Catchments;
          coordSys = ((IGeoDataset)catchClass).SpatialReference as IProjectedCoordinateSystem;
        }
      }

      string snapUnit = "[unknown]";
      if (coordSys != null)
      {
        string unitName = coordSys.CoordinateUnit.Name;
        switch (unitName)
        {
          case "Foot":
            snapUnit = "feet";
            break;
          case "Meter":
            snapUnit = "meters";
            break;
          default:
            snapUnit = "[unknown]";
            break;
        }
      }

      lblSnapUnit.Text = snapUnit;
    }

    private void cbxOutletSource_SelectedIndexChanged(object sender, EventArgs e)
    {
      if (!_updatingLayers)
      {
        UrbanDelineationForm.LastOutletSource = cbxOutletSource.SelectedValue as IFeatureLayer;
        _UpdateOutletFields();
        _UpdateControls();
      }
    }

    private void cbxOutletField_SelectedIndexChanged(object sender, EventArgs e)
    {
      if (!_updatingLayers)
        UrbanDelineationForm.LastOutletFieldText = cbxOutletField.SelectedValue as string;
    }

    private void tbxSnapDistance_TextChanged(object sender, EventArgs e)
    {
      double snapDistance;
      if (double.TryParse(tbxSnapDistance.Text, out snapDistance))
      {
        UrbanDelineationForm.LastSnapDistance = snapDistance;
        chkSnapToPourPoint.Checked = true;
      }
      else
      {
        chkSnapToPourPoint.Checked = false;
      }
    }

    private void chkSnapToPourPoint_CheckedChanged(object sender, EventArgs e)
    {
      UrbanDelineationForm.LastSnapToPourPointState = chkSnapToPourPoint.Checked;
    }
  }
}