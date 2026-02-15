namespace ItsNameless.HPort.Exceptions;

/// <summary>
/// Thrown when a server cannot be found.
/// </summary>
public class ServerNotFoundException : HPortException
{
    /// <inheritdoc />
    public ServerNotFoundException()
    {
    }

    /// <inheritdoc />
    public ServerNotFoundException(string message) : base(message)
    {
    }

    /// <inheritdoc />
    public ServerNotFoundException(string message, Exception inner) : base(message, inner)
    {
    }
}
