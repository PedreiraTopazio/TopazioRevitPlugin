using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.ExtensibleStorage;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using TopazioRevitPlugin.TopazioPanel.Documentação;

namespace TopazioRevitPluginShared
{


    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    internal class DropButton : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            return Result.Succeeded;
        }
    }
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

            //TaskDialog.Show("DEBUG", "Level ID: " + levelID.ToString());

            //PROCURANDO O SCHEMA
            Guid schemaGUID = new Guid("58ffa6af-2458-4da8-8a7c-7d8beb619a29");
            Schema schema = Schema.Lookup(schemaGUID);

            //SE NÃO TIVER ESQUEMA, CRIAR ESQUEMA
            if (schema == null)
            {
                TaskDialog.Show("Error", "Definir cores no modelo primeiro.");
                return Result.Failed;
            }


            ConfigLevelForm form = new ConfigLevelForm(this);

            IExternalEventHandler handlerSalverDesniveisEvent = new SalvarDesniveis(form);
            ExternalEvent SalvarDesniveisEvent = ExternalEvent.Create(handlerSalverDesniveisEvent);


            IExternalEventHandler handlerResetarDesniveisEvent = new ResetarDesniveis(form);
            ExternalEvent ResetarDesniveisEvent = ExternalEvent.Create(handlerResetarDesniveisEvent);


            form.aplicarEvent = SalvarDesniveisEvent;
            form.resetarEvent = ResetarDesniveisEvent;


            Level level = doc.GetElement(levelID) as Level;
            Entity entity = level.GetEntity(schema);


            if (entity.IsValid())
            {
                //GET VALORES
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
                TaskDialog.Show("Debug", "Salvando");
                Guid schemaGUID = new Guid("58ffa6af-2458-4da8-8a7c-7d8beb619a29");
                Schema schema = Schema.Lookup(schemaGUID);

                ElementId levelId = Utils.GetLevelOfView(doc, doc.ActiveView);
                Element levelElem = doc.GetElement(levelId);

                Entity entity = levelElem.GetEntity(schema);
                if (!entity.IsValid())
                {
                    TaskDialog.Show("DEBUG", "Entrei");
                    entity  = new Entity(schema);
                    TaskDialog.Show("DEBUG", "Criei");
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
            Document doc = app.ActiveUIDocument.Document;
            using (Transaction t = new Transaction(doc, "CameraTransaction"))
            {
                var vigas  = new FilteredElementCollector(doc, doc.ActiveView.Id).WherePasses(new ElementCategoryFilter(BuiltInCategory.OST_StructuralFraming)).ToElements();
                var pisos  = new FilteredElementCollector(doc, doc.ActiveView.Id).WherePasses(new ElementCategoryFilter(BuiltInCategory.OST_Floors)).ToElements();
                var desniveisList = new List<double>();

                ElementId levelId = Utils.GetLevelOfView(doc, doc.ActiveView);
                Element levelElem = doc.GetElement(levelId);
                foreach (var piso in pisos)
                {
                    if (piso.LookupParameter("Titulo").AsString() != "")
                    {
                        ElementId elementlevel = piso.LookupParameter("Nível").AsElementId();
                        var desnivel = Math.Round(Utils.ConvertFromInternalUnits(doc, piso.LookupParameter("Altura do deslocamento do nível").AsDouble()),2);

                        if (elementlevel == levelId)
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
                            if (!desniveisList.Contains(desnivelTotal))
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
                        if (elementlevel == levelId)
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
                            if (!desniveisList.Contains(desnivelTotal))
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
                    int index = 0;
                    foreach( var item in desniveisList)
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


}
