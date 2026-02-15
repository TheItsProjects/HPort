namespace ItsNameless.HPort.Exceptions;

/// <summary>
/// Thrown when a container operation fails.
/// </summary>
public class ContainerOperationException : HPortException
{
    /// <inheritdoc />
    public ContainerOperationException()
    {
    }

    /// <inheritdoc />
    public ContainerOperationException(string message) : base(message)
    {
    }

    /// <inheritdoc />
    public ContainerOperationException(string message, Exception inner) : base(message, inner)
    {
    }
}
