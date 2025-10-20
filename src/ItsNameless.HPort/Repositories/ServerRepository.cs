using System.IO.Abstractions;
using System.Runtime.CompilerServices;
using System.Text.Json;
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
    private readonly string _filePath;
    private readonly IFileSystem _fileSystem;
    private List<ServerState> _serverStates = new();

    private readonly HetznerCloudClient _hetznerCloudClient;

    /// <summary>
    /// Initializes a new instance of the <see cref="ServerRepository"/> class.
    /// </summary>
    /// <param name="filePath">Path to the .json config file to save server states to.</param>
    /// <param name="hetznerCloudClient">The client to use for interacting with the Hetzner API.</param>
    /// <param name="fileSystem">The file system to use for saving and loading the server states.</param>
    public ServerRepository(
        string filePath,
        HetznerCloudClient hetznerCloudClient,
        IFileSystem fileSystem)
    {
        _filePath = filePath;
        _hetznerCloudClient = hetznerCloudClient;
        _fileSystem = fileSystem;
    }

    /// <summary>
    /// Sets up the repository by loading the server states from the file.
    /// This should be called before any other operations.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    public async Task SetupAsync(CancellationToken cancellationToken = default)
    {
        await LoadServerStatesAsync(cancellationToken);
    }

    /// <summary>
    /// Sets up the repository by loading the server states from the file.
    /// This should be called before any other operations.
    /// </summary>
    public void Setup()
    {
        LoadServerStates();
    }

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
            _serverStates.SingleOrDefault(s => s.Name == serverName);
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

        List<PortServer> servers = [];
        foreach (var serverState in _serverStates)
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
    /// <param name="sshKeyId">An optional ID for an SSH Key to add as root login. Must exist on Hetzner.</param>
    /// <param name="initialContainerName">The name for the first container to add.</param>
    /// <param name="initialCompose">The compose for the first container to add.</param>
    /// <param name="initialEnv">The env for the first container to add.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The created Server.</returns>
    /// <exception cref="ArgumentException">Thrown when the server already exists.</exception>
    /// <exception cref="InvalidOperationException">Thrown when there was an error creating the server.</exception>
    public async Task<PortServer> CreateServer(
        string serverName,
        PortServerType serverType,
        PortDatacenter datacenter,
        long? sshKeyId,
        string initialContainerName,
        string initialCompose,
        string initialEnv,
        CancellationToken cancellationToken = default)
    {
        await RefreshServerStates(cancellationToken);

        if (_serverStates.Select(state => state.Name).Contains(serverName))
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
                    userData: cloudConfig
                );
        }
        catch (Exception e) // TODO what exception?
        {
            Console.WriteLine(e);
            throw;
        }

        await AddServerState(
            serverName,
            server.Id,
            userPassword,
            cancellationToken
        );

        var errorMessage =
            await ServerIsReady(
                server.Id,
                initialContainerName,
                userPassword,
                cancellationToken
            );
        if (errorMessage is not null)
        {
            throw new InvalidOperationException(
                $"Server {serverName} is not ready: {errorMessage}"
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
    /// <exception cref="ArgumentException">Thrown when the server does not exist.</exception>
    public async Task DeleteServer(
        string serverName,
        CancellationToken cancellationToken = default)
    {
        await RefreshServerStates(cancellationToken);

        var serverState =
            _serverStates.SingleOrDefault(s => s.Name == serverName);
        if (serverState == null)
        {
            throw new ArgumentException(
                $"Server with name {serverName} does not exist."
            );
        }

        var server = await _hetznerCloudClient.Server.Get(serverState.Id);

        await _hetznerCloudClient.Server.Delete(server.Id);

        _serverStates.Remove(serverState);
        await SaveServerStates(cancellationToken);
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
    /// <exception cref="ArgumentException">Thrown when the given server does not exist.</exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the server does not have a public IP Address
    /// or there was an error running a command.
    /// </exception>
    public async IAsyncEnumerable<string> ExecuteCommandsOnServer(
        string serverName,
        IEnumerable<string> commands,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        await RefreshServerStates(cancellationToken);

        var serverState =
            _serverStates.SingleOrDefault(s => s.Name == serverName);
        if (serverState == null)
        {
            throw new ArgumentException(
                $"Server with name {serverName} does not exist."
            );
        }

        var server = await _hetznerCloudClient.Server.Get(serverState.Id);
        var serverIp = server.PublicNet.Ipv4;

        if (serverIp == null)
        {
            throw new InvalidOperationException(
                $"Server {serverName} does not have a public IP address."
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
                            $"docker compose -f /home/{PortServer.User}/{containerName}/docker-compose.yml ps --format '{{{{.Names}}}}'",
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
        await RefreshServerStates(cancellationToken);
        _serverStates.Add(
            new ServerState
            {
                Name = serverName,
                Id = id,
                UserPassword = userPassword
            }
        );
        await SaveServerStates(cancellationToken);
    }

    private async Task RefreshServerStates(
        CancellationToken cancellationToken = default)
    {
        await LoadServerStatesAsync(cancellationToken);

        var actualServers = await _hetznerCloudClient.Server.Get();
        var actualServerIds = actualServers.Select(s => s.Id).ToList();
        foreach (var server in _serverStates.ToList())
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (actualServerIds.Contains(server.Id))
            {
                continue;
            }

            _serverStates.Remove(server);
        }

        await SaveServerStates(cancellationToken);
    }

    private async Task SaveServerStates(
        CancellationToken cancellationToken = default)
    {
        var json =
            JsonSerializer.Serialize(
                _serverStates,
                new JsonSerializerOptions { WriteIndented = true, }
            );
        await _fileSystem.File.WriteAllTextAsync(
            _filePath,
            json,
            cancellationToken
        );
    }

    private async Task LoadServerStatesAsync(
        CancellationToken cancellationToken = default)
    {
        if (!File.Exists(_filePath))
        {
            _serverStates = [];
            return;
        }

        var json =
            await _fileSystem.File.ReadAllTextAsync(
                _filePath,
                cancellationToken
            );
        _serverStates =
            JsonSerializer.Deserialize<List<ServerState>>(json) ??
            [];
    }

    private void LoadServerStates()
    {
        if (!File.Exists(_filePath))
        {
            _serverStates = [];
            return;
        }

        var json = _fileSystem.File.ReadAllText(_filePath);
        _serverStates =
            JsonSerializer.Deserialize<List<ServerState>>(json) ??
            [];
    }

    private static async IAsyncEnumerable<string> ExecuteCommands(
        Ipv4 serverIp,
        string userPassword,
        IEnumerable<string> commands,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        using var sshClient =
            new SshClient(
                serverIp.Ip,
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
            throw new InvalidOperationException(
                $"Error executing setup command '{command}': {sshCommand.Error.Trim()}."
            );
        }

        sshClient.Disconnect();
    }

    private async Task<string?> ServerIsReady(
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
                return "Server not found on Hetzner";
            }

            switch (server.Status)
            {
                case SERVER_CREATING_STATUS or SERVER_STARTING_STATUS:
                    continue;
                case SERVER_READY_STATUS:
                    isReady = true;
                    break;
                default:
                    return $"Unknown server status: {server.Status}";
            }
        }

        // Wait 10s for the server to be fully initialized
        await Task.Delay(10_000, cancellationToken);

        // Check if there are no errors in the cloud-init logs
        var errorLogs =
            await ExecuteCommands(
                    server.PublicNet.Ipv4,
                    userPassword,
                    [
                        "sudo awk '/^Error/ {print; c=5; next} c {print; c--}' /var/log/cloud-init.log",
                        "sudo awk '/^Error/ {print; c=5; next} c {print; c--}' /var/log/cloud-init-output.log",
                    ],
                    cancellationToken
                )
                .ToListAsync(cancellationToken);

        if (errorLogs.WhereNotEmptyOrNull().Any())
        {
            return
                $"Errors found when initializing server: {string.Join("\n", errorLogs)}";
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
            return "Container is not running. Please check the logs.";
        }

        return null;
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

    private record ServerState
    {
        public required string Name { get; init; }
        public required long Id { get; init; }
        public required string UserPassword { get; init; }
    }
}
