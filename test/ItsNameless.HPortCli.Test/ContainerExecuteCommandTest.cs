using ItsNameless.HPort.Services;
using NSubstitute;

namespace ItsNameless.HPortCli.Test;

[TestFixture]
public class ContainerExecuteCommandTest : CliTestBase
{
    private IContainerService _containerService = null!;

    [SetUp]
    public void LocalSetup()
    {
        _containerService = Substitute.For<IContainerService>();
        HPortMock.Container.Returns(_containerService);
    }

    [Test]
    public async Task Run_WithValidArguments_CallsServiceAndPrintsResult()
    {
        // Arrange
        var containerName = "test-container";
        var serverName = "test-server";
        var serviceName = "web";
        var command = "ls -la";
        var expectedResult = "total 0";

        _containerService.ExecuteCommandInContainer(
                containerName,
                serverName,
                serviceName,
                command,
                Arg.Any<CancellationToken>()
            )
            .Returns(Task.FromResult(expectedResult));

        // Act
        var result = await RunCommand(
            "container", "execute",
            "--name", containerName,
            "--server", serverName,
            "--service", serviceName,
            "--command", command
        );

        // Assert
        Assert.That(result, Is.EqualTo(0));
        Assert.That(Output.ToString().Trim(), Is.EqualTo(expectedResult));
        await _containerService.Received(1).ExecuteCommandInContainer(
            containerName,
            serverName,
            serviceName,
            command,
            Arg.Any<CancellationToken>()
        );
    }
}
