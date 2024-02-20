using Autodesk.Revit.DB;
using Autodesk.Revit.UI.Selection;
using System.Collections.Generic;

namespace TopazioRevitPluginShared
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

    public class ReferenceFilter : ISelectionFilter
    {
        public bool AllowElement(Element e)
        {
            return false;
        }
        public bool AllowReference(Reference r, XYZ p)
        {
            return true;
        }
    }

    public class BeamFilter : ISelectionFilter
    {
        public bool AllowElement(Element e)
        {
            List<int> categorias = new List<int>()
            {   //ADICIONE AQUI TODAS AS CATEGORIAS PERMITIDAS NO FILTRO
                (int)BuiltInCategory.OST_StructuralFraming
            };
            foreach (int cat in categorias)
            {
                if (e.Category.Id.IntegerValue.Equals(cat))
                {
                    return true;
                }
            }
            return false;
        }
        public bool AllowReference(Reference r, XYZ p)
        {
            return false;
        }
    }
}