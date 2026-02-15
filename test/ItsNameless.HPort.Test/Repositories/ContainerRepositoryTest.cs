using ItsNameless.HPort.Exceptions;
using ItsNameless.HPort.Models;
using ItsNameless.HPort.Repositories;
using ItsNameless.HPort.Test.Utils;
using ItsNameless.HPort.Test.Utils.Extensions;
using NSubstitute;

namespace ItsNameless.HPort.Test.Repositories;

[TestFixture]
[TestOf(typeof(ContainerRepository))]
public class ContainerRepositoryTest
{
    private Fakers _fakers;
    private IServerRepository _serverRepository;
    private ContainerRepository _sut;

    [SetUp]
    public async Task Setup()
    {
        _fakers = new Fakers();
        _serverRepository = Substitute.For<IServerRepository>();

        _sut = new ContainerRepository(_serverRepository);
    }

    [Test]
    public async Task
        CreateContainerOnNewServer_ReceivesOptions_CallsRepository_AndPassesOptions()
    {
        // Arrange
        var expectedServer = _fakers.PortServerFaker.Generate();
        var expectedContainer =
            _fakers.PortContainerFaker.Generate() with
            {
                Server = expectedServer
            };
        var expectedSshKeyId = _fakers.Faker.Random.Long();
        var expectedComposeContent = _fakers.Faker.Lorem.Paragraph();
        var expectedEnvContent = _fakers.Faker.Lorem.Paragraph();
        string actualServerName = null!;
        PortServerType actualServerType = default;
        PortDatacenter actualDatacenter = default;
        long? actualSshKeyId = null;
        string actualContainerName = null!;
        string actualComposeContent = null!;
        string actualEnvContent = null!;

        _serverRepository.CreateServer(
                Arg.Do<string>(s => actualServerName = s),
                Arg.Do<PortServerType>(t => actualServerType = t),
                Arg.Do<PortDatacenter>(dc => actualDatacenter = dc),
                Arg.Do<long?>(l => actualSshKeyId = l),
                Arg.Do<string>(s => actualContainerName = s),
                Arg.Do<string>(s => actualComposeContent = s),
                Arg.Do<string>(s => actualEnvContent = s),
                Arg.Any<long?>(),
                Arg.Any<CancellationToken>()
            )
            .ReturnsAsyncForAnyArgs(expectedServer);

        // Act
        var result =
            await _sut.CreateContainerOnNewServer(
                expectedContainer.Name,
                expectedServer.Type,
                expectedServer.Datacenter,
                expectedSshKeyId,
                expectedServer.Name,
                expectedComposeContent,
                expectedEnvContent
            );

        //Assert
        await _serverRepository.Received(1)
            .CreateServer(
                Arg.Any<string>(),
                Arg.Any<PortServerType>(),
                Arg.Any<PortDatacenter>(),
                Arg.Any<long?>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<long?>(),
                Arg.Any<CancellationToken>()
            );

        Assert.That(result, Is.EqualTo(expectedContainer));
        Assert.That(actualServerName, Is.EqualTo(expectedServer.Name));
        Assert.That(actualServerType, Is.EqualTo(expectedServer.Type));
        Assert.That(actualDatacenter, Is.EqualTo(expectedServer.Datacenter));
        Assert.That(actualSshKeyId, Is.EqualTo(expectedSshKeyId));
        Assert.That(actualContainerName, Is.EqualTo(expectedContainer.Name));
        Assert.That(actualComposeContent, Is.EqualTo(expectedComposeContent));
        Assert.That(actualEnvContent, Is.EqualTo(expectedEnvContent));
    }

