using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Extensions;
using Microsoft.Xrm.Sdk.Metadata;
using System.Collections.Generic;

namespace WorkflowModerniser.Inputs
{

	internal class MetadataService : IMetadataService
	{
		private readonly IOrganizationService service;

		public MetadataService(IOrganizationService service)
		{
			this.service = service;
		}

		public EntityMetadata GetEntityMetadata(string entityName)
		{
			if (!entityMetadataCache.TryGetValue(entityName, out EntityMetadata result))
			{
				result = service.GetEntityMetadata(entityName);
				entityMetadataCache[entityName] = result;
			}

			return result;
		}

		readonly Dictionary<string, EntityMetadata> entityMetadataCache = new Dictionary<string, EntityMetadata>();
	}
}
