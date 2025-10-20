using Microsoft.Crm.Workflow.ObjectModel;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Workflow.Activities;
using System;
using System.Activities;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkflowModerniser.SubstituteClientActivities
{
	public class SetMessage : CodeActivity
	{
		protected override void Execute(CodeActivityContext context)
		{
			throw new NotImplementedException();
		}


		public InArgument<string> ControlId { get; set; }

		public InArgument<string> Level { get; set; }
		public InArgument<string> EntityName
		{
			get; set;
		}

		public InArgument<Entity> Entity
		{
			get; set;
		}

		public ActivityReference Activities
		{
			get; set;
		}

		public StepLabel StepLabels
		{
			get; set;
		}

	}
}
