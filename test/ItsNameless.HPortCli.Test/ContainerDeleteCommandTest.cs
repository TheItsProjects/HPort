using ItsNameless.HPort.Services;
using NSubstitute;

namespace ItsNameless.HPortCli.Test;

[TestFixture]
public class ContainerDeleteCommandTest : CliTestBase
{
    private IContainerService _containerService = null!;

    [SetUp]
    public void LocalSetup()
    {
        _containerService = Substitute.For<IContainerService>();
        HPortMock.Container.Returns(_containerService);
    }

    [Test]
    public async Task Run_WithRequiredOptions_Works()
    {
        // Arrange
        var containerName = "test-container";
        var serverName = "test-server";
        var container = Fakers.PortContainerFaker.Generate();
        container.Name = containerName;
        
        _containerService.DeleteContainer(containerName, serverName, true)
            .Returns(Task.FromResult(container));

        // Act
        var result = await RunCommand("container", "delete", "--name", containerName, "--server", serverName);

        // Assert
        if (result != 0)
        {
            Console.WriteLine("Error output: " + Error.ToString());
            Console.WriteLine("Output: " + Output.ToString());
        }
        Assert.That(result, Is.EqualTo(0));
        Assert.That(Output.ToString(), Contains.Substring($"Successfully deleted container '{containerName}'"));
    }

    [Test]
    public async Task Run_WithDeleteServerFalse_PassesToService()
    {
        // Arrange
        var containerName = "test-container";
        var serverName = "test-server";
        var container = Fakers.PortContainerFaker.Generate();
        
        _containerService.DeleteContainer(containerName, serverName, false)
            .Returns(Task.FromResult(container));

        // Act
        await RunCommand(
            "container", "delete", 
            "--name", containerName, 
            "--server", serverName, 
            "--delete-server", "false"
        );

        // Assert
        await _containerService.Received(1).DeleteContainer(containerName, serverName, false);
    }
}
