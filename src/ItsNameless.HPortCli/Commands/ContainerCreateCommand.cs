using DotMake.CommandLine;
using ItsNameless.HPort;
using ItsNameless.HPort.Models;

namespace ItsNameless.HPortCli.Commands;

[CliCommand(
    Description = "Create a new Docker Container on a Hetzner Server",
    Name = "create",
    Alias = "c",
    Parent = typeof(ContainerCommand)
)]
public class ContainerCreateCommand(IHPort hPort)
{
    private readonly IHPort _hPort = hPort;

    [CliOption(
        Description = "Name of the container to manage",
        Name = "name",
        Alias = "n"
    )]
    public required string ContainerName { get; set; }

    [CliOption(
        Description = "Server Type to use for this container",
        Name = "type",
        Alias = "t"
    )]
    public required PortServerType ServerType { get; set; }

    [CliOption(
        Description = "Datacenter to use for this container",
        Name = "datacenter",
        Alias = "d"
    )]
    public required PortDatacenter Datacenter { get; set; }

    [CliOption(
        Description = "Path to the docker-compose file",
        Name = "compose",
        Alias = "c"
    )]
    public required string ComposeFilePath { get; set; }

    [CliOption(
        Description = "Path to the .env file",
        Name = "env",
        Alias = "e"
    )]
    public required string EnvFilePath { get; set; }

    [CliOption(
        Description =
            "SSH Key ID to use for this container in order to log in with your own device",
        Name = "ssh-key",
        Alias = "sk"
    )]
    public long? SshKeyId { get; set; }

    [CliOption(
        Description = "Whether to use a unique server for this container",
        Name = "is-unique",
        Alias = "u"
    )]
    public bool UniqueServer { get; set; } = false;

    public async Task RunAsync(CliContext context)
    {
        context.Output.WriteLine($"Creating container: {ContainerName}...");

        try
        {
            var container = await _hPort.Container.CreateContainer(
                ContainerName,
                ServerType,
                Datacenter,
                ComposeFilePath,
                EnvFilePath,
                SshKeyId,
                UniqueServer
            );

            context.Output.WriteLine(
                $"Successfully created container '{container.Name}' " +
                $"on server '{container.Server.Name}'."
            );
        }
        catch (Exception ex)
        {
            context.Error.WriteLine($"Error: {ex.Message}");
        }
    }
}
