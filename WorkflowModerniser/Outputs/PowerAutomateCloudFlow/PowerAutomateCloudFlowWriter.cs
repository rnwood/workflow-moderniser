using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using WorkflowModerniser.Outputs;
using WorkflowModerniser.Outputs.PowerAutomateCloudFlow.Schema;

namespace WorkflowModerniser.Outputs.PowerAutomateCloudFlow
{
	public class PowerAutomateCloudFlowWriter : IWorkflowWriter<PACFEntityVariable>
	{
		public PowerAutomateCloudFlowWriter()
		{
			this.currentActionList = root.Definition.Actions;
		}

		public PACFEntityVariable CloneEntityVariable(PACFEntityVariable source)
		{
			throw new NotImplementedException();
		}

		public void CopyEntityId(PACFEntityVariable source, PACFEntityVariable target)
		{
			target.IdExpression = source.IdExpression;
		}

		public void CopyEntityVariableValues(PACFEntityVariable source, PACFEntityVariable target)
		{
			target.ColumnExpressions = new Dictionary<string, string>(source.ColumnExpressions);
		}

		public string GetCoalesceExpression(string[] elements)
		{
			throw new NotImplementedException();
		}

		public string GetConditionExpression(ConditionOperator condition, string[] elements, string operand)
		{
			throw new NotImplementedException();
		}

		public string GetEntityPropertyExpresson(PACFEntityVariable entity, string columnName)
		{
			throw new NotImplementedException();
		}

		public string GetLiteral(object value)
		{
			if (value == null)
			{
				return "null";
			}
			else if (value is bool boolValue)
			{
				return boolValue.ToString();
			}
			else if (value is Guid guidValue)
			{
				return string.Format("\'{0}\'", guidValue.ToString());
			}
			else if (value is string stringValue)
			{
				return string.Format("\'{0}\'", stringValue.Replace("'", "''"));
			}
			else if (value is OptionSetValue optionSetValue)
			{
				return optionSetValue.Value.ToString();
			}
			else if (value is EntityReference entityReference)
			{
				return string.Format("{0}s({1}", entityReference.LogicalName, entityReference.Id);
			}
			else
			{
				throw new NotSupportedException($"Writing literal value of type '{value.GetType().FullName}' is not supported");
			}
		}

		public PACFEntityVariable LoadPrimaryEntity(string logicalName)
		{
			return new PACFEntityVariable("triggerOutputs()?['body']", $"triggerOutputs()?['body']['{logicalName}id']");
		}

		public PACFEntityVariable LoadRelatedEntity(PACFEntityVariable entiy, string entityName, string relationshipName)
		{
			throw new NotImplementedException();
		}

		public PACFEntityVariable NewEntityVariable(string logicalName)
		{
			return new PACFEntityVariable(null, null);
		}

		public void SetEntityProperty(PACFEntityVariable entity, string columnName, string expression)
		{
			entity.ColumnExpressions[columnName] = expression;
		}

		private Root root = new Root()
		{
			Definition = new Definition
			{
				Actions = new Dictionary<string, Schema.Action>()
			}
		};

		private IDictionary<string, Schema.Action> currentActionList;
		private string lastActionName;
		private HashSet<string> allActionNames = new HashSet<string>();


		public override string ToString()
		{

			return root.ToJson();
		}

		private object ToJson(dynamic value)
		{
			return Convert.ToString(value);
		}

		public void WriteComment(string comment)
		{
			throw new NotImplementedException();
		}

		public void WriteCreateRow(PACFEntityVariable entity)
		{
			throw new NotImplementedException();
		}

		public void WriteIf(string condition, System.Action writeThen, System.Action writeElse)
		{
			throw new NotImplementedException();
		}

		public void WriteLine(string powerFx)
		{
			throw new NotImplementedException();
		}

		public void WriteSetStatus(PACFEntityVariable entity, int statecode, int statuscode)
		{
			throw new NotImplementedException();
		}

		public void WriteUpdateRow(PACFEntityVariable entity)
		{
			WriteAction();
		}

		private void WriteAction(string nameSeed = "Action")
		{
			string name = nameSeed;
			int index = 0;
			while (allActionNames.Contains(name))
			{
				name = nameSeed + ++index;
			}

			Schema.Action result = new Schema.Action
			{
				RunAfter = lastActionName != null ?
				new Dictionary<string, ICollection<RunAfterType>> { { lastActionName, new HashSet<RunAfterType>() { RunAfterType.Succeeded } } }
				: new Dictionary<string, ICollection<RunAfterType>>(),

				Metadata = new
				{
					operationMetadataId = Guid.NewGuid().ToString()
				},
				Type = Schema.Type.OpenApiConnection,
				Inputs = new
				{
					host = new
					{
						connectionName = "shared_commondataserviceforapps",
						operationId = "UpdateRecord",
						apiId = "/providers/Microsoft.PowerApps/apis/shared_commondataserviceforapps"

					},
					parameters = new
					{
						entityName = "accounts",
						recordId = "@triggerOutputs()?['body/accountid']",
						//"item/name": "x"
					},
					authentication = "@parameters('$authentication')"
				}
			};

			currentActionList[name] = result;
			allActionNames.Add(name);
			lastActionName = name;
		}

		public void WriteVariableSet(string variableName, object value)
		{
			throw new NotImplementedException();
		}

		public string GetLogicalConditionExpression(LogicalOperator logicalOperator, string v1, string v2)
		{
			throw new NotImplementedException();
		}

		public void WriteAssignRow(PACFEntityVariable entity, string ownerExpresion)
		{
			throw new NotImplementedException();
		}

		public void WriteTerminate(OperationStatus operationStatus, string reason)
		{
			throw new NotImplementedException();
		}

		public IEnumerable<IOutput> GetOutputs()
		{
			throw new NotImplementedException();
		}

		public void WriteCallEntityAction(PACFEntityVariable entity, string schemaName)
		{
			throw new NotImplementedException();
		}

		public string GetConcatExpression(string[] elements)
		{
			throw new NotImplementedException();
		}

		public void WriteSetDisplayMode(PACFEntityVariable entity, string controlIdExpression, string isReadOnlyExpression)
		{
			throw new NotImplementedException();
		}

		public void WriteSetClientEntityAttributeValues(PACFEntityVariable entity)
		{
			throw new NotImplementedException();
		}

		public void WriteClientRecommendation(PACFEntityVariable entity, string controlIdExpression, string description, Dictionary<string, System.Action> writeActions) 
		{
			throw new NotImplementedException();
		}
	}
}
