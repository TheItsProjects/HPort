# HPort

HPort is a .NET-based tool and library for automating the management of Docker containers on Hetzner Cloud. It streamlines provisioning servers, deploying applications via Docker Compose, and managing their lifecycle.

## Documentation

-   **[Installation](docs/Installation.md)**: Prerequisites and how to build the tool.
-   **[Configuration](docs/Configuration.md)**: Setting up environment variables.
-   **[CLI Reference](docs/CliReference.md)**: Detailed command usage.
-   **[Library Usage](docs/LibraryUsage.md)**: How to use HPort in your own C# code.
-   **[Troubleshooting](docs/Troubleshooting.md)**: Common issues and fixes.
-   **[Developer Guide](docs/DeveloperGuide.md)**: Architecture and contribution guidelines.

## Quick Start

1.  **Set your Token**:
    ```bash
    export HPORT_TOKEN="your-hetzner-api-token"
    ```

2.  **Run the CLI**:
    ```bash
    # List containers (assuming you've built the project)
    dotnet run --project src/ItsNameless.HPortCli -- container list
    ```

## Features

-   **Automated Provisioning**: Spins up `cx22`, `cpx11`, etc., on demand.
-   **Docker Compose**: First-class support for standard compose deployments.
-   **Zero-Config Security**: Automatically manages server credentials (stored locally).
-   **Internal Networking**: Support for attaching servers to private networks.

## License

Released under the [MIT License](LICENSE).
