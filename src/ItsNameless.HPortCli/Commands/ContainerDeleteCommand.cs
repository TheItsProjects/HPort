using DotMake.CommandLine;
using ItsNameless.HPort;

namespace ItsNameless.HPortCli.Commands;

[CliCommand(
    Description = "Delete a Docker Container from a Hetzner Server",
    Name = "delete",
    Alias = "d",
    Parent = typeof(ContainerCommand)
)]
public class ContainerDeleteCommand(IHPort hPort)
{
    private readonly IHPort _hPort = hPort;

    [CliOption(
        Description = "Name of the container to delete",
        Name = "name",
        Alias = "n"
    )]
    public required string ContainerName { get; set; }

    [CliOption(
        Description = "Name of the server the container is on",
        Name = "server",
        Alias = "s"
    )]
    public required string ServerName { get; set; }

    [CliOption(
        Description = "Whether to delete the server if it is empty after deleting the container",
        Name = "delete-server",
        Alias = "ds"
    )]
    public bool DeleteServerIfEmpty { get; set; } = true;

    public async Task RunAsync(CliContext context)
    {
        context.Output.WriteLine($"Deleting container '{ContainerName}' from server '{ServerName}'...");

        try
        {
            var container = await _hPort.Container.DeleteContainer(
                ContainerName,
                ServerName,
                DeleteServerIfEmpty
            );

            context.Output.WriteLine($"Successfully deleted container '{container.Name}'.");
            
            if (DeleteServerIfEmpty)
            {
                context.Output.WriteLine("Server was also deleted if it was empty.");
            }
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error: {ex.Message}");
        }
    }
}
