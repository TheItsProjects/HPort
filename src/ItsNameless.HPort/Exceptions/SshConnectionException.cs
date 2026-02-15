namespace ItsNameless.HPort.Exceptions;

/// <summary>
/// Thrown when an SSH connection or command execution fails.
/// </summary>
public class SshConnectionException : HPortException
{
    /// <inheritdoc />
    public SshConnectionException()
    {
    }

    /// <inheritdoc />
    public SshConnectionException(string message) : base(message)
    {
    }

    /// <inheritdoc />
    public SshConnectionException(string message, Exception inner) : base(message, inner)
    {
    }
}
