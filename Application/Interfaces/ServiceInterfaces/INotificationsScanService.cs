using SMS_Domain.Common;
using SMS_Domain.Entities;

namespace SMS_Application.Interfaces;

/// <summary>
/// Unified notification scanning service for mitigation target-date and report status escalation workflows.
/// </summary>
public interface INotificationsScanService
{
    Task<Result<int>> ScanAllMitigationsAsync(string triggeredBy, CancellationToken cancellationToken = default);

    Task<Result<int>> ScanReportsNeedingStatusEscalationAsync(string triggeredBy, CancellationToken cancellationToken = default);

    Task<Result> ProcessMitigationUpdateAsync(Mitigation mitigation, string reportId, CancellationToken cancellationToken = default);
}
