# Developer Guide

This guide is for developers who want to contribute to HPort or understand its internal architecture.

## Architecture

HPort follows a clean architecture pattern with a clear separation of concerns.

### Project Structure

-   **`src/ItsNameless.HPort`**: The Core Library.
    -   **`Services`**: High-level business logic (e.g., `ContainerService`). These coordinate actions.
    -   **`Repositories`**: Infrastructure access layers.
        -   `ServerRepository`: Handles Hetzner Cloud API calls and SSH execution.
        -   `ContainerRepository`: Abstraction for Docker operations on remote servers.
    -   **`Models`**: Data Transfer Objects (DTOs) and Domain Models.
-   **`src/ItsNameless.HPortCli`**: The CLI Application.
    -   Uses `DotMake.CommandLine` for parsing arguments.
    -   Acts as a thin wrapper around the Core Library.
-   **`test/`**:
    -   **`ItsNameless.HPort.Test`**: Unit tests for the core.
    -   **`ItsNameless.HPort.IntegrationTest`**: End-to-end tests against real Hetzner API.

### Key Patterns

-   **Interface Generation**: We use a Source Generator (`[GenerateAutoInterface]`) to automatically keep interfaces in sync with their implementations.
-   **File System Abstraction**: All file I/O uses `System.IO.Abstractions` (`IFileSystem`). **Never use `System.IO.File` directly**; this ensures code is testable.
-   **SSH Command Centralization**: All shell commands executed on remote servers are defined in `ServerRepository.Commands.cs`. Do not hardcode strings in logic methods.

## Development Setup

1.  **Clone**:
    ```bash
    git clone https://github.com/ItsNameless/hport.git
    ```
2.  **Restore**:
    ```bash
    dotnet restore
    ```
3.  **Build**:
    ```bash
    dotnet build
    ```

## Testing

### Unit Tests
Unit tests are fast and do not require external resources. They use `NSubstitute` for mocking.

```bash
dotnet test test/ItsNameless.HPort.Test
```

### Integration Tests
Integration tests provision **real resources** on Hetzner Cloud. They **will cost money** to run.

1.  Create a file named `HETZNER_TOKEN` in `test/ItsNameless.HPort.IntegrationTest/`.
2.  Paste a valid API token into that file.
3.  Run the tests:
    ```bash
    dotnet test test/ItsNameless.HPort.IntegrationTest
    ```

> **Note**: Integration tests are designed to clean up after themselves, but always check your Hetzner Cloud Console manually if a test crashes to ensure no orphan servers are left running.

## Code Style

-   The project uses `.editorconfig` to enforce styles.
-   **Max Line Length**: 80 characters.
-   **Naming**: Standard C# conventions (PascalCase for public, camelCase for private/local).

## Release Process

(To be defined)
