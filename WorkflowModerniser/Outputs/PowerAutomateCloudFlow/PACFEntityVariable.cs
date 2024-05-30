using System.Collections.Generic;

namespace WorkflowModerniser.Outputs.PowerAutomateCloudFlow
{
	public class PACFEntityVariable : EntityVariable
	{

		public string RecordExpression { get; set; }
		public string IdExpression { get; set; }
		public Dictionary<string, string> ColumnExpressions { get; set; } = new Dictionary<string, string>();

		public PACFEntityVariable(string recordExpression, string idExpression)
		{
			RecordExpression = recordExpression;
			IdExpression = idExpression;
		}
	}
}