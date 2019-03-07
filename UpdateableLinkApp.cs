using System;
using System.Collections.Generic;
using System.IO;
using Autodesk.Revit.UI;
using RevitCommon.Attributes;

namespace Elk.UpdateableLinks
{
    [ExtApp(Name = "Information Links", Description = "Links to HKS Standards and Useful Information",
        Guid = "4595c82b-efee-405a-bb13-3cd8852bbcf8", Vendor = "HKSL", VendorDescription = "HKS LINE",
        ForceEnabled = true, Commands = new[] { "Auto-Populated" })]
    public class LinkApp : IExternalApplication
    {
        public Result OnShutdown(UIControlledApplication application)
        {
            return Result.Succeeded;
        }

        public Result OnStartup(UIControlledApplication application)
        {
            try
            {
                string appPath = Path.GetDirectoryName(typeof(LinkApp).Assembly.Location);
                string buttonLibPath = Path.Combine(appPath, "Elk.Buttons.dll");
                string configPath = Path.Combine(appPath, "Elk.Buttons.config");

                // If the config path is valid, regenerate the buttons
                if (File.Exists(configPath))
                {
                    try
                    {
                        LinkCommon.GetPanels(configPath, out List<PanelInfo> panels, out List<CommandInfo> commands);
                        if (null != panels && panels.Count > 0)
                        {
                            LinkCommon.CompileButtons(panels);
                            // Try to replace the buttons file...
                            if (File.Exists(buttonLibPath) && File.Exists(Path.Combine(appPath, "Elk.Buttons_new.dll")))
                            {
                                File.Delete(buttonLibPath);
                                File.Move(Path.Combine(appPath, "Elk.Buttons_new.dll"), buttonLibPath);
                            }

                        }
                    }
                    catch (Exception ex)
                    {
                        TaskDialog dlg = new TaskDialog("Error");
                        dlg.TitleAutoPrefix = false;
                        dlg.MainInstruction = "Error Compiling New Link Buttons";
                        dlg.MainContent = ex.Message;
                    }
                }

                if (File.Exists(buttonLibPath))
                {
                    List<PanelInfo> panels = new List<PanelInfo>();
                    LinkCommon.GetPanels(buttonLibPath, out panels);

                    //intantiate the buttons
                    LinkCommon.InstantiateButtons(application, panels, buttonLibPath, appPath);
                }
                return Result.Succeeded;
            }
            catch (Exception ex)
            {
                TaskDialog.Show("Error", ex.Message);
                return Result.Failed;
            }
        }
    }

   
}
