namespace ItsNameless.HPort.Models;

/// <summary>
/// Represents a server used in HPort.
/// </summary>
/// <remarks>A partial implementation of <see cref="Server"/>.</remarks>
public record PortServer
{
    /// <summary>
    /// The name of the server.
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// The type of the server.
    /// </summary>
    public required PortServerType Type { get; set; }

    /// <summary>
    /// The datacenter where the server is located.
    /// </summary>
    public required PortDatacenter Datacenter { get; set; }

    /// <summary>
    /// The ssh user used to connect to the server.
    /// </summary>
    public static string User => "deploy";

    /// <summary>
    /// The ssh password for the <see cref="User"/>.
    /// </summary>
    public required string UserPassword { get; set; }

    /// <summary>
    /// The IP addresses associated with the server.
    /// </summary>
    public List<PortServerIp> Ips { get; set; } = [];

    /// <summary>
    /// Creates a new instance of <see cref="PortServer"/> from a <see cref="Server"/> object.
    /// </summary>
    /// <param name="server">The <see cref="Server"/> to create a new <see cref="PortServer"/> from.</param>
    /// <param name="userPassword">The password used for the user.</param>
    /// <returns></returns>
    public static PortServer Create(Server server, string userPassword)
    {
        var ips = new List<PortServerIp>();

        if (server.PublicNet?.Ipv4 != null)
        {
            ips.Add(
                new PortServerIp
                {
                    Address = server.PublicNet.Ipv4.Ip,
                    IsPublic = true
                }
            );
        }

        if (server.PrivateNet != null)
        {
            ips.AddRange(
                server.PrivateNet.Select(
                    pn => new PortServerIp
                    {
                        Address = pn.Ip,
                        IsPublic = false,
                        NetworkId = pn.Network
                    }
                )
            );
        }

        return new PortServer
        {
            Name = server.Name,
            Type = (PortServerType)server.ServerType.Id,
            Datacenter = (PortDatacenter)server.Datacenter.Id,
            UserPassword = userPassword,
            Ips = ips
        };
    }
}

/// <summary>
/// Represents an IP address associated with a <see cref="PortServer"/>.
/// </summary>
public record PortServerIp
{
    /// <summary>
    /// The IP address.
    /// </summary>
    public required string Address { get; set; }

    /// <summary>
    /// Whether the IP is a public address.
    /// </summary>
    public required bool IsPublic { get; set; }

    /// <summary>
    /// The ID of the internal network this IP is attached to, if applicable.
    /// </summary>
    public long? NetworkId { get; set; }
}

public enum PortServerType
{
    // Shared Cost-Efficient CPU
    Cx23 = 114,
    Cx33 = 115,
    Cx43 = 116,
    Cx53 = 117,
    Cax11 = 45,
    Cax21 = 93,
    Cax31 = 94,
    Cax41 = 95,
    // Shared Regular CPU
    Cpx12 = 108,
    Cpx22 = 109,
    Cpx32 = 110,
    Cpx42 = 111,
    Cpx52 = 112,
    Cpx62 = 113,
    // Dedicated CPU
    Ccx13 = 96,
    Ccx23 = 97,
    Ccx33 = 98,
    Ccx43 = 99,
    Ccx53 = 100,
    Ccx63 = 101,
}

public enum PortDatacenter
{
    Nbg = 2,
    Hel = 3,
    Fsn = 4,
    Ash = 5,
    Hil = 6,
    Sin = 7,
}