using ItsNameless.HPort.Services;

namespace ItsNameless.HPort;

/// <summary>
/// Class used for interacting with containers on Hetzner servers.
/// </summary>
public interface IHPort
{
    /// <summary>
    /// Service for interacting with containers on Hetzner servers.
    /// </summary>
    IContainerService Container { get; init; }
}