    [Test]
    public async Task CreateContainerOnExistingServer_RunsCorrectCommands()
    {
        // Arrange
        var existingServer = _fakers.PortServerFaker.Generate();
        var containerName = _fakers.Faker.Internet.DomainWord();
        var composeContent = _fakers.Faker.Lorem.Paragraph();
        var envContent = _fakers.Faker.Lorem.Paragraph();

        _serverRepository.CheckContainerIsRunning(
                existingServer.Name,
                containerName,
                cancellationToken: Arg.Any<CancellationToken>()
            )
            .Returns(true);

        _serverRepository.ExecuteCommandsOnServer(
                existingServer.Name,
                Arg.Any<IEnumerable<string>>(),
                Arg.Any<CancellationToken>()
            )
            .Returns(ToAsyncEnumerable(Enumerable.Empty<string>()));

        // Act
        await _sut.CreateContainerOnExistingServer(
            existingServer,
            containerName,
            composeContent,
            envContent
        );

        //Assert
        _serverRepository.Received(1)
            .ExecuteCommandsOnServer(
                existingServer.Name,
                Arg.Is<IEnumerable<string>>(cmds =>
                    cmds.Count() == 5 &&
                    cmds.Any(c => c.Contains("mkdir")) &&
                    cmds.Any(c => c.Contains("docker-compose.yml")) &&
                    cmds.Any(c => c.Contains(".env")) &&
                    cmds.Any(c => c.Contains("docker compose")) &&
                    cmds.Any(c => c.Contains("touch"))
                ),
                Arg.Any<CancellationToken>()
            );
    }

    [Test]
    public async Task
        CreateContainerOnExistingServer_ContainerNotRunning_Throws()
    {
        // Arrange
        var existingServer = _fakers.PortServerFaker.Generate();
        var containerName = _fakers.Faker.Internet.DomainWord();

        _serverRepository.CheckContainerIsRunning(
                existingServer.Name,
                containerName,
                cancellationToken: Arg.Any<CancellationToken>()
            )
            .Returns(false);

        _serverRepository.ExecuteCommandsOnServer(
                Arg.Any<string>(),
                Arg.Any<IEnumerable<string>>(),
                Arg.Any<CancellationToken>()
            )
            .Returns(ToAsyncEnumerable(Enumerable.Empty<string>()));

        // Act
        AsyncTestDelegate act =
            async () =>
                await _sut.CreateContainerOnExistingServer(
                    existingServer,
                    containerName,
                    "compose",
                    "env"
                );

        //Assert
        Assert.ThrowsAsync<ContainerOperationException>(act);
    }

    [Test]
    public async Task
        CreateContainerOnExistingServer_ContainerRunning_ReturnsContainer()
    {
        // Arrange
        var existingServer = _fakers.PortServerFaker.Generate();
        var containerName = _fakers.Faker.Internet.DomainWord();

        _serverRepository.CheckContainerIsRunning(
                existingServer.Name,
                containerName,
                cancellationToken: Arg.Any<CancellationToken>()
            )
            .Returns(true);

        _serverRepository.ExecuteCommandsOnServer(
                Arg.Any<string>(),
                Arg.Any<IEnumerable<string>>(),
                Arg.Any<CancellationToken>()
            )
            .Returns(ToAsyncEnumerable(Enumerable.Empty<string>()));

        // Act
        var result =
            await _sut.CreateContainerOnExistingServer(
                existingServer,
                containerName,
                "compose",
                "env"
            );

        //Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Name, Is.EqualTo(containerName));
        Assert.That(result.Server, Is.EqualTo(existingServer));
    }

