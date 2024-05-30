# WorkflowModerniser for Dynamics 365/Dataverse

WorkflowModerniser converts Classic Workflows to Low Code Plugins (and some other experimental conversions from Business Rules and to Cloud Flows and Form Scripts).

Why? There are many D365 (and other Dataverse driven) implementations using Classic Workflows
heavily since they were the main no/low-code customisation option for a long time. 
This tool should allow large implementation to move forwards more easily. 

How to use:
This is a simple command line tool currently which must be executed as follows:
```
WorkflowModerniser.exe <connectionstring> <workflowid> <outputtype>
```
`<connectionstring>` - Connection string for the environment contaning the workflow. See https://learn.microsoft.com/en-us/power-apps/developer/data-platform/xrm-tooling/use-connection-strings-xrm-tooling-connect
`workflowid` - Workflow ID for the Classic Workflow
`<outputtype>` - Valid output types: lowcodeplugin cloudflow (experimental) formscript (experimental)

The output is a JSON serialised array of the outputs. Currently, we do not actually create these in the target environment.

Status:
- Alpha POC! 
- Only the surface area that I've encountered so far in testing is covered by the converter. It should currently throw an exception when hitting something that isn't handled. Please feed back!
- Currently the results are not saved.
- Only foreground Workflows are currently supported. This is not currently enforced.
- Terminate with success action is not supported

Future potential roadmap:
- Create some automated tests if feasible and valuable without them being too rigid (hard!)
- Testing with more real Workflows
- Save results to environment - create the LCPs automatically from the results
- Allow configurable 'best effort' mode where unsupported elements just write an Error("TODO").
- Wrap up in XrmToolbox plugin UI
- Conversion of background Workflows to Power Automate Cloud Flows. This is feasible and the project has been structured to allow it.
- Conversion of Business Rules and legacy Formula Columns. These are implemented in the same way as Workflows internally, so this ought to be possible.

FAQ:
- Isn't it "Modernizer"?
  No. I'm British.