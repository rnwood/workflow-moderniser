using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using SoftCircuits.JavaScriptFormatter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WorkflowModerniser.Outputs.JavaScriptFormScript
{
	internal class JSFSEntityVariable : EntityVariable
	{
        public string EntityExpression { get; set; }
		public IDictionary<string, string> ColumnExpressions { get; internal set; } = new Dictionary<string, string>();
	}

	internal class JavascriptFormScriptWriter : IWorkflowWriter<JSFSEntityVariable>
	{
		private readonly WriterContext ctx;
		private readonly StringBuilder headerOutputs = new StringBuilder();
		private readonly StringBuilder applyRuleOutputs = new StringBuilder();

		public JavascriptFormScriptWriter(WriterContext ctx)
		{
			this.ctx = ctx;
		}

		public JSFSEntityVariable CloneEntityVariable(JSFSEntityVariable source)
		{
			throw new NotImplementedException();
		}

		public void CopyEntityId(JSFSEntityVariable source, JSFSEntityVariable target)
		{
			target.EntityExpression = source.EntityExpression;
		}

		public void CopyEntityVariableValues(JSFSEntityVariable source, JSFSEntityVariable target)
		{
			target.ColumnExpressions.Clear();
			foreach(var kvp in source.ColumnExpressions)
			{
				target.ColumnExpressions[kvp.Key] = kvp.Value;
			}
		}

		public string GetCoalesceExpression(string[] elements)
		{
			throw new NotImplementedException();
		}

		public string GetConcatExpression(string[] elements)
		{
			throw new NotImplementedException();
		}

		public string GetConditionExpression(ConditionOperator condition, string[] elements, string operand)
		{
			switch (condition)
			{
				case ConditionOperator.Equal:
					return $"{operand} === {elements[0]}";
				case ConditionOperator.NotEqual:
					return $"{operand} !== {elements[0]}";
				case ConditionOperator.Null:
                    return $"{operand} === null || {operand} === undefined";
                case ConditionOperator.NotNull:
                    return $"{operand} !== null && {operand} !== undefined";

                default:
					throw new NotImplementedException($"Condition '{condition}' is not implemented.");
			}
		}

		public string GetEntityPropertyExpresson(JSFSEntityVariable entity, string columnName)
		{
			headerOutputs.AppendLine($"{entity.EntityExpression}.getAttribute(\"{columnName}\").addOnChange(applyRules);");

			return $"{entity.EntityExpression}.getAttribute(\"{columnName}\").getValue()";
		}

		public string GetLiteral(object value)
		{
			if (value == null)
			{
				return "null";
			}
			else if (value is bool boolValue)
			{
				return boolValue.ToString().ToLower();
			}
			else if (value is string stringValue)
			{
				return string.Format("\"{0}\"", stringValue.Replace("\"", "\"\""));
			} else if (value is int intValue)
			{
				return intValue.ToString();
			} else if (value is OptionSetValue optionSetValue)
			{
				return optionSetValue.Value.ToString();
			} else if (value is Guid guidValue)
			{
				return $"\"{guidValue}\"";
			} else if (value is EntityReference entityReferenceValue)
			{
                return $"{{\"entityType\": \"{entityReferenceValue.LogicalName}\", \"id\":\"{entityReferenceValue.Id}\"}}";
            }
			else
			{
				throw new NotSupportedException($"Writing literal of type '{value.GetType()}' not supported");
			}
		}

		public string GetLogicalConditionExpression(LogicalOperator logicalOperator, string v1, string v2)
		{
			throw new NotImplementedException();
		}

		public IEnumerable<IOutput> GetOutputs()
		{
			StringBuilder js = new StringBuilder();
			js.AppendLine("function onLoad(executionContext) {");
			js.AppendLine("   const formContext = executionContext.getFormContext();");
			js.AppendLine(headerOutputs.ToString());
			js.AppendLine("applyRules(executionContext, true);");
            js.AppendLine();
			js.AppendLine("function applyRules(executionContext, isOnLoad){");
			js.AppendLine("   const formContext = executionContext.getFormContext();");
			js.AppendLine(applyRuleOutputs.ToString());
			js.AppendLine("}");
            js.AppendLine("}");

			string formattedJs = new JavaScriptFormatter().Format(js.ToString());

            yield return new JavaScriptFormScript.JavascriptFormScript(this.ctx.WorkflowName, formattedJs);
		}

		public JSFSEntityVariable LoadPrimaryEntity(string logicalName)
		{
			return new JSFSEntityVariable() { EntityExpression = "formContext.data.entity" };
		}

		public JSFSEntityVariable LoadRelatedEntity(JSFSEntityVariable entity, string entityName, string relationshipName)
		{
			throw new NotImplementedException();
		}

		public JSFSEntityVariable NewEntityVariable(string logicalName)
		{
			return new JSFSEntityVariable();
		}

		public void SetEntityProperty(JSFSEntityVariable entity, string columnName, string expression)
		{
			entity.ColumnExpressions[columnName] = expression;
		}

		public void WriteAssignRow(JSFSEntityVariable entity, string ownerExpresion)
		{
			throw new NotImplementedException();
		}

		public void WriteCallEntityAction(JSFSEntityVariable entity, string schemaName)
		{
			throw new NotImplementedException();
		}

		private int notificatonId = 1000;

		public void WriteClientRecommendation(JSFSEntityVariable entity, string controlIdExpression, string description, Dictionary<string, Action> writeActions)
		{
			int id = notificatonId++;



			applyRuleOutputs.AppendLine($"{entity.EntityExpression}.getControl('{controlIdExpression}').addNotification({{");
			applyRuleOutputs.AppendLine($"   messages: [{description}],");
			applyRuleOutputs.AppendLine("   notificationLevel: 'RECOMMENDATION',");
			applyRuleOutputs.AppendLine($"   uniqueId: '{id}',");
			applyRuleOutputs.AppendLine("   actions: [");

			foreach (var writeAction in writeActions) {
				applyRuleOutputs.AppendLine("      {");
				applyRuleOutputs.AppendLine($"      message: {writeAction.Key},");
				applyRuleOutputs.AppendLine("      actions [ function() {");
				writeAction.Value();
				applyRuleOutputs.AppendLine($"      {entity.EntityExpression}.getControl('{controlIdExpression}').clearNotification('{id}');");
				applyRuleOutputs.AppendLine("   },");
			}
			applyRuleOutputs.AppendLine("      ]") ;
			applyRuleOutputs.AppendLine("   }]");
			applyRuleOutputs.AppendLine("});");
		}

		public void WriteComment(string comment)
		{
			throw new NotImplementedException();
		}

		public void WriteCreateRow(JSFSEntityVariable entity)
		{
            throw new NotImplementedException();
        }

		public void WriteIf(string condition, Action writeThen, Action writeElse)
		{
			applyRuleOutputs.AppendLine($"if ({condition}) {{");
			writeThen?.Invoke();
			if (writeElse != null)
			{
				applyRuleOutputs.AppendLine("} else {");
				writeElse();
			}
			applyRuleOutputs.AppendLine("}");
		}

		public void WriteLine(string powerFx)
		{
			throw new NotImplementedException();
		}

		public void WriteSetClientEntityAttributeValues(JSFSEntityVariable entity)
		{
			foreach(var kvp in entity.ColumnExpressions)
			{
				this.applyRuleOutputs.AppendLine($"{entity.EntityExpression}.getAttribute('{kvp.Key}').setValue({kvp.Value});");
			}
		}

        public void WriteSetClientEntityDefaultValues(JSFSEntityVariable entity)
        {
			this.applyRuleOutputs.AppendLine("if (isOnLoad && executionContext.getFormContext().ui.getFormType() == 1) {");

            foreach (var kvp in entity.ColumnExpressions)
            {
                this.applyRuleOutputs.AppendLine($"{entity.EntityExpression}.getAttribute('{kvp.Key}').setValue({kvp.Value})");
            }

            this.applyRuleOutputs.AppendLine("}");
        }

        public void WriteSetDisplayMode(JSFSEntityVariable entity, string controlIdExpression, string isReadOnlyExpression)
		{
			applyRuleOutputs.AppendLine($"context.getControl({controlIdExpression}).setDisabled({isReadOnlyExpression});");
		}

        public void WriteSetVisibility(JSFSEntityVariable entity, string controlIdExpression, string isVisibleExpression)
        {
            applyRuleOutputs.AppendLine($"context.getControl({controlIdExpression}).setVisible({isVisibleExpression});");
        }

        public void WriteSetStatus(JSFSEntityVariable entity, int statecode, int statuscode)
		{
			throw new NotImplementedException();
		}

		public void WriteTerminate(OperationStatus operationStatus, string reason)
		{
			throw new NotImplementedException();
		}

		public void WriteUpdateRow(JSFSEntityVariable entity)
		{
			throw new NotImplementedException();
		}

		public void WriteVariableSet(string variableName, object value)
		{
			throw new NotImplementedException();
		}
	}
}
