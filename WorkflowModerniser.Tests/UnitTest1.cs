using DG.Tools.XrmMockup;
using FakeItEasy;
using Microsoft.PowerFx;
using Microsoft.PowerFx.Dataverse;
using Microsoft.PowerFx.Types;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Extensions;
using Microsoft.Xrm.Sdk.Messages;
using System.Threading.Tasks;

namespace WorkflowModerniser.Tests
{
	/// <summary>
	/// Tests for PowerFx integration with Dataverse using XrmMockup
	/// This demonstrates that PowerFx expressions can be evaluated against Dataverse
	/// </summary>
	[TestClass]
	public class PowerFxIntegrationTests
	{
		[TestMethod]
		public async Task PowerFx_CanEvaluateSimpleExpression_WithDataverseConnection()
		{
			// Arrange
			Features toenable = Features.PowerFxV1;

			PowerFxConfig config = new PowerFxConfig(toenable);
			config.EnableSetFunction();

			Microsoft.PowerFx.RecalcEngine recalcEngine = new Microsoft.PowerFx.RecalcEngine(config);

			XrmMockupSettings xrmMockupSettings = new XrmMockupSettings();
			xrmMockupSettings.BasePluginTypes = new[] { typeof(IPlugin) };

			XrmMockup365 xrmMockup365 = XrmMockup365.GetInstance(xrmMockupSettings);

			IOrganizationService orgService = xrmMockup365.GetAdminService();
			IOrganizationService augmentedOrgService = A.Fake<IOrganizationService>(o => o.Wrapping(orgService));
			RetrieveAllEntitiesResponse entitiesResponse = new RetrieveAllEntitiesResponse();
			entitiesResponse.Results["EntityMetadata"] = new[] { orgService.GetEntityMetadata("contact") };

			A.CallTo(() => augmentedOrgService.Execute(A<RetrieveAllEntitiesRequest>.Ignored)).Returns(entitiesResponse);

			DataverseConnection dataverse = SingleOrgPolicy.New(augmentedOrgService);
			ReadOnlySymbolValues symbolValues = dataverse.SymbolValues;

			// Act
			FormulaValue result = await recalcEngine.EvalAsync("Set(a, 1)", default, symbolValues);

			// Assert
			Assert.IsNotNull(result);
		}
	}
}
