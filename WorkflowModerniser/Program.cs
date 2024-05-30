using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Tooling.Connector;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using WorkflowModerniser.Data;
using WorkflowModerniser.Inputs;
using WorkflowModerniser.Outputs.JavaScriptFormScript;
using WorkflowModerniser.Outputs.LowCodeCodePlugins;
using WorkflowModerniser.Outputs.PowerAutomateCloudFlow;

namespace WorkflowModerniser
{
	internal class Program
	{
		static void Main(string[] args)
		{
			if (args.Length != 3)
			{
				Console.Error.WriteLine("Invalid arguments.");
				Console.Error.WriteLine("Usage:");
				Console.Error.WriteLine($"{Assembly.GetEntryAssembly().GetName().Name} <connectionstring> <workflowid> <outputtype>");
				Console.Error.WriteLine("Valid output types: lowcodeplugin cloudflow formscript");
				Environment.Exit(1);
				return;
			}

			CrmServiceClient serviceClient = new CrmServiceClient(args[0]);

			IWorkflowSource workflowSource = new OrgServiceWorkflowSource(serviceClient);
			Workflow workflow = workflowSource.GetWorkflow(new Guid(args[1]));

			MetadataService metadataService = new MetadataService(serviceClient);

			IWorkflowConverter converter;

			switch (args[2])
			{
				case "lowcodeplugin":
					converter = new WorkflowConverter<LCPEntityVariable>(workflowSource, metadataService, (ctx) => new LowCodePluginPowerFxWriter(ctx));
					break;

				case "cloudflow":
					converter = new WorkflowConverter<PACFEntityVariable>(workflowSource, metadataService, (ctx) => new PowerAutomateCloudFlowWriter());
					break;

				case "formscript":
					converter = new WorkflowConverter<JSFSEntityVariable>(workflowSource, metadataService, (ctx) => new JavascriptFormScriptWriter(ctx));
					break;
				default:
					throw new Exception($"Unsupported output type '{args[2]}'");
			}

			IEnumerable<Outputs.IOutput> results = converter.
						Convert(workflow);

			string resultJson = JsonConvert.SerializeObject(results, Formatting.Indented);
			Console.WriteLine(resultJson);

		}
	}
}
