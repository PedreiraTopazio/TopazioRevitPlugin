using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System;
using System.Collections.Generic;
using System.Text;

namespace TopazioRevitPluginShared
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    internal class TestButton : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            //Get application and document objects
            UIApplication uiapp = commandData.Application;
            Document doc = uiapp.ActiveUIDocument.Document;


            try
            {
                //Define a reference Object to accept the pick result
                Reference pickedref = null;

                //Pick a element
                Selection sel = uiapp.ActiveUIDocument.Selection;
                AllFilter filter = new AllFilter();
                pickedref = sel.PickObject(ObjectType.Element, filter, "Select source element");
                Element elem = doc.GetElement(pickedref);

                TaskDialog.Show("Debug", elem.GroupId.ToString());

            }catch(Exception ex) { }
            return Result.Succeeded;
        }
    }

}
