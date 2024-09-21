using Microsoft.Crm.Sdk.Messages;
using Microsoft.Rest;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Tooling.Connector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorkflowModerniser.Inputs;
using WorkflowModerniser.Outputs.JavaScriptFormScript;
using WorkflowModerniser.Outputs.LowCodeCodePlugins;
using WorkflowModerniser.Outputs.PowerAutomateCloudFlow;

namespace WorkflowModerniser
{
	public static class WorkflowConverter
	{
		public enum OutputType
		{
			LowCodePlugin,
			CloudFlow,
			FormScript
		}

		public static IWorkflowConverter Create(IOrganizationService organizationService, OutputType outputType)
		{
			MetadataService metadataService = new MetadataService(organizationService);

			IWorkflowSource workflowSource = new OrgServiceWorkflowSource(organizationService, metadataService);

			return Create(workflowSource, metadataService, outputType);
		}

		internal static IWorkflowConverter Create(IWorkflowSource workflowSource, IMetadataService metadataService, OutputType outputType)
		{
			switch (outputType)
			{
				case OutputType.LowCodePlugin:
					return new WorkflowConverterEngine<LCPEntityVariable>(workflowSource, metadataService, (ctx) => new LowCodePluginPowerFxWriter(ctx));

				case OutputType.CloudFlow:
					return new WorkflowConverterEngine<PACFEntityVariable>(workflowSource, metadataService, (ctx) => new PowerAutomateCloudFlowWriter());

				case OutputType.FormScript:
					return new WorkflowConverterEngine<JSFSEntityVariable>(workflowSource, metadataService, (ctx) => new JavascriptFormScriptWriter(ctx));

				default:
					throw new NotImplementedException($"Output type '{outputType}' is not implemented");
			}
		}
	}
}
