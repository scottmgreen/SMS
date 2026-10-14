namespace SMS3.Hubs.Contracts;

public static class LiveUpdateEventNames
{
    public const string ReportSubmitted = "ReportSubmitted";
    public const string ReportReturnedToValidation = "ReportReturnedToValidation";
    public const string ReportProcessingValidationLocked = "ReportProcessingValidationLocked";
    public const string ReportProcessingValidationReleased = "ReportProcessingValidationReleased";
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
