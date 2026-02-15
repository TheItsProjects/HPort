namespace ItsNameless.HPort.Models;

/// <summary>
/// Represents the local state of a server managed by HPort, including information that is not stored in the Hetzner API, such as the user password.
/// </summary>
internal record ServerState
{
    /// <summary>
    /// The name of the server.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// The ID of the server in the Hetzner API.
    /// </summary>
    public required long Id { get; init; }

    /// <summary>
    /// The password for the user used to connect to the server via SSH.
    /// </summary>
    public required string UserPassword { get; init; }
}
