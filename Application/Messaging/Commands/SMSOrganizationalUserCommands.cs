//-----------------------------------------------------------------------
// <copyright file="SMSOrganizationalUserCommands.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Command definitions for SMS user management operations including create, update, delete actions.
//                  Defines command objects for write operations in the CQRS pattern.
//                  Commands represent business intentions and trigger state changes.
// </copyright>
//-----------------------------------------------------------------------

namespace SMS_Application.Messaging.Commands;

/// <summary>
/// Commands for SMS Organizational User management operations
/// </summary>

#region Create Commands

/// <summary>
/// Command to create a new SMS Organizational User
/// </summary>
public class CreateSMSOrganizationalUserCommand : BaseCommandBundle, IRequest<Result<SMSOrganizationalUser>>, ICreateCommand
{
    /// <summary>
    /// The SMS Organizational User entity to create
    /// </summary>
    public SMSOrganizationalUser SMSOrganizationalUser { get; set; }

    /// <summary>
    /// Initializes a new instance of the CreateSMSOrganizationalUserCommand class.
    /// </summary>
    /// <param name="smsOrganizationalUser">The SMS organizational user to create</param>
    /// <exception cref="ArgumentNullException">Thrown when smsOrganizationalUser is null</exception>
    public CreateSMSOrganizationalUserCommand(SMSOrganizationalUser smsOrganizationalUser)
    {
        SMSOrganizationalUser = smsOrganizationalUser ?? throw new ArgumentNullException(nameof(smsOrganizationalUser));
    }

    public void SetCreatedBy(string userId, DateTime timestamp)
    {
        SMSOrganizationalUser.CreatedBy = userId;
        SMSOrganizationalUser.CreatedDate = timestamp;
    }

    public void SetUpdatedBy(string userId, DateTime timestamp)
    {
        // For create commands, we typically don't set UpdatedBy
    }
}

#endregion

#region Update Commands

/// <summary>
/// Command to update an existing SMS Organizational User
/// </summary>
public class UpdateSMSOrganizationalUserCommand : BaseCommandBundle, IRequest<Result<SMSOrganizationalUser>>, IUpdateCommand
{
    /// <summary>
    /// The SMS Organizational User entity to update
    /// </summary>
    public SMSOrganizationalUser SMSOrganizationalUser { get; set; }

    /// <summary>
    /// Initializes a new instance of the UpdateSMSOrganizationalUserCommand class.
    /// </summary>
    /// <param name="smsOrganizationalUser">The SMS organizational user to update</param>
    /// <exception cref="ArgumentNullException">Thrown when smsOrganizationalUser is null</exception>
    public UpdateSMSOrganizationalUserCommand(SMSOrganizationalUser smsOrganizationalUser)
    {
        SMSOrganizationalUser = smsOrganizationalUser ?? throw new ArgumentNullException(nameof(smsOrganizationalUser));
    }

    public void SetCreatedBy(string userId, DateTime timestamp)
    {
        // For update commands, we typically don't modify CreatedBy
    }

    public void SetUpdatedBy(string userId, DateTime timestamp)
    {
        SMSOrganizationalUser.UpdatedBy = userId;
        SMSOrganizationalUser.UpdatedDate = timestamp;
    }
}

/// <summary>
/// Command to update SMS Organizational User password
/// </summary>
public class UpdateSMSOrganizationalUserPasswordCommand : BaseCommandBundle, IRequest<Result<bool>>, IHasAuditFields
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
    /// Initializes a new instance of the UpdateSMSOrganizationalUserPasswordCommand class.
    /// </summary>
    /// <param name="userId">The user ID</param>
    /// <param name="newPassword">The new password</param>
    /// <exception cref="ArgumentException">Thrown when userId or newPassword is null or empty</exception>
    public UpdateSMSOrganizationalUserPasswordCommand(string userId, string newPassword)
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
/// Command to authenticate SMS Organizational User
/// </summary>
public class AuthenticateSMSOrganizationalUserCommand : BaseCommandBundle, IRequest<Result<bool>>
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
    /// Initializes a new instance of the AuthenticateSMSOrganizationalUserCommand class.
    /// </summary>
    /// <param name="userName">The username</param>
    /// <param name="password">The password</param>
    /// <exception cref="ArgumentException">Thrown when userName or password is null or empty</exception>
    public AuthenticateSMSOrganizationalUserCommand(string userName, string password)
    {
        if (string.IsNullOrWhiteSpace(userName)) throw new ArgumentException("Username cannot be null or empty", nameof(userName));
        if (string.IsNullOrWhiteSpace(password)) throw new ArgumentException("Password cannot be null or empty", nameof(password));

        UserName = userName;
        Password = password;
    }
}

/// <summary>
/// Command to record SMS Organizational User login
/// </summary>
public class RecordSMSOrganizationalUserLoginCommand : BaseCommandBundle, IRequest<Result<bool>>, IHasAuditFields
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
    /// Initializes a new instance of the RecordSMSOrganizationalUserLoginCommand class.
    /// </summary>
    /// <param name="userId">The user ID</param>
    /// <param name="loginDate">The login date</param>
    /// <exception cref="ArgumentException">Thrown when userId is null or empty</exception>
    public RecordSMSOrganizationalUserLoginCommand(string userId, DateTime loginDate)
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
/// Command to delete an SMS Organizational User by ID
/// </summary>
public class DeleteSMSOrganizationalUserCommand : BaseCommandBundle, IRequest<Result<bool>>
{
    /// <summary>
    /// The ID of the SMS Organizational User to delete
    /// </summary>
    public SMSOrganizationalUserID SMSOrganizationalUserId { get; set; }

    /// <summary>
    /// Initializes a new instance of the DeleteSMSOrganizationalUserCommand class.
    /// </summary>
    /// <param name="smsOrganizationalUserId">The ID of the SMS organizational user to delete</param>
    /// <exception cref="ArgumentNullException">Thrown when smsOrganizationalUserId is null</exception>
    public DeleteSMSOrganizationalUserCommand(SMSOrganizationalUserID smsOrganizationalUserId)
    {
        SMSOrganizationalUserId = smsOrganizationalUserId ?? throw new ArgumentNullException(nameof(smsOrganizationalUserId));
    }
}

#endregion
