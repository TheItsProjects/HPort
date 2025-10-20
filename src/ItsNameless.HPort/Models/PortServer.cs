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
    /// Creates a new instance of <see cref="PortServer"/> from a <see cref="Server"/> object.
    /// </summary>
    /// <param name="server">The <see cref="Server"/> to create a new <see cref="PortServer"/> from.</param>
    /// <param name="userPassword">The password used for the user.</param>
    /// <returns></returns>
    public static PortServer Create(Server server, string userPassword)
    {
        return new PortServer
        {
            Name = server.Name,
            Type = (PortServerType)server.ServerType.Id,
            Datacenter = (PortDatacenter)server.Datacenter.Id,
            UserPassword = userPassword,
        };
    }
}

public enum PortServerType
{
    Cpx11 = 22,
    Cpx21 = 23,
    Cpx31 = 24,
    Cpx41 = 25,
    Cpx51 = 26,
    Cax11 = 45,
    Cax21 = 93,
    Cax31 = 94,
    Cax41 = 95,
    Ccx13 = 96,
    Ccx23 = 97,
    Ccx33 = 98,
    Ccx43 = 99,
    Ccx53 = 100,
    Ccx63 = 101,
    Cx22 = 104,
    Cx32 = 105,
    Cx42 = 106,
    Cx52 = 107,
}

// /// <summary>
// /// Represents a server type on Hetzner.
// /// </summary>
// public record PortServerTypee
// {
//     /// <summary>
//     /// The name of the server type.
//     /// </summary>
//     public string Name { get; }
//
//     /// <summary>
//     /// The ID of the server type.
//     /// </summary>
//     public int Id { get; }
//
//     private PortServerType(string name, int id)
//     {
//         Name = name;
//         Id = id;
//     }
//
//     public static readonly PortServerType Cpx11 = new("cpx11", 22);
//     public static readonly PortServerType Cpx21 = new("cpx21", 23);
//     public static readonly PortServerType Cpx31 = new("cpx31", 24);
//     public static readonly PortServerType Cpx41 = new("cpx41", 25);
//     public static readonly PortServerType Cpx51 = new("cpx51", 26);
//     public static readonly PortServerType Cax11 = new("cax11", 45);
//     public static readonly PortServerType Cax21 = new("cax21", 93);
//     public static readonly PortServerType Cax31 = new("cax31", 94);
//     public static readonly PortServerType Cax41 = new("cax41", 95);
//     public static readonly PortServerType Ccx13 = new("ccx13", 96);
//     public static readonly PortServerType Ccx23 = new("ccx23", 97);
//     public static readonly PortServerType Ccx33 = new("ccx33", 98);
//     public static readonly PortServerType Ccx43 = new("ccx43", 99);
//     public static readonly PortServerType Ccx53 = new("ccx53", 100);
//     public static readonly PortServerType Ccx63 = new("ccx63", 101);
//     public static readonly PortServerType Cx22 = new("cx22", 104);
//     public static readonly PortServerType Cx32 = new("cx32", 105);
//     public static readonly PortServerType Cx42 = new("cx42", 106);
//     public static readonly PortServerType Cx52 = new("cx52", 107);
//
//     public static explicit operator PortServerType(ServerType serverType)
//     {
//         return serverType.Id switch
//         {
//             22 => Cpx11,
//             23 => Cpx21,
//             24 => Cpx31,
//             25 => Cpx41,
//             26 => Cpx51,
//             45 => Cax11,
//             93 => Cax21,
//             94 => Cax31,
//             95 => Cax41,
//             96 => Ccx13,
//             97 => Ccx23,
//             98 => Ccx33,
//             99 => Ccx43,
//             100 => Ccx53,
//             101 => Ccx63,
//             104 => Cx22,
//             105 => Cx32,
//             106 => Cx42,
//             107 => Cx52,
//             _ => throw new ArgumentOutOfRangeException(
//                 nameof(serverType),
//                 $"Unknown server type ID: {serverType.Id}"
//             )
//         };
//     }
//
//     public static explicit operator PortServerType(string serverTypeName)
//     {
//         return serverTypeName.ToLower() switch
//         {
//             "cpx11" => Cpx11,
//             "cpx21" => Cpx21,
//             "cpx31" => Cpx31,
//             "cpx41" => Cpx41,
//             "cpx51" => Cpx51,
//             "cax11" => Cax11,
//             "cax21" => Cax21,
//             "cax31" => Cax31,
//             "cax41" => Cax41,
//             "ccx13" => Ccx13,
//             "ccx23" => Ccx23,
//             "ccx33" => Ccx33,
//             "ccx43" => Ccx43,
//             "ccx53" => Ccx53,
//             "ccx63" => Ccx63,
//             "cx22" => Cx22,
//             "cx32" => Cx32,
//             "cx42" => Cx42,
//             "cx52" => Cx52,
//             _ => throw new ArgumentOutOfRangeException(
//                 nameof(serverTypeName),
//                 $"Unknown server type name: {serverTypeName}"
//             )
//         };
//     }
// }

public enum PortDatacenter
{
    Nbg1 = 2,
    Hel1 = 3,
    Fsn1 = 4,
    Ash = 5,
    Hil = 6,
    Sin = 7,
}

// /// <summary>
// /// Represents a datacenter on Hetzner.
// /// </summary>
// public record PortDatacenterE
// {
//     /// <summary>
//     /// The name of the datacenter.
//     /// </summary>
//     public string Name { get; }
//
//     /// <summary>
//     /// The ID of the datacenter.
//     /// </summary>
//     public int Id { get; }
//
//     private PortDatacenter(string name, int id)
//     {
//         Name = name;
//         Id = id;
//     }
//
//     public static readonly PortDatacenter Nbg1 = new("nbg1-dc3", 2);
//     public static readonly PortDatacenter Hel1 = new("hel1-dc2", 3);
//     public static readonly PortDatacenter Fsn1 = new("fsn1-dc14", 4);
//     public static readonly PortDatacenter Ash = new("ash-dc1", 5);
//     public static readonly PortDatacenter Hil = new("hil-dc1", 6);
//     public static readonly PortDatacenter Sin = new("sin-dc1", 7);
//
//     public static explicit operator PortDatacenter(Datacenter datacenter)
//     {
//         return datacenter.Id switch
//         {
//             2 => Nbg1,
//             3 => Hel1,
//             4 => Fsn1,
//             5 => Ash,
//             6 => Hil,
//             7 => Sin,
//             _ => throw new ArgumentOutOfRangeException(
//                 nameof(datacenter),
//                 $"Unknown datacenter ID: {datacenter.Id}"
//             )
//         };
//     }
// }
