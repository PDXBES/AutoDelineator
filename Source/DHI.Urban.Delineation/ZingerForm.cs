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
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using ESRI.ArcGIS.ArcMapUI;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.Framework;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;

namespace DHI.Urban.Delineation
{
    public partial class ZingerForm : Form
    {
        private const string GRAPHICS_LAYER_PREFIX = "Zinger Graphics";

        private IMxDocument _document;
        private IActiveView _activeView;
        private bool _updatingLayers = false;

        public ZingerForm()
        {
            _document = ArcMap.Application.Document as IMxDocument;
            _activeView = _document.ActiveView;

            InitializeComponent();
        }

        protected override void OnLoad(EventArgs e)
        {
              if (_document != null)
              {
                  _SetupDocumentEvents();
              }

              _UpdateComboBoxes();

            base.OnLoad(e);
        }

        private void _UpdateComboBoxes()
        {
            _updatingLayers = true;
            try
            {
                List<ComboBoxItem> featureLayers = new List<ComboBoxItem>();

                if (_document != null)
                {
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
//                        UrbanDelineationExtension.ReleaseComObject(focusMap);
                    }
                }

                _UpdateCombobox(cbxFromLayer, featureLayers);
                _UpdateCombobox(cbxToLayer, featureLayers);

                _UpdateFields(cbxFromField, cbxFromLayer.SelectedValue as IFeatureLayer);
                _UpdateFields(cbxToField, cbxToLayer.SelectedValue as IFeatureLayer);

                _UpdateButtons();
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
//                UrbanDelineationExtension.ReleaseComObject(layer);
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

        private void _UpdateFields(ComboBox fieldCombo, IFeatureLayer sourceLayer)
        {
            fieldCombo.DataSource = null;
            fieldCombo.Items.Clear();

            List<ComboBoxItem> fieldNames = new List<ComboBoxItem>();

            if (sourceLayer != null)
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
//                        UrbanDelineationExtension.ReleaseComObject(sourceClass);
                    }
                }
            }

