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
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Xml;
using ESRI.ArcGIS.ArcMapUI;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.DataSourcesRaster;
using ESRI.ArcGIS.GeoAnalyst;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Framework;
using ESRI.ArcGIS.SpatialAnalyst;
using ESRI.ArcGIS.esriSystem;

namespace DHI.Urban.Delineation
{
	public class Delineator
	{
        private bool _disposed;
		private SetupOp _setupOp;
		private IApplication _application;
        private IList<int> _selectedJunctions = new List<int>();
        private bool _extendOverland = true;
        private bool _stopAtDisabled = true;
        private bool _snapPoints = true;
        private double _snapDistance = 10.0;
        private IFeatureLayer _sourceLayer;
        private string _idField = string.Empty;
        private string _outIdField = string.Empty;
        private IFeatureClass _outletWatersheds;
        private List<IDataset> _tempDatasets = new List<IDataset>(); // Used to delay disposal of certain temp datasets.

		public Delineator()
		{
		}

        ~Delineator()
		{
			Dispose(false);
		}

		private void Dispose(bool disposing)
		{
			if (!_disposed)
			{
				if (disposing)
				{
					//Managed classes
				}

				//Unmanaged classes
                _DisposeTempDatasets();
                _DisposeResults();

				_disposed = true;
			}
		}

        private void _DisposeTempDatasets()
        {
            try
            {
                foreach (IDataset dataset in _tempDatasets)
                {
                    if (dataset != null)
                    {
                        try
                        {
                            dataset.Delete();

                            UrbanDelineationExtension.ReleaseComObject(dataset);
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine("Delineator: " + ex.GetType().FullName + ": " + ex.Message);
                        }
                    }
                }
            }
            finally
            {
                _tempDatasets.Clear();
            }
        }

        private void _DisposeResults()
        {
            UrbanDelineationExtension.ReleaseComObject(_outletWatersheds);
            _outletWatersheds = null;
        }

		public SetupOp Setup
		{
			set { _setupOp = value; }
		}

		public IApplication Application
		{
			set { _application = value; }
		}

		/// <summary>
		/// Whether to extend the watershed overland to outlets and up the outlets' networks.
		/// </summary>
		public bool ExtendOverland
		{
			get { return _extendOverland; }
			set { _extendOverland = value; }
		}

        /// <summary>
        /// Whether to stop tracing through disabled features.
        /// </summary>
        public bool StopAtDisabledFeatures
        {
            get { return _stopAtDisabled; }
            set { _stopAtDisabled = value; }
        }

        /// <summary>
        /// Whether to snap source points on the surface to nearby low points ("pour points"). The maximum snap distance is set in the SnapDistance property.
        /// </summary>
        public bool SnapToPourPoint
        {
            get { return _snapPoints; }
            set { _snapPoints = value; }
        }

        /// <summary>
        /// The maximum snap distance to snap source points if SnapToPourPoint is set to true.
        /// </summary>
        public double SnapDistance
        {
            get { return _snapDistance; }
            set { _snapDistance = value; }
        }

        public IFeatureLayer OutletSource
        {
            get { return _sourceLayer; }
            set { _sourceLayer = value; }
        }

        /// <summary>
        /// Gets or sets the outlet id field name to include in the output.
        /// </summary>
        public string OutletIdField
        {
            get
            {
                return _outIdField; 
            }
            set
            {
                _idField = value == null ? string.Empty : value;
                _outIdField = _idField;
            }
        }

        /// <summary>
        /// The resulting watersheds with one polygon for each outlet.
        /// </summary>
        public IFeatureClass OutletWatersheds
        {
            get { return _outletWatersheds; }
        }

		public void DelineateCatchments()
		{
            _CheckInput();
            _DisposeResults();

            Dictionary<int, IGeometry> watershedShapes = new Dictionary<int, IGeometry>();
            if (_sourceLayer == null)
            {
                // If source layer is null, then we will trace from selected network nodes.

                _selectedJunctions = _GetSelectedJunctions();
                if (_selectedJunctions.Count == 0)
                    throw new Exception("There are no features selected in the designated drainage network.");

                // Create single shape for each junction
                foreach (int junctionEID in _selectedJunctions)
                {
                    int[] upstreamInlets = _TraceUpstream(junctionEID, true);
                    watershedShapes[junctionEID] = _MergeCatchments(upstreamInlets);
                    if (_extendOverland)
                    {
                        List<int> extendedInletsList = new List<int>(upstreamInlets);
                        watershedShapes[junctionEID] = _ExtendWatershed(watershedShapes[junctionEID], extendedInletsList);
                    }
                }
            }
            else
            {
                watershedShapes = _DelineateSurfaceCatchments();
                List<int> watershedIds = new List<int>(watershedShapes.Keys);
                foreach (int watershedId in watershedIds)
                {
                    if (_extendOverland)
                    {
                        List<int> extendedInletsList = new List<int>();
                        watershedShapes[watershedId] = _ExtendWatershed(watershedShapes[watershedId], extendedInletsList);
                    }
                }
            }

            ISpatialReference spatialRef = new UnknownCoordinateSystemClass();
            if (_setupOp.Catchments != null)
                spatialRef = ((IGeoDataset)_setupOp.Catchments).SpatialReference;
            else if (_setupOp.FlowDirection != null)
                spatialRef = ((IGeoDataset)_setupOp.FlowDirection).SpatialReference;

            _outletWatersheds = _MakeWatershedsClass(watershedShapes, spatialRef);
            _OutputSettings();

            _DisposeTempDatasets();
		}

