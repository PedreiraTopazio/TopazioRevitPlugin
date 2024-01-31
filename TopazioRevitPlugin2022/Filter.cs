using Autodesk.Revit.DB;
using Autodesk.Revit.UI.Selection;
using System.Collections.Generic;

namespace TopazioRevitPlugin2022
{
    public class AllFilter : ISelectionFilter
    {
        public bool AllowElement(Element e)
        {
            return true;
        }
        public bool AllowReference(Reference r, XYZ p)
        {
            return false;
        }
    }
}