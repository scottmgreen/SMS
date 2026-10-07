namespace SMS_Application.Interfaces;

public sealed class ActiveUserSessionInfo
{
    public string SessionKey { get; set; } = string.Empty;
    public string UserCode { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string UserType { get; set; } = string.Empty;
    public DateTime LoginTimeUtc { get; set; }
    public DateTime LastSeenUtc { get; set; }
}

public interface IActiveUserSessionRegistry
{
    void UpsertSession(ActiveUserSessionInfo sessionInfo);
    void RefreshActivity(string sessionKey);
    void RemoveSession(string sessionKey);
    void RemoveSessionsForUser(string userCode);
    IReadOnlyList<ActiveUserSessionInfo> GetActiveSessions();
}
