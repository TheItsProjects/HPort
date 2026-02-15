using ItsNameless.HPort.Exceptions;
using ItsNameless.HPort.Extensions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Renci.SshNet;

namespace ItsNameless.HPort.Test.Extensions;

[TestFixture]
[TestOf(typeof(SshClientExtensions))]
public class SshClientExtensionsTest
{
    [Test]
    public async Task ReceivesDefaultRetries_Retries3Times_AndThrows()
    {
        // Arrange
        var sshClient = Substitute.For<ISshClient>();
        sshClient
            .ConnectAsync(default)
            .ThrowsAsyncForAnyArgs(new Exception());

        // Act & Assert
        Assert.ThrowsAsync<SshConnectionException>(
            () => sshClient.TryConnectAsync()
        );

        //Assert
        await sshClient.Received(3).ConnectAsync(default);
    }
}
