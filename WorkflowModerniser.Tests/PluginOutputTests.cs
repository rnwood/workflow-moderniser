using Microsoft.VisualStudio.TestTools.UnitTesting;
using WorkflowModerniser.Outputs.LowCodeCodePlugins;

namespace WorkflowModerniser.Tests
{
	[TestClass]
	public class PluginOutputTests
	{
		[TestMethod]
		public void PluginBase_Constructor_SetsProperties()
		{
			// Arrange & Act
			var plugin = new AutomatedPlugin("Test Plugin", "contact", 20, "Create", "Set(x, 1)");

			// Assert
			Assert.AreEqual("Test Plugin", plugin.Name);
			Assert.AreEqual("contact", plugin.EntityLogicalName);
			Assert.AreEqual("Set(x, 1)", plugin.Expression);
		}

		[TestMethod]
		public void PluginBase_SchemaName_GeneratesFromName()
		{
			// Arrange & Act
			var plugin = new AutomatedPlugin("Test Plugin", "contact", 20, "Create", "Set(x, 1)");

			// Assert
			Assert.AreEqual("fixme_Test_Plugin", plugin.SchemaName);
		}

		[TestMethod]
		public void PluginBase_SchemaName_ReplacesSpaces()
		{
			// Arrange & Act
			var plugin = new AutomatedPlugin("My Test Plugin Name", "contact", 20, "Create", "Set(x, 1)");

			// Assert
			Assert.AreEqual("fixme_My_Test_Plugin_Name", plugin.SchemaName);
		}

		[TestMethod]
		public void AutomatedPlugin_Constructor_SetsStage()
		{
			// Arrange & Act
			var plugin = new AutomatedPlugin("Test Plugin", "contact", 20, "Create", "Set(x, 1)");

			// Assert
			Assert.AreEqual(20, plugin.Stage);
		}

		[TestMethod]
		public void AutomatedPlugin_Constructor_SetsMessageName()
		{
			// Arrange & Act
			var plugin = new AutomatedPlugin("Test Plugin", "contact", 20, "Create", "Set(x, 1)");

			// Assert
			Assert.AreEqual("Create", plugin.MessageName);
		}

		[TestMethod]
		public void InstantPlugin_Constructor_SetsProperties()
		{
			// Arrange & Act
			var plugin = new InstantPlugin("Test Action", "contact", "Set(x, 1)");

			// Assert
			Assert.AreEqual("Test Action", plugin.Name);
			Assert.AreEqual("contact", plugin.EntityLogicalName);
			Assert.AreEqual("Set(x, 1)", plugin.Expression);
		}

		[TestMethod]
		public void InstantPlugin_ImplementsICustomActionOutput()
		{
			// Arrange & Act
			var plugin = new InstantPlugin("Test Action", "contact", "Set(x, 1)");

			// Assert
			Assert.IsInstanceOfType(plugin, typeof(ICustomActionOutput));
		}

		[TestMethod]
		public void AutomatedPlugin_PropertiesCanBeModified()
		{
			// Arrange
			var plugin = new AutomatedPlugin("Test Plugin", "contact", 20, "Create", "Set(x, 1)");

			// Act
			plugin.Name = "Modified Name";
			plugin.EntityLogicalName = "account";
			plugin.Expression = "Set(y, 2)";
			plugin.Stage = 40;
			plugin.MessageName = "Update";

			// Assert
			Assert.AreEqual("Modified Name", plugin.Name);
			Assert.AreEqual("account", plugin.EntityLogicalName);
			Assert.AreEqual("Set(y, 2)", plugin.Expression);
			Assert.AreEqual(40, plugin.Stage);
			Assert.AreEqual("Update", plugin.MessageName);
		}

		[TestMethod]
		public void InstantPlugin_PropertiesCanBeModified()
		{
			// Arrange
			var plugin = new InstantPlugin("Test Action", "contact", "Set(x, 1)");

			// Act
			plugin.Name = "Modified Action";
			plugin.EntityLogicalName = "account";
			plugin.Expression = "Set(y, 2)";

			// Assert
			Assert.AreEqual("Modified Action", plugin.Name);
			Assert.AreEqual("account", plugin.EntityLogicalName);
			Assert.AreEqual("Set(y, 2)", plugin.Expression);
		}
	}
}
