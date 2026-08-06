using SMS_Domain.Entities;
using SMS_Domain.Enums;

namespace SMS_Domain.Services;

public static class MitigationStatusAggregationService
{
    private static readonly Dictionary<string, int> StatusPrecedence = new(StringComparer.OrdinalIgnoreCase)
    {
        [MitigationStatus.PendingApproval.Value] = 0,
        [MitigationStatus.Rejected.Value] = 1,
        [MitigationStatus.Approved.Value] = 2,
        [MitigationStatus.InProgress.Value] = 3,
        [MitigationStatus.PastExpectedTargetDate.Value] = 4,
        [MitigationStatus.Complete.Value] = 5,
        [MitigationStatus.MonitoringHazard.Value] = 6
    };

    public static MitigationStatus? ResolveOverallStatus(IEnumerable<Mitigation> mitigations)
    {
        if (mitigations is null)
        {
            return null;
        }

        return ResolveOverallStatus(mitigations.Select(m => m?.Status));
    }

    public static MitigationStatus? ResolveOverallStatus(IEnumerable<MitigationStatus?> statuses)
    {
        if (statuses is null)
        {
            return null;
        }

        var normalizedStatuses = statuses
            .Where(s => s is not null)
            .Select(s => s!)
            .ToList();

        if (normalizedStatuses.Count == 0)
        {
            return null;
        }

        var hasMonitoringHazard = normalizedStatuses.Any(s => s == MitigationStatus.MonitoringHazard);
        if (hasMonitoringHazard)
        {
            var allAreCompleteOrMonitoring = normalizedStatuses.All(s =>
                s == MitigationStatus.Complete ||
                s == MitigationStatus.MonitoringHazard);

            if (allAreCompleteOrMonitoring)
            {
                return MitigationStatus.MonitoringHazard;
            }
        }

        return normalizedStatuses
            .OrderBy(GetPrecedence)
            .FirstOrDefault();
    }

    public static Mitigation? SelectGoverningMitigation(IEnumerable<Mitigation> mitigations)
    {
        if (mitigations is null)
        {
            return null;
        }

        return mitigations
            .Where(m => m is not null)
            .OrderBy(m => GetPrecedence(m.Status))
            .ThenBy(m => m.TargetDate ?? DateTime.MaxValue)
            .ThenBy(m => m.Code)
            .FirstOrDefault();
    }

    private static int GetPrecedence(MitigationStatus? status)
    {
        if (status is null)
        {
            return int.MaxValue;
        }

        return StatusPrecedence.TryGetValue(status.Value, out var precedence)
            ? precedence
            : int.MaxValue - 1;
    }
}
