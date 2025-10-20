using FakeItEasy;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Metadata;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using WorkflowModerniser.Inputs;
using WorkflowModerniser.Outputs.LowCodeCodePlugins;

namespace WorkflowModerniser.Tests
{
	[TestClass]
	public class LowCodePluginPowerFxWriterTests
	{
		private IMetadataService CreateFakeMetadataService()
		{
			var metadataService = A.Fake<IMetadataService>();

			// Setup contact metadata
			var contactMetadata = new EntityMetadata
			{
				LogicalName = "contact",
				PrimaryIdAttribute = "contactid",
				DisplayCollectionName = new Label(new LocalizedLabel("Contacts", 1033), null),
			};
			contactMetadata.SetSealedPropertyValue("Attributes", new AttributeMetadata[]
			{
				new StringAttributeMetadata
				{
					LogicalName = "contactid",
					DisplayName = new Label(new LocalizedLabel("Contact", 1033), null)
				},
				new StringAttributeMetadata
				{
					LogicalName = "firstname",
					DisplayName = new Label(new LocalizedLabel("First Name", 1033), null)
				},
				new StringAttributeMetadata
				{
					LogicalName = "lastname",
					DisplayName = new Label(new LocalizedLabel("Last Name", 1033), null)
				}
			});

			A.CallTo(() => metadataService.GetEntityMetadata("contact")).Returns(contactMetadata);

			return metadataService;
		}

		[TestMethod]
		public void LoadPrimaryEntity_ReturnsPrimaryEntityVariable()
		{
			// Arrange
			var metadataService = CreateFakeMetadataService();
			var context = new WriterContext("Test Workflow", false, MessageName.Create, "contact", metadataService);
			var writer = new LowCodePluginPowerFxWriter(context);

			// Act
			var result = writer.LoadPrimaryEntity("contact");

			// Assert
			Assert.IsNotNull(result);
			Assert.AreEqual("contact", result.TableName);
			Assert.AreEqual("ThisRecord", result.RecordExpression);
			Assert.IsTrue(result.IsPrimary);
			Assert.AreEqual("ThisRecord.'Contact'", result.IdExpression);
		}

		[TestMethod]
		public void NewEntityVariable_ReturnsNewVariable()
		{
			// Arrange
			var metadataService = CreateFakeMetadataService();
			var context = new WriterContext("Test Workflow", false, MessageName.Create, "contact", metadataService);
			var writer = new LowCodePluginPowerFxWriter(context);

			// Act
			var result = writer.NewEntityVariable("contact");

			// Assert
			Assert.IsNotNull(result);
			Assert.AreEqual("contact", result.TableName);
			Assert.IsNull(result.RecordExpression);
		}

		[TestMethod]
		public void GetLiteral_ReturnsBlankForNull()
		{
			// Arrange
			var metadataService = CreateFakeMetadataService();
			var context = new WriterContext("Test Workflow", false, MessageName.Create, "contact", metadataService);
			var writer = new LowCodePluginPowerFxWriter(context);

			// Act
			var result = writer.GetLiteral(null);

			// Assert
			Assert.AreEqual("Blank()", result);
		}

		[TestMethod]
		public void GetLiteral_ReturnsTrueForBoolean()
		{
			// Arrange
			var metadataService = CreateFakeMetadataService();
			var context = new WriterContext("Test Workflow", false, MessageName.Create, "contact", metadataService);
			var writer = new LowCodePluginPowerFxWriter(context);

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
			var metadataService = CreateFakeMetadataService();
			var context = new WriterContext("Test Workflow", false, MessageName.Create, "contact", metadataService);
			var writer = new LowCodePluginPowerFxWriter(context);

			// Act
			var result = writer.GetLiteral("Hello World");

			// Assert
			Assert.AreEqual("\"Hello World\"", result);
		}

		[TestMethod]
		public void GetLiteral_EscapesQuotesInString()
		{
			// Arrange
			var metadataService = CreateFakeMetadataService();
			var context = new WriterContext("Test Workflow", false, MessageName.Create, "contact", metadataService);
			var writer = new LowCodePluginPowerFxWriter(context);

			// Act
			var result = writer.GetLiteral("He said \"Hello\"");

			// Assert
			Assert.AreEqual("\"He said \"\"Hello\"\"\"", result);
		}

