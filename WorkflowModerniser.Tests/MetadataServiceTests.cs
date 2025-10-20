using FakeItEasy;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Extensions;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using System.Collections.Generic;
using WorkflowModerniser.Inputs;

namespace WorkflowModerniser.Tests
{
	[TestClass]
	public class MetadataServiceTests
	{
		[TestMethod]
		public void GetEntityMetadata_ReturnsMetadata_WhenEntityExists()
		{
			// Arrange
			var fakeOrgService = A.Fake<IOrganizationService>();
			var entityName = "contact";
			var expectedMetadata = new EntityMetadata { LogicalName = entityName };

			A.CallTo(() => fakeOrgService.Execute(A<OrganizationRequest>.That.Matches(
				r => r is RetrieveEntityRequest && ((RetrieveEntityRequest)r).LogicalName == entityName)))
				.Returns(new RetrieveEntityResponse { Results = new ParameterCollection { { "EntityMetadata", expectedMetadata } } });

			var metadataService = new MetadataService(fakeOrgService);

			// Act
			var result = metadataService.GetEntityMetadata(entityName);

			// Assert
			Assert.IsNotNull(result);
			Assert.AreEqual(entityName, result.LogicalName);
		}

		[TestMethod]
		public void GetEntityMetadata_CachesResults_OnSecondCall()
		{
			// Arrange
			var fakeOrgService = A.Fake<IOrganizationService>();
			var entityName = "contact";
			var expectedMetadata = new EntityMetadata { LogicalName = entityName };

			A.CallTo(() => fakeOrgService.Execute(A<OrganizationRequest>.That.Matches(
				r => r is RetrieveEntityRequest && ((RetrieveEntityRequest)r).LogicalName == entityName)))
				.Returns(new RetrieveEntityResponse { Results = new ParameterCollection { { "EntityMetadata", expectedMetadata } } });

			var metadataService = new MetadataService(fakeOrgService);

			// Act
			var result1 = metadataService.GetEntityMetadata(entityName);
			var result2 = metadataService.GetEntityMetadata(entityName);

			// Assert
			Assert.AreSame(result1, result2, "Second call should return cached instance");
			A.CallTo(() => fakeOrgService.Execute(A<OrganizationRequest>.Ignored)).MustHaveHappenedOnceExactly();
		}

		[TestMethod]
		public void GetEntityMetadata_HandlesDifferentEntities()
		{
			// Arrange
			var fakeOrgService = A.Fake<IOrganizationService>();
			var metadataService = new MetadataService(fakeOrgService);

			var contactMetadata = new EntityMetadata { LogicalName = "contact" };
			var accountMetadata = new EntityMetadata { LogicalName = "account" };

			A.CallTo(() => fakeOrgService.Execute(A<OrganizationRequest>.That.Matches(
				r => r is RetrieveEntityRequest && ((RetrieveEntityRequest)r).LogicalName == "contact")))
				.Returns(new RetrieveEntityResponse { Results = new ParameterCollection { { "EntityMetadata", contactMetadata } } });

			A.CallTo(() => fakeOrgService.Execute(A<OrganizationRequest>.That.Matches(
				r => r is RetrieveEntityRequest && ((RetrieveEntityRequest)r).LogicalName == "account")))
				.Returns(new RetrieveEntityResponse { Results = new ParameterCollection { { "EntityMetadata", accountMetadata } } });

			// Act
			var result1 = metadataService.GetEntityMetadata("contact");
			var result2 = metadataService.GetEntityMetadata("account");
			var result3 = metadataService.GetEntityMetadata("contact");

			// Assert
			Assert.AreEqual("contact", result1.LogicalName);
			Assert.AreEqual("account", result2.LogicalName);
			Assert.AreSame(result1, result3, "Cached contact metadata should be returned");
			A.CallTo(() => fakeOrgService.Execute(A<OrganizationRequest>.Ignored)).MustHaveHappenedTwiceExactly();
		}
	}
}
