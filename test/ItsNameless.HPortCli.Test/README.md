# ItsNameless.HPortCli.Test

This project contains the unit tests for the HPort CLI.

## Test Architecture

The tests use:
-   **[NUnit](https://nunit.org/):** As the testing framework.
-   **[NSubstitute](https://nsubstitute.github.io/):** For mocking dependencies.
-   **[DotMake.CommandLine](https://github.com/DotMake/CommandLine):** The CLI framework used by the application.

### Dependency Injection Strategy

`DotMake.CommandLine` uses a static service configuration approach via `Cli.Ext.ConfigureServices`. This presents a challenge for unit testing, where we want to isolate tests and provide fresh mock dependencies for each run.

To solve this, we use a **Static Service Locator** pattern specifically for the tests:

1.  **`HPortWrapper`**: This class implements `IHPort` but acts as a proxy. It has a static property `Current` that holds the actual implementation (the mock) to be used.
2.  **`CliTestBase`**:
    -   In `OneTimeSetUp`, it registers `HPortWrapper` as a singleton with `DotMake.CommandLine`. This happens once for the entire test assembly execution.
    -   In `SetUp` (before each test), it creates a new mock `IHPort` and assigns it to `HPortWrapper.Current` via the `RegisterMock` helper.
    -   When the CLI runs a command, it resolves `IHPort` (which is the `HPortWrapper` instance), which then delegates calls to the current mock.

**Note:** Because of this static shared state, **tests cannot be run in parallel**.

## How to Write a New Test

1.  Create a new test class inheriting from `CliTestBase`.
2.  Override `SetUp` if you need specific mock behavior for all tests in the class, or configure the mock inside each `[Test]` method.
3.  Use the `RunCommand` helper to execute the CLI command.
4.  Assert against the exit code and the content of `Output` (stdout) or `Error` (stderr).

### Example

```csharp
[Test]
public async Task MyCommand_Works()
{
    // Arrange
    var myServiceMock = Substitute.For<IMyService>();
    HPortMock.MyService.Returns(myServiceMock);

    // Act
    var result = await RunCommand("my", "command", "--option", "value");

    // Assert
    Assert.That(result, Is.EqualTo(0));
    Assert.That(Output.ToString(), Contains.Substring("Success"));
}
```