		[TestMethod]
		public void GetLiteral_ReturnsQuotedGuidForGuid()
		{
			// Arrange
			var metadataService = CreateFakeMetadataService();
			var context = new WriterContext("Test Workflow", false, MessageName.Create, "contact", metadataService);
			var writer = new LowCodePluginPowerFxWriter(context);
			var guid = Guid.NewGuid();

			// Act
			var result = writer.GetLiteral(guid);

			// Assert
			Assert.AreEqual($"\"{guid}\"", result);
		}

		[TestMethod]
		public void GetLiteral_ReturnsOptionSetValueFormat()
		{
			// Arrange
			var metadataService = CreateFakeMetadataService();
			var context = new WriterContext("Test Workflow", false, MessageName.Create, "contact", metadataService);
			var writer = new LowCodePluginPowerFxWriter(context);
			var optionSetValue = new OptionSetValue(1);

			// Act
			var result = writer.GetLiteral(optionSetValue);

			// Assert
			Assert.AreEqual("OptionSetValue:1", result);
		}

		[TestMethod]
		public void GetLiteral_ReturnsLookupForEntityReference()
		{
			// Arrange
			var metadataService = CreateFakeMetadataService();
			var context = new WriterContext("Test Workflow", false, MessageName.Create, "contact", metadataService);
			var writer = new LowCodePluginPowerFxWriter(context);
			var guid = Guid.NewGuid();
			var entityRef = new EntityReference("contact", guid);

			// Act
			var result = writer.GetLiteral(entityRef);

			// Assert
			Assert.IsTrue(result.Contains("LookUp"));
			Assert.IsTrue(result.Contains(guid.ToString()));
		}

		[TestMethod]
		public void GetLiteral_ReturnsEmptyObjectForEmptyDictionary()
		{
			// Arrange
			var metadataService = CreateFakeMetadataService();
			var context = new WriterContext("Test Workflow", false, MessageName.Create, "contact", metadataService);
			var writer = new LowCodePluginPowerFxWriter(context);

			// Act
			var result = writer.GetLiteral(new Dictionary<string, object>());

			// Assert
			Assert.AreEqual("{}", result);
		}

		[TestMethod]
		public void GetLiteral_ReturnsNumberForInteger()
		{
			// Arrange
			var metadataService = CreateFakeMetadataService();
			var context = new WriterContext("Test Workflow", false, MessageName.Create, "contact", metadataService);
			var writer = new LowCodePluginPowerFxWriter(context);

			// Act
			var result = writer.GetLiteral(42);

			// Assert
			Assert.AreEqual("42", result);
		}

		[TestMethod]
		public void GetConditionExpression_ReturnsEqualExpression()
		{
			// Arrange
			var metadataService = CreateFakeMetadataService();
			var context = new WriterContext("Test Workflow", false, MessageName.Create, "contact", metadataService);
			var writer = new LowCodePluginPowerFxWriter(context);

			// Act
			var result = writer.GetConditionExpression(ConditionOperator.Equal, new[] { "value" }, "field");

			// Assert
			Assert.AreEqual("field = value", result);
		}

		[TestMethod]
		public void GetConditionExpression_ReturnsNotEqualExpression()
		{
			// Arrange
			var metadataService = CreateFakeMetadataService();
			var context = new WriterContext("Test Workflow", false, MessageName.Create, "contact", metadataService);
			var writer = new LowCodePluginPowerFxWriter(context);

			// Act
			var result = writer.GetConditionExpression(ConditionOperator.NotEqual, new[] { "value" }, "field");

			// Assert
			Assert.AreEqual("field != value", result);
		}

