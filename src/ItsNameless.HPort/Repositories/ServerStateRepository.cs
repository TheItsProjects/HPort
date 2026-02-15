using System.IO.Abstractions;
using System.Text.Json;
using ItsNameless.HPort.Models;

namespace ItsNameless.HPort.Repositories;

/// <summary>
/// Repository for managing the local state of servers.
/// </summary>
[GenerateAutoInterface]
internal class ServerStateRepository : IServerStateRepository
{
    private readonly string _filePath;
    private readonly IFileSystem _fileSystem;
    private readonly SemaphoreSlim _lock = new(1, 1);
    private List<ServerState> _serverStates = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="ServerStateRepository"/> class.
    /// </summary>
    /// <param name="filePath">Path to the .json config file to save server states to.</param>
    /// <param name="fileSystem">The file system to use for saving and loading the server states.</param>
    public ServerStateRepository(string filePath, IFileSystem fileSystem)
    {
        _filePath = filePath;
        _fileSystem = fileSystem;
    }

    /// <summary>
    /// Sets up the repository by loading the server states from the file.
    /// This should be called before any other operations.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    public async Task SetupAsync(CancellationToken cancellationToken = default)
    {
        await _lock.WaitAsync(cancellationToken);
        try
        {
            await LoadServerStatesAsync(cancellationToken);
        }
        finally
        {
            _lock.Release();
        }
    }

    /// <summary>
    /// Sets up the repository by loading the server states from the file.
    /// This should be called before any other operations.
    /// </summary>
    public void Setup()
    {
        _lock.Wait();
        try
        {
            LoadServerStates();
        }
        finally
        {
            _lock.Release();
        }
    }

    /// <summary>
    /// Gets a server state by name.
    /// </summary>
    /// <param name="name">The name of the server.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The server state if found, otherwise null.</returns>
    public async Task<ServerState?> GetAsync(
        string name,
        CancellationToken cancellationToken = default)
    {
        await _lock.WaitAsync(cancellationToken);
        try
        {
            await LoadServerStatesAsync(cancellationToken);
            return _serverStates.SingleOrDefault(s => s.Name == name);
        }
        finally
        {
            _lock.Release();
        }
    }

    /// <summary>
    /// Gets all server states.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A list of all server states.</returns>
    public async Task<List<ServerState>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        await _lock.WaitAsync(cancellationToken);
        try
        {
            await LoadServerStatesAsync(cancellationToken);
            return _serverStates.ToList();
        }
        finally
        {
            _lock.Release();
        }
    }

    /// <summary>
    /// Adds a new server state.
    /// </summary>
    /// <param name="state">The server state to add.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    public async Task AddAsync(
        ServerState state,
        CancellationToken cancellationToken = default)
    {
        await _lock.WaitAsync(cancellationToken);
        try
        {
            await LoadServerStatesAsync(cancellationToken);
            if (_serverStates.Any(s => s.Name == state.Name))
            {
                throw new ArgumentException(
                    $"Server with name {state.Name} already exists in state."
                );
            }

            _serverStates.Add(state);
            await SaveServerStatesAsync(cancellationToken);
        }
        finally
        {
            _lock.Release();
        }
    }

    /// <summary>
    /// Removes a server state by name.
    /// </summary>
    /// <param name="name">The name of the server state to remove.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    public async Task RemoveAsync(
        string name,
        CancellationToken cancellationToken = default)
    {
        await _lock.WaitAsync(cancellationToken);
        try
        {
            await LoadServerStatesAsync(cancellationToken);
            var state = _serverStates.SingleOrDefault(s => s.Name == name);
            if (state != null)
            {
                _serverStates.Remove(state);
                await SaveServerStatesAsync(cancellationToken);
            }
        }
        finally
        {
            _lock.Release();
        }
    }

    /// <summary>
    /// Prunes server states that are not in the provided list of active IDs.
    /// </summary>
    /// <param name="activeIds">The IDs of active servers.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    public async Task PruneAsync(
        IEnumerable<long> activeIds,
        CancellationToken cancellationToken = default)
    {
        await _lock.WaitAsync(cancellationToken);
        try
        {
            await LoadServerStatesAsync(cancellationToken);
            var activeIdSet = activeIds.ToHashSet();
            var toRemove =
                _serverStates.Where(s => !activeIdSet.Contains(s.Id)).ToList();

            if (toRemove.Any())
            {
                foreach (var state in toRemove)
                {
                    _serverStates.Remove(state);
                }

                await SaveServerStatesAsync(cancellationToken);
            }
        }
        finally
        {
            _lock.Release();
        }
    }

    private async Task LoadServerStatesAsync(
        CancellationToken cancellationToken)
    {
        if (!_fileSystem.File.Exists(_filePath))
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
            JsonSerializer.Deserialize<List<ServerState>>(json) ?? [];
    }

    private void LoadServerStates()
    {
        if (!_fileSystem.File.Exists(_filePath))
        {
            _serverStates = [];
            return;
        }

        var json = _fileSystem.File.ReadAllText(_filePath);
        _serverStates =
            JsonSerializer.Deserialize<List<ServerState>>(json) ?? [];
    }

    private async Task SaveServerStatesAsync(
        CancellationToken cancellationToken)
    {
        var json =
            JsonSerializer.Serialize(
                _serverStates,
                new JsonSerializerOptions { WriteIndented = true }
            );
        await _fileSystem.File.WriteAllTextAsync(
            _filePath,
            json,
            cancellationToken
        );
    }
}
