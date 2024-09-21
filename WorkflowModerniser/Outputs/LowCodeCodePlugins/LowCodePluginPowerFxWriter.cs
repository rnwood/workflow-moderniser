using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Metadata;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using WorkflowModerniser.Inputs;
using WorkflowModerniser.Outputs;

namespace WorkflowModerniser.Outputs.LowCodeCodePlugins
{


	internal class LowCodePluginPowerFxWriter : IWorkflowWriter<LCPEntityVariable>
	{
		public LowCodePluginPowerFxWriter(WriterContext writerContext)
		{
			this.metadataService = writerContext.MetadataService;
			this.context = writerContext;
		}

		private IMetadataService metadataService;
		private readonly WriterContext context;
		private StringWriter output = new StringWriter();
		private StringWriter outputAfter = new StringWriter();

		public LCPEntityVariable LoadPrimaryEntity(string logicalName)
		{
			return new LCPEntityVariable(logicalName, "ThisRecord")
			{
				IdExpression = $"ThisRecord.{GetPrimaryKeyIdentifier(logicalName)}",
				IsPrimary = true
			};
		}

		public LCPEntityVariable NewEntityVariable(string logicalName)
		{
			return new LCPEntityVariable(logicalName, null);
		}

		public LCPEntityVariable LoadRelatedEntity(LCPEntityVariable entity, string entityName, string relationshipName)
		{
			EntityMetadata entityMeta = metadataService.GetEntityMetadata(entity.TableName);
			LookupAttributeMetadata attrMeta = (LookupAttributeMetadata)entityMeta.Attributes.FirstOrDefault(a => a.LogicalName == relationshipName);
			OneToManyRelationshipMetadata relnMeta = entityMeta.ManyToOneRelationships.FirstOrDefault(r => r.ReferencingAttribute == relationshipName & r.ReferencedEntity == entityName);


			string recordExpresion;
			if (attrMeta.Targets.Count() > 1)
			{
				recordExpresion = $"AsType({entity.RecordExpression}.'{attrMeta.DisplayName.UserLocalizedLabel.Label}', {entityName})";
			}
			else
			{
				recordExpresion = $"{entity.RecordExpression}.'{attrMeta.DisplayName.UserLocalizedLabel.Label}'";
			}

			return new LCPEntityVariable(entityName, recordExpresion) { IdExpression = $"{recordExpresion}.{GetPrimaryKeyIdentifier(entityName)}" };

		}

		public void WriteComment(string comment)
		{
			output.WriteLine("// {0}", comment);
		}

		public void WriteVariableSet(string variableName, object value)
		{
			output.WriteLine("With(");
			output.WriteLine("{");
			output.Write(GetIdentifier(variableName));
			output.Write(": ");
			output.Write(GetLiteral(value));
			output.WriteLine();
			output.WriteLine("},");
			outputAfter.WriteLine(");");
		}

		public string GetEntityPropertyExpresson(LCPEntityVariable entity, string columnName)
		{
			EntityMetadata entityMetadata = metadataService.GetEntityMetadata(entity.TableName);
			AttributeMetadata attrMeta = entityMetadata.Attributes.First(a => a.LogicalName == columnName);

			return string.Format("{0}.{1}", entity.RecordExpression, GetIdentifier(attrMeta.DisplayName.UserLocalizedLabel.Label));
		}

		public void CopyEntityId(LCPEntityVariable source, LCPEntityVariable target)
		{
			target.IsPrimary = source.IsPrimary;
			target.IdExpression = source.IdExpression;
		}

		public LCPEntityVariable CloneEntityVariable(LCPEntityVariable source)
		{
			return new LCPEntityVariable(source.TableName, null)
			{
				ColumnExpressions = source.ColumnExpressions
			};
		}

		public void CopyEntityVariableValues(LCPEntityVariable source, LCPEntityVariable target)
		{
			target.ColumnExpressions = source.ColumnExpressions;
		}

		public void SetEntityProperty(LCPEntityVariable entity, string columnName, string expression)
		{
			entity.ColumnExpressions[columnName] = expression;
		}