		[TestMethod]
		public void GetConditionExpression_ReturnsNullExpression()
		{
			// Arrange
			var metadataService = CreateFakeMetadataService();
			var context = new WriterContext("Test Workflow", false, MessageName.Create, "contact", metadataService);
			var writer = new LowCodePluginPowerFxWriter(context);

			// Act
			var result = writer.GetConditionExpression(ConditionOperator.Null, null, "field");

			// Assert
			Assert.AreEqual("IsBlank(field)", result);
		}

		[TestMethod]
		public void GetConditionExpression_ReturnsNotNullExpression()
		{
			// Arrange
			var metadataService = CreateFakeMetadataService();
			var context = new WriterContext("Test Workflow", false, MessageName.Create, "contact", metadataService);
			var writer = new LowCodePluginPowerFxWriter(context);

			// Act
			var result = writer.GetConditionExpression(ConditionOperator.NotNull, null, "field");

			// Assert
			Assert.AreEqual("!IsBlank(field)", result);
		}

		[TestMethod]
		public void GetConditionExpression_ReturnsGreaterThanExpression()
		{
			// Arrange
			var metadataService = CreateFakeMetadataService();
			var context = new WriterContext("Test Workflow", false, MessageName.Create, "contact", metadataService);
			var writer = new LowCodePluginPowerFxWriter(context);

			// Act
			var result = writer.GetConditionExpression(ConditionOperator.GreaterThan, new[] { "10" }, "field");

			// Assert
			Assert.AreEqual("field > 10", result);
		}

		[TestMethod]
		public void GetConditionExpression_ReturnsLessThanExpression()
		{
			// Arrange
			var metadataService = CreateFakeMetadataService();
			var context = new WriterContext("Test Workflow", false, MessageName.Create, "contact", metadataService);
			var writer = new LowCodePluginPowerFxWriter(context);

			// Act
			var result = writer.GetConditionExpression(ConditionOperator.LessThan, new[] { "10" }, "field");

			// Assert
			Assert.AreEqual("field < 10", result);
		}

		[TestMethod]
		public void GetConditionExpression_ReturnsBeginsWithExpression()
		{
			// Arrange
			var metadataService = CreateFakeMetadataService();
			var context = new WriterContext("Test Workflow", false, MessageName.Create, "contact", metadataService);
			var writer = new LowCodePluginPowerFxWriter(context);

			// Act
			var result = writer.GetConditionExpression(ConditionOperator.BeginsWith, new[] { "\"prefix\"" }, "field");

			// Assert
			Assert.AreEqual("StartsWith(field, \"prefix\")", result);
		}

		[TestMethod]
		public void GetConditionExpression_ReturnsEndsWithExpression()
		{
			// Arrange
			var metadataService = CreateFakeMetadataService();
			var context = new WriterContext("Test Workflow", false, MessageName.Create, "contact", metadataService);
			var writer = new LowCodePluginPowerFxWriter(context);

			// Act
			var result = writer.GetConditionExpression(ConditionOperator.EndsWith, new[] { "\"suffix\"" }, "field");

			// Assert
			Assert.AreEqual("EndsWith(field, \"suffix\")", result);
		}

		[TestMethod]
		public void GetCoalesceExpression_ReturnsCoalesceFunction()
		{
			// Arrange
			var metadataService = CreateFakeMetadataService();
			var context = new WriterContext("Test Workflow", false, MessageName.Create, "contact", metadataService);
			var writer = new LowCodePluginPowerFxWriter(context);

			// Act
			var result = writer.GetCoalesceExpression(new[] { "field1", "field2", "field3" });

			// Assert
			Assert.AreEqual("Coalesce(field1,field2,field3)", result);
		}

		[TestMethod]
		public void GetLogicalConditionExpression_ReturnsAndExpression()
		{
			// Arrange
			var metadataService = CreateFakeMetadataService();
			var context = new WriterContext("Test Workflow", false, MessageName.Create, "contact", metadataService);
			var writer = new LowCodePluginPowerFxWriter(context);

			// Act
			var result = writer.GetLogicalConditionExpression(LogicalOperator.And, "condition1", "condition2");

			// Assert
			Assert.AreEqual("(condition1 && condition2)", result);
		}

