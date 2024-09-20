# WorkflowModerniser for Dynamics 365/Dataverse

WorkflowModerniser converts Classic Workflows to Low Code Plugins (and some other experimental conversions from Business Rules and to Cloud Flows and Form Scripts).

Why? There are many D365 (and other Dataverse driven) implementations using Classic Workflows
heavily since they were the main no/low-code customisation option for a long time. 
This tool should allow large implementation to move forwards more easily. 

How to use:
This is a simple command line tool currently which must be executed as follows:
```
WorkflowModerniser.exe <connectionstring> <workflowid> <outputtype> <solutionuniquename>
```
`<connectionstring>` - Connection string for the environment contaning the workflow. See https://learn.microsoft.com/en-us/power-apps/developer/data-platform/xrm-tooling/use-connection-strings-xrm-tooling-connect

`workflowid` - Workflow ID for the Classic Workflow

`<outputtype>` - Valid output types: `lowcodeplugin` `cloudflow` (experimental) `formscript` (experimental)

`<soluitionuniquename>` - Solution to add the outputs to (unique name).

The output is a JSON serialised array of the outputs for debugging. The program will then try actually create/update these outputs in the solution you provided.

Status:
- Alpha POC! 
- NEW! The outputs are actually created for `lowcodeplugin` output type.
- Only the surface area that I've encountered so far in testing is covered by the converter. It should currently throw an exception when hitting something that isn't handled. Please feed back!
- Only foreground Workflows are currently supported. This is not currently enforced.
- Terminate with success action is not supported (how do we achieve this?)

Future potential roadmap:
- Create some automated tests if feasible and valuable without them being too rigid (hard!)
- Testing with more real Workflows
- Allow configurable 'best effort' mode where unsupported elements just write an Error("TODO").
- Wrap up in XrmToolbox plugin UI
- Conversion of background Workflows to Power Automate Cloud Flows. This is feasible and the project has been structured to allow it.
- Conversion of Business Rules and legacy Formula Columns. These are implemented in the same way as Workflows internally, so this ought to be possible.

FAQ:
- Isn't it "Modernizer"?
  No. I'm British.