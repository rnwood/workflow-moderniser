using Microsoft.Crm.Workflow.ObjectModel;
using Microsoft.VisualBasic.Activities;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Sdk.Workflow.Activities;
using System;
using System.Activities;
using System.Activities.Expressions;
using System.Activities.Presentation.Debug;
using System.Activities.Statements;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xaml;
using WorkflowModerniser.Data;
using WorkflowModerniser.Inputs;
using WorkflowModerniser.Outputs;
using WorkflowModerniser.SubstituteClientActivities;

namespace WorkflowModerniser
{
	public partial class WorkflowConverter<TEntityVariable> : IWorkflowConverter where TEntityVariable : EntityVariable
	{
		public WorkflowConverter(IWorkflowSource workflowSource, IMetadataService metadataService, Func<WriterContext, IWorkflowWriter<TEntityVariable>> writerFactory)
		{
			this.workflowSource = workflowSource;
			this.writerFactory = writerFactory;
			this.metadataService = metadataService;
		}

		public IEnumerable<IOutput> Convert(Data.Workflow workflow)
		{

			List<IOutput> results = new List<IOutput>();

			switch (workflow.Category)
			{
				case Workflow_Category.BusinessRule:
					{
						WriterContext context = new WriterContext(workflow.Name, false, MessageName.Action, workflow.PrimaryEntityName, metadataService);
						results.AddRange(Convert(workflow, context));
						break;
					}
				case Workflow_Category.Workflow:
					{

						foreach (Workflow_Stage stage in new[] { Workflow_Stage.PreOperation, Workflow_Stage.PostOperation })
						{
							MessageName thisOperationMessages = 0;

							if (workflow.CreateStage == stage)
							{
								thisOperationMessages |= MessageName.Create;
							}
							if (workflow.UpdateStage == stage)
							{
								thisOperationMessages |= MessageName.Update;
							}
							if (workflow.DeleteStage == stage)
							{
								thisOperationMessages |= MessageName.Delete;
							}

							if (thisOperationMessages != 0)
							{
								WriterContext context = new WriterContext(workflow.Name, stage == Workflow_Stage.PreOperation, thisOperationMessages, workflow.PrimaryEntityName, metadataService);
								results.AddRange(Convert(workflow, context));
							}
						}


						if (workflow.Subprocess.GetValueOrDefault())
						{
							WriterContext context = new WriterContext(workflow.Name, false, MessageName.Action, workflow.PrimaryEntityName, metadataService);
							results.AddRange(Convert(workflow, context));
						}
					}
					break;
				default:
					throw new NotSupportedException($"Workflow type '{workflow.Category}' is not supported.");
			}

			return results;
		}

		private IEnumerable<IOutput> Convert(Data.Workflow workflow, WriterContext context)
		{
			writer = writerFactory(context);
			entities.Clear();
			variables.Clear();
			childOutputs.Clear();
			this.targetTableName = workflow.PrimaryEntity;

			entities["InputEntities(\"primaryEntity\")"] = primaryEntity = writer.LoadPrimaryEntity(workflow.PrimaryEntity);

			XamlXmlReader xamlReader = new XamlXmlReader(new StringReader(workflow.XAMl
				.Replace("clr-namespace:Microsoft.Crm.Workflow.ClientActivities;assembly=Microsoft.Crm.Workflow, Version=9.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35", "clr-namespace:WorkflowModerniser.SubstituteClientActivities;assembly=WorkflowModerniser.SubstituteClientActivities")
				.Replace("assembly=Microsoft.Crm, Version=9.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35", "assembly=WorkflowModerniser.SubstituteClientActivities")));
			ActivityBuilder builder = (ActivityBuilder)XamlServices.Load(System.Activities.XamlIntegration.ActivityXamlServices.CreateBuilderReader(xamlReader));

			Microsoft.Xrm.Sdk.Workflow.Activities.Workflow workflowImplementation = (Microsoft.Xrm.Sdk.Workflow.Activities.Workflow)builder.Implementation;

			ConvertActivities(workflowImplementation.Activities, workflowImplementation.Variables);
			return childOutputs.Concat(writer.GetOutputs());
		}

