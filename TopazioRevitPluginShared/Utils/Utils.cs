using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
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
                        if (normal.IsAlmostEqualTo(new XYZ(0, 0, -1), 1))
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
        public static bool AreCurvesEqual(Curve curve1, Curve curve2)
        {
            //TaskDialog.Show("DEBUG", "COMPARANDO CURVAS");
            // Tolerance for comparing curve endpoints
            double tolerance = 0.01; // You can adjust this tolerance as needed

            // Check if the curves have the same start and end points within tolerance
            if (!curve1.GetEndPoint(0).IsAlmostEqualTo(curve2.GetEndPoint(0), tolerance) ||
                !curve1.GetEndPoint(1).IsAlmostEqualTo(curve2.GetEndPoint(1), tolerance))
            {
                //TaskDialog.Show("DEBUG", "CURVAS NÃO SÃO IGUAIS");
                return false;
            }

            // Check if the curves have approximately the same length
            double length1 = curve1.ApproximateLength;
            double length2 = curve2.ApproximateLength;

            // Tolerance for comparing curve lengths
            double lengthTolerance = Math.Max(length1, length2) * tolerance;

            if (Math.Abs(length1 - length2) > lengthTolerance)
            {
                return false;
            }

            // Curves are considered equal
            return true;
        }

        public static bool AreCurveLoopsEqual(List<CurveLoop> curveLoop1, List<CurveLoop> curveLoop2)
        {
            // Check if the lists have the same number of curve loops
            if (curveLoop1.Count() != curveLoop2.Count())
            {
                return false;
            }

            // Iterate through each curve loop in both lists
            for (int i = 0; i < curveLoop1.Count; i++)
            {
                CurveLoop loop1 = curveLoop1[i];
                CurveLoop loop2 = curveLoop2[i];

                // Check if the current curve loops have the same number of curves
                if (loop1.Count() != loop2.Count())
                {
                    //TaskDialog.Show("DEBUG", loop1.Count().ToString() + " " + loop2.Count().ToString());
                    return false;
                }

                // Flag to track if any corresponding curves are not equal
                bool curvesEqual = true;

                // Iterate through each curve in both curve loops
                foreach (Curve curve1 in loop1)
                {
                    bool curveFound = false;

                    foreach (Curve curve2 in loop2)
                    {
                        // Check if the curves are equal
                        if (AreCurvesEqual(curve1, curve2))
                        {
                            curveFound = true;
                            break;
                        }
                    }

                    // If the current curve from loop1 is not found in loop2, return false
                    if (!curveFound)
                    {
                        curvesEqual = false;
                        break;
                    }
                }

                // If any corresponding curves are not equal, return false
                if (!curvesEqual)
                {
                    return false;
                }
            }

            // All curve loops and curves are equal
            //TaskDialog.Show("DEBUG", "SÃO IGUAIS");
            return true;
        }

    }
}
