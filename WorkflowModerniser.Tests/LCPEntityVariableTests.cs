using Microsoft.VisualStudio.TestTools.UnitTesting;
using WorkflowModerniser.Outputs.LowCodeCodePlugins;

namespace WorkflowModerniser.Tests
{
	[TestClass]
	public class LCPEntityVariableTests
	{
		[TestMethod]
		public void Constructor_SetsTableName()
		{
			// Arrange & Act
			var entityVariable = new LCPEntityVariable("contact", "ThisRecord");

			// Assert
			Assert.AreEqual("contact", entityVariable.TableName);
		}

		[TestMethod]
		public void Constructor_SetsRecordExpression()
		{
			// Arrange & Act
			var entityVariable = new LCPEntityVariable("contact", "ThisRecord");

			// Assert
			Assert.AreEqual("ThisRecord", entityVariable.RecordExpression);
		}

		[TestMethod]
		public void IsPrimary_CanBeSet()
		{
			// Arrange
			var entityVariable = new LCPEntityVariable("contact", "ThisRecord");

			// Act
			entityVariable.IsPrimary = true;

			// Assert
			Assert.IsTrue(entityVariable.IsPrimary);
		}

		[TestMethod]
		public void IdExpression_CanBeSet()
		{
			// Arrange
			var entityVariable = new LCPEntityVariable("contact", "ThisRecord");

			// Act
			entityVariable.IdExpression = "ThisRecord.contactid";

			// Assert
			Assert.AreEqual("ThisRecord.contactid", entityVariable.IdExpression);
		}

		[TestMethod]
		public void ColumnExpressions_InitializesAsEmptyDictionary()
		{
			// Arrange & Act
			var entityVariable = new LCPEntityVariable("contact", "ThisRecord");

			// Assert
			Assert.IsNotNull(entityVariable.ColumnExpressions);
			Assert.AreEqual(0, entityVariable.ColumnExpressions.Count);
		}

		[TestMethod]
		public void ColumnExpressions_CanAddExpressions()
		{
			// Arrange
			var entityVariable = new LCPEntityVariable("contact", "ThisRecord");

			// Act
			entityVariable.ColumnExpressions.Add("firstname", "ThisRecord.firstname");
			entityVariable.ColumnExpressions.Add("lastname", "ThisRecord.lastname");

			// Assert
			Assert.AreEqual(2, entityVariable.ColumnExpressions.Count);
			Assert.AreEqual("ThisRecord.firstname", entityVariable.ColumnExpressions["firstname"]);
			Assert.AreEqual("ThisRecord.lastname", entityVariable.ColumnExpressions["lastname"]);
		}

		[TestMethod]
		public void Constructor_AcceptsNullRecordExpression()
		{
			// Arrange & Act
			var entityVariable = new LCPEntityVariable("contact", null);

			// Assert
			Assert.AreEqual("contact", entityVariable.TableName);
			Assert.IsNull(entityVariable.RecordExpression);
		}
	}
}
