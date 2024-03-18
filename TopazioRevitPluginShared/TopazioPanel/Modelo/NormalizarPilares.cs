using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
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


            
            var deletarTipos = new List<dynamic>();
            foreach (var pilar in pilaresTipo)
            {
                //TaskDialog.Show("DEBUG", pilar.Name);
                if (pilar.Name.Contains("TQS") == false)
                {
                    continue;
                }

                //SALVA FAMILY TIPES NÃO UTILIZADOS PARA DELETAR

                var filtro = new FamilyInstanceFilter(doc, pilar.Id);
                var instanciasPilares = new FilteredElementCollector(doc).WherePasses(filtro).ToElements();
                if (instanciasPilares.Count() == 0)
                {
                    deletarTipos.Add(pilar);
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

            Transaction trans = new Transaction(doc);
            trans.Start("Normalizar Pilares");
            var hasElementInGroup = "";
            var hasSlantedColumn = "";
            foreach (var uniquePilarTipo in uniquePilares)
            {
                foreach (var variationPilarTipoId in uniquePilarTipo["Variations"])
                {
                    var filtro = new FamilyInstanceFilter(doc, variationPilarTipoId);
                    var pilaresVariations = new FilteredElementCollector(doc).WherePasses(filtro).ToElements();
                    
                    foreach (var pilar in pilaresVariations)
                    {
                        //APENAS SUBSTITUI INSTANCIAS VERTICAIS QUE ESTÃO FORA DE GRUPO
                        if (pilar.get_Parameter(BuiltInParameter.SLANTED_COLUMN_TYPE_PARAM).AsValueString() == "Vertical" & pilar.GroupId.IntegerValue == -1)
                        {
                            pilar.ChangeTypeId(uniquePilarTipo["ID"]);
                        }
                        if (pilar.get_Parameter(BuiltInParameter.SLANTED_COLUMN_TYPE_PARAM).AsValueString() != "Vertical")
                        {
                            var aux = Environment.NewLine + pilar.Id.ToString();
                            hasSlantedColumn += aux;
                        }
                        if (pilar.GroupId.IntegerValue != -1)
                        {
                            var aux = Environment.NewLine + pilar.Id.ToString();
                            hasElementInGroup += aux;
                        }                        
                    }
                }
            }
            trans.Commit();

            if (hasElementInGroup != "")
            {
                TaskDialog.Show("Topazio Revit Plugin", "Há pilares em grupos, para esses pilares trocar o tipo manualemte, ids:" + hasElementInGroup);
            }
            if (hasSlantedColumn != "")
            {
                TaskDialog.Show("Topazio Revit Plugin", "Há pilares inclinados, para esses pilares trocar o tipo manualemte, ids:" + hasSlantedColumn);
            }


            #region DELETAR TIPOS DE PILARES NÃO UTILIZADOS
            
            trans.Start("Deletar pilares tipo não utilizados");
            foreach (var pilarTipo in deletarTipos)
            {
                var family = pilarTipo.Family;
                var familyTypes = family.GetFamilySymbolIds();
                if (familyTypes.Count == 1)
                {
                    doc.Delete(family.Id);
                }
                else
                {
                    doc.Delete(pilarTipo.Id);
                }

            }
            trans.Commit();
            #endregion



            //TaskDialog.Show("DEBUG", uniquePilares.Count.ToString());
            return Result.Succeeded;
        }
    }
}
