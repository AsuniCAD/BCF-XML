using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace BCFXML
{
    /// <summary>
    /// XML Camera from BCF file
    /// </summary>
    public class XMLCamera
    {
        /// <summary>
        /// Location Point
        /// </summary>
        public Rhino.Geometry.Point3d Location;

        /// <summary>
        /// Camera Direction
        /// </summary>
        public Rhino.Geometry.Vector3d Direction;

        /// <summary>
        /// Up Vector
        /// </summary>
        public Rhino.Geometry.Vector3d UpVector;

        /// <summary>
        /// Create new XML Camera from XML node
        /// </summary>
        /// <param name="node"></param>
        /// <param name="scaleFactor"></param>
        public XMLCamera(XmlNode node, double scaleFactor)
        {
            this.Location = node.SelectSingleNode("CameraViewPoint").toPoint(scaleFactor);
            this.Direction = node.SelectSingleNode("CameraDirection").toVector();
            this.UpVector = node.SelectSingleNode("CameraUpVector").toVector();
        }
    }
}
