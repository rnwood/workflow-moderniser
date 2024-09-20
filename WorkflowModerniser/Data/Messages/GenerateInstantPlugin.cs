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
	[Microsoft.Xrm.Sdk.Client.RequestProxyAttribute("GenerateInstantPlugin")]
	public partial class GenerateInstantPluginRequest : Microsoft.Xrm.Sdk.OrganizationRequest
	{
		
		public static class Fields
		{
			public const string Expression = "Expression";
			public const string SolutionUniqueName = "SolutionUniqueName";
			public const string Description = "Description";
			public const string EntityLogicalName = "EntityLogicalName";
			public const string Parameters_1 = "Parameters_1";
			public const string Name = "Name";
			public const string Context = "Context";
		}
		
		public const string ActionLogicalName = "GenerateInstantPlugin";
		
		public string Expression
		{
			get
			{
				if (this.Parameters.Contains("Expression"))
				{
					return ((string)(this.Parameters["Expression"]));
				}
				else
				{
					return default(string);
				}
			}
			set
			{
				this.Parameters["Expression"] = value;
			}
		}
		
		public string SolutionUniqueName
		{
			get
			{
				if (this.Parameters.Contains("SolutionUniqueName"))
				{
					return ((string)(this.Parameters["SolutionUniqueName"]));
				}
				else
				{
					return default(string);
				}
			}
			set
			{
				this.Parameters["SolutionUniqueName"] = value;
			}
		}
		
		public string Description
		{
			get
			{
				if (this.Parameters.Contains("Description"))
				{
					return ((string)(this.Parameters["Description"]));
				}
				else
				{
					return default(string);
				}
			}
			set
			{
				this.Parameters["Description"] = value;
			}
		}
		
		public string EntityLogicalName
		{
			get
			{
				if (this.Parameters.Contains("EntityLogicalName"))
				{
					return ((string)(this.Parameters["EntityLogicalName"]));
				}
				else
				{
					return default(string);
				}
			}
			set
			{
				this.Parameters["EntityLogicalName"] = value;
			}
		}
		
		public string Parameters_1
		{
			get
			{
				if (this.Parameters.Contains("Parameters"))
				{
					return ((string)(this.Parameters["Parameters"]));
				}
				else
				{
					return default(string);
				}
			}
			set
			{
				this.Parameters["Parameters"] = value;
			}
		}
		
		public string Name
		{
			get
			{
				if (this.Parameters.Contains("Name"))
				{
					return ((string)(this.Parameters["Name"]));
				}
				else
				{
					return default(string);
				}
			}
			set
			{
				this.Parameters["Name"] = value;
			}
		}
		
		public string Context
		{
			get
			{
				if (this.Parameters.Contains("Context"))
				{
					return ((string)(this.Parameters["Context"]));
				}
				else
				{
					return default(string);
				}
			}
			set
			{
				this.Parameters["Context"] = value;
			}
		}
		
		public GenerateInstantPluginRequest()
		{
			this.RequestName = "GenerateInstantPlugin";
			this.Expression = default(string);
			this.Description = default(string);
			this.Name = default(string);
		}
	}
	
	[System.Runtime.Serialization.DataContractAttribute(Namespace="http://schemas.microsoft.com/xrm/2011/new/")]
	[Microsoft.Xrm.Sdk.Client.ResponseProxyAttribute("GenerateInstantPlugin")]
	public partial class GenerateInstantPluginResponse : Microsoft.Xrm.Sdk.OrganizationResponse
	{
		
		public static class Fields
		{
			public const string CustomApiId = "CustomApiId";
			public const string FxExpressionId = "FxExpressionId";
		}
		
		public const string ActionLogicalName = "GenerateInstantPlugin";
		
		public GenerateInstantPluginResponse()
		{
		}
		
		public System.Guid CustomApiId
		{
			get
			{
				if (this.Results.Contains("CustomApiId"))
				{
					return ((System.Guid)(this.Results["CustomApiId"]));
				}
				else
				{
					return default(System.Guid);
				}
			}
			set
			{
				this.Results["CustomApiId"] = value;
			}
		}
		
		public System.Guid FxExpressionId
		{
			get
			{
				if (this.Results.Contains("FxExpressionId"))
				{
					return ((System.Guid)(this.Results["FxExpressionId"]));
				}
				else
				{
					return default(System.Guid);
				}
			}
			set
			{
				this.Results["FxExpressionId"] = value;
			}
		}
	}
}
#pragma warning restore CS1591
