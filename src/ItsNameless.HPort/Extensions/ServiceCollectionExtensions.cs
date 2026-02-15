using Microsoft.Extensions.DependencyInjection;

namespace ItsNameless.HPort.Extensions;

/// <summary>
/// Extensions for adding HPort services to the service collection.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds the HPort service to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="hetznerToken">The API token used to access the Hetzner API.</param>
    /// <param name="serverStatesFilePath">
    /// The path to a JSON file storing information about managed servers, e.g. their passwords.
    /// </param>
    /// <returns>The service collection.</returns>
    public static async Task<IServiceCollection> AddHPort(
        this IServiceCollection services,
        string hetznerToken,
        string serverStatesFilePath)
    {
        var hport =
            await HPort.WithDefaultsAsync(hetznerToken, serverStatesFilePath);

        services.AddSingleton<IHPort>(hport);

        return services;
    }
}
