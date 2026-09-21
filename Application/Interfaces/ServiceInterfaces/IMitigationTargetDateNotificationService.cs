using SMS_Domain.Entities;

namespace SMS_Application.Interfaces;

/// <summary>
/// Central service for mitigation target-date notification scanning and queue management.
/// </summary>
public interface IMitigationTargetDateNotificationService
{
    /// <summary>
    /// Scans all mitigations and queues missing target-date alerts as manual integration events.
    /// </summary>
    Task<Result<int>> ScanAllMitigationsAsync(string triggeredBy, CancellationToken cancellationToken = default);

    /// <summary>
    /// Re-evaluates a mitigation after update, cancelling stale pending alerts and creating the current one when needed.
    /// </summary>
    Task<Result> ProcessMitigationUpdateAsync(Mitigation mitigation, string reportId, CancellationToken cancellationToken = default);
}
