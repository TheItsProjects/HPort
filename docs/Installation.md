# Installation

## Prerequisites

To use HPort, you need to have the following installed and configured:

1.  **Hetzner Cloud Account**:
    *   You must have an active Hetzner Cloud account.
    *   Generate an **API Token** in the [Hetzner Cloud Console](https://console.hetzner.cloud/) under your project's "Security" > "API Tokens" tab.
    *   Ensure you have an **SSH Key** added to your project. You will need the ID of this key to provision servers that you can access via SSH (though HPort handles most management for you).

2.  **.NET 9.0 SDK**:
    *   HPort is built on .NET 9.0. Download and install it from [dotnet.microsoft.com](https://dotnet.microsoft.com/download/dotnet/9.0).

3.  **Git** (Optional):
    *   Required if you intend to clone the repository and build from source.

## Building from Source

Currently, HPort is distributed as source code. Follow these steps to build the CLI tool:

1.  **Clone the Repository**:
    ```bash
    git clone https://github.com/ItsNameless/hport.git
    cd hport
    ```

2.  **Build the Solution**:
    ```bash
    dotnet build -c Release
    ```

3.  **Run the CLI**:
    You can run the CLI directly using `dotnet run`:
    ```bash
    dotnet run --project src/ItsNameless.HPortCli -- [command]
    ```

    *Alternatively, you can publish it as a standalone executable:*
    ```bash
    dotnet publish src/ItsNameless.HPortCli -c Release -o ./publish
    ./publish/hport [command]
    ```
