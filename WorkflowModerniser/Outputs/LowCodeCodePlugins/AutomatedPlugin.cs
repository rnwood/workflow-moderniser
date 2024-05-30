using WorkflowModerniser.Outputs;

namespace WorkflowModerniser.Outputs.LowCodeCodePlugins
{
	public class AutomatedPlugin : IOutput
	{
		public AutomatedPlugin(string name, string entityLogicalName, int stage, string messageName, string expression)
		{
			Name = name;
			EntityLogicalName = entityLogicalName;
			Stage = stage;
			MessageName = messageName;
			Expression = expression;
		}

		public string Name { get; set; }

		public string EntityLogicalName { get; set; }

		public string MessageName { get; set; }

		public string Expression { get; set; }

		public int Stage { get; set; }
	}
}
