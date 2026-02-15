using System.Globalization;
using DotMake.CommandLine;
using ItsNameless.HPort;
using ItsNameless.HPort.Test.Utils;
using ItsNameless.HPortCli.Commands;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;

namespace ItsNameless.HPortCli.Test;

public abstract class CliTestBase
{
    protected IHPort HPortMock { get; private set; } = null!;
    protected Fakers Fakers { get; private set; } = null!;
    protected StringWriter Output { get; private set; } = null!;
    protected StringWriter Error { get; private set; } = null!;

    [OneTimeSetUp]
    public void OneTimeSetup()
    {
        Cli.Ext.ConfigureServices(services =>
        {
            services.AddSingleton<IHPort, HPortWrapper>();
        });
    }

    [SetUp]
    public void Setup()
    {
        Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
        Thread.CurrentThread.CurrentUICulture = CultureInfo.InvariantCulture;

        HPortMock = Substitute.For<IHPort>();
        RegisterMock(HPortMock);
        
        Fakers = new Fakers();
        Output = new StringWriter();
        Error = new StringWriter();
    }

    /// <summary>
    /// Registers the mock instance of <see cref="IHPort"/> for the current test execution.
    /// </summary>
    protected void RegisterMock(IHPort mock)
    {
        HPortWrapper.Current = mock;
    }

    [TearDown]
    public void TearDown()
    {
        Output.Dispose();
        Error.Dispose();
    }

    protected async Task<int> RunCommand(params string[] args)
    {
        // Reset StringWriters if they were used before in the same test
        await Output.DisposeAsync();
        await Error.DisposeAsync();
        Output = new StringWriter();
        Error = new StringWriter();

        return await Cli.RunAsync<RootCommand>(args, new CliSettings
        {
            Output = Output,
            Error = Error,
            EnableDefaultExceptionHandler = false
        });
    }
}
