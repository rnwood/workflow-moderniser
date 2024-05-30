using WorkflowModerniser.Outputs;

namespace WorkflowModerniser
{
	public interface ICustomActionOutput : IOutput
	{
		string SchemaName { get; }
	}
}
