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
  }
}
