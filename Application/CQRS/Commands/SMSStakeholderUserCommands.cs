//-----------------------------------------------------------------------
// <copyright file="SMSStakeholderUserCommands.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Command definitions for SMS user management operations including create, update, delete actions.
//                  Defines command objects for write operations in the CQRS pattern.
//                  Commands represent business intentions and trigger state changes.
// </copyright>
//-----------------------------------------------------------------------

using SMS_Domain.Entities;

namespace SMS_Application.Messaging.Commands;

/// <summary>
/// Commands for SMS Stakeholder User management operations
/// </summary>

#region Create Commands

/// <summary>
/// Command to create a new SMS Stakeholder User
/// </summary>
public class CreateSMSStakeholderUserCommand : BaseCommandBundle, IRequest<Result<SMSStakeholderUser>>, ICreateCommand
{
    /// <summary>
    /// The SMS Stakeholder User entity to create
    /// </summary>
    public SMSStakeholderUser SMSStakeholderUser { get; set; }

    /// <summary>
    /// Initializes a new instance of the CreateSMSStakeholderUserCommand class.
    /// </summary>
    /// <param name="smsStakeholderUser">The SMS stakeholder user to create</param>
    /// <exception cref="ArgumentNullException">Thrown when smsStakeholderUser is null</exception>
    public CreateSMSStakeholderUserCommand(SMSStakeholderUser smsStakeholderUser)
    {
        SMSStakeholderUser = smsStakeholderUser ?? throw new ArgumentNullException(nameof(smsStakeholderUser));
    }

    public void SetCreatedBy(string userId, DateTime timestamp)
    {
        SMSStakeholderUser.CreatedBy = userId;
        SMSStakeholderUser.CreatedDate = timestamp;
    }

    public void SetUpdatedBy(string userId, DateTime timestamp)
    {
        // For create commands, we typically don't set UpdatedBy
    }
}

#endregion

#region Update Commands

/// <summary>
/// Command to update an existing SMS Stakeholder User
/// </summary>
public class UpdateSMSStakeholderUserCommand : BaseCommandBundle, IRequest<Result<SMSStakeholderUser>>, IUpdateCommand
{
    /// <summary>
    /// The SMS Stakeholder User entity to update
    /// </summary>
    public SMSStakeholderUser SMSStakeholderUser { get; set; }

    /// <summary>
    /// Initializes a new instance of the UpdateSMSStakeholderUserCommand class.
    /// </summary>
    /// <param name="smsStakeholderUser">The SMS stakeholder user to update</param>
    /// <exception cref="ArgumentNullException">Thrown when smsStakeholderUser is null</exception>
    public UpdateSMSStakeholderUserCommand(SMSStakeholderUser smsStakeholderUser)
    {
        SMSStakeholderUser = smsStakeholderUser ?? throw new ArgumentNullException(nameof(smsStakeholderUser));
    }

    public void SetCreatedBy(string userId, DateTime timestamp)
    {
        // For update commands, we typically don't modify CreatedBy
    }

    public void SetUpdatedBy(string userId, DateTime timestamp)
    {
        SMSStakeholderUser.UpdatedBy = userId;
        SMSStakeholderUser.UpdatedDate = timestamp;
    }
}

/// <summary>
/// Command to deactivate an existing SMS Stakeholder User (soft delete)
/// ✅ NEW: Proper CQRS command for user deactivation with audit pipeline support
/// </summary>
public class DeactivateSMSStakeholderUserCommand : BaseCommandBundle, IRequest<Result<SMSStakeholderUser>>, IUpdateCommand
{
    /// <summary>
    /// The SMS Stakeholder User entity to deactivate
    /// </summary>
    public SMSStakeholderUser SMSStakeholderUser { get; set; }

    /// <summary>
    /// Reason for deactivation
    /// </summary>
    public string DeactivationReason { get; set; }

