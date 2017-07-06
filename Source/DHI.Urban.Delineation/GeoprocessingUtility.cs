using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ESRI.ArcGIS.DataSourcesGDB;
using ESRI.ArcGIS.DataSourcesRaster;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geoprocessing;
using ESRI.ArcGIS.Geoprocessor;

namespace DHI.Urban.Delineation
{
  /// <summary>
  /// A class for handling geoprocessor tasks
  /// </summary>
  /// <remarks>Based on GeoprocessingUtility in Bes.Swsp.ToolBox. See that class for asynchronous option (should be implemented in UI.ArcMap project).
  /// This would allow for users to continue working while process runs, and allows user to cancel process.</remarks>
  public class GeoprocessingUtility
  {
    private static IGPUtilities3 esriUtility = new GPUtilitiesClass();

    private static bool? userTempLayers;
    private static bool? userAddOutputs;
    private static bool? userOverwrite;
    private static object scratchWorkspace;
    private static object workspace;

    /// <summary>
    /// Gets the scratch workspace being used by the ArcGIS Geoprocessor.
    /// </summary>
    public static string ScratchWorkspace
    {
      get
      {
        Geoprocessor gp = new Geoprocessor();
        return Convert.ToString(gp.GetEnvironmentValue("scratchWorkspace"));
      }
    }

    /// <summary>
    /// Gets the ArcGIS Geoprocessor, configured with the given parameters.
    /// </summary>
    /// <param name="temporaryMapLayers">Whether new map layers are temporary by default.</param>
    /// <param name="addOutputsToMap">Whether resulting output datasets should be added to the application display.</param>
    /// <param name="overwriteOutput">Whether existing output datasets should be overwritten.</param>
    /// <param name="gridReference">A raster to be used as a template for output grids in regard to dimensions (width, height, cell size, and coordinates)</param>
    /// <returns>The ArcGIS Geoprocessor</returns>
    public static Geoprocessor GetGeoprocessor(bool temporaryMapLayers = true, bool addOutputsToMap = false, bool overwriteOutput = true, IRaster gridReference = null)
    {
      Geoprocessor gp = new Geoprocessor();
      if (!userTempLayers.HasValue)
      {
        userTempLayers = gp.TemporaryMapLayers;
      }

      if (!userAddOutputs.HasValue)
      {
        userAddOutputs = gp.AddOutputsToMap;
      }

      if (!userOverwrite.HasValue)
      {
        userOverwrite = gp.OverwriteOutput;
      }

      gp.TemporaryMapLayers = temporaryMapLayers;
      gp.AddOutputsToMap = addOutputsToMap;
      gp.OverwriteOutput = overwriteOutput;

      if (gridReference != null)
      {
        IRasterDataset rds = ((IRasterAnalysisProps)gridReference).RasterDataset;
        IDataset ds = (IDataset)rds;
        string path = System.IO.Path.Combine(ds.Workspace.PathName, ds.Name);
        gp.SetEnvironmentValue("snapRaster", path);

        string extent = string.Format("{0} {1} {2} {3}", ((IGeoDataset2)rds).Extent.XMin, ((IGeoDataset2)rds).Extent.YMin, ((IGeoDataset2)rds).Extent.XMax, ((IGeoDataset2)rds).Extent.YMax);
        gp.SetEnvironmentValue("extent", extent);
      }

      if (workspace == null)
      {
        workspace = gp.GetEnvironmentValue("workspace");
      }

      if (scratchWorkspace == null)
      {
        scratchWorkspace = gp.GetEnvironmentValue("scratchWorkspace");
      }

      if (gp.GetEnvironmentValue("workspace") == null)
      {
        gp.SetEnvironmentValue("workspace", GetDefaultWorkspace());
      }

      if (gp.GetEnvironmentValue("scratchWorkspace") == null)
      {
        gp.SetEnvironmentValue("scratchWorkspace", GetDefaultWorkspace());
      }

      return gp;
    }

    /// <summary>
    /// Resets the ArcGIS Geoprocessor to the user defined environment variables.
    /// </summary>
    public static void ResetGeoprocessor()
    {
      Geoprocessor gp = new Geoprocessor();
      if (userTempLayers.HasValue)
      {
        gp.TemporaryMapLayers = userTempLayers.Value;
      }

      if (userAddOutputs.HasValue)
      {
        gp.AddOutputsToMap = userAddOutputs.Value;
      }

      if (userOverwrite.HasValue)
      {
        gp.OverwriteOutput = userOverwrite.Value;
      }

      if (workspace != null)
      {
        gp.SetEnvironmentValue("workspace", workspace);
      }

      if (scratchWorkspace != null)
      {
        gp.SetEnvironmentValue("scratchWorkspace", scratchWorkspace);
      }

      userTempLayers = null;
      userAddOutputs = null;
      userOverwrite = null;
      workspace = null;
      scratchWorkspace = null;
    }

