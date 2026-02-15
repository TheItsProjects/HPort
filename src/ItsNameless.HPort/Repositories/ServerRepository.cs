using System.Runtime.CompilerServices;
using ItsNameless.HPort.Exceptions;
using ItsNameless.HPort.Extensions;
using ItsNameless.HPort.Models;
using Renci.SshNet;

namespace ItsNameless.HPort.Repositories;

/// <summary>
/// Repository for interacting with Hetzner servers.
/// </summary>
[GenerateAutoInterface]
internal partial class ServerRepository : IServerRepository
{
    private readonly HetznerCloudClient _hetznerCloudClient;
    private readonly IServerStateRepository _stateRepository;

    /// <summary>
    /// Initializes a new instance of the <see cref="ServerRepository"/> class.
    /// </summary>
    /// <param name="hetznerCloudClient">The client to use for interacting with the Hetzner API.</param>
    /// <param name="stateRepository">The repository for managing server states.</param>
    public ServerRepository(
        HetznerCloudClient hetznerCloudClient,
        IServerStateRepository stateRepository)
    {
        _hetznerCloudClient = hetznerCloudClient;
        _stateRepository = stateRepository;
    }

    /// <summary>
    /// Sets up the repository by loading the server states from the file.
    /// This should be called before any other operations.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    public async Task SetupAsync(CancellationToken cancellationToken = default)
    {
        await _stateRepository.SetupAsync(cancellationToken);
    }

    /// <summary>
    /// Sets up the repository by loading the server states from the file.
    /// This should be called before any other operations.
    /// </summary>
    public void Setup() { _stateRepository.Setup(); }

    /// <summary>
    /// Gets the server from the local State data and the Hetzner API.
    /// </summary>
    /// <param name="serverName">The name to get the server for.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The <see cref="PortServer"/> or null if not found.</returns>
    public async Task<PortServer?> GetServer(
        string serverName,
        CancellationToken cancellationToken = default)
    {
        await RefreshServerStates(cancellationToken);

        var serverState =
            await _stateRepository.GetAsync(serverName, cancellationToken);
        if (serverState == null)
        {
            return null;
        }

        var server = await _hetznerCloudClient.Server.Get(serverState.Id);

        return PortServer.Create(server, serverState.UserPassword);
    }

    /// <summary>
    /// Gets all servers from the local State data and the Hetzner API.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A list of servers.</returns>
    public async Task<List<PortServer>> GetServers(
        CancellationToken cancellationToken = default)
    {
        await RefreshServerStates(cancellationToken);

        var serverStates =
            await _stateRepository.GetAllAsync(cancellationToken);
        List<PortServer> servers = [];
        foreach (var serverState in serverStates)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var server = await _hetznerCloudClient.Server.Get(serverState.Id);
            if (server != null)
            {
                servers.Add(
                    PortServer.Create(server, serverState.UserPassword)
                );
            }
        }

