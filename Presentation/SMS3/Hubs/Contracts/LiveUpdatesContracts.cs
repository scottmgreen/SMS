namespace SMS3.Hubs.Contracts;

public static class LiveUpdateEventNames
{
    public const string RecordCreated = "RecordCreated";
    public const string ReportReturnedToValidation = "ReportReturnedToValidation";
    public const string RecordEditLocked = "RecordEditLocked";
    public const string RecordEditReleased = "RecordEditReleased";
    public const string RecordProcessingValidationLocked = "RecordProcessingValidationLocked";
    public const string RecordProcessingValidationReleased = "RecordProcessingValidationReleased";
}

public static class LiveUpdateRecordLock
{
    private const string Prefix = "Record:";

    public static string BuildKey(string reportCode)
    {
        return $"{Prefix}{reportCode?.Trim()}";
    }

    public static bool TryGetReportCode(string? lockKey, out string reportCode)
    {
        reportCode = string.Empty;
        if (string.IsNullOrWhiteSpace(lockKey) || !lockKey.StartsWith(Prefix, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        reportCode = lockKey[Prefix.Length..].Trim();
        return !string.IsNullOrWhiteSpace(reportCode);
    }
}

public static class LiveUpdateRecordLockMessage
{
    private const string LockKeyMarker = "LockKey=";
    private const string UserCodeMarker = "UserCode=";
    private const string HeartbeatMarker = "Heartbeat=";

    public static string Build(string lockKey, string action, string? actorUserCode = null, bool isHeartbeat = false)
    {
        var userCodePart = string.IsNullOrWhiteSpace(actorUserCode)
            ? string.Empty
            : $";{UserCodeMarker}{actorUserCode.Trim()}";

        return $"{action};{LockKeyMarker}{lockKey}{userCodePart};{HeartbeatMarker}{isHeartbeat}";
    }

    public static string? TryExtractLockKey(string? message)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            return null;
        }

        var markerIndex = message.IndexOf(LockKeyMarker, StringComparison.OrdinalIgnoreCase);
        if (markerIndex < 0)
        {
            return null;
        }

        var lockKey = message[(markerIndex + LockKeyMarker.Length)..].Trim();
        if (string.IsNullOrWhiteSpace(lockKey))
        {
            return null;
        }

        var delimiterIndex = lockKey.IndexOf(';');
        return delimiterIndex >= 0
            ? lockKey[..delimiterIndex].Trim()
            : lockKey;
    }

    public static string? TryExtractUserCode(string? message)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            return null;
        }

        var markerIndex = message.IndexOf(UserCodeMarker, StringComparison.OrdinalIgnoreCase);
        if (markerIndex < 0)
        {
            return null;
        }

        var value = message[(markerIndex + UserCodeMarker.Length)..].Trim();
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var delimiterIndex = value.IndexOf(';');
        var userCode = delimiterIndex >= 0
            ? value[..delimiterIndex].Trim()
            : value;

        return string.IsNullOrWhiteSpace(userCode) ? null : userCode;
    }
}

public static class LiveUpdateHubConstants
{
    public const string ApplicationUsersGroup = "live-updates:application-users";

    public static string GetUserGroup(string userCode)
    {
        return $"live-updates:user:{userCode}";
    }
}

public sealed class LiveUpdateEnvelope
{
    public string EventName { get; init; } = string.Empty;
    public string SourceModule { get; init; } = string.Empty;
    public DateTime OccurredOn { get; init; } = DateTime.Now;
    public string? ActorUserCode { get; init; }
    public string? Severity { get; init; }
    public string? Message { get; init; }
    public string? EntityType { get; init; }
    public string? EntityCode { get; init; }
}
