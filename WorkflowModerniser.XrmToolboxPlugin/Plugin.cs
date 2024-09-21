using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XrmToolBox.Extensibility.Interfaces;
using XrmToolBox.Extensibility;

namespace WorkflowModerniser.XrmToolboxPlugin
{
	// Do not forget to update version number and author (company attribute) in AssemblyInfo.cs class
	// To generate Base64 string for Images below, you can use https://www.base64-image.de/
	[Export(typeof(IXrmToolBoxPlugin)),
		ExportMetadata("Name", "Workflow-Moderniser"),
		ExportMetadata("Description", "Converts Classic Workflows to Low Code Plugins (PowerFx)"),
		// Please specify the base64 content of a 32x32 pixels image
		ExportMetadata("SmallImageBase64", null),
		// Please specify the base64 content of a 80x80 pixels image
		ExportMetadata("BigImageBase64", null),
		ExportMetadata("BackgroundColor", "Lavender"),
		ExportMetadata("PrimaryFontColor", "Black"),
		ExportMetadata("SecondaryFontColor", "Gray")]
	public class Plugin : PluginBase
	{
		public override IXrmToolBoxPluginControl GetControl()
		{
			return new PluginControl();
		}
	}
}
