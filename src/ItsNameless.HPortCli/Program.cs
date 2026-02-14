using DotMake.CommandLine;
using ItsNameless.HPort;
using ItsNameless.HPortCli.Commands;
using Microsoft.Extensions.DependencyInjection;

var token = Environment.GetEnvironmentVariable("HPORT_TOKEN");
var stateFile = Environment.GetEnvironmentVariable("HPORT_STATE_FILE") ?? "serverStates.json";

if (string.IsNullOrEmpty(token))
{
    Console.Error.WriteLine("Error: HPORT_TOKEN environment variable is not set.");
    return;
}

var hport = await HPort.WithDefaultsAsync(token, stateFile);

Cli.Ext.ConfigureServices(services =>
{
    services.AddSingleton<IHPort>(_ => hport);
});

await Cli.RunAsync<RootCommand>(args, new CliSettings
    {
        EnableDefaultExceptionHandler = false,
        EnableSuggestDirective = true,
        EnableDiagramDirective = true,
        EnableEnvironmentVariablesDirective = true,
        ProcessTerminationTimeout = null,
        ResponseFileTokenReplacer = null,
        Output = null,
        Error = null,
        Theme = CliTheme.Blue
    }
);
