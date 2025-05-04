using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using static WorkflowModerniser.WorkflowConverter;

namespace WorkflowModerniser.XrmToolboxPlugin.Model
{
    public class WorkflowConversionOptions
    {
        private readonly Entity _entity;

        public WorkflowConversionOptions(Entity entity)
        {
            _entity = entity ?? throw new ArgumentNullException(nameof(entity));
            this.OutputType = GetDefaultOutputType();
            this.Convert = true;
        }

        public Guid Id { get => this._entity.GetAttributeValue<Guid>("workflowid"); }

        public bool Convert { get; set; }

        public string Name { get => $"{this._entity.GetAttributeValue<string>("name")} ({Category})"; }


        public string XAMl { get => this._entity.GetAttributeValue<string>("xaml"); }

        public string Category { get => this._entity.FormattedValues["category"]; }

        public OutputType OutputType { get; set; }

        private WorkflowConverter.OutputType GetDefaultOutputType()
        {
            switch (Category)
            {
                case "Workflow":
                    return WorkflowConverter.OutputType.LowCodePlugin;
                case "Business Rule":
                    return WorkflowConverter.OutputType.FormScript;
                default:
                    throw new NotImplementedException($"Output type for '{Category}' is not implemented");
            }
        }

        public static IList<WorkflowConversionOptions> GetAllClassicWorkflowsAndBusinessRules(IOrganizationService service, Solution solution)
        {
            return service.RetrieveMultiple(new QueryExpression("workflow") {
                Criteria = new FilterExpression()
                {
                    Conditions = {
                        new ConditionExpression("category", ConditionOperator.In, 0, 2)
                    }
                },
                LinkEntities =
                {
                    new LinkEntity("workflow", "solutioncomponent", "workflowid", "objectid", JoinOperator.Inner)
                    {
                        LinkCriteria =
                        {
                            Conditions = { new ConditionExpression("solutioncomponent", "solutionid", ConditionOperator.Equal, solution.Id) }
                        }
                    }
                }, 
                ColumnSet = new ColumnSet(true)
            }).Entities.Select(e => new WorkflowConversionOptions(e)).ToList();
        }
    }
}
