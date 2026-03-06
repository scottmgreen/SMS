//-----------------------------------------------------------------------
// <copyright file="SMSApplicationUserCommands.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Command definitions for SMS user management operations including create, update, delete actions.
//                  Defines command objects for write operations in the CQRS pattern.
//                  Commands represent business intentions and trigger state changes.
// </copyright>
//-----------------------------------------------------------------------

namespace SMS_Application.Messaging.Commands;

/// <summary>
/// Commands for SMS Application User management operations
/// </summary>

#region Create Commands

/// <summary>
/// Command to create a new SMS Application User
/// </summary>
public class CreateSMSApplicationUserCommand : BaseCommandBundle, IRequest<Result<SMSApplicationUser>>, ICreateCommand
{
    /// <summary>
    /// The SMS Application User entity to create
    /// </summary>
    public SMSApplicationUser SMSApplicationUser { get; set; }

    /// <summary>
    /// Initializes a new instance of the CreateSMSApplicationUserCommand class.
    /// </summary>
    /// <param name="smsApplicationUser">The SMS application user to create</param>
    /// <exception cref="ArgumentNullException">Thrown when smsApplicationUser is null</exception>
    public CreateSMSApplicationUserCommand(SMSApplicationUser smsApplicationUser)
    {
        SMSApplicationUser = smsApplicationUser ?? throw new ArgumentNullException(nameof(smsApplicationUser));
    }

    public void SetCreatedBy(string userId, DateTime timestamp)
    {
        SMSApplicationUser.CreatedBy = userId;
        SMSApplicationUser.CreatedDate = timestamp;
    }

    public void SetUpdatedBy(string userId, DateTime timestamp)
    {
        // For create commands, we typically don't set UpdatedBy
    }
}

#endregion

#region Update Commands

/// <summary>
/// Command to update an existing SMS Application User
/// </summary>
public class UpdateSMSApplicationUserCommand : BaseCommandBundle, IRequest<Result<SMSApplicationUser>>, IUpdateCommand
{
    /// <summary>
    /// The SMS Application User entity to update
    /// </summary>
    public SMSApplicationUser SMSApplicationUser { get; set; }

    /// <summary>
    /// Initializes a new instance of the UpdateSMSApplicationUserCommand class.
    /// </summary>
    /// <param name="smsApplicationUser">The SMS application user to update</param>
    /// <exception cref="ArgumentNullException">Thrown when smsApplicationUser is null</exception>
    public UpdateSMSApplicationUserCommand(SMSApplicationUser smsApplicationUser)
    {
        SMSApplicationUser = smsApplicationUser ?? throw new ArgumentNullException(nameof(smsApplicationUser));
    }

    public void SetCreatedBy(string userId, DateTime timestamp)
    {
        // For update commands, we typically don't modify CreatedBy
    }

    public void SetUpdatedBy(string userId, DateTime timestamp)
    {
        SMSApplicationUser.UpdatedBy = userId;
        SMSApplicationUser.UpdatedDate = timestamp;
    }
}

/// <summary>
/// Command to update SMS Application User password
/// </summary>
public class UpdateSMSApplicationUserPasswordCommand : BaseCommandBundle, IRequest<Result<bool>>, IUpdateCommand
{
    /// <summary>
    /// The user ID
    /// </summary>
    public string UserId { get; set; }

    /// <summary>
    /// The new password
    /// </summary>
    public string NewPassword { get; set; }

    /// <summary>
    /// User who is updating the password
    /// </summary>
    public string UpdatedBy { get; set; }

    /// <summary>
    /// Initializes a new instance of the UpdateSMSApplicationUserPasswordCommand class.
    /// </summary>
    /// <param name="userId">The user ID</param>
    /// <param name="newPassword">The new password</param>
    /// <exception cref="ArgumentException">Thrown when userId or newPassword is null or empty</exception>
    public UpdateSMSApplicationUserPasswordCommand(string userId, string newPassword)
    {
        if (string.IsNullOrWhiteSpace(userId)) throw new ArgumentException("User ID cannot be null or empty", nameof(userId));
        if (string.IsNullOrWhiteSpace(newPassword)) throw new ArgumentException("New password cannot be null or empty", nameof(newPassword));

        UserId = userId;
        NewPassword = newPassword;
    }