        private Dictionary<int, IGeometry> _DelineateSurfaceCatchments()
        {
            Dictionary<int, IGeometry> watershedShapes = new Dictionary<int, IGeometry>();
            bool snap = _snapPoints && _sourceLayer.FeatureClass.ShapeType == esriGeometryType.esriGeometryPoint;

            // Convert source locations to raster
            IConversionOp conversionOp = new RasterConversionOpClass();
            IHydrologyOp hydrologyOp = new RasterHydrologyOpClass();
            IWorkspace tempWorkspace = null;
            try
            {
                tempWorkspace = _setupOp.GetTempRasterWorkspace();

                IEnvelope outputExtent = _GetSurfaceOutputExtent();
                if (outputExtent == null)
                {
                    _setupOp.SetAnalysisEnvironment((IRasterAnalysisEnvironment)conversionOp);
                    _setupOp.SetAnalysisEnvironment((IRasterAnalysisEnvironment)hydrologyOp);
                }
                else
                {
                    // Expand to ensure full boundary of watershed is included.
                    double cellWidth = ((IRasterAnalysisProps)_setupOp.FlowDirection).PixelWidth;
                    double cellHeight = ((IRasterAnalysisProps)_setupOp.FlowDirection).PixelHeight;
                    outputExtent.Expand(cellWidth * 3.0, cellHeight * 3.0, false);

                    _setupOp.SetAnalysisEnvironment((IRasterAnalysisEnvironment)conversionOp, outputExtent);
                    _setupOp.SetAnalysisEnvironment((IRasterAnalysisEnvironment)hydrologyOp, outputExtent);
                }

                IGeoDataset flowAcc = null;
                IGeoDataset seedRaster = null;
                IGeoDataset watersheds = null;
                IFeatureClass watershedClass = null;
                try
                {
                    string outputPath = SetupOp.CreateTempFileName(_setupOp.ScratchDirectory, "SurfaceWatersheds", ".shp");
                    string outputName = System.IO.Path.GetFileNameWithoutExtension(outputPath);

                    IFeatureClassDescriptor sourceDescriptor = new FeatureClassDescriptorClass();
                    if (((IFeatureSelection)_sourceLayer).SelectionSet.Count > 0)
                    {
                        sourceDescriptor.CreateFromSelectionSet(((IFeatureSelection)_sourceLayer).SelectionSet, null, _sourceLayer.FeatureClass.OIDFieldName);
                    }
                    else
                    {
                        sourceDescriptor.Create(_sourceLayer.FeatureClass, null, _sourceLayer.FeatureClass.OIDFieldName);
                    }

                    if (snap)
                    {
                        object missing = Type.Missing;
                        flowAcc = hydrologyOp.FlowAccumulation((IGeoDataset)_setupOp.FlowDirection, ref missing);
                        seedRaster = hydrologyOp.SnapPourPoint((IGeoDataset)sourceDescriptor, flowAcc, _snapDistance);
                    }
                    else
                    {
                        string gridPath = SetupOp.CreateTempFileName(_setupOp.ScratchDirectory, "SrcSeed", null);
                        string gridName = System.IO.Path.GetFileName(gridPath);
                        seedRaster = (IGeoDataset)conversionOp.ToRasterDataset((IGeoDataset)sourceDescriptor, "GRID", tempWorkspace, gridName);
                    }

                    watersheds = hydrologyOp.Watershed((IGeoDataset)_setupOp.FlowDirection, (IGeoDataset)seedRaster);
                    watershedClass = _setupOp.RasterToPolygon(watersheds, outputName, _setupOp.ScratchDirectory, true);

                    int gridCodeField = watershedClass.FindField("GridCode");
                    IFeatureCursor cursor = watershedClass.Search(null, false);
                    try
                    {
                        IFeature watershed = cursor.NextFeature();
                        while (watershed != null)
                        {
                            try
                            {
                                int sourceId = Convert.ToInt32(watershed.get_Value(gridCodeField));
                                watershedShapes[sourceId] = watershed.ShapeCopy;
                            }
                            finally
                            {
                                UrbanDelineationExtension.ReleaseComObject(watershed);
                            }
                            watershed = cursor.NextFeature();
                        }
                    }
                    finally
                    {
                        UrbanDelineationExtension.ReleaseComObject(cursor);
                    }
                }
                finally
                {
                    if (seedRaster is IDataset && ((IDataset)seedRaster).CanDelete())
                        ((IDataset)seedRaster).Delete();
                    UrbanDelineationExtension.ReleaseComObject(seedRaster);

                    if (flowAcc is IDataset && ((IDataset)flowAcc).CanDelete())
                        ((IDataset)flowAcc).Delete();
                    UrbanDelineationExtension.ReleaseComObject(flowAcc);

                    _MarkForDisposal(watersheds as IDataset);
                    _MarkForDisposal(watershedClass as IDataset);
                }
            }
            finally
            {
                UrbanDelineationExtension.ReleaseComObject(tempWorkspace);
                UrbanDelineationExtension.ReleaseComObject(conversionOp);
            }

            return watershedShapes;
        }

