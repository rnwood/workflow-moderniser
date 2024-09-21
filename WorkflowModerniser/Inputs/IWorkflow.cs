using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorkflowModerniser.Data;

namespace WorkflowModerniser.Inputs
{
	public interface IWorkflow
	{
		string PrimaryEntity { get; }
		string XAMl { get; }
		string Name { get; }
		bool? Subprocess { get; }
		Workflow_Stage? CreateStage { get; }
		Workflow_Stage? UpdateStage { get; }
		Workflow_Stage? DeleteStage { get; }
		Workflow_Category? Category { get; }
	}
}
