using DotMake.CommandLine;
using ItsNameless.HPort;

namespace ItsNameless.HPortCli.Commands;

[CliCommand(
    Description = "List all Docker Containers",
    Name = "list",
    Alias = "l",
    Parent = typeof(ContainerCommand)
)]
public class ContainerListCommand(IHPort hPort)
{
    private readonly IHPort _hPort = hPort;

    [CliOption(
        Description = "Name of the server to search on",
        Name = "server",
        Alias = "s",
        Required = false
    )]
    public string? ServerName { get; set; }

    public async Task RunAsync(CliContext context)
    {
        try
        {
            var containers = await _hPort.Container.GetContainers(ServerName);

            if (containers.Count == 0)
            {
                await context.Output.WriteLineAsync("No containers found.");
                return;
            }

            await context.Output.WriteLineAsync($"{"Container Name",-20} | {"Server Name",-20}");
            await context.Output.WriteLineAsync(new string('-', 43));

            foreach (var container in containers)
            {
                await context.Output.WriteLineAsync($"{container.Name,-20} | {container.Server.Name,-20}");
            }
        }
        catch (Exception ex)
        {
            await context.Error.WriteLineAsync($"Error: {ex.Message}");
        }
    }
}
