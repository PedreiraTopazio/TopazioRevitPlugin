using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.ExtensibleStorage;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using TopazioRevitPlugin.TopazioPanel.Documentação;

namespace TopazioRevitPluginShared
{


    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    internal class DropButton : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            //Get application and document objects
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Document doc = uidoc.Document;

            View activeView = doc.ActiveView;
            #region TESTA SE ESTAMOS EM UMA VISTA 2D
            View3D view3D = activeView as View3D;
            if (null != view3D)
            {
                message += "Criar hachuras somente em vista 2D";
                return Result.Failed;
            }
            ViewSheet viewSheet = activeView as ViewSheet;
            if (null != viewSheet)
            {
                message += "Criar hachuras somente em vista 2D";
                return Autodesk.Revit.UI.Result.Failed;
            }
            #endregion  

            var vigas = new FilteredElementCollector(doc, doc.ActiveView.Id).WherePasses(new ElementCategoryFilter(BuiltInCategory.OST_StructuralFraming)).ToElements();
            var pisos = new FilteredElementCollector(doc, doc.ActiveView.Id).WherePasses(new ElementCategoryFilter(BuiltInCategory.OST_Floors)).ToElements();
            var pilares = new FilteredElementCollector(doc, doc.ActiveView.Id).WherePasses(new ElementCategoryFilter(BuiltInCategory.OST_StructuralColumns)).ToElements();

            ElementId levelId = Utils.GetLevelOfView(doc, doc.ActiveView);
            Element levelElem = doc.GetElement(levelId);


            //PROCURANDO O SCHEMA
            Guid schemaGUID = new Guid("58ffa6af-2458-4da8-8a7c-7d8beb619a29");
            Schema schema = Schema.Lookup(schemaGUID);

            //SE NÃO TIVER ESQUEMA, retornar erro
            if (schema == null)
            {
                TaskDialog.Show("Error", "Definir cores no modelo primeiro.");
                return Result.Failed;
            }

            var dataStorage =
            new FilteredElementCollector(doc)
            .OfClass(typeof(DataStorage))
            .FirstElement();

            if (dataStorage == null)
            {
                TaskDialog.Show("Error", "Definir cores no modelo primeiro.");
                return Result.Failed;
            }

            //GET EXISTING VALUES
            Entity entity = dataStorage.GetEntity(schema);
            Level level = doc.GetElement(levelId) as Level;
            Entity Levelentity = level.GetEntity(schema);

            Dictionary<double, Color> DictDesniveisColor = new Dictionary<double, Color>();

            if (Levelentity.IsValid())
            {
                int index = 1;
                while (index <= 20)
                {
                    if (Levelentity.Get<string>(schema.GetField("PED_HTCD" + index.ToString() + "_DESNIVEL_COLOR")) != "")
                    {
                        var desnivel = Math.Round(Convert.ToDouble(Levelentity.Get<string>(schema.GetField("PED_HTCD" + index.ToString() + "_DESNIVEL_COLOR"))), 2);
                        var color = Utils.ColorFrom9String(entity.Get<string>(schema.GetField("PED_HTCD" + index.ToString() + "_DESNIVEL_COLOR")));
                        try
                        {
                            DictDesniveisColor.Add(desnivel, color);
                        }
                        catch
                        {
                            TaskDialog.Show("Error", "Mais de uma de cor atribuida ao mesmo desnivel");
                        }
                        
                    }
                    
                    index++;
                }
            }



            #region Aplicar sobreposição nos elementos
            Transaction trans = new Transaction(doc);
            trans.Start("Colorir Desniveis");
            FillPatternElement solidFillPattern = new FilteredElementCollector(doc).OfClass(typeof(FillPatternElement)).Cast<FillPatternElement>().First(a => a.GetFillPattern().IsSolidFill);
            foreach (var piso in pisos)
                {
                if (piso.LookupParameter("Titulo").AsString() != "")
                {
                    ElementId elementlevel = piso.LookupParameter("Nível").AsElementId();
                    double desnivel = Math.Round(Utils.ConvertFromInternalUnits(doc, piso.LookupParameter("Altura do deslocamento do nível").AsDouble()), 2);
                    if (elementlevel != levelId) 
                    {
                        var elementlevelElem = doc.GetElement(elementlevel);
                        desnivel = Math.Round(Utils.ConvertFromInternalUnits(doc, elementlevelElem.LookupParameter("Elevação").AsDouble() - levelElem.LookupParameter("Elevação").AsDouble()), 2) + desnivel;
                    }                        

                    if (DictDesniveisColor.ContainsKey(desnivel)) 
                    {
                        OverrideGraphicSettings overrideGraphicSettings = activeView.GetElementOverrides(piso.Id);

                        //Corte
                        overrideGraphicSettings.SetCutForegroundPatternColor(DictDesniveisColor[desnivel]);
                        overrideGraphicSettings.SetCutForegroundPatternId(solidFillPattern.Id);

                        //Vista
                        overrideGraphicSettings.SetSurfaceForegroundPatternColor(DictDesniveisColor[desnivel]);
                        overrideGraphicSettings.SetSurfaceForegroundPatternId(solidFillPattern.Id);

                        activeView.SetElementOverrides(piso.Id, overrideGraphicSettings);
                    }
                }
            }
            foreach (var viga in vigas)
            {

                if (viga.LookupParameter("Titulo").AsString() != "")
                {
                    //COLOCAR O MÉTODO PARA DESBLOQUEAR O NIVEL

                    if (viga.LookupParameter("Deslocamento do nível inicial") == null || viga.LookupParameter("Deslocamento do nível final") == null)
                    {
                        continue;
                    }

                    var desnivelInicial = Math.Round(Utils.ConvertFromInternalUnits(doc, viga.LookupParameter("Deslocamento do nível inicial").AsDouble()), 2);
                    var desnivelFinal = Math.Round(Utils.ConvertFromInternalUnits(doc, viga.LookupParameter("Deslocamento do nível final").AsDouble()), 2);
                    var desnivel = desnivelInicial;
                    if (desnivelInicial != desnivelFinal)
                    {
                        continue;
                    }

                    ElementId elementlevel = viga.LookupParameter("Nível de referência").AsElementId();
                    if (elementlevel != levelId)
                    {
                        var elementlevelElem = doc.GetElement(elementlevel);
                        desnivel = Math.Round(Utils.ConvertFromInternalUnits(doc, elementlevelElem.LookupParameter("Elevação").AsDouble() - levelElem.LookupParameter("Elevação").AsDouble()), 2) + desnivel;
                    }

                    if (DictDesniveisColor.ContainsKey(desnivel))
                    {
                        OverrideGraphicSettings overrideGraphicSettings = activeView.GetElementOverrides(viga.Id);

                        //Corte
                        overrideGraphicSettings.SetCutForegroundPatternColor(DictDesniveisColor[desnivel]);
                        overrideGraphicSettings.SetCutForegroundPatternId(solidFillPattern.Id);

                        //Vista
                        overrideGraphicSettings.SetSurfaceForegroundPatternColor(DictDesniveisColor[desnivel]);
                        overrideGraphicSettings.SetSurfaceForegroundPatternId(solidFillPattern.Id);

                        activeView.SetElementOverrides(viga.Id, overrideGraphicSettings);
                    }

                }
            }
            foreach (var pilar in pilares)
            {
                if (pilar.LookupParameter("Titulo").AsString() == "") { continue; }
                var desnivel = Math.Round(Utils.ConvertFromInternalUnits(doc, pilar.LookupParameter("Deslocamento superior").AsDouble()), 2);

                ElementId elementlevel = pilar.LookupParameter("Nível superior").AsElementId();
                if (elementlevel != levelId)
                {
                    continue;
                }

                if (DictDesniveisColor.ContainsKey(desnivel))
                {
                    OverrideGraphicSettings overrideGraphicSettings = activeView.GetElementOverrides(pilar.Id);

                    //Corte
                    overrideGraphicSettings.SetCutBackgroundPatternColor(DictDesniveisColor[desnivel]);
                    overrideGraphicSettings.SetCutBackgroundPatternId(solidFillPattern.Id);

                    //Vista
                    overrideGraphicSettings.SetSurfaceBackgroundPatternColor(DictDesniveisColor[desnivel]);
                    overrideGraphicSettings.SetSurfaceBackgroundPatternId(solidFillPattern.Id);

                    activeView.SetElementOverrides(pilar.Id, overrideGraphicSettings);
                }
            }
            trans.Commit();
            #endregion

