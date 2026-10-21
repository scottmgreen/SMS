using System.Collections.Concurrent;
using Microsoft.Extensions.DependencyInjection;
using Radzen;

using SMS3.Components.Shared.UIHelpers;
using SMS3.Hubs.Contracts;

namespace SMS3.Components.Shared;

public interface IRecordLockStore
{
    IReadOnlyCollection<string> GetLockedKeys();
    IReadOnlyCollection<RecordLockInfo> GetActiveLocks();
    void SetLocked(string? lockKey, string? userCode = null, DateTime? heartbeatUtc = null);
    void SetReleased(string? lockKey);
    bool IsLockKeyLocked(string? lockKey);
}

public sealed class RecordLockInfo
{
    public string LockKey { get; init; } = string.Empty;
    public string ReportCode { get; init; } = string.Empty;
    public string LockedByUserCode { get; set; } = string.Empty;
    public DateTime LastHeartbeatUtc { get; set; }
}

public sealed class RecordLockStore : IRecordLockStore
{
    private readonly ConcurrentDictionary<string, RecordLockInfo> _lockedKeys = new(StringComparer.OrdinalIgnoreCase);

    public IReadOnlyCollection<string> RemoveStaleLocks(DateTime staleBeforeUtc)
    {
        var staleKeys = _lockedKeys
            .Where(x => x.Value.LastHeartbeatUtc < staleBeforeUtc)
            .Select(x => x.Key)
            .ToList();

        foreach (var staleKey in staleKeys)
        {
            _lockedKeys.TryRemove(staleKey, out _);
        }

        return staleKeys;
    }

    public IReadOnlyCollection<string> GetLockedKeys()
    {
        return _lockedKeys.Keys.ToArray();
    }

    public IReadOnlyCollection<RecordLockInfo> GetActiveLocks()
    {
        return _lockedKeys.Values.ToArray();
    }

    public void SetLocked(string? lockKey, string? userCode = null, DateTime? heartbeatUtc = null)
    {
        if (string.IsNullOrWhiteSpace(lockKey))
        {
            return;
        }

        var effectiveLockKey = lockKey!;
        var reportCode = LiveUpdateRecordLock.TryGetReportCode(effectiveLockKey, out var parsedReportCode)
            ? parsedReportCode
            : effectiveLockKey;
        var effectiveHeartbeatUtc = heartbeatUtc?.ToUniversalTime() ?? DateTime.UtcNow;

        _lockedKeys.AddOrUpdate(
            effectiveLockKey,
            _ => new RecordLockInfo
            {
                LockKey = effectiveLockKey,
                ReportCode = reportCode,
                LockedByUserCode = userCode ?? string.Empty,
                LastHeartbeatUtc = effectiveHeartbeatUtc
            },
            (_, existing) =>
            {
                existing.LastHeartbeatUtc = effectiveHeartbeatUtc;
                if (!string.IsNullOrWhiteSpace(userCode))
                {
                    existing.LockedByUserCode = userCode;
                }

                return existing;
            });

    }

    public void SetReleased(string? lockKey)
    {
        if (string.IsNullOrWhiteSpace(lockKey))
        {
            return;
        }

        _lockedKeys.TryRemove(lockKey, out _);
    }

    public bool IsLockKeyLocked(string? lockKey)
    {
        if (string.IsNullOrWhiteSpace(lockKey))
        {
            return false;
        }

        return _lockedKeys.ContainsKey(lockKey!);
    }
}

public static class RecordLockStateStore
{
    public static TimeSpan LockStaleThreshold { get; } = TimeSpan.FromSeconds(60);
    private static readonly ConcurrentDictionary<string, RecordLockInfo> _fallbackLockedKeys = new(StringComparer.OrdinalIgnoreCase);
    private static readonly object _fallbackSyncLock = new();

    private static IRecordLockStore? TryGetStore()
    {
        var store = ServiceLocator.Current?.GetService<IRecordLockStore>();
        if (store is null || _fallbackLockedKeys.IsEmpty)
        {
            return store;
        }

        lock (_fallbackSyncLock)
        {
            if (_fallbackLockedKeys.IsEmpty)
            {
                return store;
            }

            foreach (var lockKey in _fallbackLockedKeys.Keys)
            {
                if (_fallbackLockedKeys.TryGetValue(lockKey, out var lockInfo))
                {
                    store.SetLocked(lockKey, lockInfo.LockedByUserCode, lockInfo.LastHeartbeatUtc);
                }
                _fallbackLockedKeys.TryRemove(lockKey, out _);
            }
        }

        return store;
    }

