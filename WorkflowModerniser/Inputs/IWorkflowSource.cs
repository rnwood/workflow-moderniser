using System;
using WorkflowModerniser.Data;

namespace WorkflowModerniser.Inputs
{
	internal interface IWorkflowSource
	{
		Solution GetSolution(string solutionUniqueName);
		IWorkflow GetWorkflow(Guid workflowId);

	}

}
