using FakeItEasy;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Metadata;
using System;
using System.Collections.Generic;
using System.Linq;
using WorkflowModerniser.Data;
using WorkflowModerniser.Inputs;
using WorkflowModerniser.Outputs;
using WorkflowModerniser.Outputs.LowCodeCodePlugins;

namespace WorkflowModerniser.Tests
{
	[TestClass]
	public class WorkflowConverterTests
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
				}
			});

			A.CallTo(() => metadataService.GetEntityMetadata("contact")).Returns(contactMetadata);

			return metadataService;
		}

		[TestMethod]
		public void WorkflowConverter_Constructor_InitializesWithDependencies()
		{
			// Arrange
			var workflowSource = A.Fake<IWorkflowSource>();
			var metadataService = CreateFakeMetadataService();
			Func<WriterContext, IWorkflowWriter<LCPEntityVariable>> writerFactory = 
				(ctx) => new LowCodePluginPowerFxWriter(ctx);

			// Act
			var converter = new WorkflowConverter<LCPEntityVariable>(
				workflowSource, 
				metadataService, 
				writerFactory);

			// Assert
			Assert.IsNotNull(converter);
		}

		[TestMethod]
		public void WorkflowConverter_Convert_ThrowsForUnsupportedWorkflowType()
		{
			// Arrange
			var workflowSource = A.Fake<IWorkflowSource>();
			var metadataService = CreateFakeMetadataService();
			Func<WriterContext, IWorkflowWriter<LCPEntityVariable>> writerFactory =
				(ctx) => new LowCodePluginPowerFxWriter(ctx);

			var converter = new WorkflowConverter<LCPEntityVariable>(
				workflowSource,
				metadataService,
				writerFactory);

			var workflow = new Data.Workflow
			{
				Category = Workflow_Category.Dialog, // Unsupported type
				Name = "Test Workflow",
				PrimaryEntityName = "contact"
			};

			// Act & Assert
			Assert.ThrowsException<NotSupportedException>(() => converter.Convert(workflow));
		}

		[TestMethod]
		public void WorkflowConverter_Convert_HandlesBusinessRuleCategory()
		{
			// Arrange
			var workflowSource = A.Fake<IWorkflowSource>();
			var metadataService = CreateFakeMetadataService();
			Func<WriterContext, IWorkflowWriter<LCPEntityVariable>> writerFactory =
				(ctx) => new LowCodePluginPowerFxWriter(ctx);

			var converter = new WorkflowConverter<LCPEntityVariable>(
				workflowSource,
				metadataService,
				writerFactory);

			// Create a minimal business rule workflow
			var workflow = new Data.Workflow
			{
				Category = Workflow_Category.BusinessRule,
				Name = "Test Business Rule",
				PrimaryEntityName = "contact",
				PrimaryEntity = "contact",
				XAMl = CreateMinimalWorkflowXaml()
			};

			// Act
			var result = converter.Convert(workflow);

			// Assert
			Assert.IsNotNull(result);
			// Business rules should generate outputs
			var outputs = result.ToList();
			Assert.IsTrue(outputs.Count > 0);
		}

		[TestMethod]
		public void WorkflowConverter_Convert_HandlesWorkflowWithCreateStage()
		{
			// Arrange
			var workflowSource = A.Fake<IWorkflowSource>();
			var metadataService = CreateFakeMetadataService();
			Func<WriterContext, IWorkflowWriter<LCPEntityVariable>> writerFactory =
				(ctx) => new LowCodePluginPowerFxWriter(ctx);

			var converter = new WorkflowConverter<LCPEntityVariable>(
				workflowSource,
				metadataService,
				writerFactory);

			// Create a workflow with create stage
			var workflow = new Data.Workflow
			{
				Category = Workflow_Category.Workflow,
				Name = "Test Workflow",
				PrimaryEntityName = "contact",
				PrimaryEntity = "contact",
				CreateStage = Workflow_Stage.PostOperation,
				XAMl = CreateMinimalWorkflowXaml()
			};

			// Act
			var result = converter.Convert(workflow);

			// Assert
			Assert.IsNotNull(result);
			var outputs = result.ToList();
			Assert.IsTrue(outputs.Count > 0);
		}

		[TestMethod]
		public void WorkflowConverter_Convert_HandlesWorkflowWithUpdateStage()
		{
			// Arrange
			var workflowSource = A.Fake<IWorkflowSource>();
			var metadataService = CreateFakeMetadataService();
			Func<WriterContext, IWorkflowWriter<LCPEntityVariable>> writerFactory =
				(ctx) => new LowCodePluginPowerFxWriter(ctx);

			var converter = new WorkflowConverter<LCPEntityVariable>(
				workflowSource,
				metadataService,
				writerFactory);

			// Create a workflow with update stage
			var workflow = new Data.Workflow
			{
				Category = Workflow_Category.Workflow,
				Name = "Test Workflow",
				PrimaryEntityName = "contact",
				PrimaryEntity = "contact",
				UpdateStage = Workflow_Stage.PreOperation,
				XAMl = CreateMinimalWorkflowXaml()
			};

			// Act
			var result = converter.Convert(workflow);

			// Assert
			Assert.IsNotNull(result);
			var outputs = result.ToList();
			Assert.IsTrue(outputs.Count > 0);
		}

		[TestMethod]
		public void WorkflowConverter_Convert_HandlesWorkflowWithDeleteStage()
		{
			// Arrange
			var workflowSource = A.Fake<IWorkflowSource>();
			var metadataService = CreateFakeMetadataService();
			Func<WriterContext, IWorkflowWriter<LCPEntityVariable>> writerFactory =
				(ctx) => new LowCodePluginPowerFxWriter(ctx);

			var converter = new WorkflowConverter<LCPEntityVariable>(
				workflowSource,
				metadataService,
				writerFactory);

			// Create a workflow with delete stage
			var workflow = new Data.Workflow
			{
				Category = Workflow_Category.Workflow,
				Name = "Test Workflow",
				PrimaryEntityName = "contact",
				PrimaryEntity = "contact",
				DeleteStage = Workflow_Stage.PostOperation,
				XAMl = CreateMinimalWorkflowXaml()
			};

			// Act
			var result = converter.Convert(workflow);

			// Assert
			Assert.IsNotNull(result);
			var outputs = result.ToList();
			Assert.IsTrue(outputs.Count > 0);
		}

		[TestMethod]
		public void WorkflowConverter_Convert_HandlesSubprocess()
		{
			// Arrange
			var workflowSource = A.Fake<IWorkflowSource>();
			var metadataService = CreateFakeMetadataService();
			Func<WriterContext, IWorkflowWriter<LCPEntityVariable>> writerFactory =
				(ctx) => new LowCodePluginPowerFxWriter(ctx);

			var converter = new WorkflowConverter<LCPEntityVariable>(
				workflowSource,
				metadataService,
				writerFactory);

			// Create a subprocess workflow
			var workflow = new Data.Workflow
			{
				Category = Workflow_Category.Workflow,
				Name = "Test Subprocess",
				PrimaryEntityName = "contact",
				PrimaryEntity = "contact",
				Subprocess = true,
				XAMl = CreateMinimalWorkflowXaml()
			};

			// Act
			var result = converter.Convert(workflow);

			// Assert
			Assert.IsNotNull(result);
			var outputs = result.ToList();
			Assert.IsTrue(outputs.Count > 0);
			// Subprocess should generate an action (InstantPlugin)
			Assert.IsTrue(outputs.Any(o => o is InstantPlugin));
		}

		[TestMethod]
		public void WorkflowConverter_Convert_HandlesMultipleStages()
		{
			// Arrange
			var workflowSource = A.Fake<IWorkflowSource>();
			var metadataService = CreateFakeMetadataService();
			Func<WriterContext, IWorkflowWriter<LCPEntityVariable>> writerFactory =
				(ctx) => new LowCodePluginPowerFxWriter(ctx);

			var converter = new WorkflowConverter<LCPEntityVariable>(
				workflowSource,
				metadataService,
				writerFactory);

			// Create a workflow with multiple stages
			var workflow = new Data.Workflow
			{
				Category = Workflow_Category.Workflow,
				Name = "Multi-Stage Workflow",
				PrimaryEntityName = "contact",
				PrimaryEntity = "contact",
				CreateStage = Workflow_Stage.PreOperation,
				UpdateStage = Workflow_Stage.PostOperation,
				XAMl = CreateMinimalWorkflowXaml()
			};

			// Act
			var result = converter.Convert(workflow);

			// Assert
			Assert.IsNotNull(result);
			var outputs = result.ToList();
			// Should generate outputs for both stages
			Assert.IsTrue(outputs.Count >= 2);
		}

		/// <summary>
		/// Creates minimal valid workflow XAML for testing
		/// </summary>
		private string CreateMinimalWorkflowXaml()
		{
			return @"<Activity x:Class=""Microsoft.Crm.Workflow.Activities.Workflow"" 
xmlns=""http://schemas.microsoft.com/netfx/2009/xaml/activities"" 
xmlns:mva=""clr-namespace:Microsoft.VisualBasic.Activities;assembly=System.Activities, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35"" 
xmlns:mxsw=""clr-namespace:Microsoft.Xrm.Sdk.Workflow;assembly=Microsoft.Xrm.Sdk.Workflow, Version=9.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35"" 
xmlns:s=""clr-namespace:System;assembly=mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089"" 
xmlns:scg=""clr-namespace:System.Collections.Generic;assembly=System, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089"" 
xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml"">
  <x:Members>
    <x:Property Name=""InputEntities"" Type=""InArgument(scg:IDictionary(x:String, mxsw:WorkflowEntity))"" />
    <x:Property Name=""CreatedEntities"" Type=""InArgument(scg:IDictionary(x:String, mxsw:WorkflowEntity))"" />
  </x:Members>
  <mva:VisualBasic.Settings>Assembly references and imported namespaces for internal implementation</mva:VisualBasic.Settings>
  <mxsw:Workflow.Attributes>
    <mxsw:WorkflowAttribute Name=""Contact"" />
  </mxsw:Workflow.Attributes>
  <Sequence />
</Activity>";
		}
	}
}