    /// <summary>
    /// Runs a Geoprocessing tool.
    /// </summary>
    /// <param name="geoprocessor">The ArcGIS Geoprocessor defining the environment the Geoprocessing Tool will run in.</param>
    /// <param name="process">The Geoprocessing tool to run.</param>
    /// <returns>The result object of the Geoprocessing tool.</returns>
    public static object RunGpTool(Geoprocessor geoprocessor, IGPProcess process)
    {
      object result = null;
      try
      {
        bool tryAgain = false;
        int retryCount = 0;
        int maxRetries = 1;

        do
        {
          retryCount++;
          try
          {
            geoprocessor.ProgressChanged += Geoprocessor_ProgressChanged;
            geoprocessor.MessagesCreated += Geoprocessor_MessagesCreated;

            IGeoProcessorResult2 geoprocessorResult = geoprocessor.Execute(process, null) as IGeoProcessorResult2;

            result = geoprocessorResult.ReturnValue;

            tryAgain = false;
          }
          catch (Exception ex)
          {
            if (retryCount <= maxRetries)
            {
              tryAgain = true;
            }
            else
            {
              throw new Exception("Geoprocessing tool " + process.ToolName + " failed after " + retryCount + " attempts.", ex);
            }
          }
        }
        while (tryAgain);

        return result;
      }
      catch (Exception ex)
      {
        throw;
      }
    }

    /// <summary>
    /// Converts a feature class into the fully qualified path used by Geoprocessing tools.
    /// </summary>
    /// <param name="featureClass">The feature class to get the path for.</param>
    /// <returns>The fully qualified path to the feature class.</returns>
    public static string GetFeatureClassPath(IFeatureClass featureClass)
    {
      if (featureClass != null)
      {
        string path = GetWorkspacePath(((IDataset)featureClass).Workspace);

        if (featureClass.FeatureDataset != null)
        {
          path += "\\" + featureClass.FeatureDataset.Name;
        }

        path += "\\" + ((IDataset)featureClass).Name;

        var delimitedPath = "'" + path + "'";
        return delimitedPath;
      }
      else
      {
        return string.Empty;
      }
    }

    /// <summary>
    /// Converts the workspace into the fully qualified path used by Geoprocessing tools
    /// </summary>
    /// <param name="workspace">The workspace for which to get the path.</param>
    /// <returns>The path to the given workspace.</returns>
    public static string GetWorkspacePath(IWorkspace workspace)
    {
      if (workspace != null)
      {
        string path = string.Empty;
        if (workspace == esriUtility.GetInMemoryWorkspace())
        {
          path = "in_memory";
        }
        else
        {
          path = workspace.PathName;
        }

        return path;
      }
      else
      {
        return string.Empty;
      }
    }

    /// <summary>
    /// Converts the raster into the fully qualified path used by Geoprocessing tools.
    /// </summary>
    /// <param name="raster">The raster get the path for.</param>
    /// <returns>The fully qualified path to the raster.</returns>
    public static string GetRasterPath(IRaster raster)
    {
      if (raster == null)
      {
        return string.Empty;
      }

      IRasterDataset rasterDataset = ((IRasterAnalysisProps)raster).RasterDataset;
      if (rasterDataset != null)
      {
        string path = ((IDataset)rasterDataset).Workspace.PathName;
        path += "\\" + ((IDataset)rasterDataset).Name;
        return path;
      }
      else
      {
        return string.Empty;
      }
    }

    /// <summary>
    /// Opens and returns the feature class from the given path. The path is what is used in the geoprocessing environment (file path or other reference, such as "in_memory" paths).
    /// </summary>
    /// <param name="path">The path to the feature class.</param>
    /// <returns>The feature class described by the input path.</returns>
    public static IFeatureClass GetFeatureClassFromPath(string path)
    {
      return esriUtility.OpenFeatureClassFromString(path);
    }

    /// <summary>
    /// Opens and returns the raster from the given path. The path is what is used in the geoprocessing environment (file path or other reference, such as "in_memory" paths).
    /// </summary>
    /// <param name="path">The path to the raster dataset.</param>
    /// <returns>The raster described by the input path.</returns>
    public static IRaster GetRasterFromPath(string path)
    {
      IRasterDataset dataset = esriUtility.OpenRasterDatasetFromString(path);
      return dataset.CreateDefaultRaster();
    }

    private static string GetDefaultWorkspace()
    {
      string documents = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
      string tempDir = System.IO.Path.Combine(documents, "temp");
      string workspacePath = System.IO.Path.Combine(tempDir, "Scratch.gdb");

      IWorkspaceFactory workspaceFactory = new FileGDBWorkspaceFactory();
      if (!workspaceFactory.IsWorkspace(workspacePath))
      {
        IWorkspaceName defaultWorkspaceName = workspaceFactory.Create(tempDir, "Scratch", null, 0);
        return defaultWorkspaceName.PathName;
      }
      else
      {
        return workspacePath;
      }
    }

    private static void Geoprocessor_MessagesCreated(object sender, MessagesCreatedEventArgs e)
    {
      for (int i = 0; i < e.GPMessages.Count; i++)
      {
        var message = e.GPMessages.GetMessage(i);
      }

      e.GPMessages.Clear();
    }

    private static void Geoprocessor_ProgressChanged(object sender, ProgressChangedEventArgs e)
    {
    }
  }
}
