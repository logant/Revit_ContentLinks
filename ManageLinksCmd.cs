using System;
using System.Collections.Generic;
using System.IO;
using Autodesk.Revit.UI;
using Autodesk.Revit.DB;
using Autodesk.Revit.Attributes;

using RevitCommon.Attributes;

namespace Elk.UpdateableLinks
{
    [Transaction(TransactionMode.Manual)]
    public class ManageLinksCmd : IExternalCommand
    {
        static string appPath;
        string directory;

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            try
            {
                appPath = new FileInfo(typeof(ManageLinkApp).Assembly.Location).DirectoryName;
                directory = appPath;
                string buttonLibPath = directory + "\\Elk.Buttons.dll";

                List<PanelInfo> panels = new List<PanelInfo>();

                if (File.Exists(buttonLibPath))
                    LinkCommon.GetPanels(buttonLibPath, out panels);

                // Get the version
                int version = Convert.ToInt32(commandData.Application.Application.VersionNumber);

                // Get the Revit window handle
                IntPtr handle = IntPtr.Zero;
                if (version < 2019)
                    handle = System.Diagnostics.Process.GetCurrentProcess().MainWindowHandle;
                else
                    handle = commandData.Application.GetType().GetProperty("MainWindowHandle") != null
                        ? (IntPtr)commandData.Application.GetType().GetProperty("MainWindowHandle").GetValue(commandData.Application)
                        : IntPtr.Zero;
                ManageLinksForm form = new ManageLinksForm(buttonLibPath, panels);
                System.Windows.Interop.WindowInteropHelper helper = new System.Windows.Interop.WindowInteropHelper(form);
                helper.Owner = handle;
                form.Show();
                return Result.Succeeded;
            }
            catch (Exception ex)
            {
                message = ex.Message;
                return Result.Failed;
            }
        }
    }
}
