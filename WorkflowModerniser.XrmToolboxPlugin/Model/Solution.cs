using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace WorkflowModerniser.XrmToolboxPlugin.Model
{
    public class Solution
    {
        private readonly Entity _entity;

        public Solution(Entity entity)
        {
            _entity = entity ?? throw new ArgumentNullException(nameof(entity));
        }

        public string UniqueName { get => this._entity.GetAttributeValue<string>("uniquename"); }


        public Guid Id { get => this._entity.Id; }

        public static IList<Solution> GetAll(IOrganizationService service)
        {
            return service.RetrieveMultiple(new QueryExpression("solution") {
                Criteria = new FilterExpression()
                {
                    Conditions = {
                        new ConditionExpression("isvisible", ConditionOperator.Equal, true),
                        new ConditionExpression("solutiontype", ConditionOperator.Equal, 0)
                    }
                },
                ColumnSet = new ColumnSet(true) 
            }).Entities.Select(e => new Solution(e)).ToList();
        }
    }
}