		private IWorkflowWriter<TEntityVariable> writer;
		private List<IOutput> childOutputs = new List<IOutput>();
		private string targetTableName;
		private TEntityVariable primaryEntity;
		private readonly Dictionary<string, TEntityVariable> entities = new Dictionary<string, TEntityVariable>();
		private readonly Dictionary<string, string> variables = new Dictionary<string, string>();
		private readonly IWorkflowSource workflowSource;
		private readonly Func<WriterContext, IWorkflowWriter<TEntityVariable>> writerFactory;
		private readonly IMetadataService metadataService;

		private void ConvertActivities(Collection<Activity> activities, Collection<Variable> variables)
		{
			foreach (Variable variable in variables)
			{
				this.variables[variable.Name] = GetInValue(variable.Default);
			}

			foreach (Activity activity in activities)
			{
				ConvertActivity(activity);
			}
		}

		private void ConvertActivity(Activity activity)
		{

			switch (activity)
			{
				case Sequence sequence:
					ConvertSequence(sequence);
					break;
				case Assign<Entity> assign:
					ConvertAssign(assign);
					break;
				case Assign<Guid> assign:
					ConvertAssign(assign);
					break;
				case ActivityReference activityReference:
					ConvertActivityReference(activityReference);
					break;
				case SetEntityProperty setEntity:
					ConvertSetEntityProperty(setEntity);
					break;
				case GetEntityProperty getEntity:
					ConvertGetEntityProperty(getEntity);
					break;
				case UpdateEntity updateEntity:
					ConvertUpdateEntity(updateEntity);
					break;
				case SetState setState:
					ConvertSetState(setState);
					break;
				case AssignEntity assignEntity:
					ConvertAssignEntity(assignEntity);
					break;
				case CreateEntity createEntity:
					ConvertCreateEntity(createEntity);
					break; ;
				case Persist persist:
					ConvertPersist(persist);
					break;
				case TerminateWorkflow terminate:
					ConvertTerminate(terminate);
					break;
				case SendEmail sendEmail:
					ConvertSendEmail(sendEmail);
					break;
				case StartChildWorkflow startChildWorkflow:
					ConvertStartChildWorkflow(startChildWorkflow);
					break;
				case SetDisplayMode setDisplayMode:
					ConvertSetDisplayMode(setDisplayMode);
					break;
				case SetMessage setMessage:
					ConvertSetMessage(setMessage);
					break;
				case SetAttributeValue setAttributeValue:
					ConvertSetAttributeValue(setAttributeValue);
					break;
				default:
					throw new NotImplementedException($"Activity type '{activity.GetType().FullName}' is not implemented");
			}
		}

		private void ConvertSetAttributeValue(SetAttributeValue setAttributeValue)
		{
			string entityName = ((VisualBasicValue<Entity>)setAttributeValue.Entity.Expression).ExpressionText;
			TEntityVariable entity = entities[entityName];

			writer.WriteSetClientEntityAttributeValues(entity);
		}

		private void ConvertSetMessage(SetMessage setMessage)
		{
			string entityName = ((VisualBasicValue<Entity>)setMessage.Entity.Expression).ExpressionText;
			TEntityVariable entity = entities[entityName];

			string controlIdExpression = GetInValue(setMessage.ControlId.Expression);

			string description = GetInValue(setMessage.StepLabels.Description.Expression);

			Collection<StepLabel> stepLabels = (Collection<StepLabel>)setMessage.Activities.Properties["StepLabels"];
			Collection<Activity> activities = (Collection<Activity>)setMessage.Activities.Properties["Activities"];

			Dictionary<string, Action> writeActions = new Dictionary<string, Action>();
			for(int i = 0;i<activities.Count; i++)
			{
				var activity = activities[i];
				string label = activity.DisplayName.Split(new[] { ':' }, 2)[1];
				writeActions[label] = () => ConvertActivity(activity);
			}
			
	
			
			writer.WriteClientRecommendation(entity, controlIdExpression, description, writeActions);
		}