            _UpdateCombobox(fieldCombo, fieldNames);
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
//                UrbanDelineationExtension.ReleaseComObject(_activeView);
                _activeView = null;
            }

            if (_document != null)
            {
                IDocumentEvents_Event pDocEvents = (IDocumentEvents_Event)_document;
                pDocEvents.ActiveViewChanged -= new IDocumentEvents_ActiveViewChangedEventHandler(this._OnFocusMapChanged);
//                UrbanDelineationExtension.ReleaseComObject(_document);
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

        private void cbxFromLayer_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!_updatingLayers)
            {
                _UpdateButtons();
                _UpdateFields(cbxFromField, cbxFromLayer.SelectedValue as IFeatureLayer);
            }
        }

        private void cbxToLayer_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!_updatingLayers)
            {
                _UpdateButtons();
                _UpdateFields(cbxToField, cbxToLayer.SelectedValue as IFeatureLayer);
            }
        }

        private void _UpdateButtons()
        {
            if (cbxFromLayer.SelectedValue is IFeatureLayer
                && cbxToLayer.SelectedValue is IFeatureLayer
                && cbxFromField.SelectedValue is string
                && cbxToField.SelectedValue is string)
            {
                btnAdd.Enabled = true;
            }
            else
            {
                btnAdd.Enabled = false;
            }

            bool enableRemove = false;
            IFeatureLayer fromLayer = cbxFromLayer.SelectedValue as IFeatureLayer;
            if (fromLayer != null)
            {
                ICompositeGraphicsLayer basicGraphicsLayer = (ICompositeGraphicsLayer)_document.FocusMap.BasicGraphicsLayer;
                string graphicsLayerName = _GetGraphicsLayerName(fromLayer.FeatureClass);
                ICompositeLayer compositeLayer = (ICompositeLayer)basicGraphicsLayer;
                for (int i = 0; i < compositeLayer.Count; i++)
                {
                    if (compositeLayer.get_Layer(i).Name == graphicsLayerName)
                    {
                        enableRemove = true;
                        break;
                    }
                }
            }
            btnRemove.Enabled = enableRemove;
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            try
            {
                if (_CheckInput())
                {
                    System.Windows.Forms.Cursor.Current = Cursors.WaitCursor;

                    IFeatureLayer fromLayer = cbxFromLayer.SelectedValue as IFeatureLayer;
                    string fromField = cbxFromField.SelectedValue as string;
                    IFeatureLayer toLayer = cbxToLayer.SelectedValue as IFeatureLayer;
                    string toField = cbxToField.SelectedValue as string;

                    // Invalidate graphics
                    _activeView.PartialRefresh(esriViewDrawPhase.esriViewGraphics, null, null);

                    _RemoveZingers(fromLayer);
                    _AddZingers(fromLayer, fromField, toLayer, toField);

                    // Refresh graphics
                    _activeView.PartialRefresh(esriViewDrawPhase.esriViewGraphics, null, null);
                }

                _UpdateButtons();
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

        private void btnRemove_Click(object sender, EventArgs e)
        {
            try
            {
                System.Windows.Forms.Cursor.Current = Cursors.WaitCursor;

                IFeatureLayer fromLayer = cbxFromLayer.SelectedValue as IFeatureLayer;

                // Invalidate graphics
                _activeView.PartialRefresh(esriViewDrawPhase.esriViewGraphics, null, null);

                _RemoveZingers(fromLayer);

                // Refresh graphics
                _activeView.PartialRefresh(esriViewDrawPhase.esriViewGraphics, null, null);

                _UpdateButtons();
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

        private bool _CheckInput()
        {
            IFeatureLayer fromLayer = cbxFromLayer.SelectedValue as IFeatureLayer;
            if (fromLayer == null || fromLayer.FeatureClass == null)
            {
                MessageBox.Show(this, "The \"from\" layer is not a valid layer.", Properties.Resources.Caption_InvalidEntry, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            IFeatureLayer toLayer = cbxToLayer.SelectedValue as IFeatureLayer;
            if (toLayer == null || toLayer.FeatureClass == null)
            {
                MessageBox.Show(this, "The \"to\" layer is not a valid layer.", Properties.Resources.Caption_InvalidEntry, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            //if (fromLayer.FeatureClass == toLayer.FeatureClass)
            //{
            //    MessageBox.Show(this, "The \"from\" layer and \"to\" layer are the same. Please select two different layers.", Properties.Resources.Caption_InvalidEntry, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            //    return false;
            //}

            if (!(cbxFromField.SelectedValue is string))
            {
                MessageBox.Show(this, "The \"from\" field is invalid.", Properties.Resources.Caption_InvalidEntry, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (!(cbxToField.SelectedValue is string))
            {
                MessageBox.Show(this, "The \"to\" field is invalid.", Properties.Resources.Caption_InvalidEntry, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            int fromFieldIndex = fromLayer.FeatureClass.FindField((string)cbxFromField.SelectedValue);
            if (fromFieldIndex < 0)
            {
                MessageBox.Show(this, "The \"from\" field is invalid.", Properties.Resources.Caption_InvalidEntry, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            int toFieldIndex = toLayer.FeatureClass.FindField((string)cbxToField.SelectedValue);
            if (toFieldIndex < 0)
            {
                MessageBox.Show(this, "The \"to\" field is invalid.", Properties.Resources.Caption_InvalidEntry, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            //IField fromField = fromLayer.FeatureClass.Fields.get_Field(fromFieldIndex);
            //IField toField = toLayer.FeatureClass.Fields.get_Field(toFieldIndex);
            //if (fromField.VarType != toField.VarType)
            //{
            //    MessageBox.Show(this, "The \"from\" field and the \"to\" field are not of the same type and can't be compared.", Properties.Resources.Caption_InvalidEntry, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            //    return false;
            //}

            return true;
        }

        private void _AddZingers(IFeatureLayer fromLayer, string fromFieldName, IFeatureLayer toLayer, string toFieldName)
        {
            ICompositeGraphicsLayer basicGraphicsLayer = (ICompositeGraphicsLayer)_document.FocusMap.BasicGraphicsLayer;
            string graphicsLayerName = _GetGraphicsLayerName(fromLayer.FeatureClass);

            // Create lookup for "to" points
            Type toValueType;
            Dictionary<object, List<IPoint>> toPoints = _GetToPointLookup(toLayer, toFieldName, out toValueType);

            // Add zingers
            if (toPoints.Count > 0)
            {
                IGraphicsContainer graphics = (IGraphicsContainer)basicGraphicsLayer.AddLayer(graphicsLayerName, fromLayer);
                IFeatureCursor cursor = null;
                try
                {
                    int fromField = fromLayer.FeatureClass.FindField(fromFieldName);
                    cursor = fromLayer.FeatureClass.Search(null, false);
                    IFeature fromFeature = cursor.NextFeature();
                    while (fromFeature != null)
                    {
                        try
                        {
                            IPoint fromPoint = _GetFeaturePoint(fromFeature);
                            if (fromPoint != null)
                            {
                                object fromValue = fromFeature.get_Value(fromField);
                                if (fromValue != DBNull.Value)
                                {
                                    try
                                    {
                                        fromValue = Convert.ChangeType(fromValue, toValueType);
                                    }
                                    catch (Exception ex)
                                    {
                                        throw new Exception("Could not convert values in \"from\" field to type of value in \"to\" field.", ex);
                                    }

                                    if (toPoints.ContainsKey(fromValue))
                                    {
                                        foreach (IPoint toPoint in toPoints[fromValue])
                                        {
                                            IPolyline line = new PolylineClass();
                                            line.FromPoint = fromPoint;
                                            line.ToPoint = toPoint;
                                            IElement element = new LineElementClass();
                                            element.Geometry = line;
                                            ((ILineElement)element).Symbol = _GetZingerSymbol();

                                            graphics.AddElement(element, 0);
                                        }
                                    }
                                }
                            }
                        }
                        finally
                        {
//                            UrbanDelineationExtension.ReleaseComObject(fromFeature);
                        }
                        fromFeature = cursor.NextFeature();
                    }
                }
                finally
                {
//                    UrbanDelineationExtension.ReleaseComObject(cursor);
                }
            }
        }

        private ILineSymbol _GetZingerSymbol()
        {
            // Create a color
            Color color = Color.Green;
            IRgbColor symbolColor = new RgbColorClass();
            symbolColor.Red = color.R;
            symbolColor.Green = color.G;
            symbolColor.Blue = color.B;

            // Create an arrow marker
            IArrowMarkerSymbol arrowMarker = new ArrowMarkerSymbolClass();
            arrowMarker.Color = symbolColor;
            arrowMarker.Length = 6.125;
            arrowMarker.Width = 7.0;

            // Create an decoration element from arrow marker to be positioned on the line
            ISimpleLineDecorationElement decorationElement = new SimpleLineDecorationElementClass();
            decorationElement.MarkerSymbol = arrowMarker;
            decorationElement.AddPosition(0.5);
            decorationElement.PositionAsRatio = true;

            // Create a line decoration to be added to the line
            ILineDecoration lineDecoration = new LineDecorationClass();
            lineDecoration.AddElement(decorationElement);

            // Create the line symbol with decoration
            ICartographicLineSymbol lineSymbol = new CartographicLineSymbolClass();
            lineSymbol.Color = symbolColor;
            ILineProperties lineProperties = (ILineProperties)lineSymbol;
            lineProperties.LineDecoration = lineDecoration;

            return lineSymbol;
        }

        private void _RemoveZingers(IFeatureLayer fromLayer)
        {
            ICompositeGraphicsLayer basicGraphicsLayer = (ICompositeGraphicsLayer)_document.FocusMap.BasicGraphicsLayer;
            string graphicsLayerName = _GetGraphicsLayerName(fromLayer.FeatureClass);
            ICompositeLayer compositeLayer = (ICompositeLayer)basicGraphicsLayer;
            for (int i = 0; i < compositeLayer.Count; i++)
            {
                if (compositeLayer.get_Layer(i).Name == graphicsLayerName)
                {
                    basicGraphicsLayer.DeleteLayer(graphicsLayerName);
                }
            }
        }

        private string _GetGraphicsLayerName(IFeatureClass featureClass)
        {
          return "";
//            return string.Format("{0}: {1}", GRAPHICS_LAYER_PREFIX, UrbanDelineationExtension.GetDatasetPath((IDataset)featureClass));
        }

        private Dictionary<object, List<IPoint>> _GetToPointLookup(IFeatureLayer toLayer, string toFieldName, out Type toValueType)
        {
            toValueType = null;
            Dictionary<object, List<IPoint>> toPoints = new Dictionary<object, List<IPoint>>();
            IFeatureCursor cursor = null;
            try
            {
                int toField = toLayer.FeatureClass.FindField(toFieldName);
                cursor = toLayer.FeatureClass.Search(null, false);
                IFeature toFeature = cursor.NextFeature();
                while (toFeature != null)
                {
                    try
                    {
                        IPoint toPoint = _GetFeaturePoint(toFeature);
                        if (toPoint != null)
                        {
                            object toValue = toFeature.get_Value(toField);
                            if (toValue != DBNull.Value)
                            {
                                if (toValueType == null)
                                {
                                    toValueType = toValue.GetType();
                                }

                                if (!toPoints.ContainsKey(toValue))
                                {
                                    toPoints[toValue] = new List<IPoint>();
                                }
                                toPoints[toValue].Add(toPoint);
                            }
                        }
                    }
                    finally
                    {
 //                       UrbanDelineationExtension.ReleaseComObject(toFeature);
                    }
                    toFeature = cursor.NextFeature();
                }
            }
            finally
            {
//                UrbanDelineationExtension.ReleaseComObject(cursor);
            }

            return toPoints;
        }

        private IPoint _GetFeaturePoint(IFeature feature)
        {
            IPoint point = feature.Shape as IPoint;
            if (point == null)
            {
                point = new PointClass();
                if (feature.Shape is IPolyline)
                {
                    ((IPolyline)feature.Shape).QueryPoint(esriSegmentExtension.esriNoExtension, 0.5, true, point);
                }
                else if (feature.Shape is IArea)
                {
                    ((IArea)feature.Shape).QueryLabelPoint(point);
                }
            }
            return point;
        }

        private void ZingerForm_HelpButtonClicked(object sender, CancelEventArgs e)
        {
            AboutForm aboutForm = new AboutForm();
            aboutForm.ShowDialog(this);
        }
    }
}
