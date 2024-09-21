using Microsoft.Xrm.Sdk.Metadata;

namespace WorkflowModerniser.Inputs
{
	internal interface IMetadataService
	{
		EntityMetadata GetEntityMetadata(string entityName);
	}
}
