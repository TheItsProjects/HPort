using ItsNameless.HPort;
using ItsNameless.HPort.Services;

namespace ItsNameless.HPortCli.Test;

/// <summary>
/// A wrapper for <see cref="IHPort"/> that allows swapping the implementation at runtime.
/// <para>
/// This is required because <c>DotMake.CommandLine</c> uses a static configuration for dependency injection via <c>Cli.Ext.ConfigureServices</c>.
/// By registering this wrapper as a singleton, we can inject it once, and then change the <see cref="Current"/> property
/// in each test's setup to provide a fresh mock.
/// </para>
/// </summary>
public class HPortWrapper : IHPort
{
    /// <summary>
    /// The current implementation of <see cref="IHPort"/> to use.
    /// Set this property in your test setup (e.g., via <see cref="CliTestBase.RegisterMock"/>).
    /// </summary>
    public static IHPort Current { get; set; } = null!;

    public IContainerService Container
    {
        get => Current.Container;
        init => throw new NotSupportedException();
    }
}
