using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkflowModerniser.Outputs.JavaScriptFormScript
{
	public class JavascriptFormScript : IOutput
	{
		public JavascriptFormScript(string name, string javaScriptSource)
		{
			this.Name = name;
			this.JavaScriptSource = javaScriptSource;
		}

        public string JavaScriptSource { get; private set; }

		public string Name { get; private set; }

		public void Ensure(IOrganizationService serviceClient, string solutionUniqueName)
		{
			throw new NotImplementedException();
		}
	}
}
