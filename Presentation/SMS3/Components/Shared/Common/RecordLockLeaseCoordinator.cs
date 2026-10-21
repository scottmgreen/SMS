using System.Collections.Concurrent;

namespace SMS3.Components.Shared;

public interface IRecordLockLeaseCoordinator
{
    Task OnLockAcquiredAsync(
        string ownerToken,
        string lockKey,
        string? userCode,
        Func<string, bool, string, Task> publishAsync,
        string action);

    Task OnLockReleasedAsync(
        string ownerToken,
        string lockKey,
        string? userCode,
        Func<string, bool, string, Task> publishAsync,
        string action);

    IReadOnlyCollection<string> GetOwnedLockKeys();

    Task ReleaseOwnedLocksAsync(
        string? userCode,
        Func<string, bool, string, Task> publishAsync,
        string action);

    void StopHeartbeat(string ownerToken);
}

public sealed class RecordLockLeaseCoordinator : IRecordLockLeaseCoordinator
{
    private sealed class HeartbeatLease
    {
        public PeriodicTimer Timer { get; init; } = default!;
        public CancellationTokenSource CancellationTokenSource { get; init; } = default!;
    }

    private readonly ConcurrentDictionary<string, HeartbeatLease> _leases = new(StringComparer.OrdinalIgnoreCase);
    private readonly ConcurrentDictionary<string, string> _ownedLockByOwnerToken = new(StringComparer.OrdinalIgnoreCase);

    public async Task OnLockAcquiredAsync(
        string ownerToken,
        string lockKey,
        string? userCode,
        Func<string, bool, string, Task> publishAsync,
        string action)
    {
        if (string.IsNullOrWhiteSpace(lockKey))
        {
            return;
        }

        _ownedLockByOwnerToken[ownerToken] = lockKey;
        RecordLockStateStore.SetLocked(lockKey, userCode, DateTime.UtcNow);
        StartHeartbeat(ownerToken, lockKey, userCode, publishAsync);
        await publishAsync(lockKey, true, action);
    }

    public async Task OnLockReleasedAsync(
        string ownerToken,
        string lockKey,
        string? userCode,
        Func<string, bool, string, Task> publishAsync,
        string action)
    {
        if (string.IsNullOrWhiteSpace(lockKey))
        {
            return;
        }

        StopHeartbeat(ownerToken);
        _ownedLockByOwnerToken.TryRemove(ownerToken, out _);
        RecordLockStateStore.SetReleased(lockKey);
        await publishAsync(lockKey, false, action);
    }

    public IReadOnlyCollection<string> GetOwnedLockKeys()
    {
        return _ownedLockByOwnerToken.Values
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    public async Task ReleaseOwnedLocksAsync(
        string? userCode,
        Func<string, bool, string, Task> publishAsync,
        string action)
    {
        var ownershipSnapshot = _ownedLockByOwnerToken.ToArray();
        if (ownershipSnapshot.Length == 0)
        {
            return;
        }

        foreach (var ownerToken in ownershipSnapshot.Select(x => x.Key).Distinct(StringComparer.OrdinalIgnoreCase))
        {
            StopHeartbeat(ownerToken);
            _ownedLockByOwnerToken.TryRemove(ownerToken, out _);
        }

        var lockKeys = ownershipSnapshot
            .Select(x => x.Value)
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        foreach (var lockKey in lockKeys)
        {
            RecordLockStateStore.SetReleased(lockKey);
            await publishAsync(lockKey, false, action);
        }
    }

    public void StopHeartbeat(string ownerToken)
    {
        if (string.IsNullOrWhiteSpace(ownerToken))
        {
            return;
        }

        if (!_leases.TryRemove(ownerToken, out var lease))
        {
            return;
        }

        try
        {
            lease.CancellationTokenSource.Cancel();
        }
        catch
        {
        }

        lease.Timer.Dispose();
        lease.CancellationTokenSource.Dispose();
    }

    private void StartHeartbeat(
        string ownerToken,
        string lockKey,
        string? userCode,
        Func<string, bool, string, Task> publishAsync)
    {
        StopHeartbeat(ownerToken);

        if (string.IsNullOrWhiteSpace(ownerToken))
        {
            return;
        }

        var lease = new HeartbeatLease
        {
            CancellationTokenSource = new CancellationTokenSource(),
            Timer = new PeriodicTimer(TimeSpan.FromSeconds(15))
        };

        _leases[ownerToken] = lease;

        _ = Task.Run(async () =>
        {
            try
            {
                while (await lease.Timer.WaitForNextTickAsync().ConfigureAwait(false))
                {
                    if (lease.CancellationTokenSource.IsCancellationRequested)
                    {
                        break;
                    }

                    RecordLockStateStore.SetLocked(lockKey, userCode, DateTime.UtcNow);
                    await publishAsync(lockKey, true, "Heartbeat").ConfigureAwait(false);
                }
            }
            catch (OperationCanceledException)
            {
            }
            catch (ObjectDisposedException)
            {
            }
        });
    }
}
