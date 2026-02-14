using ItsNameless.HPort.Models;
using ItsNameless.HPort.Services;
using NSubstitute;

namespace ItsNameless.HPortCli.Test;

[TestFixture]
public class ContainerCreateCommandTest : CliTestBase
{
    private IContainerService _containerService = null!;

    [SetUp]
    public void LocalSetup()
    {
        _containerService = Substitute.For<IContainerService>();
        HPortMock.Container.Returns(_containerService);
    }

    [Test]
    public async Task Run_WithValidArguments_CallsServiceAndPrintsSuccess()
    {
        // Arrange
        var containerName = "test-container";
        var container = Fakers.PortContainerFaker.Generate();
        container.Name = containerName;

        _containerService.CreateContainer(
                containerName,
                PortServerType.Cax11,
                PortDatacenter.Fsn,
                "docker-compose.yml",
                ".env",
                Arg.Any<long?>(),
                Arg.Any<bool>()
            )
            .Returns(Task.FromResult(container));

        // Act
        var result = await RunCommand(
            "container", "create",
            "--name", containerName,
            "--type", "Cax11",
            "--datacenter", "Fsn",
            "--compose", "docker-compose.yml",
            "--env", ".env"
        );

        // Assert
        Assert.That(result, Is.EqualTo(0));
        Assert.That(Output.ToString(), Contains.Substring($"Successfully created container '{containerName}'"));
        await _containerService.Received(1).CreateContainer(
            containerName,
            PortServerType.Cax11,
            PortDatacenter.Fsn,
            "docker-compose.yml",
            ".env",
            null,
            false
        );
    }

    [Test]
    public async Task Run_WithOptionalArguments_PassesThemToService()
    {
        // Arrange
        var containerName = "test-container";
        var container = Fakers.PortContainerFaker.Generate();
        container.Name = containerName;

        _containerService.CreateContainer(
                Arg.Any<string>(),
                Arg.Any<PortServerType>(),
                Arg.Any<PortDatacenter>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<long?>(),
                Arg.Any<bool>()
            )
            .Returns(Task.FromResult(container));

        // Act
        await RunCommand(
            "container", "create",
            "--name", containerName,
            "--type", "Cax11",
            "--datacenter", "Fsn",
            "--compose", "docker-compose.yml",
            "--env", ".env",
            "--ssh-key", "12345",
            "--is-unique"
        );

        // Assert
        await _containerService.Received(1).CreateContainer(
            containerName,
            PortServerType.Cax11,
            PortDatacenter.Fsn,
            "docker-compose.yml",
            ".env",
            12345,
            true
        );
    }
}
