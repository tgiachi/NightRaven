using NightHeaven.Hosting.Interfaces.Metrics;
using NightHeaven.Server.Services.Metrics;

namespace NightHeaven.Server.Extensions;

public static class MetricsEndpointExtensions
{
    public static IEndpointConventionBuilder MapNightHeavenMetrics(
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
