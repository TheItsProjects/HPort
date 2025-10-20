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
        // TODO
        // Arrange


        // Act

        //Assert
    }

    [Test]
    public async Task
        CreateContainerOnExistingServer_ContainerNotRunning_Throws()
    {
        // Arrange


        // Act

        //Assert
    }

    [Test]
    public async Task
        CreateContainerOnExistingServer_ContainerRunning_ReturnsContainer()
    {
        // Arrange


        // Act

        //Assert
    }

    [Test]
    public async Task
        ExecuteCommandInContainer_RunsCorrectCommand_AndReturnsParsedValue()
    {
        // Arrange


        // Act

        //Assert
    }

    [Test]
    public async Task ExecuteCommandInContainer_CommandFailed_Throws()
    {
        // Arrange


        // Act

        //Assert
    }

    [Test]
    public async Task ExecuteCommandInContainer_CommandReturnedNoResult_Throws()
    {
        // Arrange


        // Act

        //Assert
    }

    [Test]
    public async Task ExecuteCommandInContainer_ResultParserFails_Throws()
    {
        // Arrange


        // Act

        //Assert
    }

    [Test]
    public async Task
        GetAllContainers_WithServerName_WhenServerNotFound_Throws()
    {
        // Arrange


        // Act

        //Assert
    }

    [Test]
    public async Task
        GetAllContainers_WithServerName_WhenServerFound_RunsCommands_AndReturnsResults()
    {
        // Arrange


        // Act

        //Assert
    }

    [Test]
    public async Task DeleteContainer_ServerNotFound_Throws()
    {
        // Arrange


        // Act

        //Assert
    }

    [Test]
    public async Task
        DeleteContainer_WithDeleteServerIfEmptyFalse_RunsCommands_AndReturnsContainer()
    {
        // Arrange


        // Act

        //Assert
    }

    [Test]
    public async Task
        DeleteContainer_WithDeleteServer_CallsDeleteServer_AndReturnsContainer()
    {
        // Arrange


        // Act

        //Assert
    }
}
