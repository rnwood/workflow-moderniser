using System.Collections.Generic;

namespace WorkflowModerniser.Outputs.LowCodeCodePlugins
{
	internal class LCPEntityVariable : EntityVariable
	{
		public LCPEntityVariable(string tableName, string recordExpression)
		{
			this.TableName = tableName;
			this.RecordExpression = recordExpression;
		}

		public bool IsPrimary { get; set; }

		public string TableName { get; private set; }

		public string IdExpression { get; set; }

		public Dictionary<string, string> ColumnExpressions { get; set; } = new Dictionary<string, string>();
		public string RecordExpression { get; private set; }
	}
}