		private void ConvertSetDisplayMode(SetDisplayMode setDisplayMode)
		{
			string entityName = ((VisualBasicValue<Entity>)setDisplayMode.Entity.Expression).ExpressionText;
			TEntityVariable entity = entities[entityName];

			string controlIdExpression = GetInValue(setDisplayMode.ControlId.Expression);
			string isReadOnlyExpression = GetInValue(setDisplayMode.IsReadOnly.Expression);

			writer.WriteSetDisplayMode(entity, controlIdExpression, isReadOnlyExpression);
		}

		private void ConvertStartChildWorkflow(StartChildWorkflow startChildWorkflow)
		{
			Guid workflowId = ((Literal<Guid>)startChildWorkflow.WorkflowId.Expression).Value;
			string entityName = GetInValue(startChildWorkflow.EntityName.Expression).Trim('"');
			string entityIdExpr = ((VisualBasicValue<Guid>)startChildWorkflow.EntityId.Expression).ExpressionText;
			string entityExpr = Regex.Replace(entityIdExpr, "^(.*)\\.Id$", "$1");
			TEntityVariable entity = entities[entityExpr];

			Data.Workflow workflow = workflowSource.GetWorkflow(workflowId);

			IEnumerable<IOutput> outputs = new WorkflowConverter<TEntityVariable>(workflowSource, metadataService, writerFactory).Convert(workflow);
			childOutputs.AddRange(outputs);
			writer.WriteCallEntityAction(entity, outputs.OfType<ICustomActionOutput>().Single().SchemaName);
		}

		private void ConvertSendEmail(SendEmail sendEmail)
		{
			throw new NotImplementedException("Send email is not supported");

			//VisualBasicReference<Entity> emailExpr = (VisualBasicReference<Entity>)sendEmail.Entity.Expression;
			//TEntityVariable email = entities[emailExpr.ExpressionText];

			//writer.WriteCreateRow(email);
		}

		private void ConvertTerminate(TerminateWorkflow terminate)
		{
			string reason = ((VisualBasicValue<string>)terminate.Reason.Expression).ExpressionText;
			//DirectCast(AssignStep7_3, System.String)
			reason = Regex.Replace(reason, "DirectCast\\(([^,]+), System.String\\)", "$1");
			reason = variables[reason];

			string exception = ((VisualBasicValue<Exception>)terminate.Exception.Expression).ExpressionText;

			//New Microsoft.Xrm.Sdk.InvalidPluginExecutionException(Microsoft.Xrm.Sdk.OperationStatus.Canceled)
			exception = Regex.Replace(exception, "New Microsoft\\.Xrm\\.Sdk\\.InvalidPluginExecutionException\\(Microsoft\\.Xrm\\.Sdk\\.OperationStatus\\.(.+)\\)", "$1");

			writer.WriteTerminate((OperationStatus)Enum.Parse(typeof(OperationStatus), exception), reason);
		}

		private void ConvertAssignEntity(AssignEntity assignEntity)
		{
			string varName = ((VisualBasicReference<Entity>)assignEntity.Entity.Expression).ExpressionText;
			string ownerExpression = ((VisualBasicValue<EntityReference>)assignEntity.Owner.Expression).ExpressionText;

			//DirectCast(AssignStep7_3, Microsoft.Xrm.Sdk.EntityReference)
			ownerExpression = Regex.Replace(ownerExpression, "DirectCast\\(([^,]+), Microsoft.Xrm.Sdk.EntityReference\\)", "$1");

			string ownerExpresion = variables[ownerExpression];

			TEntityVariable entity = entities[varName];
			writer.WriteAssignRow(entity, ownerExpresion);
		}

		private void ConvertSetState(SetState setState)
		{
			string varName = ((VisualBasicReference<Entity>)setState.Entity.Expression).ExpressionText;
			int state = ((ReferenceLiteral<OptionSetValue>)setState.State.Expression).Value.Value;
			int status = ((ReferenceLiteral<OptionSetValue>)setState.Status.Expression).Value.Value;

			TEntityVariable entity = entities[varName];

			writer.WriteSetStatus(entity, state, status);
		}

		private void ConvertPersist(Persist persist)
		{
			//No op
		}

		private void ConvertUpdateEntity(UpdateEntity updateEntity)
		{
			string varName = ((VisualBasicReference<Entity>)updateEntity.Entity.Expression).ExpressionText;
			writer.WriteUpdateRow(entities[varName]);
		}

