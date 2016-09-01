using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Windows.Forms;

namespace DHI.Urban.Delineation
{
  public class UrbanDelineationButton : ESRI.ArcGIS.Desktop.AddIns.Button
  {
    private ApplicationWindow _appWindow = null;
    
    public UrbanDelineationButton()
    {
      _appWindow = new ApplicationWindow(ArcMap.Application.hWnd);
    }

    protected override void OnClick()
    {
      try
      {
        base.OnClick();

        UrbanDelineationForm pForm = new UrbanDelineationForm();
        pForm.Show(_appWindow);
      }
      catch (Exception ex)
      {
        MessageBox.Show(_appWindow, ex.Message, Properties.Resources.Caption_Error, MessageBoxButtons.OK, MessageBoxIcon.Error);
      }
    }

    protected override void OnUpdate()
    {
      Enabled = ArcMap.Application != null;
    }
  }
}
