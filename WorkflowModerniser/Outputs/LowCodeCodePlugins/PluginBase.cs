using Microsoft.Xrm.Sdk;
using System.Linq;
using WorkflowModerniser.Data;

namespace WorkflowModerniser.Outputs.LowCodeCodePlugins
{
	public class PluginBase
	{
		public PluginBase(string schemaName, string name, string entityLogicalName, string expression)
		{
			EntityLogicalName = entityLogicalName;
			Expression = expression;
			Name = name;
			SchemaName = schemaName;
		}

		public string EntityLogicalName { get; set; }

		public string Expression { get; set; }

		public string Name { get; set; }

		public string SchemaName { get; set; }


	}
}