using NightHeaven.Core.Utils;
using NightHeaven.Server.Data;

namespace NightHeaven.Server.Extensions;

public static class VersionEndpointExtensions
{
    public static IEndpointConventionBuilder MapNightHeavenVersion(
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
