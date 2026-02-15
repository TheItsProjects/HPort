using System.IO.Abstractions;
using ItsNameless.HPort.Repositories;
using ItsNameless.HPort.Services;

namespace ItsNameless.HPort;

/// <inheritdoc />
public class HPort : IHPort
{
    /// <inheritdoc />
    public IContainerService Container { get; init; }

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
    /// <returns>A new instance of <see cref="HPort"/>.</returns>
    public static HPort WithDefaults(
        string hetznerToken,
        string serverStatesFilePath)
    {
        var fileSystem = new FileSystem();
        var hetznerCloudClient = new HetznerCloudClient(hetznerToken);
        var serverStateRepository =
            new ServerStateRepository(serverStatesFilePath, fileSystem);
        var serverRepository =
            new ServerRepository(
                hetznerCloudClient,
                serverStateRepository
            );
        var containerRepository = new ContainerRepository(serverRepository);
        var containerService =
            new ContainerService(
                serverRepository,
                containerRepository,
                fileSystem
            );

        serverRepository.Setup();

        return new HPort(containerService);
    }

    /// <inheritdoc cref="WithDefaults"/>
    public static async Task<HPort> WithDefaultsAsync(
        string hetznerToken,
        string serverStatesFilePath,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var fileSystem = new FileSystem();
        var hetznerCloudClient = new HetznerCloudClient(hetznerToken);
        var serverStateRepository =
            new ServerStateRepository(serverStatesFilePath, fileSystem);
        var serverRepository =
            new ServerRepository(
                hetznerCloudClient,
                serverStateRepository
            );
        var containerRepository = new ContainerRepository(serverRepository);
        var containerService =
            new ContainerService(
                serverRepository,
                containerRepository,
                fileSystem
            );

        await serverRepository.SetupAsync(cancellationToken);

        return new HPort(containerService);
    }
}
