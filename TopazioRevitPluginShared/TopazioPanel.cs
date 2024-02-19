using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using Autodesk.Revit.UI;
using System.Windows.Media.Imaging;
using System.IO;
using System.Windows.Controls;

namespace TopazioRevitPluginShared
{
    class TopazioPanel : IExternalApplication
    {
        
        public Result OnShutdown(UIControlledApplication application)
        {
            return Result.Succeeded;
        }

        public Result OnStartup(UIControlledApplication application)
        {
            //Creating Tab
            string topazioTab = "Topazio";
            application.CreateRibbonTab(topazioTab);


            //Creating Panels
            List<string> topazioPanels = new List<string>();
            topazioPanels.Add("Visualização");
            topazioPanels.Add("Documentação");

            foreach (string panel in topazioPanels)
            {
                application.CreateRibbonPanel(topazioTab, panel);
            }

            //Creating buttons
            string thisAssemblyPath = Assembly.GetExecutingAssembly().Location;


            RibbonPanel panel_Visualizacao = RibbonPanel(application, "Topazio", "Visualização");
            if (panel_Visualizacao.AddItem(new PushButtonData("Match Overrides", "Match Overrides", thisAssemblyPath, "TopazioRevitPluginShared.MatchOverrides")) is PushButton MatchOverridesButton)
            {
                MatchOverridesButton.ToolTip = "Esse comando iguala as sobreposições de elementos na vista";
                // Reflection of path to image 
                var globePath = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), "MatchOverrideGraphics.png"); //Mudar path depois para poder pegar outras imagens
                Uri uriImage = new Uri(globePath);
                // Apply image to bitmap
                BitmapImage largeImage = new BitmapImage(uriImage);
                // Apply image to button 
                MatchOverridesButton.LargeImage = largeImage;

            }
            
            RibbonPanel panel_Documentacao = RibbonPanel(application, "Topazio", "Documentação");
            if(panel_Documentacao.AddItem(new PushButtonData("Auto Dimensions", "Auto Dimensions", thisAssemblyPath, "TopazioRevitPluginShared.AutoDimension")) is PushButton SemiAutomaticDimensionsButton)
            {
                SemiAutomaticDimensionsButton.ToolTip = "Esse comando cria novas cotas expecificando dois pontos de referencia e as vigas a serem cotadas";
                // Reflection of path to image 
                var globePath = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), "AutoDimension.png"); //Mudar path depois para poder pegar outras imagens
                Uri uriImage = new Uri(globePath);
                // Apply image to bitmap
                BitmapImage largeImage = new BitmapImage(uriImage);
                // Apply image to button 
                SemiAutomaticDimensionsButton.LargeImage = largeImage;

            }

            return Result.Succeeded;
        }

        public RibbonPanel RibbonPanel(UIControlledApplication application, string tab, string panelName)
        {

            RibbonPanel ribbonPanel = null;
            List<RibbonPanel> panels = application.GetRibbonPanels(tab);
            foreach (RibbonPanel panel in panels.Where(panel => panel.Name == panelName))
            {
                ribbonPanel = panel;
            }

            return ribbonPanel;
        }
    }
}
