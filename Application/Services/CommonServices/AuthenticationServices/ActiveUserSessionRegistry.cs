using System.Collections.Concurrent;
using SMS_Application.Interfaces;

namespace SMS_Application.Services;

public sealed class ActiveUserSessionRegistry : IActiveUserSessionRegistry
{
    private static readonly TimeSpan SessionStaleWindow = TimeSpan.FromHours(12);
    private readonly ConcurrentDictionary<string, ActiveUserSessionInfo> _sessions = new(StringComparer.OrdinalIgnoreCase);

    public void UpsertSession(ActiveUserSessionInfo sessionInfo)
    {
        if (sessionInfo is null || string.IsNullOrWhiteSpace(sessionInfo.SessionKey))
        {
            return;
        }

        CleanupStaleSessions();
        var nowUtc = DateTime.UtcNow;

        _sessions.AddOrUpdate(
            sessionInfo.SessionKey,
            _ =>
            {
                sessionInfo.LoginTimeUtc = sessionInfo.LoginTimeUtc == default ? nowUtc : sessionInfo.LoginTimeUtc;
                sessionInfo.LastSeenUtc = nowUtc;
                return sessionInfo;
            },
            (_, existing) =>
            {
                existing.UserCode = sessionInfo.UserCode;
                existing.DisplayName = sessionInfo.DisplayName;
                existing.UserType = sessionInfo.UserType;
                if (existing.LoginTimeUtc == default)
                {
                    existing.LoginTimeUtc = nowUtc;
                }
                existing.LastSeenUtc = nowUtc;
                return existing;
            });
    }

    public void RefreshActivity(string sessionKey)
    {
        if (string.IsNullOrWhiteSpace(sessionKey))
        {
            return;
        }

        CleanupStaleSessions();

        if (_sessions.TryGetValue(sessionKey, out var existing))
        {
            existing.LastSeenUtc = DateTime.UtcNow;
        }
    }

    public void RemoveSession(string sessionKey)
    {
        if (string.IsNullOrWhiteSpace(sessionKey))
        {
            return;
        }

        _sessions.TryRemove(sessionKey, out _);
    }

    public void RemoveSessionsForUser(string userCode)
    {
        if (string.IsNullOrWhiteSpace(userCode))
        {
            return;
        }

        var keysForUser = _sessions
            .Where(x => string.Equals(x.Value.UserCode, userCode, StringComparison.OrdinalIgnoreCase))
            .Select(x => x.Key)
            .ToList();

        foreach (var key in keysForUser)
        {
            _sessions.TryRemove(key, out _);
        }
    }

    public IReadOnlyList<ActiveUserSessionInfo> GetActiveSessions()
    {
        CleanupStaleSessions();

        return _sessions.Values
            .GroupBy(
                s => string.IsNullOrWhiteSpace(s.UserCode)
                    ? $"display:{s.DisplayName}"
                    : $"user:{s.UserCode}",
                StringComparer.OrdinalIgnoreCase)
            .Select(g => g
                .OrderByDescending(x => x.LastSeenUtc)
                .First())
            .OrderBy(s => s.DisplayName)
            .ThenBy(s => s.UserCode)
            .ToList();
    }

    private void CleanupStaleSessions()
    {
        var staleBeforeUtc = DateTime.UtcNow.Subtract(SessionStaleWindow);

        var staleKeys = _sessions
            .Where(x => x.Value.LastSeenUtc < staleBeforeUtc)
            .Select(x => x.Key)
            .ToList();

        foreach (var staleKey in staleKeys)
        {
            _sessions.TryRemove(staleKey, out _);
        }
    }
}
