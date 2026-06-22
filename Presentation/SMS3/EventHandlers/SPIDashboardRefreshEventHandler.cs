using Microsoft.Extensions.Logging;
using SMS_Application.Interfaces;
using SMS_Domain.Common;
using SMS_Domain.Events;
using SMS3.Components.Shared;

namespace SMS3.EventHandlers;

/// <summary>
/// UI event handler for SPI dashboard refresh signals.
/// Dispatches refresh events to registered Blazor components.
/// </summary>
public sealed class SPIDashboardRefreshEventHandler : BaseUIEventHandler<SPIDashboardRefreshEvent>
{
    private readonly ILogger<SPIDashboardRefreshEventHandler> _logger;

    public SPIDashboardRefreshEventHandler(ILogger<SPIDashboardRefreshEventHandler> logger)
        : base(logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    protected override async Task<Result> ProcessUIEventAsync(SPIDashboardRefreshEvent uiEvent, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation(
                "SPIDashboardRefreshEventHandler: Dispatching refresh event for section {Section} (All={RefreshAll}, SPIs={SPICount})",
                uiEvent.DashboardSection,
                uiEvent.RefreshEntireDashboard,
                uiEvent.AffectedSPICodes?.Count ?? 0);

            await EventBusDispatcher.DispatchSPIDashboardRefreshAsync(uiEvent);

            _logger.LogInformation("SPIDashboardRefreshEventHandler: Refresh event dispatched successfully");
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "SPIDashboardRefreshEventHandler: Failed to dispatch dashboard refresh event");
            return Result.Failure(new Error("SPI_DASHBOARD_REFRESH_FAILED", $"Failed to dispatch SPI dashboard refresh: {ex.Message}"));
        }
    }
}
