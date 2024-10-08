#pragma warning disable CS1591
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace WorkflowModerniser.Data
{
	
	
	[System.Runtime.Serialization.DataContractAttribute(Namespace="http://schemas.microsoft.com/xrm/2011/new/")]
	[Microsoft.Xrm.Sdk.Client.RequestProxyAttribute("GeneratePowerFxFormula")]
	public partial class GeneratePowerFxFormulaRequest : Microsoft.Xrm.Sdk.OrganizationRequest
	{
		
		public static class Fields
		{
			public const string OperationId = "OperationId";
			public const string ConnectionReference = "ConnectionReference";
			public const string ParameterValues = "ParameterValues";
		}
		
		public const string ActionLogicalName = "GeneratePowerFxFormula";
		
		public string OperationId
		{
			get
			{
				if (this.Parameters.Contains("OperationId"))
				{
					return ((string)(this.Parameters["OperationId"]));
				}
				else
				{
					return default(string);
				}
			}
			set
			{
				this.Parameters["OperationId"] = value;
			}
		}
		
		public Microsoft.Xrm.Sdk.EntityReference ConnectionReference
		{
			get
			{
				if (this.Parameters.Contains("ConnectionReference"))
				{
					return ((Microsoft.Xrm.Sdk.EntityReference)(this.Parameters["ConnectionReference"]));
				}
				else
				{
					return default(Microsoft.Xrm.Sdk.EntityReference);
				}
			}
			set
			{
				this.Parameters["ConnectionReference"] = value;
			}
		}
		
		public Microsoft.Xrm.Sdk.Entity ParameterValues
		{
			get
			{
				if (this.Parameters.Contains("ParameterValues"))
				{
					return ((Microsoft.Xrm.Sdk.Entity)(this.Parameters["ParameterValues"]));
				}
				else
				{
					return default(Microsoft.Xrm.Sdk.Entity);
				}
			}
			set
			{
				this.Parameters["ParameterValues"] = value;
			}
		}
		
		public GeneratePowerFxFormulaRequest()
		{
			this.RequestName = "GeneratePowerFxFormula";
			this.OperationId = default(string);
			this.ConnectionReference = default(Microsoft.Xrm.Sdk.EntityReference);
		}
	}
	
	[System.Runtime.Serialization.DataContractAttribute(Namespace="http://schemas.microsoft.com/xrm/2011/new/")]
	[Microsoft.Xrm.Sdk.Client.ResponseProxyAttribute("GeneratePowerFxFormula")]
	public partial class GeneratePowerFxFormulaResponse : Microsoft.Xrm.Sdk.OrganizationResponse
	{
		
		public static class Fields
		{
			public const string Response = "Response";
		}
		
		public const string ActionLogicalName = "GeneratePowerFxFormula";
		
		public GeneratePowerFxFormulaResponse()
		{
		}
		
		public Microsoft.Xrm.Sdk.EntityCollection Response
		{
			get
			{
				if (this.Results.Contains("Response"))
				{
					return ((Microsoft.Xrm.Sdk.EntityCollection)(this.Results["Response"]));
				}
				else
				{
					return default(Microsoft.Xrm.Sdk.EntityCollection);
				}
			}
			set
			{
				this.Results["Response"] = value;
			}
		}
	}
}
#pragma warning restore CS1591