    [Test]
    public async Task
        ExecuteCommandInContainer_RunsCorrectCommand_AndReturnsParsedValue()
    {
        // Arrange
        var containerName = "test-container";
        var serverName = "test-server";
        var serviceName = "web";
        var command = "ls";
        var expectedResult = "file1.txt";
        var parsedResult = 1;

        _serverRepository.ExecuteCommandsOnServer(
                serverName,
                Arg.Any<IEnumerable<string>>(),
                Arg.Any<CancellationToken>()
            )
            .Returns(ToAsyncEnumerable(new[] { expectedResult }));

        // Act
        var result =
            await _sut.ExecuteCommandInContainer(
                containerName,
                serverName,
                serviceName,
                command,
                s =>
                {
                    Assert.That(s, Is.EqualTo(expectedResult));
                    return parsedResult;
                }
            );

        //Assert
        Assert.That(result, Is.EqualTo(parsedResult));
        _serverRepository.Received(1)
            .ExecuteCommandsOnServer(
                serverName,
                Arg.Is<IEnumerable<string>>(cmds =>
                    cmds.Single().Contains($"exec {serviceName} {command}")
                ),
                Arg.Any<CancellationToken>()
            );
    }

    [Test]
    public async Task ExecuteCommandInContainer_CommandFailed_Throws()
    {
        // Arrange
        _serverRepository.ExecuteCommandsOnServer(
                Arg.Any<string>(),
                Arg.Any<IEnumerable<string>>(),
                Arg.Any<CancellationToken>()
            )
            .Returns(x => throw new InvalidOperationException("Command failed")
            );

        // Act
        AsyncTestDelegate act =
            async () =>
                await _sut.ExecuteCommandInContainer(
                    "c",
                    "s",
                    "svc",
                    "cmd",
                    s => s
                );

        //Assert
        Assert.ThrowsAsync<ContainerOperationException>(act);
    }

    [Test]
    public async Task ExecuteCommandInContainer_CommandReturnedNoResult_Throws()
    {
        // Arrange
        _serverRepository.ExecuteCommandsOnServer(
                Arg.Any<string>(),
                Arg.Any<IEnumerable<string>>(),
                Arg.Any<CancellationToken>()
            )
            .Returns(ToAsyncEnumerable(Enumerable.Empty<string>()));

        // Act
        AsyncTestDelegate act =
            async () =>
                await _sut.ExecuteCommandInContainer<string>(
                    "c",
                    "s",
                    "svc",
                    "cmd",
                    s => s
                );

        //Assert
        var ex = Assert.ThrowsAsync<ContainerOperationException>(act);
        Assert.That(
            ex.Message,
            Does.Contain(
                "There was an error when trying to execute a command on the server"
            )
        );
    }

    [Test]
    public async Task ExecuteCommandInContainer_ResultParserFails_Throws()
    {
        // Arrange
        _serverRepository.ExecuteCommandsOnServer(
                Arg.Any<string>(),
                Arg.Any<IEnumerable<string>>(),
                Arg.Any<CancellationToken>()
            )
            .Returns(ToAsyncEnumerable(new[] { "result" }));

        // Act
        AsyncTestDelegate act =
            async () =>
                await _sut.ExecuteCommandInContainer<string>(
                    "c",
                    "s",
                    "svc",
                    "cmd",
                    s => throw new Exception("Parser failed")
                );

        //Assert
        Assert.ThrowsAsync<ContainerOperationException>(act);
    }

    [Test]
    public async Task
        GetAllContainers_WithServerName_WhenServerNotFound_Throws()
    {
        // Arrange
        _serverRepository.GetServer(
                Arg.Any<string>(),
                Arg.Any<CancellationToken>()
            )
            .Returns((PortServer?)null);

        // Act
        AsyncTestDelegate act =
            async () =>
                await _sut.GetAllContainers("unknown-server");

        //Assert
        Assert.ThrowsAsync<ServerNotFoundException>(act);
    }

