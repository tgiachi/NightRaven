using Microsoft.Extensions.Hosting;
using NightHeaven.Hosting.Interfaces.Services;
using Serilog;

namespace NightHeaven.Hosting.Internal;

/// <summary>
/// The single <see cref="IHostedService" /> registered with the host. Dispatches
/// <see cref="IHostedService.StartAsync" /> and <see cref="IHostedService.StopAsync" />
/// to every registered <see cref="INightHeavenService" /> in priority order.
/// </summary>
internal sealed class NightHeavenServiceOrchestrator : IHostedService
{
    private readonly ILogger _logger = Log.ForContext<NightHeavenServiceOrchestrator>();
    private readonly NightHeavenServiceDescriptor[] _startOrder;
    private readonly NightHeavenServiceDescriptor[] _stopOrder;

    public NightHeavenServiceOrchestrator(IEnumerable<NightHeavenServiceDescriptor> descriptors)
    {
        // OrderBy is stable, so equal priorities preserve registration order.
        _startOrder = descriptors.OrderBy(d => d.Priority).ToArray();
        _stopOrder = _startOrder.Reverse().ToArray();
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        for (var i = 0; i < _startOrder.Length; i++)
        {
            var descriptor = _startOrder[i];
            _logger.Information(
                "Starting {Service} (priority {Priority})",
                descriptor.Service.GetType().Name,
                descriptor.Priority
            );

            await descriptor.Service.StartAsync(cancellationToken);
        }
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        for (var i = 0; i < _stopOrder.Length; i++)
        {
            var descriptor = _stopOrder[i];
            _logger.Information(
                "Stopping {Service} (priority {Priority})",
                descriptor.Service.GetType().Name,
                descriptor.Priority
            );

            try
            {
                await descriptor.Service.StopAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                // Continue stopping the rest even if one service throws.
                _logger.Error(ex, "Error stopping {Service}", descriptor.Service.GetType().Name);
            }
        }
    }
}
