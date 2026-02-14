using Renci.SshNet;

namespace ItsNameless.HPort.Extensions;

/// <summary>
/// Extensions for <see cref="SshClient"/>.
/// </summary>
internal static class SshClientExtensions
{
    /// <summary>
    /// Asynchronously tries to connect to the SSH server. Retries the connection if it fails, up to a specified number of retries.
    /// </summary>
    /// <param name="client">The <see cref="SshClient"/> to connect to.</param>
    /// <param name="retries">Number of tries before failing.</param>
    /// <param name="timeout">Time in milliseconds to wait between each retry.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <exception cref="InvalidOperationException">Thrown when the connection fails after multiple attempts.</exception>
    internal static async Task TryConnectAsync(
        this ISshClient client,
        int retries = 3,
        int timeout = 5000,
        CancellationToken cancellationToken = default)
    {
        while (client.IsConnected is false)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                await client.ConnectAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                retries--;
                if (retries <= 0)
                {
                    throw new InvalidOperationException(
                        "Failed to connect to SSH server after multiple attempts.",
                        ex
                    );
                }

                await Task.Delay(timeout, cancellationToken);
            }
        }
    }
}
