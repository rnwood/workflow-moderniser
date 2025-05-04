using McTools.Xrm.Connection;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WorkflowModerniser.Outputs;
using WorkflowModerniser.XrmToolboxPlugin.Model;
using XrmToolBox.Extensibility;
using XrmToolBox.Extensibility.Interfaces;
using static WorkflowModerniser.WorkflowConverter;

namespace WorkflowModerniser.XrmToolboxPlugin
{
    public partial class PluginControl : PluginControlBase
    {
        public PluginControl()
        {
            InitializeComponent();
        }

        protected override void OnConnectionUpdated(ConnectionUpdatedEventArgs e)
        {
            base.OnConnectionUpdated(e);

            RefreshSourceOptions();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            RefreshSourceOptions();
        }

        private void RefreshSourceOptions()
        {
            this.Enabled = false;
            solutionsBindingSource.DataSource = null;
            classicWorkflowsBindingSource.DataSource = null;
            outputsBindingSource.DataSource = null;

            if (this.Service != null)
            {
                WorkAsync(new WorkAsyncInfo
                {
                    Message = "Loading solutions...",
                    Work = (worker, args) =>
                    {
                        var solutions = Solution.GetAll(this.Service);
                        args.Result = solutions;
                    },
                    PostWorkCallBack = (args) =>
                    {
                        if (args.Error != null)
                        {
                            this.ShowErrorDialog(args.Error, "Error loading solutions");
                        }
                        else
                        {
                            solutionsBindingSource.DataSource = args.Result;
                            this.Enabled = true;
                        }
                    }
                });
            }
        }

        private void solutionsBindingSource_CurrentChanged(object sender, EventArgs e)
        {
            classicWorkflowsBindingSource.DataSource = null;
            outputsBindingSource.DataSource = null;


            Solution solution = (Solution) solutionsBindingSource.Current;
            if (solution != null)
            {

                WorkAsync(new WorkAsyncInfo
                {
                    Message = "Loading workflows...",
                    Work = (worker, args) =>
                    {
                        var workflow = WorkflowConversionOptions.GetAllClassicWorkflowsAndBusinessRules(this.Service, solution);
                        args.Result = workflow;
                    },
                    PostWorkCallBack = (args) =>
                    {
                        if (args.Error != null)
                        {
                            this.ShowErrorDialog(args.Error, "Error loading workflows");
                        }
                        else
                        {
                            classicWorkflowsBindingSource.DataSource = args.Result;
                        }
                    }
                });


            }
        }

        private void previewButton_Click(object sender, EventArgs e)
        {
            var inputs = (classicWorkflowsBindingSource.DataSource as IList<WorkflowConversionOptions>)
                .Where(w => w.Convert)
                .ToList();


            WorkAsync(new WorkAsyncInfo
            {
                Message = "Previewing conversion...",
                Work = (worker, args) =>
                {
                    var outputs = inputs.Select(i => WorkflowConverter.Create(this.Service, i.OutputType).Convert(i.Id)).SelectMany(r => r).ToList();
                    args.Result = outputs;
                },
                PostWorkCallBack = (args) =>
                {
                    if (args.Error != null)
                    {
                        this.ShowErrorDialog(args.Error, "Error previewing conversion");
                    }
                    else
                    {
                        var outputs = args.Result as IList<IOutput>;

                        this.outputsBindingSource.DataSource = outputs;
                        this.saveComponentsButton.Enabled = outputs.Any();
                    }
                }
            });
        }



        private void outputsBindingSource_CurrentChanged(object sender, EventArgs e)
        {
            propertyGrid1.SelectedObject = outputsBindingSource.Current;
        }


        private void PluginControl_Load(object sender, EventArgs e)
        {
            outputTypeDataGridViewColumn.DataSource = Enum.GetValues(typeof(OutputType)).Cast<OutputType>().Select(x => new { Name = x.ToString(), Value = x }).ToList();
            outputTypeDataGridViewColumn.DisplayMember = "Name";
            outputTypeDataGridViewColumn.ValueMember = "Value";
        }

        private void classicWorkflowsBindingSource_ListChanged(object sender, ListChangedEventArgs e)
        {
            previewButton.Enabled = (classicWorkflowsBindingSource.DataSource as IList<WorkflowConversionOptions>)?.Any(w => w.Convert) ?? false;
        }

        private void saveComponentsButton_Click(object sender, EventArgs e)
        {
            IList<IOutput> inputs = (outputsBindingSource.DataSource as IList<IOutput>);


            WorkAsync(new WorkAsyncInfo
            {
                Message = "Saving components...",
                Work = (worker, args) =>
                {
                    foreach (var output in inputs)
                    {
                        output.Ensure(Service, ((Solution)solutionsBindingSource.Current).UniqueName);
                    }
                },
                PostWorkCallBack = (args) =>
                {
                    if (args.Error != null)
                    {
                        this.ShowErrorDialog(args.Error, "Error saving components");
                    }
                    else
                    {
                        MessageBox.Show("Components saved successfully", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            });




        }
    }
}
