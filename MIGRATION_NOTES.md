# .NET 8.0 Migration Notes

## Summary

This project has been partially migrated from .NET Framework 4.7.2 to .NET 8.0. The migration faced significant challenges due to dependencies on Windows-specific technologies that are not available in .NET 8.0.

## Completed Work

### ✅ Successfully Migrated:
- **WorkflowModerniser.SubstituteClientActivities** project - Builds successfully on .NET 8.0
- All three projects converted to SDK-style format
- Removed packages.config in favor of PackageReference
- Removed AssemblyInfo.cs files (SDK-style projects auto-generate assembly attributes)
- Removed App.config files (no longer needed for SDK-style projects)
- Added UiPath.Workflow 6.0.3 package for System.Activities support on .NET 8.0
- Removed unsupported references (System.Workflow.*, System.Activities.Presentation, System.Web.Services)
- Updated NuGet packages to latest available versions

### ⚠️ Known Issues:

1. **Project Reference Issue**: The WorkflowModerniser main project cannot resolve the reference to WorkflowModerniser.SubtituteClientActivities. This appears to be a file system or path resolution issue that needs further investigation.

2. **Missing Microsoft.PowerFx.Dataverse**: The Tests project requires `Microsoft.PowerFx.Dataverse` namespace which may not be available as a standalone package for .NET 8.0.

3. **System.Workflow Dependencies**: Some code depends on `System.Workflow.*` namespaces which are .NET Framework-specific and have no .NET 8.0 equivalent. These have been removed from project references.

## Compatibility Warnings

Many packages show NU1701 warnings indicating they were built for .NET Framework. While these packages work, they may not be fully compatible:
- Microsoft.CrmSdk.CoreAssemblies 9.0.2.56
- Microsoft.CrmSdk.Deployment 9.0.2.34
- Microsoft.CrmSdk.Workflow 9.0.2.56
- Microsoft.CrmSdk.XrmTooling.CoreAssembly 9.1.1.45

## Remaining Work

To complete the migration:

1. **Resolve Project Reference Issue**: Investigate why the project reference between WorkflowModerniser and WorkflowModerniser.SubtituteClientActivities is not resolving correctly.

2. **Add Missing PowerFx Packages**: Investigate if Microsoft.PowerFx.Dataverse is available for .NET 8.0 or find an alternative.

3. **Address System.Workflow Dependencies**: Some functionality may need to be rewritten or removed if it depends on System.Workflow namespaces that don't exist in .NET 8.0.

4. **Test Functionality**: Once compilation succeeds, thorough testing is needed to ensure the application still works correctly, especially given the warnings about .NET Framework package compatibility.

## Build Commands

```bash
# Build individual project (works)
dotnet build "WorkflowModerniser.SubtituteClientActivities/WorkflowModerniser.SubstituteClientActivities.csproj"

# Build solution (has errors)
dotnet build WorkflowModerniser.sln
```

## Target Framework

All projects now target: `net8.0-windows` with `EnableWindowsTargeting=true`

This is necessary because:
- The code uses Windows-specific APIs (System.Activities, WPF)
- The EnableWindowsTargeting property allows cross-platform builds on non-Windows systems (like Linux CI)

## Recommendations

1. Consider staying on .NET Framework 4.7.2 or 4.8 if the application heavily depends on Windows Workflow Foundation, as full .NET 8.0 support would require significant rewrites.

2. If migration to .NET 8.0 is required, budget time for:
   - Rewriting or removing code that depends on System.Workflow
   - Finding alternatives for missing packages
   - Extensive testing to ensure compatibility with .NET Framework-era packages

3. Monitor for updates to Microsoft Dataverse/Dynamics packages that may provide better .NET 8.0 support in the future.
