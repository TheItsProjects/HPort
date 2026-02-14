# GEMINI.md - Project Context: HPort

## Project Overview
HPort is a .NET-based solution designed to streamline the management of Docker containers on Hetzner Cloud servers. It provides a library and a CLI tool to automate the provisioning of cloud servers and the deployment of containers using Docker Compose over SSH.

## Main Technologies
- **Runtime:** .NET 9.0
- **Cloud Provider:** Hetzner Cloud (via `HetznerCloud.API`)
- **Remote Interaction:** SSH (via `SSH.NET`)
- **Containerization:** Docker & Docker Compose
- **CLI Framework:** `DotMake.CommandLine`
- **Security:** `BCrypt.Net-Next` for password hashing
- **Testing:** NUnit, `NSubstitute`, and `System.IO.Abstractions` for file system mocking.

## Architecture & Structure
The project follows a clean architecture pattern with a clear separation between the core library, CLI, and tests.

- **`src/ItsNameless.HPort`**: Core library containing business logic.
    - **`Services`**: High-level services (e.g., `ContainerService`) that coordinate between repositories.
    - **`Repositories`**: Low-level data and infrastructure access.
        - `ServerRepository`: Manages Hetzner Cloud server instances and SSH execution.
        - `ContainerRepository`: Manages Docker container lifecycle (Create, List, Delete, Execute).
    - **`Models`**: Data structures representing servers and containers.
- **`src/ItsNameless.HPortCli`**: CLI application providing a user-friendly interface for the library.
- **`test/`**:
    - `ItsNameless.HPort.Test`: Unit tests for the core library.
    - `ItsNameless.HPortCli.Test`: Unit tests for the CLI.
    - `ItsNameless.HPort.IntegrationTest`: Integration tests that interact with the real Hetzner Cloud API.

## Building and Running
- **Build Solution:**
  ```bash
  dotnet build
  ```
- **Run CLI:**
  ```bash
  dotnet run --project src/ItsNameless.HPortCli -- [args]
  ```
- **Run Tests:**
  ```bash
  dotnet test
  ```
- **Integration Tests:**
  To run integration tests, you must provide a valid Hetzner Cloud API token in a file named `HETZNER_TOKEN` within `test/ItsNameless.HPort.IntegrationTest/`. **Warning:** Running integration tests creates real resources and incurs costs.

## Development Conventions
- **Nullable Reference Types:** Enabled project-wide.
- **Implicit Usings:** Enabled project-wide.
- **Interface Generation:** Uses `InterfaceGenerator` (source generator) with the `[GenerateAutoInterface]` attribute to keep interfaces in sync with implementations.
- **Dependency Injection:** Extensively used, especially in the CLI and for service/repository coordination.
- **File System Abstraction:** Always use `IFileSystem` from `System.IO.Abstractions` to ensure testability.
- **SSH Commands:** All remote server commands are centralized in `ServerRepository.Commands.cs`.
- **Code Style:** Strict adherence to `.editorconfig`, including a maximum line length of 80 characters (via `resharper_csharp_max_line_length`).

## Key Files
- `hport.sln`: Solution file.
- `src/ItsNameless.HPort/HPort.cs`: Entry point for library consumers.
- `src/ItsNameless.HPortCli/Program.cs`: Entry point for the CLI application.
- `serverStates.json`: Default file used to persist state about managed servers (e.g., generated passwords).
