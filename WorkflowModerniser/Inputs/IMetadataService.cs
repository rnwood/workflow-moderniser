using Microsoft.Xrm.Sdk.Metadata;

namespace WorkflowModerniser.Inputs
{
	public interface IMetadataService
	{
		EntityMetadata GetEntityMetadata(string entityName);
	}
}
