using DotMake.CommandLine;
using ItsNameless.HPort;

namespace ItsNameless.HPortCli.Commands;

[CliCommand(
    Description = "Execute a command in a Docker Container",
    Name = "execute",
    Alias = "e",
    Parent = typeof(ContainerCommand)
)]
public class ContainerExecuteCommand(IHPort hPort)
{
    private readonly IHPort _hPort = hPort;

    [CliOption(
        Description = "Name of the container to run the command in",
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
        Description = "The specific service inside the container (as defined in docker-compose)",
        Name = "service",
        Alias = "svc"
    )]
    public required string ServiceName { get; set; }

    [CliOption(
        Description = "The command to run in the container",
        Name = "command",
        Alias = "cmd"
    )]
    public required string Command { get; set; }

    public async Task<int> RunAsync(CliContext context)
    {
        try
        {
            var result = await _hPort.Container.ExecuteCommandInContainer(
                ContainerName,
                ServerName,
                ServiceName,
                Command
            );

            await context.Output.WriteLineAsync(result);
        }
        catch (Exception ex)
        {
            await context.Error.WriteLineAsync($"Error: {ex.Message}");
            return 1;
        }

        return 0;
    }
}
