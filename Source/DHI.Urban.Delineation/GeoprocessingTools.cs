using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ESRI.ArcGIS.Geodatabase;

namespace DHI.Urban.Delineation
{
  /// <summary>
  /// A high level static class for executing specific geoprocessing tools
  /// </summary>
  public static class GeoprocessingTools
  {
    public static IRaster SetNull(IRaster condition, IRaster falseRaster, string outputPath)
    {
      var geoprocessor = GeoprocessingUtility.GetGeoprocessor();

      var setNullTool = new ESRI.ArcGIS.SpatialAnalystTools.SetNull();
      setNullTool.in_conditional_raster = GeoprocessingUtility.GetGPRasterObject(condition);
      setNullTool.in_false_raster_or_constant = GeoprocessingUtility.GetGPRasterObject(falseRaster);
      setNullTool.out_raster = outputPath;

      string resultPath = GeoprocessingUtility.RunGpTool(geoprocessor, setNullTool) as string;
      IRaster outputRaster = GeoprocessingUtility.GetRasterFromPath(resultPath);

      GeoprocessingUtility.ResetGeoprocessor();

      return outputRaster;
    }

    public static IRaster Con(IRaster condition, IRaster trueRaster, IRaster falseRaster, string outputPath)
    {
      var geoprocessor = GeoprocessingUtility.GetGeoprocessor(true, false, true, condition);

      var conTool = new ESRI.ArcGIS.SpatialAnalystTools.Con();
      conTool.in_conditional_raster = GeoprocessingUtility.GetGPRasterObject(condition);
      conTool.in_true_raster_or_constant = GeoprocessingUtility.GetGPRasterObject(trueRaster);
      conTool.in_false_raster_or_constant = GeoprocessingUtility.GetGPRasterObject(falseRaster);
      conTool.out_raster = outputPath;

      string resultPath = GeoprocessingUtility.RunGpTool(geoprocessor, conTool) as string;
      IRaster outputRaster = GeoprocessingUtility.GetRasterFromPath(resultPath);

      GeoprocessingUtility.ResetGeoprocessor();

      return outputRaster;
    }

    public static IRaster Fill(IRaster surface, string outputPath)
    {
      var geoprocessor = GeoprocessingUtility.GetGeoprocessor(true, false, true, surface);

      var fillTool = new ESRI.ArcGIS.SpatialAnalystTools.Fill();
      fillTool.in_surface_raster = GeoprocessingUtility.GetGPRasterObject(surface);
      fillTool.out_surface_raster = outputPath;

      string resultPath = GeoprocessingUtility.RunGpTool(geoprocessor, fillTool) as string;
      IRaster outputRaster = GeoprocessingUtility.GetRasterFromPath(resultPath);

      GeoprocessingUtility.ResetGeoprocessor();

      return outputRaster;
    }

    public static IRaster FlowDirection(IRaster surface, string outputPath, bool forceFlowToEdge = false)
    {
      var geoprocessor = GeoprocessingUtility.GetGeoprocessor(true, false, true, surface);

      var flowDirTool = new ESRI.ArcGIS.SpatialAnalystTools.FlowDirection();
      flowDirTool.in_surface_raster = GeoprocessingUtility.GetGPRasterObject(surface);
      flowDirTool.force_flow = forceFlowToEdge ? "FORCE" : "NORMAL";
      flowDirTool.out_flow_direction_raster = outputPath;

      string resultPath = GeoprocessingUtility.RunGpTool(geoprocessor, flowDirTool) as string;
      IRaster outputRaster = GeoprocessingUtility.GetRasterFromPath(resultPath);

      GeoprocessingUtility.ResetGeoprocessor();

      return outputRaster;
    }
  }
}
