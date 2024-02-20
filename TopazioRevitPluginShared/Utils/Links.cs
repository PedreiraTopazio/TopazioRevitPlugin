using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace TopazioRevitPluginShared
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    internal class LinkedInLink : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            Process.Start("https://www.linkedin.com/company/pedreira-top%C3%A1zio/");
            return Result.Succeeded;
        }
    }
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    internal class InstagramLink : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            Process.Start("https://www.instagram.com/pedreiratopazio/");
            return Result.Succeeded;
        }
    }
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    internal class SiteLink : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            Process.Start("https://pedreiratopazio.eng.br/");
            return Result.Succeeded;
        }
    }
}
