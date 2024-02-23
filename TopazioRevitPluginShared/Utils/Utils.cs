using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Text;

namespace TopazioRevitPluginShared
{
    internal class Utils
    {
        public static bool AreLinesPerpendicular(Line line1, Line line2)
        {
            // Obter os vetores diretores das linhas
            XYZ vector1 = line1.Direction;
            XYZ vector2 = line2.Direction;

            // Calcular o produto escalar dos vetores
            double dotProduct = vector1.DotProduct(vector2);

            // Se o produto escalar for aproximadamente zero, as linhas são perpendiculares
            // (usamos uma pequena tolerância para lidar com imprecisões numéricas)
            double tolerance = 1e-9; // Você pode ajustar a tolerância conforme necessário
            return Math.Abs(dotProduct) < tolerance;
        }

        public static ElementId GetLevelOfView(Document doc, View view)
        {
            var level = view.get_Parameter(BuiltInParameter.PLAN_VIEW_LEVEL).AsValueString();
            var allLevels = new FilteredElementCollector(doc).OfClass(typeof(Level)).ToElements();
            foreach (Level docLevel in allLevels)
            {
                
                if (docLevel.Name == level)
                {
                    return docLevel.Id;
                }
            }
            return null;
        }
        public static IList<CurveLoop> GetBottonFaceCurveLoop(Document doc, ElementId elementId)
        {
            // Get the element by its ElementId
            Element element = doc.GetElement(elementId);

            // Get the geometry of the element
            Options options = new Options();
            options.ComputeReferences = true;
            options.IncludeNonVisibleObjects = false;
            GeometryElement geometryElement = element.get_Geometry(options);

            // Extract faces from the geometry
            foreach (GeometryObject geomObj in geometryElement)
            {
                Solid solid = geomObj as Solid;
                if (solid != null)
                {
                    foreach (Face face in solid.Faces)
                    {
                        // Do something with each face
                        // For example, you can get the normal of the face
                        XYZ normal = face.ComputeNormal(new UV(0, 0));
                        if (normal.IsAlmostEqualTo(new XYZ(0,0,-1), 1))
                        {
                            // Get the edges of the face
                            EdgeArrayArray edgeArrayArray = face.EdgeLoops;

                            // Collect edges to form a CurveLoop
                            CurveLoop curveLoop = new CurveLoop();
                            foreach (EdgeArray edgeArray in edgeArrayArray)
                            {
                                foreach (Edge edge in edgeArray)
                                {
                                    // Get the curve of the edge and add it to the CurveLoop
                                    Curve curve = edge.AsCurve();
                                    curveLoop.Append(curve);
                                }
                            }

                            // Return the CurveLoop representing the perimeter of the face
                            return new List<CurveLoop>() { curveLoop };
                        }
                        return null;

                    }
                }
            }

            return null;

        }
    }
}
