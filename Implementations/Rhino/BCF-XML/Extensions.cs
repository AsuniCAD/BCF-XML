using System.Xml;
using System.Globalization;

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
        public static Rhino.Geometry.Vector3d toVector(this Direction node, double scaleFactor = 1.0)
        {
            return new Rhino.Geometry.Vector3d(node.X * scaleFactor, node.Y * scaleFactor, node.Z * scaleFactor);
        }

        /// <summary>
        /// Convert XML Node to Point
        /// </summary>
        /// <param name="node"></param>
        /// <param name="scaleFactor"></param>
        /// <returns>Rhino point</returns>
        public static Rhino.Geometry.Point3d toPoint(this Point node, double scaleFactor = 1.0)
        {
            return new Rhino.Geometry.Point3d(node.X * scaleFactor, node.Y * scaleFactor, node.Z * scaleFactor);
        }

        public static System.Drawing.Point toScreenPoint(this Point node)
        {
            return new System.Drawing.Point((int)node.X , (int)node.Y);
        }
    }
}
