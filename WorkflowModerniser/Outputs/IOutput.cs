using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Tooling.Connector;

namespace WorkflowModerniser.Outputs
{
	public interface IOutput
	{
		string Name { get; }

		void Ensure(IOrganizationService serviceClient, string solutionUniqueName);
	}
}