		public string GetLiteral(object value)
		{

			if (value == null)
			{
				return "Blank()";
			}
			else if (value is bool boolValue)
			{
				return boolValue.ToString().ToLower();
			}
			else if (value is Guid guidValue)
			{
				return string.Format("\"{0}\"", guidValue.ToString());
			}
			else if (value is string stringValue)
			{
				return string.Format("\"{0}\"", stringValue.Replace("\"", "\"\""));
			}
			else if (value is OptionSetValue optionSetValue)
			{
				return string.Format("OptionSetValue:{0}", optionSetValue.Value.ToString());
			}
			else if (value is EntityReference entityReference)
			{
				return string.Format("LookUp({0}, ThisRecord.{1}=GUID(\"{2}\"))", GetCollectionIdentifier(entityReference.LogicalName), GetPrimaryKeyIdentifier(entityReference.LogicalName), entityReference.Id);
			}
			else if (value is IDictionary<string, object> dict && dict.Count == 0)
			{
				return "{}";
			}
			else if (value is int || value is float || value is decimal)
			{
				return value.ToString();
			}
			else
			{
				throw new NotSupportedException($"Writing literal value of type '{value.GetType().FullName}' is not supported");
			}

		}

		private string GetIdentifier(string variableName)
		{
			return string.Format("'{0}'", variableName);
		}

		public void WriteCreateRow(LCPEntityVariable entity)
		{
			output.Write("Collect(");

			EntityMetadata entityMeta = metadataService.GetEntityMetadata(entity.TableName);
			output.Write(GetGlobalIdentifier(entityMeta.DisplayCollectionName.UserLocalizedLabel.Label));
			output.WriteLine(",");
			WriteRecord(entity, true);
			output.WriteLine(");");
		}

		private string GetGlobalIdentifier(string identifer)
		{
			return string.Format("[@'{0}']", identifer);
		}

		public void WriteUpdateRow(LCPEntityVariable entity)
		{
			if (entity.IsPrimary && context.IsPreOperation)
			{
				EntityMetadata entityMetadata = metadataService.GetEntityMetadata(entity.TableName);
				foreach (KeyValuePair<string, string> kvp in entity.ColumnExpressions)
				{
					AttributeMetadata attrMeta = entityMetadata.Attributes.First(a => a.LogicalName == kvp.Key);
					output.WriteLine($"Set({GetIdentifier(attrMeta.DisplayName.UserLocalizedLabel.Label)}, {kvp.Value});");
				}
			}
			else
			{

				output.Write("Patch(");


				output.Write(GetCollectionIdentifier(entity.TableName));

				output.WriteLine(",");

				if (string.IsNullOrEmpty(entity.IdExpression))
				{
					output.WriteLine($"Defaults('{entity.TableName}'),");
				}
				else
				{
					output.WriteLine($"{{{GetPrimaryKeyIdentifier(entity.TableName)}: {entity.IdExpression} }},");
				}
				WriteRecord(entity, false);
				output.WriteLine(");");
			}
		}

		private string GetPrimaryKeyIdentifier(string entityName)
		{
			EntityMetadata entityMeta = metadataService.GetEntityMetadata(entityName);
			AttributeMetadata attrMeta = entityMeta.Attributes.FirstOrDefault(a => a.LogicalName == entityMeta.PrimaryIdAttribute);
			return GetIdentifier(attrMeta.DisplayName.UserLocalizedLabel.Label);
		}
		private string GetCollectionIdentifier(string entityName)
		{
			EntityMetadata entityMeta = metadataService.GetEntityMetadata(entityName);
			return GetGlobalIdentifier(entityMeta.DisplayCollectionName.UserLocalizedLabel.Label);
		}

