using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Extensions;
using Microsoft.Xrm.Sdk.Metadata;
using System.Collections.Generic;

namespace WorkflowModerniser.Inputs
{

	public class MetadataService : IMetadataService
	{
		private readonly IOrganizationService service;

		public MetadataService(IOrganizationService service)
		{
			this.service = service;
		}

		public EntityMetadata GetEntityMetadata(string entityName)
		{
			if (!EntityMetadataCache.TryGetValue(entityName, out EntityMetadata result))
			{
				result = service.GetEntityMetadata(entityName);
				EntityMetadataCache[entityName] = result;
			}

			return result;
		}

		Dictionary<string, EntityMetadata> EntityMetadataCache = new Dictionary<string, EntityMetadata>();
	}
}
