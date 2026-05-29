using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using NightHeaven.Hosting.Interfaces.Metrics;
using NightHeaven.Server.Services.Metrics;

namespace NightHeaven.Server.Extensions;

public static class MetricsEndpointExtensions
{
    public static IEndpointConventionBuilder MapNightHeavenMetrics(
        this IEndpointRouteBuilder endpoints,
        string pattern = "/metrics"
    )
    {
        return endpoints.MapGet(
                pattern,
                (IMetricsService metrics) => Results.Text(
                    OpenMetricsFormatter.Format(metrics.GetSnapshot()),
                    "text/plain; charset=utf-8"
                )
            )
            .WithName("GetMetrics");
    }
}
