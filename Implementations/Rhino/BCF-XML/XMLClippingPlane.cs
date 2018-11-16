using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace BCFXML
{
    /// <summary>
    /// XML Clipping Plane from BCF file
    /// </summary>
    public class XMLClippingPlane
    {
        /// <summary>
        /// Location Point
        /// </summary>
        public Rhino.Geometry.Point3d Location;

        /// <summary>
        /// Direction
        /// </summary>
        public Rhino.Geometry.Vector3d Direction;

        /// <summary>
        /// Create new XML Clipping Plane from XML node
        /// </summary>
        /// <param name="node"></param>
        /// <param name="scaleFactor"></param>
        public XMLClippingPlane(XmlNode node, double scaleFactor)
        {
            this.Location = node.SelectSingleNode("Location").toPoint(scaleFactor);
            this.Direction = node.SelectSingleNode("Direction").toVector();
        }
    }
}
