namespace ItsNameless.HPort.Exceptions;

/// <summary>
/// Thrown when an error occurs during server creation.
/// </summary>
public class ServerCreationException : HPortException
{
    /// <inheritdoc />
    public ServerCreationException()
    {
    }

    /// <inheritdoc />
    public ServerCreationException(string message) : base(message)
    {
    }

    /// <inheritdoc />
    public ServerCreationException(string message, Exception inner) : base(message, inner)
    {
    }
}
