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
using static WorkflowModerniser.WorkflowConverter;

namespace WorkflowModerniser
{
	internal class Program
	{
		static void Main(string[] args)
		{
			if (args.Length != 4)
			{
				Console.Error.WriteLine("Invalid arguments.");
				Console.Error.WriteLine("Usage:");
				Console.Error.WriteLine($"{Assembly.GetEntryAssembly().GetName().Name} <connectionstring> <workflowid> <outputtype> <solutionuniquename>");
				Console.Error.WriteLine("Valid output types: lowcodeplugin cloudflow formscript");
				Environment.Exit(1);
				return;
			}

			CrmServiceClient serviceClient = new CrmServiceClient(args[0]);

			string solutionUniqueName = args[3];


			WorkflowConverter.OutputType outputType;

			if (!Enum.TryParse(args[2], true, out outputType))
			{
				throw new Exception($"Unsupported output type '{args[2]}'");
			}


			IWorkflowConverter converter = WorkflowConverter.Create(serviceClient, outputType);

			IEnumerable<Outputs.IOutput> results = converter.
						Convert(new Guid(args[1]));

			string resultJson = JsonConvert.SerializeObject(results, Formatting.Indented);
			Console.WriteLine(resultJson);

			foreach (var result in results)
			{
				Console.Error.WriteLine($"Creating or updating output '{result.Name}' in solution '{solutionUniqueName}'");
				result.Ensure(serviceClient, solutionUniqueName);
			}

			Console.Error.WriteLine("Complete!");
		}
	}
}
