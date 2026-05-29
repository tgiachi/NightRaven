using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Scalar.AspNetCore;

namespace NightHeaven.Server.Extensions;

public static class ApiDocsEndpointExtensions
{
    public static IEndpointConventionBuilder MapNightHeavenApiDocs(
        this IEndpointRouteBuilder endpoints,
        string pattern = "/api/docs"
    )
    {
        endpoints.MapOpenApi();

        return endpoints.MapScalarApiReference(
            pattern,
            options =>
            {
                options.Title = "NightHeaven API";
                options.Theme = ScalarTheme.DeepSpace;
            }
        );
    }
}
