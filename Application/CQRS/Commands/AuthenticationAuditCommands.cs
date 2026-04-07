
namespace SMS_Application.Messaging.Commands;
#region Authentication Audit Commands

/// <summary>
/// Command to record successful user authentication events for audit trails
/// </summary>
public class RecordAuthenticationSuccessCommand : BaseCommandBundle, IRequest<Result<bool>>, ICreateCommand
{
    public string UserName { get; set; }
    public SMSUserType UserType { get; set; }
    public string UserDisplayName { get; set; }
    public string IPAddress { get; set; }
    public string UserAgent { get; set; }
    public DateTime AuthenticationTime { get; set; }
    public string SessionId { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; }

    public RecordAuthenticationSuccessCommand(
        string userName,
        SMSUserType userType,
        string userDisplayName,
        string ipAddress,
        string userAgent,
        string sessionId)
    {
        UserName = userName ?? throw new ArgumentNullException(nameof(userName));
        UserType = userType;
        UserDisplayName = userDisplayName ?? throw new ArgumentNullException(nameof(userDisplayName));
        IPAddress = ipAddress ?? "Unknown";
        UserAgent = userAgent ?? "Unknown";
        SessionId = sessionId ?? throw new ArgumentNullException(nameof(sessionId));
        AuthenticationTime = DateTime.UtcNow;
    }

    public void SetCreatedBy(string userId, DateTime timestamp)
    {
        CreatedBy = userId;
        CreatedDate = timestamp;
    }

    public void SetUpdatedBy(string userId, DateTime timestamp)
    {
        // Not applicable for create commands
    }
}

/// <summary>
/// Command to record failed authentication attempts for security audit
/// </summary>
public class RecordAuthenticationFailureCommand : BaseCommandBundle, IRequest<Result<bool>>, ICreateCommand
{
    public string UserName { get; set; }
    public string FailureReason { get; set; }
    public string IPAddress { get; set; }
    public string UserAgent { get; set; }
    public DateTime AttemptTime { get; set; }
    public int AttemptCount { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; }

    public RecordAuthenticationFailureCommand(
        string userName,
        string failureReason,
        string ipAddress,
        string userAgent,
        int attemptCount = 1)
    {
        UserName = userName ?? throw new ArgumentNullException(nameof(userName));
        FailureReason = failureReason ?? throw new ArgumentNullException(nameof(failureReason));
        IPAddress = ipAddress ?? "Unknown";
        UserAgent = userAgent ?? "Unknown";
        AttemptTime = DateTime.UtcNow;
        AttemptCount = attemptCount;
    }

    public void SetCreatedBy(string userId, DateTime timestamp)
    {
        CreatedBy = userId;
        CreatedDate = timestamp;
    }

    public void SetUpdatedBy(string userId, DateTime timestamp)
    {
        // Not applicable for create commands
    }
}

/// <summary>
/// Command to record user logout events for session tracking
/// </summary>
public class RecordAuthenticationLogoutCommand : BaseCommandBundle, IRequest<Result<bool>>, ICreateCommand
{
    public string UserName { get; set; }
    public SMSUserType UserType { get; set; }
    public string SessionId { get; set; }
    public DateTime LogoutTime { get; set; }
    public string LogoutType { get; set; } // Manual, Timeout, System
    public TimeSpan SessionDuration { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; }

    public RecordAuthenticationLogoutCommand(
        string userName,
        SMSUserType userType,
        string sessionId,
        string logoutType,
        TimeSpan sessionDuration)
    {
        UserName = userName ?? throw new ArgumentNullException(nameof(userName));
        UserType = userType;
        SessionId = sessionId ?? throw new ArgumentNullException(nameof(sessionId));
        LogoutType = logoutType ?? "Manual";
        SessionDuration = sessionDuration;
        LogoutTime = DateTime.UtcNow;
    }

    public void SetCreatedBy(string userId, DateTime timestamp)
    {
        CreatedBy = userId;
        CreatedDate = timestamp;
    }

    public void SetUpdatedBy(string userId, DateTime timestamp)
    {
        // Not applicable for create commands
    }
}

#endregion