        private IEnvelope _GetSurfaceOutputExtent()
        {
            IEnvelope extent = null;

            if (_setupOp.Catchments != null)
            {
                // Get all source shapes
                List<IGeometry> sourceShapes = new List<IGeometry>();
                IFeatureClass sourceClass = _sourceLayer.FeatureClass;
                try
                {
                    if (((IFeatureSelection)_sourceLayer).SelectionSet.Count > 0)
                    {
                        IEnumIDs selection = ((IFeatureSelection)_sourceLayer).SelectionSet.IDs;
                        int selectedId = selection.Next();
                        while (selectedId != -1)
                        {
                            IFeature feature = sourceClass.GetFeature(selectedId);
                            try
                            {
                                sourceShapes.Add(feature.ShapeCopy);
                            }
                            finally
                            {
                                UrbanDelineationExtension.ReleaseComObject(feature);
                            }
                            selectedId = selection.Next();
                        }
                    }
                    else
                    {
                        IFeatureCursor cursor = sourceClass.Search(null, false);
                        try
                        {
                            IFeature feature = cursor.NextFeature();
                            while (feature != null)
                            {
                                try
                                {
                                    sourceShapes.Add(feature.ShapeCopy);
                                }
                                finally
                                {
                                    UrbanDelineationExtension.ReleaseComObject(feature);
                                }
                                feature = cursor.NextFeature();
                            }
                        }
                        finally
                        {
                            UrbanDelineationExtension.ReleaseComObject(cursor);
                        }
                    }
                }
                finally
                {
                    UrbanDelineationExtension.ReleaseComObject(sourceClass);
                }

                // Find all inlet catchments that intersect any source feature and union envelopes
                IFeatureCursor catchCursor = _setupOp.Catchments.Search(null, false);
                try
                {
                    IFeature catchment = catchCursor.NextFeature();
                    while (catchment != null)
                    {
                        try
                        {
                            IRelationalOperator relationOp = (IRelationalOperator)catchment.Shape;
                            foreach (IGeometry sourceShape in sourceShapes)
                            {
                                if (!relationOp.Disjoint(sourceShape))
                                {
                                    if (extent == null)
                                    {
                                        extent = new EnvelopeClass();
                                        catchment.Extent.QueryEnvelope(extent);
                                    }
                                    else
                                    {
                                        extent.Union(catchment.Extent);
                                    }
                                }
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

        private void _CheckInput()
        {
            if (_setupOp == null)
            {
                throw new Exception("The setup object has not been set for the delineator.");
            }

            if (_setupOp.GeometricNetwork == null)
            {
                throw new Exception("The geometric network has not been set in the delineator setup.");
            }

            if (_setupOp.InletClass == null)
            {
                throw new Exception("The inlet feature class has not been set in the delineator setup.");
            }

            if (_sourceLayer == null || _extendOverland)
            {
                if (_setupOp.Catchments == null)
                {
                    throw new Exception("The preprocessed catchments have not been calculated in the delineator setup.");
                }
            }

            if (_sourceLayer != null && _sourceLayer.FeatureClass == null)
            {
                throw new Exception("The selected input feature layer is missing its data source. Please repair the feature layer.");
            }

            if (_snapPoints)
            {
                if (_sourceLayer == null || _sourceLayer.FeatureClass.ShapeType != esriGeometryType.esriGeometryPoint || _snapDistance <= 0.0)
                {
                    _snapPoints = false;
                    _snapDistance = 0.0;
                }
            }
        }

        private void _MarkForDisposal(IDataset dataset)
        {
            if (dataset != null)
            {
                if (!_tempDatasets.Contains(dataset))
                {
                    _tempDatasets.Add(dataset);
                }
            }
        }

		private IGeometry _ExtendWatershed(IGeometry catchmentShape, List<int> usedInletsList)
		{
			int[] outletEIDs = _FindOutlets(catchmentShape);
			if (outletEIDs.Length == 0)
				return catchmentShape;

			Dictionary<int, int[]> upstreamInlets = new Dictionary<int, int[]>(outletEIDs.Length);
			List<int> newInletsList = new List<int>();

			//Collect all upstream inlets for discovered outlets
			foreach (int outletEID in outletEIDs)
			{
				int[] inletEIDs = _TraceUpstream(outletEID, false);

				upstreamInlets[outletEID] = inletEIDs;
				foreach (int inletEID in inletEIDs)
				{
					if (!usedInletsList.Contains(inletEID) && !newInletsList.Contains(inletEID))
					{
						usedInletsList.Add(inletEID);
						newInletsList.Add(inletEID);
					}
				}
			}

			//break in case all inlets have already been delineated (overland loops)
			if (newInletsList.Count == 0)
				return catchmentShape;

			IGeometry extendedArea = _MergeCatchments(newInletsList.ToArray());
			extendedArea = _ExtendWatershed(extendedArea, usedInletsList); //iterate further until no more outlets

			return ((ITopologicalOperator)catchmentShape).Union(extendedArea);
		}

		private int[] _FindOutlets(IGeometry watershedShape)
		{
			List<int> outletEIDs = new List<int>();

			INetTopology netTopology = _setupOp.GeometricNetwork.Network as INetTopology;
			INetElements netElements = netTopology as INetElements;

			ISpatialFilter filter = new SpatialFilterClass();
			filter.Geometry = watershedShape;
			filter.SpatialRel = esriSpatialRelEnum.esriSpatialRelContains;

			IEnumFeatureClass junctionClasses = _setupOp.GeometricNetwork.get_ClassesByType(esriFeatureType.esriFTSimpleJunction);
			IFeatureClass junctionClass = junctionClasses.Next();
			while(junctionClass != null)
			{
                try
                {
                    int classID = junctionClass.FeatureClassID;
                    filter.GeometryField = junctionClass.ShapeFieldName;

                    IFeatureCursor cursor = null;
                    try
                    {
                        cursor = junctionClass.Search(filter, true);
                        IFeature junction = cursor.NextFeature();
                        while (junction != null)
                        {
                            try
                            {
                                bool junctionDisabled = false;
                                if (_stopAtDisabled)
                                    junctionDisabled = !((INetworkFeature)junction).Enabled;

                                if (!junctionDisabled)
                                {
                                    int junctionId = netElements.GetEID(classID, junction.OID, 0, esriElementType.esriETJunction);
                                    int edgeCount = netTopology.GetAdjacentEdgeCount(junctionId);
                                    bool isOutlet = edgeCount > 0;
                                    for (int i = 0; i < edgeCount; i++)
                                    {
                                        int edgeId;
                                        bool towardsJunction;
                                        netTopology.GetAdjacentEdge(junctionId, i, out edgeId, out towardsJunction);
                                        if (!towardsJunction)
                                        {
                                            isOutlet = false;
                                            break;
                                        }
                                    }
                                    if (isOutlet)
                                        outletEIDs.Add(junctionId);
                                }
                            }
                            finally
                            {
                                UrbanDelineationExtension.ReleaseComObject(junction);
                            }
                            junction = cursor.NextFeature();
                        }
                    }
                    finally
                    {
                        UrbanDelineationExtension.ReleaseComObject(cursor);
                    }
                }
                finally
                {
                    UrbanDelineationExtension.ReleaseComObject(junctionClass);
                }
				junctionClass = junctionClasses.Next();
			}

			return outletEIDs.ToArray();
		}

		private IFeatureClass _MakeWatershedsClass(Dictionary<int, IGeometry> watershedShapes, ISpatialReference spatialReference)
		{
            const string SOURCE_ID_FIELD = "SourceID";

            INetElements netElements = _setupOp.GeometricNetwork.Network as INetElements;
            Dictionary<int, string> dicJunctionClasses = _GetJunctionClassLookup();

			string outputDirectory = _setupOp.ResultsDirectory;
			string outputPath = SetupOp.CreateTempFileName(outputDirectory, "OutletWatersheds", ".shp");
			string outputFileName = System.IO.Path.GetFileNameWithoutExtension(outputPath);

            IFeatureClass watershedClass = _setupOp.CreateShapefile(outputDirectory, outputFileName, esriGeometryType.esriGeometryPolygon, spatialReference, SOURCE_ID_FIELD);
			_AddField(watershedClass, "SourceLyr", esriFieldType.esriFieldTypeString);
			_AddField(watershedClass, "Area", esriFieldType.esriFieldTypeDouble);
			_AddField(watershedClass, "Area_Acres", esriFieldType.esriFieldTypeDouble);
            bool outletFieldAdded = _AddOutletIdField(watershedClass, watershedShapes, netElements);

			int classID;
			int oid;
			int subID;

			IFeatureCursor cursor = watershedClass.Insert(true);
			try
			{
                int oidField = watershedClass.FindField(SOURCE_ID_FIELD);
                int classNameField = watershedClass.FindField("SourceLyr");
				int areaField = watershedClass.FindField("Area");
				int acreField = watershedClass.FindField("Area_Acres");
                int outletIdField = watershedClass.FindField(_outIdField);

				IFeatureBuffer buffer = watershedClass.CreateFeatureBuffer();
                object defaultIdValue = null;
                if (outletFieldAdded)
                    defaultIdValue = _GetDefaultValue(buffer.Fields.get_Field(outletIdField));
                foreach (int id in watershedShapes.Keys)
				{
                    // Set shape
                    buffer.Shape = watershedShapes[id];
                    buffer.set_Value(areaField, ((IArea)watershedShapes[id]).Area);

                    // Set source attributes
                    object idValue = defaultIdValue;
                    if (_sourceLayer == null)
                    {
                        netElements.QueryIDs(id, esriElementType.esriETJunction, out classID, out oid, out subID);
                        buffer.set_Value(classNameField, dicJunctionClasses[classID]);
                        buffer.set_Value(oidField, oid);

                        if (outletFieldAdded)
                        {
                            INetElementDescriptionEdit elementDescription = new NetElementDescriptionClass();
                            elementDescription.ElementType_2 = esriElementType.esriETJunction;
                            elementDescription.UserClassID_2 = classID;
                            elementDescription.UserID_2 = oid;
                            elementDescription.UserSubID_2 = subID;

                            IFeature netFeature = _setupOp.GeometricNetwork.get_NetworkFeature(elementDescription) as IFeature;
                            int netIdField = netFeature.Fields.FindField(_idField);
                            if (netIdField > -1)
                            {
                                esriFieldType netFieldType = netFeature.Fields.get_Field(netIdField).Type;
                                if (netFieldType == esriFieldType.esriFieldTypeOID || netFieldType == watershedClass.Fields.get_Field(outletIdField).Type)
                                {
                                    idValue = netFeature.get_Value(netIdField);
                                }
                            }
                        }
                    }
                    else
                    {
                        buffer.set_Value(classNameField, _sourceLayer.FeatureClass.AliasName);
                        buffer.set_Value(oidField, id);

                        if (outletFieldAdded)
                        {
                            IFeature sourceFeature = _sourceLayer.FeatureClass.GetFeature(id);
                            int sourceIdField = sourceFeature.Fields.FindField(_idField);
                            if (sourceIdField > -1)
                            {
                                esriFieldType sourceFieldType = sourceFeature.Fields.get_Field(sourceIdField).Type;
                                if (sourceFieldType == esriFieldType.esriFieldTypeOID || sourceFieldType == watershedClass.Fields.get_Field(outletIdField).Type)
                                {
                                    idValue = sourceFeature.get_Value(sourceIdField);
                                }
                            }
                        }
                    }

                    if(outletFieldAdded)
                        buffer.set_Value(outletIdField, idValue == null || idValue == DBNull.Value ? defaultIdValue : idValue);

					cursor.InsertFeature(buffer);
				}
			}
			finally
			{
				UrbanDelineationExtension.ReleaseComObject(cursor);
			}
			return watershedClass;
		}

        private object _GetDefaultValue(IField field)
        {
            if (field.DefaultValue == DBNull.Value)
            {
                switch (field.Type)
                {
                    case esriFieldType.esriFieldTypeDouble:
                        return 0.0;
                    case esriFieldType.esriFieldTypeInteger:
                        return 0;
                    case esriFieldType.esriFieldTypeOID:
                        return 0;
                    case esriFieldType.esriFieldTypeSingle:
                        return 0.0f;
                    case esriFieldType.esriFieldTypeSmallInteger:
                        return (short)0;
                    case esriFieldType.esriFieldTypeString:
                        return string.Empty;
                    default:
                        return DBNull.Value;
                }
            }
            else
            {
                return field.DefaultValue;
            }
        }

		private Dictionary<int, string> _GetJunctionClassLookup()
		{
			Dictionary<int, string> dicJunctionClasses = new Dictionary<int, string>();
			IEnumFeatureClass pJunctionLayers = _setupOp.GeometricNetwork.get_ClassesByType(esriFeatureType.esriFTSimpleJunction);
			IFeatureClass pJunctionLayer = pJunctionLayers.Next();
			while (pJunctionLayer != null)
			{
				try
				{
					dicJunctionClasses[pJunctionLayer.FeatureClassID] = ((IDataset)pJunctionLayer).Name;
				}
				finally
				{
					UrbanDelineationExtension.ReleaseComObject(pJunctionLayer);
				}
				pJunctionLayer = pJunctionLayers.Next();
			}

			return dicJunctionClasses;
		}

		private void _AddField(IFeatureClass pFeatureClass, string sFieldName, esriFieldType eFieldType)
		{
			IFieldEdit pField;
			pField = new FieldClass();
			pField.Name_2 = sFieldName;
			pField.DefaultValue_2 = 0.0;
			pField.Type_2 = eFieldType;
			if (eFieldType == esriFieldType.esriFieldTypeString)
				pField.Length_2 = 50;

			pFeatureClass.AddField(pField);
		}

        private bool _AddOutletIdField(IFeatureClass watershedClass, Dictionary<int, IGeometry> watershedShapes, INetElements netElements)
        {
            bool added = false;

            if (!string.IsNullOrEmpty(_idField))
            {
                if (_sourceLayer == null)
                {
                    foreach (int eid in watershedShapes.Keys)
                    {
                        int classID;
                        int oid;
                        int subID;
                        netElements.QueryIDs(eid, esriElementType.esriETJunction, out classID, out oid, out subID);

                        INetElementDescriptionEdit elementDescription = new NetElementDescriptionClass();
                        elementDescription.ElementType_2 = esriElementType.esriETJunction;
                        elementDescription.UserClassID_2 = classID;
                        elementDescription.UserID_2 = oid;
                        elementDescription.UserSubID_2 = subID;

                        IFeature netFeature = _setupOp.GeometricNetwork.get_NetworkFeature(elementDescription) as IFeature;
                        int netIdField = netFeature.Fields.FindField(_idField);
                        if (netIdField > -1)
                        {
                            if (_idField != ((IFeatureClass)netFeature.Class).ShapeFieldName)
                            {
                                IField netField = netFeature.Fields.get_Field(netIdField);

                                IFieldEdit field;
                                field = new FieldClass();
                                _outIdField = _GetUniqueFieldName(_idField, watershedClass);
                                field.Name_2 = _outIdField;
                                field.AliasName_2 = netField.AliasName;
                                field.IsNullable_2 = true;
                                field.DefaultValue_2 = netField.DefaultValue;
                                if (netField.Type == esriFieldType.esriFieldTypeOID)
                                    field.Type_2 = esriFieldType.esriFieldTypeInteger;
                                else
                                    field.Type_2 = netField.Type;
                                field.Length_2 = netField.Length;
                                field.Precision_2 = netField.Precision;
                                field.Scale_2 = netField.Scale;

                                watershedClass.AddField(field);

                                added = true;
                                break;
                            }
                        }

                        if (added)
                            break;
                    }
                }
                else
                {
                    IFeatureClass sourceClass = _sourceLayer.FeatureClass;
                    if (sourceClass != null)
                    {
                        try
                        {
                            if (_idField != sourceClass.ShapeFieldName)
                            {
                                int sourceIdField = sourceClass.Fields.FindField(_idField);
                                if (sourceIdField != -1)
                                {
                                    IField sourceField = sourceClass.Fields.get_Field(sourceIdField);

                                    IFieldEdit field;
                                    field = new FieldClass();
                                    _outIdField = _GetUniqueFieldName(_idField, watershedClass);
                                    field.Name_2 = _outIdField;
                                    field.AliasName_2 = sourceField.AliasName;
                                    field.IsNullable_2 = true;
                                    field.DefaultValue_2 = sourceField.DefaultValue;
                                    if (sourceField.Type == esriFieldType.esriFieldTypeOID)
                                        field.Type_2 = esriFieldType.esriFieldTypeInteger;
                                    else
                                        field.Type_2 = sourceField.Type;
                                    field.Length_2 = sourceField.Length;
                                    field.Precision_2 = sourceField.Precision;
                                    field.Scale_2 = sourceField.Scale;

                                    watershedClass.AddField(field);

                                    added = true;
                                }
                            }
                        }
                        finally
                        {
                            UrbanDelineationExtension.ReleaseComObject(sourceClass);
                        }
                    }
                }
            }

            return added;
        }

        private string _GetUniqueFieldName(string baseName, IFeatureClass featureClass)
        {
            int n = 1;
            string fieldName = baseName;
            while (featureClass.FindField(fieldName) != -1)
            {
                fieldName = string.Format("{0}_{1}", baseName, n++);
            }
            return fieldName;
        }

		private IGeometry _MergeCatchments(int[] inletEIDs)
		{
			IGeometry watershedShape = new PolygonClass();
			List<IGeometry> catchmentShapes = new List<IGeometry>(inletEIDs.Length);

			IFeatureCursor cursor = null;
			try
			{
				List<int> inletEIDsList = new List<int>(inletEIDs);
				inletEIDsList.Sort();

				int gridCodeField = _setupOp.Catchments.FindField("GridCode");
				object dbValue = null;
				int gridCode = -1;

				//Get all watershed shapes
                cursor = _setupOp.Catchments.Search(null, true);
				IFeature catchment = cursor.NextFeature();
				while (catchment != null)
				{
					try
					{
						dbValue = catchment.get_Value(gridCodeField);
						gridCode = Convert.IsDBNull(dbValue) ? -1 : Convert.ToInt32(dbValue);
						if (inletEIDsList.BinarySearch(gridCode) >= 0)
							catchmentShapes.Add(catchment.ShapeCopy);
					}
					finally
					{
						UrbanDelineationExtension.ReleaseComObject(catchment);
					}
					catchment = cursor.NextFeature();
				}

				//Merge shapes
				while (catchmentShapes.Count > 1)
				{
					List<IGeometry> tempList = new List<IGeometry>(catchmentShapes.Count / 2);
					for (int i = 0; i < catchmentShapes.Count; i += 2)
					{
						if (i < catchmentShapes.Count - 1)
							tempList.Add(((ITopologicalOperator)catchmentShapes[i]).Union(catchmentShapes[i + 1]));
						else
							tempList.Add(catchmentShapes[i]); //Last shape in odd numbered list
					}
					catchmentShapes = tempList;
				}

                if (catchmentShapes.Count == 1)
                {
                    watershedShape = catchmentShapes[0];
                    watershedShape.SpatialReference = ((IGeoDataset)_setupOp.Catchments).SpatialReference;
                }
			}
			finally
			{
				UrbanDelineationExtension.ReleaseComObject(cursor);
			}

			return watershedShape;
		}

		private int[] _TraceUpstream(int baseEID, bool firstNode)
		{
			List<int> inletEIDs = new List<int>();
			List<int> traversedEIDs = new List<int>();
			INetElements netElements = _setupOp.GeometricNetwork.Network as INetElements;
			IForwardStarGEN forwardStar = _setupOp.GeometricNetwork.Network.CreateForwardStar(false, null, null, null, null) as IForwardStarGEN;
			int inletClassId = _setupOp.InletClass.FeatureClassID;

			_GetUpstreamInlets(baseEID, forwardStar, netElements, inletClassId, inletEIDs, traversedEIDs, firstNode);

			return inletEIDs.ToArray();
		}

		private void _GetUpstreamInlets(int baseEID, IForwardStarGEN forwardStar, INetElements netElements, int inletClassId, List<int> inletEIDs, List<int> traversedEIDs, bool firstNode)
		{
			// Break out of loops
			if (traversedEIDs.Contains(baseEID))
				return;
            else
    		    traversedEIDs.Add(baseEID);

            // First node is always treated as enabled, since the user has selected it.
            bool junctionDisabled = firstNode ? false : _CheckDisabled(baseEID, esriElementType.esriETJunction, netElements);
            if (!junctionDisabled)
            {
                int edgeCount = 0;
                forwardStar.FindAdjacent(0, baseEID, out edgeCount);

                //Initialize to true only if edges connect to this junction (edgeCount > 0)
                bool upstreamEnd = edgeCount > 0;
                bool downstreamEnd = edgeCount > 0;

                if (edgeCount > 0)
                {
                    int[] edgeEIDs = new int[edgeCount];
                    int[] junctionEIDs = new int[edgeCount];
                    bool[] towardsJunction = new bool[edgeCount];
                    object[] weights = new object[edgeCount];

                    forwardStar.QueryAdjacentEdges(ref edgeEIDs, ref towardsJunction, ref weights);
                    forwardStar.QueryAdjacentJunctions(ref junctionEIDs, ref weights);
                    for (int i = 0; i < edgeCount; i++)
                    {
                        if (towardsJunction[i])
                        {
                            upstreamEnd = false;
                            bool edgeDisabled = _CheckDisabled(edgeEIDs[i], esriElementType.esriETEdge, netElements);
                            if (!edgeDisabled)
                            {
                                _GetUpstreamInlets(junctionEIDs[i], forwardStar, netElements, inletClassId, inletEIDs, traversedEIDs, false);
                            }
                        }
                        else
                        {
                            downstreamEnd = false;
                        }
                    }
                }

                if (upstreamEnd && _setupOp.IncludeUpstreamPipeEnds)
                {
                    inletEIDs.Add(baseEID);
                }
                else if (!(downstreamEnd && _setupOp.ExcludeDownstreamPipeEnds))
                {
                    int classId;
                    int userId;
                    int subId;
                    netElements.QueryIDs(baseEID, esriElementType.esriETJunction, out classId, out userId, out subId);
                    if (classId == inletClassId)
                        inletEIDs.Add(baseEID);
                }
            }
		}

        private bool _CheckDisabled(int eid, esriElementType elementType, INetElements netElements)
        {
            bool enabled = true;
            if (_stopAtDisabled)
            {
                // Check enabled field in Network Feature since it might not be in synch with network element enabled property
                int classID, userID, subID;
                netElements.QueryIDs(eid, elementType, out classID, out userID, out subID);

                INetElementDescriptionEdit elementDescription = new NetElementDescriptionClass();
                elementDescription.ElementType_2 = elementType;
                elementDescription.UserClassID_2 = classID;
                elementDescription.UserID_2 = userID;
                elementDescription.UserSubID_2 = subID;

                INetworkFeature netFeature = _setupOp.GeometricNetwork.get_NetworkFeature(elementDescription);
                IFeature feature = (IFeature)netFeature;
                int enabledField = feature.Fields.FindField("Enabled");
                if (enabledField > -1)
                {
                    object enabledValue = feature.get_Value(enabledField);
                    if (enabledValue != DBNull.Value)
                    {
                        enabled = Convert.ToBoolean(enabledValue);
                    }
                }
            }
            return !enabled;
        }

		private int[] _GetSelectedJunctions()
		{
			List<int> lstJunctionIDs = new List<int>();
			IGeometricNetwork pGeoNetwork = _setupOp.GeometricNetwork;
			INetElements pNetElements = pGeoNetwork.Network as INetElements;

			//Get Selected Junction IDs
			IFeatureLayer[] pJunctionLayers = _GetJunctionLayers();
			foreach (IFeatureLayer pJunctionLayer in pJunctionLayers)
			{
				int iClassID = pJunctionLayer.FeatureClass.FeatureClassID;
				IEnumIDs pOIDs = ((IFeatureSelection)pJunctionLayer).SelectionSet.IDs;
				
				int iOID = pOIDs.Next();
				while (iOID != -1)
				{
					lstJunctionIDs.Add(pNetElements.GetEID(iClassID, iOID, -1, esriElementType.esriETJunction));
					iOID = pOIDs.Next();
				}
			}

			//TODO: Add downstream junctions from selected edges (but only delineate up the selected edge)
			//List<int> lstEdgeClassIDs = _GetNetworkClassIDs(pGeoNetwork, esriFeatureType.esriFTSimpleEdge);

			return lstJunctionIDs.ToArray();
		}

		private IFeatureLayer[] _GetJunctionLayers()
		{
			List<IFeatureLayer> lstJunctionLayers = new List<IFeatureLayer>();

			List<int> lstJunctionClassIDs = _GetNetworkClassIDs(esriFeatureType.esriFTSimpleJunction);

			IMxDocument pMxDoc = null;
			IMap pMap = null;
			try
			{
				pMxDoc = this._application.Document as IMxDocument;
				pMap = pMxDoc.FocusMap;

				for (int i = 0; i < pMap.LayerCount; i++)
				{
					ILayer pLayer = pMap.get_Layer(i);
					if (pLayer is ICompositeLayer)
					{
						_GetJunctionLayers((ICompositeLayer)pLayer, lstJunctionClassIDs, lstJunctionLayers);
					}
					else if(pLayer is IFeatureLayer)
					{
						IFeatureLayer pFeatureLayer = (IFeatureLayer)pLayer;
						if(pFeatureLayer.FeatureClass != null)
							if (lstJunctionClassIDs.Contains(pFeatureLayer.FeatureClass.FeatureClassID))
								lstJunctionLayers.Add(pFeatureLayer);
					}
				}
			}
			finally
			{
				UrbanDelineationExtension.ReleaseComObject(pMap);
				UrbanDelineationExtension.ReleaseComObject(pMxDoc);
			}

			return lstJunctionLayers.ToArray();
		}

		private void _GetJunctionLayers(ICompositeLayer pCompositeLayer, List<int> lstJunctionClassIDs, List<IFeatureLayer> lstJunctionLayers)
		{
			for (int i = 0; i < pCompositeLayer.Count; i++)
			{
				ILayer pLayer = pCompositeLayer.get_Layer(i);
				if (pLayer is ICompositeLayer)
				{
					_GetJunctionLayers((ICompositeLayer)pLayer, lstJunctionClassIDs, lstJunctionLayers);
				}
				else if (pLayer is IFeatureLayer)
				{
					IFeatureLayer pFeatureLayer = (IFeatureLayer)pLayer;
					if (lstJunctionClassIDs.Contains(pFeatureLayer.FeatureClass.FeatureClassID))
						lstJunctionLayers.Add(pFeatureLayer);
				}
			}
		}

		private List<int> _GetNetworkClassIDs(esriFeatureType eFeatureType)
		{
			List<int> lstClassIDs = new List<int>();

			IEnumFeatureClass pNetworkClasses = _setupOp.GeometricNetwork.get_ClassesByType(eFeatureType);
			IFeatureClass pNetworkClass = pNetworkClasses.Next();
			while (pNetworkClass != null)
			{
				try
				{
					lstClassIDs.Add(pNetworkClass.FeatureClassID);
				}
				finally
				{
					UrbanDelineationExtension.ReleaseComObject(pNetworkClass);
				}
				pNetworkClass = pNetworkClasses.Next();
			}

			return lstClassIDs;
		}

        private void _OutputSettings()
        {
            if (_outletWatersheds != null)
            {
                XmlDocument settingsDoc = new XmlDocument();

                // Root node
                XmlElement rootNode = settingsDoc.CreateElement("UrbanDelineation");
                settingsDoc.AppendChild(rootNode);

                {
                    //Environment
                    XmlElement environmentElement = settingsDoc.CreateElement("Environment");
                    rootNode.AppendChild(environmentElement);

                    XmlElement userElement = settingsDoc.CreateElement("User");
                    string user = string.Format("{0}\\{1}", Environment.UserDomainName, Environment.UserName);
                    userElement.InnerText = user;
                    environmentElement.AppendChild(userElement);

                    XmlElement machineElement = settingsDoc.CreateElement("Machine");
                    machineElement.InnerText = Environment.MachineName;
                    environmentElement.AppendChild(machineElement);

                    XmlElement timeElement = settingsDoc.CreateElement("ExecutionDateTime");
                    timeElement.InnerText = DateTime.Now.ToString();
                    environmentElement.AppendChild(timeElement);
                }

                {
                    // Preprocessing settings
                    XmlElement preprocessNode = settingsDoc.CreateElement("Preprocessing");
                    rootNode.AppendChild(preprocessNode);

                    XmlElement networkElement = settingsDoc.CreateElement("PipeNetwork");
                    networkElement.InnerText = _GetDatasetPath(_setupOp.GeometricNetwork as IDataset);
                    preprocessNode.AppendChild(networkElement);

                    XmlElement inletsElement = settingsDoc.CreateElement("NetworkInlets");
                    inletsElement.InnerText = _GetDatasetPath(_setupOp.InletClass as IDataset);
                    preprocessNode.AppendChild(inletsElement);

                    XmlElement upstreamEndsElement = settingsDoc.CreateElement("IncludeUpstreamEnds");
                    upstreamEndsElement.InnerText = _setupOp.IncludeUpstreamPipeEnds.ToString();
                    preprocessNode.AppendChild(upstreamEndsElement);

                    XmlElement excludeDisabledPreprocessElement = settingsDoc.CreateElement("ExcludeDisabledInlets");
                    excludeDisabledPreprocessElement.InnerText = _setupOp.ExcludeDisabledNodes.ToString();
                    preprocessNode.AppendChild(excludeDisabledPreprocessElement);

                    XmlElement excludeDownstreamEndsElement = settingsDoc.CreateElement("ExcludeDownstreamEnds");
                    excludeDownstreamEndsElement.InnerText = _setupOp.ExcludeDownstreamPipeEnds.ToString();
                    preprocessNode.AppendChild(excludeDownstreamEndsElement);

                    XmlElement demElement = settingsDoc.CreateElement("DEM");
                    if (_setupOp.DEM != null)
                        demElement.InnerText = _GetDatasetPath(((IRasterAnalysisProps)_setupOp.DEM).RasterDataset as IDataset);
                    else
                        demElement.InnerText = _GetDatasetPath(null);
                    preprocessNode.AppendChild(demElement);

                    XmlElement smoothElement = settingsDoc.CreateElement("SmoothCatchments");
                    smoothElement.InnerText = _setupOp.SmoothBoundaries.ToString();
                    preprocessNode.AppendChild(smoothElement);

                    XmlElement resultsNode = settingsDoc.CreateElement("Results");
                    preprocessNode.AppendChild(resultsNode);

                    XmlElement flowDirElement = settingsDoc.CreateElement("FlowDirection");
                    if (_setupOp.FlowDirection != null)
                        flowDirElement.InnerText = _GetDatasetPath(((IRasterAnalysisProps)_setupOp.FlowDirection).RasterDataset as IDataset);
                    else
                        flowDirElement.InnerText = _GetDatasetPath(null);
                    resultsNode.AppendChild(flowDirElement);

                    XmlElement inletCatchmentsElement = settingsDoc.CreateElement("InletCatchments");
                    inletCatchmentsElement.InnerText = _GetDatasetPath(_setupOp.Catchments as IDataset);
                    resultsNode.AppendChild(inletCatchmentsElement);
                }

                {
                    // Delineation Settings
                    XmlElement delineationNode = settingsDoc.CreateElement("Delineation");
                    rootNode.AppendChild(delineationNode);

                    XmlElement outletSourceElement = settingsDoc.CreateElement("OutletLocations");
                    if (this.OutletSource == null)
                        outletSourceElement.InnerText = "(Selected Network Nodes)";
                    else
                        outletSourceElement.InnerText = _GetDatasetPath(this.OutletSource.FeatureClass as IDataset);
                    delineationNode.AppendChild(outletSourceElement);

                    XmlElement outletFieldElement = settingsDoc.CreateElement("OutletLabelField");
                    if (string.IsNullOrEmpty(this.OutletIdField))
                        outletFieldElement.InnerText = "(None)";
                    else
                        outletFieldElement.InnerText = _idField;
                    delineationNode.AppendChild(outletFieldElement);

                    XmlElement extendOverlandElement = settingsDoc.CreateElement("ExtendOverland");
                    extendOverlandElement.InnerText = this.ExtendOverland.ToString();
                    delineationNode.AppendChild(extendOverlandElement);

                    XmlElement stopAtDisabledElement = settingsDoc.CreateElement("StopTracingAtDisabledFeatures");
                    stopAtDisabledElement.InnerText = this.StopAtDisabledFeatures.ToString();
                    delineationNode.AppendChild(stopAtDisabledElement);

                    XmlElement snapPointsElement = settingsDoc.CreateElement("SnapPointsToStream");
                    snapPointsElement.InnerText = this.SnapToPourPoint.ToString();
                    delineationNode.AppendChild(snapPointsElement);

                    if (this.SnapToPourPoint)
                    {
                        XmlElement snapDistanceElement = settingsDoc.CreateElement("SnapDistance");
                        snapDistanceElement.InnerText = this.SnapDistance.ToString();
                        delineationNode.AppendChild(snapDistanceElement);
                    }
                }

                // Write file
                string xmlPath = _GetDatasetPath((IDataset)_outletWatersheds);
                xmlPath = System.IO.Path.ChangeExtension(xmlPath, ".settings.xml");
                using (XmlTextWriter xmlWriter = new XmlTextWriter(xmlPath, Encoding.UTF8))
                {
                    xmlWriter.Formatting = Formatting.Indented;
                    settingsDoc.Save(xmlWriter);
                }
            }
        }

        private static string _GetDatasetPath(IDataset dataset)
        {
            string datasetPath = UrbanDelineationExtension.GetDatasetPath(dataset);
            return datasetPath == null ? "(Null)" : datasetPath;
        }
	}
}
