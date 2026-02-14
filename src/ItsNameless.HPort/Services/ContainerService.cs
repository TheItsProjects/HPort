using System.IO.Abstractions;
using ItsNameless.HPort.Models;
using ItsNameless.HPort.Repositories;

namespace ItsNameless.HPort.Services;

/// <summary>
/// Service for managing containers on Hetzner servers.
/// </summary>
[GenerateAutoInterface]
public class ContainerService : IContainerService
{
    private readonly IServerRepository _serverRepository;
    private readonly IContainerRepository _containerRepository;
    private readonly IFileSystem _fileSystem;

    internal ContainerService(
        IServerRepository serverRepository,
        IContainerRepository containerRepository,
        IFileSystem fileSystem)
    {
        _serverRepository = serverRepository;
        _containerRepository = containerRepository;
        _fileSystem = fileSystem;
    }

    /// <summary>
    /// Creates a new container on a server. Depending on the options and the
    /// existing servers, creates a new server as well.
    /// </summary>
    /// <param name="containerName">The name of the container to add.</param>
    /// <param name="serverType">The server type.</param>
    /// <param name="datacenter">The datacenter.</param>
    /// <param name="composeFilePath">The docker compose file.</param>
    /// <param name="envFilePath">The .env file.</param>
    /// <param name="sshKeyId">The root SSH Key.</param>
    /// <param name="uniqueServer">Whether the service should receive a unique server.</param>
    /// <param name="networkId">The internal network ID.</param>
    /// <returns>The created <see cref="PortContainer"/>.</returns>
    /// <exception cref="InvalidOperationException">Thrown when there was an error creating the server.</exception>
    public async Task<PortContainer> CreateContainer(
        string containerName,
        PortServerType serverType,
        PortDatacenter datacenter,
        string composeFilePath,
        string envFilePath,
        long? sshKeyId = null,
        bool uniqueServer = false,
        long? networkId = null
    )
    {
        var composeContent =
            (await _fileSystem.File.ReadAllTextAsync(composeFilePath));
        var envContent =
            (await _fileSystem.File.ReadAllTextAsync(envFilePath));

        var serverName =
            ServerRepository.Name.GetServerName(
                containerName,
                serverType,
                datacenter,
                uniqueServer
            );

        var existingServer = await _serverRepository.GetServer(serverName);

        if (existingServer is not null && uniqueServer)
        {
            throw new InvalidOperationException(
                "Cannot create container with unique server, " +
                $"because a server with the name {serverName} already exists."
            );
        }

        if (existingServer is not null)
        {
            return await _containerRepository.CreateContainerOnExistingServer(
                existingServer,
                containerName,
                composeContent,
                envContent
            );
        }

        return await _containerRepository.CreateContainerOnNewServer(
            containerName,
            serverType,
            datacenter,
            sshKeyId,
            serverName,
            composeContent,
            envContent,
            networkId
        );
    }

    /// <summary>
    /// Returns a list of all containers on the specified server or all servers if no server is specified.
    /// </summary>
    /// <param name="serverName">The server to search on.</param>
    /// <returns>A list of containers.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the server does not exist.</exception>
    public async Task<List<PortContainer>> GetContainers(
        string? serverName = null)
    {
        if (serverName is null)
        {
            return await _containerRepository.GetAllContainers();
        }

        return await _containerRepository.GetAllContainers(serverName);
    }

    /// <summary>
    /// Executes a command in a container on a specific server and returns the result as a string.
    /// </summary>
    /// <param name="containerName">The name of the container to run the command in.</param>
    /// <param name="serverName">The name of the server the container is on.</param>
    /// <param name="serviceName">The specific service inside the container.</param>
    /// <param name="command">The command to run in the container.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The result text.</returns>
    /// <exception cref="InvalidOperationException">Thrown when there was an error while executing the command.</exception>
    public async Task<string> ExecuteCommandInContainer(
        string containerName,
        string serverName,
        string serviceName,
        string command,
        CancellationToken cancellationToken = default
    )
    {
        return await ExecuteCommandInContainer(
            containerName,
            serverName,
            serviceName,
            command,
            result => result,
            cancellationToken
        );
    }

    /// <summary>
    /// Executes a command in a container on a specific server and parses the result.
    /// </summary>
    /// <param name="containerName">The name of the container to run the command in.</param>
    /// <param name="serverName">The name of the server the container is on.</param>
    /// <param name="serviceName">The specific service inside the container.</param>
    /// <param name="command">The command to run in the container.</param>
    /// <param name="resultParser">A parser to parse the result into the required type.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <typeparam name="T">The required type to return.</typeparam>
    /// <returns>The <typeparamref name="T"/>.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the server does not exist or there was
    /// an error while executing the command.
    /// </exception>
    public async Task<T> ExecuteCommandInContainer<T>(
        string containerName,
        string serverName,
        string serviceName,
        string command,
        Func<string, T> resultParser,
        CancellationToken cancellationToken = default
    )
    {
        var server =
            await _serverRepository.GetServer(serverName, cancellationToken);

        if (server is null)
        {
            throw new InvalidOperationException(
                $"Server {serverName} does not exist."
            );
        }

        return await _containerRepository.ExecuteCommandInContainer(
            containerName,
            serverName,
            serviceName,
            command,
            resultParser,
            cancellationToken
        );
    }

    /// <summary>
    /// Deletes a container from a server.
    /// If the server is empty after deleting the container, by default it will also delete the server.
    /// </summary>
    /// <param name="containerName">The name of the container to delete.</param>
    /// <param name="serverName">The name of the server the container is on.</param>
    /// <param name="deleteServerIfEmpty">If true, deletes the server if it is empty.</param>
    /// <returns>The deleted container.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the server does not exist.</exception>
    public async Task<PortContainer> DeleteContainer(
        string containerName,
        string serverName,
        bool deleteServerIfEmpty = true)
    {
        return await _containerRepository.DeleteContainer(
            containerName,
            serverName,
            deleteServerIfEmpty
        );
    }
}