    public void SetCreatedBy(string userId, DateTime timestamp)
    {
        // This is an update operation, not create
    }

    public void SetUpdatedBy(string userId, DateTime timestamp)
    {
        UpdatedBy = userId;
    }
}

#endregion

#region Authentication Commands

/// <summary>
/// Command to authenticate SMS Application User
/// </summary>
public class AuthenticateSMSApplicationUserCommand : BaseCommandBundle, IRequest<Result<bool>>
{
    /// <summary>
    /// The username
    /// </summary>
    public string UserName { get; set; }

    /// <summary>
    /// The password
    /// </summary>
    public string Password { get; set; }

    /// <summary>
    /// Initializes a new instance of the AuthenticateSMSApplicationUserCommand class.
    /// </summary>
    /// <param name="userName">The username</param>
    /// <param name="password">The password</param>
    /// <exception cref="ArgumentException">Thrown when userName or password is null or empty</exception>
    public AuthenticateSMSApplicationUserCommand(string userName, string password)
    {
        if (string.IsNullOrWhiteSpace(userName)) throw new ArgumentException("Username cannot be null or empty", nameof(userName));
        if (string.IsNullOrWhiteSpace(password)) throw new ArgumentException("Password cannot be null or empty", nameof(password));

        UserName = userName;
        Password = password;
    }
}

/// <summary>
/// Command to record SMS Application User login
/// </summary>
public class RecordSMSApplicationUserLoginCommand : BaseCommandBundle, IRequest<Result<bool>>, IUpdateCommand
{
    /// <summary>
    /// The user ID
    /// </summary>
    public string UserId { get; set; }

    /// <summary>
    /// The login date
    /// </summary>
    public DateTime LoginDate { get; set; }

    /// <summary>
    /// User who updated the login record
    /// </summary>
    public string UpdatedBy { get; set; }

    /// <summary>
    /// Initializes a new instance of the RecordSMSApplicationUserLoginCommand class.
    /// </summary>
    /// <param name="userId">The user ID</param>
    /// <param name="loginDate">The login date</param>
    /// <exception cref="ArgumentException">Thrown when userId is null or empty</exception>
    public RecordSMSApplicationUserLoginCommand(string userId, DateTime loginDate)
    {
        if (string.IsNullOrWhiteSpace(userId)) throw new ArgumentException("User ID cannot be null or empty", nameof(userId));

        UserId = userId;
        LoginDate = loginDate;
    }

    public void SetCreatedBy(string userId, DateTime timestamp)
    {
        // This is an update operation, not create
    }

    public void SetUpdatedBy(string userId, DateTime timestamp)
    {
        UpdatedBy = userId;
    }
}

#endregion

#region Delete Commands

/// <summary>
/// Command to delete an SMS Application User by ID
/// </summary>
public class DeleteSMSApplicationUserCommand : BaseCommandBundle, IRequest<Result<bool>>, IDeleteCommand
{
    /// <summary>
    /// The ID of the SMS Application User to delete
    /// </summary>
    public SMSApplicationUserID SMSApplicationUserId { get; set; }

    /// <summary>
    /// User who performed the deletion
    /// </summary>
    public string DeletedBy { get; set; } = string.Empty;

    /// <summary>
    /// Initializes a new instance of the DeleteSMSApplicationUserCommand class.
    /// </summary>
    /// <param name="smsApplicationUserId">The ID of the SMS application user to delete</param>
    /// <exception cref="ArgumentNullException">Thrown when smsApplicationUserId is null</exception>
    public DeleteSMSApplicationUserCommand(SMSApplicationUserID smsApplicationUserId)
    {
        SMSApplicationUserId = smsApplicationUserId ?? throw new ArgumentNullException(nameof(smsApplicationUserId));
    }

    public void SetDeletedBy(string userId, DateTime timestamp)
    {
        DeletedBy = userId;
    }
}

#endregion

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
public class RecordLogoutCommand : BaseCommandBundle, IRequest<Result<bool>>, ICreateCommand
{
    public string UserName { get; set; }
    public SMSUserType UserType { get; set; }
    public string SessionId { get; set; }
    public DateTime LogoutTime { get; set; }
    public string LogoutType { get; set; } // Manual, Timeout, System
    public TimeSpan SessionDuration { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; }

    public RecordLogoutCommand(
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
