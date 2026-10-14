namespace SMS3.Hubs.Contracts;

public static class LiveUpdateEventNames
{
    public const string RecordCreated = "RecordCreated";
    public const string ReportReturnedToValidation = "ReportReturnedToValidation";
    public const string RecordEditLocked = "RecordEditLocked";
    public const string RecordEditReleased = "RecordEditReleased";
    public const string ReportProcessingValidationLocked = "ReportProcessingValidationLocked";
    public const string ReportProcessingValidationReleased = "ReportProcessingValidationReleased";
}

public static class LiveUpdateRecordLockMessage
{
    private const string LockKeyMarker = "LockKey=";

    public static string Build(string lockKey, string action)
    {
        return $"{action};{LockKeyMarker}{lockKey}";
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
