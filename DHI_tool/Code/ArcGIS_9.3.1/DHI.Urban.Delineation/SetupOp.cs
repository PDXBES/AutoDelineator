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
using System.IO;
using System.Runtime.InteropServices;
using ESRI.ArcGIS.GeoAnalyst;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.DataSourcesRaster;
using ESRI.ArcGIS.SpatialAnalyst;
using ESRI.ArcGIS.esriSystem;

namespace DHI.Urban.Delineation
{
	public class SetupOp : IDisposable
	{
		public const string EID_FIELD_NAME = "EID";

		private IGeometricNetwork _geometricNetwork;
		private IFeatureClass _inletClass;
		private bool _includeUpstreamEnds = false;
        private bool _excludeDownstreamEnds = true;
        private bool _excludeDisabled = false;
        private bool _smooth = true;
        private IRaster _dem;
		private IRaster _punchedDEM;
		private IRaster _filledDEM;
		private IRaster _flowDir;
        private IFeatureClass _drainClass;
        private IFeatureClass _catchmentClass;
		private string _tempDir = @"C:\Temp";
        private string _resultDir = @"C:\Temp";
		private bool _disposed = false;
        private List<IDataset> _tempDatasets = new List<IDataset>(); // Used to delay disposal of certain temp datasets.

		public SetupOp()
		{
		}

		~SetupOp()
		{
			Dispose(false);
		}

        #region IDisposable Members

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion

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