		private void WriteRecord(LCPEntityVariable entity, bool excludeDefaultValues)
		{
			EntityMetadata entityMetadata = metadataService.GetEntityMetadata(entity.TableName);


			output.WriteLine("{");
			int count = 0;
			foreach (KeyValuePair<string, string> kvp in entity.ColumnExpressions)
			{
				AttributeMetadata attrMeta = entityMetadata.Attributes.First(a => a.LogicalName == kvp.Key);


				string value = kvp.Value;

				if (value == "true" || value == "false")
				{
					BooleanAttributeMetadata attributeMeta = (BooleanAttributeMetadata)attrMeta;
					OptionMetadata option = value == "true" ? attributeMeta.OptionSet.TrueOption : attributeMeta.OptionSet.FalseOption;
					if (excludeDefaultValues && attributeMeta.DefaultValue == (option.Value == 1))
					{
						continue;
					}
					value = string.Format("'{0} ({1})'.'{2}'", attributeMeta.DisplayName.UserLocalizedLabel.Label, entityMetadata.DisplayCollectionName.UserLocalizedLabel.Label, option.Label.UserLocalizedLabel.Label);

				}
				else if (value.StartsWith("OptionSetValue:"))
				{
					EnumAttributeMetadata attributeMeta = (EnumAttributeMetadata)attrMeta;
					OptionMetadata option = attributeMeta.OptionSet.Options.FirstOrDefault(o => o.Value == int.Parse(value.Substring("OptionSetValue:".Length)));
					if (excludeDefaultValues && attributeMeta.DefaultFormValue == option.Value)
					{
						continue;
					}

					value = string.Format("'{0} ({1})'.'{2}'", attributeMeta.DisplayName.UserLocalizedLabel.Label, entityMetadata.DisplayCollectionName.UserLocalizedLabel.Label, option.Label.UserLocalizedLabel.Label);
				}

				if (count++ >0)
				{
					output.WriteLine(",");
				}
				output.Write(GetIdentifier(attrMeta.DisplayName.UserLocalizedLabel.Label));
				output.Write(": ");
				output.Write(value);
				

				
			}


			output.WriteLine("}");
		}

		public void WriteSetStatus(LCPEntityVariable entity, int statecode, int statuscode)
		{

			WriteUpdateRow(new LCPEntityVariable(entity.TableName, null) { IdExpression = entity.IdExpression, ColumnExpressions = { { "statecode", $"OptionSetValue:{statecode}" }, { "statuscode", $"OptionSetValue:{statuscode}" } } });
		}


		public void WriteLine(string powerFx)
		{
			output.WriteLine(powerFx);
		}

		public void WriteIf(string condition, Action writeThen, Action writeElse)
		{
			output.WriteLine(string.Format("If({0},", condition));

			if (writeThen != null)
			{
				writeThen();
			}
			else
			{
				output.WriteLine("false;");
			}

			if (writeElse != null)
			{
				output.WriteLine(",");
				writeElse();
			}

			output.WriteLine(")");
		}

		public string GetConditionExpression(ConditionOperator condition, string[] elements, string operand)
		{
			string valueExpression;
			switch (condition)
			{
				case ConditionOperator.Equal:
					valueExpression = string.Format("{0} = {1}", operand, elements[0]);
					break;
				case ConditionOperator.NotEqual:
					valueExpression = string.Format("{0} != {1}", operand, elements[0]);
					break;
				case ConditionOperator.Null:
					valueExpression = string.Format("IsBlank({0})", operand);
					break;
				case ConditionOperator.NotNull:
					valueExpression = string.Format("!IsBlank({0})", operand);
					break;
				case ConditionOperator.Contains:
					valueExpression = $"{elements[0]} in {operand}";
					break;
				case ConditionOperator.DoesNotContain:
					valueExpression = $"!({elements[0]} in {operand})";
					break;
				case ConditionOperator.GreaterThan:
					valueExpression = string.Format("{0} > {1}", operand, elements[0]);
					break;
				case ConditionOperator.GreaterEqual:
					valueExpression = string.Format("{0} >= {1}", operand, elements[0]);
					break;
				case ConditionOperator.LessThan:
					valueExpression = string.Format("{0} < {1}", operand, elements[0]);
					break;
				case ConditionOperator.LessEqual:
					valueExpression = string.Format("{0} <= {1}", operand, elements[0]);
					break;
				case ConditionOperator.BeginsWith:
					valueExpression = string.Format("StartsWith({0}, {1})", operand, elements[0]);
					break;
				case ConditionOperator.DoesNotBeginWith:
					valueExpression = string.Format("!(StartsWith({0}, {1}))", operand, elements[0]);
					break;
				case ConditionOperator.EndsWith:
					valueExpression = string.Format("EndsWith({0}, {1})", operand, elements[0]);
					break;
				case ConditionOperator.DoesNotEndWith:
					valueExpression = string.Format("!(EndsWith({0}, {1}))", operand, elements[0]);
					break;
				case ConditionOperator.On:
					valueExpression = string.Format("(DateValue({1}) <= {0} && DateAdd(DateValue({1}), 1, TimeUnit.Days) > {0})", operand, elements[0]);
					break;
				case ConditionOperator.OnOrAfter:
					valueExpression = string.Format("DateValue({1}) <= {0}", operand, elements[0]);
					break;
				case ConditionOperator.OnOrBefore:
					valueExpression = string.Format("DateAdd(DateValue({1}), 1, TimeUnit.Days) > {0}", operand, elements[0]);
					break;
				case ConditionOperator.NotOn:
					valueExpression = string.Format("(DateValue({1}) > {0} || DateAdd(DateValue({1}), 1, TimeUnit.Days) <= {0})", operand, elements[0]);
					break;




				default:
					throw new NotImplementedException($"Condition operator '{condition}' is not implemented");
			}

			return valueExpression;
		}

