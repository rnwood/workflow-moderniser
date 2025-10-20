using FakeItEasy;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using WorkflowModerniser.Inputs;
using WorkflowModerniser.Outputs.JavaScriptFormScript;

namespace WorkflowModerniser.Tests
{
	[TestClass]
	public class JavascriptFormScriptWriterTests
	{
		[TestMethod]
		public void Constructor_AcceptsWriterContext()
		{
			// Arrange
			var metadataService = A.Fake<IMetadataService>();
			var context = new WriterContext("Test", false, MessageName.Create, "contact", metadataService);

			// Act
			var writer = new JavascriptFormScriptWriter(context);

			// Assert
			Assert.IsNotNull(writer);
		}

		[TestMethod]
		public void GetLiteral_ReturnsNullForNull()
		{
			// Arrange
			var metadataService = A.Fake<IMetadataService>();
			var context = new WriterContext("Test", false, MessageName.Create, "contact", metadataService);
			var writer = new JavascriptFormScriptWriter(context);

			// Act
			var result = writer.GetLiteral(null);

			// Assert
			Assert.AreEqual("null", result);
		}

		[TestMethod]
		public void GetLiteral_ReturnsTrueForBoolean()
		{
			// Arrange
			var metadataService = A.Fake<IMetadataService>();
			var context = new WriterContext("Test", false, MessageName.Create, "contact", metadataService);
			var writer = new JavascriptFormScriptWriter(context);

			// Act
			var resultTrue = writer.GetLiteral(true);
			var resultFalse = writer.GetLiteral(false);

			// Assert
			Assert.AreEqual("true", resultTrue);
			Assert.AreEqual("false", resultFalse);
		}

		[TestMethod]
		public void GetLiteral_ReturnsQuotedStringForString()
		{
			// Arrange
			var metadataService = A.Fake<IMetadataService>();
			var context = new WriterContext("Test", false, MessageName.Create, "contact", metadataService);
			var writer = new JavascriptFormScriptWriter(context);

			// Act
			var result = writer.GetLiteral("test value");

			// Assert
			Assert.AreEqual("\"test value\"", result);
		}

		[TestMethod]
		public void GetLiteral_EscapesQuotesInString()
		{
			// Arrange
			var metadataService = A.Fake<IMetadataService>();
			var context = new WriterContext("Test", false, MessageName.Create, "contact", metadataService);
			var writer = new JavascriptFormScriptWriter(context);

			// Act
			var result = writer.GetLiteral("He said \"hello\"");

			// Assert
			Assert.IsTrue(result.Contains("\"\""));
		}

		[TestMethod]
		public void GetConditionExpression_ReturnsEqualExpression()
		{
			// Arrange
			var metadataService = A.Fake<IMetadataService>();
			var context = new WriterContext("Test", false, MessageName.Create, "contact", metadataService);
			var writer = new JavascriptFormScriptWriter(context);

			// Act
			var result = writer.GetConditionExpression(ConditionOperator.Equal, new[] { "value" }, "field");

			// Assert
			Assert.AreEqual("field === value", result);
		}

		[TestMethod]
		public void GetConditionExpression_ReturnsNotEqualExpression()
		{
			// Arrange
			var metadataService = A.Fake<IMetadataService>();
			var context = new WriterContext("Test", false, MessageName.Create, "contact", metadataService);
			var writer = new JavascriptFormScriptWriter(context);

			// Act
			var result = writer.GetConditionExpression(ConditionOperator.NotEqual, new[] { "value" }, "field");

			// Assert
			Assert.AreEqual("field !== value", result);
		}

		[TestMethod]
		public void CopyEntityId_CopiesEntityExpression()
		{
			// Arrange
			var metadataService = A.Fake<IMetadataService>();
			var context = new WriterContext("Test", false, MessageName.Create, "contact", metadataService);
			var writer = new JavascriptFormScriptWriter(context);
			var source = new JSFSEntityVariable { EntityExpression = "formContext" };
			var target = new JSFSEntityVariable();

			// Act
			writer.CopyEntityId(source, target);

			// Assert
			Assert.AreEqual("formContext", target.EntityExpression);
		}

		[TestMethod]
		public void CopyEntityVariableValues_CopiesColumnExpressions()
		{
			// Arrange
			var metadataService = A.Fake<IMetadataService>();
			var context = new WriterContext("Test", false, MessageName.Create, "contact", metadataService);
			var writer = new JavascriptFormScriptWriter(context);
			var source = new JSFSEntityVariable();
			source.ColumnExpressions["firstname"] = "John";
			source.ColumnExpressions["lastname"] = "Doe";
			var target = new JSFSEntityVariable();

			// Act
			writer.CopyEntityVariableValues(source, target);

			// Assert
			Assert.AreEqual(2, target.ColumnExpressions.Count);
			Assert.AreEqual("John", target.ColumnExpressions["firstname"]);
			Assert.AreEqual("Doe", target.ColumnExpressions["lastname"]);
		}

		[TestMethod]
		public void GetEntityPropertyExpression_ReturnsGetValueExpression()
		{
			// Arrange
			var metadataService = A.Fake<IMetadataService>();
			var context = new WriterContext("Test", false, MessageName.Create, "contact", metadataService);
			var writer = new JavascriptFormScriptWriter(context);
			var entity = new JSFSEntityVariable { EntityExpression = "formContext" };

			// Act
			var result = writer.GetEntityPropertyExpresson(entity, "firstname");

			// Assert
			Assert.IsTrue(result.Contains("getAttribute"));
			Assert.IsTrue(result.Contains("getValue"));
			Assert.IsTrue(result.Contains("firstname"));
		}

		[TestMethod]
		public void JSFSEntityVariable_CanSetEntityExpression()
		{
			// Arrange & Act
			var entity = new JSFSEntityVariable { EntityExpression = "test" };

			// Assert
			Assert.AreEqual("test", entity.EntityExpression);
		}

		[TestMethod]
		public void JSFSEntityVariable_InitializesWithEmptyColumnExpressions()
		{
			// Arrange & Act
			var entity = new JSFSEntityVariable();

			// Assert
			Assert.IsNotNull(entity.ColumnExpressions);
			Assert.AreEqual(0, entity.ColumnExpressions.Count);
		}
	}
}
