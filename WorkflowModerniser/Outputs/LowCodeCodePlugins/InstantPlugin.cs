namespace WorkflowModerniser.Outputs.LowCodeCodePlugins
{
	public class InstantPlugin : ICustomActionOutput
	{
		public InstantPlugin(string name, string entityLogicalName, string messageName, string expression)
		{
			Name = name;
			EntityLogicalName = entityLogicalName;
			Expression = expression;
		}

		public string Name { get; set; }

		public string EntityLogicalName { get; set; }

		public string Expression { get; set; }
		public string SchemaName { get => $"fixme_{Name.Replace(" ", "_")}"; }
	}
}
