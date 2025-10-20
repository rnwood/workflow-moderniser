using FakeItEasy;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WorkflowModerniser.Inputs;

namespace WorkflowModerniser.Tests
{
	[TestClass]
	public class WriterContextTests
	{
		[TestMethod]
		public void WriterContext_StoresWorkflowName()
		{
			// Arrange
			var metadataService = A.Fake<IMetadataService>();
			var workflowName = "Test Workflow";

			// Act
			var context = new WriterContext(workflowName, false, MessageName.Create, "contact", metadataService);

			// Assert
			Assert.AreEqual(workflowName, context.WorkflowName);
		}

		[TestMethod]
		public void WriterContext_StoresIsPreOperation()
		{
			// Arrange
			var metadataService = A.Fake<IMetadataService>();

			// Act - Pre-operation
			var preOpContext = new WriterContext("Test", true, MessageName.Create, "contact", metadataService);
			var postOpContext = new WriterContext("Test", false, MessageName.Create, "contact", metadataService);

			// Assert
			Assert.IsTrue(preOpContext.IsPreOperation);
			Assert.IsFalse(postOpContext.IsPreOperation);
		}

		[TestMethod]
		public void WriterContext_StoresMessageNames()
		{
			// Arrange
			var metadataService = A.Fake<IMetadataService>();

			// Act
			var context = new WriterContext("Test", false, MessageName.Create | MessageName.Update, "contact", metadataService);

			// Assert
			Assert.AreEqual(MessageName.Create | MessageName.Update, context.MessageNames);
		}

		[TestMethod]
		public void WriterContext_StoresPrimaryEntityName()
		{
			// Arrange
			var metadataService = A.Fake<IMetadataService>();
			var entityName = "contact";

			// Act
			var context = new WriterContext("Test", false, MessageName.Create, entityName, metadataService);

			// Assert
			Assert.AreEqual(entityName, context.PrimaryEntityName);
		}

		[TestMethod]
		public void WriterContext_StoresMetadataService()
		{
			// Arrange
			var metadataService = A.Fake<IMetadataService>();

			// Act
			var context = new WriterContext("Test", false, MessageName.Create, "contact", metadataService);

			// Assert
			Assert.AreSame(metadataService, context.MetadataService);
		}

		[TestMethod]
		public void MessageName_SupportsFlags()
		{
			// Arrange & Act
			var createUpdate = MessageName.Create | MessageName.Update;
			var createUpdateDelete = MessageName.Create | MessageName.Update | MessageName.Delete;

			// Assert
			Assert.IsTrue(createUpdate.HasFlag(MessageName.Create));
			Assert.IsTrue(createUpdate.HasFlag(MessageName.Update));
			Assert.IsFalse(createUpdate.HasFlag(MessageName.Delete));

			Assert.IsTrue(createUpdateDelete.HasFlag(MessageName.Create));
			Assert.IsTrue(createUpdateDelete.HasFlag(MessageName.Update));
			Assert.IsTrue(createUpdateDelete.HasFlag(MessageName.Delete));
			Assert.IsFalse(createUpdateDelete.HasFlag(MessageName.Action));
		}
	}
}
