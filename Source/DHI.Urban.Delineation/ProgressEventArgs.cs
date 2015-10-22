using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DHI.Urban.Delineation
{
  public class ProgressEventArgs : EventArgs
  {
    public ProgressEventArgs(string message)
      : base()
    {
      Message = message;
    }

    public string Message { get; set; }
  }
}