            return Result.Succeeded;
        }
    }

    #region Botão Config Model
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class ConfigModelButton : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            //Get application and document objects
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Document doc = uidoc.Document;

            var dataStorage =
            new FilteredElementCollector(doc)
            .OfClass(typeof(DataStorage))
            .FirstElement();

            if (dataStorage == null)
            {
                //Criando o data Storage
                dataStorage = DataStorage.Create(doc);
            }


            //PROCURANDO O SCHEMA
            Guid schemaGUID = new Guid("58ffa6af-2458-4da8-8a7c-7d8beb619a29");
            Schema schema = Schema.Lookup(schemaGUID);

            //SE NÃO TIVER ESQUEMA, CRIAR ESQUEMA
            if (schema == null)
            {
                SchemaBuilder storeCreator = new SchemaBuilder(schemaGUID); //TALVEZ SE EU COLOCAR O MESMO GUID AQUI E NA HORA QUE ESTIVER BUSCANDO DE CERTO
                storeCreator.SetReadAccessLevel(AccessLevel.Public);
                storeCreator.SetWriteAccessLevel(AccessLevel.Public);
                storeCreator.SetVendorId("TopazioBIM");
                storeCreator.SetSchemaName("DesniveisSchema");

                //CRIANDO OS FIELDS
                storeCreator.AddSimpleField("PED_HTCD1_DESNIVEL_COLOR", typeof(string));
                storeCreator.AddSimpleField("PED_HTCD2_DESNIVEL_COLOR", typeof(string));
                storeCreator.AddSimpleField("PED_HTCD3_DESNIVEL_COLOR", typeof(string));
                storeCreator.AddSimpleField("PED_HTCD4_DESNIVEL_COLOR", typeof(string));
                storeCreator.AddSimpleField("PED_HTCD5_DESNIVEL_COLOR", typeof(string));
                storeCreator.AddSimpleField("PED_HTCD6_DESNIVEL_COLOR", typeof(string));
                storeCreator.AddSimpleField("PED_HTCD7_DESNIVEL_COLOR", typeof(string));
                storeCreator.AddSimpleField("PED_HTCD8_DESNIVEL_COLOR", typeof(string));
                storeCreator.AddSimpleField("PED_HTCD9_DESNIVEL_COLOR", typeof(string));
                storeCreator.AddSimpleField("PED_HTCD10_DESNIVEL_COLOR", typeof(string));
                storeCreator.AddSimpleField("PED_HTCD11_DESNIVEL_COLOR", typeof(string));
                storeCreator.AddSimpleField("PED_HTCD12_DESNIVEL_COLOR", typeof(string));
                storeCreator.AddSimpleField("PED_HTCD13_DESNIVEL_COLOR", typeof(string));
                storeCreator.AddSimpleField("PED_HTCD14_DESNIVEL_COLOR", typeof(string));
                storeCreator.AddSimpleField("PED_HTCD15_DESNIVEL_COLOR", typeof(string));
                storeCreator.AddSimpleField("PED_HTCD16_DESNIVEL_COLOR", typeof(string));
                storeCreator.AddSimpleField("PED_HTCD17_DESNIVEL_COLOR", typeof(string));
                storeCreator.AddSimpleField("PED_HTCD18_DESNIVEL_COLOR", typeof(string));
                storeCreator.AddSimpleField("PED_HTCD19_DESNIVEL_COLOR", typeof(string));
                storeCreator.AddSimpleField("PED_HTCD20_DESNIVEL_COLOR", typeof(string));
                schema = storeCreator.Finish();  //RETORNA O SCHEMA CRIADO
                Transaction trans = new Transaction(doc);
                trans.Start("Schema");
                dataStorage.SetEntity(new Entity(schema));
                trans.Commit();
            }


            

            ConfigModelForm form = new ConfigModelForm(this);

            IExternalEventHandler handler_event = new AplicarCoresForm(form);
            ExternalEvent exEvent = ExternalEvent.Create(handler_event);

            form.exEvent = exEvent;




            //GET EXISTING VALUES
            Entity entity = dataStorage.GetEntity(schema);
            var color1Value = entity.Get<string>(schema.GetField("PED_HTCD1_DESNIVEL_COLOR"));
            var color2Value = entity.Get<string>(schema.GetField("PED_HTCD2_DESNIVEL_COLOR"));
            var color3Value = entity.Get<string>(schema.GetField("PED_HTCD3_DESNIVEL_COLOR"));
            var color4Value = entity.Get<string>(schema.GetField("PED_HTCD4_DESNIVEL_COLOR"));
            var color5Value = entity.Get<string>(schema.GetField("PED_HTCD5_DESNIVEL_COLOR"));
            var color6Value = entity.Get<string>(schema.GetField("PED_HTCD6_DESNIVEL_COLOR"));
            var color7Value = entity.Get<string>(schema.GetField("PED_HTCD7_DESNIVEL_COLOR"));
            var color8Value = entity.Get<string>(schema.GetField("PED_HTCD8_DESNIVEL_COLOR"));
            var color9Value = entity.Get<string>(schema.GetField("PED_HTCD9_DESNIVEL_COLOR"));
            var color10Value = entity.Get<string>(schema.GetField("PED_HTCD10_DESNIVEL_COLOR"));
            var color11Value = entity.Get<string>(schema.GetField("PED_HTCD11_DESNIVEL_COLOR"));
            var color12Value = entity.Get<string>(schema.GetField("PED_HTCD12_DESNIVEL_COLOR"));
            var color13Value = entity.Get<string>(schema.GetField("PED_HTCD13_DESNIVEL_COLOR"));
            var color14Value = entity.Get<string>(schema.GetField("PED_HTCD14_DESNIVEL_COLOR"));
            var color15Value = entity.Get<string>(schema.GetField("PED_HTCD15_DESNIVEL_COLOR"));
            var color16Value = entity.Get<string>(schema.GetField("PED_HTCD16_DESNIVEL_COLOR"));
            var color17Value = entity.Get<string>(schema.GetField("PED_HTCD17_DESNIVEL_COLOR"));
            var color18Value = entity.Get<string>(schema.GetField("PED_HTCD18_DESNIVEL_COLOR"));
            var color19Value = entity.Get<string>(schema.GetField("PED_HTCD19_DESNIVEL_COLOR"));
            var color20Value = entity.Get<string>(schema.GetField("PED_HTCD20_DESNIVEL_COLOR"));

            if (color1Value != "")
            {
                form.PED_HTCD1_DESNIVEL_COLOR.BackColor = System.Drawing.Color.FromArgb(Convert.ToInt32(color1Value.Substring(0, 3)), Convert.ToInt32(color1Value.Substring(3, 3)), Convert.ToInt32(color1Value.Substring(6, 3)));
            }
            if (color2Value != "")
            {
                form.PED_HTCD2_DESNIVEL_COLOR.BackColor = System.Drawing.Color.FromArgb(Convert.ToInt32(color2Value.Substring(0, 3)), Convert.ToInt32(color2Value.Substring(3, 3)), Convert.ToInt32(color2Value.Substring(6, 3)));
            }
            if (color3Value != "")
            {
                form.PED_HTCD3_DESNIVEL_COLOR.BackColor = System.Drawing.Color.FromArgb(Convert.ToInt32(color3Value.Substring(0, 3)), Convert.ToInt32(color3Value.Substring(3, 3)), Convert.ToInt32(color3Value.Substring(6, 3)));
            }
            if (color4Value != "")
            {
                form.PED_HTCD4_DESNIVEL_COLOR.BackColor = System.Drawing.Color.FromArgb(Convert.ToInt32(color4Value.Substring(0, 3)), Convert.ToInt32(color4Value.Substring(3, 3)), Convert.ToInt32(color4Value.Substring(6, 3)));
            }
            if (color5Value != "")
            {
                form.PED_HTCD5_DESNIVEL_COLOR.BackColor = System.Drawing.Color.FromArgb(Convert.ToInt32(color5Value.Substring(0, 3)), Convert.ToInt32(color5Value.Substring(3, 3)), Convert.ToInt32(color5Value.Substring(6, 3)));
            }
            if (color6Value != "")
            {
                form.PED_HTCD6_DESNIVEL_COLOR.BackColor = System.Drawing.Color.FromArgb(Convert.ToInt32(color6Value.Substring(0, 3)), Convert.ToInt32(color6Value.Substring(3, 3)), Convert.ToInt32(color6Value.Substring(6, 3)));
            }
            if (color7Value != "")
            {
                form.PED_HTCD7_DESNIVEL_COLOR.BackColor = System.Drawing.Color.FromArgb(Convert.ToInt32(color7Value.Substring(0, 3)), Convert.ToInt32(color7Value.Substring(3, 3)), Convert.ToInt32(color7Value.Substring(6, 3)));
            }
            if (color8Value != "")
            {
                form.PED_HTCD8_DESNIVEL_COLOR.BackColor = System.Drawing.Color.FromArgb(Convert.ToInt32(color8Value.Substring(0, 3)), Convert.ToInt32(color8Value.Substring(3, 3)), Convert.ToInt32(color8Value.Substring(6, 3)));
            }
            if (color9Value != "")
            {
                form.PED_HTCD9_DESNIVEL_COLOR.BackColor = System.Drawing.Color.FromArgb(Convert.ToInt32(color9Value.Substring(0, 3)), Convert.ToInt32(color9Value.Substring(3, 3)), Convert.ToInt32(color9Value.Substring(6, 3)));
            }
            if (color10Value != "")
            {
                form.PED_HTCD10_DESNIVEL_COLOR.BackColor = System.Drawing.Color.FromArgb(Convert.ToInt32(color10Value.Substring(0, 3)), Convert.ToInt32(color10Value.Substring(3, 3)), Convert.ToInt32(color10Value.Substring(6, 3)));
            }
            if (color11Value != "")
            {
                form.PED_HTCD11_DESNIVEL_COLOR.BackColor = System.Drawing.Color.FromArgb(Convert.ToInt32(color11Value.Substring(0, 3)), Convert.ToInt32(color11Value.Substring(3, 3)), Convert.ToInt32(color11Value.Substring(6, 3)));
            }
            if (color12Value != "")
            {
                form.PED_HTCD12_DESNIVEL_COLOR.BackColor = System.Drawing.Color.FromArgb(Convert.ToInt32(color12Value.Substring(0, 3)), Convert.ToInt32(color12Value.Substring(3, 3)), Convert.ToInt32(color12Value.Substring(6, 3)));
            }
            if (color13Value != "")
            {
                form.PED_HTCD13_DESNIVEL_COLOR.BackColor = System.Drawing.Color.FromArgb(Convert.ToInt32(color13Value.Substring(0, 3)), Convert.ToInt32(color13Value.Substring(3, 3)), Convert.ToInt32(color13Value.Substring(6, 3)));
            }
            if (color14Value != "")
            {
                form.PED_HTCD14_DESNIVEL_COLOR.BackColor = System.Drawing.Color.FromArgb(Convert.ToInt32(color14Value.Substring(0, 3)), Convert.ToInt32(color14Value.Substring(3, 3)), Convert.ToInt32(color14Value.Substring(6, 3)));
            }
            if (color15Value != "")
            {
                form.PED_HTCD15_DESNIVEL_COLOR.BackColor = System.Drawing.Color.FromArgb(Convert.ToInt32(color15Value.Substring(0, 3)), Convert.ToInt32(color15Value.Substring(3, 3)), Convert.ToInt32(color15Value.Substring(6, 3)));
            }
            if (color16Value != "")
            {
                form.PED_HTCD16_DESNIVEL_COLOR.BackColor = System.Drawing.Color.FromArgb(Convert.ToInt32(color16Value.Substring(0, 3)), Convert.ToInt32(color16Value.Substring(3, 3)), Convert.ToInt32(color16Value.Substring(6, 3)));
            }
            if (color17Value != "")
            {
                form.PED_HTCD17_DESNIVEL_COLOR.BackColor = System.Drawing.Color.FromArgb(Convert.ToInt32(color17Value.Substring(0, 3)), Convert.ToInt32(color17Value.Substring(3, 3)), Convert.ToInt32(color17Value.Substring(6, 3)));
            }
            if (color18Value != "")
            {
                form.PED_HTCD18_DESNIVEL_COLOR.BackColor = System.Drawing.Color.FromArgb(Convert.ToInt32(color18Value.Substring(0, 3)), Convert.ToInt32(color18Value.Substring(3, 3)), Convert.ToInt32(color18Value.Substring(6, 3)));
            }
            if (color19Value != "")
            {
                form.PED_HTCD19_DESNIVEL_COLOR.BackColor = System.Drawing.Color.FromArgb(Convert.ToInt32(color19Value.Substring(0, 3)), Convert.ToInt32(color19Value.Substring(3, 3)), Convert.ToInt32(color19Value.Substring(6, 3)));
            }
            if (color20Value != "")
            {
                form.PED_HTCD20_DESNIVEL_COLOR.BackColor = System.Drawing.Color.FromArgb(Convert.ToInt32(color20Value.Substring(0, 3)), Convert.ToInt32(color20Value.Substring(3, 3)), Convert.ToInt32(color20Value.Substring(6, 3)));
            }

            form.Show();

            return Result.Succeeded;
        }
    }
    #region Salvar Cores no Modelo
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    class AplicarCoresForm : IExternalEventHandler
    {
        ConfigModelForm form;

        public AplicarCoresForm(ConfigModelForm form)
        {
            this.form = form;
        }

        public void Execute(UIApplication app)
        {
            Document doc = app.ActiveUIDocument.Document;
            using (Transaction t = new Transaction(doc, "CameraTransaction"))
            {
                t.Start();
                Dictionary<string, string> colorlist = new Dictionary<string, string>
            {
                {"PED_HTCD1_DESNIVEL_COLOR", form.PED_HTCD1_DESNIVEL_COLOR.BackColor.R.ToString().PadLeft(3, '0')   + form.PED_HTCD1_DESNIVEL_COLOR.BackColor.G.ToString().PadLeft(3, '0')  + form.PED_HTCD1_DESNIVEL_COLOR.BackColor.B.ToString().PadLeft(3, '0')},
                {"PED_HTCD2_DESNIVEL_COLOR", form.PED_HTCD2_DESNIVEL_COLOR.BackColor.R.ToString().PadLeft(3, '0')   + form.PED_HTCD2_DESNIVEL_COLOR.BackColor.G.ToString().PadLeft(3, '0')  + form.PED_HTCD2_DESNIVEL_COLOR.BackColor.B.ToString().PadLeft(3, '0')},
                {"PED_HTCD3_DESNIVEL_COLOR", form.PED_HTCD3_DESNIVEL_COLOR.BackColor.R.ToString().PadLeft(3, '0')   + form.PED_HTCD3_DESNIVEL_COLOR.BackColor.G.ToString().PadLeft(3, '0')  + form.PED_HTCD3_DESNIVEL_COLOR.BackColor.B.ToString().PadLeft(3, '0')},
                {"PED_HTCD4_DESNIVEL_COLOR", form.PED_HTCD4_DESNIVEL_COLOR.BackColor.R.ToString().PadLeft(3, '0')   + form.PED_HTCD4_DESNIVEL_COLOR.BackColor.G.ToString().PadLeft(3, '0')  + form.PED_HTCD4_DESNIVEL_COLOR.BackColor.B.ToString().PadLeft(3, '0')},
                {"PED_HTCD5_DESNIVEL_COLOR", form.PED_HTCD5_DESNIVEL_COLOR.BackColor.R.ToString().PadLeft(3, '0')   + form.PED_HTCD5_DESNIVEL_COLOR.BackColor.G.ToString().PadLeft(3, '0')  + form.PED_HTCD5_DESNIVEL_COLOR.BackColor.B.ToString().PadLeft(3, '0')},
                {"PED_HTCD6_DESNIVEL_COLOR", form.PED_HTCD6_DESNIVEL_COLOR.BackColor.R.ToString().PadLeft(3, '0')   + form.PED_HTCD6_DESNIVEL_COLOR.BackColor.G.ToString().PadLeft(3, '0')  + form.PED_HTCD6_DESNIVEL_COLOR.BackColor.B.ToString().PadLeft(3, '0')},
                {"PED_HTCD7_DESNIVEL_COLOR", form.PED_HTCD7_DESNIVEL_COLOR.BackColor.R.ToString().PadLeft(3, '0')   + form.PED_HTCD7_DESNIVEL_COLOR.BackColor.G.ToString().PadLeft(3, '0')  + form.PED_HTCD7_DESNIVEL_COLOR.BackColor.B.ToString().PadLeft(3, '0')},
                {"PED_HTCD8_DESNIVEL_COLOR", form.PED_HTCD8_DESNIVEL_COLOR.BackColor.R.ToString().PadLeft(3, '0')   + form.PED_HTCD8_DESNIVEL_COLOR.BackColor.G.ToString().PadLeft(3, '0')  + form.PED_HTCD8_DESNIVEL_COLOR.BackColor.B.ToString().PadLeft(3, '0')},
                {"PED_HTCD9_DESNIVEL_COLOR", form.PED_HTCD9_DESNIVEL_COLOR.BackColor.R.ToString().PadLeft(3, '0')   + form.PED_HTCD9_DESNIVEL_COLOR.BackColor.G.ToString().PadLeft(3, '0')  + form.PED_HTCD9_DESNIVEL_COLOR.BackColor.B.ToString().PadLeft(3, '0')},
                {"PED_HTCD10_DESNIVEL_COLOR", form.PED_HTCD10_DESNIVEL_COLOR.BackColor.R.ToString().PadLeft(3, '0') + form.PED_HTCD10_DESNIVEL_COLOR.BackColor.G.ToString().PadLeft(3, '0') + form.PED_HTCD10_DESNIVEL_COLOR.BackColor.B.ToString().PadLeft(3, '0')},
                {"PED_HTCD11_DESNIVEL_COLOR", form.PED_HTCD11_DESNIVEL_COLOR.BackColor.R.ToString().PadLeft(3, '0') + form.PED_HTCD11_DESNIVEL_COLOR.BackColor.G.ToString().PadLeft(3, '0') + form.PED_HTCD11_DESNIVEL_COLOR.BackColor.B.ToString().PadLeft(3, '0')},
                {"PED_HTCD12_DESNIVEL_COLOR", form.PED_HTCD12_DESNIVEL_COLOR.BackColor.R.ToString().PadLeft(3, '0') + form.PED_HTCD12_DESNIVEL_COLOR.BackColor.G.ToString().PadLeft(3, '0') + form.PED_HTCD12_DESNIVEL_COLOR.BackColor.B.ToString().PadLeft(3, '0')},
                {"PED_HTCD13_DESNIVEL_COLOR", form.PED_HTCD13_DESNIVEL_COLOR.BackColor.R.ToString().PadLeft(3, '0') + form.PED_HTCD13_DESNIVEL_COLOR.BackColor.G.ToString().PadLeft(3, '0') + form.PED_HTCD13_DESNIVEL_COLOR.BackColor.B.ToString().PadLeft(3, '0')},
                {"PED_HTCD14_DESNIVEL_COLOR", form.PED_HTCD14_DESNIVEL_COLOR.BackColor.R.ToString().PadLeft(3, '0') + form.PED_HTCD14_DESNIVEL_COLOR.BackColor.G.ToString().PadLeft(3, '0') + form.PED_HTCD14_DESNIVEL_COLOR.BackColor.B.ToString().PadLeft(3, '0')},
                {"PED_HTCD15_DESNIVEL_COLOR", form.PED_HTCD15_DESNIVEL_COLOR.BackColor.R.ToString().PadLeft(3, '0') + form.PED_HTCD15_DESNIVEL_COLOR.BackColor.G.ToString().PadLeft(3, '0') + form.PED_HTCD15_DESNIVEL_COLOR.BackColor.B.ToString().PadLeft(3, '0')},
                {"PED_HTCD16_DESNIVEL_COLOR", form.PED_HTCD16_DESNIVEL_COLOR.BackColor.R.ToString().PadLeft(3, '0') + form.PED_HTCD16_DESNIVEL_COLOR.BackColor.G.ToString().PadLeft(3, '0') + form.PED_HTCD16_DESNIVEL_COLOR.BackColor.B.ToString().PadLeft(3, '0')},
                {"PED_HTCD17_DESNIVEL_COLOR", form.PED_HTCD17_DESNIVEL_COLOR.BackColor.R.ToString().PadLeft(3, '0') + form.PED_HTCD17_DESNIVEL_COLOR.BackColor.G.ToString().PadLeft(3, '0') + form.PED_HTCD17_DESNIVEL_COLOR.BackColor.B.ToString().PadLeft(3, '0')},
                {"PED_HTCD18_DESNIVEL_COLOR", form.PED_HTCD18_DESNIVEL_COLOR.BackColor.R.ToString().PadLeft(3, '0') + form.PED_HTCD18_DESNIVEL_COLOR.BackColor.G.ToString().PadLeft(3, '0') + form.PED_HTCD18_DESNIVEL_COLOR.BackColor.B.ToString().PadLeft(3, '0')},
                {"PED_HTCD19_DESNIVEL_COLOR", form.PED_HTCD19_DESNIVEL_COLOR.BackColor.R.ToString().PadLeft(3, '0') + form.PED_HTCD19_DESNIVEL_COLOR.BackColor.G.ToString().PadLeft(3, '0') + form.PED_HTCD19_DESNIVEL_COLOR.BackColor.B.ToString().PadLeft(3, '0')},
                {"PED_HTCD20_DESNIVEL_COLOR", form.PED_HTCD20_DESNIVEL_COLOR.BackColor.R.ToString().PadLeft(3, '0') + form.PED_HTCD20_DESNIVEL_COLOR.BackColor.G.ToString().PadLeft(3, '0') + form.PED_HTCD20_DESNIVEL_COLOR.BackColor.B.ToString().PadLeft(3, '0')}
            };

                //PROCURANDO O SCHEMA
                Guid schemaGUID = new Guid("58ffa6af-2458-4da8-8a7c-7d8beb619a29");
                Schema schema = Schema.Lookup(schemaGUID);

                var dataStorage =
                new FilteredElementCollector(doc)
                .OfClass(typeof(DataStorage))
                .FirstElement();

                Entity entity = dataStorage.GetEntity(schema);

                foreach (var par in colorlist)
                {
                    Field fieldStoreCreator = schema.GetField(par.Key);
                    entity.Set<string>(fieldStoreCreator, par.Value);
                }

                dataStorage.SetEntity(entity);
                t.Commit();

            };
        }

        public string GetName()
        {
            return "AplicarCoresForm";
        }
    }
    #endregion
    #endregion

    #region Botão Config Level
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class ConfigLevelButton : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            //Get application and document objects
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Document doc = uidoc.Document;

            View activeView = doc.ActiveView;
            #region TESTA SE ESTAMOS EM UMA VISTA 2D
            View3D view3D = activeView as View3D;
            if (null != view3D)
            {
                message += "Criar hachuras somente em vista 2D";
                return Result.Failed;
            }
            ViewSheet viewSheet = activeView as ViewSheet;
            if (null != viewSheet)
            {
                message += "Criar hachuras somente em vista 2D";
                return Autodesk.Revit.UI.Result.Failed;
            }
            #endregion  

            ElementId levelID = Utils.GetLevelOfView(doc, activeView);

            //PROCURANDO O SCHEMA
            Guid schemaGUID = new Guid("58ffa6af-2458-4da8-8a7c-7d8beb619a29");
            Schema schema = Schema.Lookup(schemaGUID);

            //SE NÃO TIVER ESQUEMA, retornar erro
            if (schema == null)
            {
                TaskDialog.Show("Error", "Definir cores no modelo primeiro.");
                return Result.Failed;
            }


            ConfigLevelForm form = new ConfigLevelForm(this);

            #region Aplicando cores do documento ao forms do nivel
            var dataStorage =
            new FilteredElementCollector(doc)
            .OfClass(typeof(DataStorage))
            .FirstElement();

            if (dataStorage == null)
            {
                TaskDialog.Show("Error", "Definir cores no modelo primeiro.");
                return Result.Failed;
            }

            //GET EXISTING VALUES
            Entity entity = dataStorage.GetEntity(schema);
            var color1Value = entity.Get<string>(schema.GetField("PED_HTCD1_DESNIVEL_COLOR"));
            var color2Value = entity.Get<string>(schema.GetField("PED_HTCD2_DESNIVEL_COLOR"));
            var color3Value = entity.Get<string>(schema.GetField("PED_HTCD3_DESNIVEL_COLOR"));
            var color4Value = entity.Get<string>(schema.GetField("PED_HTCD4_DESNIVEL_COLOR"));
            var color5Value = entity.Get<string>(schema.GetField("PED_HTCD5_DESNIVEL_COLOR"));
            var color6Value = entity.Get<string>(schema.GetField("PED_HTCD6_DESNIVEL_COLOR"));
            var color7Value = entity.Get<string>(schema.GetField("PED_HTCD7_DESNIVEL_COLOR"));
            var color8Value = entity.Get<string>(schema.GetField("PED_HTCD8_DESNIVEL_COLOR"));
            var color9Value = entity.Get<string>(schema.GetField("PED_HTCD9_DESNIVEL_COLOR"));
            var color10Value = entity.Get<string>(schema.GetField("PED_HTCD10_DESNIVEL_COLOR"));
            var color11Value = entity.Get<string>(schema.GetField("PED_HTCD11_DESNIVEL_COLOR"));
            var color12Value = entity.Get<string>(schema.GetField("PED_HTCD12_DESNIVEL_COLOR"));
            var color13Value = entity.Get<string>(schema.GetField("PED_HTCD13_DESNIVEL_COLOR"));
            var color14Value = entity.Get<string>(schema.GetField("PED_HTCD14_DESNIVEL_COLOR"));
            var color15Value = entity.Get<string>(schema.GetField("PED_HTCD15_DESNIVEL_COLOR"));
            var color16Value = entity.Get<string>(schema.GetField("PED_HTCD16_DESNIVEL_COLOR"));
            var color17Value = entity.Get<string>(schema.GetField("PED_HTCD17_DESNIVEL_COLOR"));
            var color18Value = entity.Get<string>(schema.GetField("PED_HTCD18_DESNIVEL_COLOR"));
            var color19Value = entity.Get<string>(schema.GetField("PED_HTCD19_DESNIVEL_COLOR"));
            var color20Value = entity.Get<string>(schema.GetField("PED_HTCD20_DESNIVEL_COLOR"));

            if (color1Value != "")
            {
                form.PED_HTCD1_DESNIVEL_COLOR.BackColor = System.Drawing.Color.FromArgb(Convert.ToInt32(color1Value.Substring(0, 3)), Convert.ToInt32(color1Value.Substring(3, 3)), Convert.ToInt32(color1Value.Substring(6, 3)));
            }
            if (color2Value != "")
            {
                form.PED_HTCD2_DESNIVEL_COLOR.BackColor = System.Drawing.Color.FromArgb(Convert.ToInt32(color2Value.Substring(0, 3)), Convert.ToInt32(color2Value.Substring(3, 3)), Convert.ToInt32(color2Value.Substring(6, 3)));
            }
            if (color3Value != "")
            {
                form.PED_HTCD3_DESNIVEL_COLOR.BackColor = System.Drawing.Color.FromArgb(Convert.ToInt32(color3Value.Substring(0, 3)), Convert.ToInt32(color3Value.Substring(3, 3)), Convert.ToInt32(color3Value.Substring(6, 3)));
            }
            if (color4Value != "")
            {
                form.PED_HTCD4_DESNIVEL_COLOR.BackColor = System.Drawing.Color.FromArgb(Convert.ToInt32(color4Value.Substring(0, 3)), Convert.ToInt32(color4Value.Substring(3, 3)), Convert.ToInt32(color4Value.Substring(6, 3)));
            }
            if (color5Value != "")
            {
                form.PED_HTCD5_DESNIVEL_COLOR.BackColor = System.Drawing.Color.FromArgb(Convert.ToInt32(color5Value.Substring(0, 3)), Convert.ToInt32(color5Value.Substring(3, 3)), Convert.ToInt32(color5Value.Substring(6, 3)));
            }
            if (color6Value != "")
            {
                form.PED_HTCD6_DESNIVEL_COLOR.BackColor = System.Drawing.Color.FromArgb(Convert.ToInt32(color6Value.Substring(0, 3)), Convert.ToInt32(color6Value.Substring(3, 3)), Convert.ToInt32(color6Value.Substring(6, 3)));
            }
            if (color7Value != "")
            {
                form.PED_HTCD7_DESNIVEL_COLOR.BackColor = System.Drawing.Color.FromArgb(Convert.ToInt32(color7Value.Substring(0, 3)), Convert.ToInt32(color7Value.Substring(3, 3)), Convert.ToInt32(color7Value.Substring(6, 3)));
            }
            if (color8Value != "")
            {
                form.PED_HTCD8_DESNIVEL_COLOR.BackColor = System.Drawing.Color.FromArgb(Convert.ToInt32(color8Value.Substring(0, 3)), Convert.ToInt32(color8Value.Substring(3, 3)), Convert.ToInt32(color8Value.Substring(6, 3)));
            }
            if (color9Value != "")
            {
                form.PED_HTCD9_DESNIVEL_COLOR.BackColor = System.Drawing.Color.FromArgb(Convert.ToInt32(color9Value.Substring(0, 3)), Convert.ToInt32(color9Value.Substring(3, 3)), Convert.ToInt32(color9Value.Substring(6, 3)));
            }
            if (color10Value != "")
            {
                form.PED_HTCD10_DESNIVEL_COLOR.BackColor = System.Drawing.Color.FromArgb(Convert.ToInt32(color10Value.Substring(0, 3)), Convert.ToInt32(color10Value.Substring(3, 3)), Convert.ToInt32(color10Value.Substring(6, 3)));
            }
            if (color11Value != "")
            {
                form.PED_HTCD11_DESNIVEL_COLOR.BackColor = System.Drawing.Color.FromArgb(Convert.ToInt32(color11Value.Substring(0, 3)), Convert.ToInt32(color11Value.Substring(3, 3)), Convert.ToInt32(color11Value.Substring(6, 3)));
            }
            if (color12Value != "")
            {
                form.PED_HTCD12_DESNIVEL_COLOR.BackColor = System.Drawing.Color.FromArgb(Convert.ToInt32(color12Value.Substring(0, 3)), Convert.ToInt32(color12Value.Substring(3, 3)), Convert.ToInt32(color12Value.Substring(6, 3)));
            }
            if (color13Value != "")
            {
                form.PED_HTCD13_DESNIVEL_COLOR.BackColor = System.Drawing.Color.FromArgb(Convert.ToInt32(color13Value.Substring(0, 3)), Convert.ToInt32(color13Value.Substring(3, 3)), Convert.ToInt32(color13Value.Substring(6, 3)));
            }
            if (color14Value != "")
            {
                form.PED_HTCD14_DESNIVEL_COLOR.BackColor = System.Drawing.Color.FromArgb(Convert.ToInt32(color14Value.Substring(0, 3)), Convert.ToInt32(color14Value.Substring(3, 3)), Convert.ToInt32(color14Value.Substring(6, 3)));
            }
            if (color15Value != "")
            {
                form.PED_HTCD15_DESNIVEL_COLOR.BackColor = System.Drawing.Color.FromArgb(Convert.ToInt32(color15Value.Substring(0, 3)), Convert.ToInt32(color15Value.Substring(3, 3)), Convert.ToInt32(color15Value.Substring(6, 3)));
            }
            if (color16Value != "")
            {
                form.PED_HTCD16_DESNIVEL_COLOR.BackColor = System.Drawing.Color.FromArgb(Convert.ToInt32(color16Value.Substring(0, 3)), Convert.ToInt32(color16Value.Substring(3, 3)), Convert.ToInt32(color16Value.Substring(6, 3)));
            }
            if (color17Value != "")
            {
                form.PED_HTCD17_DESNIVEL_COLOR.BackColor = System.Drawing.Color.FromArgb(Convert.ToInt32(color17Value.Substring(0, 3)), Convert.ToInt32(color17Value.Substring(3, 3)), Convert.ToInt32(color17Value.Substring(6, 3)));
            }
            if (color18Value != "")
            {
                form.PED_HTCD18_DESNIVEL_COLOR.BackColor = System.Drawing.Color.FromArgb(Convert.ToInt32(color18Value.Substring(0, 3)), Convert.ToInt32(color18Value.Substring(3, 3)), Convert.ToInt32(color18Value.Substring(6, 3)));
            }
            if (color19Value != "")
            {
                form.PED_HTCD19_DESNIVEL_COLOR.BackColor = System.Drawing.Color.FromArgb(Convert.ToInt32(color19Value.Substring(0, 3)), Convert.ToInt32(color19Value.Substring(3, 3)), Convert.ToInt32(color19Value.Substring(6, 3)));
            }
            if (color20Value != "")
            {
                form.PED_HTCD20_DESNIVEL_COLOR.BackColor = System.Drawing.Color.FromArgb(Convert.ToInt32(color20Value.Substring(0, 3)), Convert.ToInt32(color20Value.Substring(3, 3)), Convert.ToInt32(color20Value.Substring(6, 3)));
            }
            #endregion

            #region Eventos dos botões
            IExternalEventHandler handlerSalverDesniveisEvent = new SalvarDesniveis(form);
            ExternalEvent SalvarDesniveisEvent = ExternalEvent.Create(handlerSalverDesniveisEvent);


            IExternalEventHandler handlerResetarDesniveisEvent = new ResetarDesniveis(form);
            ExternalEvent ResetarDesniveisEvent = ExternalEvent.Create(handlerResetarDesniveisEvent);


            form.aplicarEvent = SalvarDesniveisEvent;
            form.resetarEvent = ResetarDesniveisEvent;
            #endregion

            Level level = doc.GetElement(levelID) as Level;
            Entity Levelentity = level.GetEntity(schema);


            if (Levelentity.IsValid())
            {
                //GET VALORES
                color1Value = Levelentity.Get<string>(schema.GetField("PED_HTCD1_DESNIVEL_COLOR"));
                color2Value = Levelentity.Get<string>(schema.GetField("PED_HTCD2_DESNIVEL_COLOR"));
                color3Value = Levelentity.Get<string>(schema.GetField("PED_HTCD3_DESNIVEL_COLOR"));
                color4Value = Levelentity.Get<string>(schema.GetField("PED_HTCD4_DESNIVEL_COLOR"));
                color5Value = Levelentity.Get<string>(schema.GetField("PED_HTCD5_DESNIVEL_COLOR"));
                color6Value = Levelentity.Get<string>(schema.GetField("PED_HTCD6_DESNIVEL_COLOR"));
                color7Value = Levelentity.Get<string>(schema.GetField("PED_HTCD7_DESNIVEL_COLOR"));
                color8Value = Levelentity.Get<string>(schema.GetField("PED_HTCD8_DESNIVEL_COLOR"));
                color9Value = Levelentity.Get<string>(schema.GetField("PED_HTCD9_DESNIVEL_COLOR"));
                color10Value = Levelentity.Get<string>(schema.GetField("PED_HTCD10_DESNIVEL_COLOR"));
                color11Value = Levelentity.Get<string>(schema.GetField("PED_HTCD11_DESNIVEL_COLOR"));
                color12Value = Levelentity.Get<string>(schema.GetField("PED_HTCD12_DESNIVEL_COLOR"));
                color13Value = Levelentity.Get<string>(schema.GetField("PED_HTCD13_DESNIVEL_COLOR"));
                color14Value = Levelentity.Get<string>(schema.GetField("PED_HTCD14_DESNIVEL_COLOR"));
                color15Value = Levelentity.Get<string>(schema.GetField("PED_HTCD15_DESNIVEL_COLOR"));
                color16Value = Levelentity.Get<string>(schema.GetField("PED_HTCD16_DESNIVEL_COLOR"));
                color17Value = Levelentity.Get<string>(schema.GetField("PED_HTCD17_DESNIVEL_COLOR"));
                color18Value = Levelentity.Get<string>(schema.GetField("PED_HTCD18_DESNIVEL_COLOR"));
                color19Value = Levelentity.Get<string>(schema.GetField("PED_HTCD19_DESNIVEL_COLOR"));
                color20Value = Levelentity.Get<string>(schema.GetField("PED_HTCD20_DESNIVEL_COLOR"));


                form.textBox1.Text = color1Value;
                form.textBox2.Text = color2Value;
                form.textBox3.Text = color3Value;
                form.textBox4.Text = color4Value;
                form.textBox5.Text = color5Value;
                form.textBox6.Text = color6Value;
                form.textBox7.Text = color7Value;
                form.textBox8.Text = color8Value;
                form.textBox9.Text = color9Value;
                form.textBox10.Text = color10Value;
                form.textBox11.Text = color11Value;
                form.textBox12.Text = color12Value;
                form.textBox13.Text = color13Value;
                form.textBox14.Text = color14Value;
                form.textBox15.Text = color15Value;
                form.textBox16.Text = color16Value;
                form.textBox17.Text = color17Value;
                form.textBox18.Text = color18Value;
                form.textBox19.Text = color19Value;
                form.textBox20.Text = color20Value;
            }
            

            form.Show();

            return Result.Succeeded;
        }
    }

    #region Função do botão salvar do forms
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    class SalvarDesniveis : IExternalEventHandler
    {
        ConfigLevelForm form;

        public SalvarDesniveis(ConfigLevelForm form)
        {
            this.form = form;
        }

        public void Execute(UIApplication app)
        {
            Document doc = app.ActiveUIDocument.Document;
            using (Transaction t = new Transaction(doc, "CameraTransaction"))
            {
                Guid schemaGUID = new Guid("58ffa6af-2458-4da8-8a7c-7d8beb619a29");
                Schema schema = Schema.Lookup(schemaGUID);

                ElementId levelId = Utils.GetLevelOfView(doc, doc.ActiveView);
                Element levelElem = doc.GetElement(levelId);

                Entity entity = levelElem.GetEntity(schema);
                if (!entity.IsValid())
                {
                    entity  = new Entity(schema);
                }

                var desniveislist = form.textBoxList;
                int index = 0;
                foreach (var item in desniveislist)
                {
                    Field fieldStoreCreator = schema.GetField("PED_HTCD"+(index+1).ToString()+ "_DESNIVEL_COLOR");
                    entity.Set<string>(fieldStoreCreator, item.Text);
                    index++;
                }

                t.Start();
                levelElem.SetEntity(entity);
                t.Commit();

            };
        }

        public string GetName()
        {
            return "AplicarCoresForm";
        }
    }
    #endregion

    #region Função do botão reset do forms
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    class ResetarDesniveis : IExternalEventHandler
    {
        ConfigLevelForm form;

        public ResetarDesniveis(ConfigLevelForm form)
        {
            this.form = form;
        }

        public void Execute(UIApplication app)
        {
            TaskDialog.Show("Debug", "Resetar");
            Document doc = app.ActiveUIDocument.Document;
            using (Transaction t = new Transaction(doc, "Resetar Formulario"))
            {
                var vigas  = new FilteredElementCollector(doc, doc.ActiveView.Id).WherePasses(new ElementCategoryFilter(BuiltInCategory.OST_StructuralFraming)).ToElements();
                var pisos  = new FilteredElementCollector(doc, doc.ActiveView.Id).WherePasses(new ElementCategoryFilter(BuiltInCategory.OST_Floors)).ToElements();
                var desniveisList = new List<double>();

                ElementId levelId = Utils.GetLevelOfView(doc, doc.ActiveView);
                Element levelElem = doc.GetElement(levelId);

                TaskDialog.Show("Debug", vigas.Count.ToString());
                TaskDialog.Show("Debug", pisos.Count.ToString());
                foreach (var piso in pisos)
                {
                    if (piso.LookupParameter("Titulo").AsString() != "")
                    {
                        ElementId elementlevel = piso.LookupParameter("Nível").AsElementId();
                        var desnivel = Math.Round(Utils.ConvertFromInternalUnits(doc, piso.LookupParameter("Altura do deslocamento do nível").AsDouble()),2);
                        if (elementlevel == levelId && desnivel != 0)
                        {
                            if (!desniveisList.Contains(desnivel))
                            {
                                desniveisList.Add(desnivel);
                            }

                        }
                        else
                        {
                            var elementlevelElem = doc.GetElement(elementlevel);
                            var desnivelTotal = Math.Round(Utils.ConvertFromInternalUnits(doc, elementlevelElem.LookupParameter("Elevação").AsDouble() - levelElem.LookupParameter("Elevação").AsDouble()), 2) + desnivel;
                            if (!desniveisList.Contains(desnivelTotal) && desnivelTotal != 0)
                            {
                                desniveisList.Add(desnivelTotal);
                            }
                        }
                    }
                }
                foreach (var viga in vigas)
                {
                    if (viga.LookupParameter("Titulo").AsString() != "")
                    {
                        //COLOCAR O MÉTODO PARA DESBLOQUEAR O NIVEL

                        if (viga.LookupParameter("Deslocamento do nível inicial") == null || viga.LookupParameter("Deslocamento do nível final") == null)
                        {
                            continue;
                        }
                        

                        

                        var desnivelInicial = Math.Round(Utils.ConvertFromInternalUnits(doc, viga.LookupParameter("Deslocamento do nível inicial").AsDouble()), 2);
                        var desnivelFinal = Math.Round(Utils.ConvertFromInternalUnits(doc, viga.LookupParameter("Deslocamento do nível final").AsDouble()), 2);
                        if (desnivelInicial != desnivelFinal)
                        {
                            continue;
                        }
                        
                        ElementId elementlevel = viga.LookupParameter("Nível de referência").AsElementId();
                        if (elementlevel == levelId && desnivelInicial != 0)
                        {
                            if (!desniveisList.Contains(desnivelInicial))
                            {
                                desniveisList.Add(desnivelInicial);
                            }

                        }
                        else
                        {
                            var elementlevelElem = doc.GetElement(elementlevel);
                            var desnivelTotal = Math.Round(Utils.ConvertFromInternalUnits(doc, elementlevelElem.LookupParameter("Elevação").AsDouble() - levelElem.LookupParameter("Elevação").AsDouble()), 2) + desnivelInicial;
                            if (!desniveisList.Contains(desnivelTotal) && desnivelTotal != 0)
                            {
                                desniveisList.Add(desnivelTotal);
                            }
                        }
                    }
                }

                if (desniveisList.Count > 20)
                {
                    TaskDialog.Show("Error", "Muitos desniveis no pavimento, especificar manualmente quais serão coloridos");
                }
                else
                {
                    desniveisList.Sort();
                    desniveisList.Reverse();
                    t.Start();
                    foreach( var item in form.textBoxList)
                    {
                        item.Text = "";
                    }
                    int index = 0;
                    foreach ( var item in desniveisList)
                    {
                        form.textBoxList[index].Text = item.ToString();
                        index++;
                    }
                    t.Commit();

                }

                
            };
        }

        public string GetName()
        {
            return "AplicarCoresForm";
        }
    }

    #endregion
    #endregion

}