                UrbanDelineationExtension.ReleaseComObject(_geometricNetwork);
				_geometricNetwork = null;
                UrbanDelineationExtension.ReleaseComObject(_inletClass);
				_inletClass = null;
                UrbanDelineationExtension.ReleaseComObject(_dem);
				_dem = null;
                UrbanDelineationExtension.ReleaseComObject(_punchedDEM);
				_punchedDEM = null;
                UrbanDelineationExtension.ReleaseComObject(_filledDEM);
				_filledDEM = null;
                UrbanDelineationExtension.ReleaseComObject(_flowDir);
				_flowDir = null;
                UrbanDelineationExtension.ReleaseComObject(_drainClass);
                _drainClass = null;
                UrbanDelineationExtension.ReleaseComObject(_catchmentClass);
                _catchmentClass = null;

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
                            System.Diagnostics.Debug.WriteLine("SetupOp: " + ex.GetType().FullName + ": " + ex.Message);
                        }
                    }
                }
            }
            finally
            {
                _tempDatasets.Clear();
            }
        }

        public static string CreateTempFileName(string directoryPath, string baseName, string extension)
        {
            DirectoryInfo directory = new DirectoryInfo(directoryPath);
            if (!directory.Exists)
            {
                throw new DirectoryNotFoundException();
            }

            if (extension == null)
            {
                extension = string.Empty;
            }

            if (extension.Length > 0 && !extension.StartsWith("."))
            {
                extension = "." + extension;
            }

            int subItemCount = directory.GetFileSystemInfos(baseName + "*").Length;

            string result = null;
            string fileName = null;
            for (int i = 1; i < subItemCount + 2; i++)
            {
                fileName = baseName + i.ToString(System.Globalization.CultureInfo.CurrentCulture) + extension;
                result = System.IO.Path.Combine(directoryPath, fileName);
                if (!File.Exists(result) && !Directory.Exists(result))
                    return result;
            }

            throw new Exception("Could not create temporary file name!");
        }

		/// <summary>
		/// The sewer network.
		/// </summary>
		public IGeometricNetwork GeometricNetwork
		{
			get { return _geometricNetwork; }
			set
			{
                UrbanDelineationExtension.ReleaseComObject(_geometricNetwork);

				_geometricNetwork = value;

                if (_geometricNetwork != null && Marshal.IsComObject(_geometricNetwork))
                {
                    IntPtr ptr = Marshal.GetIUnknownForObject(_geometricNetwork);
                    object obj = Marshal.GetObjectForIUnknown(ptr);
                }
			}
		}

		/// <summary>
		/// The class which contains the inlet points in the sewer network.
		/// </summary>
		/// <remarks>Must be part of the geometric network set on the GeometricNetwork property.</remarks>
		public IFeatureClass InletClass
		{
			get { return _inletClass; }
			set
			{
                UrbanDelineationExtension.ReleaseComObject(_inletClass);

                _inletClass = value;

                if (_inletClass != null && Marshal.IsComObject(_inletClass))
                {
                    IntPtr ptr = Marshal.GetIUnknownForObject(_inletClass);
                    object obj = Marshal.GetObjectForIUnknown(ptr);
                }
            }
		}

		/// <summary>
		/// Whether to include the upstream ends of pipes when punching drainage holes
		/// in the DEM, regardless of their type. Default is false.
		/// </summary>
		public bool IncludeUpstreamPipeEnds
		{
			get { return _includeUpstreamEnds; }
			set { _includeUpstreamEnds = value; }
		}

        /// <summary>
        /// Whether to exclude the downstream ends of pipes when punching drainage holes
        /// in the DEM, regardless of their type. Should generally always be true.
        /// </summary>
        public bool ExcludeDownstreamPipeEnds
        {
            get { return _excludeDownstreamEnds; }
            set { _excludeDownstreamEnds = value; }
        }

        /// <summary>
        /// Whether to exclude the disabled nodes when punching holes
        /// in the DEM, regardless of their type. Default is false.
        /// </summary>
        public bool ExcludeDisabledNodes
        {
            get { return _excludeDisabled; }
            set { _excludeDisabled = value; }
        }

        /// <summary>
        /// Whether to smooth resulting delineation boundaries. If false, the boundaries will follow cell boundaries.
        /// </summary>
        public bool SmoothBoundaries
        {
            get { return _smooth; }
            set { _smooth = value; }
        }

		/// <summary>
		/// The original DEM
		/// </summary>
		public IRaster DEM
		{
			get { return _dem; }
			set
			{
                UrbanDelineationExtension.ReleaseComObject(_dem);

                _dem = value;

                if (_dem != null && Marshal.IsComObject(_dem))
                {
                    IntPtr ptr = Marshal.GetIUnknownForObject(_dem);
                    object obj = Marshal.GetObjectForIUnknown(ptr);
                }
            }
		}

        /// <summary>
        /// The extracted drainage points to be punched into the DEM
        /// </summary>
        public IFeatureClass DrainagePoints
        {
            get
            {
                return _drainClass;
            }

            set
            {
                UrbanDelineationExtension.ReleaseComObject(_drainClass);

                _drainClass = value;

                if (_drainClass != null && Marshal.IsComObject(_drainClass))
                {
                    IntPtr ptr = Marshal.GetIUnknownForObject(_drainClass);
                    object obj = Marshal.GetObjectForIUnknown(ptr);
                }
            }
        }

		/// <summary>
		/// The DEM with drainage holes "punched" into it.
		/// </summary>
		public IRaster PunchedDEM
		{
			get { return _punchedDEM; }
			set
			{
                UrbanDelineationExtension.ReleaseComObject(_punchedDEM);

                _punchedDEM = value;

                if (_punchedDEM != null && Marshal.IsComObject(_punchedDEM))
                {
                    IntPtr ptr = Marshal.GetIUnknownForObject(_punchedDEM);
                    object obj = Marshal.GetObjectForIUnknown(ptr);
                }
            }
		}

		/// <summary>
		/// The Filled DEM used to create the flow direction grid.
		/// </summary>
		public IRaster FilledDEM
		{
			get { return _filledDEM; }
			set
			{
                UrbanDelineationExtension.ReleaseComObject(_filledDEM);

                _filledDEM = value;

                if (_filledDEM != null && Marshal.IsComObject(_filledDEM))
                {
                    IntPtr ptr = Marshal.GetIUnknownForObject(_filledDEM);
                    object obj = Marshal.GetObjectForIUnknown(ptr);
                }
            }
		}

		/// <summary>
		/// The resulting flow direction grid.
		/// </summary>
		public IRaster FlowDirection
		{
			get
			{ 
				return _flowDir; 
			}
			set
			{
                UrbanDelineationExtension.ReleaseComObject(_flowDir);

                _flowDir = value;

                if (_flowDir != null && Marshal.IsComObject(_flowDir))
                {
                    IntPtr ptr = Marshal.GetIUnknownForObject(_flowDir);
                    object obj = Marshal.GetObjectForIUnknown(ptr);
                }
            }
		}
        
        /// <summary>
        /// The individual catchments for each inlet point
        /// </summary>
        public IFeatureClass Catchments
        {
            get
            {
                return _catchmentClass;
            }

            set
            {
                UrbanDelineationExtension.ReleaseComObject(_catchmentClass);

                _catchmentClass = value;

                if (_catchmentClass != null && Marshal.IsComObject(_catchmentClass))
                {
                    IntPtr ptr = Marshal.GetIUnknownForObject(_catchmentClass);
                    object obj = Marshal.GetObjectForIUnknown(ptr);
                }
            }
        }

		/// <summary>
		/// The scratch directory where temporary files should be written.
		/// </summary>
		public string ScratchDirectory
		{
			get { return _tempDir; }
			set { _tempDir = value; }
		}

        /// <summary>
        /// The output directory for "permanent" result files.
        /// </summary>
        public string ResultsDirectory
        {
            get { return _resultDir; }
            set { _resultDir = value; }
        }

        public void Preprocess()
        {
            // Clear current results
            this.DrainagePoints = null;
            this.PunchedDEM = null;
            this.FilledDEM = null;
            this.FlowDirection = null;
            this.Catchments = null;

            _ExtractDrainagePoints();
            _PunchDEM();
            _CalculateFlowDir();
            _DelineateInletCatchments();

            _DisposeTempDatasets();
        }

		private void _ExtractDrainagePoints()
		{
            Dictionary<IPoint, int> drainagePoints = new Dictionary<IPoint, int>();
            if (_includeUpstreamEnds)
            {
                drainagePoints = _ExtractUpstreamPipeEnds();
            }

            List<int> excludedOids = new List<int>();
            if (_excludeDownstreamEnds)
            {
                excludedOids = _FindDownstreamPipeEnds();
            }

			//Add inlets
			IFeatureCursor cursor = null;
			try
			{
				cursor = _inletClass.Search(null, false);
				IFeature inlet = cursor.NextFeature();
				while(inlet != null)
				{
					try
					{
                        if (!(_excludeDownstreamEnds && excludedOids.Contains(inlet.OID)))
                        { 
                            bool enabled = true;
                            if(_excludeDisabled)
                                enabled = _IsEnabled(inlet);
                            if(enabled)
                                drainagePoints.Add((IPoint)inlet.ShapeCopy, ((ISimpleJunctionFeature)inlet).EID);
                        }
					}
					finally
					{
                        UrbanDelineationExtension.ReleaseComObject(inlet);
					}
					inlet = cursor.NextFeature();
				}
			}
			finally
			{
                UrbanDelineationExtension.ReleaseComObject(cursor);
			}

			//Create FeatureClass
            string outputPath = CreateTempFileName(_GetResultDir(), "DrainPoints", "shp");
            string outputName = System.IO.Path.GetFileNameWithoutExtension(outputPath);
            _drainClass = CreateShapefile(_GetResultDir(), outputName, esriGeometryType.esriGeometryPoint, new UnknownCoordinateSystemClass(), SetupOp.EID_FIELD_NAME);

            cursor = _drainClass.Insert(true);
			try
			{
                int idFieldIndex = _drainClass.FindField(EID_FIELD_NAME);
                IFeatureBuffer buffer = _drainClass.CreateFeatureBuffer();
				foreach (IPoint point in drainagePoints.Keys)
				{
					buffer.Shape = point;
					buffer.set_Value(idFieldIndex, drainagePoints[point]);
					cursor.InsertFeature(buffer);
				}
			}
			finally
			{
                UrbanDelineationExtension.ReleaseComObject(cursor);
			}
		}

		internal IFeatureClass CreateShapefile(string path, string name, esriGeometryType geometryType, ISpatialReference spatialRef, string idFieldName)
		{
			const string SHAPE_FIELD = "Shape";

			IFeatureClass shapeFile = null;
			IWorkspace workspace = null;
			try
			{
				workspace = OpenShapeFileWorkspace(path);
				IFeatureWorkspace featureWorkspace = workspace as IFeatureWorkspace;

				//Delete any existing
				IEnumDatasetName datasets = workspace.get_DatasetNames(esriDatasetType.esriDTFeatureClass);
				datasets.Reset();
				IDataset existing = null;
				IDatasetName datasetName = datasets.Next();
				while (datasetName != null)
				{
					if (string.Compare(name, datasetName.Name, true) == 0)
					{
						existing = (IDataset)((IName)datasetName).Open();
						break;
					}
					datasetName = datasets.Next();
				}
				if (existing != null)
				{
					try
					{
						existing.Delete();
					}
					finally
					{
                        UrbanDelineationExtension.ReleaseComObject(existing);
					}
				}

				//Get elements to create a new table/feature class
				IFieldsEdit fields = new FieldsClass();

				IGeometryDefEdit geometryDef = new GeometryDefClass();
				geometryDef.GeometryType_2 = geometryType;
				geometryDef.SpatialReference_2 = spatialRef;

				IFieldEdit shapeField = new FieldClass();
				shapeField.Name_2 = SHAPE_FIELD;
				shapeField.Type_2 = esriFieldType.esriFieldTypeGeometry;
				shapeField.GeometryDef_2 = geometryDef;
				fields.AddField(shapeField);

				IFieldEdit eidField = new FieldClass();
				eidField.Name_2 = idFieldName;
				eidField.Type_2 = esriFieldType.esriFieldTypeInteger;
				eidField.Length_2 = 16;
				fields.AddField(eidField as IField);

				//Create feature class
				shapeFile = featureWorkspace.CreateFeatureClass(name, fields, null, null, esriFeatureType.esriFTSimple, SHAPE_FIELD, null);
			}
			finally
			{
                UrbanDelineationExtension.ReleaseComObject(workspace);
			}

			return shapeFile;
		}

		/// <summary>
		/// Sets the output environment for this raster tool, using the default extent (of the DEM)
		/// </summary>
		/// <param name="pRasterTool">The tool to set the ouput environment for.</param>
		/// <param name="sSubDir">The subdirectory to use for output.</param>
		internal void SetAnalysisEnvironment(IRasterAnalysisEnvironment rasterTool)
		{
            object inputGrid = _GetDefaultGrid();
            SetAnalysisEnvironment(rasterTool, inputGrid);
		}

        /// <summary>
        /// Sets the output environment for this raster tool, using the specified extent for output.
        /// </summary>
        /// <param name="pRasterTool">The tool to set the ouput environment for.</param>
        /// <param name="sSubDir">The subdirectory to use for output.</param>
        /// <param name="extentProvider">An IEnvelope or RasterDataset that defines the output extent.</param>
        /// <remarks>The extentProvider determines the extent, but the default raster (the input DEM) determines cell size
        /// and snap registration of the cells.</remarks>
        internal void SetAnalysisEnvironment(IRasterAnalysisEnvironment rasterTool, object extentProvider)
        {
            object inputGrid = _GetDefaultGrid();

            rasterTool.DefaultOutputRasterPrefix = "UrbDelTmp";
            rasterTool.OutWorkspace = GetTempRasterWorkspace();
            rasterTool.OutSpatialReference = ((IGeoDataset)inputGrid).SpatialReference;
            rasterTool.SetCellSize(esriRasterEnvSettingEnum.esriRasterEnvValue, ref inputGrid);
            rasterTool.SetExtent(esriRasterEnvSettingEnum.esriRasterEnvValue, ref extentProvider, ref inputGrid);
        }

        private object _GetDefaultGrid()
        {
            object inputGrid = null;
            if (_dem != null)
                inputGrid = _dem;
            else if (_punchedDEM != null)
                inputGrid = _punchedDEM;
            else if (_flowDir != null)
                inputGrid = _flowDir;

            return inputGrid;
        }

		internal IWorkspace GetTempRasterWorkspace()
		{
			IWorkspaceFactory pWSFactory = new RasterWorkspaceFactoryClass();
			return pWSFactory.OpenFromFile(_GetTempDir(), 0);
		}

        internal IWorkspace GetResultRasterWorkspace()
        {
            IWorkspaceFactory pWSFactory = new RasterWorkspaceFactoryClass();
            return pWSFactory.OpenFromFile(_GetResultDir(), 0);
        }

		private string _GetTempDir()
		{
			Directory.CreateDirectory(_tempDir);
			return _tempDir;
		}

        private string _GetResultDir()
        {
            Directory.CreateDirectory(_resultDir);
            return _resultDir;
        }

		/// <summary>
		/// Opens a shapefile workspace and set the Workspace property to the opened workspace.
		/// </summary>
		/// <param name="szShapeFilePath">A path to a directory containing shapefiles.</param>
		/// <returns>IWorkspace interface to opened workspace.</returns>
		internal static IWorkspace OpenShapeFileWorkspace(string szShapeFilePath)
		{
			IWorkspaceName pWSName = new WorkspaceNameClass();
			pWSName.WorkspaceFactoryProgID = "esriDataSourcesFile.ShapefileWorkspaceFactory";
			pWSName.PathName = szShapeFilePath;

			IName pName = (IName)pWSName;
			return pName.Open() as IWorkspace;
		}

		private void _PunchDEM()
		{
			IConversionOp conversionOp = new RasterConversionOpClass();
			ILogicalOp logicalOp = new RasterMathOpsClass();
			IConditionalOp conditionalOp = new RasterConditionalOpClass();
            IGeoDataset punchedDEM = null;
			IWorkspace tempWorkspace = null;
            IWorkspace resultWorkspace = null;
			try
			{
				tempWorkspace = GetTempRasterWorkspace();

				SetAnalysisEnvironment((IRasterAnalysisEnvironment)conversionOp);
				SetAnalysisEnvironment((IRasterAnalysisEnvironment)logicalOp);
                SetAnalysisEnvironment((IRasterAnalysisEnvironment)conditionalOp);

				IFeatureClassDescriptor sourceDescriptor = new FeatureClassDescriptorClass();
				sourceDescriptor.Create(_drainClass, null, _drainClass.OIDFieldName);
                string gridPath = SetupOp.CreateTempFileName(_GetTempDir(), "TmpInlets", null);
				string gridName = System.IO.Path.GetFileName(gridPath);

				IRasterDataset pRasterDataset = null;
				try
				{
					pRasterDataset = conversionOp.ToRasterDataset((IGeoDataset)sourceDescriptor, "GRID", tempWorkspace, gridName);
					punchedDEM = conditionalOp.SetNull(logicalOp.BooleanNot(logicalOp.IsNull((IGeoDataset)pRasterDataset)), (IGeoDataset)_dem);

                    resultWorkspace = GetResultRasterWorkspace();
                    ITemporaryDataset tempDataset = ((IRasterAnalysisProps)punchedDEM).RasterDataset as ITemporaryDataset;
                    string outputPath = CreateTempFileName(_GetResultDir(), "punchdem", "");
                    string outputFileName = System.IO.Path.GetFileName(outputPath);
                    tempDataset.MakePermanentAs(outputFileName, resultWorkspace, "GRID");
                    _punchedDEM = ((IRasterWorkspace)resultWorkspace).OpenRasterDataset(outputFileName).CreateDefaultRaster();
				}
				finally
				{
					if (((IDataset)pRasterDataset).CanDelete())
						((IDataset)pRasterDataset).Delete();
                    UrbanDelineationExtension.ReleaseComObject(pRasterDataset);
				}
			}
			finally
			{
                UrbanDelineationExtension.ReleaseComObject(tempWorkspace);
                UrbanDelineationExtension.ReleaseComObject(resultWorkspace);
                UrbanDelineationExtension.ReleaseComObject(punchedDEM);
                UrbanDelineationExtension.ReleaseComObject(conversionOp);
                UrbanDelineationExtension.ReleaseComObject(conditionalOp);
                UrbanDelineationExtension.ReleaseComObject(logicalOp);
			}
		}

		private void _CalculateFlowDir()
		{
			// - Fill & calculate flow direction
            IHydrologyOp hydroOp = new RasterHydrologyOpClass();
            IRasterMakerOp rasterMaker = new RasterMakerOpClass();
			ILogicalOp logicalOp = new RasterMathOpsClass();
			IConditionalOp conditionalOp = new RasterConditionalOpClass();
            IGeoDataset fillTemp = null;
            IGeoDataset flowTemp = null;
			IGeoDataset flowTemp2 = null;
            IGeoDataset flowTemp3 = null;
            IGeoDataset zeroRaster = null;
            IWorkspace resultWorkspace = null;
			try
			{
                SetAnalysisEnvironment((IRasterAnalysisEnvironment)hydroOp);
                SetAnalysisEnvironment((IRasterAnalysisEnvironment)rasterMaker);
                SetAnalysisEnvironment((IRasterAnalysisEnvironment)conditionalOp);
                SetAnalysisEnvironment((IRasterAnalysisEnvironment)logicalOp);

				object zLimit = null;
                fillTemp = hydroOp.Fill((IGeoDataset)_punchedDEM, ref zLimit);
				flowTemp = hydroOp.FlowDirection((IGeoDataset)fillTemp, false, true);

				//Set holes to flowdir of 0
				object boxedFlowTemp = flowTemp;
				zeroRaster = rasterMaker.MakeConstant(0.0, true);
				flowTemp2 = conditionalOp.Con(logicalOp.IsNull((IGeoDataset)fillTemp), zeroRaster, ref boxedFlowTemp);
                flowTemp3 = conditionalOp.SetNull(logicalOp.IsNull((IGeoDataset)_dem), flowTemp2);

                //Make output permanent
                resultWorkspace = GetResultRasterWorkspace();
                ITemporaryDataset tempFillDataset = ((IRasterAnalysisProps)fillTemp).RasterDataset as ITemporaryDataset;
                string fillPath = CreateTempFileName(_GetResultDir(), "filldem", "");
                string fillFileName = System.IO.Path.GetFileName(fillPath);
                tempFillDataset.MakePermanentAs(fillFileName, resultWorkspace, "GRID");
                _filledDEM = ((IRasterWorkspace)resultWorkspace).OpenRasterDataset(fillFileName).CreateDefaultRaster() as IRaster;

                ITemporaryDataset tempFlowDataset = ((IRasterAnalysisProps)flowTemp3).RasterDataset as ITemporaryDataset;
                string flowPath = CreateTempFileName(_GetResultDir(), "flowdir", "");
                string flowFileName = System.IO.Path.GetFileName(flowPath);
                tempFlowDataset.MakePermanentAs(flowFileName, resultWorkspace, "GRID");
                _flowDir = ((IRasterWorkspace)resultWorkspace).OpenRasterDataset(flowFileName).CreateDefaultRaster() as IRaster;
			}
			finally
			{
				UrbanDelineationExtension.ReleaseComObject(flowTemp);
                UrbanDelineationExtension.ReleaseComObject(flowTemp2);
                UrbanDelineationExtension.ReleaseComObject(flowTemp3);
                UrbanDelineationExtension.ReleaseComObject(zeroRaster);
                UrbanDelineationExtension.ReleaseComObject(conditionalOp);
                UrbanDelineationExtension.ReleaseComObject(logicalOp);
                UrbanDelineationExtension.ReleaseComObject(rasterMaker);
                UrbanDelineationExtension.ReleaseComObject(hydroOp);
                UrbanDelineationExtension.ReleaseComObject(resultWorkspace);
            }
		}

        private void _DelineateInletCatchments()
        {
            //Determine output path
            string outputDir = _GetResultDir();
            string outputPath = SetupOp.CreateTempFileName(outputDir, "Catchments", ".shp");
            string outputName = System.IO.Path.GetFileNameWithoutExtension(outputPath);

            //Calculate catchments
            IHydrologyOp hydroOp = new RasterHydrologyOpClass();
            IGeoDataset seedGrid = null;
            IGeoDataset catchmentGrid = null;
            try
            {
                SetAnalysisEnvironment((IRasterAnalysisEnvironment)hydroOp);
                seedGrid = (IGeoDataset)_FeatureClassToGrid(_drainClass, "SeedPts", SetupOp.EID_FIELD_NAME);
                catchmentGrid = hydroOp.Watershed((IGeoDataset)_flowDir, seedGrid);
                _catchmentClass = RasterToPolygon(catchmentGrid, outputName, outputDir, _smooth);

                if (((IDataset)seedGrid).CanDelete())
                    ((IDataset)seedGrid).Delete();

                _MarkForDisposal(catchmentGrid as IDataset);
            }
            finally
            {
                UrbanDelineationExtension.ReleaseComObject(seedGrid);
                UrbanDelineationExtension.ReleaseComObject(hydroOp);
            }
        }

        private IRasterDataset _FeatureClassToGrid(IFeatureClass featureClass, string baseName, string valueField)
        {
            //Convert to Grid
            IConversionOp conversionOp = new RasterConversionOpClass();
            try
            {
                SetAnalysisEnvironment((IRasterAnalysisEnvironment)conversionOp);
                string outputName = SetupOp.CreateTempFileName(this.ScratchDirectory, baseName, null);
                outputName = System.IO.Path.GetFileName(outputName);

                IFeatureClassDescriptor descriptor = new FeatureClassDescriptorClass();
                descriptor.Create(featureClass, null, valueField);

                return conversionOp.ToRasterDataset((IGeoDataset)descriptor, "GRID", GetTempRasterWorkspace(), outputName);
            }
            finally
            {
                UrbanDelineationExtension.ReleaseComObject(conversionOp);
            }
        }

        internal IFeatureClass RasterToPolygon(IGeoDataset inputRaster, string outputName, string outputDir, bool smoothShapes)
        {
            IWorkspace workspace = SetupOp.OpenShapeFileWorkspace(outputDir);
            IConversionOp conversionOp = new RasterConversionOpClass();
            try
            {
                IFeatureWorkspace featureWorkspace = workspace as IFeatureWorkspace;

                //Delete any existing
                IEnumDatasetName datasets = workspace.get_DatasetNames(esriDatasetType.esriDTFeatureClass);
                datasets.Reset();
                IDataset existing = null;
                IDatasetName datasetName = datasets.Next();
                while (datasetName != null)
                {
                    if (string.Compare(outputName, datasetName.Name, true) == 0)
                    {
                        existing = (IDataset)((IName)datasetName).Open();
                        break;
                    }
                    datasetName = datasets.Next();
                }
                if (existing != null)
                {
                    try
                    {
                        existing.Delete();
                    }
                    finally
                    {
                        UrbanDelineationExtension.ReleaseComObject(existing);
                    }
                }

                //Convert to polygon feature
                SetAnalysisEnvironment((IRasterAnalysisEnvironment)conversionOp);
                IGeoDataset polygons = conversionOp.RasterDataToPolygonFeatureData(inputRaster, workspace, outputName, smoothShapes);
                return (IFeatureClass)polygons;
            }
            finally
            {
                UrbanDelineationExtension.ReleaseComObject(workspace);
                UrbanDelineationExtension.ReleaseComObject(conversionOp);
            }
        }

		private Dictionary<IPoint, int> _ExtractUpstreamPipeEnds()
		{
			int iInletClassID = _inletClass.FeatureClassID;

			INetwork network = _geometricNetwork.Network;
			INetElements netElements = network as INetElements;
			INetTopology netTopology = network as INetTopology;

            Dictionary<IPoint, int> endPoints = new Dictionary<IPoint, int>();

			IEnumNetEID netEnum = network.CreateNetBrowser(esriElementType.esriETJunction);
			int junctionCount = netEnum.Count;
			int classId, userId, subId, edgeId;
			bool towardJunction;
			int junctionId = -1;
			for (int j = 0; j < junctionCount; j++)
			{
				junctionId = netEnum.Next();

				netElements.QueryIDs(junctionId, esriElementType.esriETJunction, out classId, out userId, out subId);

				if (classId != iInletClassID)
				{
                    bool disabled = false;
                    if(_excludeDisabled)
                        disabled = _IsDisabled(junctionId, esriElementType.esriETJunction, netElements);

                    if (!(_excludeDisabled && disabled))
                    {
                        int edgeCount = netTopology.GetAdjacentEdgeCount(junctionId);
                        bool isUpstreamEnd = edgeCount > 0; // initializing only (zero edge count always excluded)
                        for (int e = 0; e < edgeCount; e++)
                        {
                            netTopology.GetAdjacentEdge(junctionId, e, out edgeId, out towardJunction);
                            if (towardJunction)
                            {
                                isUpstreamEnd = false;
                                break;
                            }
                        }
                        if (isUpstreamEnd)
                        {
                            endPoints.Add(_geometricNetwork.get_GeometryForJunctionEID(junctionId) as IPoint, junctionId);
                        }
                    }
				}
			}

            return endPoints;
		}

        private bool _IsDisabled(int eid, esriElementType elementType, INetElements netElements)
        {
            // Check enabled field in Network Feature since it might not be in synch with network element enabled property (ESRI bug?)
            int classID, userID, subID;
            netElements.QueryIDs(eid, elementType, out classID, out userID, out subID);

            INetElementDescriptionEdit elementDescription = new NetElementDescriptionClass();
            elementDescription.ElementType_2 = elementType;
            elementDescription.UserClassID_2 = classID;
            elementDescription.UserID_2 = userID;
            elementDescription.UserSubID_2 = subID;

            INetworkFeature netFeature = this.GeometricNetwork.get_NetworkFeature(elementDescription);
            bool enabled = _IsEnabled((IFeature)netFeature);

            return !enabled;
        }

        private bool _IsEnabled(IFeature netFeature)
        {
            bool enabled = true;

            int enabledField = netFeature.Fields.FindField("Enabled");
            if (enabledField > -1)
            {
                object enabledValue = netFeature.get_Value(enabledField);
                if (enabledValue != DBNull.Value)
                {
                    enabled = Convert.ToBoolean(enabledValue);
                }
            }

            return enabled;
        }

        private List<int> _FindDownstreamPipeEnds()
        {
            List<int> endPointOidList = new List<int>();

            INetwork network = _geometricNetwork.Network;
            INetTopology netTopology = network as INetTopology;

            bool inletInNetwork = false;
            IEnumFeatureClass junctionClasses = _geometricNetwork.get_ClassesByType(esriFeatureType.esriFTSimpleJunction);
            IFeatureClass junctionClass = junctionClasses.Next();
            while (junctionClass != null)
            {
                if (junctionClass == _inletClass)
                {
                    inletInNetwork = true;
                    break;
                }
                else
                {
                    UrbanDelineationExtension.ReleaseComObject(junctionClass);
                }
                junctionClass = junctionClasses.Next();
            }

            if (!inletInNetwork)
            {
                throw new Exception("The selected inlet class is not part of the pipe network.");
            }
            
            IFeatureCursor cursor = null;
            try
            {
                int edgeId;
                bool towardJunction;

                cursor = _inletClass.Search(null, false);
                IFeature inlet = cursor.NextFeature();
                while (inlet != null)
                {
                    try
                    {
                        ISimpleJunctionFeature junction = inlet as ISimpleJunctionFeature;
                        bool isDownstream = true;
                        for (int i = 0; i < junction.EdgeFeatureCount; i++)
                        {
                            netTopology.GetAdjacentEdge(junction.EID, i, out edgeId, out towardJunction);
                            if (!towardJunction)
                            {
                                isDownstream = false;
                                break;
                            }
                        }

                        if (isDownstream)
                        {
                            endPointOidList.Add(inlet.OID);
                        }
                    }
                    finally
                    {
                        UrbanDelineationExtension.ReleaseComObject(inlet);
                    }
                    inlet = cursor.NextFeature();
                }
            }
            finally
            {
                UrbanDelineationExtension.ReleaseComObject(cursor);
            }

            return endPointOidList;
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
    }
}
