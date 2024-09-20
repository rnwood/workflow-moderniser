using System;
using WorkflowModerniser.Data;

namespace WorkflowModerniser.Inputs
{
	public interface IWorkflowSource
	{
		Solution GetSolution(string solutionUniqueName);
		Workflow GetWorkflow(Guid workflowId);

	}

}
