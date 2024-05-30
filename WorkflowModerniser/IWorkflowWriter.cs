using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using WorkflowModerniser.Outputs;

namespace WorkflowModerniser
{
	public interface IWorkflowWriter<TEntityVariable> where TEntityVariable : EntityVariable
	{
		TEntityVariable CloneEntityVariable(TEntityVariable source);
		void CopyEntityId(TEntityVariable source, TEntityVariable target);
		void CopyEntityVariableValues(TEntityVariable source, TEntityVariable target);
		string GetCoalesceExpression(string[] elements);
		string GetConditionExpression(ConditionOperator condition, string[] elements, string operand);
		string GetEntityPropertyExpresson(TEntityVariable entity, string columnName);
		string GetLiteral(object value);
		string GetLogicalConditionExpression(LogicalOperator logicalOperator, string v1, string v2);
		TEntityVariable LoadPrimaryEntity(string logicalName);
		TEntityVariable LoadRelatedEntity(TEntityVariable entity, string entityName, string relationshipName);
		TEntityVariable NewEntityVariable(string logicalName);
		void SetEntityProperty(TEntityVariable entity, string columnName, string expression);
		IEnumerable<IOutput> GetOutputs();
		void WriteAssignRow(TEntityVariable entity, string ownerExpresion);
		void WriteComment(string comment);
		void WriteCreateRow(TEntityVariable entity);
		void WriteIf(string condition, Action writeThen, Action writeElse);
		void WriteLine(string powerFx);
		void WriteSetStatus(TEntityVariable entity, int statecode, int statuscode);
		void WriteTerminate(OperationStatus operationStatus, string reason);
		void WriteUpdateRow(TEntityVariable entity);
		void WriteVariableSet(string variableName, object value);
		void WriteCallEntityAction(TEntityVariable entity, string schemaName);
		string GetConcatExpression(string[] elements);
		void WriteSetDisplayMode(TEntityVariable entity, string controlIdExpression, string isReadOnlyExpression);
		void WriteSetClientEntityAttributeValues(TEntityVariable entity);
		void WriteClientRecommendation(TEntityVariable entity, string controlIdExpression, string description, Dictionary<string, Action> writeActions) ;
	}
}