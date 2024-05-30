using System;
using WorkflowModerniser.Data;

namespace WorkflowModerniser.Inputs
{
	public interface IWorkflowSource
	{
		Workflow GetWorkflow(Guid workflowId);

	}

}