        return servers;
    }

    /// <summary>
    /// Create a new server with the given parameters.
    /// </summary>
    /// <param name="serverName">The name to use for the server.</param>
    /// <param name="serverType">The Hetzner <see cref="PortServerType"/> to use.</param>
    /// <param name="datacenter">The Hetzner <see cref="PortDatacenter"/> to use.</param>
    /// <param name="sshKeyId">An optional ID of an SSH key, existing on Hetzner, to attach to the server for SSH access.</param>
    /// <param name="initialContainerName">The name for the first container to add.</param>
    /// <param name="initialCompose">The compose for the first container to add.</param>
    /// <param name="initialEnv">The env for the first container to add.</param>
    /// <param name="networkId">The ID of the network to attach the server to.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The created Server.</returns>
    /// <exception cref="ArgumentException">Thrown when the server already exists.</exception>
    /// <exception cref="ServerCreationException">Thrown when there was an error creating the server.</exception>
    public async Task<PortServer> CreateServer(
        string serverName,
        PortServerType serverType,
        PortDatacenter datacenter,
        long? sshKeyId,
        string initialContainerName,
        string initialCompose,
        string initialEnv,
        long? networkId = null,
        CancellationToken cancellationToken = default)
    {
        await RefreshServerStates(cancellationToken);

        var existingServer =
            await _stateRepository.GetAsync(serverName, cancellationToken);
        if (existingServer != null)
        {
            throw new ArgumentException(
                $"Server with name {serverName} already exists"
            );
        }

        var userPassword = GenerateRandomPassword(30);

        var cloudConfig =
            GetCloudConfig(
                userPassword,
                initialContainerName,
                initialCompose.Replace("\n", "\n      "),
                initialEnv.Replace("\n", "\n      ")
            );

        Server server;
        try
        {
            server =
                await _hetznerCloudClient.Server.Create(
                    (long)datacenter,
                    DOCKER_IMAGE_ID,
                    serverName,
                    (long)serverType,
                    sshKeysIds: sshKeyId != null ? [sshKeyId.Value,] : [],
                    userData: cloudConfig,
                    privateNetoworksIds: networkId.HasValue
                        ? [networkId.Value,]
                        : [],
                    ipv4: !networkId.HasValue
                );
        }
        catch (Exception e)
        {
            throw new ServerCreationException(
                $"Failed to create server '{serverName}' on Hetzner: {e.Message}",
                e
            );
        }

        await AddServerState(
            serverName,
            server.Id,
            userPassword,
            cancellationToken
        );

        try
        {
            await ServerIsReady(
                server.Id,
                initialContainerName,
                userPassword,
                cancellationToken
            );
        }
        catch (Exception e) when (e is ServerNotReadyException
                                      or MissingIpException
                                      or ServerNotFoundException
                                      or SshConnectionException)
        {
            throw new ServerCreationException(
                $"Server '{serverName}' created but failed readiness check: {e.Message}",
                e
            );
        }

        return new PortServer
        {
            Name = serverName,
            Type = serverType,
            Datacenter = datacenter,
            UserPassword = userPassword
        };
    }

    /// <summary>
    /// Delete the server with the given name.
    /// </summary>
    /// <param name="serverName">The name of the server to delete.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <exception cref="ServerNotFoundException">Thrown when the server does not exist.</exception>
    public async Task DeleteServer(
        string serverName,
        CancellationToken cancellationToken = default)
    {
        await RefreshServerStates(cancellationToken);

        var serverState =
            await _stateRepository.GetAsync(serverName, cancellationToken);
        if (serverState == null)
        {
            throw new ServerNotFoundException(
                $"Server with name {serverName} does not exist."
            );
        }

        var server = await _hetznerCloudClient.Server.Get(serverState.Id);

        await _hetznerCloudClient.Server.Delete(server.Id);

        await _stateRepository.RemoveAsync(serverName, cancellationToken);
    }

    /// <summary>
    /// Executes several commands on the server in a single session using SSH.
    /// The commands are executed in the order they are provided.
    /// If any command fails, an exception is thrown with the error message.
    /// </summary>
    /// <param name="serverName">The server to run the commands on.</param>
    /// <param name="commands">The commands to run.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The result of each command.</returns>
    /// <exception cref="ServerNotFoundException">Thrown when the given server does not exist.</exception>
    /// <exception cref="MissingIpException">Thrown when the server does not have a public IP Address.</exception>
    /// <exception cref="SshConnectionException">Thrown when there was an error running a command.</exception>
    public async IAsyncEnumerable<string> ExecuteCommandsOnServer(
        string serverName,
        IEnumerable<string> commands,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        await RefreshServerStates(cancellationToken);

        var serverState =
            await _stateRepository.GetAsync(serverName, cancellationToken);
        if (serverState == null)
        {
            throw new ServerNotFoundException(
                $"Server with name {serverName} does not exist."
            );
        }

        var server = await _hetznerCloudClient.Server.Get(serverState.Id);
        string? serverIp = server.PublicNet.Ipv4?.Ip;
        if (serverIp == null && server.PrivateNet != null &&
            server.PrivateNet.Any())
        {
            serverIp = server.PrivateNet.First().Ip;
        }

        if (serverIp == null)
        {
            throw new MissingIpException(
                $"Server {serverName} does not have a public or private IP address."
            );
        }

        await foreach (var p in
                       ExecuteCommands(
                           serverIp,
                           serverState.UserPassword,
                           commands,
                           cancellationToken
                       ))
        {
            cancellationToken.ThrowIfCancellationRequested();

            yield return p;
        }
    }

    /// <summary>
    /// Checks if a container is running on the given server.
    /// Retries the check a number of times before failing,
    /// in case the container is still starting up.
    /// </summary>
    /// <param name="serverName">The server to check on.</param>
    /// <param name="containerName">The name of the container to check for.</param>
    /// <param name="retries">Number of tries before failing.</param>
    /// <param name="timeout">Time in milliseconds to wait between each retry.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>Whether the given container is running.</returns>
    public async Task<bool> CheckContainerIsRunning(
        string serverName,
        string containerName,
        int retries = 3,
        int timeout = 5000,
        CancellationToken cancellationToken = default)
    {
        for (var i = 0; i < retries; i++)
        {
            var runningContainers =
                await ExecuteCommandsOnServer(
                        serverName,
                        [
                            Commands.CheckContainerIsRunning(containerName),
                        ],
                        cancellationToken
                    )
                    .ToListAsync(cancellationToken);

            if (runningContainers.Single() != "")
            {
                return true;
            }

            await Task.Delay(timeout, cancellationToken);
        }

        return false;
    }

    private async Task AddServerState(
        string serverName,
        long id,
        string userPassword,
        CancellationToken cancellationToken = default)
    {
        await _stateRepository.AddAsync(
            new ServerState
            {
                Name = serverName,
                Id = id,
                UserPassword = userPassword
            },
            cancellationToken
        );
    }

    private async Task RefreshServerStates(
        CancellationToken cancellationToken = default)
    {
        var actualServers = await _hetznerCloudClient.Server.Get();
        var actualServerIds = actualServers.Select(s => s.Id).ToList();

        await _stateRepository.PruneAsync(actualServerIds, cancellationToken);
    }

    private static async IAsyncEnumerable<string> ExecuteCommands(
        string serverIp,
        string userPassword,
        IEnumerable<string> commands,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        using var sshClient =
            new SshClient(
                serverIp,
                PortServer.User,
                userPassword
            );

        await sshClient.TryConnectAsync(cancellationToken: cancellationToken);

        foreach (var command in commands)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var sshCommand = sshClient.CreateCommand(command);
            await sshCommand.ExecuteAsync(cancellationToken);

            if (sshCommand.ExitStatus is 0)
            {
                yield return GetCommandResult(sshCommand);
                continue;
            }

            sshClient.Disconnect();
            throw new SshConnectionException(
                $"Error executing setup command '{command}': {sshCommand.Error.Trim()}."
            );
        }

        sshClient.Disconnect();
    }

    private async Task ServerIsReady(
        long serverId,
        string containerName,
        string userPassword,
        CancellationToken cancellationToken = default)
    {
        Server server = null!;
        bool isReady = false;
        while (isReady is false)
        {
            cancellationToken.ThrowIfCancellationRequested();

            server = await _hetznerCloudClient.Server.Get(serverId);

            if (server == null)
            {
                throw new ServerNotFoundException(
                    "Server not found on Hetzner"
                );
            }

            switch (server.Status)
            {
                case SERVER_CREATING_STATUS or SERVER_STARTING_STATUS:
                    await Task.Delay(2000, cancellationToken);
                    continue;
                case SERVER_READY_STATUS:
                    isReady = true;
                    break;
                default:
                    throw new ServerNotReadyException(
                        $"Unknown server status: {server.Status}"
                    );
            }
        }

        // Wait 10s for the server to be fully initialized
        await Task.Delay(10_000, cancellationToken);

        string? serverIp = server.PublicNet.Ipv4?.Ip;
        if (serverIp == null && server.PrivateNet != null &&
            server.PrivateNet.Any())
        {
            serverIp = server.PrivateNet.First().Ip;
        }

        if (serverIp == null)
        {
            throw new MissingIpException(
                "Server does not have a public or private IP address."
            );
        }

        // Check if there are no errors in the cloud-init logs
        var errorLogs =
            await ExecuteCommands(
                    serverIp,
                    userPassword,
                    Commands.CheckCloudInitLogs(),
                    cancellationToken
                )
                .ToListAsync(cancellationToken);

        if (errorLogs.WhereNotEmptyOrNull().Any())
        {
            throw new ServerNotReadyException(
                $"Errors found when initializing server: {string.Join("\n", errorLogs)}"
            );
        }

        // Check if a container is running
        var containerIsRunning =
            await CheckContainerIsRunning(
                server.Name,
                containerName,
                cancellationToken: cancellationToken
            );

        if (containerIsRunning is false)
        {
            throw new ServerNotReadyException(
                "Container is not running. Please check the logs."
            );
        }
    }

    /// <summary>
    /// Returns the result of the command, or the error if the result is empty.
    /// This handling is required because docker commands may return empty
    /// results with the actual output in the error field (because there was
    /// an unrelated warning).
    /// </summary>
    /// <param name="command">The <see cref="SshCommand"/> to get the result from.</param>
    /// <returns>The result text.</returns>
    private static string GetCommandResult(SshCommand command)
    {
        return string.IsNullOrEmpty(command.Result.Trim())
            ? command.Error.Trim()
            : command.Result.Trim();
    }
}
