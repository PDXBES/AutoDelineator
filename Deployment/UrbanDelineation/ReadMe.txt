Install ArcGIS 10.1 with SP1 first.

Then, save the files in this directory anywhere on your C:\ drive.

Double-click on "InstallUrbanDelineation.bat".

To uninstall, double-click on UninstallUrbanDelineation.bat.

Questions should be directed to Arnold Engelmann (ahe@dhigroup.com).

CHANGE LOG:

10/26/2007
- Initial release.

11/6/2007
- Area (in map units) is now calculated and included in catchment attributes.
- The feature class of the selected outfall points is now included in the catchment attributes along with the ObjectID of the outfall point.
- Pipe outlets with more than one pipe leading to them are now recognized as outlets. (Previously only outlets with a single pipe were used.)
- Layers with missing link to dataset no longer cause error.
- Uninstall batch file included.

3/25/2010
- Recompiled for ArcMap 9.3/MIKE 2009

6/18/2012
- Simplified deployment by removing references to proprietary DHI dlls.
- Added option to exclude downstream pipe ends from inlets.

6/20/2012
- Added flow tracing tool to diagnose issues with DEM.

6/21/2012
- Better filtering of layers shown in setup pull-down menus.
- Bug fix: Pull-down menus in setup no longer clear selected items.
- Bug fix: No more crash with multi-level group layers.

6/28/2012
- No longer hangs in some cases where there is more than one junction feature class.
- Upstream nodes with more than one pipe exiting are now included as "upstream pipe ends"

7/11/2012
- Option to smooth catchment boundaries
- Individual inlet watersheds are added to the map

7/19/2012
- Reorganized to precalculate inlet catchments. Improves delineation speed after preprocessing.
- Catchments are now topologically correct across deliniations.
- Settings are saved to MXD.
- Local disk is used as scratch space, reducing network load.

7/29/2012
- Added option to ignore disabled network features when preprocessing and delineating.

7/30/2012
- Added option to specify custom ID field for output.
- Bug fix: Fixed problem where value in "Enabled" field was not always read correctly.

8/3/2012
- Delineation dialog remembers selected label field.
- Minor dialog layout changes.
- Output layer now has labels displayed by default.

9/10/2012
- Settings are now output to an xml file, named after the output watersheds feature class.
- Code is now open source (GPL v.3)

10/9/2012
- Adding delineation from surface features

10/23/2012
- Added option to snap surface points to streams (pour points)
- Updated icons

10/25/2012
- Added "Zingers" tool
- Performance improvement to flow tracing tool
- Flow tracing tool output now in blue

11/1/2012
- Updated documentation
- Reverted flow tracing tool to red

12/6/2012
- Updated to ArcGIS 10.1

12/7/2012
- Fixed flow tracing tool (no longer over-generalizes flow path)

2/21/2013
- Fixed error when using source features that are in different projection from flow direction data

2/27/2013
- Always output smooth and detailed catchments

4/11/2013
- Clean up temporary files after using flow tracing tool
- Allow from and to layer to be the same for zingers

5/7/2013
- Fixed bug with catchment creation
- Added license file to deployment
- Improved install scripts

12/17/2013
- Allows for infiltration inlets (inlets not connected to pipes)
- Better cleanup of geoprocessing tasks
- Added info about "blocked" downloads to manual

1/2/2014
- Updated to ArcGIS 10.2
- Allow disconnected inlets (e.g. infiltration inlets)
- Fixed handling of disabled inlet nodes
