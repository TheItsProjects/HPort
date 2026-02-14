using DotMake.CommandLine;

namespace ItsNameless.HPortCli.Commands;

[CliCommand(
    Description = "Create and Manage Docker Containers on Hetzner Servers",
    Alias = "cont",
    Parent = typeof(RootCommand)
)]
public class ContainerCommand
{
}
