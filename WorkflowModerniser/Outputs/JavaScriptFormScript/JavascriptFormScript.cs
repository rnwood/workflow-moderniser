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
        public string JavaScriptSource { get; set; }

		public string Name => throw new NotImplementedException();

		public void Ensure(IOrganizationService serviceClient, Data.Solution solution)
		{
			throw new NotImplementedException();
		}
	}
}
