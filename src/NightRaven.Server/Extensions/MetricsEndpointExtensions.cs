using NightRaven.Hosting.Interfaces.Metrics;
using NightRaven.Server.Services.Metrics;

namespace NightRaven.Server.Extensions;

public static class MetricsEndpointExtensions
{
    public static IEndpointConventionBuilder MapNightRavenMetrics(
        this IEndpointRouteBuilder endpoints,
        string pattern = "/metrics"
    )
        => endpoints.MapGet(
                        pattern,
                        (IMetricsService metrics) => Results.Text(
                            OpenMetricsFormatter.Format(metrics.GetSnapshot()),
                            "text/plain; charset=utf-8"
                        )
                    )
                    .WithName("GetMetrics");
}
