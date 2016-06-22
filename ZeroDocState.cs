using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace Elk.UpdateableLinks
{
    public class ZeroDocAvailability : IExternalCommandAvailability
    {
        public bool IsCommandAvailable(UIApplication applicationData, CategorySet selectedCategories)
        {
            return true;
        }
    }
}
