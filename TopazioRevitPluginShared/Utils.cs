using Autodesk.Revit.DB;
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
    }
}
