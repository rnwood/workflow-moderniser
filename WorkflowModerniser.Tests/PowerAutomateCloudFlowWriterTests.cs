using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xrm.Sdk;
using System;
using WorkflowModerniser.Outputs.PowerAutomateCloudFlow;

namespace WorkflowModerniser.Tests
{
	[TestClass]
	public class PowerAutomateCloudFlowWriterTests
	{
		[TestMethod]
		public void Constructor_InitializesWriter()
		{
			// Arrange & Act
			var writer = new PowerAutomateCloudFlowWriter();

			// Assert
			Assert.IsNotNull(writer);
		}

		[TestMethod]
		public void LoadPrimaryEntity_ReturnsEntityVariable()
		{
			// Arrange
			var writer = new PowerAutomateCloudFlowWriter();

			// Act
			var result = writer.LoadPrimaryEntity("contact");

			// Assert
			Assert.IsNotNull(result);
			Assert.IsTrue(result.RecordExpression.Contains("triggerOutputs"));
			Assert.IsTrue(result.IdExpression.Contains("contactid"));
		}

		[TestMethod]
		public void NewEntityVariable_ReturnsNewVariable()
		{
			// Arrange
			var writer = new PowerAutomateCloudFlowWriter();

			// Act
			var result = writer.NewEntityVariable("contact");

			// Assert
			Assert.IsNotNull(result);
		}

		[TestMethod]
		public void GetLiteral_ReturnsNullForNull()
		{
			// Arrange
			var writer = new PowerAutomateCloudFlowWriter();

			// Act
			var result = writer.GetLiteral(null);

			// Assert
			Assert.AreEqual("null", result);
		}

		[TestMethod]
		public void GetLiteral_ReturnsTrueForBoolean()
		{
			// Arrange
			var writer = new PowerAutomateCloudFlowWriter();

			// Act
			var resultTrue = writer.GetLiteral(true);
			var resultFalse = writer.GetLiteral(false);

			// Assert
			Assert.AreEqual("True", resultTrue);
			Assert.AreEqual("False", resultFalse);
		}

		[TestMethod]
		public void GetLiteral_ReturnsQuotedGuidForGuid()
		{
			// Arrange
			var writer = new PowerAutomateCloudFlowWriter();
			var guid = Guid.NewGuid();

			// Act
			var result = writer.GetLiteral(guid);

			// Assert
			Assert.IsTrue(result.Contains(guid.ToString()));
			Assert.IsTrue(result.StartsWith("'"));
			Assert.IsTrue(result.EndsWith("'"));
		}

		[TestMethod]
		public void GetLiteral_ReturnsQuotedStringForString()
		{
			// Arrange
			var writer = new PowerAutomateCloudFlowWriter();

			// Act
			var result = writer.GetLiteral("test value");

			// Assert
			Assert.AreEqual("'test value'", result);
		}

		[TestMethod]
		public void GetLiteral_EscapesSingleQuotesInString()
		{
			// Arrange
			var writer = new PowerAutomateCloudFlowWriter();

			// Act
			var result = writer.GetLiteral("It's a test");

			// Assert
			Assert.IsTrue(result.Contains("''"));
		}

		[TestMethod]
		public void GetLiteral_ReturnsNumberForOptionSetValue()
		{
			// Arrange
			var writer = new PowerAutomateCloudFlowWriter();
			var optionSet = new OptionSetValue(1);

			// Act
			var result = writer.GetLiteral(optionSet);

			// Assert
			Assert.AreEqual("1", result);
		}

		[TestMethod]
		public void GetLiteral_ReturnsExpressionForEntityReference()
		{
			// Arrange
			var writer = new PowerAutomateCloudFlowWriter();
			var guid = Guid.NewGuid();
			var entityRef = new EntityReference("contact", guid);

			// Act
			var result = writer.GetLiteral(entityRef);

			// Assert
			Assert.IsTrue(result.Contains("contact"));
			Assert.IsTrue(result.Contains(guid.ToString()));
		}

		[TestMethod]
		public void SetEntityProperty_AddsToColumnExpressions()
		{
			// Arrange
			var writer = new PowerAutomateCloudFlowWriter();
			var entity = writer.NewEntityVariable("contact");

			// Act
			writer.SetEntityProperty(entity, "firstname", "'John'");

			// Assert
			Assert.AreEqual(1, entity.ColumnExpressions.Count);
			Assert.AreEqual("'John'", entity.ColumnExpressions["firstname"]);
		}

		[TestMethod]
		public void CopyEntityId_CopiesIdExpression()
		{
			// Arrange
			var writer = new PowerAutomateCloudFlowWriter();
			var source = new PACFEntityVariable("source", "sourceId");
			var target = new PACFEntityVariable("target", null);

			// Act
			writer.CopyEntityId(source, target);

			// Assert
			Assert.AreEqual("sourceId", target.IdExpression);
		}

		[TestMethod]
		public void CopyEntityVariableValues_CopiesColumnExpressions()
		{
			// Arrange
			var writer = new PowerAutomateCloudFlowWriter();
			var source = new PACFEntityVariable("source", "id");
			source.ColumnExpressions["firstname"] = "'John'";
			source.ColumnExpressions["lastname"] = "'Doe'";
			var target = new PACFEntityVariable("target", "id");

			// Act
			writer.CopyEntityVariableValues(source, target);

			// Assert
			Assert.AreEqual(2, target.ColumnExpressions.Count);
			Assert.AreEqual("'John'", target.ColumnExpressions["firstname"]);
			Assert.AreEqual("'Doe'", target.ColumnExpressions["lastname"]);
		}

		[TestMethod]
		public void PACFEntityVariable_Constructor_SetsProperties()
		{
			// Arrange & Act
			var entity = new PACFEntityVariable("recordExpr", "idExpr");

			// Assert
			Assert.AreEqual("recordExpr", entity.RecordExpression);
			Assert.AreEqual("idExpr", entity.IdExpression);
		}

		[TestMethod]
		public void PACFEntityVariable_InitializesWithEmptyColumnExpressions()
		{
			// Arrange & Act
			var entity = new PACFEntityVariable("record", "id");

			// Assert
			Assert.IsNotNull(entity.ColumnExpressions);
			Assert.AreEqual(0, entity.ColumnExpressions.Count);
		}
	}
}
