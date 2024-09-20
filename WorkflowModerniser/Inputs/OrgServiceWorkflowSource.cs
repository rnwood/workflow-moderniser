using Microsoft.Xrm.Sdk;
using System;
using System.Linq;
using WorkflowModerniser.Data;

namespace WorkflowModerniser.Inputs
{
	public class OrgServiceWorkflowSource : IWorkflowSource
	{
		private IOrganizationService organizationService;

		public OrgServiceWorkflowSource(IOrganizationService organizationService)
		{
			this.organizationService = organizationService;
		}

		public Solution GetSolution(string solutionUniqueName)
		{
			return new DataverseContext(organizationService).SolutionSet.FirstOrDefault(s => s.UniqueName == solutionUniqueName);

		}

		public Workflow GetWorkflow(Guid workflowId)
		{
			return new DataverseContext(organizationService).WorkflowSet.FirstOrDefault(w => w.Id == workflowId);
		}
	}

}
