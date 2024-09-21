using Microsoft.Xrm.Sdk;
using System.Linq;
using System.Web.Services.Description;
using WorkflowModerniser.Data;

namespace WorkflowModerniser.Outputs.LowCodeCodePlugins
{
	public class InstantPlugin : PluginBase, ICustomActionOutput
	{
		public InstantPlugin(string name, string entityLogicalName,string expression) : base(name, entityLogicalName, expression)
		{
		}

		public void Ensure(IOrganizationService service, string solutionUniqueName)
		{
			var ctx = new DataverseContext(service);
			var existingFx = ctx.FxExpressionSet.Where(f => f.Name == Name).FirstOrDefault();

			if (existingFx == null)
			{
				service.Execute(new GenerateInstantPluginRequest
				{
					EntityLogicalName = this.EntityLogicalName,
					Expression = this.Expression,
					Name = this.Name,
					Description = this.Name,
					SolutionUniqueName = solutionUniqueName
				});
			}
			else
			{
				service.Execute(new UpdateInstantPluginRequest
				{
					EntityLogicalName = this.EntityLogicalName,
					Expression = this.Expression,
					Name = this.Name,
					Description = this.Name,
					FxExpressionUniqueName = existingFx.UniqueName,
					SolutionUniqueName = solutionUniqueName
				});
			}
		}
	}
}
