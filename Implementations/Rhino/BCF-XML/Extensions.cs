using System.Xml;

namespace BCFXML
{
    /// <summary>
    /// Extension Class
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// Convert XML Node to Vector
        /// </summary>
        /// <param name="node"></param>
        /// <param name="scaleFactor"></param>
        /// <returns>Rhino vector</returns>
        public static Rhino.Geometry.Vector3d toVector(this XmlNode node, double scaleFactor = 1.0)
        {
            double X = double.Parse(node.SelectSingleNode("X").InnerText);
            double Y = double.Parse(node.SelectSingleNode("Y").InnerText);
            double Z = double.Parse(node.SelectSingleNode("Z").InnerText);
            return new Rhino.Geometry.Vector3d(X * scaleFactor, Y * scaleFactor, Z * scaleFactor);
        }

        /// <summary>
        /// Convert XML Node to Point
        /// </summary>
        /// <param name="node"></param>
        /// <param name="scaleFactor"></param>
        /// <returns>Rhino point</returns>
        public static Rhino.Geometry.Point3d toPoint(this XmlNode node, double scaleFactor = 1.0)
        {
            double X = double.Parse(node.SelectSingleNode("X").InnerText);
            double Y = double.Parse(node.SelectSingleNode("Y").InnerText);
            double Z = double.Parse(node.SelectSingleNode("Z").InnerText);
            return new Rhino.Geometry.Point3d(X * scaleFactor, Y * scaleFactor, Z * scaleFactor);
        }
    }
}
