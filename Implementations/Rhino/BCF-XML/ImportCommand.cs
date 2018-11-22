using System;
using System.Collections.Generic;
using Rhino;
using Rhino.Commands;
using Rhino.Geometry;
using Rhino.Input;
using Rhino.Input.Custom;
using System.Xml;
using System;
using System.Xml.Serialization;
using System.Collections.Generic;

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
        public static VisualizationInfo ParseVisInfo(string filepath, double scaleFactor)
        {
            try
            {
                string d = System.IO.File.ReadAllText(filepath);
                XmlSerializer serializer = new XmlSerializer(typeof(VisualizationInfo));
                VisualizationInfo result = null;
                using (System.IO.TextReader reader = new System.IO.StringReader(d))
                {
                    result = (VisualizationInfo) serializer.Deserialize(reader);
                }
                return result;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// returns a scalefactor for common AEC units
        /// </summary>
        /// <param name="doc"></param>
        /// <returns></returns>
        private static double GetScaleFactor(RhinoDoc doc)
        {
            // All BCF Viewpoint data is in meters by default
            switch (doc.ModelUnitSystem)
            {
                // Metric
                case UnitSystem.Millimeters: return 1000;
                case UnitSystem.Centimeters: return 100;
                case UnitSystem.Decimeters: return 10;
                case UnitSystem.Meters: return 1.0;

                // Imperial
                case UnitSystem.Miles: return 0.000621371;
                case UnitSystem.Feet: return 3.28084;
                case UnitSystem.Inches: return 39.3701;

                default: return 1;
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
                double scaleFactor = GetScaleFactor(doc);
                VisualizationInfo vinfo = ParseVisInfo(bcfv, scaleFactor);
                if (vinfo == null) continue;

                // 4) get active view
                var view = doc.Views.ActiveView;
                Rhino.Display.RhinoViewport vp = view.ActiveViewport;
                // save the current viewport projection
                vp.PushViewProjection();

                if (vinfo.PerspectiveCamera != null)
                {
                    vp.CameraUp = vinfo.PerspectiveCamera.CameraUpVector.toVector();
                    vp.SetCameraLocation(vinfo.PerspectiveCamera.CameraViewPoint.toPoint(scaleFactor), false);
                    vp.SetCameraDirection(vinfo.PerspectiveCamera.CameraDirection.toVector(), true);
                    vp.Name = System.IO.Path.GetFileNameWithoutExtension(bcfv);
                    doc.NamedViews.Add(vp.Name, vp.Id);
                }

                if (vinfo.OrthogonalCamera != null)
                {
                    vp.CameraUp = vinfo.OrthogonalCamera.CameraUpVector.toVector();
                    vp.SetCameraLocation(vinfo.OrthogonalCamera.CameraViewPoint.toPoint(scaleFactor), false);
                    vp.SetCameraDirection(vinfo.OrthogonalCamera.CameraDirection.toVector(), true);
                    vp.ChangeToParallelProjection(true);
                    vp.Name = System.IO.Path.GetFileNameWithoutExtension(bcfv);
                    doc.NamedViews.Add(vp.Name, vp.Id);
                }

                if (vinfo.ClippingPlanes != null)
                {
                    foreach (ClippingPlane plane in vinfo.ClippingPlanes)
                    {
                        double magnitude = 100; // not sure if this value makes much sense
                        Plane cplane = new Plane(plane.Location.toPoint(scaleFactor), plane.Direction.toVector());
                        doc.Objects.AddClippingPlane(cplane, magnitude, magnitude, vp.Id);
                    }
                }
                
                bcfConduit = new BCFConduit() { Visinfo = vinfo, Enabled = true };
                
                view.Redraw();

            }

            Rhino.Display.RhinoView.SetActive += RhinoView_SetActive;

            return Result.Success;
        }

        /// <summary>
        /// BCF Viewport conduit
        /// </summary>
        BCFConduit bcfConduit;

        /// <summary>
        /// Toggle BCF Conduit on and off depeding on the active viewport
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RhinoView_SetActive(object sender, Rhino.Display.ViewEventArgs e)
        {
            if (e.View.ActiveViewport.Name.ToLower().Contains("viewpoint"))
            { bcfConduit = new BCFConduit() { Enabled = true }; }
            else
            {
                bcfConduit.Enabled = false;
                bcfConduit = null;
            }
             e.View.Document.Views.Redraw();
        }
    }

    /// <summary>
    /// BCF Conduit
    /// </summary>
    class BCFConduit : Rhino.Display.DisplayConduit
    {
        public VisualizationInfo Visinfo;

        public System.Drawing.Color Color = System.Drawing.Color.Red;

        public float Thickness = 1;

        protected override void DrawForeground(Rhino.Display.DrawEventArgs e)
        {
            // Draw only on top of BCF Viewpoints
            if (e.Viewport.Name.ToLower().Contains("viewpoint"))
            {                
                var bounds = e.Viewport.Bounds;
                var pt = new Rhino.Geometry.Point2d(bounds.Left + 2, bounds.Bottom - 14);
                e.Display.Draw2dRectangle(new System.Drawing.Rectangle(bounds.Left, bounds.Bottom - 20, bounds.Width, 30), System.Drawing.Color.White, 0, System.Drawing.Color.White);
                e.Display.Draw2dText(Properties.Resources.DisplayTitle, System.Drawing.Color.Black, pt, false, 12);
                if (Visinfo != null && Visinfo.Lines != null)
                {
                    foreach (Line l in Visinfo.Lines)
                    {
                        e.Display.Draw2dLine(l.StartPoint.toScreenPoint(), l.EndPoint.toScreenPoint(), Color, Thickness);
                    }
                }
            }
        }
    }


}
