using System;
using System.Activities;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Crm.Workflow.ObjectModel
{
	public class StepLabel
	{
		public InArgument<string> LabelId { get; set; }
		public InArgument<string> Description { get; set; }
		public InArgument<string> LanguageCode { get; set; }
	}
}
