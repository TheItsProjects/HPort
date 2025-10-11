using System.IO.Abstractions;
using ItsNameless.HPort.Repositories;
using ItsNameless.HPort.Services;

namespace ItsNameless.HPort;

/// <summary>
/// Class used for interacting with containers on Hetzner servers.
/// </summary>
[GenerateAutoInterface]
public class HPort : IHPort
{
    /// <summary>
    /// Service for interacting with containers on Hetzner servers.
    /// </summary>
    public readonly IContainerService Container;

    /// <summary>
    /// Initializes a new instance of the <see cref="HPort"/> class.
    /// </summary>
    /// <param name="containerService"></param>
    public HPort(IContainerService containerService)
    {
        Container = containerService;
    }

    /// <summary>
    /// Creates an instance of <see cref="HPort"/> with default settings.
    /// </summary>
    /// <param name="hetznerToken">The API token used to access the Hetzner API.</param>
    /// <param name="serverStatesFilePath">
    ///     The path to a JSON file storing information about managed services, e.g. their passwords.
    /// </param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A new instance of <see cref="HPort"/>.</returns>
    public static async Task<HPort> WithDefaults(
        string hetznerToken,
        string serverStatesFilePath,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var fileSystem = new FileSystem();
        var hetznerCloudClient = new HetznerCloudClient(hetznerToken);
        var serverRepository =
            new ServerRepository(
                serverStatesFilePath,
                hetznerCloudClient,
                fileSystem
            );
        var containerRepository = new ContainerRepository(serverRepository);
        var containerService =
            new ContainerService(
                serverRepository,
                containerRepository,
                fileSystem
            );

        await serverRepository.Setup(cancellationToken);

        return new HPort(containerService);
    }
}