		[TestMethod]
		public void GetLogicalConditionExpression_ReturnsOrExpression()
		{
			// Arrange
			var metadataService = CreateFakeMetadataService();
			var context = new WriterContext("Test Workflow", false, MessageName.Create, "contact", metadataService);
			var writer = new LowCodePluginPowerFxWriter(context);

			// Act
			var result = writer.GetLogicalConditionExpression(LogicalOperator.Or, "condition1", "condition2");

			// Assert
			Assert.AreEqual("(condition1 || condition2)", result);
		}

		[TestMethod]
		public void CopyEntityId_CopiesIdAndPrimaryFlag()
		{
			// Arrange
			var metadataService = CreateFakeMetadataService();
			var context = new WriterContext("Test Workflow", false, MessageName.Create, "contact", metadataService);
			var writer = new LowCodePluginPowerFxWriter(context);
			var source = new LCPEntityVariable("contact", "SourceRecord")
			{
				IdExpression = "SourceRecord.contactid",
				IsPrimary = true
			};
			var target = new LCPEntityVariable("contact", "TargetRecord");

			// Act
			writer.CopyEntityId(source, target);

			// Assert
			Assert.AreEqual("SourceRecord.contactid", target.IdExpression);
			Assert.IsTrue(target.IsPrimary);
		}

		[TestMethod]
		public void CloneEntityVariable_CreatesNewInstanceWithSameColumns()
		{
			// Arrange
			var metadataService = CreateFakeMetadataService();
			var context = new WriterContext("Test Workflow", false, MessageName.Create, "contact", metadataService);
			var writer = new LowCodePluginPowerFxWriter(context);
			var source = new LCPEntityVariable("contact", "SourceRecord");
			source.ColumnExpressions["firstname"] = "John";
			source.ColumnExpressions["lastname"] = "Doe";

			// Act
			var clone = writer.CloneEntityVariable(source);

			// Assert
			Assert.AreNotSame(source, clone);
			Assert.AreEqual("contact", clone.TableName);
			Assert.AreSame(source.ColumnExpressions, clone.ColumnExpressions);
		}

		[TestMethod]
		public void SetEntityProperty_AddsColumnExpression()
		{
			// Arrange
			var metadataService = CreateFakeMetadataService();
			var context = new WriterContext("Test Workflow", false, MessageName.Create, "contact", metadataService);
			var writer = new LowCodePluginPowerFxWriter(context);
			var entity = new LCPEntityVariable("contact", "ThisRecord");

			// Act
			writer.SetEntityProperty(entity, "firstname", "\"John\"");

			// Assert
			Assert.AreEqual(1, entity.ColumnExpressions.Count);
			Assert.AreEqual("\"John\"", entity.ColumnExpressions["firstname"]);
		}

		[TestMethod]
		public void CopyEntityVariableValues_CopiesColumnExpressions()
		{
			// Arrange
			var metadataService = CreateFakeMetadataService();
			var context = new WriterContext("Test Workflow", false, MessageName.Create, "contact", metadataService);
			var writer = new LowCodePluginPowerFxWriter(context);
			var source = new LCPEntityVariable("contact", "SourceRecord");
			source.ColumnExpressions["firstname"] = "John";
			source.ColumnExpressions["lastname"] = "Doe";
			var target = new LCPEntityVariable("contact", "TargetRecord");

			// Act
			writer.CopyEntityVariableValues(source, target);

			// Assert
			Assert.AreSame(source.ColumnExpressions, target.ColumnExpressions);
		}

		[TestMethod]
		public void GetEntityPropertyExpression_ReturnsFieldExpression()
		{
			// Arrange
			var metadataService = CreateFakeMetadataService();
			var context = new WriterContext("Test Workflow", false, MessageName.Create, "contact", metadataService);
			var writer = new LowCodePluginPowerFxWriter(context);
			var entity = new LCPEntityVariable("contact", "ThisRecord");

			// Act
			var result = writer.GetEntityPropertyExpresson(entity, "firstname");

			// Assert
			Assert.AreEqual("ThisRecord.'First Name'", result);
		}
	}
}
