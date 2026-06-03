using NightRaven.Core.Utils;
using NightRaven.Server.Data;

namespace NightRaven.Server.Extensions.Endpoints;

public static class VersionEndpointExtensions
{
    public static IEndpointConventionBuilder MapNightRavenVersion(
        this IEndpointRouteBuilder endpoints,
        string pattern = "/api/version"
    )
    {
        var assembly = typeof(VersionEndpointExtensions).Assembly;
        var info = new ServerVersionInfo(
            VersionUtils.GetVersion(assembly),
            VersionUtils.GetMetadata(assembly, "Codename")
        );

        return endpoints.MapGet(pattern, () => Results.Json(info))
                        .WithName("GetVersion");
    }
}
