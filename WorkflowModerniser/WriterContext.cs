using System;
using WorkflowModerniser.Inputs;

namespace WorkflowModerniser
{
	public class WriterContext
	{
		public bool IsPreOperation { get; private set; }
		public string PrimaryEntityName { get; private set; }

		public IMetadataService MetadataService { get; private set; }

		public WriterContext(string workflowName, bool isPreOperation, MessageName messageNames, string primaryEntityName, IMetadataService metadataService)
		{
			WorkflowName = workflowName;
			IsPreOperation = isPreOperation;
			MessageNames = messageNames;
			PrimaryEntityName = primaryEntityName;
			MetadataService = metadataService;
		}

		public MessageName MessageNames { get; set; }
		public string WorkflowName { get; set; }
	}


	[Flags]
	public enum MessageName
	{
		Create = 1, Update = 2, Delete = 4, Action = 8
	}
}
