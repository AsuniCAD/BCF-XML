using System;
using System.Collections.Generic;
using Rhino;
using Rhino.Commands;
using Rhino.Geometry;
using Rhino.Input;
using Rhino.Input.Custom;
using System.Xml;

namespace BCFXML
{
    public class RhinoBCFImportCommand : Command
    {
        public RhinoBCFImportCommand()
        {
            // Rhino only creates one instance of each command class defined in a
            // plug-in, so it is safe to store a refence in a static property.
            Instance = this;
        }

        ///<summary>The only instance of this command.</summary>
        public static RhinoBCFImportCommand Instance
        {
            get; private set;
        }

        ///<returns>The command name as it appears on the Rhino command line.</returns>
        public override string EnglishName
        {
            get { return "bcfImport"; }
        }

        /// <summary>
        /// Get XML Camera from VisInfo bcfv xml file
        /// </summary>
        /// <param name="filepath">bcfv xml file</param>
        /// <param name="scaleFactor"></param>
        /// <returns>Camera details as XMLCamera</returns>
        public static bool ParseVisInfo(string filepath, double scaleFactor, out XMLCamera cam, out List<XMLClippingPlane> planes)
        {
            planes = new List<XMLClippingPlane>();
            cam = null;

            // https://github.com/buildingSMART/BCF-XML/tree/release_2_1/Schemas
            try
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(filepath);
                XmlNode visinfo = doc.SelectSingleNode("VisualizationInfo");
                XmlNode camera = visinfo.SelectSingleNode("PerspectiveCamera");
                if (camera != null) cam = new XMLCamera(camera, scaleFactor);
                XmlNode cplanes = visinfo.SelectSingleNode("ClippingPlanes");
                if (cplanes != null) {
                    foreach (XmlNode cp in cplanes.ChildNodes)
                    {
                        planes.Add(new XMLClippingPlane(cp, scaleFactor));
                    }
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        protected override Result RunCommand(RhinoDoc doc, RunMode mode)
        {
            // 1) select BCF-Zip file
            var fd = new Rhino.UI.OpenFileDialog { Filter = "BCF Files (*.bcfzip;*.bcf)|*.bcfzip;*.bcf" };
            if (!fd.ShowOpenDialog())
                return Rhino.Commands.Result.Cancel;

            // 2) Extract all bcfv files from bcf
            List<string> bcfvs = ExtractZip.ExtractBCFVFiles(fd.FileName);
            foreach (string bcfv in bcfvs)
            {
                // 3) translate camera & clipping plane data from xml
                XMLCamera cam = null;
                List<XMLClippingPlane> planes = new List<XMLClippingPlane>();
                bool success = ParseVisInfo(bcfv, 1.0, out cam, out planes);
                if (!success) return Rhino.Commands.Result.Cancel;

                // 4) get active view
                var view = doc.Views.ActiveView;
                Rhino.Display.RhinoViewport vp = view.ActiveViewport;
                // save the current viewport projection
                vp.PushViewProjection();

                // 5) apply all camera settings to Rhino camera
                vp.CameraUp = cam.UpVector;
                vp.SetCameraLocation(cam.Location, false);
                vp.SetCameraDirection(cam.Direction, true);
                vp.Name = System.IO.Path.GetFileNameWithoutExtension(bcfv);
                doc.NamedViews.Add(vp.Name, vp.Id);

                foreach (XMLClippingPlane plane in planes)
                {
                    double magnitude = 100; // not sure if this value makes much sense
                    Plane cplane = new Plane(plane.Location, plane.Direction);
                    doc.Objects.AddClippingPlane(cplane, magnitude, magnitude, vp.Id);
                }

                view.Redraw();
            }

            return Result.Success;
        }
    }
}