		private void ConvertCreateEntity(CreateEntity updateEntity)
		{
			string varName = ((VisualBasicReference<Entity>)updateEntity.Entity.Expression).ExpressionText;
			writer.WriteCreateRow(entities[varName]);
		}

		private void ConvertSetEntityProperty(SetEntityProperty setEntity)
		{
			string varName = ((VisualBasicReference<Entity>)setEntity.Entity.Expression).ExpressionText;
			string attr = ((Literal<string>)setEntity.Attribute.Expression).Value;
			string expression = ((VisualBasicValue<object>)setEntity.Value.Expression).ExpressionText;

			string value = variables[expression];
			writer.SetEntityProperty(entities[varName], attr, value);
		}

		private void ConvertGetEntityProperty(GetEntityProperty getEntity)
		{
			string entityVar = ((VisualBasicValue<Entity>)getEntity.Entity.Expression).ExpressionText;
			string attr = ((Literal<string>)getEntity.Attribute.Expression).Value;
			string expression = ((VisualBasicReference<object>)getEntity.Value.Expression).ExpressionText;

			CreateRelatedEntityIfNeeded(entityVar);
			variables[expression] = writer.GetEntityPropertyExpresson(entities[entityVar], attr);
		}

		private void ConvertActivityReference(ActivityReference activityReference)
		{
			if (activityReference.AssemblyQualifiedName.StartsWith("Microsoft.Crm.Workflow.Activities.EvaluateExpression,"))
			{
				ConvertEvaluateExpression(activityReference);
			}
			else if (activityReference.AssemblyQualifiedName.StartsWith("Microsoft.Crm.Workflow.Activities.ConditionSequence,"))
			{
				ConvertConditionSequence(activityReference);
			}
			else if (activityReference.AssemblyQualifiedName.StartsWith("Microsoft.Crm.Workflow.Activities.EvaluateCondition,"))
			{
				ConvertEvaluateCondition(activityReference);
			}
			else if (activityReference.AssemblyQualifiedName.StartsWith("Microsoft.Crm.Workflow.Activities.ConditionBranch,"))
			{
				ConvertConditionBranch(activityReference);
			}
			else if (activityReference.AssemblyQualifiedName.StartsWith("Microsoft.Crm.Workflow.Activities.EvaluateLogicalCondition,"))
			{
				ConvertEvaluateLogicalCondition(activityReference);
			}
			else if (activityReference.AssemblyQualifiedName.StartsWith("Microsoft.Crm.Workflow.Activities.Composite,"))
			{
				ConvertComposite(activityReference);
			}
			else if (activityReference.AssemblyQualifiedName.StartsWith("Microsoft.Crm.Workflow.Activities.ActionComposite,"))
			{
				ConvertComposite(activityReference);
			}
			else
			{
				throw new NotImplementedException($"Activity reference type '{activityReference.AssemblyQualifiedName}' is not implemented");
			}
		}

		private void ConvertComposite(ActivityReference activityReference)
		{
			Collection<Activity> activities = (Collection<Activity>)activityReference.Properties["Activities"];
			Collection<Variable> variables = (Collection<Variable>)activityReference.Properties["Variables"];

			ConvertActivities(activities, variables);
		}

		private void ConvertEvaluateLogicalCondition(ActivityReference activityReference)
		{
			InArgument<LogicalOperator> operatorArg = (InArgument<LogicalOperator>)activityReference.Arguments["LogicalOperator"];
			LogicalOperator logicalOperator = ((Literal<LogicalOperator>)operatorArg.Expression).Value;

			InArgument<bool> leftOperandArg = (InArgument<bool>)activityReference.Arguments["LeftOperand"];
			string leftOperandVariable = ((VisualBasicValue<bool>)leftOperandArg.Expression).ExpressionText;

			InArgument<bool> rightOperandArg = (InArgument<bool>)activityReference.Arguments["RightOperand"];
			string rightOperandVariable = ((VisualBasicValue<bool>)rightOperandArg.Expression).ExpressionText;

			string valueExpression = writer.GetLogicalConditionExpression(logicalOperator, variables[leftOperandVariable], variables[rightOperandVariable]);

			OutArgument<bool> resultArg = (OutArgument<bool>)activityReference.Arguments["Result"];
			string identifier = ((VisualBasicReference<bool>)resultArg.Expression).ExpressionText;

			variables[identifier] = valueExpression;
		}

