using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Text;

namespace TopazioRevitPluginShared
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    internal class NormalizarPilares : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            //Get application and document objects
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Document doc = uidoc.Document;

            var pilaresDicts = new List<Dictionary<string, dynamic>>();

            var pilaresTipo = new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_StructuralColumns).WhereElementIsElementType();
            foreach (var pilar in pilaresTipo)
            {
                //TaskDialog.Show("DEBUG", pilar.Name);
                if (!pilar.Name.Contains("TQS"))
                {
                    continue;
                }
                var sketchPilar = Utils.GetBottonFaceCurveLoop(doc, pilar.Id);
                var aux = new Dictionary<string, dynamic> 
                {
                    {"ID", pilar.Id },
                    {"Sketch", sketchPilar },
                    {"Variations", new List<dynamic>() }
                };
                pilaresDicts.Add(aux);
            }

            var uniquePilares = new List<Dictionary<string, dynamic>>();
            foreach (var pilar in pilaresDicts)
            {
                if (uniquePilares.Count == 0)
                {
                    //TaskDialog.Show("DEBUG", "ADICIONEI O 1");
                    uniquePilares.Add(pilar);
                    continue;
                }
                int i = 0;
                bool add = true;
                foreach (var uniquePilar in uniquePilares)
                {
                    //TaskDialog.Show("DEBUG", i.ToString());
                    
                    if (Utils.AreCurveLoopsEqual(pilar["Sketch"], uniquePilar["Sketch"]))
                    {
                        add = false;
                        uniquePilar["Variations"].Add(pilar["ID"]);
                    }
                    i++;
                }
                if (add)
                {
                    uniquePilares.Add(pilar);
                }

            }
            //PARA CADA TIPO PILAR EM UNIQUEPILAR
                //PARA CADA TIPO EM VARIATIONS
                //SELECIONAR TODAS AS INSTANCIAS E SUBSTITUIR PELA INSTANCIA "MÃE"

            TaskDialog.Show("DEBUG", uniquePilares.Count.ToString());
            return Result.Succeeded;
        }
    }
}
