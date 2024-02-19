using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using TopazioRevitPlugin2022;



namespace TopazioRevitPluginShared
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    internal class AutoDimension : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            //Get application and document objects
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Document doc = uidoc.Document;

            try
            {
                View view = commandData.Application.ActiveUIDocument.Document.ActiveView;
                View3D view3D = view as View3D;
                if (null != view3D)
                {
                    message += "Only create dimensions in 2D";
                    return Result.Failed;
                }
                ViewSheet viewSheet = view as ViewSheet;
                if (null != viewSheet)
                {
                    message += "Only create dimensions in 2D";
                    return Autodesk.Revit.UI.Result.Failed;
                }


                // Pick a point.
                Selection sel = uiapp.ActiveUIDocument.Selection;
                XYZ firstPoint = sel.PickPoint("Seleciona o primeiro ponto.");
                XYZ secondPoint = sel.PickPoint("Seleciona o segundo ponto.");

                if (firstPoint.X - secondPoint.X != 0 & firstPoint.Y - secondPoint.Y != 0)
                {
                    //Se dist em X for maior que dist em Y
                    if (Math.Abs(firstPoint.X - secondPoint.X) > Math.Abs(firstPoint.Y - secondPoint.Y))
                    {
                        double yMedio = (firstPoint.Y + secondPoint.Y) / 2;
                        firstPoint = new XYZ(firstPoint.X, yMedio, firstPoint.Z);
                        secondPoint = new XYZ(secondPoint.X, yMedio, secondPoint.Z);

                    }
                    //Se dist em Y for maior que dist em X
                    if (Math.Abs(firstPoint.Y - secondPoint.Y) > Math.Abs(firstPoint.X - secondPoint.X))
                    {
                        double xMedio = (firstPoint.X + secondPoint.X) / 2;
                        firstPoint = new XYZ(xMedio, firstPoint.Y, firstPoint.Z);
                        secondPoint = new XYZ(xMedio, secondPoint.Y, secondPoint.Z);

                    }
                }

                //PRECISO SUBSTITUIR AS LINHAS DE CÓDIGO ABAIXO PARA CONSEGUIR SELECIONAR AS VIGAS AUTOMATICAMENTE
                //Pick beams
                BeamFilter filter = new BeamFilter();
                //Define a reference Object to accept the pick result
                IList<Reference> pickedrefs = null;

                sel = uiapp.ActiveUIDocument.Selection;
                pickedrefs = sel.PickObjects(ObjectType.Element, filter, "Select beams elements");

                //get reference
                ReferenceArray referenceArray = new ReferenceArray();
                Line Line = Line.CreateBound(firstPoint, secondPoint);

                var vigasNaoPerpendiculares = new List<string>();
                foreach (Reference elemReference in pickedrefs)
                {
                    FamilyInstance elem = doc.GetElement(elemReference) as FamilyInstance;
                    var directionLocationCurve = elem.Location as LocationCurve;
                    Curve directionCurve = directionLocationCurve.Curve;
                    Line directionLine = directionCurve as Line;
                    if (Utils.AreLinesPerpendicular(directionLine, Line))
                    {
                        referenceArray.Append(elem.GetReferenceByName("Front"));
                        referenceArray.Append(elem.GetReferenceByName("Back"));
                    }
                    else 
                    {
                        vigasNaoPerpendiculares.Add(elem.Id.ToString());
                    };
                    
                }
                if (vigasNaoPerpendiculares.Count > 0)
                {
                    string errorMessage = "As vigas: ";
                    foreach (var viga in vigasNaoPerpendiculares)
                    {
                        errorMessage = errorMessage + viga + " ";
                    }
                    errorMessage = errorMessage + "não estão perpendiculares, e não foram cotadas.";
                    TaskDialog.Show("Topazio Erro", errorMessage);
                }
                Transaction trans = new Transaction(doc);
                trans.Start("Automatic Dimension");
                Dimension newDimension = doc.Create.NewDimension(doc.ActiveView, Line, referenceArray);
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
