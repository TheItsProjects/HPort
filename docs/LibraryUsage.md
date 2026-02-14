# Library Usage

HPort is designed as a "Library First" solution. `ItsNameless.HPort` can be referenced in any .NET 9.0 application to programmatically manage infrastructure.

## Getting Started

1.  **Reference the Project**: Add a reference to `ItsNameless.HPort` in your solution.
2.  **Initialize**: Use the factory method to create an instance.

## Code Example

```csharp
using ItsNameless.HPort;
using ItsNameless.HPort.Models;

public class InfrastructureManager
{
    public async Task DeployApp()
    {
        // 1. Initialize HPort
        // Provide your token and a path to store state
        var hport = await HPort.WithDefaultsAsync(
            hetznerToken: "YOUR_API_TOKEN",
            serverStatesFilePath: "serverStates.json"
        );

        // 2. Define Deployment Config
        var containerName = "production-api";
        var composeFile = "./docker-compose.yml";
        var envFile = "./.env";

        try 
        {
            Console.WriteLine("Deploying container...");

            // 3. Create Container (and Server if needed)
            var container = await hport.Container.CreateContainer(
                containerName: containerName,
                serverType: PortServerType.cpx11, // 2 vCPU, 4GB RAM
                datacenter: PortDatacenter.fsn1,  // Falkenstein
                composeFilePath: composeFile,
                envFilePath: envFile
            );

            Console.WriteLine($"Deployed {container.Name} to {container.Server.Name}!");
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Deployment failed: {ex.Message}");
        }
    }

    public async Task ListApps()
    {
        var hport = await HPort.WithDefaultsAsync("TOKEN", "state.json");
        
        var containers = await hport.Container.GetContainers();
        
        foreach (var c in containers)
        {
            Console.WriteLine($"- {c.Name} (Server: {c.Server.Name} / IP: {c.Server.Ip})");
        }
    }
}
```

## Dependency Injection

HPort is built with DI in mind. The `HPort` class implements `IHPort`, and the underlying services `IContainerService` and `IServerRepository` are also exposed interfaces.

You can register it in your `IServiceCollection`:

```csharp
services.AddSingleton<IHPort>(sp => 
    HPort.WithDefaults(
        config["HetznerToken"], 
        config["StateFilePath"]
    )
);
```
