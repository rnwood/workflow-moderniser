using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorkflowModerniser.Outputs;

namespace WorkflowModerniser
{
	public interface IWorkflowConverter
	{
		IEnumerable<IOutput> Convert(Data.Workflow workflow);
	}
}
