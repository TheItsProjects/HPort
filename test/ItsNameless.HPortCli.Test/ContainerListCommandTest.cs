using ItsNameless.HPort.Models;
using ItsNameless.HPort.Services;
using NSubstitute;

namespace ItsNameless.HPortCli.Test;

[TestFixture]
public class ContainerListCommandTest : CliTestBase
{
    private IContainerService _containerService = null!;

    [SetUp]
    public void LocalSetup()
    {
        _containerService = Substitute.For<IContainerService>();
        HPortMock.Container.Returns(_containerService);
    }

    [Test]
    public async Task Run_Help_Works()
    {
        // Act
        var result = await RunCommand("container", "list", "--help");

        // Assert
        Assert.That(result, Is.EqualTo(0));
        Assert.That(Output.ToString(), Does.Contain("Usage:").IgnoreCase.Or.Contain("Nutzung:"));
        Assert.That(Output.ToString(), Contains.Substring("container list"));
    }

    [Test]
    public async Task Run_WhenNoContainersFound_PrintsMessage()
    {
        // Arrange
        var serverName = "testserver";
        _containerService.GetContainers(serverName)
            .Returns(Task.FromResult(new List<PortContainer>()));

        // Act
        var result = await RunCommand("container", "list", "--server", serverName);

        // Assert
        Assert.That(result, Is.EqualTo(0));
        Assert.That(Output.ToString(), Contains.Substring("No containers found."));
    }

    [Test]
    public async Task Run_WhenContainersFound_PrintsTable()
    {
        // Arrange
        var serverName = "testserver";
        var containers = Fakers.PortContainerFaker.Generate(2);
        _containerService.GetContainers(serverName)
            .Returns(Task.FromResult(containers));

        // Act
        var result = await RunCommand("container", "list", "--server", serverName);

        // Assert
        Assert.That(result, Is.EqualTo(0));
        var output = Output.ToString();
        Assert.That(output, Contains.Substring("Container Name"));
        Assert.That(output, Contains.Substring("Server Name"));
        foreach (var container in containers)
        {
            Assert.That(output, Contains.Substring(container.Name));
            Assert.That(output, Contains.Substring(container.Server.Name));
        }
    }

    [Test]
    public async Task Run_WithoutServerName_Works()
    {
        // Arrange
        _containerService.GetContainers(null)
            .Returns(Task.FromResult(new List<PortContainer>()));

        // Act
        var result = await RunCommand("container", "list");

        // Assert
        Assert.That(result, Is.EqualTo(0));
    }
}
