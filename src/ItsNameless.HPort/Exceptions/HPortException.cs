namespace ItsNameless.HPort.Exceptions;

/// <summary>
/// Base exception for all HPort related errors.
/// </summary>
public class HPortException : Exception
{
    /// <inheritdoc />
    public HPortException()
    {
    }

    /// <inheritdoc />
    public HPortException(string message) : base(message)
    {
    }

    /// <inheritdoc />
    public HPortException(string message, Exception inner) : base(message, inner)
    {
    }
}
