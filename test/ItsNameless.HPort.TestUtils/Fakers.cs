using Bogus;
using HetznerCloudApi.Object.Datacenter;
using HetznerCloudApi.Object.Server;
using HetznerCloudApi.Object.ServerType;
using ItsNameless.HPort.Models;

namespace ItsNameless.HPort.Test.Utils;

/// <summary>
/// Utility for creating fake types needed for HPort.
/// </summary>
public class Fakers
{
    /// <summary>
    /// Faker for <see cref="PortServer"/>.
    /// </summary>
    public readonly Faker<PortServer> PortServerFaker;

    /// <summary>
    /// Faker for <see cref="PortContainer"/>.
    /// </summary>
    public readonly Faker<PortContainer> PortContainerFaker;

    public readonly Faker<Server> HetznerServerFaker;

    /// <summary>
    /// Default faker used for generating random data.
    /// </summary>
    public readonly Faker Faker;

    public Fakers()
    {
        PortServerFaker = new Faker<PortServer>();
        PortServerFaker
            .RuleFor(s => s.Name, f => f.Internet.DomainName())
            .RuleFor(s => s.UserPassword, f => f.Random.AlphaNumeric(30))
            .RuleFor(
                s => s.Datacenter,
                f => f.PickRandom(Enum.GetValues<PortDatacenter>())
            )
            .RuleFor(
                s => s.Type,
                f => f.PickRandom(Enum.GetValues<PortServerType>())
            );

        PortContainerFaker = new Faker<PortContainer>();
        PortContainerFaker
            .RuleFor(s => s.Name, f => f.Internet.DomainName())
            .RuleFor(s => s.Server, f => PortServerFaker.Generate());

        HetznerServerFaker = new Faker<Server>();
        HetznerServerFaker
            .RuleFor(s => s.Id, f => f.Random.Long())
            .RuleFor(s => s.Name, f => f.Internet.DomainName())
            .RuleFor(s => s.ServerType, RandomServerType)
            .RuleFor(s => s.Datacenter, RandomDatacenter);

        Faker = new Faker("en_US");
    }

    private static ServerType RandomServerType(Faker f)
        => new()
        {
            Id =
                f.PickRandom(
                    Enum.GetValues<PortServerType>().Select(t => (long)t)
                )
        };

    private static Datacenter RandomDatacenter(Faker f)
        => new()
        {
            Id =
                f.PickRandom(
                    Enum.GetValues<PortDatacenter>().Select(t => (long)t)
                )
        };
}
