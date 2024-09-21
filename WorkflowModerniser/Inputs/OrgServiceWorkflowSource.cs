using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Metadata;
using System;
using System.Linq;
using WorkflowModerniser.Data;

namespace WorkflowModerniser.Inputs
{
	internal class OrgServiceWorkflowSource : IWorkflowSource
	{
		private readonly IOrganizationService organizationService;
		private readonly IMetadataService metadataService;

		public OrgServiceWorkflowSource(IOrganizationService organizationService, IMetadataService metadataService)
		{
			this.organizationService = organizationService;
			this.metadataService = metadataService;
		}

		public Solution GetSolution(string solutionUniqueName)
		{
			return new DataverseContext(organizationService).SolutionSet.FirstOrDefault(s => s.UniqueName == solutionUniqueName);

		}

		public IWorkflow GetWorkflow(Guid workflowId)
		{
			return new DataverseContext(organizationService).WorkflowSet.FirstOrDefault(w => w.Id == workflowId);
		}
	}

}