    public static IReadOnlyCollection<string> GetLockedKeys()
    {
        CleanupStaleLocks();

        var store = TryGetStore();
        if (store is null)
        {
            return _fallbackLockedKeys.Keys.ToArray();
        }

        if (_fallbackLockedKeys.IsEmpty)
        {
            return store.GetLockedKeys();
        }

        return store.GetLockedKeys()
            .Concat(_fallbackLockedKeys.Keys)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    public static IReadOnlyCollection<RecordLockInfo> GetActiveLocks()
    {
        CleanupStaleLocks();

        var store = TryGetStore();
        if (store is null)
        {
            return _fallbackLockedKeys.Values.ToArray();
        }

        if (_fallbackLockedKeys.IsEmpty)
        {
            return store.GetActiveLocks();
        }

        return store.GetActiveLocks()
            .Concat(_fallbackLockedKeys.Values)
            .GroupBy(x => x.LockKey, StringComparer.OrdinalIgnoreCase)
            .Select(g => g.OrderByDescending(x => x.LastHeartbeatUtc).First())
            .ToArray();
    }

    public static void SetLocked(string? lockKey, string? userCode = null, DateTime? heartbeatUtc = null)
    {
        var store = TryGetStore();
        if (store is not null)
        {
            store.SetLocked(lockKey, userCode, heartbeatUtc);
            if (!string.IsNullOrWhiteSpace(lockKey))
            {
                _fallbackLockedKeys.TryRemove(lockKey, out _);
            }
            return;
        }

        if (string.IsNullOrWhiteSpace(lockKey))
        {
            return;
        }

        var effectiveLockKey = lockKey!;
        _fallbackLockedKeys.AddOrUpdate(
            effectiveLockKey,
            _ => new RecordLockInfo
            {
                LockKey = effectiveLockKey,
                ReportCode = LiveUpdateRecordLock.TryGetReportCode(effectiveLockKey, out var reportCode) ? reportCode : effectiveLockKey,
                LockedByUserCode = userCode ?? string.Empty,
                LastHeartbeatUtc = heartbeatUtc?.ToUniversalTime() ?? DateTime.UtcNow
            },
            (_, existing) =>
            {
                existing.LastHeartbeatUtc = heartbeatUtc?.ToUniversalTime() ?? DateTime.UtcNow;
                if (!string.IsNullOrWhiteSpace(userCode))
                {
                    existing.LockedByUserCode = userCode;
                }

                return existing;
            });
    }

    public static void SetReleased(string? lockKey)
    {
        var store = TryGetStore();
        if (store is not null)
        {
            store.SetReleased(lockKey);
            if (!string.IsNullOrWhiteSpace(lockKey))
            {
                _fallbackLockedKeys.TryRemove(lockKey, out _);
            }
            return;
        }

        if (string.IsNullOrWhiteSpace(lockKey))
        {
            return;
        }

        _fallbackLockedKeys.TryRemove(lockKey, out _);
    }

    public static bool IsLockKeyLocked(string? lockKey)
    {
        CleanupStaleLocks();

        var store = TryGetStore();
        if (store is not null)
        {
            return store.IsLockKeyLocked(lockKey) || (!string.IsNullOrWhiteSpace(lockKey) && _fallbackLockedKeys.ContainsKey(lockKey));
        }

        if (string.IsNullOrWhiteSpace(lockKey))
        {
            return false;
        }

        return _fallbackLockedKeys.ContainsKey(lockKey!);
    }

    private static void CleanupStaleLocks()
    {
        var staleBeforeUtc = DateTime.UtcNow.Subtract(LockStaleThreshold);

        var fallbackStaleKeys = _fallbackLockedKeys
            .Where(x => x.Value.LastHeartbeatUtc < staleBeforeUtc)
            .Select(x => x.Key)
            .ToList();

        foreach (var staleKey in fallbackStaleKeys)
        {
            _fallbackLockedKeys.TryRemove(staleKey, out _);
            PublishStaleTimeoutRelease(staleKey);
        }

        var store = ServiceLocator.Current?.GetService<IRecordLockStore>();
        if (store is not RecordLockStore concreteStore)
        {
            return;
        }

        var removedStoreKeys = concreteStore.RemoveStaleLocks(staleBeforeUtc);
        foreach (var staleKey in removedStoreKeys)
        {
            PublishStaleTimeoutRelease(staleKey);
        }
    }

    private static void PublishStaleTimeoutRelease(string? lockKey)
    {
        if (string.IsNullOrWhiteSpace(lockKey))
        {
            return;
        }

        _ = Task.Run(async () =>
        {
            try
            {
                await EventBusDispatcher.DispatchNotificationAsync(
                    NotificationSeverity.Info,
                    LiveUpdateEventNames.RecordEditReleased,
                    LiveUpdateRecordLockMessage.Build(lockKey!, "StaleTimeout"),
                    3000);
            }
            catch
            {
            }
        });
    }
}
