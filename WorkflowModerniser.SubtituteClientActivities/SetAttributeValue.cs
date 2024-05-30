using Microsoft.Xrm.Sdk;
using System;
using System.Activities;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkflowModerniser.SubstituteClientActivities
{
	public class SetAttributeValue : CodeActivity
	{
		protected override void Execute(CodeActivityContext context)
		{
			throw new NotImplementedException();
		}

		public InArgument<string> ControlId { get; set; }
		public InArgument<string> ControlType
		{
			get; set;
		}

		public InArgument<string> EntityName
		{
			get; set;
		}

		public InArgument<Entity> Entity
		{
			get; set;
		}
	}
}
