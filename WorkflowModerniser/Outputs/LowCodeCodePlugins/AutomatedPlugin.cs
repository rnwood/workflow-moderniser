using Microsoft.Xrm.Sdk;
using System.Linq;
using WorkflowModerniser.Data;
using WorkflowModerniser.Outputs;

namespace WorkflowModerniser.Outputs.LowCodeCodePlugins
{
	public class AutomatedPlugin : PluginBase, IOutput
	{
		public AutomatedPlugin(string name, string entityLogicalName, int stage, string messageName, string expression) : base(name, entityLogicalName, expression)
		{
			Stage = stage;
			MessageName = messageName;
		}

		public string MessageName { get; set; }

		public int Stage { get; set; }

		public void Ensure(IOrganizationService service, string solutionUniqueName)
		{
			var ctx = new DataverseContext(service);
			var existingFx = ctx.FxExpressionSet.Where(f => f.Name == Name).FirstOrDefault();

			if (existingFx == null)
			{
				service.Execute(new GenerateAutomatedPluginRequest
				{
					EntityLogicalName = this.EntityLogicalName,
					MessageName = this.MessageName,
					Expression = this.Expression,
					Name = this.Name,
					Stage = this.Stage,
					SolutionUniqueName = solutionUniqueName
				});
			} else
			{
				service.Execute(new UpdateAutomatedPluginRequest
				{
					EntityLogicalName = this.EntityLogicalName,
					MessageName = this.MessageName,
					Expression = this.Expression,
					Name = this.Name,
					Stage = this.Stage,
					FxExpressionUniqueName = existingFx.UniqueName,
					SolutionUniqueName = solutionUniqueName
				});
			}

		}
	}
}
