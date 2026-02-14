namespace ItsNameless.HPort.Models;

/// <summary>
/// Represents a container used in HPort.
/// </summary>
public record PortContainer
{
    /// <summary>
    /// The name of the container.
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// The server that the container is on.
    /// </summary>
    public required PortServer Server { get; set; }
}
