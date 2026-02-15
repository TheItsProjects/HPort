namespace ItsNameless.HPort.Exceptions;

/// <summary>
/// Thrown when a server is not ready within the expected timeframe.
/// </summary>
public class ServerNotReadyException : HPortException
{
    /// <inheritdoc />
    public ServerNotReadyException()
    {
    }

    /// <inheritdoc />
    public ServerNotReadyException(string message) : base(message)
    {
    }

    /// <inheritdoc />
    public ServerNotReadyException(string message, Exception inner) : base(message, inner)
    {
    }
}
