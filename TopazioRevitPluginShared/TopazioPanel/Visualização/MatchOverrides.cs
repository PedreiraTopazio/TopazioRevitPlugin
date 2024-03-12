using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System;
using System.Collections.Generic;

namespace TopazioRevitPluginShared
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    internal class MatchOverrides : IExternalCommand
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

                OverrideGraphicSettings currentOverride = doc.ActiveView.GetElementOverrides(elem.Id);


                //Pick a element
                sel = uiapp.ActiveUIDocument.Selection;
                //Define a reference Object to accept the pick result
                IList<Reference> pickedrefs = null;
                pickedrefs = sel.PickObjects(ObjectType.Element, filter, "Select destination elements");

                Transaction trans = new Transaction(doc);
                trans.Start("Override Graphics");
                foreach(Reference elem_reference in pickedrefs)
                {
                    elem = doc.GetElement(elem_reference);
                    doc.ActiveView.SetElementOverrides(elem.Id, currentOverride);
                }
                trans.Commit();

            }
            //If the user right-clicks or presses Esc, handle the exception
            catch (Autodesk.Revit.Exceptions.OperationCanceledException)
            {
                return Result.Cancelled;
            } catch (Exception ex)
            {
                message = ex.Message;
                return Result.Failed;
            }

            return Result.Succeeded;
        }
    }
}