		private string GetInValue(ActivityWithResult activity)
		{
			if (activity == null)
			{
				return writer.GetLiteral(null);
			}

			if (activity.GetType().GetGenericTypeDefinition() == typeof(Literal<>))
			{
				object value = activity.GetType().GetProperty("Value").GetValue(activity);
				return writer.GetLiteral(value);

			}

			if (activity.GetType().GetGenericTypeDefinition() == typeof(VisualBasicValue<>))
			{
				string expressionText = (string)activity.GetType().GetProperty("ExpressionText").GetValue(activity);

				if (expressionText == "New Dictionary(Of System.String, System.Object)")
				{
					return writer.GetLiteral(new Dictionary<string, object>());
				}
				return variables[expressionText];
			}

			throw new NotImplementedException($"Unhandled type for activity '{activity.DisplayName}'");
		}

		private void ConvertConditionBranch(ActivityReference activityReference)
		{
			string condition = GetInValue(activityReference.Arguments["Condition"].Expression);

			ActivityReference thenProp = (ActivityReference)activityReference.Properties["Then"];
			Collection<Activity> thenActivities = (Collection<Activity>)thenProp?.Properties["Activities"];
			Collection<Variable> thenVariables = (Collection<Variable>)thenProp?.Properties["Variables"];

			ActivityReference elseProp = (ActivityReference)activityReference.Properties["Else"];
			Collection<Activity> elseActivities = (Collection<Activity>)elseProp?.Properties["Activities"];
			Collection<Variable> elseVariables = (Collection<Variable>)elseProp?.Properties["Variables"];

			writer.WriteIf(condition, thenActivities != null ? (() =>

				ConvertActivities(thenActivities, thenVariables)
			) : (Action)null, elseActivities != null ? (() => ConvertActivities(elseActivities, elseVariables)) : (Action)null);
		}

		private void ConvertConditionSequence(ActivityReference activityReference)
		{
			Collection<Activity> activities = (Collection<Activity>)activityReference.Properties["Activities"];
			Collection<Variable> variables = (Collection<Variable>)activityReference.Properties["Variables"];
			ConvertActivities(activities, variables);
		}

		private void ConvertEvaluateCondition(ActivityReference activityReference)
		{
			InArgument<ConditionOperator> conditionArg = (InArgument<ConditionOperator>)activityReference.Arguments["ConditionOperator"];
			ConditionOperator condition = ((Literal<ConditionOperator>)conditionArg.Expression).Value;
			InArgument<object[]> parametersArg = (InArgument<object[]>)activityReference.Arguments["Parameters"];

			string[] elements = null;
			if (parametersArg != null)
			{
				string elementsExpressions = Regex.Replace(((VisualBasicValue<object[]>)parametersArg.Expression).ExpressionText, @"(^New Object\(\) \{|\}$)", "");
				string[] elementVariables = elementsExpressions.Split(',').Select(e => e.Trim()).ToArray();
				elements = elementVariables.Select(e => variables[e]).ToArray();
			}


			InArgument<object> operandArg = (InArgument<object>)activityReference.Arguments["Operand"];
			string operandVariable = ((VisualBasicValue<object>)operandArg.Expression).ExpressionText;

			string valueExpression;

			string operand = variables[operandVariable];
			valueExpression = writer.GetConditionExpression(condition, elements, operand);

			OutArgument<bool> resultArg = (OutArgument<bool>)activityReference.Arguments["Result"];
			string identifier = ((VisualBasicReference<bool>)resultArg.Expression).ExpressionText;

			variables[identifier] = valueExpression;
		}



