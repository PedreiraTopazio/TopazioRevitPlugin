using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.ExtensibleStorage;
using Autodesk.Revit.UI;
using System;
using System.CodeDom;
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

            //CRIANDO O SCHEMA
            Guid schemaGUID = new Guid("196ce32e-39c1-44ea-8a47-cbb11f0b428a");
            Schema schema = Schema.Lookup(schemaGUID);
            if (schema == null)
            {
                SchemaBuilder storeCreator = new SchemaBuilder(schemaGUID); //TALVEZ SE EU COLOCAR O MESMO GUID AQUI E NA HORA QUE ESTIVER BUSCANDO DE CERTO
                storeCreator.SetReadAccessLevel(AccessLevel.Vendor);
                storeCreator.SetWriteAccessLevel(AccessLevel.Vendor);
                storeCreator.SetVendorId("TopazioBIM");
                storeCreator.SetSchemaName("HatchSchema");
                FieldBuilder fieldBuilder = storeCreator.AddSimpleField("Creator", typeof(string));
                schema = storeCreator.Finish();  //RETORNA O SCHEMA CRIADO
            }



            Entity entity = new Entity(schema);
            Field fieldStoreCreator = schema.GetField("Creator");

            //Excluir todos os hatchs
            Transaction trans = new Transaction(doc);
            trans.Start("Excluir Hachuras");
            var hatches = new FilteredElementCollector(doc, doc.ActiveView.Id).OfClass(typeof(FilledRegion)).WhereElementIsNotElementType().ToElements();
            foreach (var hatch in hatches)
            {
                try
                {
                    var creatorValue = hatch.GetEntity(schema).Get<string>(schema.GetField("Creator"));
                    if (creatorValue == "PilarNMC - TopazioRevitPlugin")
                    {
                        doc.Delete(hatch.Id);
                    }
                }
                catch (Exception) { }
            }
            trans.Commit();


            try
            {
                //Pega o nivel da vista
                var nivelId = Utils.GetLevelOfView(doc, view);

                //Verificar se existe eixos nesse projeto
                var grids = new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_Grids).WhereElementIsNotElementType().ToElements();
                if (grids.Count == 0)
                {
                    message += "Não há eixos criados no modelo";
                    return Result.Failed;
                }

                //Seleciona todos os pilares visiveis na vista
                List<Element> pilaresVisiveis = (List<Element>)new FilteredElementCollector(doc, view.Id).OfCategory(BuiltInCategory.OST_StructuralColumns).WhereElementIsNotElementType().ToElements();
                var pilaresDict = new List<Dictionary<string, dynamic>>();


                var pilaresInclinadosId = "";
                //Classificar os pilares em terminam e começam
                foreach (var pilar in pilaresVisiveis)
                {

                    string NM = "";

                    try
                    { //Se os pilares não tiverem esses parametros, simplesmente será ignorado
                        if (pilar.get_Parameter(BuiltInParameter.FAMILY_TOP_LEVEL_PARAM).AsElementId() == nivelId)
                        {
                            NM = "Morre";
                        }
                        if (pilar.get_Parameter(BuiltInParameter.FAMILY_BASE_LEVEL_PARAM).AsElementId() == nivelId)
                        {
                            NM = "Nasce";
                        }
                    }
                    catch
                    {
                        continue;
                    }

                    //Não vai rodar se o pilar for inclinado
                    if (pilar.get_Parameter(BuiltInParameter.SLANTED_COLUMN_TYPE_PARAM).AsValueString() != "Vertical")
                    {
                        pilaresInclinadosId += Environment.NewLine + pilar.Id.ToString();
                        continue;
                    }

                    var location = pilar.Location;
                    var point = location as LocationPoint;

                    var pilarId = pilar.Id;
                    var pilarTipo = pilar.GetTypeId();
                    var pilarXY = pilar.get_Parameter(BuiltInParameter.COLUMN_LOCATION_MARK).AsString();
                    var rotacao = point.Rotation;




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

                var overrideGraphicsReset = new OverrideGraphicSettings();
                Color cinzaMorre = new Color(128, 128, 128);

                var morrePatternId = new FilteredElementCollector(doc)
                    .OfClass(typeof(FillPatternElement))
                    .FirstOrDefault(pattern => pattern.Name == "PED_MORRE")
                    .Id;
                var nascePatternId = new FilteredElementCollector(doc)
                    .OfClass(typeof(FilledRegionType))
                    .FirstOrDefault(pattern => pattern.Name == "PED_HTC10_PILAR.NASCE")
                    .Id;

                trans.Start("Pilar NMC");
                var pilaresNascemErrorId = "";
                foreach (Dictionary<string, dynamic> dict in pilaresDict)
                {
                    string NM = dict["NM"];
                    ElementId ID = dict["Id"];
                    Element pilar = doc.GetElement(ID);
                    //Pilares que continua -> Reseta o override graphics
                    if (NM == "Continua")
                    {
                        doc.ActiveView.SetElementOverrides(ID, overrideGraphicsReset);
                    }
                    //Pilares que morrem->Sobrepõe grafico com hatch de morre
                    if (NM == "Morre")
                    {
                        OverrideGraphicSettings CurrentOverride = doc.ActiveView.GetElementOverrides(ID);
                        //CORTE
                        CurrentOverride.SetCutForegroundPatternId(morrePatternId); //PREENCHIMENTO MORRE ID
                        CurrentOverride.SetCutForegroundPatternColor(cinzaMorre);
                        //VISTA
                        CurrentOverride.SetSurfaceForegroundPatternId(morrePatternId); //PREENCHIMENTO MORRE ID
                        CurrentOverride.SetSurfaceForegroundPatternColor(cinzaMorre);
                        doc.ActiveView.SetElementOverrides(ID, CurrentOverride);
                    }
                    //Pilares que nasce -> Cria grafico 2D em planta para esses pilares
                    
                    if (NM == "Nasce")
                    {
                        pilar = doc.GetElement(ID);
                        doc.ActiveView.SetElementOverrides(ID, overrideGraphicsReset);
                        var CurveLoop = Utils.GetBottonFaceCurveLoop(doc, ID);
                        try
                        {
                            var hatchId = FilledRegion.Create(doc, nascePatternId, doc.ActiveView.Id, CurveLoop).Id;
                            //AQUI EU VOU DEFINIR O Extensible Storage NOS HATCHS
                            entity.Set<string>(fieldStoreCreator, "PilarNMC - TopazioRevitPlugin");
                            doc.GetElement(hatchId).SetEntity(entity);

                        }
                        catch (Exception ex)
                        {
                            pilaresNascemErrorId += Environment.NewLine + pilar.Id.ToString();
                        }

                    }



                }
                trans.Commit();
                if (pilaresNascemErrorId != "")
                {
                    TaskDialog.Show("Erro", "Verificar os pilares que nascem, ids: " + pilaresNascemErrorId);
                }
                
                if (pilaresInclinadosId != "") 
                {
                    TaskDialog.Show("Erro", "Verificar os pilares inclinados manualmente, ids: " + pilaresInclinadosId);
                }
                
                return Result.Succeeded;

            }
            catch (Exception exception)
            {
                TaskDialog.Show("REVIT", exception.Message + " " + exception.Source + " " + exception.InnerException);
                return Result.Failed;
            };

        }
    }
}
