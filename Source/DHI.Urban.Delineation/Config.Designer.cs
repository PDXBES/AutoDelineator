//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace DHI.Urban.Delineation {
    using ESRI.ArcGIS.Framework;
    using ESRI.ArcGIS.ArcMapUI;
    using ESRI.ArcGIS.Editor;
    using ESRI.ArcGIS.esriSystem;
    using System;
    using System.Collections.Generic;
    using ESRI.ArcGIS.Desktop.AddIns;
    
    
    /// <summary>
    /// A class for looking up declarative information in the associated configuration xml file (.esriaddinx).
    /// </summary>
    internal static class ThisAddIn {
        
        internal static string Name {
            get {
                return "DHI.Urban.Delineation";
            }
        }
        
        internal static string AddInID {
            get {
                return "{159adb30-7e36-4e4b-abdf-608c73e7f903}";
            }
        }
        
        internal static string Company {
            get {
                return "DHI";
            }
        }
        
        internal static string Version {
            get {
                return "5.1";
            }
        }
        
        internal static string Description {
            get {
                return "Urban catchment delineation using surface and sub-surface conveyance to trace wat" +
                    "er flow.";
            }
        }
        
        internal static string Author {
            get {
                return "Arnold Engelmann";
            }
        }
        
        internal static string Date {
            get {
                return "3/7/2018";
            }
        }
        
        internal static ESRI.ArcGIS.esriSystem.UID ToUID(this System.String id) {
            ESRI.ArcGIS.esriSystem.UID uid = new ESRI.ArcGIS.esriSystem.UIDClass();
            uid.Value = id;
            return uid;
        }
        
        /// <summary>
        /// A class for looking up Add-in id strings declared in the associated configuration xml file (.esriaddinx).
        /// </summary>
        internal class IDs {
            
            /// <summary>
            /// Returns 'DHI_DHI.Urban.Delineation_SetupButton', the id declared for Add-in Button class 'SetupButton'
            /// </summary>
            internal static string SetupButton {
                get {
                    return "DHI_DHI.Urban.Delineation_SetupButton";
                }
            }
            
            /// <summary>
            /// Returns 'DHI_DHI.Urban.Delineation_UrbanDelineationButton', the id declared for Add-in Button class 'UrbanDelineationButton'
            /// </summary>
            internal static string UrbanDelineationButton {
                get {
                    return "DHI_DHI.Urban.Delineation_UrbanDelineationButton";
                }
            }
            
            /// <summary>
            /// Returns 'DHI_DHI.Urban.Delineation_FlowTracingTool', the id declared for Add-in Tool class 'FlowTracingTool'
            /// </summary>
            internal static string FlowTracingTool {
                get {
                    return "DHI_DHI.Urban.Delineation_FlowTracingTool";
                }
            }
            
            /// <summary>
            /// Returns 'DHI_DHI.Urban.Delineation_ZingerButton', the id declared for Add-in Button class 'ZingerButton'
            /// </summary>
            internal static string ZingerButton {
                get {
                    return "DHI_DHI.Urban.Delineation_ZingerButton";
                }
            }
            
            /// <summary>
            /// Returns 'DHI_DHI.Urban.Delineation_UrbanDelineationExtension', the id declared for Add-in Extension class 'UrbanDelineationExtension'
            /// </summary>
            internal static string UrbanDelineationExtension {
                get {
                    return "DHI_DHI.Urban.Delineation_UrbanDelineationExtension";
                }
            }
        }
    }
    
internal static class ArcMap
{
  private static IApplication s_app = null;
  private static IDocumentEvents_Event s_docEvent;

  public static IApplication Application
  {
    get
    {
      if (s_app == null)
      {
        s_app = Internal.AddInStartupObject.GetHook<IMxApplication>() as IApplication;
        if (s_app == null)
        {
          IEditor editorHost = Internal.AddInStartupObject.GetHook<IEditor>();
          if (editorHost != null)
            s_app = editorHost.Parent;
        }
      }
      return s_app;
    }
  }

  public static IMxDocument Document
  {
    get
    {
      if (Application != null)
        return Application.Document as IMxDocument;

      return null;
    }
  }
  public static IMxApplication ThisApplication
  {
    get { return Application as IMxApplication; }
  }
  public static IDockableWindowManager DockableWindowManager
  {
    get { return Application as IDockableWindowManager; }
  }
  public static IDocumentEvents_Event Events
  {
    get
    {
      s_docEvent = Document as IDocumentEvents_Event;
      return s_docEvent;
    }
  }
  public static IEditor Editor
  {
    get
    {
      UID editorUID = new UID();
      editorUID.Value = "esriEditor.Editor";
      return Application.FindExtensionByCLSID(editorUID) as IEditor;
    }
  }
}

namespace Internal
{
  [StartupObjectAttribute()]
  [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
  [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
  public sealed partial class AddInStartupObject : AddInEntryPoint
  {
    private static AddInStartupObject _sAddInHostManager;
    private List<object> m_addinHooks = null;

    [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Never)]
    public AddInStartupObject()
    {
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Never)]
    protected override bool Initialize(object hook)
    {
      bool createSingleton = _sAddInHostManager == null;
      if (createSingleton)
      {
        _sAddInHostManager = this;
        m_addinHooks = new List<object>();
        m_addinHooks.Add(hook);
      }
      else if (!_sAddInHostManager.m_addinHooks.Contains(hook))
        _sAddInHostManager.m_addinHooks.Add(hook);

      return createSingleton;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Never)]
    protected override void Shutdown()
    {
      _sAddInHostManager = null;
      m_addinHooks = null;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Never)]
    internal static T GetHook<T>() where T : class
    {
      if (_sAddInHostManager != null)
      {
        foreach (object o in _sAddInHostManager.m_addinHooks)
        {
          if (o is T)
            return o as T;
        }
      }

      return null;
    }

    // Expose this instance of Add-in class externally
    public static AddInStartupObject GetThis()
    {
      return _sAddInHostManager;
    }
  }
}
}
