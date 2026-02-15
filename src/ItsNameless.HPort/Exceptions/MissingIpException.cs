namespace ItsNameless.HPort.Exceptions;

/// <summary>
/// Thrown when a server is missing an expected IP address.
/// </summary>
public class MissingIpException : HPortException
{
    /// <inheritdoc />
    public MissingIpException()
    {
    }

    /// <inheritdoc />
    public MissingIpException(string message) : base(message)
    {
    }

    /// <inheritdoc />
    public MissingIpException(string message, Exception inner) : base(message, inner)
    {
    }
}
