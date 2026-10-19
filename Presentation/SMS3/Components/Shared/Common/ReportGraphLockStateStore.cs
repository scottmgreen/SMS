using System.Collections.Concurrent;
using Microsoft.Extensions.DependencyInjection;

using SMS3.Components.Shared.UIHelpers;
using SMS3.Hubs.Contracts;

namespace SMS3.Components.Shared;

public interface IReportGraphLockStore
{
    IReadOnlyCollection<string> GetLockedKeys();
    void SetLocked(string? lockKey);
    void SetReleased(string? lockKey);
    bool IsLockKeyLocked(string? lockKey);
}

public sealed class ReportGraphLockStore : IReportGraphLockStore
{
    private readonly ConcurrentDictionary<string, byte> _lockedKeys = new(StringComparer.OrdinalIgnoreCase);

    public IReadOnlyCollection<string> GetLockedKeys()
    {
        return _lockedKeys.Keys.ToArray();
    }

    public void SetLocked(string? lockKey)
    {
        if (!LiveUpdateReportGraphLock.TryGetReportCode(lockKey, out _))
        {
            return;
        }

        _lockedKeys[lockKey!] = 0;
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
        if (!LiveUpdateReportGraphLock.TryGetReportCode(lockKey, out _))
        {
            return false;
        }

        return _lockedKeys.ContainsKey(lockKey!);
    }
}

public static class ReportGraphLockStateStore
{
    private static readonly ConcurrentDictionary<string, byte> _fallbackLockedKeys = new(StringComparer.OrdinalIgnoreCase);
    private static readonly object _fallbackSyncLock = new();

    private static IReportGraphLockStore? TryGetStore()
    {
        var store = ServiceLocator.Current?.GetService<IReportGraphLockStore>();
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
                store.SetLocked(lockKey);
                _fallbackLockedKeys.TryRemove(lockKey, out _);
            }
        }

        return store;
    }

    public static IReadOnlyCollection<string> GetLockedKeys()
    {
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

    public static void SetLocked(string? lockKey)
    {
        var store = TryGetStore();
        if (store is not null)
        {
            store.SetLocked(lockKey);
            if (!string.IsNullOrWhiteSpace(lockKey))
            {
                _fallbackLockedKeys.TryRemove(lockKey, out _);
            }
            return;
        }

        if (!LiveUpdateReportGraphLock.TryGetReportCode(lockKey, out _))
        {
            return;
        }

        _fallbackLockedKeys[lockKey!] = 0;
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
        var store = TryGetStore();
        if (store is not null)
        {
            return store.IsLockKeyLocked(lockKey) || (!string.IsNullOrWhiteSpace(lockKey) && _fallbackLockedKeys.ContainsKey(lockKey));
        }

        if (!LiveUpdateReportGraphLock.TryGetReportCode(lockKey, out _))
        {
            return false;
        }

        return _fallbackLockedKeys.ContainsKey(lockKey!);
    }
}