		private void ConvertEvaluateExpression(ActivityReference activityReference)
		{
			InArgument<string> expressionOperatorArg = (InArgument<string>)activityReference.Arguments["ExpressionOperator"];
			string expressionOperator = ((Literal<string>)expressionOperatorArg.Expression).Value;

			if (expressionOperator == "CreateCrmType")
			{
				ConvertCreateCrmType(activityReference);
			}
			else if (expressionOperator == "SelectFirstNonNull")
			{
				ConvertSelectFirstNonNull(activityReference);
			}
			else if (expressionOperator == "Add")
			{
				ConvertAdd(activityReference);
			}
			else
			{
				throw new NotImplementedException($"Evaluate expression expression operator '{expressionOperator}' is not implemented");
			}
		}

		private void ConvertAdd(ActivityReference activityReference)
		{
			ReferenceLiteral<Type> targetTypeArg = (ReferenceLiteral<Type>)activityReference.Arguments["TargetType"].Expression;
			Type targetType = targetTypeArg.Value;

			if (targetType != typeof(string))
			{
				throw new NotImplementedException($"Add is not implemented for type '{targetType}'");
			}

			InArgument<object[]> paramArg = (InArgument<object[]>)activityReference.Arguments["Parameters"];
			OutArgument<object> resultArg = (OutArgument<object>)activityReference.Arguments["Result"];

			VisualBasicValue<object[]> input = (VisualBasicValue<object[]>)paramArg.Expression;


			string expression = input.ExpressionText;

			//New Object() { CreateStep3_2 }

			string elementsExpressions = Regex.Replace(expression, @"(^New Object\(\) \{|\}$)", "");
			string[] elements = elementsExpressions.Split(',').Select(e => e.Trim()).Select(e => variables[e]).ToArray();

			string valueExpression = writer.GetConcatExpression(elements);

			string identifier = ((VisualBasicReference<object>)resultArg.Expression).ExpressionText;

			variables[identifier] = valueExpression;
		}

		private void ConvertSelectFirstNonNull(ActivityReference activityReference)
		{
			InArgument<object[]> paramArg = (InArgument<object[]>)activityReference.Arguments["Parameters"];
			OutArgument<object> resultArg = (OutArgument<object>)activityReference.Arguments["Result"];

			VisualBasicValue<object[]> input = (VisualBasicValue<object[]>)paramArg.Expression;

			string valueExpression;

			string expression = input.ExpressionText;

			//New Object() { CreateStep3_2 }

			string elementsExpressions = Regex.Replace(expression, @"(^New Object\(\) \{|\}$)", "");
			string[] elements = elementsExpressions.Split(',').Select(e => e.Trim()).ToArray();

			if (elements.Length > 1)
			{
				valueExpression = writer.GetCoalesceExpression(elements.Select(e => variables[e]).ToArray());
			}
			else
			{
				valueExpression = variables[elements[0]];
			}

			string identifier = ((VisualBasicReference<object>)resultArg.Expression).ExpressionText;

			variables[identifier] = valueExpression;

		}

		private void ConvertCreateCrmType(ActivityReference activityReference)
		{
			InArgument<object[]> paramArg = (InArgument<object[]>)activityReference.Arguments["Parameters"];
			OutArgument<object> resultArg = (OutArgument<object>)activityReference.Arguments["Result"];

			Activity<object[]> expression = paramArg.Expression;

			string valueExpression;

			switch (expression)
			{
				case VisualBasicValue<object[]> vbv:
					valueExpression = ConvertCreateCrmType_VisualBasicValue(vbv);
					break;
				default:
					throw new NotImplementedException($"Expression of type '{paramArg.GetType().FullName}' is not implemented");
			}

			string identifier = ((VisualBasicReference<object>)resultArg.Expression).ExpressionText;

			variables[identifier] = valueExpression;

		}