    /// <summary>
    /// Initializes a new instance of the DeactivateSMSStakeholderUserCommand class.
    /// </summary>
    /// <param name="smsStakeholderUser">The SMS stakeholder user to deactivate</param>
    /// <param name="deactivationReason">Reason for deactivation</param>
    /// <exception cref="ArgumentNullException">Thrown when smsStakeholderUser is null</exception>
    public DeactivateSMSStakeholderUserCommand(SMSStakeholderUser smsStakeholderUser, string deactivationReason = "Deactivated by administrator")
    {
        SMSStakeholderUser = smsStakeholderUser ?? throw new ArgumentNullException(nameof(smsStakeholderUser));
        DeactivationReason = deactivationReason;
    }

    public void SetCreatedBy(string userId, DateTime timestamp)
    {
        // For deactivation commands, we don't modify CreatedBy
    }

    public void SetUpdatedBy(string userId, DateTime timestamp)
    {
        SMSStakeholderUser.UpdatedBy = userId;
        SMSStakeholderUser.UpdatedDate = timestamp;
    }
}

/// <summary>
/// Command to update SMS Stakeholder User password
/// </summary>
public class UpdateSMSStakeholderUserPasswordCommand : BaseCommandBundle, IRequest<Result<bool>>, IUpdateCommand
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
    /// Initializes a new instance of the UpdateSMSStakeholderUserPasswordCommand class.
    /// </summary>
    /// <param name="userId">The user ID</param>
    /// <param name="newPassword">The new password</param>
    /// <exception cref="ArgumentException">Thrown when userId or newPassword is null or empty</exception>
    public UpdateSMSStakeholderUserPasswordCommand(string userId, string newPassword)
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
/// Command to authenticate SMS Stakeholder User
/// </summary>
public class AuthenticateSMSStakeholderUserCommand : BaseCommandBundle, IRequest<Result<bool>>
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
    /// Initializes a new instance of the AuthenticateSMSStakeholderUserCommand class.
    /// </summary>
    /// <param name="userName">The username</param>
    /// <param name="password">The password</param>
    /// <exception cref="ArgumentException">Thrown when userName or password is null or empty</exception>
    public AuthenticateSMSStakeholderUserCommand(string userName, string password)
    {
        if (string.IsNullOrWhiteSpace(userName)) throw new ArgumentException("Username cannot be null or empty", nameof(userName));
        if (string.IsNullOrWhiteSpace(password)) throw new ArgumentException("Password cannot be null or empty", nameof(password));

        UserName = userName;
        Password = password;
    }
}

/// <summary>
/// Command to record SMS Stakeholder User login
/// </summary>
public class RecordSMSStakeholderUserLoginCommand : BaseCommandBundle, IRequest<Result<bool>>, IUpdateCommand
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
    /// Initializes a new instance of the RecordSMSStakeholderUserLoginCommand class.
    /// </summary>
    /// <param name="userId">The user ID</param>
    /// <param name="loginDate">The login date</param>
    /// <exception cref="ArgumentException">Thrown when userId is null or empty</exception>
    public RecordSMSStakeholderUserLoginCommand(string userId, DateTime loginDate)
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
/// Command to delete an SMS Stakeholder User by ID (legacy - use DeactivateSMSStakeholderUserCommand instead)
/// </summary>
public class DeleteSMSStakeholderUserCommand : BaseCommandBundle, IRequest<Result<bool>>, IDeleteCommand
{
    /// <summary>
    /// The ID of the SMS Stakeholder User to delete
    /// </summary>
    public SMSStakeholderUserID SMSStakeholderUserId { get; set; }

    /// <summary>
    /// User who performed the deletion
    /// </summary>
    public string DeletedBy { get; set; } = string.Empty;

    /// <summary>
    /// Initializes a new instance of the DeleteSMSStakeholderUserCommand class.
    /// </summary>
    /// <param name="smsStakeholderUserId">The ID of the SMS stakeholder user to delete</param>
    /// <exception cref="ArgumentNullException">Thrown when smsStakeholderUserId is null</exception>
    public DeleteSMSStakeholderUserCommand(SMSStakeholderUserID smsStakeholderUserId)
    {
        SMSStakeholderUserId = smsStakeholderUserId ?? throw new ArgumentNullException(nameof(smsStakeholderUserId));
    }

    public void SetDeletedBy(string userId, DateTime timestamp)
    {
        DeletedBy = userId;
    }
}

#endregion
