using Scalar.AspNetCore;

namespace NightRaven.Server.Extensions;

public static class ApiDocsEndpointExtensions
{
    public static IEndpointConventionBuilder MapNightRavenApiDocs(
        this IEndpointRouteBuilder endpoints,
        string pattern = "/api/docs"
    )
    {
        endpoints.MapOpenApi();

        return endpoints.MapScalarApiReference(
            pattern,
            options =>
            {
                options.Title = "NightRaven API";
                options.Theme = ScalarTheme.DeepSpace;
            }
        );
    }
}
