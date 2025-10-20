# WorkflowModerniser Test Suite

This directory contains comprehensive tests for the WorkflowModerniser project.

## Test Organization

The tests are organized by component to ensure thorough coverage of the codebase:

### Core Component Tests

- **MetadataServiceTests.cs** - Tests for entity metadata caching and retrieval
- **WriterContextTests.cs** - Tests for workflow context configuration and message name flags
- **OrgServiceWorkflowSourceTests.cs** - Tests for Dataverse workflow and solution retrieval

### Entity Variable Tests

- **LCPEntityVariableTests.cs** - Tests for Low Code Plugin entity variable management

### Writer Tests

- **LowCodePluginPowerFxWriterTests.cs** - Comprehensive tests for PowerFx expression generation including:
  - Literal value formatting (strings, GUIDs, booleans, option sets, etc.)
  - Condition operators (Equal, NotEqual, Null, NotNull, GreaterThan, etc.)
  - Logical operators (And, Or)
  - Entity operations (LoadPrimaryEntity, NewEntityVariable, etc.)
  - Property expressions and coalescing

### Output Tests

- **PluginOutputTests.cs** - Tests for plugin output classes (AutomatedPlugin, InstantPlugin)
  - Schema name generation
  - Property initialization and modification
  - Interface implementation

### Integration Tests

- **WorkflowConverterTests.cs** - Integration tests for workflow conversion including:
  - Business Rule conversion
  - Workflow stage handling (Create, Update, Delete)
  - Pre/Post operation stages
  - Subprocess workflows
  - Multi-stage workflows

- **PowerFxIntegrationTests.cs** (formerly UnitTest1.cs) - PowerFx integration with XrmMockup
  - Validates PowerFx expressions work with Dataverse

## Running Tests

Tests are automatically run in the CI/CD pipeline on Windows 2019 via GitHub Actions.

To run tests locally:

```bash
# Restore packages
nuget restore WorkflowModerniser.sln

# Build the test project
msbuild WorkflowModerniser.Tests\WorkflowModerniser.Tests.csproj -t:build -p:Configuration=Release

# Run tests using dotnet test or vstest.console.exe
dotnet test WorkflowModerniser.Tests\WorkflowModerniser.Tests.csproj --configuration Release
```

## Test Dependencies

The test suite uses:
- **MSTest** - Test framework
- **FakeItEasy** - Mocking framework for dependencies
- **XrmMockup365** - In-memory Dataverse instance for integration testing
- **Microsoft.PowerFx** - PowerFx expression engine for formula validation

## Test Coverage

The test suite covers:
- ✅ Metadata service caching
- ✅ Writer context configuration
- ✅ Entity variable management
- ✅ PowerFx expression generation
- ✅ Literal value formatting
- ✅ Condition and logical operators
- ✅ Plugin output generation
- ✅ Workflow conversion for different categories and stages
- ✅ PowerFx integration with Dataverse

## Future Test Enhancements

Potential areas for additional testing:
- More complex workflow conversion scenarios with actual XAML
- Form script writer tests
- Cloud flow writer tests
- Error handling and edge cases
- Performance testing for large workflows

## Notes

- Tests are designed to be valuable without being too rigid, following the project's philosophy
- Integration tests use minimal XAML to avoid brittleness
- Mock objects are used extensively to isolate units under test
- XrmMockup is used for realistic Dataverse integration scenarios
