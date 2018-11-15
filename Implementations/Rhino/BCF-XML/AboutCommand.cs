using System;
using System.Collections.Generic;
using Rhino;
using Rhino.Commands;
using Rhino.Geometry;
using Rhino.Input;
using Rhino.Input.Custom;

namespace BCFXML
{
	public class AboutCommand : Command
	{
		///<returns>The command name as it appears on the Rhino command line.</returns>
		public override string EnglishName => "bcfAbout";

		public AboutCommand()
		{
			// Rhino only creates one instance of each command class defined in a
			// plug-in, so it is safe to store a refence in a static property.
			Instance = this;
		}

		///<summary>The only instance of this command.</summary>
		public static AboutCommand Instance
		{
			get; private set;
		}

		protected override Result RunCommand(RhinoDoc doc, RunMode mode)
		{
			using (System.Diagnostics.Process.Start("https://github.com/AsuniCAD/BCF-XML/tree/development/Implementations/Rhino")) { }

			return Result.Success;
		}
	}
}