		public string GetCoalesceExpression(string[] elements)
		{
			string valueExpression = "Coalesce(";
			valueExpression += string.Join(",", elements);
			valueExpression += ")";
			return valueExpression;
		}

		public string GetLogicalConditionExpression(LogicalOperator logicalOperator, string leftOperand, string rightOperand)
		{
			switch (logicalOperator)
			{
				case LogicalOperator.And:
					return string.Format("({0} && {1})", leftOperand, rightOperand);
				case LogicalOperator.Or:
					return string.Format("({0} || {1})", leftOperand, rightOperand);
				default:
					throw new NotImplementedException($"Logical condition '{logicalOperator}' is not implemented");
			}
		}

		public void WriteAssignRow(LCPEntityVariable entity, string ownerExpresion)
		{
			WriteUpdateRow(new LCPEntityVariable(entity.TableName, null) { IdExpression = entity.IdExpression, ColumnExpressions = { { "ownerid", ownerExpresion } } });

		}

		public void WriteTerminate(OperationStatus operationStatus, string reasonExpression)
		{
			if (!new[] { OperationStatus.Failed, OperationStatus.Canceled }.Contains(operationStatus))
			{
				throw new NotImplementedException($"Terminate status '{operationStatus}' is not supported in LCP");
			}

			output.WriteLine($"Error({{Kind: ErrorKind.Custom, Message: {reasonExpression}}});");
		}

		public IEnumerable<IOutput> GetOutputs()
		{
			StringBuilder expr = new StringBuilder(output.ToString());
			expr.Append(outputAfter.ToString());

			//Remove trailing ;
			if (expr[expr.Length - 1] == ';')
			{
				expr.Remove(expr.Length- 1, 1);
			}

			foreach (MessageName messageName in Enum.GetValues(typeof(MessageName)))
			{
				if (context.MessageNames.HasFlag(messageName))
				{
					if (messageName == MessageName.Action)
					{
						yield return new InstantPlugin(string.Format("{0} - {1}", context.WorkflowName, messageName), context.PrimaryEntityName.ToLower(), expr.ToString());
					}
					else
					{
						yield return new AutomatedPlugin(string.Format("{0} - {1}", context.WorkflowName, messageName), context.PrimaryEntityName.ToLower(), context.IsPreOperation ? 20 : 40, messageName.ToString(), expr.ToString());
					}
				}
			}
		}

		public void WriteCallEntityAction(LCPEntityVariable entity, string schemaName)
		{
			output.WriteLine(string.Format("Environment.{0}({1});", GetIdentifier(schemaName), entity.RecordExpression));
		}

		public string GetConcatExpression(string[] elements)
		{
			if (elements.Length == 0)
			{
				return "";
			}

			if (elements.Length == 1)
			{
				return elements.First();
			}

			return string.Format("Concatenate({0})", string.Join(", ", elements));
		}

		public void WriteSetDisplayMode(LCPEntityVariable entity, string controlIdExpression, string isReadOnlyExpression)
		{
			throw new NotImplementedException();
		}

		public void WriteSetClientEntityAttributeValues(LCPEntityVariable entity)
		{
			this.WriteUpdateRow(entity);
		}

		public void WriteClientRecommendation(LCPEntityVariable entity, string controlIdExpression, string description, Dictionary<string, Action> writeActions) 
		{
			throw new NotImplementedException();
		}
	}
}
