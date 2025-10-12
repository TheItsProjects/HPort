using ItsNameless.HPort.Extensions;
using ItsNameless.HPort.Models;

namespace ItsNameless.HPort.Repositories;

/// <summary>
/// Repository for interacting with containers on Hetzner servers.
/// </summary>
[GenerateAutoInterface]
internal class ContainerRepository : IContainerRepository
{
    private readonly IServerRepository _serverRepository;

    /// <summary>
    /// Initializes a new instance of the <see cref="ContainerRepository"/> class.
    /// </summary>
    /// <param name="serverRepository">The <see cref="ServerRepository"/> to use.</param>
    public ContainerRepository(IServerRepository serverRepository)
    {
        _serverRepository = serverRepository;
    }


    /// <summary>
    /// Creates a new server and ads the container to it.
    /// </summary>
    /// <param name="containerName">The name of the container to add.</param>
    /// <param name="serverType">The server type.</param>
    /// <param name="datacenter">The datacenter.</param>
    /// <param name="sshKeyId">The root SSH Key.</param>
    /// <param name="serverName">The name of the new server.</param>
    /// <param name="composeContent">The content of the docker compose file.</param>
    /// <param name="envContent">The content of the .env file.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The created <see cref="PortContainer"/>.</returns>
    /// <exception cref="ArgumentException">Thrown when the server already exists.</exception>
    /// <exception cref="InvalidOperationException">Thrown when there was an error creating the server.</exception>
    public async Task<PortContainer> CreateContainerOnNewServer(
        string containerName,
        PortServerType serverType,
        PortDatacenter datacenter,
        long? sshKeyId,
        string serverName,
        string composeContent,
        string envContent,
        CancellationToken cancellationToken = default
    )
    {
        var server =
            await _serverRepository.CreateServer(
                serverName,
                serverType,
                datacenter,
                sshKeyId,
                containerName,
                composeContent,
                envContent,
                cancellationToken
            );

        return new PortContainer
        {
            Name = containerName,
            Server = server,
        };
    }


    /// <summary>
    /// Adds a container to an existing server.
    /// </summary>
    /// <param name="existingServer">The existing server.</param>
    /// <param name="containerName">The name of the container to add.</param>
    /// <param name="composeContent">The content of the docker compose file.</param>
    /// <param name="envContent">The content of the .env file.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The <see cref="PortContainer"/>.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when there was an error adding the container to the server.
    /// </exception>
    public async Task<PortContainer> CreateContainerOnExistingServer(
        PortServer existingServer,
        string containerName,
        string composeContent,
        string envContent,
        CancellationToken cancellationToken = default
    )
    {
        List<string> commands =
        [
            ServerRepository.Commands
                .CreateContainerDirectory(containerName),
            ServerRepository.Commands
                .CreateComposeFile(containerName, composeContent),
            ServerRepository.Commands
                .CreateEnvFile(containerName, envContent),
            ServerRepository.Commands.StartContainer(containerName),
            ServerRepository.Commands.CreateInitializedFile(containerName),
        ];

        try
        {
            var results =
                await _serverRepository.ExecuteCommandsOnServer(
                        existingServer.Name,
                        commands,
                        cancellationToken
                    )
                    .ToListAsync(cancellationToken);

            var containerIsRunning =
                await _serverRepository.CheckContainerIsRunning(
                    existingServer.Name,
                    containerName,
                    cancellationToken: cancellationToken
                );

            if (containerIsRunning is false)
            {
                throw new InvalidOperationException(
                    $"The container {containerName} is not running on server {existingServer.Name}."
                );
            }
        }
        catch (InvalidOperationException e)
        {
            throw new InvalidOperationException(
                $"Failed to create new container on server {existingServer.Name}.",
                e
            );
        }

        return new PortContainer
        {
            Name = containerName,
            Server = existingServer,
        };
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
    /// <exception cref="InvalidOperationException">Thrown when there was an error while executing the command.</exception>
    public async Task<T> ExecuteCommandInContainer<T>(
        string containerName,
        string serverName,
        string serviceName,
        string command,
        Func<string, T> resultParser,
        CancellationToken cancellationToken = default
    )
    {
        var dockerCommand =
            ServerRepository.Commands.ExecInContainer(
                containerName,
                serviceName,
                command
            );

        string result;
        try
        {
            result =
                await _serverRepository
                    .ExecuteCommandsOnServer(
                        serverName,
                        [dockerCommand],
                        cancellationToken
                    )
                    .SingleAsync(cancellationToken);
        }
        catch (InvalidOperationException e)
        {
            throw new InvalidOperationException(
                $"There was an error when trying to execute a command on the server: {e.Message}.",
                e
            );
        }

        if (string.IsNullOrWhiteSpace(result))
        {
            throw new InvalidOperationException(
                "The command did not return any result."
            );
        }

        try
        {
            return resultParser(result);
        }
        catch (Exception e)
        {
            throw new InvalidOperationException(
                "There was an error parsing the result of the command.",
                e
            );
        }
    }

    /// <summary>
    /// Returns all containers on all servers.
    /// </summary>
    /// <returns>The containers.</returns>
    public async Task<List<PortContainer>> GetAllContainers()
    {
        var servers = await _serverRepository.GetServers();

        var tasks =
            servers.Select(
                server => GetAllContainers(server.Name)
            );

        var results = await Task.WhenAll(tasks);

        return results.SelectMany(c => c).ToList();
    }

    /// <summary>
    /// Returns all containers on a specific server.
    /// </summary>
    /// <param name="serverName">A server to search on.</param>
    /// <returns>The containers.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the server does not exist.
    /// </exception>
    public async Task<List<PortContainer>> GetAllContainers(
        string serverName)
    {
        var server = await _serverRepository.GetServer(serverName);
        if (server is null)
        {
            throw new InvalidOperationException(
                $"Server {serverName} does not exist."
            );
        }

        var result =
            await _serverRepository
                .ExecuteCommandsOnServer(
                    server.Name,
                    [ServerRepository.Commands.GetContainers(),]
                )
                .SingleAsync();

        return result.Split("\n")
            .WhereNotEmptyOrNull()
            .Select(
                c =>
                    new PortContainer
                    {
                        Name = c.Trim(),
                        Server = server,
                    }
            )
            .ToList();
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
        var server = await _serverRepository.GetServer(serverName);

        if (server is null)
        {
            throw new InvalidOperationException(
                $"Server {serverName} does not exist."
            );
        }

        List<string> commands =
        [
            ServerRepository.Commands.StopContainer(containerName),
            ServerRepository.Commands.DeleteContainerDirectory(containerName),
        ];

        try
        {
            await _serverRepository.ExecuteCommandsOnServer(
                    server.Name,
                    commands
                )
                .ToListAsync();
        }
        catch (InvalidOperationException e)
        {
            throw new InvalidOperationException(
                $"There was an error while deleting the container: {e.Message}",
                e
            );
        }

        if (deleteServerIfEmpty is false)
        {
            return new PortContainer
            {
                Name = containerName,
                Server = server,
            };
        }

        var containers = await GetAllContainers(server.Name);

        if (containers.Any())
        {
            return new PortContainer
            {
                Name = containerName,
                Server = server,
            };
        }

        await _serverRepository.DeleteServer(server.Name);

        return new PortContainer
        {
            Name = containerName,
            Server = server,
        };
    }
}
