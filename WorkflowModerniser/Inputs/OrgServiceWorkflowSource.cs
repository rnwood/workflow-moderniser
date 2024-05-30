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

		public Workflow GetWorkflow(Guid workflowId)
		{
			return new DataverseContext(organizationService).WorkflowSet.FirstOrDefault(w => w.Id == workflowId);
		}
	}

}