    [Test]
    public async Task
        GetAllContainers_WithServerName_WhenServerFound_RunsCommands_AndReturnsResults()
    {
        // Arrange
        var server = _fakers.PortServerFaker.Generate();
        var containerNames = new[] { "c1", "c2" };
        var commandResult = string.Join("\n", containerNames);

        _serverRepository.GetServer(
                server.Name,
                Arg.Any<CancellationToken>()
            )
            .Returns(server);

        _serverRepository.ExecuteCommandsOnServer(
                server.Name,
                Arg.Any<IEnumerable<string>>(),
                Arg.Any<CancellationToken>()
            )
            .Returns(ToAsyncEnumerable(new[] { commandResult }));

        // Act
        var result = await _sut.GetAllContainers(server.Name);

        //Assert
        Assert.That(result, Has.Count.EqualTo(2));
        Assert.That(
            result.Select(c => c.Name),
            Is.EquivalentTo(containerNames)
        );
        Assert.That(result.All(c => c.Server == server), Is.True);
    }

    [Test]
    public async Task DeleteContainer_ServerNotFound_Throws()
    {
        // Arrange
        _serverRepository.GetServer(
                Arg.Any<string>(),
                Arg.Any<CancellationToken>()
            )
            .Returns((PortServer?)null);

        // Act
        AsyncTestDelegate act =
            async () =>
                await _sut.DeleteContainer("c", "unknown-server");

        //Assert
        Assert.ThrowsAsync<ServerNotFoundException>(act);
    }

    [Test]
    public async Task
        DeleteContainer_WithDeleteServerIfEmptyFalse_RunsCommands_AndReturnsContainer()
    {
        // Arrange
        var server = _fakers.PortServerFaker.Generate();
        var containerName = "c1";

        _serverRepository.GetServer(
                server.Name,
                Arg.Any<CancellationToken>()
            )
            .Returns(server);

        _serverRepository.ExecuteCommandsOnServer(
                server.Name,
                Arg.Any<IEnumerable<string>>(),
                Arg.Any<CancellationToken>()
            )
            .Returns(ToAsyncEnumerable(Enumerable.Empty<string>()));

        // Act
        var result =
            await _sut.DeleteContainer(containerName, server.Name, false);

        //Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Name, Is.EqualTo(containerName));
        Assert.That(result.Server, Is.EqualTo(server));

        await _serverRepository.DidNotReceiveWithAnyArgs()
            .DeleteServer(default!, default);

        _serverRepository.Received(1)
            .ExecuteCommandsOnServer(
                server.Name,
                Arg.Is<IEnumerable<string>>(cmds =>
                    cmds.Count() == 2 &&
                    cmds.Any(c => c.Contains("down")) &&
                    cmds.Any(c => c.Contains("rm -rf"))
                ),
                Arg.Any<CancellationToken>()
            );
    }

    [Test]
    public async Task
        DeleteContainer_WithDeleteServer_CallsDeleteServer_AndReturnsContainer()
    {
        // Arrange
        var server = _fakers.PortServerFaker.Generate();
        var containerName = "c1";

        _serverRepository.GetServer(
                server.Name,
                Arg.Any<CancellationToken>()
            )
            .Returns(server);

        _serverRepository.ExecuteCommandsOnServer(
                server.Name,
                Arg.Any<IEnumerable<string>>(),
                Arg.Any<CancellationToken>()
            )
            .Returns(ToAsyncEnumerable(Enumerable.Empty<string>()));

        _serverRepository.ExecuteCommandsOnServer(
                server.Name,
                Arg.Is<IEnumerable<string>>(cmds =>
                    cmds.Any(c => c.Contains("ls -d"))
                ),
                Arg.Any<CancellationToken>()
            )
            .Returns(ToAsyncEnumerable(new[] { "" }));

        // Act
        var result =
            await _sut.DeleteContainer(containerName, server.Name, true);

        //Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Name, Is.EqualTo(containerName));
        Assert.That(result.Server, Is.EqualTo(server));

        await _serverRepository.Received(1)
            .DeleteServer(server.Name, Arg.Any<CancellationToken>());
    }

    private static async IAsyncEnumerable<T> ToAsyncEnumerable<T>(
        IEnumerable<T> items)
    {
        foreach (var item in items)
        {
            yield return item;
            await Task.CompletedTask;
        }
    }
}