		private string ConvertCreateCrmType_VisualBasicValue(VisualBasicValue<object[]> vbv)
		{
			string expression = vbv.ExpressionText;

			//New Object() { Microsoft.Xrm.Sdk.Workflow.WorkflowPropertyType.String, "Chief troublemaker", "String" }

			string elementsExpressions = Regex.Replace(expression, @"(^New Object\(\) \{|\}$)", "");

			string[] elements = elementsExpressions.Split(',').Select(e => e.Trim()).ToArray();
			string typeExpression = elements[0];
			string valueExpression = elements[1];
			valueExpression = valueExpression.Substring(1, valueExpression.Length - 2).Replace("\"\"", "\"");

			switch (typeExpression)
			{
				case "Microsoft.Xrm.Sdk.Workflow.WorkflowPropertyType.String":
					return writer.GetLiteral(valueExpression);
				case "Microsoft.Xrm.Sdk.Workflow.WorkflowPropertyType.Guid":
					return writer.GetLiteral(new Guid(valueExpression));
				case "Microsoft.Xrm.Sdk.Workflow.WorkflowPropertyType.OptionSetValue":
					return writer.GetLiteral(new OptionSetValue(int.Parse(valueExpression)));
				case "Microsoft.Xrm.Sdk.Workflow.WorkflowPropertyType.Boolean":
					return writer.GetLiteral(bool.Parse(valueExpression.ToLower()));
				case "Microsoft.Xrm.Sdk.Workflow.WorkflowPropertyType.EntityReference":
					if (elements.Length == 3)
					{
						string varValue = variables[elements[1]];

						return varValue;
					}
					return writer.GetLiteral(new EntityReference(valueExpression, new Guid(variables[elements[3]].Trim('"'))));
				default:
					throw new NotSupportedException($"VBExpression of type '{typeExpression}' is not supported");
			}
		}

		private void ConvertAssign(Assign<Entity> assign)
		{
			VisualBasicValue<Entity> e = (VisualBasicValue<Entity>)((InArgument<Entity>)assign.Value.Expression).Expression;
			VisualBasicReference<Entity> t = (VisualBasicReference<Entity>)((OutArgument<Entity>)assign.To).Expression;

			//New Entity("contact")
			Match newEntityMatch = Regex.Match(e.ExpressionText, "New Entity\\(\"([^\"]+)\"\\)");
			if (newEntityMatch.Success)
			{
				string tableName = newEntityMatch.Groups[1].Value;

				entities[t.ExpressionText] = writer.NewEntityVariable(tableName);

			}
			else if (entities.ContainsKey(e.ExpressionText))
			{
				if (!entities.ContainsKey(t.ExpressionText))
				{
					entities[t.ExpressionText] = writer.CloneEntityVariable(entities[e.ExpressionText]);
				}
				else
				{
					writer.CopyEntityVariableValues(entities[e.ExpressionText], entities[t.ExpressionText]);
				}
			}
			else
			{
				throw new NotImplementedException($"Assign expression '{e.ExpressionText}' is not supported");
			}


		}

		private void ConvertAssign(Assign<Guid> assign)
		{
			VisualBasicValue<Guid> e = (VisualBasicValue<Guid>)((InArgument<Guid>)assign.Value.Expression).Expression;
			VisualBasicReference<Guid> t = (VisualBasicReference<Guid>)((OutArgument<Guid>)assign.To).Expression;

			string sourceProp = e.ExpressionText.Split('.').Last();
			string targetProp = t.ExpressionText.Split('.').Last();

			if (sourceProp == targetProp && sourceProp == "Id")
			{
				string sourceVar = e.ExpressionText.Split('.').First();
				string targetVar = t.ExpressionText.Split('.').First();


				CreateRelatedEntityIfNeeded(sourceVar);
				CreateRelatedEntityIfNeeded(targetVar);

				writer.CopyEntityId(entities[sourceVar], entities[targetVar]);


			}
			else
			{
				throw new NotImplementedException("Assign '{0}' from '{1}' is not supported");
			}

		}

		private void CreateRelatedEntityIfNeeded(string varName)
		{
			//InputEntities("related_parentcustomerid#account")
			if (!entities.ContainsKey(varName))
			{
				Match relatedVarMatch = Regex.Match(varName, "^InputEntities\\(\"related_([^#]+)#([^\"]+)\"\\)$");
				if (relatedVarMatch.Success)
				{
					string relationshipName = relatedVarMatch.Groups[1].Value;
					string entityName = relatedVarMatch.Groups[2].Value;

					entities[varName] = writer.LoadRelatedEntity(this.primaryEntity, entityName, relationshipName);
				}
			}

		}

		private void ConvertSequence(Sequence sequenceActivity)
		{
			ConvertActivities(sequenceActivity.Activities, sequenceActivity.Variables);
		}
	}
}