using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;

namespace TopazioRevitPluginShared
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    internal class StructuralColumnHatches : IExternalCommand
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
                    message += "Criar hachuras somente em vista 2D";
                    return Result.Failed;
                }
                ViewSheet viewSheet = view as ViewSheet;
                if (null != viewSheet)
                {
                    message += "Criar hachuras somente em vista 2D";
                    return Autodesk.Revit.UI.Result.Failed;
                }

                //Pega o nivel da vista
                TaskDialog.Show("Revit", "Cheguei aqui");
                var nivelId = Utils.GetLevelOfView(doc, view);
                if (nivelId != null)
                {
                    TaskDialog.Show("Revit", nivelId.ToString());
                }
                else
                {
                    TaskDialog.Show("Revit", "Retornou null.");
                }
                

                //Verificar se existe eixos nesse projeto
                var grids = new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_Grids).WhereElementIsNotElementType().ToElements();
                if (grids.Count == 0)
                {
                    message += "Não há eixos criados no modelo";
                    return Result.Failed;
                }

                //Seleciona todos os pilares visiveis na vista
                var pilaresVisiveis = new FilteredElementCollector(doc, view.Id).OfCategory(BuiltInCategory.OST_StructuralColumns).WhereElementIsNotElementType().ToElements();
                var pilaresDict = new List<Dictionary<string, dynamic>>();

                Transaction trans = new Transaction(doc);
                trans.Start("Pilar NMC");

                //Classificar os pilares em terminam e começam
                foreach (var pilar in pilaresVisiveis)
                {
                    var location = pilar.Location;
                    var point = location as LocationPoint;

                    var pilarId = pilar.Id;
                    var pilarTipo = pilar.GetTypeId();
                    var pilarXY = pilar.get_Parameter(BuiltInParameter.COLUMN_LOCATION_MARK).AsString();
                    var rotacao = point.Rotation;
                    string NM = "";

                    if (pilar.get_Parameter(BuiltInParameter.FAMILY_TOP_LEVEL_PARAM).AsElementId() == nivelId)
                    {
                        NM = "Morre";
                    }
                    if (pilar.get_Parameter(BuiltInParameter.FAMILY_BASE_LEVEL_PARAM).AsElementId() == nivelId)
                    {
                        NM = "Nasce";
                    }


                    foreach (Dictionary<string, dynamic> dict in pilaresDict)
                    {
                        if (pilarId != dict["Id"] &&
                            pilarTipo == dict["Tipo"] &&
                            pilarXY == dict["XY"] &&
                            rotacao == dict["rotacao"])
                        {
                            dict["NM"] = "Continua";
                            NM = "Continua";
                        }
                    }
                    //TaskDialog.Show("TESTE", NM);
                    Dictionary<string, dynamic> aux = new Dictionary<string, dynamic>
                    {
                        { "Id", pilar.Id },
                        { "Tipo", pilar.GetTypeId() },
                        { "XY", pilar.get_Parameter(BuiltInParameter.COLUMN_LOCATION_MARK).AsString() },
                        { "rotacao", rotacao },
                        { "NM", NM }
                    };
                    pilaresDict.Add(aux);
                }

                //TESTE PARA VER SE ESTÁ FUNCIONANDO
                //foreach (Dictionary<string, dynamic> dict in pilaresDict)
                //{
                //    doc.GetElement(dict["Id"]).get_Parameter(BuiltInParameter.ALL_MODEL_INSTANCE_COMMENTS).Set(dict["NM"]);
                //}
                //trans.Commit();

                //Pilares que continua -> Não faz nada

                //Pilares que morrem -> Sobrepõe grafico com hatch de morre

                //Pilares que nasce -> Cria grafico 2D em planta para esses pilares

                return Result.Succeeded;
            }
            catch (Exception exception) 
            {
                TaskDialog.Show("REVIT", exception.Message);
                return Result.Failed; 
            };
            
        }
    }
}
