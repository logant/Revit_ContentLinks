using System;
using System.IO;
using System.Windows.Media.Imaging;
using Autodesk.Revit.UI;
using RevitCommon.Attributes;

namespace Elk.UpdateableLinks
{
    [ExtApp(Name = "Manage Info Links", Description = "Manage Links to Useful Information",
          Guid = "b82cbf5e-5cd8-4389-8991-bb39cc7eabf1", Vendor = "HKSL", VendorDescription = "HKS LINE, www.hksline.com",
          ForceEnabled = false, Commands = new[] { "Manage Links" })]
    public class ManageLinkApp : IExternalApplication
    {
        static string appPath;

        public string AppPath
        {
            get { return appPath; }
        }

        public Result OnShutdown(UIControlledApplication application)
        {
            return Result.Succeeded;
        }

        public Result OnStartup(UIControlledApplication application)
        {
            appPath = new FileInfo(typeof(ManageLinkApp).Assembly.Location).DirectoryName;
            try
            {
                if (File.Exists(appPath + "\\Elk.Buttons_New.dll"))
                {
                    File.Delete(appPath + "\\Elk.Buttons.dll");
                    File.Move(appPath + "\\Elk.Buttons_New.dll", appPath + "\\Elk.Buttons.dll");
                }

                // Add manage form button
                PushButtonData managePBD = new PushButtonData("Manage Link Buttons", "Manage\nLinks", typeof(ManageLinkApp).Assembly.Location, typeof(ManageLinksCmd).FullName)
                {
                    LargeImage = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(Properties.Resources.manageButton.GetHbitmap(), IntPtr.Zero, System.Windows.Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions()),
                    ToolTip = "Manage the Updateable Links",
                    AvailabilityClassName = typeof(ZeroDocAvailability).FullName
                };

                RevitCommon.UI.AddToRibbon(application, "Add-Ins", "Links", managePBD);


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
