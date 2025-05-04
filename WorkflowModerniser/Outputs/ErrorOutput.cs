using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xrm.Sdk;

namespace WorkflowModerniser.Outputs
{
    public class ErrorOutput : IOutput
    {
        public ErrorOutput(string name, Exception error)
        {
            this.Name = name ?? throw new ArgumentNullException(nameof(name));
            this.Error = error ?? throw new ArgumentNullException(nameof(error));
        }


        public string Name {get;private set;}

        public Exception Error { get; private set; }


        public void Ensure(IOrganizationService serviceClient, string solutionUniqueName)
        {
            throw new NotImplementedException();
        }
    }
}
