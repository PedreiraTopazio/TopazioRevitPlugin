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
            string topazioTab = "Topazio";
            List<string> topazioPanels = new List<string>();
            topazioPanels.Add("Visualização");
            
            //Creating Tab
            application.CreateRibbonTab(topazioTab);
            

            //Creating Panels
            foreach(string panel in topazioPanels)
            {
                application.CreateRibbonPanel(topazioTab, panel);
            }

            //Creating buttons
            string thisAssemblyPath = Assembly.GetExecutingAssembly().Location;

            RibbonPanel panel_Visualizacao = RibbonPanel(application, "Topazio", "Visualização");
            if(panel_Visualizacao.AddItem(new PushButtonData("Match Overrides", "Match Overrides", thisAssemblyPath, "TopazioRevitPluginShared.MatchOverrides")) is PushButton button)
            {
                button.ToolTip = "Esse comando iguala as sobreposições de elementos na vista";
                // Reflection of path to image 
                var globePath = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), "MatchOverrideGraphics.png"); //Mudar path depois para poder pegar outras imagens
                Uri uriImage = new Uri(globePath);
                // Apply image to bitmap
                BitmapImage largeImage = new BitmapImage(uriImage);
                // Apply image to button 
                button.LargeImage = largeImage;

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
